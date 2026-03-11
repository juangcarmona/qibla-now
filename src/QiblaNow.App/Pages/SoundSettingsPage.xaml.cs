using QiblaNow.Presentation.ViewModels;

namespace QiblaNow.App.Pages;

public partial class SoundSettingsPage : ContentPage
{
    public SoundSettingsPage(SettingsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
#if ANDROID
        BottomBanner.AdsId = AdMobConfig.BannerId;
#endif
    }
}
