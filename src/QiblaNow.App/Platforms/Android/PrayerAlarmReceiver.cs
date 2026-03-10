using Android.Content;
using Microsoft.Extensions.DependencyInjection;
using QiblaNow.Core.Abstractions;

namespace QiblaNow.App.Platforms.Android;

/// <summary>
/// BroadcastReceiver that fires when a scheduled prayer alarm triggers.
/// Shows the notification and schedules the next alarm via ReconcileOnStartupAsync.
/// Exported=false: only the AlarmManager (same app) can trigger this.
/// </summary>
[BroadcastReceiver(
    Enabled = true,
    Exported = false,
    Name = "com.jgcarmona.qiblanow.PrayerAlarmReceiver")]
public class PrayerAlarmReceiver : BroadcastReceiver
{
    internal const string ExtraPrayerType = "prayer_type";

    public override void OnReceive(Context? context, Intent? intent)
    {
        if (context == null || intent == null) return;

        var pending = GoAsync();
        _ = Task.Run(async () =>
        {
            try
            {
                var scheduler = IPlatformApplication.Current?.Services
                    .GetService<INotificationScheduler>();

                if (scheduler != null)
                    await scheduler.ReconcileOnStartupAsync();
                else
                    System.Diagnostics.Debug.WriteLine("PrayerAlarmReceiver: INotificationScheduler not available");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"PrayerAlarmReceiver error: {ex.Message}");
            }
            finally
            {
                pending.Finish();
            }
        });
    }
}
