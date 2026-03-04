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
}
