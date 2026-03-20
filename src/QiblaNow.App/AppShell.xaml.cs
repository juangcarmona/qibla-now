using QiblaNow.App.Pages;

namespace QiblaNow.App
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute("qibla", typeof(QiblaPage));
            Routing.RegisterRoute("times", typeof(PrayerTimesPage));
            Routing.RegisterRoute("settings", typeof(SettingsPage));
            Routing.RegisterRoute("map", typeof(MapPage));
            Routing.RegisterRoute("location-settings", typeof(LocationSettingsPage));
            Routing.RegisterRoute("calculation-settings", typeof(CalculationSettingsPage));
            Routing.RegisterRoute("sound-settings", typeof(SoundSettingsPage));
            Routing.RegisterRoute("display-settings", typeof(DisplaySettingsPage));
            Routing.RegisterRoute("language-settings", typeof(LanguageSettingsPage));
            Routing.RegisterRoute("about", typeof(AboutPage));
            Routing.RegisterRoute("prayer-alert", typeof(PrayerAlertPage));
        }
    }
}
