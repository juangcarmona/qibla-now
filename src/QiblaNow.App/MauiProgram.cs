using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using Plugin.MauiMtAdmob;
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
        // Apply persisted language before any XAML is inflated
        LocalizationHelper.ApplyPersistedCulture();

        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .UseMauiMaps()
            .UseMauiMTAdmob()
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
        builder.Services.AddSingleton<Android.Content.Context>(Android.App.Application.Context);
        builder.Services.AddSingleton<INotificationScheduler, Platforms.Android.AndroidNotificationScheduler>();
#else
        builder.Services.AddSingleton<INotificationScheduler, QiblaNow.Core.Abstractions.NullNotificationScheduler>();
#endif

        builder.Services.AddTransient<MapViewModel>();
        builder.Services.AddTransient<MapPage>();

        builder.Services.AddTransient<QiblaPage>();

        builder.Services.AddTransient<SettingsPage>();
        builder.Services.AddTransient<LanguageSettingsPage>();
        builder.Services.AddTransient<LocationSettingsPage>();
        builder.Services.AddTransient<CalculationSettingsPage>();
        builder.Services.AddTransient<SoundSettingsPage>();
        builder.Services.AddTransient<DisplaySettingsPage>();
        builder.Services.AddTransient<AboutPage>();

        return builder.Build();
    }
}