namespace QiblaNow.Core.Models;

/// <summary>
/// Settings for per-prayer notification toggles
/// Only Fajr, Dhuhr, Asr, Maghrib, and Isha are notification targets
/// </summary>
public sealed class PrayerNotificationSettings
{
    public bool FajrEnabled { get; set; } = true;
    public bool DhuhrEnabled { get; set; } = true;
    public bool AsrEnabled { get; set; } = true;
    public bool MaghribEnabled { get; set; } = true;
    public bool IshaEnabled { get; set; } = true;

    /// <summary>
    /// Checks if any prayer notification is enabled
    /// </summary>
    public bool IsAnyEnabled => FajrEnabled || DhuhrEnabled || AsrEnabled || MaghribEnabled || IshaEnabled;

    /// <summary>
    /// Gets all enabled prayer types
    /// </summary>
    public IEnumerable<PrayerType> GetEnabledTypes()
    {
        if (FajrEnabled) yield return PrayerType.Fajr;
        if (DhuhrEnabled) yield return PrayerType.Dhuhr;
        if (AsrEnabled) yield return PrayerType.Asr;
        if (MaghribEnabled) yield return PrayerType.Maghrib;
        if (IshaEnabled) yield return PrayerType.Isha;
    }

    /// <summary>
    /// Resets all enabled flags to false
    /// </summary>
    public void Reset()
    {
        FajrEnabled = false;
        DhuhrEnabled = false;
        AsrEnabled = false;
        MaghribEnabled = false;
        IshaEnabled = false;
    }
}
