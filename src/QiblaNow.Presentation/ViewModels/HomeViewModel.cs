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
    [ObservableProperty] private string _nextPrayerName = "—";
    [ObservableProperty] private string _nextPrayerTime = "—";
    [ObservableProperty] private string _nextPrayerCountdown = "—";

    // ── Location ─────────────────────────────────────────────────────────────
    [ObservableProperty] private string _locationLabel = "Detecting...";

    // ── Schedule meta ─────────────────────────────────────────────────────────
    [ObservableProperty] private string _methodLabel = string.Empty;
    [ObservableProperty] private string _asrLabel = string.Empty;

    // ── Day navigation ────────────────────────────────────────────────────────
    [ObservableProperty] private string _selectedDateLabel = string.Empty;
    [ObservableProperty] private bool _isToday;

    public bool IsNotToday => !IsToday;

    // ── Loading ───────────────────────────────────────────────────────────────
    [ObservableProperty] private bool _isLoading;

    // ── Prayer rows ───────────────────────────────────────────────────────────
    public ObservableCollection<PrayerRowItem> PrayerRows { get; } = new();

    // ── Internal state ────────────────────────────────────────────────────────
    private DateOnly _selectedDate;
    private CancellationTokenSource? _countdownCts;

    public HomeViewModel(
        ISettingsStore settingsStore,
        ILocationService locationService,
        IPrayerTimesCalculator calculator)
    {
        _settingsStore   = settingsStore;
        _locationService = locationService;
        _calculator      = calculator;

        _selectedDate    = DateOnly.FromDateTime(DateTime.Today);
        SelectedDateLabel = FormatSelectedDate(_selectedDate);
    }

    // Notify IsNotToday whenever IsToday changes
    partial void OnIsTodayChanged(bool value) => OnPropertyChanged(nameof(IsNotToday));

    // ── Public entry point called by the page on Appearing ───────────────────
    public async Task LoadAsync()
    {
        StopCountdownTimer();
        IsLoading = true;
        try
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            IsToday           = _selectedDate == today;
            SelectedDateLabel = FormatSelectedDate(_selectedDate);

            var location = await _locationService.GetCurrentLocationAsync();
            LocationLabel = location is null
                ? "Location unavailable"
                : !string.IsNullOrEmpty(location.Label)
                    ? location.Label
                    : $"{location.Latitude:F4}, {location.Longitude:F4}";

            if (location is null)
            {
                PrayerRows.Clear();
                NextPrayerName      = "—";
                NextPrayerTime      = "—";
                NextPrayerCountdown = "Set a location in Settings";
                return;
            }

            var calcSettings  = _settingsStore.GetCalculationSettings();
            var notifSettings = _settingsStore.GetNotificationSettings();

            MethodLabel = $"Method: {FormatMethodName(calcSettings.Method)}";
            AsrLabel    = $"Asr: {calcSettings.Madhab}";

            var dateOffset = new DateTimeOffset(
                _selectedDate.Year, _selectedDate.Month, _selectedDate.Day,
                0, 0, 0, TimeSpan.Zero);

            var schedule = await _calculator.CalculateDailyScheduleAsync(
                location, dateOffset, calcSettings);

            // Build prayer rows (exclude Sunrise)
            PrayerRows.Clear();
            foreach (var prayer in schedule.Prayers.Where(p => p.Type != PrayerType.Sunrise))
                PrayerRows.Add(new PrayerRowItem(
                    prayer.Type.ToString(),
                    prayer.DateTime.ToLocalTime().ToString("HH:mm")));

            // Next-prayer block
            var now = DateTimeOffset.UtcNow;
            if (IsToday)
            {
                var nextResult = await _calculator.CalculateNextPrayerAsync(
                    schedule, notifSettings, now);

                if (nextResult is not null)
                {
                    NextPrayerName = nextResult.Type.ToString();
                    NextPrayerTime = nextResult.Time.ToLocalTime().ToString("HH:mm");

                    var countdown = await _calculator.CalculateCountdownAsync(
                        schedule, notifSettings, now);
                    NextPrayerCountdown = countdown is not null
                        ? FormatCountdown(countdown.RemainingSeconds)
                        : "All prayers completed";

                    StartCountdownTimer(schedule);
                }
                else
                {
                    NextPrayerName      = "—";
                    NextPrayerTime      = "—";
                    NextPrayerCountdown = "All prayers completed today";
                }
            }
            else
            {
                // Non-today: show the first prayer of the selected day, no live countdown
                var firstPrayer = schedule.Prayers
                    .Where(p => p.Type != PrayerType.Sunrise)
                    .OrderBy(p => p.DateTime)
                    .Cast<PrayerTime?>()
                    .FirstOrDefault();

                if (firstPrayer.HasValue)
                {
                    NextPrayerName = firstPrayer.Value.Type.ToString();
                    NextPrayerTime = firstPrayer.Value.DateTime.ToLocalTime().ToString("HH:mm");
                }
                else
                {
                    NextPrayerName = "—";
                    NextPrayerTime = "—";
                }

                var dayDiff         = _selectedDate.DayNumber - today.DayNumber;
                NextPrayerCountdown = dayDiff > 0
                    ? $"In {dayDiff} day{(dayDiff == 1 ? "" : "s")}"
                    : "Past date";
            }
        }
        finally
        {
            IsLoading = false;
        }
    }

    // ── Navigation commands ───────────────────────────────────────────────────

    [RelayCommand]
    private async Task GoQibla() => await Shell.Current.GoToAsync("qibla");

    [RelayCommand]
    private async Task GoMap() => await Shell.Current.GoToAsync("map");

    [RelayCommand]
    private async Task GoTimes() => await Shell.Current.GoToAsync("times");

    [RelayCommand]
    private async Task GoSettings() => await Shell.Current.GoToAsync("settings");

    // ── Day navigation commands ───────────────────────────────────────────────

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

    // ── Cleanup (called by page on Disappearing) ──────────────────────────────

    public void Cleanup() => StopCountdownTimer();

    // ── Countdown timer ───────────────────────────────────────────────────────

    private void StartCountdownTimer(DailyPrayerSchedule schedule)
    {
        _countdownCts = new CancellationTokenSource();
        var token = _countdownCts.Token;

        _ = Task.Run(async () =>
        {
            while (!token.IsCancellationRequested)
            {
                try   { await Task.Delay(1000, token); }
                catch (TaskCanceledException) { return; }

                var now           = DateTimeOffset.UtcNow;
                var notifSettings = _settingsStore.GetNotificationSettings();
                var countdown     = await _calculator.CalculateCountdownAsync(
                    schedule, notifSettings, now);
                var nextResult    = await _calculator.CalculateNextPrayerAsync(
                    schedule, notifSettings, now);

                Microsoft.Maui.ApplicationModel.MainThread.BeginInvokeOnMainThread(() =>
                {
                    NextPrayerCountdown = countdown is not null
                        ? FormatCountdown(countdown.RemainingSeconds)
                        : "All prayers completed";

                    if (nextResult is not null)
                    {
                        NextPrayerName = nextResult.Type.ToString();
                        NextPrayerTime = nextResult.Time.ToLocalTime().ToString("HH:mm");
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

    // ── Formatting helpers ────────────────────────────────────────────────────

    private static string FormatCountdown(int totalSeconds)
    {
        var ts = TimeSpan.FromSeconds(totalSeconds);
        return ts.TotalHours >= 1
            ? $"{(int)ts.TotalHours}h {ts.Minutes:D2}m {ts.Seconds:D2}s remaining"
            : $"{ts.Minutes}m {ts.Seconds:D2}s remaining";
    }

    private static string FormatSelectedDate(DateOnly date)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        if (date == today)                return "Today";
        if (date == today.AddDays(1))     return "Tomorrow";
        if (date == today.AddDays(-1))    return "Yesterday";
        return date.ToString("ddd, MMM d");
    }

    private static string FormatMethodName(CalculationMethod method) => method switch
    {
        CalculationMethod.MuslimWorldLeague       => "MWL",
        CalculationMethod.EgyptianGeneralAuthority => "Egyptian",
        CalculationMethod.ISNA                    => "ISNA",
        CalculationMethod.Karachi                 => "Karachi",
        CalculationMethod.Kuwait                  => "Kuwait",
        CalculationMethod.UmmAlQura               => "Umm al-Qura",
        _                                         => method.ToString()
    };
}
