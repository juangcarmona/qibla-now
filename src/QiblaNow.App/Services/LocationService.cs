using QiblaNow.Core.Abstractions;
using QiblaNow.Core.Models;

namespace QiblaNow.App.Services;

public sealed class LocationService : ILocationService
{
    private readonly ISettingsStore _settings;

    public LocationService(ISettingsStore settings)
    {
        _settings = settings;
    }

    public Task<LocationSnapshot?> GetCurrentLocationAsync()
    {
        // Your persisted “source of truth”
        var mode = _settings.GetLocationMode();

        if (mode == LocationMode.Manual)
        {
            return Task.FromResult(_settings.GetLastSnapshot());
        }

        // GPS mode: return last snapshot if you have one; callers can force refresh via RequestGpsLocationAsync
        return Task.FromResult(_settings.GetLastSnapshot());
    }

    public async Task<LocationSnapshot?> RequestGpsLocationAsync()
    {
        var permission = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
        if (permission != PermissionStatus.Granted)
            return null;

        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));

            var request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10));
            var loc = await Geolocation.Default.GetLocationAsync(request, cts.Token);

            if (loc is null)
                return null;

            var label = await TryReverseGeocodeAsync(loc.Latitude, loc.Longitude);

            var snap = new LocationSnapshot(LocationMode.GPS, loc.Latitude, loc.Longitude, label)
            {
                Timestamp = DateTimeOffset.UtcNow
            };

            _settings.SaveSnapshot(snap);
            _settings.SetLocationMode(LocationMode.GPS);

            return snap;
        }
        catch (FeatureNotSupportedException)
        {
            return null;
        }
        catch (FeatureNotEnabledException)
        {
            return null;
        }
        catch (PermissionException)
        {
            return null;
        }
        catch (OperationCanceledException)
        {
            return null;
        }
    }

    private static async Task<string?> TryReverseGeocodeAsync(double lat, double lon)
    {
        try
        {
            var placemarks = await Geocoding.Default.GetPlacemarksAsync(lat, lon);
            var p = placemarks?.FirstOrDefault();
            if (p is null) return null;

            // Keep it deterministic and short
            // Example: "Paracuellos de Jarama, Community of Madrid"
            var parts = new[]
            {
                p.Locality,
                p.AdminArea
            }.Where(s => !string.IsNullOrWhiteSpace(s));

            var s = string.Join(", ", parts);
            return string.IsNullOrWhiteSpace(s) ? null : s;
        }
        catch
        {
            return null;
        }
    }
}