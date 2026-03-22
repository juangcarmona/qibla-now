using QiblaNow.Core.Models;

namespace QiblaNow.Core.Abstractions;

public interface IReverseGeocodingService
{
    Task<ResolvedPlace?> ReverseGeocodeAsync(double latitude, double longitude, string language, CancellationToken cancellationToken);
}
