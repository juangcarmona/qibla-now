namespace QiblaNow.App.Pages;

public partial class DisplaySettingsPage : ContentPage
{
    public DisplaySettingsPage()
    {
        InitializeComponent();
#if ANDROID
        BottomBanner.AdsId = AdMobConfig.BannerId;
#endif
    }
}
