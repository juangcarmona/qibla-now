using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.Maps;
using QiblaNow.Presentation.ViewModels;

namespace QiblaNow.App.Pages;

public partial class MapPage : ContentPage
{
    private const double EarthRadiusMeters = 6_371_000.0;
    private const double LocalQiblaRayLengthMeters = 35000.0;
    private const int GreatCircleSegments = 64;

    private readonly MapViewModel _viewModel;

    private Polyline? _localQiblaRay;
    private Polyline? _greatCircleLine;
    private Polyline? _straightLine;
    private Pin? _userPin;
    private Pin? _meccaPin;
    private bool _initialFlightCompleted;

    public MapPage(MapViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        _viewModel = viewModel;

        InitializeNativeMap();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        CenterOnMecca();

        await _viewModel.LoadAsync();
        RenderMap();
        await TryRunInitialFlightAsync();

        StartCompass();
    }

    protected override void OnDisappearing()
    {
        StopCompass();
        base.OnDisappearing();
    }

    private void CenterOnMecca()
    {
        var mecca = new Location(_viewModel.TargetLatitude, _viewModel.TargetLongitude);

        QiblaMap.MoveToRegion(
            MapSpan.FromCenterAndRadius(
                mecca,
                Distance.FromKilometers(5)));
    }

    private void RenderMap()
    {
        QiblaMap.Pins.Clear();
        QiblaMap.MapElements.Clear();

        _localQiblaRay = null;
        _greatCircleLine = null;
        _straightLine = null;
        _userPin = null;
        _meccaPin = null;

        if (!_viewModel.HasLocation)
            return;

        var userLocation = new Location(_viewModel.UserLatitude, _viewModel.UserLongitude);
        var meccaLocation = new Location(_viewModel.TargetLatitude, _viewModel.TargetLongitude);
        var qiblaRayEnd = CalculateDestinationPoint(
            userLocation,
            _viewModel.QiblaBearing,
            LocalQiblaRayLengthMeters);

        _userPin = new Pin
        {
            Label = "You",
            Location = userLocation
        };

        _meccaPin = new Pin
        {
            Label = "Mecca",
            Location = meccaLocation
        };

        _localQiblaRay = new Polyline
        {
            StrokeColor = _viewModel.IsAligned ? Colors.Gold : Colors.Goldenrod,
            StrokeWidth = _viewModel.IsAligned ? 8f : 7f
        };

        _localQiblaRay.Geopath.Add(userLocation);
        _localQiblaRay.Geopath.Add(qiblaRayEnd);

        _greatCircleLine = new Polyline
        {
            StrokeColor = Colors.Green,
            StrokeWidth = 4f
        };

        foreach (var point in BuildGreatCirclePath(userLocation, meccaLocation, GreatCircleSegments))
            _greatCircleLine.Geopath.Add(point);

        _straightLine = new Polyline
        {
            StrokeColor = Colors.DarkGreen,
            StrokeWidth = 3f
        };

        _straightLine.Geopath.Add(userLocation);
        _straightLine.Geopath.Add(meccaLocation);

        QiblaMap.Pins.Add(_userPin);
        QiblaMap.Pins.Add(_meccaPin);
        QiblaMap.MapElements.Add(_straightLine);
        QiblaMap.MapElements.Add(_greatCircleLine);
        QiblaMap.MapElements.Add(_localQiblaRay);

        UpdateNativeUserLocation(userLocation.Latitude, userLocation.Longitude);
    }

    private void UpdateLineStyle()
    {
        if (_localQiblaRay is null)
            return;

        _localQiblaRay.StrokeColor = _viewModel.IsAligned ? Colors.Gold : Colors.Goldenrod;
        _localQiblaRay.StrokeWidth = _viewModel.IsAligned ? 18f : 7f;
    }

    private void StartCompass()
    {
        if (!Compass.Default.IsSupported)
            return;

        Compass.Default.ReadingChanged -= OnCompassReadingChanged;
        Compass.Default.ReadingChanged += OnCompassReadingChanged;

        try
        {
            if (!Compass.Default.IsMonitoring)
                Compass.Default.Start(SensorSpeed.UI);
        }
        catch
        {
        }
    }

    private void StopCompass()
    {
        if (!Compass.Default.IsSupported)
            return;

        Compass.Default.ReadingChanged -= OnCompassReadingChanged;

        if (Compass.Default.IsMonitoring)
            Compass.Default.Stop();
    }

