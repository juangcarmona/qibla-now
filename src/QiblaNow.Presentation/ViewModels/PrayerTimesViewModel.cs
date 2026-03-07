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

    [ObservableProperty]
    private DailyPrayerSchedule? _schedule;

    [ObservableProperty]
    private NextPrayerResult? _nextPrayer;

    [ObservableProperty]
    private CountdownTargetResult? _countdown;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string? _error;

    public PrayerTimesViewModel(
        IPrayerTimesCalculator calculator,
        ISettingsStore settingsStore)
    {
        _calculator = calculator;
        _settingsStore = settingsStore;
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        IsLoading = true;
        Error = null;

        try
        {
            var location = _settingsStore.GetLastValidLocation();
            if (location == null)
            {
                Error = "No valid location available. Please set your location in Settings.";
                IsLoading = false;
                return;
            }

            var settings = _settingsStore.GetCalculationSettings();
            var notificationSettings = _settingsStore.GetNotificationSettings();

            // Calculate schedule
            var schedule = await _calculator.CalculateDailyScheduleAsync(location, DateTimeOffset.UtcNow, settings);
            Schedule = schedule;

            // Calculate next prayer
            var nextPrayer = await _calculator.CalculateNextPrayerAsync(schedule, notificationSettings);
            NextPrayer = nextPrayer;

            // Calculate countdown
            var countdown = await _calculator.CalculateCountdownAsync(schedule, notificationSettings);
            Countdown = countdown;

            // Start countdown timer
            StartCountdownTimer();
        }
        catch (Exception ex)
        {
            Error = $"Error loading prayer times: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void TogglePrayerNotification(PrayerType type)
    {
        var settings = _settingsStore.GetNotificationSettings();
        settings.Reset();

        switch (type)
        {
            case PrayerType.Fajr:
                settings.FajrEnabled = !settings.FajrEnabled;
                break;
            case PrayerType.Dhuhr:
                settings.DhuhrEnabled = !settings.DhuhrEnabled;
                break;
            case PrayerType.Asr:
                settings.AsrEnabled = !settings.AsrEnabled;
                break;
            case PrayerType.Maghrib:
                settings.MaghribEnabled = !settings.MaghribEnabled;
                break;
            case PrayerType.Isha:
                settings.IshaEnabled = !settings.IshaEnabled;
                break;
        }

        _settingsStore.SaveNotificationSettings(settings);
    }

    public bool FajrEnabled => _settingsStore.GetNotificationSettings().FajrEnabled;
    public bool DhuhrEnabled => _settingsStore.GetNotificationSettings().DhuhrEnabled;
    public bool AsrEnabled => _settingsStore.GetNotificationSettings().AsrEnabled;
    public bool MaghribEnabled => _settingsStore.GetNotificationSettings().MaghribEnabled;
    public bool IshaEnabled => _settingsStore.GetNotificationSettings().IshaEnabled;
    public bool AnyNotificationEnabled => _settingsStore.GetNotificationSettings().IsAnyEnabled;

    private void StartCountdownTimer()
    {
        StopCountdownTimer();

        _countdownCts = new CancellationTokenSource();

        _ = Task.Run(async () =>
        {
            var settings = _settingsStore.GetCalculationSettings();
            var notificationSettings = _settingsStore.GetNotificationSettings();

            while (_countdownCts != null && !_countdownCts.Token.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(1000, _countdownCts.Token);
                    await Task.Run(() =>
                    {
                        if (Schedule == null) return;

                        var countdown = _calculator.CalculateCountdownAsync(Schedule, notificationSettings);
                        countdown.Wait(_countdownCts?.Token ?? CancellationToken.None);
                        Countdown = countdown.Result;
                    }, _countdownCts.Token);
                }
                catch (TaskCanceledException)
                {
                    break;
                }
                catch (Exception)
                {
                    // Ignore errors during countdown updates
                }
            }
        }, _countdownCts.Token);
    }

    private void StopCountdownTimer()
    {
        _countdownCts?.Cancel();
        _countdownCts?.Dispose();
        _countdownCts = null;
    }
}
