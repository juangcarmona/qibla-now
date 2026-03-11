#if !ANDROID
using Microsoft.Maui.Maps;

namespace QiblaNow.App.Pages;

public partial class MapPage
{
    partial void InitializeNativeMap()
    {
    }

    partial void ApplyNativeMapBearing(double bearing)
    {
    }

    partial void UpdateNativeUserLocation(double latitude, double longitude)
    {
        if (!_viewModel.HasLocation)
            return;

        var user = new Location(latitude, longitude);

        QiblaMap.MoveToRegion(
            MapSpan.FromCenterAndRadius(
                user,
                Distance.FromKilometers(0.5)));
    }

    private partial Task TryRunInitialFlightAsync()
    {
        if (!_viewModel.HasLocation)
            return Task.CompletedTask;

        var user = new Location(_viewModel.UserLatitude, _viewModel.UserLongitude);

        QiblaMap.MoveToRegion(
            MapSpan.FromCenterAndRadius(
                user,
                Distance.FromKilometers(0.5)));

        _initialFlightCompleted = true;
        return Task.CompletedTask;
    }
}
#endif