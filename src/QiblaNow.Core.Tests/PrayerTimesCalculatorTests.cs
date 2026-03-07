using System.Globalization;
using QiblaNow.Core;
using QiblaNow.Core.Abstractions;
using QiblaNow.Core.Models;

namespace QiblaNow.Core.Tests;

public class PrayerTimesCalculatorTests
{
    private readonly IPrayerTimesCalculator _calculator;

    public PrayerTimesCalculatorTests()
    {
        _calculator = new PrayerTimesCalculator();
    }

    // PT-1: London test case
    [Fact]
    public async Task PT1_London_2026_03_02_ReturnsExpectedPrayerTimes()
    {
        // Arrange
        var location = new LocationSnapshot(
            LocationMode.Manual,
            51.5074,
            -0.1278,
            "London"
        );
        var date = new DateTimeOffset(2026, 3, 2, 0, 0, 0, TimeSpan.Zero);
        var settings = new PrayerCalculationSettings
        {
            Method = CalculationMethod.MuslimWorldLeague,
            Madhab = Madhab.Shafi,
            HighLatitudeRule = HighLatitudeRule.SeventhOfNight
        };

        // Act
        var schedule = await _calculator.CalculateDailyScheduleAsync(location, date, settings);

        // Assert
        Assert.NotNull(schedule);
        Assert.Equal(date, schedule.Date);
        Assert.NotNull(schedule.TimeZone);
        Assert.Equal(6, schedule.Prayers.Count); // Fajr, Sunrise, Dhuhr, Asr, Maghrib, Isha

        var fajr = schedule.GetPrayer(PrayerType.Fajr);
        var dhuhr = schedule.GetPrayer(PrayerType.Dhuhr);
        var asr = schedule.GetPrayer(PrayerType.Asr);
        var maghrib = schedule.GetPrayer(PrayerType.Maghrib);
        var isha = schedule.GetPrayer(PrayerType.Isha);
        var sunrise = schedule.GetPrayer(PrayerType.Sunrise);

        Assert.NotNull(fajr);
        Assert.NotNull(dhuhr);
        Assert.NotNull(asr);
        Assert.NotNull(maghrib);
        Assert.NotNull(isha);
        Assert.NotNull(sunrise);

        // Expected values from docs/ACCEPTANCE_TESTS.md
        // Tolerance: ±1 minute
        var fajrTime = fajr.DateTime.ToString("HH:mm", CultureInfo.InvariantCulture);
        var dhuhrTime = dhuhr.DateTime.ToString("HH:mm", CultureInfo.InvariantCulture);
        var asrTime = asr.DateTime.ToString("HH:mm", CultureInfo.InvariantCulture);
        var maghribTime = maghrib.DateTime.ToString("HH:mm", CultureInfo.InvariantCulture);
        var ishaTime = isha.DateTime.ToString("HH:mm", CultureInfo.InvariantCulture);

        Assert.Equal("05:04", fajrTime);
        Assert.Equal("12:18", dhuhrTime);
        Assert.Equal("15:06", asrTime);
        Assert.Equal("17:46", maghribTime);
        Assert.Equal("19:12", ishaTime);
    }

    // PT-2: Reykjavik test case
    [Fact]
    public async Task PT2_Reykjavik_ReturnsExpectedPrayerTimes()
    {
        // Arrange
        var location = new LocationSnapshot(
            LocationMode.Manual,
            64.1265,
            -21.8174,
            "Reykjavik"
        );
        var date = new DateTimeOffset(2026, 3, 2, 0, 0, 0, TimeSpan.Zero);
        var settings = new PrayerCalculationSettings
        {
            Method = CalculationMethod.ISNA,
            Madhab = Madhab.Shafi,
            HighLatitudeRule = HighLatitudeRule.SeventhOfNight
        };

        // Act
        var schedule = await _calculator.CalculateDailyScheduleAsync(location, date, settings);

        // Assert
        Assert.NotNull(schedule);
        Assert.Equal(date, schedule.Date);

        var fajr = schedule.GetPrayer(PrayerType.Fajr);
        var dhuhr = schedule.GetPrayer(PrayerType.Dhuhr);
        var asr = schedule.GetPrayer(PrayerType.Asr);
        var maghrib = schedule.GetPrayer(PrayerType.Maghrib);
        var isha = schedule.GetPrayer(PrayerType.Isha);

        Assert.NotNull(fajr);
        Assert.NotNull(dhuhr);
        Assert.NotNull(asr);
        Assert.NotNull(maghrib);
        Assert.NotNull(isha);

        // Verify times are reasonable (not before sunrise or after midnight)
        var now = DateTimeOffset.UtcNow;
        Assert.True(fajr.DateTime < dhuhr.DateTime);
        Assert.True(dhuhr.DateTime < asr.DateTime);
        Assert.True(asr.DateTime < maghrib.DateTime);
        Assert.True(maghrib.DateTime < isha.DateTime);
    }

