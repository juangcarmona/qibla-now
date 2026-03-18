using Android.App;
using Android.Content.PM;
using Android.Content;
using Android.OS;
using Android.Provider;
using Plugin.MauiMtAdmob;
using QiblaNow.App.Platforms.Android;
using QiblaNow.Core.Models;

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

            // Handle prayer alert from notification tap (cold start).
            HandlePrayerAlertIntent(Intent);
        }

        protected override void OnNewIntent(Intent? intent)
        {
            base.OnNewIntent(intent);

            // Handle prayer alert from notification tap (warm start / app already running).
            HandlePrayerAlertIntent(intent);
        }

        private static void HandlePrayerAlertIntent(Intent? intent)
        {
            if (intent?.GetBooleanExtra(PrayerNavigationRequest.ExtraPrayerAlert, false) != true)
                return;

            var prayerTypeValue = intent.GetIntExtra(PrayerNavigationRequest.ExtraPrayerType, -1);
            if (prayerTypeValue < 0 || !Enum.IsDefined(typeof(PrayerType), prayerTypeValue))
                return;

            var prayerType = (PrayerType)prayerTypeValue;
            System.Diagnostics.Debug.WriteLine(
                $"MainActivity.HandlePrayerAlertIntent: navigating to prayer-alert for {prayerType}");

            // Ensure Shell is ready before navigating.
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                try
                {
                    if (Shell.Current != null)
                    {
                        PrayerNavigationRequest.TakeAndClear(); // clear any background-set pending
                        await Shell.Current.GoToAsync(
                            $"prayer-alert?prayerType={(int)prayerType}");
                    }
                    else
                    {
                        // Shell not yet ready — store for HomePage to consume on appearing.
                        PrayerNavigationRequest.Set(prayerType);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(
                        $"MainActivity: navigation to prayer-alert failed: {ex.Message}");
                }
            });
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
