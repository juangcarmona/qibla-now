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
        builder.Services.AddSingleton<ISavedLocationStore>(sp => (ISavedLocationStore)sp.GetRequiredService<ISettingsStore>());
        builder.Services.AddSingleton<IReverseGeocodingService, GoogleReverseGeocodingService>();
        builder.Services.AddSingleton<ILocationService, LocationService>();
        builder.Services.AddSingleton<IPrayerTimesCalculator, PrayerTimesCalculator>();

        builder.Services.AddPresentationServices();

#if ANDROID
        builder.Services.AddSingleton<Android.Content.Context>(Android.App.Application.Context);
        builder.Services.AddSingleton<INotificationScheduler, Platforms.Android.AndroidNotificationScheduler>();
        builder.Services.AddSingleton<IAdhanPlayer, Platforms.Android.AndroidAdhanPlayer>();
        builder.Services.AddSingleton<INotificationSettingsOpener, Platforms.Android.AndroidNotificationSettingsOpener>();
#elif IOS || MACCATALYST
        builder.Services.AddSingleton<INotificationScheduler, QiblaNow.Core.Abstractions.NullNotificationScheduler>();
        builder.Services.AddSingleton<IAdhanPlayer, Platforms.iOS.iOSAdhanPlayer>();
        builder.Services.AddSingleton<INotificationSettingsOpener, QiblaNow.Core.Abstractions.NullNotificationSettingsOpener>();
#else
        builder.Services.AddSingleton<INotificationScheduler, QiblaNow.Core.Abstractions.NullNotificationScheduler>();
        builder.Services.AddSingleton<IAdhanPlayer, QiblaNow.Core.Abstractions.NullAdhanPlayer>();
        builder.Services.AddSingleton<INotificationSettingsOpener, QiblaNow.Core.Abstractions.NullNotificationSettingsOpener>();
#endif

        builder.Services.AddTransient<MapViewModel>();
        builder.Services.AddTransient<MapPage>();
        builder.Services.AddTransient<PrayerAlertPage>();

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