    private void OnCompassReadingChanged(object? sender, CompassChangedEventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            _viewModel.UpdateHeading(e.Reading.HeadingMagneticNorth);
            UpdateLineStyle();
            ApplyNativeMapBearing(_viewModel.DeviceHeading);
        });
    }

    private static IReadOnlyList<Location> BuildGreatCirclePath(
        Location from,
        Location to,
        int segments)
    {
        var points = new List<Location>(segments + 1);

        var lat1 = DegreesToRadians(from.Latitude);
        var lon1 = DegreesToRadians(from.Longitude);
        var lat2 = DegreesToRadians(to.Latitude);
        var lon2 = DegreesToRadians(to.Longitude);

        var angularDistance = 2d * Math.Asin(Math.Sqrt(
            Math.Pow(Math.Sin((lat2 - lat1) / 2d), 2d) +
            Math.Cos(lat1) * Math.Cos(lat2) * Math.Pow(Math.Sin((lon2 - lon1) / 2d), 2d)));

        if (angularDistance == 0d)
        {
            points.Add(from);
            return points;
        }

        double? previousLongitude = null;

        for (var i = 0; i <= segments; i++)
        {
            var fraction = (double)i / segments;
            var a = Math.Sin((1d - fraction) * angularDistance) / Math.Sin(angularDistance);
            var b = Math.Sin(fraction * angularDistance) / Math.Sin(angularDistance);

            var x = (a * Math.Cos(lat1) * Math.Cos(lon1)) + (b * Math.Cos(lat2) * Math.Cos(lon2));
            var y = (a * Math.Cos(lat1) * Math.Sin(lon1)) + (b * Math.Cos(lat2) * Math.Sin(lon2));
            var z = (a * Math.Sin(lat1)) + (b * Math.Sin(lat2));

            var latitude = Math.Atan2(z, Math.Sqrt((x * x) + (y * y)));
            var longitude = Math.Atan2(y, x);

            var longitudeDegrees = RadiansToDegrees(longitude);

            if (previousLongitude.HasValue)
                longitudeDegrees = UnwrapLongitude(previousLongitude.Value, longitudeDegrees);

            previousLongitude = longitudeDegrees;

            points.Add(new Location(
                RadiansToDegrees(latitude),
                longitudeDegrees));
        }

        return points;
    }

    private static Location CalculateDestinationPoint(
        Location start,
        double bearingDegrees,
        double distanceMeters)
    {
        var angularDistance = distanceMeters / EarthRadiusMeters;
        var bearing = DegreesToRadians(bearingDegrees);

        var lat1 = DegreesToRadians(start.Latitude);
        var lon1 = DegreesToRadians(start.Longitude);

        var lat2 = Math.Asin(
            (Math.Sin(lat1) * Math.Cos(angularDistance)) +
            (Math.Cos(lat1) * Math.Sin(angularDistance) * Math.Cos(bearing)));

        var lon2 = lon1 + Math.Atan2(
            Math.Sin(bearing) * Math.Sin(angularDistance) * Math.Cos(lat1),
            Math.Cos(angularDistance) - (Math.Sin(lat1) * Math.Sin(lat2)));

        return new Location(
            RadiansToDegrees(lat2),
            NormalizeLongitude(RadiansToDegrees(lon2)));
    }

    private static double DegreesToRadians(double degrees) => degrees * Math.PI / 180d;
    private static double RadiansToDegrees(double radians) => radians * 180d / Math.PI;

    private static double NormalizeLongitude(double longitude)
    {
        longitude %= 360d;

        if (longitude > 180d)
            longitude -= 360d;
        else if (longitude < -180d)
            longitude += 360d;

        return longitude;
    }

    private static double UnwrapLongitude(double previousLongitude, double longitude)
    {
        var candidate = longitude;

        while (candidate - previousLongitude > 180d)
            candidate -= 360d;

        while (previousLongitude - candidate > 180d)
            candidate += 360d;

        return candidate;
    }

    partial void InitializeNativeMap();
    partial void ApplyNativeMapBearing(double bearing);
    partial void UpdateNativeUserLocation(double latitude, double longitude);
    private partial Task TryRunInitialFlightAsync();

    private async void OnInfoClicked(object? sender, EventArgs e)
    {
        await DisplayAlert(
            "How Qibla is shown",
            "The true Qibla direction is the shortest path on the Earth's surface toward Mecca.\n\n" +
            "On a globe, that path is a great-circle route.\n" +
            "On a flat map, a straight line can look simpler but may not match the true bearing.\n\n" +
            "The short gold line near your location is your actual local Qibla direction.\n" +
            "The curved green line shows the global great-circle path.\n" +
            "The dark green line shows the flat-map reference only.",
            "OK");
    }
}