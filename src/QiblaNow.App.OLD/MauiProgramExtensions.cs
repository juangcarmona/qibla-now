using Microsoft.Extensions.Logging;
using QiblaNow.App.Pages;
using QiblaNow.App.Services;
using QiblaNow.Presentation.ViewModels;
using QiblaNow.Core.Abstractions;
using QiblaNow.Core.Services;

namespace QiblaNow.App
{
    public static class MauiProgramExtensions
    {
        public static MauiAppBuilder UseSharedMauiApp(this MauiAppBuilder builder)
        {
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("MaterialIcons-Regular.ttf", "MaterialIcons");
                });

#if DEBUG
		builder.Logging.AddDebug();
#endif

            // Core services (singleton)
            builder.Services.AddSingleton<IPreferencesService, PreferencesService>();
            builder.Services.AddSingleton<ISettingsStore, SettingsStore>();
            builder.Services.AddSingleton<ILocationService, LocationService>();

            // ViewModels
            builder.Services.AddTransient<HomeViewModel>();
            builder.Services.AddTransient<HomePage>();

            builder.Services.AddTransient<PrayerTimesViewModel>();
            builder.Services.AddTransient<PrayerTimesPage>();

            builder.Services.AddTransient<QiblaViewModel>();
            builder.Services.AddTransient<QiblaPage>();

            builder.Services.AddTransient<MapViewModel>();
            builder.Services.AddTransient<MapPage>();

            builder.Services.AddTransient<SettingsViewModel>();
            builder.Services.AddTransient<SettingsPage>();



            return builder;
        }
    }
}
