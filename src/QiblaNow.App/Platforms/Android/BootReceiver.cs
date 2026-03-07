using Android.Content;
using Android.OS;
using QiblaNow.Core.Abstractions;
using QiblaNow.Core.Models;

namespace QiblaNow.App.Platforms.Android;

/// <summary>
/// BroadcastReceiver that handles device boot completion
/// </summary>
[BroadcastReceiver(Enabled = true, Exported = false)]
[IntentFilter(new[] { "android.intent.action.BOOT_COMPLETED" })]
public class BootReceiver : BroadcastReceiver
{
    private INotificationScheduler? _scheduler;

    public override void OnReceive(Context? context, Intent? intent)
    {
        if (context == null || intent == null) return;

        try
        {
            // Only handle BOOT_COMPLETED action
            if (intent.Action != "android.intent.action.BOOT_COMPLETED")
            {
                return;
            }

            // Initialize scheduler from dependency injection
            _scheduler ??= Android.App.Platform.CurrentActivity?
                .Services.GetService(typeof(INotificationScheduler)) as INotificationScheduler;

            if (_scheduler == null)
            {
                System.Diagnostics.Debug.WriteLine("BootReceiver: Could not get scheduler instance");
                return;
            }

            // Handle boot completion
            _scheduler.HandleBootCompletedAsync().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"BootReceiver error: {ex.Message}");
        }
    }
}
