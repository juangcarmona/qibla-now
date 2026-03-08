using System.Globalization;
using QiblaNow.Core;
using QiblaNow.Core.Abstractions;
using QiblaNow.Core.Models;

namespace QiblaNow.Core.Tests;

/// <summary>
/// Behavioral and invariant tests for the prayer-times calculator.
///
/// These tests verify structural guarantees:
///   – schedule shape and Sunrise inclusion
///   – chronological ordering for all latitudes
///   – next-prayer resolution (before prayers, after last prayer, disabled filtering)
///   – offset/tuning application
///   – countdown positivity
///
/// For exact-value oracle assertions against the PrayTimes reference, see
/// OraclePrayerTimesTests.cs.
/// </summary>
public class PrayerTimesCalculatorTests
{
    private readonly IPrayerTimesCalculator _calculator;

    // Fixed "now" anchored before all prayers on the test date (UTC midnight).
    private static readonly DateTimeOffset TestNowMidnight =
        new(2026, 3, 2, 0, 0, 0, TimeSpan.Zero);

    private static readonly LocationSnapshot LondonLocation = new(
        LocationMode.Manual, 51.5074, -0.1278, "London");

    private static readonly LocationSnapshot ReykjavikLocation = new(
        LocationMode.Manual, 64.1265, -21.8174, "Reykjavik");

    private static readonly PrayerCalculationSettings LondonSettings = new()
    {
        Method           = CalculationMethod.MuslimWorldLeague,
        Madhab           = Madhab.Shafi,
        HighLatitudeRule = HighLatitudeRule.SeventhOfNight
    };

