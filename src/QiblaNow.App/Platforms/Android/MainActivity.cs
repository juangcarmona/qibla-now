using Android.App;
using Android.Content.PM;
using Android.Gms.Ads;
using Android.OS;
using Plugin.MauiMtAdmob;

namespace QiblaNow.App
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            if (Build.VERSION.SdkInt >= BuildVersionCodes.Tiramisu)
            {
                Permissions.RequestAsync<Permissions.PostNotifications>();
            }
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
    }
}
