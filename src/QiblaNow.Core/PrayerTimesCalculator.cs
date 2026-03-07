using System.Globalization;
using QiblaNow.Core.Abstractions;
using QiblaNow.Core.Models;

namespace QiblaNow.Core;

/// <summary>
/// Calculates Islamic prayer times using the Jafari formula
/// All calculations are performed in UTC and converted to the target timezone
/// </summary>
public sealed class PrayerTimesCalculator : IPrayerTimesCalculator
{
    // Kaaba coordinates
    private const double KaabaLatitude = 21.422487;
    private const double KaabaLongitude = 39.826206;

    // Calculation method constants (angle values in degrees)
    private static readonly Dictionary<CalculationMethod, (double fajrAngle, double ishaAngle)> CalculationMethodConstants =
    {
        { CalculationMethod.MuslimWorldLeague, (18.0, 17.0) },
        { CalculationMethod.EgyptianGeneralAuthority, (19.5, 17.5) },
        { CalculationMethod.UmmAlQura, (18.5, 90.0) }, // Special case for Umm Al-Qura
        { CalculationMethod.ISNA, (15.0, 15.0) },
        { CalculationMethod.Karachi, (18.0, 18.0) },
        { CalculationMethod.Kuwait, (17.5, 17.5) }
    };

    // Isha night fraction constants (for Umm Al-Qura)
    private const double UmmAlQuraIshaNightFraction = 1.25;

    public Task<DailyPrayerSchedule> CalculateDailyScheduleAsync(
        LocationSnapshot location,
        DateTimeOffset date,
        PrayerCalculationSettings settings)
    {
        // Convert location to radians
        var latRad = location.Latitude * Math.PI / 180.0;
        var lonRad = location.Longitude * Math.PI / 180.0;

        // Get calculation method constants
        var (fajrAngle, ishaAngle) = CalculationMethodConstants[settings.Method];

        // Calculate declination angle
        var declination = CalculateDeclination(latRad, date);
        var equationOfTime = CalculateEquationOfTime(latRad, date);

        // Calculate prayer times (in UTC)
        var fajr = CalculatePrayerTime(latRad, declination, fajrAngle, date, equationOfTime, PrayerType.Fajr);
        var sunrise = CalculatePrayerTime(latRad, declination, 0.0, date, equationOfTime, PrayerType.Sunrise);
        var dhuhr = CalculatePrayerTime(latRad, declination, 0.0, date, equationOfTime, PrayerType.Dhuhr);
        var asr = CalculateAsr(latRad, declination, settings.Madhab, date, equationOfTime);
        var maghrib = CalculatePrayerTime(latRad, declination, 0.0, date, equationOfTime, PrayerType.Maghrib);
        var isha = CalculateIsha(latRad, declination, ishaAngle, date, equationOfTime, settings.HighLatitudeRule);

        // Apply offsets
        settings.ApplyOffsets(new DailyPrayerSchedule(date, location.DateTimeZone ?? TimeZoneInfo.Local));

        // Return schedule
        var schedule = new DailyPrayerSchedule(date, location.DateTimeZone ?? TimeZoneInfo.Local);
        schedule.Prayers.AddRange(new[] { fajr, sunrise, dhuhr, asr, maghrib, isha });

        return Task.FromResult(schedule);
    }

