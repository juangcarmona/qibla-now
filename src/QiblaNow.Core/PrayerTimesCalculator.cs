using QiblaNow.Core.Abstractions;
using QiblaNow.Core.Models;

namespace QiblaNow.Core;

public sealed class PrayerTimesCalculator : IPrayerTimesCalculator
{
    private const double Deg2Rad = Math.PI / 180.0;
    private const double Rad2Deg = 180.0 / Math.PI;

    private static readonly Dictionary<CalculationMethod, (double fajr, double isha)> Methods =
        new()
        {
            { CalculationMethod.MuslimWorldLeague, (18, 17) },
            { CalculationMethod.EgyptianGeneralAuthority, (19.5, 17.5) },
            { CalculationMethod.ISNA, (15, 15) },
            { CalculationMethod.Karachi, (18, 18) },
            { CalculationMethod.Kuwait, (17.5, 17.5) },
            { CalculationMethod.UmmAlQura, (18.5, 0) }
        };

    // ── Schedule calculation ─────────────────────────────────────────────────

    public Task<DailyPrayerSchedule> CalculateDailyScheduleAsync(
        LocationSnapshot location,
        DateTimeOffset date,
        PrayerCalculationSettings settings)
    {
        double lat = location.Latitude;
        double lng = location.Longitude;

        // Days from J2000.0 at UTC midnight of this date.
        // J2000.0 = 2000-01-01T12:00:00Z = Unix day 10957.5.
        // Matches the PrayTimes reference: D = utcTime/864e5 - 10957.5 + seedHour/24 - lng/360.
        double d0 = date.ToUniversalTime().ToUnixTimeMilliseconds() / 86400000.0 - 10957.5;

        // Compute solar declination and UTC solar noon for a given prayer seed hour.
        // PrayTimes uses per-prayer seed times (fajr=5, sunrise=6, dhuhr=12, asr=13, sunset/isha=18)
        // to evaluate the solar position at each prayer's approximate local time.
        (double Decl, double NoonUtc) SolarForSeed(double seedHour)
        {
            double D = d0 + seedHour / 24.0 - lng / 360.0;
            var (decl, eqt) = SolarPosition(D);
            return (decl, FixHour(12.0 - eqt - lng / 15.0));
        }

        var (fajrAngle, ishaAngle) = Methods[settings.Method];

        var (declFajr,    noonFajr)    = SolarForSeed(5);
        var (declSunrise, noonSunrise) = SolarForSeed(6);
        var (_,           noonDhuhr)   = SolarForSeed(12);
        var (declAsr,     noonAsr)     = SolarForSeed(13);
        var (declSunset,  noonSunset)  = SolarForSeed(18);

        double sunrise = SunAngleTime(-0.833,      lat, declSunrise, noonSunrise, false);
        double sunset  = SunAngleTime(-0.833,      lat, declSunset,  noonSunset,  true);
        // Maghrib: all supported methods use sunset + 1 minute
        // (PrayTimes defaults: maghrib = '1 min').
        double maghrib = sunset + 1.0 / 60.0;

        double fajrRaw = SunAngleTime(-fajrAngle,  lat, declFajr,    noonFajr,    false);
        // UmmAlQura: isha = maghrib + 90 min (PrayTimes updateTimes: isha = times.maghrib + 90/60).
        // All other methods: angle-based. Isha uses the same seed as sunset (seed=18).
        double ishaRaw = settings.Method == CalculationMethod.UmmAlQura
            ? maghrib + 90.0 / 60.0
            : SunAngleTime(-ishaAngle, lat, declSunset, noonSunset, true);

        // High-latitude adjustment.
        // Matches PrayTimes adjustHighLats: apply when time is NaN *or* when the
        // prayer falls outside the allowed night fraction (timeDiff > portion).
        // Night duration uses the PrayTimes formula: night = 24 + sunrise - sunset.
        double nightDuration = 24.0 + sunrise - sunset;
        fajrRaw = ApplyHighLatitudeRule(fajrRaw, sunrise, nightDuration, settings.HighLatitudeRule, false);
        ishaRaw = ApplyHighLatitudeRule(ishaRaw, sunset,  nightDuration, settings.HighLatitudeRule, true);

        double dhuhr = noonDhuhr;
        double asr   = AsrTime(settings.Madhab, lat, declAsr, noonAsr);

        // Build schedule (offsets applied, then rounded to nearest minute)
        var schedule = new DailyPrayerSchedule(date, TimeZoneInfo.Utc);

        schedule.Prayers.Add(new PrayerTime(PrayerType.Fajr,    ToDate(date, fajrRaw  + settings.FajrOffsetMinutes    / 60.0), settings.FajrOffsetMinutes));
        schedule.Prayers.Add(new PrayerTime(PrayerType.Sunrise,  ToDate(date, sunrise)));
        schedule.Prayers.Add(new PrayerTime(PrayerType.Dhuhr,   ToDate(date, dhuhr    + settings.DhuhrOffsetMinutes   / 60.0), settings.DhuhrOffsetMinutes));
        schedule.Prayers.Add(new PrayerTime(PrayerType.Asr,     ToDate(date, asr      + settings.AsrOffsetMinutes     / 60.0), settings.AsrOffsetMinutes));
        schedule.Prayers.Add(new PrayerTime(PrayerType.Maghrib, ToDate(date, maghrib  + settings.MaghribOffsetMinutes / 60.0), settings.MaghribOffsetMinutes));
        schedule.Prayers.Add(new PrayerTime(PrayerType.Isha,    ToDate(date, ishaRaw  + settings.IshaOffsetMinutes    / 60.0), settings.IshaOffsetMinutes));

        return Task.FromResult(schedule);
    }

