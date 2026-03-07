using QiblaNow.Core.Models;

namespace QiblaNow.Core.Abstractions;

/// <summary>
/// Interface for scheduling and managing prayer notifications
/// </summary>
public interface INotificationScheduler
{
    /// <summary>
    /// Schedules the next prayer notification alarm
    /// </summary>
    Task ScheduleNextNotificationAsync(NextNotificationCandidateResult candidate);

    /// <summary>
    /// Cancels all pending prayer notification alarms
    /// </summary>
    Task CancelAllNotificationsAsync();

    /// <summary>
    /// Reconciles and restores notification scheduling on app startup
    /// </summary>
    Task ReconcileOnStartupAsync();

    /// <summary>
    /// Called when a scheduled alarm fires
    /// </summary>
    Task HandleAlarmFiredAsync(PrayerType prayerType);

    /// <summary>
    /// Called when the device boots up
    /// </summary>
    Task HandleBootCompletedAsync();
}
