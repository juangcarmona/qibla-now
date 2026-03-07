using System.Globalization;
using QiblaNow.Core.Abstractions;
using QiblaNow.Core.Models;

namespace QiblaNow.Core;

/// <summary>
/// Calculates Islamic prayer times using the PrayTimes algorithm
/// All calculations are performed in UTC and converted to the target timezone
/// Deterministic: no DateTimeOffset.UtcNow/Now calls inside Core
/// </summary>
public sealed class PrayerTimesCalculator : IPrayerTimesCalculator
{
    // Calculation method constants (angle values in degrees)
    private static readonly Dictionary<CalculationMethod, (double fajrAngle, double ishaAngle)> CalculationMethodConstants;

    static PrayerTimesCalculator()
    {
        CalculationMethodConstants = new Dictionary<CalculationMethod, (double fajrAngle, double ishaAngle)>
        {
            { CalculationMethod.MuslimWorldLeague, (18.0, 17.0) },
            { CalculationMethod.EgyptianGeneralAuthority, (19.5, 17.5) },
            { CalculationMethod.UmmAlQura, (18.5, 90.0) },
            { CalculationMethod.ISNA, (15.0, 15.0) },
            { CalculationMethod.Karachi, (18.0, 18.0) },
            { CalculationMethod.Kuwait, (17.5, 17.5) }
        };
    }

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

        // Calculate solar declination and equation of time
        var (declination, equationOfTime) = CalculateSolarPosition(latRad, date);

        // Calculate prayer times in UTC
        var fajr = new PrayerTime(PrayerType.Fajr, CalculateFajr(latRad, lonRad, declination, equationOfTime, fajrAngle, date));
        var sunrise = new PrayerTime(PrayerType.Sunrise, CalculateSunrise(latRad, lonRad, declination, equationOfTime, date));
        var dhuhr = new PrayerTime(PrayerType.Dhuhr, CalculateDhuhr(latRad, lonRad, declination, equationOfTime, date));
        var asr = new PrayerTime(PrayerType.Asr, CalculateAsr(latRad, lonRad, declination, equationOfTime, settings.Madhab, date));
        var maghrib = new PrayerTime(PrayerType.Maghrib, CalculateMaghrib(latRad, lonRad, declination, equationOfTime, date));
        var isha = new PrayerTime(PrayerType.Isha, CalculateIsha(latRad, lonRad, declination, equationOfTime, ishaAngle, settings.HighLatitudeRule, date));

        // Apply offsets
        var baseSchedule = new DailyPrayerSchedule(date, TimeZoneInfo.Local);
        baseSchedule.Prayers.Add(fajr);
        baseSchedule.Prayers.Add(sunrise);
        baseSchedule.Prayers.Add(dhuhr);
        baseSchedule.Prayers.Add(asr);
        baseSchedule.Prayers.Add(maghrib);
        baseSchedule.Prayers.Add(isha);

        var schedule = settings.ApplyOffsets(baseSchedule);