    private static readonly PrayerCalculationSettings ReykjavikSettings = new()
    {
        Method           = CalculationMethod.ISNA,
        Madhab           = Madhab.Shafi,
        HighLatitudeRule = HighLatitudeRule.SeventhOfNight
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

    // ── Schedule shape ─────────────────────────────────────────────────────

    [Fact]
    public async Task Schedule_AlwaysContainsSixPrayers_IncludingSunrise()
    {
        var schedule = await _calculator.CalculateDailyScheduleAsync(
            LondonLocation, TestNowMidnight, LondonSettings);

        Assert.NotNull(schedule);
        Assert.Equal(6, schedule.Prayers.Count);

        // All six types must be present
        Assert.NotNull(schedule.GetPrayer(PrayerType.Fajr));
        Assert.NotNull(schedule.GetPrayer(PrayerType.Sunrise));
        Assert.NotNull(schedule.GetPrayer(PrayerType.Dhuhr));
        Assert.NotNull(schedule.GetPrayer(PrayerType.Asr));
        Assert.NotNull(schedule.GetPrayer(PrayerType.Maghrib));
        Assert.NotNull(schedule.GetPrayer(PrayerType.Isha));
    }

    [Fact]
    public async Task Schedule_DateMatchesInputDate()
    {
        var schedule = await _calculator.CalculateDailyScheduleAsync(
            LondonLocation, TestNowMidnight, LondonSettings);

        Assert.Equal(TestNowMidnight, schedule.Date);
    }

    // ── Sunrise ordering and presence ──────────────────────────────────────

    [Fact]
    public async Task Sunrise_IsAfterFajrAndBeforeDhuhr_London()
    {
        var schedule = await _calculator.CalculateDailyScheduleAsync(
            LondonLocation, TestNowMidnight, LondonSettings);

        var fajr    = schedule.GetPrayer(PrayerType.Fajr)!.Value;
        var sunrise = schedule.GetPrayer(PrayerType.Sunrise)!.Value;
        var dhuhr   = schedule.GetPrayer(PrayerType.Dhuhr)!.Value;

        Assert.True(fajr.DateTime    < sunrise.DateTime, "Sunrise must follow Fajr");
        Assert.True(sunrise.DateTime < dhuhr.DateTime,   "Sunrise must precede Dhuhr");
    }

    [Fact]
    public async Task Sunrise_IsAfterFajrAndBeforeDhuhr_Reykjavik()
    {
        var schedule = await _calculator.CalculateDailyScheduleAsync(
            ReykjavikLocation, TestNowMidnight, ReykjavikSettings);

        var fajr    = schedule.GetPrayer(PrayerType.Fajr)!.Value;
        var sunrise = schedule.GetPrayer(PrayerType.Sunrise)!.Value;
        var dhuhr   = schedule.GetPrayer(PrayerType.Dhuhr)!.Value;

        Assert.True(fajr.DateTime    < sunrise.DateTime, "Sunrise must follow Fajr");
        Assert.True(sunrise.DateTime < dhuhr.DateTime,   "Sunrise must precede Dhuhr");
    }

    // ── Chronological ordering ─────────────────────────────────────────────

    [Fact]
    public async Task London_PrayerTimesAreChronologicallyOrdered()
    {
        var schedule = await _calculator.CalculateDailyScheduleAsync(
            LondonLocation, TestNowMidnight, LondonSettings);

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

    [Fact]
    public async Task Reykjavik_PrayerTimesAreChronologicallyOrdered()
    {
        var schedule = await _calculator.CalculateDailyScheduleAsync(
            ReykjavikLocation, TestNowMidnight, ReykjavikSettings);

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

        // 00:00 UTC is before Fajr (~04:53 UTC) → next prayer is Fajr
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

        // After Isha (19:27 UTC) → safely past all prayers
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
            Method            = CalculationMethod.MuslimWorldLeague,
            Madhab            = Madhab.Shafi,
            HighLatitudeRule  = HighLatitudeRule.SeventhOfNight,
            FajrOffsetMinutes = 5
        };

        var schedule = await _calculator.CalculateDailyScheduleAsync(
            LondonLocation, TestNowMidnight, settings);

        var fajr  = schedule.GetPrayer(PrayerType.Fajr)!.Value;
        var dhuhr = schedule.GetPrayer(PrayerType.Dhuhr)!.Value;

        Assert.Equal(5, fajr.OffsetMinutes);
        Assert.Equal(0, dhuhr.OffsetMinutes);

        // Fajr base is 04:53 (reference value); +5 min offset → 04:58
        Assert.Equal("04:58", fajr.DateTime.ToString("HH:mm", CultureInfo.InvariantCulture));
    }

    [Fact]
    public async Task ApplyOffsets_OnlyAffectsTargetedPrayer()
    {
        var baseSettings = new PrayerCalculationSettings
        {
            Method           = CalculationMethod.MuslimWorldLeague,
            Madhab           = Madhab.Shafi,
            HighLatitudeRule = HighLatitudeRule.SeventhOfNight
        };

        var withOffset = new PrayerCalculationSettings
        {
            Method           = CalculationMethod.MuslimWorldLeague,
            Madhab           = Madhab.Shafi,
            HighLatitudeRule = HighLatitudeRule.SeventhOfNight,
            AsrOffsetMinutes = 10
        };

        var baseSchedule   = await _calculator.CalculateDailyScheduleAsync(LondonLocation, TestNowMidnight, baseSettings);
        var offsetSchedule = await _calculator.CalculateDailyScheduleAsync(LondonLocation, TestNowMidnight, withOffset);

        var baseFajr = baseSchedule.GetPrayer(PrayerType.Fajr)!.Value;
        var baseAsr  = baseSchedule.GetPrayer(PrayerType.Asr)!.Value;
        var offFajr  = offsetSchedule.GetPrayer(PrayerType.Fajr)!.Value;
        var offAsr   = offsetSchedule.GetPrayer(PrayerType.Asr)!.Value;

        // Fajr must be unaffected
        Assert.Equal(baseFajr.DateTime, offFajr.DateTime);

        // Asr must be shifted by exactly 10 minutes
        Assert.Equal(TimeSpan.FromMinutes(10), offAsr.DateTime - baseAsr.DateTime);
    }
}
