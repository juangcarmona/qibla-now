using QiblaNow.Core.Models;

namespace QiblaNow.Core.Abstractions;

/// <summary>
/// Interface for persisting and retrieving location settings
/// </summary>
public interface ISettingsStore
{
    /// <summary>
    /// Gets the current location mode (GPS or Manual)
    /// </summary>
    LocationMode GetLocationMode();

    /// <summary>
    /// Sets the current location mode
    /// </summary>
    void SetLocationMode(LocationMode mode);

    /// <summary>
    /// Gets the last captured location snapshot
    /// </summary>
    LocationSnapshot? GetLastSnapshot();

    /// <summary>
    /// Saves a location snapshot to persistent storage
    /// </summary>
    void SaveSnapshot(LocationSnapshot snapshot);

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

    /// <summary>
    /// Gets the persisted alarm scheduling state used for recovery after reboot or process death.
    /// </summary>
    SchedulingState GetSchedulingState();

    /// <summary>
    /// Persists alarm scheduling state for deterministic recovery.
    /// Must NOT modify user notification preference flags.
    /// </summary>
    void SaveSchedulingState(SchedulingState state);
}
