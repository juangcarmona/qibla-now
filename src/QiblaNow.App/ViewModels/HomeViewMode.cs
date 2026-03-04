using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace QiblaNow.App.ViewModels;

public sealed partial class HomeViewModel : ObservableObject
{
    [ObservableProperty]
    private string _nextPrayerName = "Maghrib";

    [ObservableProperty]
    private string _nextPrayerTime = "19:42";

    [ObservableProperty]
    private string _nextPrayerCountdown = "In 12 min";

    [RelayCommand] 
    private async Task GoQibla() => await Shell.Current.GoToAsync("qibla");

    [RelayCommand] 
    private async Task GoMap() => await Shell.Current.GoToAsync("map");

    [RelayCommand]
    private async Task GoTimes() => await Shell.Current.GoToAsync("times");

    [RelayCommand]
    private async Task GoSettings() => await Shell.Current.GoToAsync("settings");
}