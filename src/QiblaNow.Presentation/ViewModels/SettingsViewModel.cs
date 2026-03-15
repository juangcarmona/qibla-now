using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using QiblaNow.Core.Abstractions;
using QiblaNow.Core.Models;

namespace QiblaNow.Presentation.ViewModels;

public sealed partial class SettingsViewModel : ObservableObject
{
    private readonly ISettingsStore   _settingsStore;
    private readonly ILocationService _locationService;
    private readonly INotificationScheduler _notificationScheduler;
    private readonly IAdhanPlayer _adhanPlayer;
    private readonly INotificationSettingsOpener _notificationSettingsOpener;

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
        set { NotificationSettings.FajrEnabled = value; OnPropertyChanged(); OnPropertyChanged(nameof(AnyNotificationEnabled)); OnPropertyChanged(nameof(NotificationSummary)); SaveAndReconcileNotifications(); }
    }

    public bool DhuhrEnabled
    {
        get => NotificationSettings.DhuhrEnabled;
        set { NotificationSettings.DhuhrEnabled = value; OnPropertyChanged(); OnPropertyChanged(nameof(AnyNotificationEnabled)); OnPropertyChanged(nameof(NotificationSummary)); SaveAndReconcileNotifications(); }
    }

    public bool AsrEnabled
    {
        get => NotificationSettings.AsrEnabled;
        set { NotificationSettings.AsrEnabled = value; OnPropertyChanged(); OnPropertyChanged(nameof(AnyNotificationEnabled)); OnPropertyChanged(nameof(NotificationSummary)); SaveAndReconcileNotifications(); }
    }

    public bool MaghribEnabled
    {
        get => NotificationSettings.MaghribEnabled;
        set { NotificationSettings.MaghribEnabled = value; OnPropertyChanged(); OnPropertyChanged(nameof(AnyNotificationEnabled)); OnPropertyChanged(nameof(NotificationSummary)); SaveAndReconcileNotifications(); }
    }

    public bool IshaEnabled
    {
        get => NotificationSettings.IshaEnabled;
        set { NotificationSettings.IshaEnabled = value; OnPropertyChanged(); OnPropertyChanged(nameof(AnyNotificationEnabled)); OnPropertyChanged(nameof(NotificationSummary)); SaveAndReconcileNotifications(); }
    }

    // ── Adhan sound selection ────────────────────────────────────────────

    public AdhanSound SelectedAdhan
    {
        get => NotificationSettings.SelectedAdhan;
        set
        {
            if (NotificationSettings.SelectedAdhan == value) return;
            NotificationSettings.SelectedAdhan = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsDefaultSoundSelected));
            OnPropertyChanged(nameof(IsAdhan1Selected));
            OnPropertyChanged(nameof(IsAdhan2Selected));
            OnPropertyChanged(nameof(IsAdhan3Selected));
            _settingsStore.SaveNotificationSettings(NotificationSettings);
        }
    }

    public bool IsDefaultSoundSelected => SelectedAdhan == AdhanSound.Default;
    public bool IsAdhan1Selected       => SelectedAdhan == AdhanSound.Adhan1;
    public bool IsAdhan2Selected       => SelectedAdhan == AdhanSound.Adhan2;
    public bool IsAdhan3Selected       => SelectedAdhan == AdhanSound.Adhan3;

    [RelayCommand]
    private void SelectDefaultSound() => SelectedAdhan = AdhanSound.Default;

    [RelayCommand]
    private void SelectAdhan1() => SelectedAdhan = AdhanSound.Adhan1;

    [RelayCommand]
    private void SelectAdhan2() => SelectedAdhan = AdhanSound.Adhan2;

    [RelayCommand]
    private void SelectAdhan3() => SelectedAdhan = AdhanSound.Adhan3;

    [RelayCommand]
    private void PreviewDefaultSound() => _adhanPlayer.Preview(AdhanSound.Default);

    [RelayCommand]
    private void PreviewAdhan1() => _adhanPlayer.Preview(AdhanSound.Adhan1);

    [RelayCommand]
    private void PreviewAdhan2() => _adhanPlayer.Preview(AdhanSound.Adhan2);

    [RelayCommand]
    private void PreviewAdhan3() => _adhanPlayer.Preview(AdhanSound.Adhan3);

    [RelayCommand]
    private Task OpenChannelSettingsAsync() => _notificationSettingsOpener.OpenChannelSettingsAsync();

    private void SaveAndReconcileNotifications()
    {
        _settingsStore.SaveNotificationSettings(NotificationSettings);
        _ = _notificationScheduler.ReconcileOnStartupAsync();
    }

    /// <summary>
    /// Stops any active Adhan preview. Called by the page's OnDisappearing.
    /// </summary>
    public void Cleanup() => _adhanPlayer.StopPreview();

    public bool AnyNotificationEnabled => NotificationSettings.IsAnyEnabled;

    // ── Summary properties (used by the Settings index page) ────────────────

    public string LocationSummary
    {
        get
        {
            if (LocationMode == LocationMode.GPS)
                return string.IsNullOrEmpty(LocationName) ? "GPS" : $"GPS — {LocationName}";

            if (!string.IsNullOrEmpty(Latitude) && !string.IsNullOrEmpty(Longitude))
                return $"Manual — {Latitude}, {Longitude}";

            return "Manual";
        }
    }

    public string CalculationSummary =>
        $"{CalculationSettings.Method} · {CalculationSettings.Madhab}";

    public string NotificationSummary
    {
        get
        {
            var count = new[]
            {
                NotificationSettings.FajrEnabled,
                NotificationSettings.DhuhrEnabled,
                NotificationSettings.AsrEnabled,
                NotificationSettings.MaghribEnabled,
                NotificationSettings.IshaEnabled
            }.Count(e => e);
            return count == 0 ? "No alerts" : $"{count} prayer alert{(count == 1 ? "" : "s")} on";
        }
    }

    // ── Flat calculation properties (auto-save, used by CalculationSettingsPage) ─

    public CalculationMethod SelectedMethod
    {
        get => CalculationSettings.Method;
        set
        {
            if (CalculationSettings.Method == value) return;
            CalculationSettings.Method = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CalculationSummary));
            _settingsStore.SaveCalculationSettings(CalculationSettings);
        }
    }

    public Madhab SelectedMadhab
    {
        get => CalculationSettings.Madhab;
        set
        {
            if (CalculationSettings.Madhab == value) return;
            CalculationSettings.Madhab = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CalculationSummary));
            _settingsStore.SaveCalculationSettings(CalculationSettings);
        }
    }

    public HighLatitudeRule SelectedHighLatitudeRule
    {
        get => CalculationSettings.HighLatitudeRule;
        set
        {
            if (CalculationSettings.HighLatitudeRule == value) return;
            CalculationSettings.HighLatitudeRule = value;
            OnPropertyChanged();
            _settingsStore.SaveCalculationSettings(CalculationSettings);
        }
    }

    // ── Navigation commands (Settings index → sub-pages) ────────────────────

    [RelayCommand]
    private Task GoLocationSettings() => Shell.Current.GoToAsync("location-settings");

    [RelayCommand]
    private Task GoLanguageSettings() => Shell.Current.GoToAsync("language-settings");

    [RelayCommand]
    private Task GoCalculationSettings() => Shell.Current.GoToAsync("calculation-settings");

    [RelayCommand]
    private Task GoSoundSettings() => Shell.Current.GoToAsync("sound-settings");

    [RelayCommand]
    private Task GoDisplaySettings() => Shell.Current.GoToAsync("display-settings");

    [RelayCommand]
    private Task GoAbout() => Shell.Current.GoToAsync("about");

    // ── Refresh from store (called by SettingsPage index on re-appearing) ────

    public void Refresh()
    {
        var cs   = _settingsStore.GetCalculationSettings();
        var ns   = _settingsStore.GetNotificationSettings();
        var mode = _settingsStore.GetLocationMode();
        var saved = _settingsStore.GetLastSnapshot();

        // Assign through generated setters so MVVM Toolkit tracks changes correctly.
        // Set the objects first before mode/location to avoid partial-method side-effects
        // overriding them.
        CalculationSettings  = cs;
        NotificationSettings = ns;

        // Setting LocationMode triggers OnLocationModeChanged which re-populates
        // Latitude/Longitude from the store — so the snapshot fields come after.
        LocationMode = mode;

        if (saved?.Label != null)
            LocationName = saved.Label;

        // Fire summary notifications for properties not covered by generated setters
        OnPropertyChanged(nameof(LocationSummary));
        OnPropertyChanged(nameof(CalculationSummary));
        OnPropertyChanged(nameof(NotificationSummary));
        OnPropertyChanged(nameof(SelectedMethod));
        OnPropertyChanged(nameof(SelectedMadhab));
        OnPropertyChanged(nameof(SelectedHighLatitudeRule));
        OnPropertyChanged(nameof(FajrEnabled));
        OnPropertyChanged(nameof(DhuhrEnabled));
        OnPropertyChanged(nameof(AsrEnabled));
        OnPropertyChanged(nameof(MaghribEnabled));
        OnPropertyChanged(nameof(IshaEnabled));
        OnPropertyChanged(nameof(AnyNotificationEnabled));
        OnPropertyChanged(nameof(SelectedAdhan));
        OnPropertyChanged(nameof(IsDefaultSoundSelected));
        OnPropertyChanged(nameof(IsAdhan1Selected));
        OnPropertyChanged(nameof(IsAdhan2Selected));
        OnPropertyChanged(nameof(IsAdhan3Selected));
    }

    public SettingsViewModel(ISettingsStore settingsStore, ILocationService locationService, INotificationScheduler notificationScheduler, IAdhanPlayer adhanPlayer, INotificationSettingsOpener notificationSettingsOpener)
    {
        _settingsStore               = settingsStore;
        _locationService             = locationService;
        _notificationScheduler       = notificationScheduler;
        _adhanPlayer                 = adhanPlayer;
        _notificationSettingsOpener  = notificationSettingsOpener;

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
        OnPropertyChanged(nameof(LocationSummary));

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
        OnPropertyChanged(nameof(LocationSummary));
    }

    partial void OnLatitudeChanged(string value) => OnPropertyChanged(nameof(LocationSummary));

    partial void OnLongitudeChanged(string value) => OnPropertyChanged(nameof(LocationSummary));

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
        OnPropertyChanged(nameof(NotificationSummary));
    }
}