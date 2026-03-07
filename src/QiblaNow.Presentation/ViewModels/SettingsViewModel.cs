using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QiblaNow.Core.Abstractions;
using QiblaNow.Core.Models;

namespace QiblaNow.Presentation.ViewModels;

public sealed partial class SettingsViewModel : ObservableObject
{
    private readonly ISettingsStore _settingsStore;
    private readonly ILocationService _locationService;
    private readonly IPrayerSettingsStore _prayerSettingsStore;

    public SettingsViewModel(ISettingsStore settingsStore, ILocationService locationService, IPrayerSettingsStore prayerSettingsStore)
    {
        _settingsStore = settingsStore;
        _locationService = locationService;
        _prayerSettingsStore = prayerSettingsStore;

        LocationMode = _settingsStore.GetLocationMode();
        NotificationSettings = _prayerSettingsStore.GetNotificationSettings();
        CalculationSettings = _prayerSettingsStore.GetCalculationSettings();
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

    public bool IsNotSaving => !IsSaving;

    partial void OnIsSavingChanged(bool value)
    {
        OnPropertyChanged(nameof(IsNotSaving));
    }

    public IEnumerable<LocationMode> LocationModeOptions =>
        Enum.GetValues(typeof(LocationMode)).Cast<LocationMode>();

    public bool IsManualLocation => LocationMode == LocationMode.Manual;
    public bool IsGpsLocation => LocationMode == LocationMode.GPS;
    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

    // Prayer notification settings
    [ObservableProperty]
    private PrayerNotificationSettings _notificationSettings;

    [ObservableProperty]
    private PrayerCalculationSettings _calculationSettings;

    [ObservableProperty]
    private bool _isLoading;

    public bool IsNotLoading => !IsLoading;

    partial void OnIsLoadingChanged(bool value)
    {
        OnPropertyChanged(nameof(IsNotLoading));
    }

    // Prayer calculation settings
    public IEnumerable<CalculationMethod> CalculationMethodOptions =>
        Enum.GetValues(typeof(CalculationMethod)).Cast<CalculationMethod>();

    public IEnumerable<Madhab> MadhabOptions =>
        Enum.GetValues(typeof(Madhab)).Cast<Madhab>();

    public IEnumerable<HighLatitudeRule> HighLatitudeRuleOptions =>
        Enum.GetValues(typeof(HighLatitudeRule)).Cast<HighLatitudeRule>();

    // Prayer notification toggles
    public bool FajrEnabled => NotificationSettings.FajrEnabled;
    public bool DhuhrEnabled => NotificationSettings.DhuhrEnabled;
    public bool AsrEnabled => NotificationSettings.AsrEnabled;
    public bool MaghribEnabled => NotificationSettings.MaghribEnabled;
    public bool IshaEnabled => NotificationSettings.IshaEnabled;

    public bool AnyNotificationEnabled => NotificationSettings.IsAnyEnabled;

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

        // If your UI binds to IsManualLocation/IsGpsLocation, force property change notifications:
        OnPropertyChanged(nameof(IsManualLocation));
        OnPropertyChanged(nameof(IsGpsLocation));
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
            _prayerSettingsStore.SaveCalculationSettings(CalculationSettings);
            _prayerSettingsStore.SaveNotificationSettings(NotificationSettings);

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
                NotificationSettings.FajrEnabled = !NotificationSettings.FajrEnabled;
                break;
            case PrayerType.Dhuhr:
                NotificationSettings.DhuhrEnabled = !NotificationSettings.DhuhrEnabled;
                break;
            case PrayerType.Asr:
                NotificationSettings.AsrEnabled = !NotificationSettings.AsrEnabled;
                break;
            case PrayerType.Maghrib:
                NotificationSettings.MaghribEnabled = !NotificationSettings.MaghribEnabled;
                break;
            case PrayerType.Isha:
                NotificationSettings.IshaEnabled = !NotificationSettings.IshaEnabled;
                break;
        }
    }
}