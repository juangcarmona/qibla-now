namespace QiblaNow.App.Pages;

public partial class QiblaPage : ContentPage
{
	public QiblaPage()
	{
		InitializeComponent();
#if ANDROID
        BottomBanner.AdsId = AdMobConfig.BannerId;
#endif
    }
}