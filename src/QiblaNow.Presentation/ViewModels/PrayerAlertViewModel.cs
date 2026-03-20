using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace QiblaNow.Presentation.ViewModels;

public sealed partial class PrayerAlertViewModel : ObservableObject
{
    [ObservableProperty] private string _prayerName = "Prayer";
    [ObservableProperty] private string _prayerTime = "--:--";
    [ObservableProperty] private string _currentTime = "--:--";

    public void UpdateAlert(string prayerName, string prayerTime, string currentTime)
    {
        PrayerName = string.IsNullOrWhiteSpace(prayerName) ? "Prayer" : prayerName;
        PrayerTime = string.IsNullOrWhiteSpace(prayerTime) ? "--:--" : prayerTime;
        CurrentTime = string.IsNullOrWhiteSpace(currentTime) ? "--:--" : currentTime;
    }

    [RelayCommand]
    private async Task GoQibla() => await Shell.Current.GoToAsync("qibla");

    [RelayCommand]
    private async Task GoMap() => await Shell.Current.GoToAsync("map");

    [RelayCommand]
    private async Task GoHome() => await Shell.Current.GoToAsync("//home");
}
