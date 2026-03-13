using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QiblaNow.Core.Abstractions;
using QiblaNow.Core.Models;

namespace QiblaNow.Presentation.ViewModels;

internal enum HomeState
{
    Initializing,
    ResolvingLocation,
    LocationUnavailable,
    Ready
}

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

    // ── Hijri date ───────────────────────────────────────────────────────────
    [ObservableProperty] private string _hijriDateLabel = string.Empty;
    [ObservableProperty] private bool _hasHijriDate;

    // ── Loading ──────────────────────────────────────────────────────────────
    [ObservableProperty] private bool _isLoading;

    // ── Next-prayer alarm indicator ──────────────────────────────────────────
    [ObservableProperty] private bool _nextPrayerAlarmEnabled;

    // ── Prayer rows ──────────────────────────────────────────────────────────
    public ObservableCollection<PrayerRowItem> PrayerRows { get; } = new();

    // ── State machine ────────────────────────────────────────────────────────
    private HomeState _state = HomeState.Initializing;
    private LocationSnapshot? _location;
    private DailyPrayerSchedule? _schedule;
    private int _generation;
    private CancellationTokenSource? _countdownCts;
    private DateOnly _selectedDate;

    // The single authoritative next-prayer target (may be from today or tomorrow).
    // null when no enabled prayer exists or location is unavailable.
    private PrayerTime? _target;
    // True when _target belongs to the currently displayed _schedule.
    private bool _targetBelongsToDisplayedSchedule;

    // All enabled: used to determine next-prayer display regardless of notification prefs.
    private static readonly HashSet<PrayerType> _allEnabledSet = new()
    {
        PrayerType.Fajr,
        PrayerType.Dhuhr,
        PrayerType.Asr,
        PrayerType.Maghrib,
        PrayerType.Isha,
    };

    public HomeViewModel(
        ISettingsStore settingsStore,
        ILocationService locationService,
        IPrayerTimesCalculator calculator)
    {
        _settingsStore   = settingsStore;
        _locationService = locationService;
        _calculator      = calculator;

        _selectedDate = DateOnly.FromDateTime(DateTime.Today);
        SelectedDateLabel = FormatSelectedDate(_selectedDate);
        HijriDateLabel = FormatHijriDate(_selectedDate);
        HasHijriDate = !string.IsNullOrEmpty(HijriDateLabel);
    }

    partial void OnIsTodayChanged(bool value) => OnPropertyChanged(nameof(IsNotToday));

    // ── Public entry point called by the page on Appearing ──────────────────
    /// <summary>
    /// Full startup: resolves location then computes the schedule and starts the
    /// countdown. Increments <c>_generation</c> so any in-flight prior call is
    /// automatically invalidated after every await.
    /// </summary>
    public async Task InitializeAsync()
    {
        int version = ++_generation;
        StopCountdownTimer();

        _state = HomeState.ResolvingLocation;
        IsLoading = true;
        LocationLabel = "Detecting...";

        // Reset to today on every full initialization.
        _selectedDate = DateOnly.FromDateTime(DateTime.Today);
        IsToday = true;
        SelectedDateLabel = FormatSelectedDate(_selectedDate);
        HijriDateLabel = FormatHijriDate(_selectedDate);
        HasHijriDate = !string.IsNullOrEmpty(HijriDateLabel);
        ShowLiveCountdown = true;

        try
        {
            // Step 1: Try GPS with timeout.
            var snapshot = await _locationService.TryGetGpsLocationAsync(TimeSpan.FromSeconds(10));
            if (version != _generation) return;

            // Step 2: Fallback to last known snapshot.
            if (snapshot is null)
                snapshot = _settingsStore.GetLastSnapshot();

            if (version != _generation) return;

            // Step 3: No location at all.
            if (snapshot is null)
            {
                _state = HomeState.LocationUnavailable;
                LocationLabel = "Location unavailable";
                PrayerRows.Clear();
                NextPrayerPrefix    = "Next:";
                NextPrayerName      = "—";
                NextPrayerTime      = "—";
                NextPrayerCountdown = "00:00:00";
                ShowLiveCountdown   = false;
                return;
            }

            _location     = snapshot;
            LocationLabel = FormatLocationLabel(snapshot);
            _state        = HomeState.Ready;

            await LoadPrayerScheduleAsync(version);
        }
        finally
        {
            if (version == _generation)
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
        int version = ++_generation;
        StopCountdownTimer();
        await LoadPrayerScheduleAsync(version);
    }

    [RelayCommand]
    private async Task NextDay()
    {
        _selectedDate = _selectedDate.AddDays(1);
        int version = ++_generation;
        StopCountdownTimer();
        await LoadPrayerScheduleAsync(version);
    }

    [RelayCommand]
    private async Task GoToday()
    {
        _selectedDate = DateOnly.FromDateTime(DateTime.Today);
        int version = ++_generation;
        StopCountdownTimer();
        await LoadPrayerScheduleAsync(version);
    }

    // ── Cleanup (called by page on Disappearing) ─────────────────────────────

    public void Cleanup() => StopCountdownTimer();

    // ── Schedule computation ─────────────────────────────────────────────────

    private async Task LoadPrayerScheduleAsync(int version)
    {
        if (_location is null || _state != HomeState.Ready) return;

        var today = DateOnly.FromDateTime(DateTime.Today);
        IsToday = _selectedDate == today;
        SelectedDateLabel = FormatSelectedDate(_selectedDate);
        HijriDateLabel = FormatHijriDate(_selectedDate);
        HasHijriDate = !string.IsNullOrEmpty(HijriDateLabel);
        ShowLiveCountdown = IsToday;

        var calcSettings  = _settingsStore.GetCalculationSettings();
        var notifSettings = _settingsStore.GetNotificationSettings();

        MethodLabel = FormatMethodName(calcSettings.Method);
        AsrLabel    = calcSettings.Madhab.ToString();

        var dateOffset = new DateTimeOffset(
            _selectedDate.Year,
            _selectedDate.Month,
            _selectedDate.Day,
            0, 0, 0,
            TimeSpan.Zero);

        var schedule = await _calculator.CalculateDailyScheduleAsync(
            _location,
            dateOffset,
            calcSettings);

        if (version != _generation) return;

        _schedule = schedule;

        // ── Derive the single authoritative target ─────────────────────
        _target = null;
        _targetBelongsToDisplayedSchedule = false;

        if (IsToday)
        {
            NextPrayerPrefix  = "Next:";
            ShowLiveCountdown = true;

            var now = DateTimeOffset.UtcNow;

            // 1) Search today's schedule for a future enabled prayer.
            var found = _calculator.FindNextPrayerInSchedule(schedule, _allEnabledSet, now);

            if (found.HasValue)
            {
                _target = found.Value;
                _targetBelongsToDisplayedSchedule = true;
            }
            else
            {
                // 2) Today exhausted — compute tomorrow's schedule explicitly.
                var tomorrowDate = _selectedDate.AddDays(1);
                var tomorrowOffset = new DateTimeOffset(
                    tomorrowDate.Year,
                    tomorrowDate.Month,
                    tomorrowDate.Day,
                    0, 0, 0,
                    TimeSpan.Zero);

                var tomorrowSchedule = await _calculator.CalculateDailyScheduleAsync(
                    _location,
                    tomorrowOffset,
                    calcSettings);

                if (version != _generation) return;

                var firstTomorrow = _calculator.FindFirstEnabledPrayer(tomorrowSchedule, _allEnabledSet);
                if (firstTomorrow.HasValue)
                {
                    _target = firstTomorrow.Value;
                    _targetBelongsToDisplayedSchedule = false;
                }
            }

            if (_target.HasValue)
            {
#if DEBUG
                var nowCheck = DateTimeOffset.UtcNow;
                if (_target.Value.DateTime <= nowCheck)
                    throw new InvalidOperationException(
                        $"Next prayer invariant broken: {_target.Value.Type} at {_target.Value.DateTime:u} is not after {nowCheck:u}.");
#endif
                NextPrayerName         = _target.Value.Type.ToString();
                NextPrayerTime         = _target.Value.DateTime.ToLocalTime().ToString("HH:mm");
                NextPrayerAlarmEnabled = IsNotifEnabled(notifSettings, _target.Value.Type);
                NextPrayerCountdown    = FormatCountdown(_target.Value.DateTime);

                StartCountdownLoop(schedule, calcSettings, version);
            }
            else
            {
                NextPrayerName      = "—";
                NextPrayerTime      = "—";
                NextPrayerCountdown = "00:00:00";
                ShowLiveCountdown   = false;
            }
        }
        else
        {
            // Non-today: show only the schedule. No next-prayer semantics, no highlight.
            ShowLiveCountdown = false;
            NextPrayerName    = "—";
            NextPrayerTime    = "—";
            NextPrayerCountdown = string.Empty;
        }

        // Build rows — highlight only if target belongs to the displayed schedule.
        PrayerRows.Clear();
        foreach (var prayer in schedule.Prayers.Where(p => p.Type != PrayerType.Sunrise))
        {
            PrayerRows.Add(new PrayerRowItem(
                prayer.Type.ToString(),
                prayer.DateTime.ToLocalTime().ToString("HH:mm"),
                _targetBelongsToDisplayedSchedule
                    && _target.HasValue
                    && prayer.DateTime == _target.Value.DateTime,
                IsNotifEnabled(notifSettings, prayer.Type)));
        }
    }

    // ── Countdown loop ───────────────────────────────────────────────────────

    /// <summary>
    /// Ticks every second. Recomputes the authoritative target using
    /// today → tomorrow orchestration (no +24h approximation). Derives
    /// all mutable UI from that single target.
    /// </summary>
    private void StartCountdownLoop(
        DailyPrayerSchedule todaySchedule,
        PrayerCalculationSettings calcSettings,
        int version)
    {
        _countdownCts = new CancellationTokenSource();
        var token = _countdownCts.Token;

        _ = Task.Run(async () =>
        {
            // Cache tomorrow's schedule — computed once, lazily.
            DailyPrayerSchedule? tomorrowSchedule = null;

            while (!token.IsCancellationRequested && version == _generation)
            {
                try
                {
                    await Task.Delay(1000, token);
                }
                catch (TaskCanceledException)
                {
                    return;
                }

                if (version != _generation) return;

                var now           = DateTimeOffset.UtcNow;
                var notifSettings = _settingsStore.GetNotificationSettings();

                // 1) Try today
                var nextInToday = _calculator.FindNextPrayerInSchedule(todaySchedule, _allEnabledSet, now);
                PrayerTime? target;
                bool belongsToToday;

                if (nextInToday.HasValue)
                {
                    target = nextInToday.Value;
                    belongsToToday = true;
                }
                else
                {
                    // 2) Compute tomorrow (once)
                    if (tomorrowSchedule is null && _location is not null)
                    {
                        var todayDate = DateOnly.FromDateTime(DateTime.Today);
                        var tomorrowDate = todayDate.AddDays(1);
                        var tomorrowOffset = new DateTimeOffset(
                            tomorrowDate.Year, tomorrowDate.Month, tomorrowDate.Day,
                            0, 0, 0, TimeSpan.Zero);

                        tomorrowSchedule = await _calculator.CalculateDailyScheduleAsync(
                            _location, tomorrowOffset, calcSettings);

                        if (version != _generation) return;
                    }

                    var firstTomorrow = tomorrowSchedule is not null
                        ? _calculator.FindFirstEnabledPrayer(tomorrowSchedule, _allEnabledSet)
                        : null;

                    target = firstTomorrow;
                    belongsToToday = false;
                }

                if (version != _generation) return;

                Microsoft.Maui.ApplicationModel.MainThread.BeginInvokeOnMainThread(() =>
                {
                    if (version != _generation) return;

                    NextPrayerCountdown = target.HasValue
                        ? FormatCountdown(target.Value.DateTime)
                        : "00:00:00";

                    if (target.HasValue)
                    {
                        NextPrayerName         = target.Value.Type.ToString();
                        NextPrayerTime         = target.Value.DateTime.ToLocalTime().ToString("HH:mm");
                        NextPrayerAlarmEnabled = IsNotifEnabled(notifSettings, target.Value.Type);

                        for (var i = 0; i < PrayerRows.Count; i++)
                        {
                            var row = PrayerRows[i];
                            if (!Enum.TryParse<PrayerType>(row.Name, out var rowType)) continue;

                            var entry = todaySchedule.Prayers
                                .Where(p => p.Type == rowType)
                                .Cast<PrayerTime?>()
                                .FirstOrDefault();
                            var isHighlighted = belongsToToday
                                && entry.HasValue
                                && entry.Value.DateTime == target.Value.DateTime;
                            var alarmEnabled = IsNotifEnabled(notifSettings, rowType);

                            if (row.IsHighlighted != isHighlighted || row.AlarmEnabled != alarmEnabled)
                            {
                                PrayerRows[i] = new PrayerRowItem(
                                    row.Name,
                                    row.Time,
                                    isHighlighted,
                                    alarmEnabled);
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

    private static string FormatLocationLabel(LocationSnapshot snapshot) =>
        !string.IsNullOrWhiteSpace(snapshot.Label)
            ? snapshot.Label
            : $"{snapshot.Latitude:F4}, {snapshot.Longitude:F4}";

    private static readonly string[] _hijriMonthNames =
    [
        "Muharram", "Safar", "Rabi' al-Awwal", "Rabi' al-Thani",
        "Jumada al-Awwal", "Jumada al-Thani", "Rajab", "Sha'ban",
        "Ramadan", "Shawwal", "Dhu al-Qa'dah", "Dhu al-Hijjah"
    ];

    private static string FormatHijriDate(DateOnly date)
    {
        try
        {
            var cal   = new HijriCalendar();
            var dt    = date.ToDateTime(TimeOnly.MinValue);
            var day   = cal.GetDayOfMonth(dt);
            var month = cal.GetMonth(dt);
            var year  = cal.GetYear(dt);
            var name  = month >= 1 && month <= 12
                ? _hijriMonthNames[month - 1]
                : month.ToString();
            return $"{day} {name} {year} AH";
        }
        catch
        {
            return string.Empty;
        }
    }

    private static string FormatCountdown(DateTimeOffset target)
    {
        var remaining = target - DateTimeOffset.UtcNow;
        var ts         = TimeSpan.FromSeconds(Math.Max(0, remaining.TotalSeconds));
        var totalHours = (int)ts.TotalHours;
        return $"{totalHours:D2}:{ts.Minutes:D2}:{ts.Seconds:D2}";
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
        CalculationMethod.MuslimWorldLeague        => "MWL",
        CalculationMethod.EgyptianGeneralAuthority => "Egyptian",
        CalculationMethod.ISNA                     => "ISNA",
        CalculationMethod.Karachi                  => "Karachi",
        CalculationMethod.Kuwait                   => "Kuwait",
        CalculationMethod.UmmAlQura                => "Umm al-Qura",
        _                                          => method.ToString()
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
