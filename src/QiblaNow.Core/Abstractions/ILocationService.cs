using QiblaNow.Core.Models;

namespace QiblaNow.Core.Abstractions;

/// <summary>
/// Interface for location services (GPS or manual entry)
/// </summary>
public interface ILocationService
{
    /// <summary>
    /// Gets the current location snapshot
    /// </summary>
    /// <returns>Current location snapshot or null if not available</returns>
    Task<LocationSnapshot?> GetCurrentLocationAsync();

    /// <summary>
    /// Requests a one-time GPS location update
    /// </summary>
    Task<LocationSnapshot?> RequestGpsLocationAsync();

    /// <summary>
    /// Tries to acquire a live GPS fix within <paramref name="timeout"/>.
    /// Falls back to the stored snapshot if GPS is unavailable or times out.
    /// Returns null only when no location has ever been captured.
    /// </summary>
    Task<LocationSnapshot?> TryGetGpsLocationAsync(TimeSpan timeout);

    Task<LocationSnapshot> SaveManualLocationAsync(double latitude, double longitude);

    IReadOnlyList<SavedLocation> GetRecentLocations();

    Task<LocationSnapshot?> SelectRecentLocationAsync(double latitude, double longitude);
}
