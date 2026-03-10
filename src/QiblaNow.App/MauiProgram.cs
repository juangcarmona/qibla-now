using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using QiblaNow.App.Pages;
using QiblaNow.App.Services;
using QiblaNow.Core;
using QiblaNow.Core.Abstractions;
using QiblaNow.Presentation.DI;
using QiblaNow.Presentation.ViewModels;

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
            builder.Services.AddSingleton<IPrayerTimesCalculator, PrayerTimesCalculator>();

            // Register presentation services
            builder.Services.AddPresentationServices();

#if ANDROID
            builder.Services.AddSingleton<INotificationScheduler, Platforms.Android.AndroidNotificationScheduler>();
#endif

            // Pages (transient — resolved by Shell navigation)
            builder.Services.AddTransient<SettingsPage>();
            builder.Services.AddTransient<LocationSettingsPage>();
            builder.Services.AddTransient<CalculationSettingsPage>();
            builder.Services.AddTransient<SoundSettingsPage>();
            builder.Services.AddTransient<DisplaySettingsPage>();
            builder.Services.AddTransient<AboutPage>();

            return builder.Build();
        }
    }
}
