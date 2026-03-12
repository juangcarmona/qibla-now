using Android.Content;
using Microsoft.Extensions.DependencyInjection;
using QiblaNow.Core.Abstractions;
using QiblaNow.Core.Models;

namespace QiblaNow.App.Platforms.Android;

[BroadcastReceiver(
    Enabled = true,
    Exported = false,
    Name = "com.jgcarmona.qiblanow.PrayerAlarmReceiver")]
public class PrayerAlarmReceiver : BroadcastReceiver
{
    internal const string ExtraPrayerType = "prayer_type";

    public override void OnReceive(Context? context, Intent? intent)
    {
        if (context == null || intent == null)
            return;

        var prayerTypeValue = intent.GetIntExtra(ExtraPrayerType, -1);
        if (prayerTypeValue < 0 || !Enum.IsDefined(typeof(PrayerType), prayerTypeValue))
            return;

        var prayerType = (PrayerType)prayerTypeValue;

        var pending = GoAsync();
        _ = Task.Run(async () =>
        {
            try
            {
                var scheduler = IPlatformApplication.Current?.Services
                    .GetService<INotificationScheduler>();

                if (scheduler != null)
                    await scheduler.HandleAlarmTriggeredAsync(prayerType);
                else
                    System.Diagnostics.Debug.WriteLine("PrayerAlarmReceiver: INotificationScheduler not available");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"PrayerAlarmReceiver error: {ex}");
            }
            finally
            {
                pending?.Finish();
            }
        });
    }
}