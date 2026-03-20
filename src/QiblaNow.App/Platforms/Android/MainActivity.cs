using Android.App;
using Android.Content.PM;
using Android.Content;
using Android.OS;
using Android.Provider;
using Microsoft.Maui.ApplicationModel;
using Plugin.MauiMtAdmob;
using QiblaNow.App.Pages;
using QiblaNow.Presentation.ViewModels;

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

            TryNavigateToPrayerAlert(Intent);
        }

        protected override void OnNewIntent(Intent? intent)
        {
            base.OnNewIntent(intent);
            TryNavigateToPrayerAlert(intent);
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

        private static void TryNavigateToPrayerAlert(Intent? intent)
        {
            if (intent?.Action != Platforms.Android.AndroidNotificationScheduler.TapActionOpenPrayerAlert)
                return;

            var prayerName = intent.GetStringExtra(Platforms.Android.AndroidNotificationScheduler.TapExtraPrayerName) ?? "Prayer";
            var prayerTime = intent.GetStringExtra(Platforms.Android.AndroidNotificationScheduler.TapExtraPrayerTime) ?? "--:--";
            var currentTime = intent.GetStringExtra(Platforms.Android.AndroidNotificationScheduler.TapExtraCurrentTime) ?? "--:--";

            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await Task.Delay(150);

                if (global::Microsoft.Maui.Controls.Application.Current?.Windows.FirstOrDefault()?.Page is not global::Microsoft.Maui.Controls.Shell shell)
                    return;

                var services = IPlatformApplication.Current?.Services;
                var page = services?.GetService(typeof(PrayerAlertPage)) as PrayerAlertPage;
                var viewModel = services?.GetService(typeof(PrayerAlertViewModel)) as PrayerAlertViewModel;
                if (page == null && viewModel != null)
                    page = new PrayerAlertPage(viewModel);

                if (page == null)
                    return;

                page.Configure(prayerName, prayerTime, currentTime);
                await shell.Navigation.PushAsync(page);
            });
        }
    }
}