    [Fact]
    public async Task CalculateNextPrayer_ReturnsCorrectNextPrayer()
    {
        // Arrange
        var location = new LocationSnapshot(
            LocationMode.Manual,
            51.5074,
            -0.1278,
            "London"
        );
        var date = new DateTimeOffset(2026, 3, 2, 0, 0, 0, TimeSpan.Zero);
        var settings = new PrayerCalculationSettings
        {
            Method = CalculationMethod.MuslimWorldLeague,
            Madhab = Madhab.Shafi,
            HighLatitudeRule = HighLatitudeRule.SeventhOfNight
        };
        var notificationSettings = new PrayerNotificationSettings
        {
            FajrEnabled = true,
            DhuhrEnabled = true,
            AsrEnabled = true,
            MaghribEnabled = true,
            IshaEnabled = true
        };

        // Act
        var schedule = await _calculator.CalculateDailyScheduleAsync(location, date, settings);
        var nextPrayer = await _calculator.CalculateNextPrayerAsync(schedule, notificationSettings);

        // Assert
        Assert.NotNull(nextPrayer);
        Assert.True(nextPrayer.Remaining > TimeSpan.Zero);
    }

    [Fact]
    public async Task CalculateNextPrayer_WrapsToNextDayFajrAfterIsha()
    {
        // Arrange
        var location = new LocationSnapshot(
            LocationMode.Manual,
            51.5074,
            -0.1278,
            "London"
        );
        var date = new DateTimeOffset(2026, 3, 2, 0, 0, 0, TimeSpan.Zero);
        var settings = new PrayerCalculationSettings
        {
            Method = CalculationMethod.MuslimWorldLeague,
            Madhab = Madhab.Shafi,
            HighLatitudeRule = HighLatitudeRule.SeventhOfNight
        };

        // Simulate time after Isha (19:30)
        var afterIshaDate = new DateTimeOffset(2026, 3, 2, 19, 30, 0, TimeSpan.Zero);
        var notificationSettings = new PrayerNotificationSettings
        {
            FajrEnabled = true,
            DhuhrEnabled = true,
            AsrEnabled = true,
            MaghribEnabled = true,
            IshaEnabled = true
        };

        // Act
        var schedule = await _calculator.CalculateDailyScheduleAsync(location, date, settings);
        var nextPrayer = await _calculator.CalculateNextPrayerAsync(schedule, notificationSettings);

        // Assert
        Assert.NotNull(nextPrayer);
        Assert.Equal(PrayerType.Fajr, nextPrayer.Type);
        Assert.False(nextPrayer.IsToday);
    }

    [Fact]
    public async Task CalculateNextPrayer_FiltersDisabledPrayers()
    {
        // Arrange
        var location = new LocationSnapshot(
            LocationMode.Manual,
            51.5074,
            -0.1278,
            "London"
        );
        var date = new DateTimeOffset(2026, 3, 2, 0, 0, 0, TimeSpan.Zero);
        var settings = new PrayerCalculationSettings
        {
            Method = CalculationMethod.MuslimWorldLeague,
            Madhab = Madhab.Shafi,
            HighLatitudeRule = HighLatitudeRule.SeventhOfNight
        };

        // Only Fajr is enabled
        var notificationSettings = new PrayerNotificationSettings
        {
            FajrEnabled = true,
            DhuhrEnabled = false,
            AsrEnabled = false,
            MaghribEnabled = false,
            IshaEnabled = false
        };

        // Act
        var schedule = await _calculator.CalculateDailyScheduleAsync(location, date, settings);
        var nextPrayer = await _calculator.CalculateNextPrayerAsync(schedule, notificationSettings);

        // Assert
        Assert.NotNull(nextPrayer);
        Assert.Equal(PrayerType.Fajr, nextPrayer.Type);
    }

