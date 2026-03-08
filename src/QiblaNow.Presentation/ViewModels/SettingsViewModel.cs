using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QiblaNow.Core.Abstractions;
using QiblaNow.Core.Models;

namespace QiblaNow.Presentation.ViewModels;

public sealed partial class SettingsViewModel : ObservableObject
{
    private readonly ISettingsStore   _settingsStore;
    private readonly ILocationService _locationService;

    [ObservableProperty] private LocationMode _locationMode;
    [ObservableProperty] private string _latitude     = string.Empty;
    [ObservableProperty] private string _longitude    = string.Empty;
    [ObservableProperty] private string _errorMessage = string.Empty;
    [ObservableProperty] private string _locationName = string.Empty;
    [ObservableProperty] private bool   _isSaving;
    [ObservableProperty] private bool   _isLoading;
    [ObservableProperty] private PrayerCalculationSettings  _calculationSettings;
    [ObservableProperty] private PrayerNotificationSettings _notificationSettings;

    public bool IsNotSaving     => !IsSaving;
    public bool IsNotLoading    => !IsLoading;
    public bool IsManualLocation => LocationMode == LocationMode.Manual;
    public bool IsGpsLocation    => LocationMode == LocationMode.GPS;
    public bool HasError         => !string.IsNullOrEmpty(ErrorMessage);
    public bool HasLocationName  => !string.IsNullOrEmpty(LocationName);

    public IEnumerable<LocationMode> LocationModeOptions =>
        Enum.GetValues(typeof(LocationMode)).Cast<LocationMode>();

    public IEnumerable<CalculationMethod> CalculationMethodOptions =>
        Enum.GetValues(typeof(CalculationMethod)).Cast<CalculationMethod>();

    public IEnumerable<Madhab> MadhabOptions =>
        Enum.GetValues(typeof(Madhab)).Cast<Madhab>();

    public IEnumerable<HighLatitudeRule> HighLatitudeRuleOptions =>
        Enum.GetValues(typeof(HighLatitudeRule)).Cast<HighLatitudeRule>();

    // Notification toggle properties — two-way bindable, auto-persist on change
    public bool FajrEnabled
    {
        get => NotificationSettings.FajrEnabled;
        set { NotificationSettings.FajrEnabled = value; OnPropertyChanged(); OnPropertyChanged(nameof(AnyNotificationEnabled)); _settingsStore.SaveNotificationSettings(NotificationSettings); }
    }

    public bool DhuhrEnabled
    {
        get => NotificationSettings.DhuhrEnabled;
        set { NotificationSettings.DhuhrEnabled = value; OnPropertyChanged(); OnPropertyChanged(nameof(AnyNotificationEnabled)); _settingsStore.SaveNotificationSettings(NotificationSettings); }
    }

    public bool AsrEnabled
    {
        get => NotificationSettings.AsrEnabled;
        set { NotificationSettings.AsrEnabled = value; OnPropertyChanged(); OnPropertyChanged(nameof(AnyNotificationEnabled)); _settingsStore.SaveNotificationSettings(NotificationSettings); }
    }

    public bool MaghribEnabled
    {
        get => NotificationSettings.MaghribEnabled;
        set { NotificationSettings.MaghribEnabled = value; OnPropertyChanged(); OnPropertyChanged(nameof(AnyNotificationEnabled)); _settingsStore.SaveNotificationSettings(NotificationSettings); }
    }

    public bool IshaEnabled
    {
        get => NotificationSettings.IshaEnabled;
        set { NotificationSettings.IshaEnabled = value; OnPropertyChanged(); OnPropertyChanged(nameof(AnyNotificationEnabled)); _settingsStore.SaveNotificationSettings(NotificationSettings); }
    }

    public bool AnyNotificationEnabled => NotificationSettings.IsAnyEnabled;

    public SettingsViewModel(ISettingsStore settingsStore, ILocationService locationService)
    {
        _settingsStore   = settingsStore;
        _locationService = locationService;

        // Initialise from persisted state
        _calculationSettings  = _settingsStore.GetCalculationSettings();
        _notificationSettings = _settingsStore.GetNotificationSettings();
        _locationMode         = _settingsStore.GetLocationMode();

        // Restore coordinates if manual mode was persisted
        var saved = _settingsStore.GetLastSnapshot();
        if (_locationMode == LocationMode.Manual && saved != null)
        {
            _latitude  = saved.Latitude.ToString(System.Globalization.CultureInfo.InvariantCulture);
            _longitude = saved.Longitude.ToString(System.Globalization.CultureInfo.InvariantCulture);
        }

        // Restore location name if a GPS snapshot with a label was persisted
        if (saved?.Label != null)
            _locationName = saved.Label;
    }

