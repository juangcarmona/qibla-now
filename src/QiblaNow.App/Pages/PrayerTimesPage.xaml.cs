using Microsoft.Maui.Controls;
using QiblaNow.Presentation.ViewModels;

namespace QiblaNow.App.Pages;

public partial class PrayerTimesPage : ContentPage
{
    public PrayerTimesPage(PrayerTimesViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
#if ANDROID
        BottomBanner.AdsId = AdMobConfig.BannerId;
#endif
    }
}
