using QiblaNow.Core.Abstractions.Services;
using QiblaNow.App.Droid.Services;

namespace QiblaNow.App.Droid
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();

            builder
                .UseSharedMauiApp();

            // Register Android-specific services
            builder.Services.AddSingleton<ILocationService, LocationService>();

            return builder.Build();
        }
    }
}
