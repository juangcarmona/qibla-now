using QiblaNow.Core.Models;

namespace QiblaNow.Core.Abstractions;

/// <summary>
/// Plays or stops an Adhan sound preview from within the settings page.
/// Implementations are platform-specific; see AndroidAdhanPlayer on Android.
/// </summary>
public interface IAdhanPlayer
{
    /// <summary>
    /// Starts playing the given Adhan, stopping any active preview first.
    /// </summary>
    void Preview(AdhanSound sound);

    /// <summary>
    /// Stops any active preview immediately and releases audio resources.
    /// </summary>
    void StopPreview();
}
