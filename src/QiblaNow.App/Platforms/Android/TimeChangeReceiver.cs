using Android.App;
using Android.Content;
using Microsoft.Extensions.DependencyInjection;
using QiblaNow.Core.Abstractions;

namespace QiblaNow.App.Platforms.Android;

/// <summary>
/// BroadcastReceiver that reacts to timezone and time-set changes.
/// Reconciles prayer alarm scheduling so alarms remain accurate after
/// clock adjustments or travel across timezones.
/// Must be Exported=true so Android system can deliver these broadcasts.
/// </summary>
[BroadcastReceiver(Enabled = true, Exported = true)]
[IntentFilter(new[]
{
    "android.intent.action.TIMEZONE_CHANGED",
    "android.intent.action.TIME_SET"
})]
public class TimeChangeReceiver : BroadcastReceiver
{
    public override void OnReceive(Context? context, Intent? intent)
    {
        if (context == null) return;

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
                    System.Diagnostics.Debug.WriteLine("TimeChangeReceiver: INotificationScheduler not available");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"TimeChangeReceiver error: {ex.Message}");
            }
            finally
            {
                pending?.Finish();
            }
        });
    }
}
