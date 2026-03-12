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

    public string GuidanceText
    {
        get
        {
            if (!HasLocation)
                return "Location unavailable";

            if (IsAligned)
                return "Facing Qibla";

            if (HeadingError > 0)
                return $"Turn right {Math.Abs(HeadingError):0.0}°";

            if (HeadingError < 0)
                return $"Turn left {Math.Abs(HeadingError):0.0}°";

            return "Facing Qibla";
        }
    }

    public string TurnArrowText
    {
        get
        {
            if (!HasLocation || IsAligned)
                return string.Empty;

            return HeadingError < 0 ? "←" : "→";
        }
    }

    public bool ShowTurnArrow => HasLocation && !IsAligned;

    public string StatusBadgeText => !HasLocation
        ? "NO LOCATION"
        : IsAligned
            ? "ALIGNED"
            : "ADJUST DIRECTION";

    public string InfoText => "Gold ray = local Qibla. Green curve = great-circle path. Dark green line = flat-map reference.";

    public string QiblaBearingCaption
    {
        get
        {
            if (!HasLocation)
                return string.Empty;

            var b = QiblaBearing;
            if (b < 90.0)
                return $"Qibla: {b:0}° from North to East";
            if (b < 180.0)
                return $"Qibla: {180.0 - b:0}° from South to East";
            if (b < 270.0)
                return $"Qibla: {b - 180.0:0}° from South to West";
            return $"Qibla: {360.0 - b:0}° from North to West";
        }
    }

    /// <summary>
    /// Rotates the compass board opposite to the device heading so it tracks North.
    /// </summary>
    public double CompassBoardRotation => -DeviceHeading;

    /// <summary>
    /// Rotates the Qibla arrow to the real local Qibla direction relative to the
    /// device's current orientation. When the device faces Qibla this equals 0.
    /// </summary>
    public double CompassArrowRotation => NormalizeDegrees(QiblaBearing - DeviceHeading);

    public double TargetLatitude => MeccaLatitude;
    public double TargetLongitude => MeccaLongitude;

    public MapViewModel(ILocationService locationService)
    {
        _locationService = locationService;
    }

    partial void OnErrorMessageChanged(string value) => OnPropertyChanged(nameof(HasError));
    partial void OnQiblaBearingChanged(double value)
    {
        OnPropertyChanged(nameof(QiblaBearingText));
        OnPropertyChanged(nameof(QiblaBearingCaption));
        OnPropertyChanged(nameof(CompassArrowRotation));
    }

    partial void OnDeviceHeadingChanged(double value)
    {
        OnPropertyChanged(nameof(DeviceHeadingText));
        OnPropertyChanged(nameof(CompassBoardRotation));
        OnPropertyChanged(nameof(CompassArrowRotation));
    }

    partial void OnHeadingErrorChanged(double value)
    {
        OnPropertyChanged(nameof(HeadingErrorText));
        OnPropertyChanged(nameof(GuidanceText));
    }

    partial void OnIsAlignedChanged(bool value)
    {
        OnPropertyChanged(nameof(AlignmentText));
        OnPropertyChanged(nameof(GuidanceText));
    }

    partial void OnHasLocationChanged(bool value)
    {
        OnPropertyChanged(nameof(QiblaBearingText));
        OnPropertyChanged(nameof(DeviceHeadingText));
        OnPropertyChanged(nameof(HeadingErrorText));
        OnPropertyChanged(nameof(AlignmentText));
        OnPropertyChanged(nameof(GuidanceText));
        OnPropertyChanged(nameof(QiblaBearingCaption));
        OnPropertyChanged(nameof(CompassBoardRotation));
        OnPropertyChanged(nameof(CompassArrowRotation));
    }

    public async Task LoadAsync()
    {
        IsLoading = true;
        ErrorMessage = string.Empty;

        try
        {
            var location = await _locationService.RequestGpsLocationAsync()
                ?? await _locationService.GetCurrentLocationAsync();

            if (location is null)
            {
                HasLocation = false;
                LocationLabel = "Location unavailable";
                ErrorMessage = "No location available. Set location first in Settings.";
                return;
            }

            ApplyLocation(location);
        }
        catch (Exception ex)
        {
            HasLocation = false;
            LocationLabel = "Location unavailable";
            ErrorMessage = $"Error loading map data: {ex.Message}";
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

    private void ApplyLocation(LocationSnapshot location)
    {
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

        HeadingError = NormalizeSignedDegrees(QiblaBearing - DeviceHeading);
        IsAligned = Math.Abs(HeadingError) <= AlignmentToleranceDegrees;
        HasLocation = true;
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