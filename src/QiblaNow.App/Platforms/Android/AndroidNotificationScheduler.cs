using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using AndroidX.Core.App;
using QiblaNow.Core.Abstractions;
using QiblaNow.Core.Models;

namespace QiblaNow.App.Platforms.Android;

public sealed class AndroidNotificationScheduler : INotificationScheduler
{
    private readonly Context _context;
    private readonly ISettingsStore _settingsStore;
    private readonly IPrayerTimesCalculator _calculator;

    private const string PrayerTypeExtra = "prayer_type";
    private const string NotificationChannelId = "prayer_notifications";
    private const string NotificationChannelName = "Prayer Notifications";

    public AndroidNotificationScheduler(
        Context context,
        ISettingsStore settingsStore,
        IPrayerTimesCalculator calculator)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _settingsStore = settingsStore ?? throw new ArgumentNullException(nameof(settingsStore));
        _calculator = calculator ?? throw new ArgumentNullException(nameof(calculator));
    }

    public async Task ScheduleNextNotificationAsync(NextNotificationCandidateResult candidate)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(candidate);

            await CancelAllNotificationsAsync();
            CreateNotificationChannel();

            var pendingIntent = CreateAlarmPendingIntent(candidate.Type);

            var alarmManager = _context.GetSystemService(Context.AlarmService) as AlarmManager;
            if (alarmManager == null)
            {
                System.Diagnostics.Debug.WriteLine("AlarmManager unavailable.");
                return;
            }

            var triggerAtMillis = candidate.ScheduledTime.ToUnixTimeMilliseconds();

            if (Build.VERSION.SdkInt >= BuildVersionCodes.S && !alarmManager.CanScheduleExactAlarms())
            {
                System.Diagnostics.Debug.WriteLine("Exact alarm permission not granted; falling back to inexact alarm.");
                alarmManager.SetAndAllowWhileIdle(AlarmType.RtcWakeup, triggerAtMillis, pendingIntent);
            }
            else
            {
                alarmManager.SetExactAndAllowWhileIdle(AlarmType.RtcWakeup, triggerAtMillis, pendingIntent);
            }

            _settingsStore.SaveSchedulingState(new SchedulingState
            {
                LastPlannedPrayer = candidate.Type.ToString(),
                LastPlannedTriggerUtc = triggerAtMillis,
                LastPlannedRequestCode = (int)candidate.Type,
                LastReconciledUtc = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error scheduling notification: {ex}");
            throw;
        }
    }

    public Task CancelAllNotificationsAsync()
    {
        try
        {
            var alarmManager = _context.GetSystemService(Context.AlarmService) as AlarmManager;
            if (alarmManager == null)
            {
                System.Diagnostics.Debug.WriteLine("AlarmManager unavailable during cancellation.");
                return Task.CompletedTask;
            }

            foreach (PrayerType prayerType in Enum.GetValues<PrayerType>())
            {
                if (prayerType == PrayerType.Sunrise)
                    continue;

                var pendingIntent = CreateAlarmPendingIntent(prayerType);
                alarmManager.Cancel(pendingIntent);
                pendingIntent.Cancel();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error canceling notifications: {ex}");
            throw;
        }

        return Task.CompletedTask;
    }

    public async Task ReconcileOnStartupAsync()
    {
        try
        {
            var notificationSettings = _settingsStore.GetNotificationSettings();

            if (!notificationSettings.IsAnyEnabled)
            {
                await CancelAllNotificationsAsync();
                return;
            }

            var location = _settingsStore.GetLastValidLocation();
            if (location == null)
            {
                System.Diagnostics.Debug.WriteLine("No valid location for scheduling.");
                return;
            }

            var calculationSettings = _settingsStore.GetCalculationSettings();
            var now = DateTimeOffset.UtcNow;

            var schedule = await _calculator.CalculateDailyScheduleAsync(location, now, calculationSettings);
            var candidate = await _calculator.CalculateNextNotificationCandidateAsync(
                schedule,
                notificationSettings,
                now);

            if (candidate != null)
                await ScheduleNextNotificationAsync(candidate);
            else
                System.Diagnostics.Debug.WriteLine("No next notification candidate found.");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error reconciling on startup: {ex}");
            throw;
        }
    }

    public async Task HandleAlarmTriggeredAsync(PrayerType prayerType)
    {
        try
        {
            ShowPrayerNotification(prayerType);
            await ReconcileOnStartupAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error handling fired alarm: {ex}");
            throw;
        }
    }

    private PendingIntent CreateAlarmPendingIntent(PrayerType prayerType)
    {
        var intent = new Intent(_context, typeof(PrayerAlarmReceiver));
        intent.PutExtra(PrayerTypeExtra, (int)prayerType);

        return PendingIntent.GetBroadcast(
            _context,
            (int)prayerType,
            intent,
            PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);
    }

    private void CreateNotificationChannel()
    {
        if (Build.VERSION.SdkInt < BuildVersionCodes.O)
            return;

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
        CreateNotificationChannel();

        if (Build.VERSION.SdkInt >= BuildVersionCodes.Tiramisu &&
            _context.CheckSelfPermission(global::Android.Manifest.Permission.PostNotifications) != Permission.Granted)
        {
            System.Diagnostics.Debug.WriteLine("POST_NOTIFICATIONS not granted; notification suppressed.");
            return;
        }

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

        var builder = new NotificationCompat.Builder(_context, NotificationChannelId);
        builder.SetSmallIcon(Resource.Drawable.notification_icon);
        builder.SetContentTitle(prayerName);
        builder.SetContentText("Time to pray");
        builder.SetPriority((int)NotificationPriority.Default);
        builder.SetAutoCancel(true);

        var notification = builder.Build();
        if (notification == null)
        {
            System.Diagnostics.Debug.WriteLine("Failed to build notification.");
            return;
        }

        var manager = NotificationManagerCompat.From(_context);
        manager.Notify((int)prayerType, notification);
    }
}