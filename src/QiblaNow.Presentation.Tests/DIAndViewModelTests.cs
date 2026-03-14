using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using QiblaNow.Core.Abstractions;
using QiblaNow.Core.Models;
using QiblaNow.Presentation.ViewModels;

namespace QiblaNow.Presentation.Tests;

public class DIAndViewModelTests
{
    // Minimal stubs so ViewModels can be resolved in the test DI container
    // without pulling in platform-specific implementations (MAUI Preferences, etc.).

    private sealed class StubCalculator : IPrayerTimesCalculator
    {
        public Task<DailyPrayerSchedule> CalculateDailyScheduleAsync(LocationSnapshot l, DateTimeOffset d, PrayerCalculationSettings s)
            => Task.FromResult(new DailyPrayerSchedule(d, TimeZoneInfo.Utc));

        public Task<NextPrayerResult?> CalculateNextPrayerAsync(DailyPrayerSchedule s, PrayerNotificationSettings n, DateTimeOffset now)
            => Task.FromResult<NextPrayerResult?>(null);

        public Task<NextNotificationCandidateResult?> CalculateNextNotificationCandidateAsync(DailyPrayerSchedule s, PrayerNotificationSettings n, DateTimeOffset now)
            => Task.FromResult<NextNotificationCandidateResult?>(null);

        public Task<CountdownTargetResult?> CalculateCountdownAsync(DailyPrayerSchedule s, PrayerNotificationSettings n, DateTimeOffset now)
            => Task.FromResult<CountdownTargetResult?>(null);

        public PrayerTime? FindNextPrayerInSchedule(DailyPrayerSchedule schedule, IReadOnlySet<PrayerType> enabled, DateTimeOffset now)
            => null;

        public PrayerTime? FindFirstEnabledPrayer(DailyPrayerSchedule schedule, IReadOnlySet<PrayerType> enabled)
            => null;
    }

    private sealed class StubSettingsStore : ISettingsStore
    {
        public LocationMode GetLocationMode() => LocationMode.GPS;
        public void SetLocationMode(LocationMode mode) { }
        public LocationSnapshot? GetLastSnapshot() => null;
        public void SaveSnapshot(LocationSnapshot snapshot) { }
        public PrayerCalculationSettings GetCalculationSettings() => new();
        public void SaveCalculationSettings(PrayerCalculationSettings settings) { }
        public PrayerNotificationSettings GetNotificationSettings() => new();
        public void SaveNotificationSettings(PrayerNotificationSettings settings) { }
        public LocationSnapshot? GetLastValidLocation() => null;
        public void SaveLastValidLocation(LocationSnapshot location) { }
        public SchedulingState GetSchedulingState() => new();
        public void SaveSchedulingState(SchedulingState state) { }
    }

    private sealed class StubLocationService : ILocationService
    {
        public Task<LocationSnapshot?> GetCurrentLocationAsync() => Task.FromResult<LocationSnapshot?>(null);
        public Task<LocationSnapshot?> RequestGpsLocationAsync() => Task.FromResult<LocationSnapshot?>(null);
        public Task<LocationSnapshot?> TryGetGpsLocationAsync(TimeSpan timeout) => Task.FromResult<LocationSnapshot?>(null);
    }

    private static IServiceProvider BuildServices()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IPrayerTimesCalculator, StubCalculator>();
        services.AddSingleton<ISettingsStore, StubSettingsStore>();
        services.AddSingleton<ILocationService, StubLocationService>();
        services.AddSingleton<INotificationScheduler, NullNotificationScheduler>();
        services.AddSingleton<IAdhanPlayer, NullAdhanPlayer>();
        services.AddTransient<PrayerTimesViewModel>();
        services.AddTransient<SettingsViewModel>();
        services.AddTransient<QiblaViewModel>();
        services.AddTransient<MapViewModel>();
        return services.BuildServiceProvider();
    }

    [Fact]
    public void DI_Registers_ViewModels()
    {
        var sp = BuildServices();

        var timesViewModel    = sp.GetRequiredService<PrayerTimesViewModel>();
        var settingsViewModel = sp.GetRequiredService<SettingsViewModel>();
        var qiblaViewModel    = sp.GetRequiredService<QiblaViewModel>();
        var mapViewModel      = sp.GetRequiredService<MapViewModel>();

        Assert.NotNull(timesViewModel);
        Assert.NotNull(settingsViewModel);
        Assert.NotNull(qiblaViewModel);
        Assert.NotNull(mapViewModel);
    }

    [Fact]
    public void ViewModels_Inherit_From_ObservableObject()
    {
        var sp = BuildServices();

        var prayerTimesViewModel = sp.GetRequiredService<PrayerTimesViewModel>();
        var settingsViewModel    = sp.GetRequiredService<SettingsViewModel>();
        var qiblaViewModel       = sp.GetRequiredService<QiblaViewModel>();
        var mapViewModel         = sp.GetRequiredService<MapViewModel>();

        Assert.IsAssignableFrom<ObservableObject>(prayerTimesViewModel);
        Assert.IsAssignableFrom<ObservableObject>(settingsViewModel);
        Assert.IsAssignableFrom<ObservableObject>(qiblaViewModel);
        Assert.IsAssignableFrom<ObservableObject>(mapViewModel);
    }

    [Fact]
    public void ViewModels_Resolved_Via_DI()
    {
        var sp = BuildServices();

        var timesViewModel  = sp.GetRequiredService<PrayerTimesViewModel>();
        var timesViewModel2 = sp.GetRequiredService<PrayerTimesViewModel>();

        // Transient → different instances each resolution
        Assert.NotNull(timesViewModel);
        Assert.NotNull(timesViewModel2);
        Assert.NotSame(timesViewModel, timesViewModel2);
    }
}

