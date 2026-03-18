using QiblaNow.Presentation.ViewModels;

namespace QiblaNow.App.Pages;

public partial class PrayerAlertPage : ContentPage
{
    private readonly PrayerAlertViewModel _vm;

    public PrayerAlertPage(PrayerAlertViewModel viewModel)
    {
        InitializeComponent();
        _vm = viewModel;
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        // Ensure playback is running when the page appears.
        // This handles the notification-tap path where the alarm was already started
        // in the background, as well as any edge case where it was not.
        _vm.EnsurePlayback();
    }
}
