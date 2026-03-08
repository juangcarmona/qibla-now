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
}
