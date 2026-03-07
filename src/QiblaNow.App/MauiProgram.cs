using Microsoft.Extensions.Logging;
using QiblaNow.App.Pages;
using QiblaNow.Presentation.ViewModels;
using QiblaNow.Presentation.DI;
using CommunityToolkit.Maui;
using QiblaNow.Core.Abstractions;
using QiblaNow.Core.Services;

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

            // Android notification scheduler (singleton)
            builder.Services.AddSingleton<INotificationScheduler, AndroidNotificationScheduler>();

            return builder.Build();
        }
    }
}
