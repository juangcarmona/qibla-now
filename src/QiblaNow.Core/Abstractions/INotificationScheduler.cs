using QiblaNow.Core.Models;

namespace QiblaNow.Core.Abstractions;

/// <summary>
/// Platform-neutral interface for scheduling and managing prayer notification alarms.
/// Android-specific lifecycle events (boot, alarm fired) are handled in the platform
/// layer — not through this interface.
/// </summary>
public interface INotificationScheduler
{
    /// <summary>
    /// Schedules the next prayer notification alarm, replacing any currently pending one.
    /// </summary>
    Task ScheduleNextNotificationAsync(NextNotificationCandidateResult candidate);

    /// <summary>
    /// Cancels all pending prayer notification alarms.
    /// </summary>
    Task CancelAllNotificationsAsync();

    /// <summary>
    /// Reconciles and restores notification scheduling.
    /// Must be idempotent — safe to call on app startup, boot, and timezone change.
    /// </summary>
    Task ReconcileOnStartupAsync();

    /// <summary>
    /// Handles the event triggered by an alarm for a specific prayer type asynchronously.
    /// </summary>
    /// <remarks>This method is intended to be called when an alarm for a prayer is triggered, allowing for
    /// appropriate actions to be taken based on the prayer type.</remarks>
    /// <param name="prayerType">The type of prayer associated with the alarm that was triggered. This parameter influences the behavior of the
    /// method based on the specific prayer type being handled.</param>
    /// <returns>A task representing the asynchronous operation of handling the alarm.</returns>
    Task HandleAlarmTriggeredAsync(PrayerType prayerType);
}
