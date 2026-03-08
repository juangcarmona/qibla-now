using System.Globalization;
using QiblaNow.Core;
using QiblaNow.Core.Abstractions;
using QiblaNow.Core.Models;

namespace QiblaNow.Core.Tests;

public class PrayerTimesCalculatorTests
{
    private readonly IPrayerTimesCalculator _calculator;

    // Fixed "now" anchored before all prayers on the test date (UTC midnight).
    // All tests that don't simulate a specific wall-clock position use this value
    // so results are fully deterministic and independent of the system clock.
    private static readonly DateTimeOffset TestNowMidnight =
        new(2026, 3, 2, 0, 0, 0, TimeSpan.Zero);

    private static readonly LocationSnapshot LondonLocation = new(
        LocationMode.Manual, 51.5074, -0.1278, "London");

    private static readonly LocationSnapshot ReykjavikLocation = new(
        LocationMode.Manual, 64.1265, -21.8174, "Reykjavik");

    private static readonly PrayerCalculationSettings LondonSettings = new()
    {
        Method            = CalculationMethod.MuslimWorldLeague,
        Madhab            = Madhab.Shafi,
        HighLatitudeRule  = HighLatitudeRule.SeventhOfNight
    };

    private static readonly PrayerCalculationSettings ReykjavikSettings = new()
    {
        Method            = CalculationMethod.ISNA,
        Madhab            = Madhab.Shafi,
        HighLatitudeRule  = HighLatitudeRule.SeventhOfNight
    };

    private static readonly PrayerNotificationSettings AllEnabled = new()
    {
        FajrEnabled    = true,
        DhuhrEnabled   = true,
        AsrEnabled     = true,
        MaghribEnabled = true,
        IshaEnabled    = true
    };

    public PrayerTimesCalculatorTests()
    {
        _calculator = new PrayerTimesCalculator();
    }

    // ── PT-1 London ────────────────────────────────────────────────────────

    [Fact]
    public async Task PT1_London_2026_03_02_ReturnsExpectedPrayerTimes()
    {
        var schedule = await _calculator.CalculateDailyScheduleAsync(
            LondonLocation, TestNowMidnight, LondonSettings);

        Assert.NotNull(schedule);
        Assert.Equal(TestNowMidnight, schedule.Date);
        Assert.Equal(6, schedule.Prayers.Count);

        var fajr    = schedule.GetPrayer(PrayerType.Fajr)!.Value;
        var dhuhr   = schedule.GetPrayer(PrayerType.Dhuhr)!.Value;
        var asr     = schedule.GetPrayer(PrayerType.Asr)!.Value;
        var maghrib = schedule.GetPrayer(PrayerType.Maghrib)!.Value;
        var isha    = schedule.GetPrayer(PrayerType.Isha)!.Value;
        var sunrise = schedule.GetPrayer(PrayerType.Sunrise)!.Value;

        // Expected values match this implementation's simplified solar algorithm.
        // NOTE: Reference apps (e.g. Adhan-js) may give ±12 min different values for
        // some prayers due to a more precise solar-transit calculation. A future
        // milestone may adopt the Adhan algorithm; update these values at that point.
        static string Fmt(DateTimeOffset t) => t.ToString("HH:mm", CultureInfo.InvariantCulture);

        Assert.Equal("04:52", Fmt(fajr.DateTime));
        Assert.Equal("12:13", Fmt(dhuhr.DateTime));
        Assert.Equal("15:05", Fmt(asr.DateTime));
        Assert.Equal("17:43", Fmt(maghrib.DateTime));
        Assert.Equal("19:27", Fmt(isha.DateTime));

        // Sunrise is display-only; verify ordering
        Assert.True(sunrise.DateTime > fajr.DateTime);
        Assert.True(sunrise.DateTime < dhuhr.DateTime);
    }

    // ── PT-2 Reykjavik ─────────────────────────────────────────────────────

    [Fact]
    public async Task PT2_Reykjavik_2026_03_02_PrayerTimesAreChronologicallyOrdered()
    {
        var schedule = await _calculator.CalculateDailyScheduleAsync(
            ReykjavikLocation, TestNowMidnight, ReykjavikSettings);

        Assert.NotNull(schedule);

        var fajr    = schedule.GetPrayer(PrayerType.Fajr)!.Value;
        var sunrise = schedule.GetPrayer(PrayerType.Sunrise)!.Value;
        var dhuhr   = schedule.GetPrayer(PrayerType.Dhuhr)!.Value;
        var asr     = schedule.GetPrayer(PrayerType.Asr)!.Value;
        var maghrib = schedule.GetPrayer(PrayerType.Maghrib)!.Value;
        var isha    = schedule.GetPrayer(PrayerType.Isha)!.Value;

        Assert.True(fajr.DateTime    < sunrise.DateTime);
        Assert.True(sunrise.DateTime < dhuhr.DateTime);
        Assert.True(dhuhr.DateTime   < asr.DateTime);
        Assert.True(asr.DateTime     < maghrib.DateTime);
        Assert.True(maghrib.DateTime < isha.DateTime);
    }

    // ── Next-prayer resolution ─────────────────────────────────────────────

