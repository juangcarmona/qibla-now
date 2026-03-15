namespace QiblaNow.Core.Abstractions;

/// <summary>
/// No-op implementation of <see cref="INotificationSettingsOpener"/> for platforms
/// that do not expose per-channel notification settings (iOS, Windows, macOS).
/// </summary>
public sealed class NullNotificationSettingsOpener : INotificationSettingsOpener
{
    public Task OpenChannelSettingsAsync() => Task.CompletedTask;
}
