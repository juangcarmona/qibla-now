using Android.Content;
using Android.OS;
using QiblaNow.Core.Abstractions;

namespace QiblaNow.App.Platforms.Android;

/// <summary>
/// BroadcastReceiver that handles device boot completion
/// Registered in AndroidManifest.xml
/// </summary>
[BroadcastReceiver(Enabled = true, Exported = false)]
[IntentFilter(new[] { "android.intent.action.BOOT_COMPLETED" })]
public class BootReceiver : BroadcastReceiver
{
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

            // Handle boot completion
            // TODO: Implement proper boot handling
            // For now, just log the boot event
            System.Diagnostics.Debug.WriteLine("Device booted, will reconcile scheduling");

            // TODO: Call scheduler to reconcile scheduling after reboot
            // This will be implemented after fixing the DI issue
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"BootReceiver error: {ex.Message}");
        }
    }
}
