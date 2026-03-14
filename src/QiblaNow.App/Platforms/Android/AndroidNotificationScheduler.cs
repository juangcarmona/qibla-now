using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Media;
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

    // One channel per selectable sound because Android 8.0+ locks a channel's sound URI
    // after the first createNotificationChannel() call.  Changing sound requires a new
    // channel ID — that is the only supported mechanism without user intervention.
    private const string ChannelIdDefault = "prayer_default";
    private const string ChannelIdAdhan1  = "prayer_adhan1";
    private const string ChannelIdAdhan2  = "prayer_adhan2";
    private const string ChannelIdAdhan3  = "prayer_adhan3";

    // Legacy channel created by earlier app versions without a sound URI.
    // Not deleted (Android does not guarantee a clean delete) but no longer used
    // for posting new notifications.
    private const string ChannelIdLegacy  = "prayer_notifications";

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
            CreateNotificationChannels();

            var pendingIntent = CreateAlarmPendingIntent(candidate.Type);

            var alarmManager = _context.GetSystemService(Context.AlarmService) as AlarmManager;
            if (alarmManager == null)
            {
                System.Diagnostics.Debug.WriteLine("AlarmManager unavailable.");
                return;
            }

            var triggerAtMillis = candidate.ScheduledTime.ToUnixTimeMilliseconds();

            if (OperatingSystem.IsAndroidVersionAtLeast(31) && !alarmManager.CanScheduleExactAlarms())
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

                var pendingIntent = FindExistingAlarmPendingIntent(prayerType);
                if (pendingIntent != null)
                {
                    alarmManager.Cancel(pendingIntent);
                    pendingIntent.Cancel();
                }
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
            PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable)!;
    }

    private PendingIntent? FindExistingAlarmPendingIntent(PrayerType prayerType)
    {
        var intent = new Intent(_context, typeof(PrayerAlarmReceiver));
        intent.PutExtra(PrayerTypeExtra, (int)prayerType);

        return PendingIntent.GetBroadcast(
            _context,
            (int)prayerType,
            intent,
            PendingIntentFlags.NoCreate | PendingIntentFlags.Immutable);
    }

    // ── Notification channels ─────────────────────────────────────────────────

    private void CreateNotificationChannels()
    {
        if (!OperatingSystem.IsAndroidVersionAtLeast(26))
            return;

        var notificationManager = _context.GetSystemService(Context.NotificationService) as NotificationManager;
        if (notificationManager == null)
            return;

        // AudioAttributes required when setting a custom sound on a channel (API 21+)
        var audioAttrs = new AudioAttributes.Builder()
            .SetUsage(AudioUsageKind.Notification)!
            .SetContentType(AudioContentType.Sonification)!
            .Build()!;

        EnsureChannel(
            notificationManager, ChannelIdDefault, "Prayer Notifications (Default)",
            RingtoneManager.GetDefaultUri(RingtoneType.Notification),
            audioAttrs);

        EnsureChannel(
            notificationManager, ChannelIdAdhan1, "Prayer Notifications (Adhan 1)",
            BuildRawUri("adhan"),
            audioAttrs);

        EnsureChannel(
            notificationManager, ChannelIdAdhan2, "Prayer Notifications (Adhan 2)",
            BuildRawUri("adhan2"),
            audioAttrs);

        EnsureChannel(
            notificationManager, ChannelIdAdhan3, "Prayer Notifications (Adhan 3)",
            BuildRawUri("adhan3"),
            audioAttrs);
    }

    private void EnsureChannel(
        NotificationManager manager,
        string id,
        string name,
        global::Android.Net.Uri? soundUri,
        AudioAttributes attrs)
    {
        // createNotificationChannel is idempotent by ID — existing channel settings
        // (including sound) are NOT overwritten.  A distinct ID per sound means the
        // correct URI is locked in on first install.
        var channel = new NotificationChannel(id, name, NotificationImportance.High)
        {
            Description = "Prayer time notifications"
        };

        if (soundUri != null)
            channel.SetSound(soundUri, attrs);

        manager.CreateNotificationChannel(channel);
    }

    private global::Android.Net.Uri BuildRawUri(string rawName) =>
        global::Android.Net.Uri.Parse(
            $"android.resource://{_context.PackageName}/raw/{rawName}")!;

    private string GetActiveChannelId()
    {
        var adhan = _settingsStore.GetNotificationSettings().SelectedAdhan;
        return adhan switch
        {
            AdhanSound.Adhan1  => ChannelIdAdhan1,
            AdhanSound.Adhan2  => ChannelIdAdhan2,
            AdhanSound.Adhan3  => ChannelIdAdhan3,
            AdhanSound.Default => ChannelIdDefault,
            _                  => ChannelIdAdhan1,
        };
    }

    private void ShowPrayerNotification(PrayerType prayerType)
    {
        CreateNotificationChannels();

        if (OperatingSystem.IsAndroidVersionAtLeast(33) &&
            _context.CheckSelfPermission(global::Android.Manifest.Permission.PostNotifications) != Permission.Granted)
        {
            System.Diagnostics.Debug.WriteLine("POST_NOTIFICATIONS not granted; notification suppressed.");
            return;
        }

        var prayerName = prayerType switch
        {
            PrayerType.Fajr    => "Fajr",
            PrayerType.Sunrise => "Sunrise",
            PrayerType.Dhuhr   => "Dhuhr",
            PrayerType.Asr     => "Asr",
            PrayerType.Maghrib => "Maghrib",
            PrayerType.Isha    => "Isha",
            _                  => "Prayer"
        };

        var timeText = DateTimeOffset.Now.ToString("HH:mm");

        // Select the channel that matches the user's Adhan preference.
        // The channel's sound URI was locked in at creation time (see CreateNotificationChannels).
        var channelId = GetActiveChannelId();
        var builder = new NotificationCompat.Builder(_context, channelId);
        builder.SetSmallIcon(Resource.Drawable.ic_prayer_notification);
        builder.SetContentTitle($"{prayerName} — {timeText}");
        builder.SetContentText($"It's time for {prayerName}");
        builder.SetPriority((int)NotificationPriority.High);
        builder.SetCategory(NotificationCompat.CategoryAlarm);
        builder.SetVisibility((int)NotificationVisibility.Public);
        builder.SetAutoCancel(true);

        // Tap opens the main activity
        var tapIntent = _context.PackageManager?.GetLaunchIntentForPackage(_context.PackageName ?? "");
        if (tapIntent != null)
        {
            tapIntent.SetFlags(ActivityFlags.ClearTop | ActivityFlags.SingleTop);
            var tapPendingIntent = PendingIntent.GetActivity(
                _context, 0, tapIntent,
                PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);
            builder.SetContentIntent(tapPendingIntent);
        }

        var notification = builder.Build();
        if (notification == null)
        {
            System.Diagnostics.Debug.WriteLine("Failed to build notification.");
            return;
        }

        var manager = NotificationManagerCompat.From(_context);
        if (manager == null)
        {
            System.Diagnostics.Debug.WriteLine("NotificationManagerCompat unavailable.");
            return;
        }
        manager.Notify((int)prayerType, notification);
    }
}