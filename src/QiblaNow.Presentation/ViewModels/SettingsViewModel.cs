using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QiblaNow.Core.Abstractions;
using QiblaNow.Core.Models;

namespace QiblaNow.Presentation.ViewModels;

public sealed partial class SettingsViewModel : ObservableObject
{
    private readonly ISettingsStore _settingsStore;
    private readonly ILocationService _locationService;

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

    public bool IsNotSaving => !IsSaving;

    [ObservableProperty]
    private PrayerCalculationSettings _calculationSettings;

    [ObservableProperty]
    private bool _isLoading;

    public bool IsNotLoading => !IsLoading;

    // Prayer calculation settings
    public IEnumerable<CalculationMethod> CalculationMethodOptions =>
        Enum.GetValues(typeof(CalculationMethod)).Cast<CalculationMethod>();

    public IEnumerable<Madhab> MadhabOptions =>
        Enum.GetValues(typeof(Madhab)).Cast<Madhab>();

    public IEnumerable<HighLatitudeRule> HighLatitudeRuleOptions =>
        Enum.GetValues(typeof(HighLatitudeRule)).Cast<HighLatitudeRule>();

    // Prayer notification toggles
    public bool FajrEnabled => CalculationSettings.FajrEnabled;
    public bool DhuhrEnabled => CalculationSettings.DhuhrEnabled;
    public bool AsrEnabled => CalculationSettings.AsrEnabled;
    public bool MaghribEnabled => CalculationSettings.MaghribEnabled;
    public bool IshaEnabled => CalculationSettings.IshaEnabled;

    public bool AnyNotificationEnabled => CalculationSettings.IsAnyEnabled;

    partial void OnLocationModeChanged(LocationMode value)
    {
        ErrorMessage = string.Empty;

        if (value == LocationMode.Manual)
        {
            var snapshot = _settingsStore.GetLastSnapshot();
            if (snapshot != null)
            {
                Latitude = snapshot.Latitude.ToString(System.Globalization.CultureInfo.InvariantCulture);
                Longitude = snapshot.Longitude.ToString(System.Globalization.CultureInfo.InvariantCulture);
            }
            else
            {
                Latitude = string.Empty;
                Longitude = string.Empty;
            }
        }
    }

    [RelayCommand]
    private Task SaveManualLocationAsync()
    {
        ErrorMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(Latitude) || string.IsNullOrWhiteSpace(Longitude))
        {
            ErrorMessage = "Please enter both latitude and longitude";
            return Task.CompletedTask;
        }

        // Parse using invariant culture (avoids comma/period issues)
        if (!double.TryParse(Latitude, System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out var lat) ||
            !double.TryParse(Longitude, System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out var lon))
        {
            ErrorMessage = "Latitude and longitude must be valid numbers";
            return Task.CompletedTask;
        }

        if (lat is < -90 or > 90)
        {
            ErrorMessage = "Latitude must be between -90 and 90";
            return Task.CompletedTask;
        }

        if (lon is < -180 or > 180)
        {
            ErrorMessage = "Longitude must be between -180 and 180";
            return Task.CompletedTask;
        }

        IsSaving = true;
        try
        {
            var snapshot = new LocationSnapshot(LocationMode.Manual, lat, lon);

            _settingsStore.SetLocationMode(LocationMode.Manual);
            _settingsStore.SaveSnapshot(snapshot);

            Latitude = string.Empty;
            Longitude = string.Empty;
        }
        finally
        {
            IsSaving = false;
        }

        return Task.CompletedTask;
    }

    [RelayCommand]
    private async Task RequestGpsLocationAsync()
    {
        ErrorMessage = string.Empty;
        IsSaving = true;

        try
        {
            var location = await _locationService.RequestGpsLocationAsync();

            if (location is null)
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

    [RelayCommand]
    private Task ApplySettingsAsync()
    {
        IsLoading = true;

        try
        {
            _settingsStore.SaveCalculationSettings(CalculationSettings);
            _settingsStore.SaveNotificationSettings(CalculationSettings);

            return Task.CompletedTask;
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void TogglePrayerNotification(PrayerType type)
    {
        switch (type)
        {
            case PrayerType.Fajr:
                CalculationSettings.FajrEnabled = !CalculationSettings.FajrEnabled;
                break;
            case PrayerType.Dhuhr:
                CalculationSettings.DhuhrEnabled = !CalculationSettings.DhuhrEnabled;
                break;
            case PrayerType.Asr:
                CalculationSettings.AsrEnabled = !CalculationSettings.AsrEnabled;
                break;
            case PrayerType.Maghrib:
                CalculationSettings.MaghribEnabled = !CalculationSettings.MaghribEnabled;
                break;
            case PrayerType.Isha:
                CalculationSettings.IshaEnabled = !CalculationSettings.IshaEnabled;
                break;
        }
    }
}
