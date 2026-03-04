using Android.Content;
using Android.Locations;
using Android.OS;
using QiblaNow.Core.Abstractions.Models;
using QiblaNow.Core.Abstractions.Services;
using QiblaNow.Core.Services;
using Exception = System.Exception;

namespace QiblaNow.App.Droid.Services;

/// <summary>
/// Android implementation of ILocationService using GPS and Geocoder
/// </summary>
public sealed class LocationService : ILocationService
{
    private readonly Context _context;
    private readonly LocationManager _locationManager;
    private readonly ISettingsStore _settingsStore;
    private CancellationTokenSource? _cancellationTokenSource;

    public LocationService(Context context, ISettingsStore settingsStore)
    {
        _context = context;
        _locationManager = context.GetSystemService(Context.LocationService) as LocationManager ?? throw new ArgumentNullException(nameof(context));
        _settingsStore = settingsStore;
    }

    public async Task<LocationSnapshot?> GetCurrentLocationAsync()
    {
        // Try to get last saved snapshot first
        var lastSnapshot = _settingsStore.GetLastSnapshot();
        if (lastSnapshot != null)
        {
            return lastSnapshot;
        }

        // Fall back to GPS request
        return await RequestGpsLocationAsync();
    }

    public async Task<LocationSnapshot?> RequestGpsLocationAsync()
    {
        // Check if GPS is enabled
        if (!_locationManager.IsProviderEnabled(LocationManager.GpsProvider))
        {
            return null;
        }

        // Check permissions
        if (!_context.CheckSelfPermission(Manifest.Permission.AccessFineLocation) ==
            (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.M
                ? Permission.Granted
                : PackageInfo.PermissionStatus.Granted))
        {
            return null;
        }

        // Cancel any pending requests
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource = new CancellationTokenSource();

        try
        {
            // Configure location request
            var locationRequest = new LocationRequest.Builder()
                .SetMinUpdateIntervalMillis(500)
                .SetPriority(LocationRequest.PriorityHighAccuracy)
                .Build();

            var locationListener = new LocationListener(async location =>
            {
                try
                {
                    var snapshot = new LocationSnapshot(
                        LocationMode.GPS,
                        location.Latitude,
                        location.Longitude,
                        await GetLocationLabelAsync(location.Latitude, location.Longitude));

                    _settingsStore.SaveSnapshot(snapshot);
                    _cancellationTokenSource?.Cancel();
                }
                catch
                {
                    // Ignore errors during listener callback
                }
            });

            // Request single location update
            await Task.Run(() =>
            {
                try
                {
                    _locationManager.RequestSingleUpdate(
                        LocationRequest.PriorityHighAccuracy,
                        locationListener,
                        Looper.MainLooper);
                }
                catch
                {
                    // Ignore permission denial or other errors
                }
            }, _cancellationTokenSource.Token);

            // Wait for location with timeout
            var timeoutTask = Task.Delay(TimeSpan.FromSeconds(10), _cancellationTokenSource.Token);
            await timeoutTask;

            return _settingsStore.GetLastSnapshot();
        }
        catch (Exception)
        {
            return null;
        }
    }

    private async Task<string?> GetLocationLabelAsync(double latitude, double longitude)
    {
        try
        {
            var geocoder = new Geocoder(_context);
            var addresses = await geocoder.GetFromLocationAsync(latitude, longitude, 1);

            if (addresses?.Count > 0 && !string.IsNullOrEmpty(addresses[0].Locality))
            {
                return addresses[0].Locality;
            }

            return null;
        }
        catch
        {
            return null;
        }
    }
}

internal class LocationListener : Java.Lang.Object, ILocationListener
{
    private readonly Action<Android.Locations.Location> _onLocationChanged;

    public LocationListener(Action<Android.Locations.Location> onLocationChanged)
    {
        _onLocationChanged = onLocationChanged;
    }

    public void OnLocationChanged(Android.Locations.Location location) => _onLocationChanged(location);
    public void OnProviderDisabled(string provider) { }
    public void OnProviderEnabled(string provider) { }
    public void OnStatusChanged(string provider, [GeneratedEnum] Availability status, Android.Locations.Location? location) { }
}
