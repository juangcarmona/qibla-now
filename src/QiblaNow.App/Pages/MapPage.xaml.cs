using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;
using QiblaNow.Presentation.ViewModels;

namespace QiblaNow.App.Pages;

public partial class MapPage : ContentPage
{
    private readonly MapViewModel _viewModel;
    private Polyline? _qiblaLine;
    private Pin? _userPin;
    private Pin? _meccaPin;

    public MapPage(MapViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await _viewModel.LoadAsync();
        RenderMap();

        StartCompass();
    }

    protected override void OnDisappearing()
    {
        StopCompass();
        base.OnDisappearing();
    }

    private void RenderMap()
    {
        if (!_viewModel.HasLocation)
        {
            QiblaMap.Pins.Clear();
            QiblaMap.MapElements.Clear();
            return;
        }

        var userLocation = new Location(_viewModel.UserLatitude, _viewModel.UserLongitude);
        var meccaLocation = new Location(_viewModel.TargetLatitude, _viewModel.TargetLongitude);

        QiblaMap.Pins.Clear();
        QiblaMap.MapElements.Clear();

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

        _qiblaLine = new Polyline
        {
            StrokeColor = Colors.Green,
            StrokeWidth = _viewModel.IsAligned ? 8 : 5
        };

        _qiblaLine.Geopath.Add(userLocation);
        _qiblaLine.Geopath.Add(meccaLocation);

        QiblaMap.Pins.Add(_userPin);
        QiblaMap.Pins.Add(_meccaPin);
        QiblaMap.MapElements.Add(_qiblaLine);

        QiblaMap.MoveToRegion(
            MapSpan.FromCenterAndRadius(
                userLocation,
                Distance.FromKilometers(1)));
    }

    private void UpdateLineStyle()
    {
        if (_qiblaLine is null)
            return;

        _qiblaLine.StrokeWidth = _viewModel.IsAligned ? 8 : 5;
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
            // Keep the page usable even if compass is unavailable.
        }
    }

    private void StopCompass()
    {
        if (Compass.Default.IsSupported)
        {
            Compass.Default.ReadingChanged -= OnCompassReadingChanged;

            if (Compass.Default.IsMonitoring)
                Compass.Default.Stop();
        }
    }

    private void OnCompassReadingChanged(object? sender, CompassChangedEventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            _viewModel.UpdateHeading(e.Reading.HeadingMagneticNorth);
            UpdateLineStyle();
        });
    }
}