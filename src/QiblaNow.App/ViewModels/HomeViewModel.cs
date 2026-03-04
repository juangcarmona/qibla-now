using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QiblaNow.Core.Abstractions.Models;
using QiblaNow.Core.Abstractions.Services;

namespace QiblaNow.App.ViewModels;

public sealed partial class HomeViewModel : ObservableObject
{
    private readonly ISettingsStore _settingsStore;

    [ObservableProperty]
    private string _nextPrayerName = "Maghrib";

    [ObservableProperty]
    private string _nextPrayerTime = "19:42";

    [ObservableProperty]
    private string _nextPrayerCountdown = "In 12 min";

    [ObservableProperty]
    private string _locationLabel = "Detecting...";

    [ObservableProperty]
    private bool _isLoadingLocation;

    public HomeViewModel(ISettingsStore settingsStore)
    {
        _settingsStore = settingsStore;
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
            var locationService = App.Current?.Handlers.GetHandler<LocationService>();
            var location = await locationService?.RequestGpsLocationAsync() ??
                           throw new InvalidOperationException("Location service not available");

            if (location != null)
            {
                LocationLabel = location.Label ?? $"{location.Latitude:F4}, {location.Longitude:F4}";
            }
        }
        catch
        {
            LocationLabel = "Location unavailable";
        }
        finally
        {
            IsLoadingLocation = false;
        }
    }

    private async Task LoadLocationAsync()
    {
        try
        {
            var snapshot = _settingsStore.GetLastSnapshot();
            if (snapshot != null)
            {
                LocationLabel = snapshot.Label ?? $"{snapshot.Latitude:F4}, {snapshot.Longitude:F4}";
            }
        }
        catch
        {
            LocationLabel = "Location unavailable";
        }
    }
}
