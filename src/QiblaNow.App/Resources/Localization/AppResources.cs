using System.Resources;
using System.Runtime.CompilerServices;

namespace QiblaNow.App.Resources.Localization;

/// <summary>
/// Strongly-typed static accessor for AppResources.resx.
/// Each property reads from the ResourceManager using CurrentUICulture at call time,
/// which means pages inflated after a culture change will get the new language strings.
/// </summary>
public static class AppResources
{
    private static readonly ResourceManager _rm =
        new("QiblaNow.App.Resources.Localization.AppResources", typeof(AppResources).Assembly);

    private static string Get([CallerMemberName] string? key = null)
        => _rm.GetString(key!) ?? key!;

    // App-wide
    public static string AppTitle => Get();

    // Home Page
    public static string HomePage_TapToGoBackToToday => Get();
    public static string HomePage_CalculationMethodPrefix => Get();
    public static string HomePage_NavQibla => Get();
    public static string HomePage_NavMap => Get();
    public static string HomePage_NavAlarms => Get();

    // Settings Page
    public static string SettingsPage_Title => Get();
    public static string SettingsPage_Location => Get();
    public static string SettingsPage_Display => Get();
    public static string SettingsPage_DisplaySummary => Get();
    public static string SettingsPage_Calculation => Get();
    public static string SettingsPage_SoundNotifications => Get();
    public static string SettingsPage_Language => Get();
    public static string SettingsPage_LanguageSummary => Get();
    public static string SettingsPage_About => Get();
    public static string SettingsPage_AboutSummary => Get();

    // Location Settings
    public static string LocationSettings_Title => Get();
    public static string LocationSettings_LocationMode => Get();
    public static string LocationSettings_SelectMode => Get();
    public static string LocationSettings_GpsDescription => Get();
    public static string LocationSettings_RequestGps => Get();
    public static string LocationSettings_Latitude => Get();
    public static string LocationSettings_Longitude => Get();
    public static string LocationSettings_SaveLocation => Get();

    // Calculation Settings
    public static string CalculationSettings_Title => Get();
    public static string CalculationSettings_Header => Get();
    public static string CalculationSettings_Method => Get();
    public static string CalculationSettings_SelectMethod => Get();
    public static string CalculationSettings_Madhab => Get();
    public static string CalculationSettings_SelectMadhab => Get();
    public static string CalculationSettings_HighLatitudeRule => Get();
    public static string CalculationSettings_SelectRule => Get();
    public static string CalculationSettings_AutoSave => Get();

    // Sound Settings
    public static string SoundSettings_Title => Get();
    public static string SoundSettings_PrayerNotifications => Get();
    public static string SoundSettings_NotificationsDescription => Get();
    public static string SoundSettings_Adhan => Get();
    public static string SoundSettings_AdhanDescription => Get();
    public static string SoundSettings_AdhanDefault => Get();
    public static string SoundSettings_Adhan1 => Get();
    public static string SoundSettings_Adhan2 => Get();
    public static string SoundSettings_Adhan3 => Get();
    public static string SoundSettings_Preview => Get();

    // Prayer Names
    public static string Prayer_Fajr => Get();
    public static string Prayer_Dhuhr => Get();
    public static string Prayer_Asr => Get();
    public static string Prayer_Maghrib => Get();
    public static string Prayer_Isha => Get();

    // Display Settings
    public static string DisplaySettings_Title => Get();
    public static string DisplaySettings_TimeFormat => Get();
    public static string DisplaySettings_24HourClock => Get();
    public static string DisplaySettings_TimeFormatComingSoon => Get();
    public static string DisplaySettings_PrayerVisibility => Get();
    public static string DisplaySettings_PrayerVisibilityComingSoon => Get();
    public static string DisplaySettings_HijriAdjustment => Get();
    public static string DisplaySettings_HijriAdjustmentComingSoon => Get();

    // About Page
    public static string AboutPage_Title => Get();
    public static string AboutPage_Version => Get();
    public static string AboutPage_Description => Get();
    public static string AboutPage_Features => Get();
    public static string AboutPage_FeaturePrayerTimes => Get();
    public static string AboutPage_FeatureQibla => Get();
    public static string AboutPage_FeatureNotifications => Get();
    public static string AboutPage_FeatureCalculationMethods => Get();
    public static string AboutPage_FeatureOffline => Get();
    public static string AboutPage_Links => Get();
    public static string AboutPage_SourceCode => Get();
    public static string AboutPage_Website => Get();
    public static string AboutPage_Support => Get();
    public static string AboutPage_SupportDescription => Get();
    public static string AboutPage_SupportButton => Get();

    // Map Page
    public static string MapPage_Title => Get();
    public static string MapPage_QiblaColumn => Get();
    public static string MapPage_HeadingColumn => Get();
    public static string MapPage_ErrorColumn => Get();

    // Qibla Page
    public static string QiblaPage_Title => Get();
    public static string QiblaPage_QiblaColumn => Get();
    public static string QiblaPage_HeadingColumn => Get();
    public static string QiblaPage_ErrorColumn => Get();

    // Prayer Times Page
    public static string PrayerTimesPage_Title => Get();

    // Language Settings
    public static string LanguageSettings_Title => Get();
    public static string LanguageSettings_AppLanguage => Get();
    public static string LanguageSettings_Description => Get();
    public static string LanguageSettings_RestartNote => Get();
    public static string LanguageSettings_SystemDefault => Get();
    public static string LanguageSettings_English => Get();
    public static string LanguageSettings_Arabic => Get();
    public static string LanguageSettings_Spanish => Get();
}
