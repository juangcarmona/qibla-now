using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using QiblaNow.App.Pages;
using QiblaNow.App.Services;
using QiblaNow.Core;
using QiblaNow.Core.Abstractions;
using QiblaNow.Presentation.DI;
using QiblaNow.Presentation.ViewModels;

namespace QiblaNow.App;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .UseMauiMaps()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                fonts.AddFont("MaterialIcons-Regular.ttf", "MaterialIcons");
            });

#if DEBUG
        builder.Logging.AddDebug();
#endif

        builder.Services.AddSingleton<ISettingsStore, SettingsStore>();
        builder.Services.AddSingleton<ILocationService, LocationService>();
        builder.Services.AddSingleton<IPrayerTimesCalculator, PrayerTimesCalculator>();

        builder.Services.AddPresentationServices();

#if ANDROID
        builder.Services.AddSingleton<INotificationScheduler, Platforms.Android.AndroidNotificationScheduler>();
#endif

        builder.Services.AddTransient<MapViewModel>();
        builder.Services.AddTransient<MapPage>();

        builder.Services.AddTransient<SettingsPage>();
        builder.Services.AddTransient<LocationSettingsPage>();
        builder.Services.AddTransient<CalculationSettingsPage>();
        builder.Services.AddTransient<SoundSettingsPage>();
        builder.Services.AddTransient<DisplaySettingsPage>();
        builder.Services.AddTransient<AboutPage>();

        return builder.Build();
    }
}