using QiblaNow.Core.Abstractions.Models;

namespace QiblaNow.Core.Abstractions.Services;

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
}
