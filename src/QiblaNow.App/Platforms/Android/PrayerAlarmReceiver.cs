using Android.Content;
using Android.OS;
using QiblaNow.Core.Abstractions;

namespace QiblaNow.App.Platforms.Android;

/// <summary>
/// BroadcastReceiver that handles fired prayer alarm notifications
/// Registered in AndroidManifest.xml
/// </summary>
[BroadcastReceiver(Enabled = true, Exported = false)]
public class PrayerAlarmReceiver : BroadcastReceiver
{
    public override void OnReceive(Context? context, Intent? intent)
    {
        if (context == null || intent == null) return;

        try
        {
            // Extract prayer type from intent
            if (!intent.HasExtra("prayer_type"))
            {
                System.Diagnostics.Debug.WriteLine("PrayerAlarmReceiver: No prayer type in intent");
                return;
            }

            var prayerType = (PrayerType)intent.GetIntExtra("prayer_type", (int)PrayerType.Fajr);

            // Get INotificationScheduler from application context
            // TODO: Implement proper DI resolution for broadcast receivers
            // For now, return null - this needs to be handled differently
            // The scheduler should be accessible through the application instance
            _ = HandleAlarmFired(context, prayerType);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"PrayerAlarmReceiver error: {ex.Message}");
        }
    }

    private static Task HandleAlarmFired(Context context, PrayerType prayerType)
    {
        // TODO: Implement proper alarm handling
        // For now, just log the prayer time
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
        System.Diagnostics.Debug.WriteLine($"Prayer alarm fired: {prayerName}");

        // TODO: Show notification and schedule next alarm
        // This will be implemented after fixing the DI issue

        return Task.CompletedTask;
    }
}