    public Task<NextPrayerResult> CalculateNextPrayerAsync(
        DailyPrayerSchedule schedule,
        PrayerNotificationSettings notificationSettings)
    {
        var now = DateTimeOffset.UtcNow; // Use UTC for consistency

        // Get next enabled prayer
        var enabledTypes = notificationSettings.GetEnabledTypes();
        var nextEnabledPrayer = schedule
            .Prayers
            .Where(p => enabledTypes.Contains(p.Type) && p.DateTime > now)
            .OrderBy(p => p.DateTime)
            .FirstOrDefault();

        if (nextEnabledPrayer != null)
        {
            var remaining = nextEnabledPrayer.DateTime - now;
            return Task.FromResult(new NextPrayerResult(
                nextEnabledPrayer.Type,
                nextEnabledPrayer.DateTime,
                remaining,
                true
            ));
        }

        // If no next enabled prayer today, wrap to next day Fajr
        var fajr = schedule.GetPrayer(PrayerType.Fajr);
        if (fajr != null && fajr.DateTime > now)
        {
            var remaining = fajr.DateTime - now;
            return Task.FromResult(new NextPrayerResult(
                PrayerType.Fajr,
                fajr.DateTime,
                remaining,
                true
            ));
        }

        // Wrap to tomorrow's Fajr
        var tomorrow = schedule.Date.AddDays(1);
        var tomorrowSchedule = new DailyPrayerSchedule(tomorrow, schedule.TimeZone);
        var tomorrowFajr = tomorrowSchedule.GetPrayer(PrayerType.Fajr);
        if (tomorrowFajr != null)
        {
            var remaining = tomorrowFajr.DateTime - now;
            return Task.FromResult(new NextPrayerResult(
                PrayerType.Fajr,
                tomorrowFajr.DateTime,
                remaining,
                false
            ));
        }

        // Fallback: return current prayer if it's today's Fajr
        var currentFajr = schedule.GetPrayer(PrayerType.Fajr);
        if (currentFajr != null && currentFajr.DateTime <= now)
        {
            return Task.FromResult(new NextPrayerResult(
                PrayerType.Fajr,
                currentFajr.DateTime,
                TimeSpan.Zero,
                true
            ));
        }

        return Task.FromResult(new NextPrayerResult(
            PrayerType.Fajr,
            now,
            TimeSpan.Zero,
            false
        ));
    }

    public Task<NextNotificationCandidateResult> CalculateNextNotificationCandidateAsync(
        DailyPrayerSchedule schedule,
        PrayerNotificationSettings notificationSettings)
    {
        var now = DateTimeOffset.UtcNow;

        // Get next enabled prayer
        var enabledTypes = notificationSettings.GetEnabledTypes();
        var nextEnabledPrayer = schedule
            .Prayers
            .Where(p => enabledTypes.Contains(p.Type) && p.DateTime > now)
            .OrderBy(p => p.DateTime)
            .FirstOrDefault();

        if (nextEnabledPrayer != null)
        {
            return Task.FromResult(new NextNotificationCandidateResult(
                nextEnabledPrayer.Type,
                nextEnabledPrayer.DateTime
            ));
        }

        // Wrap to next day Fajr
        var tomorrow = schedule.Date.AddDays(1);
        var tomorrowSchedule = new DailyPrayerSchedule(tomorrow, schedule.TimeZone);
        var tomorrowFajr = tomorrowSchedule.GetPrayer(PrayerType.Fajr);
        if (tomorrowFajr != null)
        {
            return Task.FromResult(new NextNotificationCandidateResult(
                PrayerType.Fajr,
                tomorrowFajr.DateTime
            ));
        }

        // Default to next day Fajr
        return Task.FromResult(new NextNotificationCandidateResult(
            PrayerType.Fajr,
            now.AddDays(1)
        ));
    }

    public Task<CountdownTargetResult> CalculateCountdownAsync(
        DailyPrayerSchedule schedule,
        PrayerNotificationSettings notificationSettings)
    {
        var nextPrayer = schedule.GetNextPrayer();
        if (nextPrayer == null)
        {
            return Task.FromResult(new CountdownTargetResult(
                PrayerType.Fajr,
                schedule.Date.AddDays(1),
                0
            ));
        }

        var now = DateTimeOffset.UtcNow;
        var remaining = nextPrayer.DateTime - now;
        return Task.FromResult(new CountdownTargetResult(
            nextPrayer.Type,
            nextPrayer.DateTime,
            (int)remaining.TotalSeconds
        ));
    }

    // --- Private Calculation Methods ---

    private static double CalculateDeclination(double latRad, DateTimeOffset date)
    {
        var dayOfYear = date.DayOfYear;
        var solarDeclination = 23.45 * Math.Sin((360.0 / 365.0) * (dayOfYear - 81) * Math.PI / 180.0);
        return solarDeclination * Math.PI / 180.0;
    }

