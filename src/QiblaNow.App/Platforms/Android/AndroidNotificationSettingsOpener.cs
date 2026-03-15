using Android.Content;
using Android.Provider;
using QiblaNow.Core.Abstractions;
using QiblaNow.Core.Models;

namespace QiblaNow.App.Platforms.Android;

/// <summary>
/// Opens the Android notification channel settings screen for the currently-selected
/// Adhan prayer channel.  This lets users inspect or override the sound that their device
/// applies to prayer notifications.
///
/// Android 8.0+ (API 26) locks channel sounds after first creation.  If the user wants a
/// different sound than the app provides they can change it here, but only at the OS level.
/// </summary>
public sealed class AndroidNotificationSettingsOpener : INotificationSettingsOpener
{
    private readonly Context _context;
    private readonly ISettingsStore _settingsStore;

    // Channel IDs must stay in sync with AndroidNotificationScheduler.
    private const string ChannelIdDefault = "prayer_default";
    private const string ChannelIdAdhan1  = "prayer_adhan1";
    private const string ChannelIdAdhan2  = "prayer_adhan2";
    private const string ChannelIdAdhan3  = "prayer_adhan3";

    public AndroidNotificationSettingsOpener(Context context, ISettingsStore settingsStore)
    {
        _context       = context       ?? throw new ArgumentNullException(nameof(context));
        _settingsStore = settingsStore ?? throw new ArgumentNullException(nameof(settingsStore));
    }

    public Task OpenChannelSettingsAsync()
    {
        try
        {
            Intent intent;

            if (OperatingSystem.IsAndroidVersionAtLeast(26))
            {
                // Deep-link directly to the notification channel detail screen (API 26+).
                var channelId = GetActiveChannelId();
                intent = new Intent(Settings.ActionChannelNotificationSettings);
                intent.PutExtra(Settings.ExtraAppPackage, _context.PackageName);
                intent.PutExtra(Settings.ExtraChannelId,  channelId);
            }
            else
            {
                // Fallback: open the app's notification settings page (API < 26).
                intent = new Intent(Settings.ActionApplicationDetailsSettings);
                intent.SetData(
                    global::Android.Net.Uri.Parse($"package:{_context.PackageName}"));
            }

            intent.SetFlags(ActivityFlags.NewTask);
            _context.StartActivity(intent);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(
                $"AndroidNotificationSettingsOpener: could not open settings: {ex}");
        }

        return Task.CompletedTask;
    }

    private string GetActiveChannelId()
    {
        var adhan = _settingsStore.GetNotificationSettings().SelectedAdhan;
        return adhan switch
        {
            AdhanSound.Adhan1  => ChannelIdAdhan1,
            AdhanSound.Adhan2  => ChannelIdAdhan2,
            AdhanSound.Adhan3  => ChannelIdAdhan3,
            AdhanSound.Default => ChannelIdDefault,
            // Unrecognised value (e.g. from a future enum addition): fall back to Adhan1
            // so the user lands on a working channel rather than an unknown one.
            _                  => ChannelIdAdhan1,
        };
    }
}
