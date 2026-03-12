using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QiblaNow.Core.Abstractions;
using QiblaNow.Core.Models;

namespace QiblaNow.Presentation.ViewModels;

public sealed partial class HomeViewModel : ObservableObject
{
    private readonly ISettingsStore _settingsStore;
    private readonly ILocationService _locationService;
    private readonly IPrayerTimesCalculator _calculator;

    // ── Next-prayer block ────────────────────────────────────────────────────
    [ObservableProperty] private string _nextPrayerPrefix = "Next:";
    [ObservableProperty] private string _nextPrayerName = "—";
    [ObservableProperty] private string _nextPrayerTime = "—";
    [ObservableProperty] private string _nextPrayerCountdown = "00:00:00";
    [ObservableProperty] private bool _showLiveCountdown;

    // ── Location ─────────────────────────────────────────────────────────────
    [ObservableProperty] private string _locationLabel = "Detecting...";

    // ── Schedule meta ────────────────────────────────────────────────────────
    [ObservableProperty] private string _methodLabel = string.Empty;
    [ObservableProperty] private string _asrLabel = string.Empty;

    // ── Day navigation ───────────────────────────────────────────────────────
    [ObservableProperty] private string _selectedDateLabel = string.Empty;
    [ObservableProperty] private bool _isToday;

    public bool IsNotToday => !IsToday;

    // ── Loading ──────────────────────────────────────────────────────────────
    [ObservableProperty] private bool _isLoading;

    // ── Next-prayer alarm indicator ──────────────────────────────────────────
    [ObservableProperty] private bool _nextPrayerAlarmEnabled;

    // ── Prayer rows ──────────────────────────────────────────────────────────
    public ObservableCollection<PrayerRowItem> PrayerRows { get; } = new();

    // ── Internal state ───────────────────────────────────────────────────────
    private DateOnly _selectedDate;
    private CancellationTokenSource? _countdownCts;

    // Sentinel used to find the next prayer regardless of notification settings.
    private static readonly PrayerNotificationSettings _allEnabled = new()
    {
        FajrEnabled    = true,
        DhuhrEnabled   = true,
        AsrEnabled     = true,
        MaghribEnabled = true,
        IshaEnabled    = true,
    };

    public HomeViewModel(
        ISettingsStore settingsStore,
        ILocationService locationService,
        IPrayerTimesCalculator calculator)
    {
        _settingsStore = settingsStore;
        _locationService = locationService;
        _calculator = calculator;

        _selectedDate = DateOnly.FromDateTime(DateTime.Today);
        SelectedDateLabel = FormatSelectedDate(_selectedDate);
    }

    partial void OnIsTodayChanged(bool value) => OnPropertyChanged(nameof(IsNotToday));