    private static double CalculateEquationOfTime(double latRad, DateTimeOffset date)
    {
        var dayOfYear = date.DayOfYear;
        var B = (360.0 / 365.0) * (dayOfYear - 81) * Math.PI / 180.0;
        var equationOfTime = 9.87 * Math.Sin(2 * B) - 7.53 * Math.Cos(B) - 1.5 * Math.Sin(B);
        return equationOfTime * Math.PI / 180.0;
    }

    private static DateTimeOffset CalculatePrayerTime(
        double latRad,
        double declination,
        double angle,
        DateTimeOffset date,
        double equationOfTime,
        PrayerType type)
    {
        var now = date.ToUniversalTime();
        var timeOffset = GetSunriseTime(latRad, declination, angle) - equationOfTime;

        var prayerTime = now.AddMinutes(timeOffset);
        return RoundToNearestMinute(prayerTime, type);
    }

    private static DateTimeOffset CalculateAsr(
        double latRad,
        double declination,
        Madhab madhab,
        DateTimeOffset date,
        double equationOfTime)
    {
        var now = date.ToUniversalTime();
        var angle = madhab == Madhab.Hanafi ? 18.0 : 15.0;
        var timeOffset = GetSunsetTime(latRad, declination) - GetAsrTime(latRad, declination, angle) - equationOfTime;

        var asrTime = now.AddMinutes(timeOffset);
        return RoundToNearestMinute(asrTime, PrayerType.Asr);
    }

    private static DateTimeOffset CalculateIsha(
        double latRad,
        double declination,
        double angle,
        DateTimeOffset date,
        double equationOfTime,
        HighLatitudeRule rule)
    {
        var now = date.ToUniversalTime();
        var timeOffset = GetSunsetTime(latRad, declination) - GetIshaTime(latRad, declination, angle, rule) - equationOfTime;

        var ishaTime = now.AddMinutes(timeOffset);
        return RoundToNearestMinute(ishaTime, PrayerType.Isha);
    }

    private static double GetSunriseTime(double latRad, double declination, double angle)
    {
        var sinAltitude = Math.Sin(angle * Math.PI / 180.0);
        var sinDec = Math.Sin(declination);
        var cosLat = Math.Cos(latRad);

        var result = -Math.Cos(latRad) * sinDec - sinAltitude;
        return 180.0 * Math.Asin(result) / Math.PI;
    }

    private static double GetSunsetTime(double latRad, double declination)
    {
        var sinAltitude = 0.0;
        var sinDec = Math.Sin(declination);
        var cosLat = Math.Cos(latRad);

        var result = Math.Cos(latRad) * sinDec - sinAltitude;
        return 180.0 * Math.Acos(result) / Math.PI;
    }

    private static double GetAsrTime(double latRad, double declination, double angle)
    {
        var tanDec = Math.Tan(declination);
        var value = 1.0 / tanDec;
        var correction = -Math.Cos(Math.Asin(value)) * 180.0 / Math.PI;
        return Math.Acos(correction) * 180.0 / Math.PI;
    }

    private static double GetIshaTime(double latRad, double declination, double angle, HighLatitudeRule rule)
    {
        var sunset = GetSunsetTime(latRad, declination);
        var nightLength = sunset * 2.0;

        return rule switch
        {
            HighLatitudeRule.SeventhOfNight => nightLength / 7.0,
            HighLatitudeRule.MiddleOfNight => nightLength / 2.0,
            HighLatitudeRule.OneSeventh => nightLength / 7.0,
            _ => nightLength / 7.0
        };
    }

    private static DateTimeOffset RoundToNearestMinute(DateTimeOffset time, PrayerType type)
    {
        // Round to nearest minute
        var minutes = Math.Round(time.TimeOfDay.TotalMinutes);
        var roundedTime = new DateTime(time.Year, time.Month, time.Day, time.Hour, (int)minutes, 0, DateTimeKind.Utc);
        return new DateTimeOffset(roundedTime, time.Offset);
    }
}
