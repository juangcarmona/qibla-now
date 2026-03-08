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

            // Use PendingIntent for alarm trigger — stable request code per prayer type
            var pendingIntent = PendingIntent.GetBroadcast(
                _context,
                (int)candidate.Type,
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

            // Persist scheduling metadata for reconciliation after reboot/kill
            _settingsStore.SaveSchedulingState(new SchedulingState
            {
                LastPlannedPrayer = candidate.Type.ToString(),
                LastPlannedTriggerUtc = candidate.ScheduledTime.ToUnixTimeMilliseconds(),
                LastPlannedRequestCode = (int)candidate.Type,
                LastReconciledUtc = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            });
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
            // Show notification if a scheduled alarm just fired (within last 30 minutes)
            var state = _settingsStore.GetSchedulingState();
            if (state.LastPlannedTriggerUtc.HasValue && state.LastPlannedPrayer != null)
            {
                var triggerTime = DateTimeOffset.FromUnixTimeMilliseconds(state.LastPlannedTriggerUtc.Value);
                var utcNow = DateTimeOffset.UtcNow;
                if (triggerTime <= utcNow && (utcNow - triggerTime).TotalMinutes < 30)
                {
                    if (Enum.TryParse<PrayerType>(state.LastPlannedPrayer, out var firedPrayer))
                        ShowPrayerNotification(firedPrayer);
                }
            }

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
            var candidate = await _calculator.CalculateNextNotificationCandidateAsync(schedule, notificationSettings, date);

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

}
