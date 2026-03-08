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
    /// <param name="location">Geographic location snapshot.</param>
    /// <param name="date">Target date (UTC day reference).</param>
    /// <param name="settings">Calculation configuration.</param>
    /// <returns>Daily prayer schedule.</returns>
    Task<DailyPrayerSchedule> CalculateDailyScheduleAsync(
        LocationSnapshot location,
        DateTimeOffset date,
        PrayerCalculationSettings settings);

}