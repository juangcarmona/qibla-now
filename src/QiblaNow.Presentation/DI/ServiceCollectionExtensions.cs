using QiblaNow.Presentation.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace QiblaNow.Presentation.DI;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers Presentation ViewModels only.
    /// Core services (ISettingsStore, ILocationService, IPrayerTimesCalculator) must be registered
    /// in the composition root (MauiProgram.cs), not here.
    /// </summary>
    public static IServiceCollection AddPresentationServices(this IServiceCollection services)
    {
        // ViewModels (transient)
        services.AddTransient<HomeViewModel>();
        services.AddTransient<PrayerTimesViewModel>();
        services.AddTransient<SettingsViewModel>();
        services.AddTransient<QiblaViewModel>();
        services.AddTransient<MapViewModel>();
        services.AddTransient<PrayerAlertViewModel>();

        return services;
    }
}
