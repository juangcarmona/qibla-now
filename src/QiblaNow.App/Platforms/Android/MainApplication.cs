using Android.App;
using Android.Runtime;
using Android.Content;
using QiblaNow.App.Platforms.Android;

namespace QiblaNow.App
{
    [Application]
    public class MainApplication : MauiApplication
    {
        private BootReceiver? _bootReceiver;

        public MainApplication(IntPtr handle, JniHandleOwnership ownership)
            : base(handle, ownership)
        {
        }

        protected override void OnCreate()
        {
            base.OnCreate();

            // Register boot receiver
            _bootReceiver = new BootReceiver();
            var bootFilter = new IntentFilter("android.intent.action.BOOT_COMPLETED");
            RegisterReceiver(_bootReceiver, bootFilter);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            // Unregister boot receiver
            _bootReceiver?.Dispose();
        }

        protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
    }
}
