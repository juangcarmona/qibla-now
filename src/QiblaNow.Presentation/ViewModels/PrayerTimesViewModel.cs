using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QiblaNow.Core.Abstractions;
using QiblaNow.Core.Models;

namespace QiblaNow.Presentation.ViewModels;

public sealed partial class PrayerTimesViewModel : ObservableObject
{
    private readonly IPrayerTimesCalculator _calculator;
    private readonly ISettingsStore _settingsStore;
    private CancellationTokenSource? _countdownCts;

    [ObservableProperty] private DailyPrayerSchedule? _schedule;
    [ObservableProperty] private NextPrayerResult?     _nextPrayer;
    [ObservableProperty] private CountdownTargetResult? _countdown;
    [ObservableProperty] private bool   _isLoading;
    [ObservableProperty] private string? _error;

    public PrayerTimesViewModel(IPrayerTimesCalculator calculator, ISettingsStore settingsStore)
    {
        _calculator    = calculator;
        _settingsStore = settingsStore;
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        IsLoading = true;
        Error     = null;
        try
        {
            var location = _settingsStore.GetLastValidLocation();
            if (location == null) { Error = "No valid location available. Please set your location in Settings."; return; }

            var calcSettings  = _settingsStore.GetCalculationSettings();
            var notifSettings = _settingsStore.GetNotificationSettings();
            var now           = DateTimeOffset.UtcNow;

            Schedule   = await _calculator.CalculateDailyScheduleAsync(location, now, calcSettings);
            NextPrayer = await _calculator.CalculateNextPrayerAsync(Schedule, notifSettings, now);
            Countdown  = await _calculator.CalculateCountdownAsync(Schedule, notifSettings, now);

            StartCountdownTimer();
        }
        catch (Exception ex) { Error = $"Error loading prayer times: {ex.Message}"; }
        finally { IsLoading = false; }
    }

    /// <summary>
    /// Toggles the notification flag for a specific prayer and persists it.
    /// Does NOT call Reset() — toggles only the targeted prayer.
    /// </summary>
    [RelayCommand]
    private void TogglePrayerNotification(PrayerType type)
    {
        var s = _settingsStore.GetNotificationSettings();
        switch (type)
        {
            case PrayerType.Fajr:    s.FajrEnabled    = !s.FajrEnabled;    break;
            case PrayerType.Dhuhr:   s.DhuhrEnabled   = !s.DhuhrEnabled;   break;
            case PrayerType.Asr:     s.AsrEnabled     = !s.AsrEnabled;     break;
            case PrayerType.Maghrib: s.MaghribEnabled = !s.MaghribEnabled; break;
            case PrayerType.Isha:    s.IshaEnabled    = !s.IshaEnabled;    break;
        }
        _settingsStore.SaveNotificationSettings(s);
        OnPropertyChanged(nameof(FajrEnabled));
        OnPropertyChanged(nameof(DhuhrEnabled));
        OnPropertyChanged(nameof(AsrEnabled));
        OnPropertyChanged(nameof(MaghribEnabled));
        OnPropertyChanged(nameof(IshaEnabled));
        OnPropertyChanged(nameof(AnyNotificationEnabled));
    }

    public bool FajrEnabled            => _settingsStore.GetNotificationSettings().FajrEnabled;
    public bool DhuhrEnabled           => _settingsStore.GetNotificationSettings().DhuhrEnabled;
    public bool AsrEnabled             => _settingsStore.GetNotificationSettings().AsrEnabled;
    public bool MaghribEnabled         => _settingsStore.GetNotificationSettings().MaghribEnabled;
    public bool IshaEnabled            => _settingsStore.GetNotificationSettings().IshaEnabled;
    public bool AnyNotificationEnabled => _settingsStore.GetNotificationSettings().IsAnyEnabled;

    private void StartCountdownTimer()
    {
        StopCountdownTimer();
        _countdownCts = new CancellationTokenSource();
        var token = _countdownCts.Token;

        _ = Task.Run(async () =>
        {
            while (!token.IsCancellationRequested)
            {
                try   { await Task.Delay(1000, token); }
                catch (TaskCanceledException) { break; }

                if (Schedule == null) continue;

                var notifSettings = _settingsStore.GetNotificationSettings();
                var updated = await _calculator.CalculateCountdownAsync(
                    Schedule, notifSettings, DateTimeOffset.UtcNow);

                Microsoft.Maui.ApplicationModel.MainThread.BeginInvokeOnMainThread(
                    () => Countdown = updated);
            }
        }, token);
    }

    private void StopCountdownTimer()
    {
        _countdownCts?.Cancel();
        _countdownCts?.Dispose();
        _countdownCts = null;
    }
}