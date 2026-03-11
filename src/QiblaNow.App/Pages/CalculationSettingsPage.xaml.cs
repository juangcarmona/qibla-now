using QiblaNow.Presentation.ViewModels;

namespace QiblaNow.App.Pages;

public partial class CalculationSettingsPage : ContentPage
{
    public CalculationSettingsPage(SettingsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
#if ANDROID
        BottomBanner.AdsId = AdMobConfig.BannerId;
#endif
    }
}
