using QiblaNow.Presentation.ViewModels;

namespace QiblaNow.App.Pages;

public partial class PrayerAlertPage : ContentPage
{
    private readonly PrayerAlertViewModel _viewModel;

    public PrayerAlertPage(PrayerAlertViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
#if ANDROID
        BottomBanner.AdsId = AdMobConfig.BannerId;
#endif
    }

    public void Configure(string prayerName, string prayerTime, string currentTime)
        => _viewModel.UpdateAlert(prayerName, prayerTime, currentTime);
}