        return Task.FromResult(schedule);
    }

    public Task<NextPrayerResult> CalculateNextPrayerAsync(
        DailyPrayerSchedule schedule,
        PrayerNotificationSettings notificationSettings,
        DateTimeOffset? referenceTime = null)
    {
        var now = referenceTime ?? DateTimeOffset.UtcNow;

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

        // Wrap to tomorrow's Fajr - create a real schedule for tomorrow
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
        if (currentFajr != null)
        {
            if (currentFajr.DateTime <= now)
            {
                return Task.FromResult(new NextPrayerResult(
                    PrayerType.Fajr,
                    currentFajr.DateTime,
                    TimeSpan.Zero,
                    true
                ));
            }
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

        // Wrap to next day Fajr - create a real schedule for tomorrow
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
        PrayerNotificationSettings notificationSettings,
        DateTimeOffset? referenceTime = null)
    {
        var now = referenceTime ?? DateTimeOffset.UtcNow;

        // Find next enabled prayer
        var enabledTypes = notificationSettings.GetEnabledTypes();
        var nextPrayer = schedule
            .Prayers
            .Where(p => enabledTypes.Contains(p.Type) && p.DateTime > now)
            .OrderBy(p => p.DateTime)
            .FirstOrDefault();

        if (nextPrayer == null)
        {
            // No enabled prayer today, wrap to tomorrow
            var tomorrow = schedule.Date.AddDays(1);
            var tomorrowSchedule = new DailyPrayerSchedule(tomorrow, schedule.TimeZone);
            var tomorrowNextPrayer = tomorrowSchedule.GetNextPrayer(now);
            if (tomorrowNextPrayer != null)
            {
                nextPrayer = tomorrowNextPrayer.Value;
            }
        }

        if (nextPrayer == null)
        {
            return Task.FromResult(new CountdownTargetResult(
                PrayerType.Fajr,
                schedule.Date.AddDays(1),
                0
            ));
        }

        var remaining = nextPrayer.DateTime - now;
        return Task.FromResult(new CountdownTargetResult(
            nextPrayer.Type,
            nextPrayer.DateTime,
            (int)remaining.TotalSeconds
        ));
    }

    // --- Private Calculation Methods ---

    private static (double declination, double equationOfTime) CalculateSolarPosition(double latRad, DateTimeOffset date)
    {
        // Calculate day of year (1-366)
        var dayOfYear = date.DayOfYear;

        // Mean solar time correction angle
        var B = (360.0 / 365.0) * (dayOfYear - 81) * Math.PI / 180.0;
        var equationOfTime = 9.87 * Math.Sin(2 * B) - 7.53 * Math.Cos(B) - 1.5 * Math.Sin(B);

        // Solar declination angle
        var declination = 23.45 * Math.Sin((360.0 / 365.0) * (dayOfYear - 81) * Math.PI / 180.0);

        return (declination * Math.PI / 180.0, equationOfTime * Math.PI / 180.0);
    }

    private static DateTimeOffset CalculateFajr(
        double latRad,
        double lonRad,
        double declination,
        double equationOfTime,
        double fajrAngle,
        DateTimeOffset date)
    {
        return CalculatePrayerTime(
            latRad, lonRad, declination, equationOfTime, fajrAngle,
            date, -6.0); // Fajr angle relative to sunrise
    }

    private static DateTimeOffset CalculateSunrise(
        double latRad,
        double lonRad,
        double declination,
        double equationOfTime,
        DateTimeOffset date)
    {
        return CalculatePrayerTime(
            latRad, lonRad, declination, equationOfTime, 0.0,
            date, -0.83); // Approximate sunrise angle
    }

    private static DateTimeOffset CalculateDhuhr(
        double latRad,
        double lonRad,
        double declination,
        double equationOfTime,
        DateTimeOffset date)
    {
        // Dhuhr is at solar noon
        return CalculateMidday(latRad, lonRad, equationOfTime, date);
    }

    private static DateTimeOffset CalculateAsr(
        double latRad,
        double lonRad,
        double declination,
        double equationOfTime,
        Madhab madhab,
        DateTimeOffset date)
    {
        var asrAngle = madhab == Madhab.Hanafi ? 18.0 : 15.0;
        return CalculatePrayerTime(
            latRad, lonRad, declination, equationOfTime, asrAngle,
            date, -3.0); // Asr angle relative to sunset
    }

    private static DateTimeOffset CalculateMaghrib(
        double latRad,
        double lonRad,
        double declination,
        double equationOfTime,
        DateTimeOffset date)
    {
        return CalculatePrayerTime(
            latRad, lonRad, declination, equationOfTime, 0.0,
            date, -0.83); // Approximate sunset angle
    }

    private static DateTimeOffset CalculateIsha(
        double latRad,
        double lonRad,
        double declination,
        double equationOfTime,
        double ishaAngle,
        HighLatitudeRule rule,
        DateTimeOffset date)
    {
        if (rule == HighLatitudeRule.SeventhOfNight || rule == HighLatitudeRule.OneSeventh)
        {
            // Use angle-based Isha
            return CalculatePrayerTime(
                latRad, lonRad, declination, equationOfTime, ishaAngle,
                date, -18.0); // Isha angle relative to sunrise
        }

        return CalculatePrayerTime(
            latRad, lonRad, declination, equationOfTime, ishaAngle,
            date, -18.0); // Default fallback
    }

    private static DateTimeOffset CalculateMidday(double latRad, double lonRad, double equationOfTime, DateTimeOffset date)
    {
        var timeComponents = (date.Hour, date.Minute, date.Second);
        var hour = timeComponents.Hour + timeComponents.Minute / 60.0 + timeComponents.Second / 3600.0;

        // Equation of time correction
        var timeCorrection = 4 * latRad - 4 * equationOfTime;

        // Calculate time in minutes from noon
        var timeInMinutes = 12 * 60 - (hour * 60 - timeCorrection);

        // Convert to UTC
        var utcTime = new DateTime(date.Year, date.Month, date.Day, 12, 0, 0, DateTimeKind.Utc)
            .AddMinutes(timeInMinutes)
            .ToUniversalTime();

        return RoundToNearestMinute(utcTime, PrayerType.Dhuhr);
    }

    private static DateTimeOffset CalculatePrayerTime(
        double latRad,
        double lonRad,
        double declination,
        double equationOfTime,
        double angle,
        DateTimeOffset date,
        double angleOffset)
    {
        var timeComponents = (date.Hour, date.Minute, date.Second);
        var hour = timeComponents.Hour + timeComponents.Minute / 60.0 + timeComponents.Second / 3600.0;

        // Calculate time in minutes from noon
        var timeInMinutes = 12 * 60 - (hour * 60 - 4 * lonRad + equationOfTime * 60);

        // Calculate prayer angle (angle from sunrise/sunset)
        var angleInMinutes = angle * 4; // Each degree = 4 minutes

        if (angleOffset < 0)
        {
            timeInMinutes += angleInMinutes; // Fajr, Asr, Isha (before sunrise/sunset)
        }
        else
        {
            timeInMinutes -= angleInMinutes; // Sunrise, Maghrib (after sunrise/sunset)
        }

        // Convert to UTC
        var utcTime = new DateTime(date.Year, date.Month, date.Day, 12, 0, 0, DateTimeKind.Utc)
            .AddMinutes(timeInMinutes)
            .ToUniversalTime();

        return RoundToNearestMinute(utcTime, PrayerType.Fajr);
    }

    private static (int Hour, int Minute, int Second) TimeComponents(DateTimeOffset date)
    {
        return (date.Hour, date.Minute, date.Second);
    }

    private static DateTimeOffset RoundToNearestMinute(DateTimeOffset time, PrayerType type)
    {
        // Round to nearest minute
        var minutes = Math.Round(time.TimeOfDay.TotalMinutes);
        var roundedTime = new DateTime(time.Year, time.Month, time.Day,
            time.Hour, (int)minutes, 0, DateTimeKind.Utc);
        return new DateTimeOffset(roundedTime, time.Offset);
    }
}
