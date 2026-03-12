using Android.App;
using Android.Content.PM;
using Android.Content;
using Android.OS;
using Android.Provider;
using Plugin.MauiMtAdmob;

namespace QiblaNow.App
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        protected override async void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            if (Build.VERSION.SdkInt >= BuildVersionCodes.Tiramisu)
            {
                var status = await Permissions.CheckStatusAsync<Permissions.PostNotifications>();
                if (status != PermissionStatus.Granted)
                    await Permissions.RequestAsync<Permissions.PostNotifications>();
            }

            EnsureExactAlarmAccess();

            var appId = PackageManager?
                .GetApplicationInfo(PackageName!, PackageInfoFlags.MetaData)?
                .MetaData?
                .GetString("com.google.android.gms.ads.APPLICATION_ID");
            
            CrossMauiMTAdmob.Current.Init(
                this,
                appId ?? string.Empty,
#if DEBUG
                debugMode: true
#else
                        debugMode: false
#endif
            );
        }



        private void EnsureExactAlarmAccess()
        {
            if (!OperatingSystem.IsAndroidVersionAtLeast(31))
                return;

            var alarmManager = GetSystemService(AlarmService) as AlarmManager;
            if (alarmManager == null)
                return;

            if (alarmManager.CanScheduleExactAlarms())
                return;

            var intent = new Intent(Settings.ActionRequestScheduleExactAlarm);
            intent.SetData(global::Android.Net.Uri.Parse($"package:{PackageName}"));
            StartActivity(intent);
        }
    }
}
