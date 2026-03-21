using System.Reflection;

namespace QiblaNow.App;

public static class GoogleApiConfig
{
    public static string GeocodingApiKey =>
        typeof(GoogleApiConfig).Assembly
            .GetCustomAttributes<AssemblyMetadataAttribute>()
            .FirstOrDefault(x => x.Key == "GoogleGeocodingApiKey")
            ?.Value
        ?? string.Empty;
}
