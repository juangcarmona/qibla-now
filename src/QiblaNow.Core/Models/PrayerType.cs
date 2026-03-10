namespace QiblaNow.Core.Models;

/// <summary>
/// Represents a daily prayer
/// Sunrise is calculated but NOT a notification target in this milestone
/// </summary>
public enum PrayerType
{
    /// <summary>
    /// Dawn prayer - first of the day
    /// </summary>
    Fajr,

    /// <summary>
    /// Sunrise - calculated but not a notification target
    /// </summary>
    Sunrise,

    /// <summary>
    /// Noon prayer
    /// </summary>
    Dhuhr,

    /// <summary>
    /// Afternoon prayer
    /// </summary>
    Asr,

    /// <summary>
    /// Sunset prayer
    /// </summary>
    Maghrib,

    /// <summary>
    /// Night prayer
    /// </summary>
    Isha
}
