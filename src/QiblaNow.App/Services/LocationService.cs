using System.Globalization;
using QiblaNow.App;
using QiblaNow.Core.Abstractions;
using QiblaNow.Core.Models;

namespace QiblaNow.App.Services;

public sealed class LocationService : ILocationService
{
    private readonly ISettingsStore _settings;
    private readonly IReverseGeocodingService _reverseGeocodingService;
    private readonly ISavedLocationStore _savedLocationStore;

    public LocationService(
        ISettingsStore settings,
        IReverseGeocodingService reverseGeocodingService,
        ISavedLocationStore savedLocationStore)
    {
        _settings = settings;
        _reverseGeocodingService = reverseGeocodingService;
        _savedLocationStore = savedLocationStore;
    }

    public Task<LocationSnapshot?> GetCurrentLocationAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var mode = _settings.GetLocationMode();

        if (mode == LocationMode.Manual)
            return Task.FromResult(_settings.GetLastSnapshot());

        // GPS mode: return last snapshot if available.
        // Callers can force a live refresh via RequestGpsLocationAsync / TryGetGpsLocationAsync.
        return Task.FromResult(_settings.GetLastSnapshot());
    }

    public async Task<LocationSnapshot?> RequestGpsLocationAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var permission = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
        if (permission != PermissionStatus.Granted)
            return null;

        try
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(15));

            var request = new GeolocationRequest(
                GeolocationAccuracy.Medium,
                TimeSpan.FromSeconds(10));

            var loc = await Geolocation.Default.GetLocationAsync(request, cts.Token);

            if (loc is null)
                return null;

            var label = await ResolveLocationLabelAsync(loc.Latitude, loc.Longitude, cts.Token);

            var snap = new LocationSnapshot(LocationMode.GPS, loc.Latitude, loc.Longitude, label)
            {
                Timestamp = DateTimeOffset.UtcNow
            };

            _settings.SaveSnapshot(snap);
            _settings.SetLocationMode(LocationMode.GPS);
            _savedLocationStore.UpsertRecentLocation(
                new SavedLocation(label, loc.Latitude, loc.Longitude, DateTimeOffset.UtcNow));

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

    public async Task<LocationSnapshot?> TryGetGpsLocationAsync(
        TimeSpan timeout,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var mode = _settings.GetLocationMode();
        if (mode == LocationMode.Manual)
            return _settings.GetLastSnapshot();

        var permission = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
        if (permission != PermissionStatus.Granted)
            permission = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();

        if (permission == PermissionStatus.Granted)
        {
            try
            {
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                cts.CancelAfter(timeout);

                var request = new GeolocationRequest(GeolocationAccuracy.Medium, timeout);
                var loc = await Geolocation.Default.GetLocationAsync(request, cts.Token);

                if (loc is not null)
                {
                    var label = await ResolveLocationLabelAsync(loc.Latitude, loc.Longitude, cts.Token);

                    var snap = new LocationSnapshot(LocationMode.GPS, loc.Latitude, loc.Longitude, label)
                    {
                        Timestamp = DateTimeOffset.UtcNow
                    };

                    _settings.SaveSnapshot(snap);
                    _settings.SetLocationMode(LocationMode.GPS);
                    _savedLocationStore.UpsertRecentLocation(
                        new SavedLocation(label, loc.Latitude, loc.Longitude, DateTimeOffset.UtcNow));

                    return snap;
                }
            }
            catch (FeatureNotSupportedException)
            {
            }
            catch (FeatureNotEnabledException)
            {
            }
            catch (PermissionException)
            {
            }
            catch (OperationCanceledException)
            {
            }
        }

        return _settings.GetLastSnapshot();
    }

    public async Task<LocationSnapshot> SaveManualLocationAsync(
        double latitude,
        double longitude,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var label = await ResolveLocationLabelAsync(latitude, longitude, cancellationToken);

        var snapshot = new LocationSnapshot(LocationMode.Manual, latitude, longitude, label)
        {
            Timestamp = DateTimeOffset.UtcNow
        };

        _settings.SetLocationMode(LocationMode.Manual);
        _settings.SaveSnapshot(snapshot);
        _savedLocationStore.UpsertRecentLocation(
            new SavedLocation(label, latitude, longitude, DateTimeOffset.UtcNow));

        return snapshot;
    }

    public IReadOnlyList<SavedLocation> GetRecentLocations()
        => _savedLocationStore.GetRecentLocations();

    public async Task<LocationSnapshot?> SelectRecentLocationAsync(
        double latitude,
        double longitude,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var existing = _savedLocationStore.GetRecentLocations()
            .FirstOrDefault(x =>
                ReverseGeocodingHelper.RoundCoordinate(x.Latitude) == ReverseGeocodingHelper.RoundCoordinate(latitude) &&
                ReverseGeocodingHelper.RoundCoordinate(x.Longitude) == ReverseGeocodingHelper.RoundCoordinate(longitude));

        if (existing is null)
            return null;

        var label = string.IsNullOrWhiteSpace(existing.Name)
            ? await ResolveLocationLabelAsync(latitude, longitude, cancellationToken)
            : existing.Name;

        var snapshot = new LocationSnapshot(LocationMode.Manual, latitude, longitude, label)
        {
            Timestamp = DateTimeOffset.UtcNow
        };

        _settings.SetLocationMode(LocationMode.Manual);
        _settings.SaveSnapshot(snapshot);
        _savedLocationStore.UpsertRecentLocation(
            new SavedLocation(label, latitude, longitude, DateTimeOffset.UtcNow));

        return snapshot;
    }

    private async Task<string> ResolveLocationLabelAsync(
        double lat,
        double lon,
        CancellationToken cancellationToken = default)
    {
        var language = LocalizationHelper.GetSavedLanguageCode();
        if (string.IsNullOrWhiteSpace(language))
            language = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;

        var place = await _reverseGeocodingService.ReverseGeocodeAsync(
            lat,
            lon,
            language,
            cancellationToken);

        return ReverseGeocodingHelper.SelectDisplayName(place, lat, lon);
    }
}