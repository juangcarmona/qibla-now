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
        private const int ShellInitializationDelayMs = 150;

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
                await Task.Delay(ShellInitializationDelayMs);

                if (global::Microsoft.Maui.Controls.Application.Current?.Windows.FirstOrDefault()?.Page is not global::Microsoft.Maui.Controls.Shell shell)
                {
                    System.Diagnostics.Debug.WriteLine("Prayer alert navigation skipped: Shell is not ready.");
                    return;
                }

                var services = IPlatformApplication.Current?.Services;
                if (services == null)
                {
                    System.Diagnostics.Debug.WriteLine("Prayer alert navigation skipped: service provider unavailable.");
                    return;
                }

                var page = services.GetService(typeof(PrayerAlertPage)) as PrayerAlertPage;
                if (page == null)
                {
                    var viewModel = services.GetService(typeof(PrayerAlertViewModel)) as PrayerAlertViewModel;
                    if (viewModel == null)
                    {
                        System.Diagnostics.Debug.WriteLine("Prayer alert navigation skipped: PrayerAlertViewModel could not be resolved.");
                        return;
                    }

                    page = new PrayerAlertPage(viewModel);
                }

                if (page == null)
                {
                    System.Diagnostics.Debug.WriteLine("Prayer alert navigation skipped: PrayerAlertPage could not be resolved.");
                    return;
                }

                page.Configure(prayerName, prayerTime, currentTime);
                await shell.Navigation.PushAsync(page);
            });
        }
    }
}
