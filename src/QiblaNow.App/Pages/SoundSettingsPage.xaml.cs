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

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        if (BindingContext is SettingsViewModel vm)
            vm.Cleanup();
    }
}
