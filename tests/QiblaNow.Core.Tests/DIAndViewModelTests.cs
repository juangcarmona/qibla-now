using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace QiblaNow.Core.Tests;

using QiblaNow.App;
public class DIAndViewModelTests
{
    [Fact]
    public void DI_Registers_ViewModels()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddTransient<TimesViewModel>();
        services.AddTransient<CompassViewModel>();
        services.AddTransient<MapViewModel>();
        
        var serviceProvider = services.BuildServiceProvider();

        // Act
        var timesViewModel = serviceProvider.GetRequiredService<TimesViewModel>();
        var compassViewModel = serviceProvider.GetRequiredService<CompassViewModel>();
        var mapViewModel = serviceProvider.GetRequiredService<MapViewModel>();

        // Assert
        Assert.NotNull(timesViewModel);
        Assert.NotNull(compassViewModel);
        Assert.NotNull(mapViewModel);
    }

    [Fact]
    public void ViewModels_Inherit_From_ObservableObject()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddTransient<TimesViewModel>();
        services.AddTransient<CompassViewModel>();
        services.AddTransient<MapViewModel>();
        
        var serviceProvider = services.BuildServiceProvider();

        // Act
        var timesViewModel = serviceProvider.GetRequiredService<TimesViewModel>();
        var compassViewModel = serviceProvider.GetRequiredService<CompassViewModel>();
        var mapViewModel = serviceProvider.GetRequiredService<MapViewModel>();

        // Assert
        Assert.IsAssignableFrom<ObservableObject>(timesViewModel);
        Assert.IsAssignableFrom<ObservableObject>(compassViewModel);
        Assert.IsAssignableFrom<ObservableObject>(mapViewModel);
    }

    [Fact]
    public void ViewModels_Resolved_Via_DI()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddTransient<TimesViewModel>();
        
        var serviceProvider = services.BuildServiceProvider();

        // Act
        var timesViewModel = serviceProvider.GetRequiredService<TimesViewModel>();
        var timesViewModel2 = serviceProvider.GetRequiredService<TimesViewModel>();

        // Assert - Transient services are different instances
        Assert.NotNull(timesViewModel);
        Assert.NotNull(timesViewModel2);
        Assert.NotSame(timesViewModel, timesViewModel2);
    }
}
