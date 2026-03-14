namespace QiblaNow.Core.Models;

/// <summary>
/// Identifies which Adhan audio clip to play for prayer notifications.
/// Each custom value maps to an Android raw resource file (no extension).
/// </summary>
public enum AdhanSound
{
    Default = 0,    // System default notification tone
    Adhan1  = 1,    // raw/adhan.mp3
    Adhan2  = 2,    // raw/adhan2.mp3
    Adhan3  = 3,    // raw/adhan3.mp3
}
