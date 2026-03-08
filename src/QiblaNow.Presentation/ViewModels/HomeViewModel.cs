using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QiblaNow.Core.Abstractions;

namespace QiblaNow.Presentation.ViewModels;

public sealed partial class HomeViewModel : ObservableObject
{
    private readonly ISettingsStore _settingsStore;
    private readonly ILocationService _locationService;

    [ObservableProperty]
    private string _nextPrayerName = string.Empty;

    [ObservableProperty]
    private string _nextPrayerTime = string.Empty;

    [ObservableProperty]
    private string _nextPrayerCountdown = string.Empty;

    [ObservableProperty]
    private string _locationLabel = "Detecting...";

    [ObservableProperty]
    private bool _isLoadingLocation;


    public HomeViewModel(ISettingsStore settingsStore, ILocationService locationService)
    {
        _settingsStore = settingsStore;
        _locationService = locationService;

        _ = LoadLocationAsync();
    }

    [RelayCommand]
    private async Task GoQibla() => await Shell.Current.GoToAsync("qibla");

    [RelayCommand]
    private async Task GoMap() => await Shell.Current.GoToAsync("map");

    [RelayCommand]
    private async Task GoTimes() => await Shell.Current.GoToAsync("times");

    [RelayCommand]
    private async Task GoSettings() => await Shell.Current.GoToAsync("settings");

    [RelayCommand]
    private async Task RequestLocationAsync()
    {
        IsLoadingLocation = true;
        try
        {
            var location = await _locationService.RequestGpsLocationAsync();
            LocationLabel = location?.Label ?? (location is null
                ? "Location unavailable"
                : $"{location.Latitude:F4}, {location.Longitude:F4}");
        }
        finally
        {
            IsLoadingLocation = false;
        }
    }

    private async Task LoadLocationAsync()
    {
        var snapshot = await _locationService.GetCurrentLocationAsync();
        LocationLabel = snapshot?.Label ?? (snapshot is null
            ? "Location unavailable"
            : $"{snapshot.Latitude:F4}, {snapshot.Longitude:F4}");
    }
}
