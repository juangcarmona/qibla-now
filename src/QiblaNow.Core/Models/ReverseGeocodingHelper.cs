using System.Globalization;

namespace QiblaNow.Core.Models;

public static class ReverseGeocodingHelper
{
    public static double RoundCoordinate(double value)
        => Math.Round(value, 3, MidpointRounding.AwayFromZero);

    public static string BuildCacheKey(double latitude, double longitude, string language)
    {
        var lat = RoundCoordinate(latitude);
        var lon = RoundCoordinate(longitude);
        var lang = string.IsNullOrWhiteSpace(language) ? "en" : language.Trim().ToLowerInvariant();
        return $"{lat:F3}|{lon:F3}|{lang}";
    }

    public static string SelectDisplayName(ResolvedPlace? place, double latitude, double longitude)
    {
        var selected = FirstNonEmpty(
            place?.Locality,
            place?.Sublocality,
            place?.AdminArea,
            place?.Country,
            place?.FormattedAddress);

        return string.IsNullOrWhiteSpace(selected)
            ? FormatCoordinates(latitude, longitude)
            : selected;
    }

    public static string FormatCoordinates(double latitude, double longitude)
        => $"{latitude.ToString("F4", CultureInfo.InvariantCulture)}, {longitude.ToString("F4", CultureInfo.InvariantCulture)}";

    private static string? FirstNonEmpty(params string?[] values)
        => values.FirstOrDefault(v => !string.IsNullOrWhiteSpace(v));
}