    // ── Next-prayer / notification helpers ──────────────────────────────────

    public Task<NextPrayerResult?> CalculateNextPrayerAsync(
        DailyPrayerSchedule schedule,
        PrayerNotificationSettings notificationSettings,
        DateTimeOffset now)
    {
        var enabled = notificationSettings.GetEnabledTypes().ToHashSet();
        if (enabled.Count == 0)
            return Task.FromResult<NextPrayerResult?>(null);

        // First enabled prayer strictly after now
        var next = schedule.Prayers
            .Where(p => enabled.Contains(p.Type) && p.DateTime > now)
            .OrderBy(p => p.DateTime)
            .Select(p => (PrayerTime?)p)
            .FirstOrDefault();

        if (next.HasValue)
        {
            return Task.FromResult<NextPrayerResult?>(new NextPrayerResult(
                next.Value.Type,
                next.Value.DateTime,
                next.Value.DateTime - now,
                isToday: true));
        }

        // All today's prayers have passed — roll over to tomorrow's first enabled prayer.
        // Use a +24h approximation; callers that need precision should recompute tomorrow's
        // schedule explicitly and call this method again.
        var firstEnabled = schedule.Prayers
            .Where(p => enabled.Contains(p.Type))
            .OrderBy(p => p.DateTime)
            .Select(p => (PrayerTime?)p)
            .FirstOrDefault();

        if (firstEnabled.HasValue)
        {
            var tomorrowTime = firstEnabled.Value.DateTime.AddDays(1);
            return Task.FromResult<NextPrayerResult?>(new NextPrayerResult(
                firstEnabled.Value.Type,
                tomorrowTime,
                tomorrowTime - now,
                isToday: false));
        }

        return Task.FromResult<NextPrayerResult?>(null);
    }

