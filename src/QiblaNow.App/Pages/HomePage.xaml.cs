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
#if ANDROID
        await CheckPendingPrayerAlertAsync();
#endif
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

#if ANDROID
    /// <summary>
    /// Checks for a pending prayer-alert navigation request (set by
    /// <see cref="Platforms.Android.PrayerNavigationRequest"/> when the Shell was not yet
    /// ready at the time the alarm fired or the notification was tapped).
    /// </summary>
    private static async Task CheckPendingPrayerAlertAsync()
    {
        try
        {
            var pending = Platforms.Android.PrayerNavigationRequest.TakeAndClear();
            if (pending.HasValue)
            {
                await Task.Yield();
                await Shell.Current.GoToAsync(
                    $"prayer-alert?prayerType={(int)pending.Value}");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(
                $"HomePage: pending prayer-alert navigation failed: {ex.Message}");
        }
    }
#endif
}