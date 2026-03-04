using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QiblaNow.Core.Abstractions.Models;
using QiblaNow.Core.Abstractions.Services;

namespace QiblaNow.App.ViewModels;

public sealed partial class SettingsViewModel : ObservableObject
{
    private readonly ISettingsStore _settingsStore;

    public SettingsViewModel(ISettingsStore settingsStore)
    {
        _settingsStore = settingsStore;
        LocationMode = _settingsStore.GetLocationMode();
    }

    [ObservableProperty]
    private LocationMode _locationMode;

    [ObservableProperty]
    private string _latitude = string.Empty;

    [ObservableProperty]
    private string _longitude = string.Empty;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _isSaving;

    /// <summary>
    /// Available location mode options for the picker
    /// </summary>
    public IEnumerable<LocationMode> LocationModeOptions => Enum.GetValues(typeof(LocationMode)).Cast<LocationMode>();

    /// <summary>
    /// Indicates whether manual location fields should be visible
    /// </summary>
    public bool IsManualLocation => LocationMode == LocationMode.Manual;

    /// <summary>
    /// Indicates whether GPS section should be visible
    /// </summary>
    public bool IsGpsLocation => LocationMode == LocationMode.GPS;

    /// <summary>
    /// Indicates whether an error message is displayed
    /// </summary>
    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

    partial void OnLocationModeChanged(LocationMode value)
    {
        ErrorMessage = string.Empty;

        if (value == LocationMode.Manual)
        {
            var snapshot = _settingsStore.GetLastSnapshot();
            if (snapshot != null)
            {
                Latitude = snapshot.Latitude.ToString();
                Longitude = snapshot.Longitude.ToString();
            }
            else
            {
                Latitude = string.Empty;
                Longitude = string.Empty;
            }
        }
    }

    [RelayCommand]
    private async Task SaveManualLocationAsync()
    {
        if (string.IsNullOrWhiteSpace(Latitude) || string.IsNullOrWhiteSpace(Longitude))
        {
            ErrorMessage = "Please enter both latitude and longitude";
            return;
        }

        if (!double.TryParse(Latitude, out double lat) ||
            !double.TryParse(Longitude, out double lon))
        {
            ErrorMessage = "Latitude and longitude must be valid numbers";
            return;
        }

        // Validate ranges
        if (lat < -90 || lat > 90)
        {
            ErrorMessage = "Latitude must be between -90 and 90";
            return;
        }

        if (lon < -180 || lon > 180)
        {
            ErrorMessage = "Longitude must be between -180 and 180";
            return;
        }

        IsSaving = true;
        try
        {
            var snapshot = new LocationSnapshot(LocationMode.Manual, lat, lon);
            _settingsStore.SetLocationMode(LocationMode.Manual);
            _settingsStore.SaveSnapshot(snapshot);

            // Reset latitude/longitude fields
            Latitude = string.Empty;
            Longitude = string.Empty;
        }
        finally
        {
            IsSaving = false;
        }
    }

    [RelayCommand]
    private async Task RequestGpsLocationAsync()
    {
        ErrorMessage = string.Empty;
        IsSaving = true;

        try
        {
            var locationService = App.Current?.Handlers.GetHandler<LocationService>();
            var location = await locationService?.RequestGpsLocationAsync() ??
                           throw new InvalidOperationException("Location service not available");

            if (location == null)
            {
                ErrorMessage = "Unable to get location. Please check GPS and permissions.";
                return;
            }

            _settingsStore.SetLocationMode(LocationMode.GPS);
            _settingsStore.SaveSnapshot(location);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error requesting location: {ex.Message}";
        }
        finally
        {
            IsSaving = false;
        }
    }

    partial void OnErrorMessageChanged(string value)
    {
        // UI will bind to this to show error messages
    }
}
