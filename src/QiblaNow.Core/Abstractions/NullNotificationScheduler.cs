using QiblaNow.Core.Models;

namespace QiblaNow.Core.Abstractions;

/// <summary>
/// No-op notification scheduler for platforms that don't support native alarms.
/// </summary>
public sealed class NullNotificationScheduler : INotificationScheduler
{
    public Task ScheduleNextNotificationAsync(NextNotificationCandidateResult candidate) => Task.CompletedTask;
    public Task CancelAllNotificationsAsync() => Task.CompletedTask;
    public Task ReconcileOnStartupAsync() => Task.CompletedTask;
    public Task HandleAlarmTriggeredAsync(PrayerType prayerType) => Task.CompletedTask;
}
