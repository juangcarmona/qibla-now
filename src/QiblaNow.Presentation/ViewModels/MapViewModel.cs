using CommunityToolkit.Mvvm.ComponentModel;
using QiblaNow.Core.Abstractions;
using QiblaNow.Core.Models;

namespace QiblaNow.Presentation.ViewModels;

public sealed partial class MapViewModel : ObservableObject
{
    private const double MeccaLatitude = 21.4225;
    private const double MeccaLongitude = 39.8262;
    private const double AlignmentToleranceDegrees = 5.0;

    private readonly ILocationService _locationService;
    private readonly ISettingsStore _settingsStore;

    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private string _errorMessage = string.Empty;
    [ObservableProperty] private string _locationLabel = "Detecting...";
    [ObservableProperty] private double _userLatitude;
    [ObservableProperty] private double _userLongitude;
    [ObservableProperty] private double _qiblaBearing;
    [ObservableProperty] private double _deviceHeading;
    [ObservableProperty] private double _headingError;
    [ObservableProperty] private bool _isAligned;
    [ObservableProperty] private bool _hasLocation;

    public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);

    public string QiblaBearingText => HasLocation ? $"{QiblaBearing:0.0}°" : "—";
    public string DeviceHeadingText => HasLocation ? $"{DeviceHeading:0.0}°" : "—";
    public string HeadingErrorText => HasLocation ? $"{HeadingError:+0.0;-0.0;0.0}°" : "—";
    public string AlignmentText => !HasLocation
        ? "No location"
        : IsAligned
            ? "Aligned"
            : "Adjust direction";

    public double TargetLatitude => MeccaLatitude;
    public double TargetLongitude => MeccaLongitude;

    public MapViewModel(
        ILocationService locationService,
        ISettingsStore settingsStore)
    {
        _locationService = locationService;
        _settingsStore = settingsStore;
    }

    partial void OnErrorMessageChanged(string value) => OnPropertyChanged(nameof(HasError));
    partial void OnQiblaBearingChanged(double value) => OnPropertyChanged(nameof(QiblaBearingText));
    partial void OnDeviceHeadingChanged(double value) => OnPropertyChanged(nameof(DeviceHeadingText));
    partial void OnHeadingErrorChanged(double value) => OnPropertyChanged(nameof(HeadingErrorText));
    partial void OnIsAlignedChanged(bool value) => OnPropertyChanged(nameof(AlignmentText));
    partial void OnHasLocationChanged(bool value)
    {
        OnPropertyChanged(nameof(QiblaBearingText));
        OnPropertyChanged(nameof(DeviceHeadingText));
        OnPropertyChanged(nameof(HeadingErrorText));
        OnPropertyChanged(nameof(AlignmentText));
    }

    public async Task LoadAsync()
    {
        IsLoading = true;
        ErrorMessage = string.Empty;

        try
        {
            var location = await _locationService.GetCurrentLocationAsync();

            if (location is null)
            {
                HasLocation = false;
                LocationLabel = "Location unavailable";
                ErrorMessage = "No stored location available. Set location first in Settings.";
                return;
            }

            UserLatitude = location.Latitude;
            UserLongitude = location.Longitude;
            LocationLabel = !string.IsNullOrWhiteSpace(location.Label)
                ? location.Label
                : $"{location.Latitude:F4}, {location.Longitude:F4}";

            QiblaBearing = CalculateInitialBearing(
                location.Latitude,
                location.Longitude,
                MeccaLatitude,
                MeccaLongitude);

            DeviceHeading = 0;
            HeadingError = NormalizeSignedDegrees(QiblaBearing - DeviceHeading);
            IsAligned = Math.Abs(HeadingError) <= AlignmentToleranceDegrees;
            HasLocation = true;
        }
        catch (Exception ex)
        {
            HasLocation = false;
            ErrorMessage = $"Error loading map data: {ex.Message}";
            LocationLabel = "Location unavailable";
        }
        finally
        {
            IsLoading = false;
        }
    }

    public void UpdateHeading(double headingMagneticNorth)
    {
        if (!HasLocation)
            return;

        DeviceHeading = NormalizeDegrees(headingMagneticNorth);
        HeadingError = NormalizeSignedDegrees(QiblaBearing - DeviceHeading);
        IsAligned = Math.Abs(HeadingError) <= AlignmentToleranceDegrees;
    }

    private static double CalculateInitialBearing(
        double fromLatitude,
        double fromLongitude,
        double toLatitude,
        double toLongitude)
    {
        var lat1 = DegreesToRadians(fromLatitude);
        var lon1 = DegreesToRadians(fromLongitude);
        var lat2 = DegreesToRadians(toLatitude);
        var lon2 = DegreesToRadians(toLongitude);

        var dLon = lon2 - lon1;

        var y = Math.Sin(dLon) * Math.Cos(lat2);
        var x = Math.Cos(lat1) * Math.Sin(lat2)
              - Math.Sin(lat1) * Math.Cos(lat2) * Math.Cos(dLon);

        var bearing = RadiansToDegrees(Math.Atan2(y, x));
        return NormalizeDegrees(bearing);
    }

    private static double DegreesToRadians(double degrees) => degrees * Math.PI / 180.0;
    private static double RadiansToDegrees(double radians) => radians * 180.0 / Math.PI;

    private static double NormalizeDegrees(double degrees)
    {
        degrees %= 360.0;
        if (degrees < 0)
            degrees += 360.0;

        return degrees;
    }

    private static double NormalizeSignedDegrees(double degrees)
    {
        var normalized = NormalizeDegrees(degrees);
        return normalized > 180.0 ? normalized - 360.0 : normalized;
    }
}