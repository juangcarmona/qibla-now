using Android.Content;
using Microsoft.Extensions.DependencyInjection;
using QiblaNow.Core.Abstractions;

namespace QiblaNow.App.Platforms.Android;

/// <summary>
/// BroadcastReceiver that restores prayer alarm scheduling after device reboot.
/// Must be Exported=true so the Android system can deliver BOOT_COMPLETED.
/// </summary>
[BroadcastReceiver(Enabled = true, Exported = true)]
[IntentFilter(new[] { "android.intent.action.BOOT_COMPLETED" })]
public class BootReceiver : BroadcastReceiver
{
    public override void OnReceive(Context? context, Intent? intent)
    {
        if (context == null || intent?.Action != "android.intent.action.BOOT_COMPLETED")
            return;

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
                    System.Diagnostics.Debug.WriteLine("BootReceiver: INotificationScheduler not available");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"BootReceiver error: {ex.Message}");
            }
            finally
            {
                pending.Finish();
            }
        });
    }
}

