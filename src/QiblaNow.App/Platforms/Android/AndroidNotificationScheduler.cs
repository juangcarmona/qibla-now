using Android.App;
using Android.Content;
using Android.OS;
using QiblaNow.Core.Abstractions;
using QiblaNow.Core.Models;

namespace QiblaNow.App.Platforms.Android;

/// <summary>
/// Android implementation of prayer notification scheduling using AlarmManager
/// Uses AlarmManager for exact-time triggers that survive app process death
/// </summary>
public sealed class AndroidNotificationScheduler : INotificationScheduler
{
    private readonly Context _context;
    private readonly IPrayerSettingsStore _prayerSettingsStore;
    private readonly IPrayerTimesCalculator _calculator;

    private const string AlarmAction = "com.qiblanow.PRAYER_ALARM";
    private const string PrayerTypeExtra = "prayer_type";

    public AndroidNotificationScheduler(Context context, IPrayerSettingsStore prayerSettingsStore, IPrayerTimesCalculator calculator)
    {
        _context = context;
        _prayerSettingsStore = prayerSettingsStore;
        _calculator = calculator;
    }

    public async Task ScheduleNextNotificationAsync(NextNotificationCandidateResult candidate)
    {
        try
        {
            // Cancel any existing alarms
            await CancelAllNotificationsAsync();

            // Create intent for alarm
            var intent = new Intent(AlarmAction);
            intent.SetPackage(_context.PackageName);
            intent.PutExtra(PrayerTypeExtra, (int)candidate.Type);

            // Use PendingIntent for alarm trigger
            var pendingIntent = PendingIntent.GetBroadcast(
                _context,
                0,
                intent,
                PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);

            // Get AlarmManager
            var alarmManager = _context.GetSystemService(Context.AlarmService) as AlarmManager;
            if (alarmManager == null) return;

            // Schedule exact alarm
            // Use RTC_WAKEUP to wake device if needed
            // Use SetExactAndAllowWhileIdle for battery optimization
            alarmManager.SetExactAndAllowWhileIdle(
                AlarmType.RtcWakeup,
                candidate.ScheduledTime.ToUnixTimeMilliseconds(),
                pendingIntent);

            // Save scheduling metadata for reconciliation
            SaveSchedulingMetadata(candidate.Type, candidate.ScheduledTime);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error scheduling notification: {ex.Message}");
        }
    }

    public async Task CancelAllNotificationsAsync()
    {
        try
        {
            var alarmManager = _context.GetSystemService(Context.AlarmService) as AlarmManager;
            if (alarmManager == null) return;

            // Cancel any existing alarms
            var intent = new Intent(AlarmAction);
            intent.SetPackage(_context.PackageName);

            var pendingIntent = PendingIntent.GetBroadcast(
                _context,
                0,
                intent,
                PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);

            alarmManager.Cancel(pendingIntent);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error canceling notifications: {ex.Message}");
        }
    }

    public async Task ReconcileOnStartupAsync()
    {
        try
        {
            var notificationSettings = _prayerSettingsStore.GetNotificationSettings();

            if (!notificationSettings.IsAnyEnabled)
            {
                // No notifications enabled, clear all
                await CancelAllNotificationsAsync();
                return;
            }

            var location = _prayerSettingsStore.GetLastValidLocation();
            if (location == null)
            {
                // No valid location, cannot schedule
                System.Diagnostics.Debug.WriteLine("No valid location for scheduling");
                return;
            }

            var calculationSettings = _prayerSettingsStore.GetCalculationSettings();
            var date = DateTimeOffset.UtcNow;
            var schedule = await _calculator.CalculateDailyScheduleAsync(location, date, calculationSettings);

            // Calculate next notification candidate
            var candidate = await _calculator.CalculateNextNotificationCandidateAsync(schedule, notificationSettings);

            if (candidate != null)
            {
                await ScheduleNextNotificationAsync(candidate);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error reconciling on startup: {ex.Message}");
        }
    }

    public Task HandleAlarmFiredAsync(PrayerType prayerType)
    {
        try
        {
            // Dispatch notification (will be implemented with platform-specific notification API)
            System.Diagnostics.Debug.WriteLine($"Prayer alarm fired for {prayerType}");

            // Recalculate and schedule next notification
            _ = Task.Run(async () =>
            {
                var location = _prayerSettingsStore.GetLastValidLocation();
                if (location == null) return;

                var calculationSettings = _prayerSettingsStore.GetCalculationSettings();
                var notificationSettings = _prayerSettingsStore.GetNotificationSettings();

                var schedule = await _calculator.CalculateDailyScheduleAsync(location, DateTimeOffset.UtcNow, calculationSettings);

                // Calculate next notification candidate
                var candidate = await _calculator.CalculateNextNotificationCandidateAsync(schedule, notificationSettings);

                if (candidate != null)
                {
                    await ScheduleNextNotificationAsync(candidate);
                }
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error handling alarm fire: {ex.Message}");
        }

        return Task.CompletedTask;
    }

    public Task HandleBootCompletedAsync()
    {
        try
        {
            // Reconcile scheduling after reboot
            _ = Task.Run(async () =>
            {
                await ReconcileOnStartupAsync();
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error handling boot completed: {ex.Message}");
        }

        return Task.CompletedTask;
    }

    private void SaveSchedulingMetadata(PrayerType prayerType, DateTimeOffset scheduledTime)
    {
        try
        {
            _prayerSettingsStore.SaveNotificationSettings(new PrayerNotificationSettings
            {
                FajrEnabled = prayerType == PrayerType.Fajr,
                DhuhrEnabled = prayerType == PrayerType.Dhuhr,
                AsrEnabled = prayerType == PrayerType.Asr,
                MaghribEnabled = prayerType == PrayerType.Maghrib,
                IshaEnabled = prayerType == PrayerType.Isha
            });

            _prayerSettingsStore.SaveLastValidLocation(new LocationSnapshot(
                LocationMode.Manual,
                51.5074, -0.1278,  // London (example)
                null
            ));
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error saving scheduling metadata: {ex.Message}");
        }
    }
}
