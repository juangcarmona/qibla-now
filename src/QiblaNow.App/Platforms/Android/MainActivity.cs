using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Content;
using QiblaNow.App.Platforms.Android;

namespace QiblaNow.App
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        private PrayerAlarmReceiver? _alarmReceiver;

        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Register prayer alarm receiver
            _alarmReceiver = new PrayerAlarmReceiver();
            var alarmFilter = new IntentFilter("com.qiblanow.PRAYER_ALARM");
            RegisterReceiver(_alarmReceiver, alarmFilter);

            // Request POST_NOTIFICATIONS permission for Android 13+
            if (Build.VERSION.SdkInt >= BuildVersionCodes.Tiramisu)
            {
                Permissions.RequestAsync<Permissions.PostNotifications>();
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            // Unregister prayer alarm receiver
            _alarmReceiver?.Dispose();
        }
    }
}
