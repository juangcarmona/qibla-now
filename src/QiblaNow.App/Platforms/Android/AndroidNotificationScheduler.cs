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
    private readonly ISettingsStore _settingsStore;
    private readonly IPrayerTimesCalculator _calculator;

    private const string AlarmAction = "com.qiblanow.PRAYER_ALARM";
    private const string PrayerTypeExtra = "prayer_type";
    private const string NotificationChannelId = "prayer_notifications";
    private const string NotificationChannelName = "Prayer Notifications";

    public AndroidNotificationScheduler(Context context, ISettingsStore settingsStore, IPrayerTimesCalculator calculator)
    {
        _context = context;
        _settingsStore = settingsStore;
        _calculator = calculator;
    }

    public async Task ScheduleNextNotificationAsync(NextNotificationCandidateResult candidate)
    {
        try
        {
            // Cancel any existing alarms
            await CancelAllNotificationsAsync();

            // Create notification channel for Android O+
            CreateNotificationChannel();

            // Create intent for alarm
            var intent = new Intent(AlarmAction);
            intent.SetPackage(_context.PackageName);
            intent.PutExtra(PrayerTypeExtra, (int)candidate.Type);

            // Use PendingIntent for alarm trigger
            var pendingIntent = PendingIntent.GetBroadcast(
                _context,
                0,
                intent,
                PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable | PendingIntentFlags.CancelCurrent);

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
                PendingIntentFlags.UpdateCurrent | PendingIntentFlags.CancelCurrent);

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
            var notificationSettings = _settingsStore.GetNotificationSettings();

            if (!notificationSettings.IsAnyEnabled)
            {
                // No notifications enabled, clear all
                await CancelAllNotificationsAsync();
                return;
            }

            var location = _settingsStore.GetLastValidLocation();
            if (location == null)
            {
                // No valid location, cannot schedule
                System.Diagnostics.Debug.WriteLine("No valid location for scheduling");
                return;
            }

            var calculationSettings = _settingsStore.GetCalculationSettings();
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
            // Show notification
            ShowPrayerNotification(prayerType);

            // Recalculate and schedule next notification
            _ = Task.Run(async () =>
            {
                var location = _settingsStore.GetLastValidLocation();
                if (location == null) return;

                var calculationSettings = _settingsStore.GetCalculationSettings();
                var notificationSettings = _settingsStore.GetNotificationSettings();

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

    private void CreateNotificationChannel()
    {
        if (Build.VERSION.SdkInt < BuildVersionCodes.O)
        {
            return;
        }

        var channel = new NotificationChannel(
            NotificationChannelId,
            NotificationChannelName,
            NotificationImportance.Default)
        {
            Description = "Prayer time notifications"
        };

        var notificationManager = _context.GetSystemService(Context.NotificationService) as NotificationManager;
        notificationManager?.CreateNotificationChannel(channel);
    }

    private void ShowPrayerNotification(PrayerType prayerType)
    {
        var prayerName = prayerType switch
        {
            PrayerType.Fajr => "Fajr",
            PrayerType.Sunrise => "Sunrise",
            PrayerType.Dhuhr => "Dhuhr",
            PrayerType.Asr => "Asr",
            PrayerType.Maghrib => "Maghrib",
            PrayerType.Isha => "Isha",
            _ => "Prayer"
        };

        var notification = new Notification.Builder(_context, NotificationChannelId)
            .SetSmallIcon(Resource.Drawable.MaterialIcons)
            .SetContentTitle(prayerName)
            .SetContentText("Time to pray")
            .SetPriority(NotificationPriority.Default)
            .Build();

        var notificationManager = _context.GetSystemService(Context.NotificationService) as NotificationManager;
        notificationManager?.Notify((int)prayerType, notification);
    }

    private void SaveSchedulingMetadata(PrayerType prayerType, DateTimeOffset scheduledTime)
    {
        try
        {
            // Save notification settings with this prayer enabled
            var settings = _settingsStore.GetNotificationSettings();
            settings.Reset();

            switch (prayerType)
            {
                case PrayerType.Fajr:
                    settings.FajrEnabled = true;
                    break;
                case PrayerType.Dhuhr:
                    settings.DhuhrEnabled = true;
                    break;
                case PrayerType.Asr:
                    settings.AsrEnabled = true;
                    break;
                case PrayerType.Maghrib:
                    settings.MaghribEnabled = true;
                    break;
                case PrayerType.Isha:
                    settings.IshaEnabled = true;
                    break;
            }

            _settingsStore.SaveNotificationSettings(settings);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error saving scheduling metadata: {ex.Message}");
        }
    }
}