    public Task<NextNotificationCandidateResult?> CalculateNextNotificationCandidateAsync(
        DailyPrayerSchedule schedule,
        PrayerNotificationSettings notificationSettings,
        DateTimeOffset now)
    {
        var enabled = notificationSettings.GetEnabledTypes().ToHashSet();
        if (enabled.Count == 0)
            return Task.FromResult<NextNotificationCandidateResult?>(null);

        var next = schedule.Prayers
            .Where(p => enabled.Contains(p.Type) && p.DateTime > now)
            .OrderBy(p => p.DateTime)
            .Select(p => (PrayerTime?)p)
            .FirstOrDefault();

        if (next.HasValue)
        {
            return Task.FromResult<NextNotificationCandidateResult?>(
                new NextNotificationCandidateResult(next.Value.Type, next.Value.DateTime));
        }

        // Roll over to tomorrow's first enabled prayer (~+24h approximation)
        var firstEnabled = schedule.Prayers
            .Where(p => enabled.Contains(p.Type))
            .OrderBy(p => p.DateTime)
            .Select(p => (PrayerTime?)p)
            .FirstOrDefault();

        if (firstEnabled.HasValue)
        {
            return Task.FromResult<NextNotificationCandidateResult?>(
                new NextNotificationCandidateResult(
                    firstEnabled.Value.Type,
                    firstEnabled.Value.DateTime.AddDays(1)));
        }

        return Task.FromResult<NextNotificationCandidateResult?>(null);
    }

    public Task<CountdownTargetResult?> CalculateCountdownAsync(
        DailyPrayerSchedule schedule,
        PrayerNotificationSettings notificationSettings,
        DateTimeOffset now)
    {
        var enabled = notificationSettings.GetEnabledTypes().ToHashSet();
        if (enabled.Count == 0)
            return Task.FromResult<CountdownTargetResult?>(null);

        var next = schedule.Prayers
            .Where(p => enabled.Contains(p.Type) && p.DateTime > now)
            .OrderBy(p => p.DateTime)
            .Select(p => (PrayerTime?)p)
            .FirstOrDefault();

        DateTimeOffset target;
        PrayerType prayerType;

        if (next.HasValue)
        {
            target      = next.Value.DateTime;
            prayerType  = next.Value.Type;
        }
        else
        {
            // Roll over — same +24h approximation
            var firstEnabled = schedule.Prayers
                .Where(p => enabled.Contains(p.Type))
                .OrderBy(p => p.DateTime)
                .Select(p => (PrayerTime?)p)
                .FirstOrDefault();

            if (!firstEnabled.HasValue)
                return Task.FromResult<CountdownTargetResult?>(null);

            target     = firstEnabled.Value.DateTime.AddDays(1);
            prayerType = firstEnabled.Value.Type;
        }

        var remaining = (int)Math.Max(0, Math.Round((target - now).TotalSeconds));
        return Task.FromResult<CountdownTargetResult?>(
            new CountdownTargetResult(prayerType, target, remaining));
    }

    // ── Solar math ──────────────────────────────────────────────────────────

    /// <summary>
    /// Computes solar declination and equation of time for a given Julian Day
    /// offset from J2000.0 (D = utcTime_days_from_unix - 10957.5 + seed/24 - lng/360).
    /// Matches the PrayTimes v3.2 sunPosition() formula exactly.
    /// </summary>
    private static (double declination, double equation) SolarPosition(double D)
    {
        double g = FixAngle(357.529 + 0.98560028 * D);
        double q = FixAngle(280.459 + 0.98564736 * D);
        double L = FixAngle(q + 1.915 * Sin(g) + 0.020 * Sin(2 * g));
        double e = 23.439 - 0.00000036 * D;

        // RA must be normalized to [0, 24) to match PrayTimes mod(arctan2/15, 24)
        double RA   = FixHour(Atan2(Cos(e) * Sin(L), Cos(L)) / 15.0);
        double decl = Asin(Sin(e) * Sin(L));
        double eqt  = q / 15.0 - RA;

        return (decl, eqt);
    }