    // ── Public entry point called by the page on Appearing ──────────────────
    public async Task LoadAsync()
    {
        StopCountdownTimer();
        IsLoading = true;

        try
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            IsToday = _selectedDate == today;
            SelectedDateLabel = FormatSelectedDate(_selectedDate);
            ShowLiveCountdown = IsToday;

            var location = await _locationService.GetCurrentLocationAsync();
            LocationLabel = location is null
                ? "Location unavailable"
                : !string.IsNullOrWhiteSpace(location.Label)
                    ? location.Label
                    : $"{location.Latitude:F4}, {location.Longitude:F4}";

            if (location is null)
            {
                PrayerRows.Clear();
                NextPrayerPrefix = "Next:";
                NextPrayerName = "—";
                NextPrayerTime = "—";
                NextPrayerCountdown = "00:00:00";
                ShowLiveCountdown = false;
                return;
            }

            var calcSettings = _settingsStore.GetCalculationSettings();
            var notifSettings = _settingsStore.GetNotificationSettings();

            MethodLabel = FormatMethodName(calcSettings.Method);
            AsrLabel = calcSettings.Madhab.ToString();

            var dateOffset = new DateTimeOffset(
                _selectedDate.Year,
                _selectedDate.Month,
                _selectedDate.Day,
                0, 0, 0,
                TimeSpan.Zero);

            var schedule = await _calculator.CalculateDailyScheduleAsync(
                location,
                dateOffset,
                calcSettings);

            PrayerTime? highlightedPrayer = null;

            if (IsToday)
            {
                NextPrayerPrefix = "Next:";
                ShowLiveCountdown = true;

                var now = DateTimeOffset.UtcNow;

                // Use _allEnabled so next-prayer display is never gated on notification prefs.
                var nextForDisplay = await _calculator.CalculateNextPrayerAsync(
                    schedule,
                    _allEnabled,
                    now);

                var countdownForDisplay = await _calculator.CalculateCountdownAsync(
                    schedule,
                    _allEnabled,
                    now);

                if (nextForDisplay is not null)
                {
                    NextPrayerName        = nextForDisplay.Type.ToString();
                    NextPrayerTime        = nextForDisplay.Time.ToLocalTime().ToString("HH:mm");
                    NextPrayerAlarmEnabled = IsNotifEnabled(notifSettings, nextForDisplay.Type);

                    NextPrayerCountdown = countdownForDisplay is not null
                        ? FormatCountdown(countdownForDisplay.RemainingSeconds)
                        : "00:00:00";

                    StartCountdownTimer(schedule);
                }
                else
                {
                    NextPrayerName = "—";
                    NextPrayerTime = "—";
                    NextPrayerCountdown = "00:00:00";
                    ShowLiveCountdown = false;
                }
            }
            else
            {
                NextPrayerPrefix = "First prayer:";
                ShowLiveCountdown = false;

                var firstPrayer = schedule.Prayers
                    .Where(p => p.Type != PrayerType.Sunrise)
                    .OrderBy(p => p.DateTime)
                    .Cast<PrayerTime?>()
                    .FirstOrDefault();

                if (firstPrayer.HasValue)
                {
                    NextPrayerName = firstPrayer.Value.Type.ToString();
                    NextPrayerTime = firstPrayer.Value.DateTime.ToLocalTime().ToString("HH:mm");
                    highlightedPrayer = firstPrayer.Value;
                }
                else
                {
                    NextPrayerName = "—";
                    NextPrayerTime = "—";
                }

                NextPrayerCountdown = string.Empty;
            }

            PrayerRows.Clear();

            foreach (var prayer in schedule.Prayers.Where(p => p.Type != PrayerType.Sunrise))
            {
                PrayerRows.Add(new PrayerRowItem(
                    prayer.Type.ToString(),
                    prayer.DateTime.ToLocalTime().ToString("HH:mm"),
                    highlightedPrayer.HasValue && prayer.Type == highlightedPrayer.Value.Type,
                    IsNotifEnabled(notifSettings, prayer.Type)));
            }
        }
        finally
        {
            IsLoading = false;
        }
    }

    // ── Navigation commands ──────────────────────────────────────────────────

    [RelayCommand]
    private async Task GoQibla() => await Shell.Current.GoToAsync("qibla");

    [RelayCommand]
    private async Task GoMap() => await Shell.Current.GoToAsync("map");

    [RelayCommand]
    private async Task GoTimes() => await Shell.Current.GoToAsync("times");

    [RelayCommand]
    private async Task GoSettings() => await Shell.Current.GoToAsync("settings");

    [RelayCommand]
    private async Task GoNotificationSettings() => await Shell.Current.GoToAsync("sound-settings");

    // ── Day navigation commands ──────────────────────────────────────────────

    [RelayCommand]
    private async Task PreviousDay()
    {
        _selectedDate = _selectedDate.AddDays(-1);
        await LoadAsync();
    }

    [RelayCommand]
    private async Task NextDay()
    {
        _selectedDate = _selectedDate.AddDays(1);
        await LoadAsync();
    }

    [RelayCommand]
    private async Task GoToday()
    {
        _selectedDate = DateOnly.FromDateTime(DateTime.Today);
        await LoadAsync();
    }

    // ── Cleanup (called by page on Disappearing) ─────────────────────────────

    public void Cleanup() => StopCountdownTimer();

    // ── Countdown timer ──────────────────────────────────────────────────────

    private void StartCountdownTimer(DailyPrayerSchedule schedule)
    {
        _countdownCts = new CancellationTokenSource();
        var token = _countdownCts.Token;

        _ = Task.Run(async () =>
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(1000, token);
                }
                catch (TaskCanceledException)
                {
                    return;
                }

                var now             = DateTimeOffset.UtcNow;
                var notifSettings   = _settingsStore.GetNotificationSettings();

                var countdown = await _calculator.CalculateCountdownAsync(
                    schedule,
                    _allEnabled,
                    now);

                var nextResult = await _calculator.CalculateNextPrayerAsync(
                    schedule,
                    _allEnabled,
                    now);

                Microsoft.Maui.ApplicationModel.MainThread.BeginInvokeOnMainThread(() =>
                {
                    NextPrayerCountdown = countdown is not null
                        ? FormatCountdown(countdown.RemainingSeconds)
                        : "00:00:00";

                    if (nextResult is not null)
                    {
                        NextPrayerName         = nextResult.Type.ToString();
                        NextPrayerTime         = nextResult.Time.ToLocalTime().ToString("HH:mm");
                        NextPrayerAlarmEnabled = IsNotifEnabled(notifSettings, nextResult.Type);

                        var liveNotifSettings = _settingsStore.GetNotificationSettings();
                        for (var i = 0; i < PrayerRows.Count; i++)
                        {
                            var row = PrayerRows[i];
                            if (Enum.TryParse<PrayerType>(row.Name, out var rowType))
                            {
                                PrayerRows[i] = new PrayerRowItem(
                                    row.Name,
                                    row.Time,
                                    row.Name == NextPrayerName,
                                    IsNotifEnabled(liveNotifSettings, rowType));
                            }
                        }
                    }
                });
            }
        }, token);
    }

    private void StopCountdownTimer()
    {
        _countdownCts?.Cancel();
        _countdownCts?.Dispose();
        _countdownCts = null;
    }

    // ── Formatting helpers ───────────────────────────────────────────────────

    private static string FormatCountdown(int totalSeconds)
    {
        var ts = TimeSpan.FromSeconds(Math.Max(0, totalSeconds));
        var totalHours = (int)ts.TotalHours;
        return $"{totalHours:D2}:{ts.Minutes:D2}:{ts.Seconds:D2}";
    }

    private static string FormatSelectedDate(DateOnly date)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);

        if (date == today)
        {
            return "Today";
        }

        if (date == today.AddDays(1))
        {
            return "Tomorrow";
        }

        if (date == today.AddDays(-1))
        {
            return "Yesterday";
        }

        return date.ToString("ddd, MMM d");
    }

    private static string FormatMethodName(CalculationMethod method) => method switch
    {
        CalculationMethod.MuslimWorldLeague => "MWL",
        CalculationMethod.EgyptianGeneralAuthority => "Egyptian",
        CalculationMethod.ISNA => "ISNA",
        CalculationMethod.Karachi => "Karachi",
        CalculationMethod.Kuwait => "Kuwait",
        CalculationMethod.UmmAlQura => "Umm al-Qura",
        _ => method.ToString()
    };

    private static bool IsNotifEnabled(PrayerNotificationSettings s, PrayerType type) => type switch
    {
        PrayerType.Fajr    => s.FajrEnabled,
        PrayerType.Dhuhr   => s.DhuhrEnabled,
        PrayerType.Asr     => s.AsrEnabled,
        PrayerType.Maghrib => s.MaghribEnabled,
        PrayerType.Isha    => s.IshaEnabled,
        _                  => false,
    };
}