using QiblaNow.Core.Models;

namespace QiblaNow.Core.Abstractions;

/// <summary>
/// Interface for persisting and retrieving prayer-related settings
/// </summary>
public interface IPrayerSettingsStore
{
    /// <summary>
    /// Gets the current prayer calculation settings
    /// </summary>
    PrayerCalculationSettings GetCalculationSettings();

    /// <summary>
    /// Saves prayer calculation settings
    /// </summary>
    void SaveCalculationSettings(PrayerCalculationSettings settings);

    /// <summary>
    /// Gets the current prayer notification settings
    /// </summary>
    PrayerNotificationSettings GetNotificationSettings();

    /// <summary>
    /// Saves prayer notification settings
    /// </summary>
    void SaveNotificationSettings(PrayerNotificationSettings settings);

    /// <summary>
    /// Gets the last known valid location for prayer calculation
    /// </summary>
    LocationSnapshot? GetLastValidLocation();

    /// <summary>
    /// Saves the last known valid location
    /// </summary>
    void SaveLastValidLocation(LocationSnapshot location);
}
