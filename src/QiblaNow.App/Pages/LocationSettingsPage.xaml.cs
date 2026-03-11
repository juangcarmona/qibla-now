using QiblaNow.Presentation.ViewModels;

namespace QiblaNow.App.Pages;

public partial class LocationSettingsPage : ContentPage
{
    public LocationSettingsPage(SettingsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
#if ANDROID
        BottomBanner.AdsId = AdMobConfig.BannerId;
#endif
    }
}
