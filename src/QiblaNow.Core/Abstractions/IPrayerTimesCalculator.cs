using QiblaNow.Core.Models;

namespace QiblaNow.Core.Abstractions;

/// <summary>
/// Calculates Islamic prayer times from location and calculation settings
/// </summary>
public interface IPrayerTimesCalculator
{
    /// <summary>
    /// Calculates the complete daily prayer schedule for a specific date and location
    /// </summary>
    Task<DailyPrayerSchedule> CalculateDailyScheduleAsync(
        LocationSnapshot location,
        DateTimeOffset date,
        PrayerCalculationSettings settings);

    /// <summary>
    /// Calculates the next prayer after the current time
    /// </summary>
    Task<NextPrayerResult> CalculateNextPrayerAsync(
        DailyPrayerSchedule schedule,
        PrayerNotificationSettings notificationSettings);

    /// <summary>
    /// Calculates the next notification candidate based on enabled prayer notifications
    /// </summary>
    Task<NextNotificationCandidateResult> CalculateNextNotificationCandidateAsync(
        DailyPrayerSchedule schedule,
        PrayerNotificationSettings notificationSettings);

    /// <summary>
    /// Calculates the countdown to the next prayer
    /// </summary>
    Task<CountdownTargetResult> CalculateCountdownAsync(
        DailyPrayerSchedule schedule,
        PrayerNotificationSettings notificationSettings);
}
