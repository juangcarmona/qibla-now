using QiblaNow.Core.Models;

namespace QiblaNow.Core.Abstractions;

/// <summary>
/// Controls alarm-style Adhan playback at prayer time.
/// Unlike <see cref="IAdhanPlayer"/> (which is used for preview in settings),
/// this interface drives the full prayer-alert experience.
/// </summary>
public interface IAdhanAlarmPlayer
{
    /// <summary>
    /// Starts playing the given Adhan as an alarm.
    /// Stops any already-running playback before starting the new one.
    /// </summary>
    void Play(AdhanSound sound);

    /// <summary>
    /// Stops any active alarm playback and releases audio resources.
    /// </summary>
    void Stop();

    /// <summary>
    /// Returns true when an Adhan is currently being played as an alarm.
    /// </summary>
    bool IsPlaying { get; }
}
