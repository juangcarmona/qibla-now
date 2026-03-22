using QiblaNow.Core.Models;

public interface ILocationService
{
    Task<LocationSnapshot?> GetCurrentLocationAsync(CancellationToken cancellationToken = default);
    Task<LocationSnapshot?> RequestGpsLocationAsync(CancellationToken cancellationToken = default);
    Task<LocationSnapshot?> TryGetGpsLocationAsync(TimeSpan timeout, CancellationToken cancellationToken = default);
    Task<LocationSnapshot> SaveManualLocationAsync(double latitude, double longitude, CancellationToken cancellationToken = default);
    IReadOnlyList<SavedLocation> GetRecentLocations();
    Task<LocationSnapshot?> SelectRecentLocationAsync(double latitude, double longitude, CancellationToken cancellationToken = default);
}