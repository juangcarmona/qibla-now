using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QiblaNow.Core.Abstractions;
using QiblaNow.Core.Models;

namespace QiblaNow.Presentation.ViewModels;

/// <summary>
/// ViewModel for the "It's Prayer Time" alert page.
/// Receives the current prayer type as a Shell query parameter,
/// controls Adhan alarm playback, and exposes navigation commands.
/// </summary>
[QueryProperty(nameof(PrayerTypeValue), "prayerType")]
public sealed partial class PrayerAlertViewModel : ObservableObject
{
    private readonly IAdhanAlarmPlayer _alarmPlayer;
    private readonly ISettingsStore _settingsStore;

    [ObservableProperty]
    private string _prayerName = string.Empty;

    [ObservableProperty]
    private bool _isPlaying;

    private PrayerType _prayerType = PrayerType.Fajr;

    public PrayerAlertViewModel(IAdhanAlarmPlayer alarmPlayer, ISettingsStore settingsStore)
    {
        _alarmPlayer = alarmPlayer ?? throw new ArgumentNullException(nameof(alarmPlayer));
        _settingsStore = settingsStore ?? throw new ArgumentNullException(nameof(settingsStore));
    }

    /// <summary>
    /// Set by Shell navigation as a query parameter ("?prayerType=0").
    /// </summary>
    public int PrayerTypeValue
    {
        set
        {
            _prayerType = Enum.IsDefined(typeof(PrayerType), value)
                ? (PrayerType)value
                : PrayerType.Fajr;

            PrayerName = ResolvePrayerName(_prayerType);
            RefreshIsPlaying();
        }
    }

    /// <summary>
    /// Called from the page's OnAppearing to start playback if not already running
    /// (handles the case where the page is opened by tapping the notification after
    /// the alarm player was already started by the background alarm handler).
    /// </summary>
    public void EnsurePlayback()
    {
        if (!_alarmPlayer.IsPlaying)
        {
            var sound = _settingsStore.GetNotificationSettings().SelectedAdhan;
            _alarmPlayer.Play(sound);
        }
        RefreshIsPlaying();
    }

    [RelayCommand]
    private void StopAdhan()
    {
        _alarmPlayer.Stop();
        RefreshIsPlaying();
    }

    [RelayCommand]
    private static async Task GoQibla()
    {
        if (Shell.Current != null)
            await Shell.Current.GoToAsync("qibla");
    }

    [RelayCommand]
    private static async Task GoMap()
    {
        if (Shell.Current != null)
            await Shell.Current.GoToAsync("map");
    }

    [RelayCommand]
    private static async Task GoHome()
    {
        if (Shell.Current != null)
            await Shell.Current.GoToAsync("//home");
    }

    private void RefreshIsPlaying() => IsPlaying = _alarmPlayer.IsPlaying;

    private static string ResolvePrayerName(PrayerType prayerType) => prayerType switch
    {
        PrayerType.Fajr    => "Fajr",
        PrayerType.Sunrise => "Sunrise",
        PrayerType.Dhuhr   => "Dhuhr",
        PrayerType.Asr     => "Asr",
        PrayerType.Maghrib => "Maghrib",
        PrayerType.Isha    => "Isha",
        _                  => "Prayer",
    };
}
