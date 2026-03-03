using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Hosting;
using QiblaNow.App;

var builder = MauiApp.CreateBuilder();
builder
    .UseMauiApp<App>()
    .ConfigureFonts(fonts =>
    {
        fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
        fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
    })
    .ConfigureServices(services =>
    {
        // Transient ViewModels
        services.AddTransient<TimesViewModel>();
        services.AddTransient<CompassViewModel>();
        services.AddTransient<MapViewModel>();
    });

return builder.Build();
