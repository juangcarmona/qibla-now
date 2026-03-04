using Microsoft.Extensions.Logging;
using QiblaNow.App.Pages;
using QiblaNow.App.ViewModels;

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
                });

#if DEBUG
    		builder.Logging.AddDebug();
#endif
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
