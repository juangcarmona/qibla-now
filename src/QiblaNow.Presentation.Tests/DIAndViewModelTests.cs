using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using QiblaNow.Presentation.ViewModels;

namespace QiblaNow.Presentation.Tests;

public class DIAndViewModelTests
{
    [Fact]
    public void DI_Registers_ViewModels()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddTransient<PrayerTimesViewModel>();
        services.AddTransient<QiblaViewModel>();
        services.AddTransient<MapViewModel>();
        
        var serviceProvider = services.BuildServiceProvider();

        // Act
        var timesViewModel = serviceProvider.GetRequiredService<PrayerTimesViewModel>();
        var qiblaViewModel = serviceProvider.GetRequiredService<QiblaViewModel>();
        var mapViewModel = serviceProvider.GetRequiredService<MapViewModel>();

        // Assert
        Assert.NotNull(timesViewModel);
        Assert.NotNull(qiblaViewModel);
        Assert.NotNull(mapViewModel);
    }

    [Fact]
    public void ViewModels_Inherit_From_ObservableObject()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddTransient<PrayerTimesViewModel>();
        services.AddTransient<QiblaViewModel>();
        services.AddTransient<MapViewModel>();
        
        var serviceProvider = services.BuildServiceProvider();

        // Act
        var prayerTimesViewModel = serviceProvider.GetRequiredService<PrayerTimesViewModel>();
        var qiblaViewModel = serviceProvider.GetRequiredService<QiblaViewModel>();
        var mapViewModel = serviceProvider.GetRequiredService<MapViewModel>();

        // Assert
        Assert.IsAssignableFrom<ObservableObject>(prayerTimesViewModel);
        Assert.IsAssignableFrom<ObservableObject>(qiblaViewModel);
        Assert.IsAssignableFrom<ObservableObject>(mapViewModel);
    }

    [Fact]
    public void ViewModels_Resolved_Via_DI()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddTransient<PrayerTimesViewModel>();
        
        var serviceProvider = services.BuildServiceProvider();

        // Act
        var timesViewModel = serviceProvider.GetRequiredService<PrayerTimesViewModel>();
        var timesViewModel2 = serviceProvider.GetRequiredService<PrayerTimesViewModel>();

        // Assert - Transient services are different instances
        Assert.NotNull(timesViewModel);
        Assert.NotNull(timesViewModel2);
        Assert.NotSame(timesViewModel, timesViewModel2);
    }
}