    partial void OnLocationModeChanged(LocationMode value)
    {
        ErrorMessage = string.Empty;
        OnPropertyChanged(nameof(IsManualLocation));
        OnPropertyChanged(nameof(IsGpsLocation));

        if (value == LocationMode.Manual)
        {
            var snapshot = _settingsStore.GetLastSnapshot();
            if (snapshot != null)
            {
                Latitude  = snapshot.Latitude.ToString(System.Globalization.CultureInfo.InvariantCulture);
                Longitude = snapshot.Longitude.ToString(System.Globalization.CultureInfo.InvariantCulture);
            }
            else
            {
                Latitude  = string.Empty;
                Longitude = string.Empty;
            }
        }
    }

    partial void OnIsSavingChanged(bool value)
    {
        OnPropertyChanged(nameof(IsNotSaving));
    }

    partial void OnErrorMessageChanged(string value)
    {
        OnPropertyChanged(nameof(HasError));
    }

    partial void OnLocationNameChanged(string value)
    {
        OnPropertyChanged(nameof(HasLocationName));
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

        if (!double.TryParse(Latitude, System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out var lat) ||
            !double.TryParse(Longitude, System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out var lon))
        {
            ErrorMessage = "Latitude and longitude must be valid numbers";
            return Task.CompletedTask;
        }

        if (lat is < -90 or > 90)
        { ErrorMessage = "Latitude must be between -90 and 90"; return Task.CompletedTask; }

        if (lon is < -180 or > 180)
        { ErrorMessage = "Longitude must be between -180 and 180"; return Task.CompletedTask; }

        IsSaving = true;
        try
        {
            _settingsStore.SetLocationMode(LocationMode.Manual);
            _settingsStore.SaveSnapshot(new LocationSnapshot(LocationMode.Manual, lat, lon));
            Latitude  = string.Empty;
            Longitude = string.Empty;
        }
        finally { IsSaving = false; }

        return Task.CompletedTask;
    }

    [RelayCommand]
    private async Task RequestGpsLocationAsync()
    {
        ErrorMessage = string.Empty;
        IsSaving     = true;
        try
        {
            var location = await _locationService.RequestGpsLocationAsync();
            if (location is null)
            {
                ErrorMessage = "Unable to get location. Please check GPS settings and permissions.";
                return;
            }

            _settingsStore.SetLocationMode(LocationMode.GPS);
            _settingsStore.SaveSnapshot(location);

            // Reflect resolved place name (or coordinates fallback) in the UI
            LocationName = !string.IsNullOrEmpty(location.Label)
                ? location.Label
                : $"{location.Latitude:F4}, {location.Longitude:F4}";
        }
        catch (Exception ex) { ErrorMessage = $"Error requesting location: {ex.Message}"; }
        finally { IsSaving = false; }
    }

    [RelayCommand]
    private Task ApplySettingsAsync()
    {
        IsLoading = true;
        try
        {
            // Save calculation settings and notification settings independently
            _settingsStore.SaveCalculationSettings(CalculationSettings);
            _settingsStore.SaveNotificationSettings(NotificationSettings);
            return Task.CompletedTask;
        }
        finally { IsLoading = false; }
    }

    [RelayCommand]
    private void TogglePrayerNotification(PrayerType type)
    {
        switch (type)
        {
            case PrayerType.Fajr:    NotificationSettings.FajrEnabled    = !NotificationSettings.FajrEnabled;    break;
            case PrayerType.Dhuhr:   NotificationSettings.DhuhrEnabled   = !NotificationSettings.DhuhrEnabled;   break;
            case PrayerType.Asr:     NotificationSettings.AsrEnabled     = !NotificationSettings.AsrEnabled;     break;
            case PrayerType.Maghrib: NotificationSettings.MaghribEnabled = !NotificationSettings.MaghribEnabled; break;
            case PrayerType.Isha:    NotificationSettings.IshaEnabled    = !NotificationSettings.IshaEnabled;    break;
        }
        OnPropertyChanged(nameof(FajrEnabled));
        OnPropertyChanged(nameof(DhuhrEnabled));
        OnPropertyChanged(nameof(AsrEnabled));
        OnPropertyChanged(nameof(MaghribEnabled));
        OnPropertyChanged(nameof(IshaEnabled));
        OnPropertyChanged(nameof(AnyNotificationEnabled));
    }
}