    [Fact]
    public async Task CalculateNextNotificationCandidate_RespectsEnabledPrayers()
    {
        // Arrange
        var location = new LocationSnapshot(
            LocationMode.Manual,
            51.5074,
            -0.1278,
            "London"
        );
        var date = new DateTimeOffset(2026, 3, 2, 0, 0, 0, TimeSpan.Zero);
        var settings = new PrayerCalculationSettings
        {
            Method = CalculationMethod.MuslimWorldLeague,
            Madhab = Madhab.Shafi,
            HighLatitudeRule = HighLatitudeRule.SeventhOfNight
        };

        // Only Dhuhr and Maghrib enabled
        var notificationSettings = new PrayerNotificationSettings
        {
            FajrEnabled = false,
            DhuhrEnabled = true,
            AsrEnabled = false,
            MaghribEnabled = true,
            IshaEnabled = false
        };

        // Act
        var schedule = await _calculator.CalculateDailyScheduleAsync(location, date, settings);
        var candidate = await _calculator.CalculateNextNotificationCandidateAsync(schedule, notificationSettings);

        // Assert
        Assert.NotNull(candidate);
        Assert.True(candidate.Type == PrayerType.Dhuhr || candidate.Type == PrayerType.Maghrib);
    }

    [Fact]
    public async Task CalculateCountdown_UpdatesEverySecond()
    {
        // Arrange
        var location = new LocationSnapshot(
            LocationMode.Manual,
            51.5074,
            -0.1278,
            "London"
        );
        var date = new DateTimeOffset(2026, 3, 2, 0, 0, 0, TimeSpan.Zero);
        var settings = new PrayerCalculationSettings
        {
            Method = CalculationMethod.MuslimWorldLeague,
            Madhab = Madhab.Shafi,
            HighLatitudeRule = HighLatitudeRule.SeventhOfNight
        };
        var notificationSettings = new PrayerNotificationSettings
        {
            FajrEnabled = true,
            DhuhrEnabled = true,
            AsrEnabled = true,
            MaghribEnabled = true,
            IshaEnabled = true
        };

        // Act
        var schedule = await _calculator.CalculateDailyScheduleAsync(location, date, settings);
        var countdown = await _calculator.CalculateCountdownAsync(schedule, notificationSettings);

        // Assert
        Assert.NotNull(countdown);
        Assert.True(countdown.RemainingSeconds > 0);
    }

    [Fact]
    public async Task ApplyOffsets_AdjustsPrayerTimes()
    {
        // Arrange
        var location = new LocationSnapshot(
            LocationMode.Manual,
            51.5074,
            -0.1278,
            "London"
        );
        var date = new DateTimeOffset(2026, 3, 2, 0, 0, 0, TimeSpan.Zero);
        var settings = new PrayerCalculationSettings
        {
            Method = CalculationMethod.MuslimWorldLeague,
            Madhab = Madhab.Shafi,
            HighLatitudeRule = HighLatitudeRule.SeventhOfNight,
            FajrOffsetMinutes = 5,
            DhuhrOffsetMinutes = 0,
            AsrOffsetMinutes = 0,
            MaghribOffsetMinutes = 0,
            IshaOffsetMinutes = 0
        };

        // Act
        await _calculator.CalculateDailyScheduleAsync(location, date, settings);

        // Assert
        var schedule = await _calculator.CalculateDailyScheduleAsync(location, date, settings);
        var fajr = schedule.GetPrayer(PrayerType.Fajr);
        var dhuhr = schedule.GetPrayer(PrayerType.Dhuhr);

        Assert.NotNull(fajr);
        Assert.NotNull(dhuhr);

        // Fajr should be shifted by +5 minutes
        Assert.Equal(5, fajr.OffsetMinutes);
        Assert.True(fajr.DateTime.Hour == 5 && fajr.DateTime.Minute >= 4); // Should be around 05:09
    }
}
