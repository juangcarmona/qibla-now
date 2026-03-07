using QiblaNow.Core.Services;
using QiblaNow.Core.Abstractions;
using QiblaNow.Presentation.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace QiblaNow.Presentation.DI;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPresentationServices(this IServiceCollection services)
    {
        // Core services (singleton)
        services.AddSingleton<ISettingsStore, SettingsStore>();
        services.AddSingleton<ILocationService, LocationService>();
        services.AddSingleton<IPrayerTimesCalculator, PrayerTimesCalculator>();

        // ViewModels (transient)
        services.AddTransient<HomeViewModel>();
        services.AddTransient<PrayerTimesViewModel>();
        services.AddTransient<SettingsViewModel>();
        services.AddTransient<QiblaViewModel>();
        services.AddTransient<MapViewModel>();

        return services;
    }
}
