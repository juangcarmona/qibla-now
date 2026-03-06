using Microsoft.Extensions.Logging;
using QiblaNow.App.Pages;
using QiblaNow.Presentation.ViewModels;
using QiblaNow.App.Services;
using CommunityToolkit.Maui;
using QiblaNow.Core.Abstractions;

namespace QiblaNow.App
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                // Initialize the .NET MAUI Community Toolkit by adding the below line of code
                .UseMauiCommunityToolkit()
                   // After initializing the .NET MAUI Community Toolkit, optionally add additional fonts
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



            return builder.Build();
        }
    }
}
