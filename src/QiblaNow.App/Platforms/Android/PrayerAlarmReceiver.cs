using Android.Content;
using Android.OS;
using QiblaNow.Core.Abstractions;
using QiblaNow.Core.Models;

namespace QiblaNow.App.Platforms.Android;

/// <summary>
/// BroadcastReceiver that handles fired prayer alarm notifications
/// </summary>
[BroadcastReceiver(Enabled = true, Exported = false)]
public class PrayerAlarmReceiver : BroadcastReceiver
{
    private INotificationScheduler? _scheduler;

    public override void OnReceive(Context? context, Intent? intent)
    {
        if (context == null || intent == null) return;

        try
        {
            // Extract prayer type from intent
            if (!intent.HasExtra(PrayerTypeExtra))
            {
                System.Diagnostics.Debug.WriteLine("PrayerAlarmReceiver: No prayer type in intent");
                return;
            }

            var prayerType = (PrayerType)intent.GetIntExtra(PrayerTypeExtra, (int)PrayerType.Fajr);

            // Initialize scheduler from dependency injection
            _scheduler ??= Android.App.Platform.CurrentActivity?
                .Services.GetService(typeof(INotificationScheduler)) as INotificationScheduler;

            if (_scheduler == null)
            {
                System.Diagnostics.Debug.WriteLine("PrayerAlarmReceiver: Could not get scheduler instance");
                return;
            }

            // Handle alarm firing
            _scheduler.HandleAlarmFiredAsync(prayerType).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"PrayerAlarmReceiver error: {ex.Message}");
        }
    }
}
