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
        }
    }
}
