using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QiblaNow.Core.Abstractions;
using QiblaNow.Core.Models;

namespace QiblaNow.Presentation.ViewModels;

public sealed partial class PrayerTimesViewModel : ObservableObject
{
    private readonly IPrayerTimesCalculator _calculator;
    private readonly IPrayerSettingsStore _prayerSettingsStore;

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

    [ObservableProperty]
    private DateTimeOffset _currentTime;

    private CancellationTokenSource? _countdownCts;

    public PrayerTimesViewModel(
        IPrayerTimesCalculator calculator,
        IPrayerSettingsStore prayerSettingsStore)
    {
        _calculator = calculator;
        _prayerSettingsStore = prayerSettingsStore;

        _currentTime = DateTimeOffset.UtcNow;
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        IsLoading = true;
        Error = null;

        try
        {
            var location = _prayerSettingsStore.GetLastValidLocation();
            if (location == null)
            {
                Error = "No valid location available. Please set your location in Settings.";
                IsLoading = false;
                return;
            }

            var settings = _prayerSettingsStore.GetCalculationSettings();
            var notificationSettings = _prayerSettingsStore.GetNotificationSettings();

            // Calculate schedule
            var schedule = await _calculator.CalculateDailyScheduleAsync(location, DateTimeOffset.UtcNow, settings);
            _schedule = schedule;

            // Calculate next prayer
            var nextPrayer = await _calculator.CalculateNextPrayerAsync(schedule, notificationSettings);
            _nextPrayer = nextPrayer;

            // Calculate countdown
            var countdown = await _calculator.CalculateCountdownAsync(schedule, notificationSettings);
            _countdown = countdown;

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
        var settings = _prayerSettingsStore.GetNotificationSettings();
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

        _prayerSettingsStore.SaveNotificationSettings(settings);
        OnPropertyChanged(nameof(FajrEnabled));
        OnPropertyChanged(nameof(DhuhrEnabled));
        OnPropertyChanged(nameof(AsrEnabled));
        OnPropertyChanged(nameof(MaghribEnabled));
        OnPropertyChanged(nameof(IshaEnabled));
        OnPropertyChanged(nameof(AnyNotificationEnabled));
    }

    public bool FajrEnabled => _prayerSettingsStore.GetNotificationSettings().FajrEnabled;
    public bool DhuhrEnabled => _prayerSettingsStore.GetNotificationSettings().DhuhrEnabled;
    public bool AsrEnabled => _prayerSettingsStore.GetNotificationSettings().AsrEnabled;
    public bool MaghribEnabled => _prayerSettingsStore.GetNotificationSettings().MaghribEnabled;
    public bool IshaEnabled => _prayerSettingsStore.GetNotificationSettings().IshaEnabled;
    public bool AnyNotificationEnabled => _prayerSettingsStore.GetNotificationSettings().IsAnyEnabled;

    private void StartCountdownTimer()
    {
        StopCountdownTimer();

        _countdownCts = new CancellationTokenSource();

        _ = Task.Run(async () =>
        {
            while (!_countdownCts.Token.IsCancellationRequested)
            {
                await Task.Delay(1000, _countdownCts.Token);
                _currentTime = DateTimeOffset.UtcNow;

                if (Countdown != null)
                {
                    var countdown = await _calculator.CalculateCountdownAsync(
                        Schedule ?? new DailyPrayerSchedule(DateTimeOffset.UtcNow, TimeZoneInfo.Local),
                        _prayerSettingsStore.GetNotificationSettings());
                    Countdown = countdown;
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
