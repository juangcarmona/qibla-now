using QiblaNow.Core.Models;

namespace QiblaNow.Core.Abstractions;

/// <summary>
/// Contract for prayer time calculation and related queries.
/// Implementations must be deterministic and must not access system time.
/// All reference times must be provided explicitly by the caller.
/// </summary>
public interface IPrayerTimesCalculator
{
    /// <summary>
    /// Calculates the complete prayer schedule for a given date and location.
    /// </summary>
    Task<DailyPrayerSchedule> CalculateDailyScheduleAsync(
        LocationSnapshot location,
        DateTimeOffset date,
        PrayerCalculationSettings settings);

    /// <summary>
    /// Returns the next enabled prayer after <paramref name="now"/>.
    /// If all today's prayers have passed, returns tomorrow's first enabled prayer
    /// (time is approximated as +24h; callers that need precision should recompute
    /// tomorrow's schedule and call this method again).
    /// Returns null if no prayer notifications are enabled.
    /// </summary>
    Task<NextPrayerResult?> CalculateNextPrayerAsync(
        DailyPrayerSchedule schedule,
        PrayerNotificationSettings notificationSettings,
        DateTimeOffset now);

    /// <summary>
    /// Returns the next notification alarm candidate after <paramref name="now"/>.
    /// Like <see cref="CalculateNextPrayerAsync"/> but returns the scheduling-oriented result.
    /// Returns null if no prayer notifications are enabled.
    /// </summary>
    Task<NextNotificationCandidateResult?> CalculateNextNotificationCandidateAsync(
        DailyPrayerSchedule schedule,
        PrayerNotificationSettings notificationSettings,
        DateTimeOffset now);

    /// <summary>
    /// Computes the countdown to the next enabled prayer after <paramref name="now"/>.
    /// Returns null if no prayer notifications are enabled.
    /// </summary>
    Task<CountdownTargetResult?> CalculateCountdownAsync(
        DailyPrayerSchedule schedule,
        PrayerNotificationSettings notificationSettings,
        DateTimeOffset now);
}