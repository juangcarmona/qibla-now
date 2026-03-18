using QiblaNow.Core.Models;

namespace QiblaNow.Core.Abstractions;

/// <summary>
/// No-op implementation of <see cref="IAdhanAlarmPlayer"/> for platforms that
/// do not support alarm playback (iOS, Windows, MacCatalyst).
/// </summary>
public sealed class NullAdhanAlarmPlayer : IAdhanAlarmPlayer
{
    public bool IsPlaying => false;
    public void Play(AdhanSound sound) { }
    public void Stop() { }
}