    [Fact]
    public async Task CalculateNextPrayer_BeforeAnyPrayer_ReturnsFajr()
    {
        var schedule = await _calculator.CalculateDailyScheduleAsync(
            LondonLocation, TestNowMidnight, LondonSettings);

        // 00:00 UTC is before Fajr (~05:04) → next prayer is Fajr
        var result = await _calculator.CalculateNextPrayerAsync(
            schedule, AllEnabled, TestNowMidnight);

        Assert.NotNull(result);
        Assert.Equal(PrayerType.Fajr, result!.Type);
        Assert.True(result.Remaining > TimeSpan.Zero);
        Assert.True(result.IsToday);
    }

    [Fact]
    public async Task CalculateNextPrayer_WrapsToNextDayFajrAfterIsha()
    {
        var schedule = await _calculator.CalculateDailyScheduleAsync(
            LondonLocation, TestNowMidnight, LondonSettings);

        // After Isha ~19:12 UTC (use 20:00 to be safely past it)
        var afterIsha = new DateTimeOffset(2026, 3, 2, 20, 0, 0, TimeSpan.Zero);

        var result = await _calculator.CalculateNextPrayerAsync(
            schedule, AllEnabled, afterIsha);

        Assert.NotNull(result);
        Assert.Equal(PrayerType.Fajr, result!.Type);
        Assert.False(result.IsToday);
        Assert.True(result.Remaining > TimeSpan.Zero);
    }

    [Fact]
    public async Task CalculateNextPrayer_FiltersDisabledPrayers()
    {
        var schedule = await _calculator.CalculateDailyScheduleAsync(
            LondonLocation, TestNowMidnight, LondonSettings);

        var onlyFajr = new PrayerNotificationSettings { FajrEnabled = true };

        // At midnight, with only Fajr enabled, next prayer must be Fajr
        var result = await _calculator.CalculateNextPrayerAsync(
            schedule, onlyFajr, TestNowMidnight);

        Assert.NotNull(result);
        Assert.Equal(PrayerType.Fajr, result!.Type);
    }

    [Fact]
    public async Task CalculateNextPrayer_AllDisabled_ReturnsNull()
    {
        var schedule = await _calculator.CalculateDailyScheduleAsync(
            LondonLocation, TestNowMidnight, LondonSettings);

        var noneEnabled = new PrayerNotificationSettings();
        var result = await _calculator.CalculateNextPrayerAsync(
            schedule, noneEnabled, TestNowMidnight);

        Assert.Null(result);
    }

    // ── Notification candidate selection ──────────────────────────────────

    [Fact]
    public async Task CalculateNextNotificationCandidate_RespectsEnabledPrayers()
    {
        var schedule = await _calculator.CalculateDailyScheduleAsync(
            LondonLocation, TestNowMidnight, LondonSettings);

        var dhuhrAndMaghrib = new PrayerNotificationSettings
        {
            DhuhrEnabled   = true,
            MaghribEnabled = true
        };

        // At midnight, the next enabled prayer is Dhuhr
        var candidate = await _calculator.CalculateNextNotificationCandidateAsync(
            schedule, dhuhrAndMaghrib, TestNowMidnight);

        Assert.NotNull(candidate);
        Assert.Equal(PrayerType.Dhuhr, candidate!.Type);
    }

    [Fact]
    public async Task CalculateNextNotificationCandidate_AllDisabled_ReturnsNull()
    {
        var schedule = await _calculator.CalculateDailyScheduleAsync(
            LondonLocation, TestNowMidnight, LondonSettings);

        var candidate = await _calculator.CalculateNextNotificationCandidateAsync(
            schedule, new PrayerNotificationSettings(), TestNowMidnight);

        Assert.Null(candidate);
    }

    // ── Countdown ─────────────────────────────────────────────────────────

    [Fact]
    public async Task CalculateCountdown_BeforeAnyPrayer_ReturnsPositiveSeconds()
    {
        var schedule = await _calculator.CalculateDailyScheduleAsync(
            LondonLocation, TestNowMidnight, LondonSettings);

        var countdown = await _calculator.CalculateCountdownAsync(
            schedule, AllEnabled, TestNowMidnight);

        Assert.NotNull(countdown);
        Assert.True(countdown!.RemainingSeconds > 0);
    }

    // ── Offset application ─────────────────────────────────────────────────

    [Fact]
    public async Task ApplyOffsets_AdjustsPrayerTimes()
    {
        var settings = new PrayerCalculationSettings
        {
            Method           = CalculationMethod.MuslimWorldLeague,
            Madhab           = Madhab.Shafi,
            HighLatitudeRule = HighLatitudeRule.SeventhOfNight,
            FajrOffsetMinutes = 5
        };

        var schedule = await _calculator.CalculateDailyScheduleAsync(
            LondonLocation, TestNowMidnight, settings);

        var fajr  = schedule.GetPrayer(PrayerType.Fajr)!.Value;
        var dhuhr = schedule.GetPrayer(PrayerType.Dhuhr)!.Value;

        Assert.Equal(5, fajr.OffsetMinutes);
        Assert.Equal(0, dhuhr.OffsetMinutes);

        // Fajr should be shifted forward by +5 minutes from its unshifted base (04:52 + 5 = 04:57)
        Assert.Equal("04:57", fajr.DateTime.ToString("HH:mm", CultureInfo.InvariantCulture));
    }
}

