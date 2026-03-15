namespace QiblaNow.Core.Abstractions;

/// <summary>
/// Opens the platform's notification settings UI for the active prayer notification channel.
/// On Android 8.0+ this deep-links to the specific notification channel so the user can review
/// or override the sound that the system applies.  On other platforms this is a no-op.
/// </summary>
public interface INotificationSettingsOpener
{
    /// <summary>
    /// Opens the notification settings for the currently-selected Adhan channel.
    /// On Android this navigates to the channel detail screen inside system Settings.
    /// </summary>
    Task OpenChannelSettingsAsync();
}
