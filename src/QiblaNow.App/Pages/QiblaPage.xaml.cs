using Microsoft.Maui.Devices.Sensors;
using QiblaNow.Presentation.ViewModels;

namespace QiblaNow.App.Pages;

public partial class QiblaPage : ContentPage
{
    private readonly MapViewModel _viewModel;

    public QiblaPage(MapViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;

#if ANDROID
        BottomBanner.AdsId = AdMobConfig.BannerId;
#endif
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadAsync();
        StartCompass();
    }

    protected override void OnDisappearing()
    {
        StopCompass();
        base.OnDisappearing();
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
            _viewModel.UpdateHeading(e.Reading.HeadingMagneticNorth));
    }
}