    private static double SunAngleTime(
        double angle,
        double lat,
        double decl,
        double noon,
        bool afterNoon)
    {
        double term =
            (Sin(angle) - Sin(lat) * Sin(decl)) /
            (Cos(lat) * Cos(decl));

        if (double.IsNaN(term) || term < -1.0 || term > 1.0)
            return double.NaN;

        double t = Acos(term) / 15.0;
        return noon + (afterNoon ? t : -t);
    }

    private static double AsrTime(Madhab madhab, double lat, double decl, double noon)
    {
        // Shadow ratio: Shafi = object shadow + 1× height, Hanafi = +2× height
        double factor = madhab == Madhab.Hanafi ? 2 : 1;
        // Positive angle: sun is ABOVE the horizon at Asr elevation
        double angle = Acot(factor + Tan(Math.Abs(lat - decl)));
        return SunAngleTime(angle, lat, decl, noon, true);
    }

    // ── High-latitude correction ─────────────────────────────────────────────

    /// <summary>
    /// Matches PrayTimes adjustTime(): applies the high-latitude rule whenever
    /// the prayer time is NaN (sun never reaches the required angle) OR the
    /// prayer falls farther from sunrise/sunset than the allowed portion.
    /// For SeventhOfNight: Fajr = sunrise − night/7, Isha = sunset + night/7.
    /// For MiddleOfNight:  Fajr = sunrise − night/2, Isha = sunset + night/2.
    /// night = 24 + sunrise − sunset (PrayTimes formula).
    /// </summary>
    private static double ApplyHighLatitudeRule(
        double time,
        double anchor,        // sunrise for Fajr, sunset for Isha
        double nightDuration, // = 24 + sunrise − sunset
        HighLatitudeRule rule,
        bool isIsha)
    {
        double portion = rule switch
        {
            HighLatitudeRule.SeventhOfNight => nightDuration / 7.0,
            HighLatitudeRule.MiddleOfNight  => nightDuration / 2.0,
            _                               => nightDuration / 7.0  // default: SeventhOfNight
        };

        // timeDiff: how far the prayer is from its anchor (positive = farther)
        double timeDiff = isIsha ? (time - anchor) : (anchor - time);

        if (double.IsNaN(time) || timeDiff > portion)
            return isIsha ? anchor + portion : anchor - portion;

        return time;
    }

    // ── Utilities ───────────────────────────────────────────────────────────

    /// <summary>Converts fractional hours to a rounded-to-nearest-minute DateTimeOffset.</summary>
    private static DateTimeOffset ToDate(DateTimeOffset date, double hours)
    {
        if (double.IsNaN(hours))
            hours = 0;

        // Round total minutes first, then split into h/m
        int totalMinutes = (int)Math.Round(hours * 60);
        int h = totalMinutes / 60;
        int m = totalMinutes % 60;

        // Clamp to valid hour range (handles edge-case overflow from rounding)
        h = Math.Clamp(h, 0, 23);
        m = Math.Clamp(m, 0, 59);

        return new DateTimeOffset(
            date.Year, date.Month, date.Day,
            h, m, 0,
            TimeSpan.Zero);
    }

    private static double FixAngle(double a)
    {
        a %= 360;
        return a < 0 ? a + 360 : a;
    }

    private static double FixHour(double h)
    {
        h %= 24;
        return h < 0 ? h + 24 : h;
    }

    private static double Sin(double d)  => Math.Sin(d * Deg2Rad);
    private static double Cos(double d)  => Math.Cos(d * Deg2Rad);
    private static double Tan(double d)  => Math.Tan(d * Deg2Rad);
    private static double Asin(double x) => Rad2Deg * Math.Asin(x);
    private static double Acos(double x) => Rad2Deg * Math.Acos(x);
    private static double Atan2(double y, double x) => Rad2Deg * Math.Atan2(y, x);
    private static double Acot(double x) => Rad2Deg * Math.Atan(1.0 / x);
}

