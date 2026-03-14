namespace QiblaNow.App.Pages;

public partial class AboutPage : ContentPage
{
    private const string GithubUrl = "https://github.com/juangcarmona/qibla-now";
    private const string WebsiteUrl = "https://jgcarmona.com/";
    private const string DonateUrl = "https://github.com/sponsors/juangcarmona";

    public string AppVersion => AppInfo.Current.VersionString;

    public AboutPage()
    {
        InitializeComponent();
        BindingContext = this;
    }

    private async void OnGithubTapped(object? sender, TappedEventArgs e)
    {
        await Launcher.Default.OpenAsync(GithubUrl);
    }

    private async void OnWebsiteTapped(object? sender, TappedEventArgs e)
    {
        await Launcher.Default.OpenAsync(WebsiteUrl);
    }

    private async void OnDonateClicked(object? sender, EventArgs e)
    {
        await Launcher.Default.OpenAsync(DonateUrl);
    }
}