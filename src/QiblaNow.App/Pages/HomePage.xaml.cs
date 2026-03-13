using Microsoft.Extensions.DependencyInjection;
using QiblaNow.Core.Abstractions;
using QiblaNow.Presentation.ViewModels;

namespace QiblaNow.App.Pages;

public partial class HomePage : ContentPage
{
    private readonly HomeViewModel _vm;

    public HomePage(HomeViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = vm;

#if ANDROID
        BottomBanner.AdsId = AdMobConfig.BannerId;
#endif
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.InitializeAsync();
        await ReconcileNotificationsAsync();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _vm.Cleanup();
    }

    private static async Task ReconcileNotificationsAsync()
    {
        try
        {
            await Task.Yield();
            var scheduler = IPlatformApplication.Current?.Services?.GetService<INotificationScheduler>();
            if (scheduler != null)
                await scheduler.ReconcileOnStartupAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Notification reconciliation failed: {ex.Message}");
        }
    }
}