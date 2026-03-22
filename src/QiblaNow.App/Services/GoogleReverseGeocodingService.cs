using System.Globalization;
using System.Text.Json;
using QiblaNow.Core.Abstractions;
using QiblaNow.Core.Models;

namespace QiblaNow.App.Services;

public sealed class GoogleReverseGeocodingService : IReverseGeocodingService
{
    private static readonly HttpClient Http = new()
    {
        Timeout = TimeSpan.FromSeconds(10)
    };

    private static readonly TimeSpan CacheTtl = TimeSpan.FromHours(24);
    private const int CacheSizeLimit = 200;

    private readonly Dictionary<string, CacheEntry> _cache = new();
    private readonly object _cacheLock = new();

    public async Task<ResolvedPlace?> ReverseGeocodeAsync(
    double latitude,
    double longitude,
    string language,
    CancellationToken cancellationToken = default)
    {
        var key = ReverseGeocodingHelper.BuildCacheKey(latitude, longitude, language);
        var now = DateTimeOffset.UtcNow;

        lock (_cacheLock)
        {
            if (_cache.TryGetValue(key, out var cached))
            {
                if (now - cached.TimestampUtc <= CacheTtl)
                    return cached.Place;

                _cache.Remove(key);
            }
        }

        var apiKey = GoogleApiConfig.GeocodingApiKey;
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            System.Diagnostics.Debug.WriteLine("Reverse geocoding: API key is empty.");
            return null;
        }

        var normalizedLanguage = string.IsNullOrWhiteSpace(language)
            ? CultureInfo.CurrentUICulture.TwoLetterISOLanguageName
            : language.Trim();

        var url =
            $"https://maps.googleapis.com/maps/api/geocode/json?latlng={latitude.ToString(CultureInfo.InvariantCulture)},{longitude.ToString(CultureInfo.InvariantCulture)}&key={Uri.EscapeDataString(apiKey)}&language={Uri.EscapeDataString(normalizedLanguage)}";

        try
        {
            System.Diagnostics.Debug.WriteLine($"Reverse geocoding request: {latitude}, {longitude}, lang={normalizedLanguage}");

            using var response = await Http.GetAsync(url, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);

            System.Diagnostics.Debug.WriteLine($"Reverse geocoding HTTP {(int)response.StatusCode}");
            System.Diagnostics.Debug.WriteLine(body);

            if (!response.IsSuccessStatusCode)
                return null;

            using var json = JsonDocument.Parse(body);
            var root = json.RootElement;

            var status = root.TryGetProperty("status", out var statusElement)
                ? statusElement.GetString()
                : null;

            var errorMessage = root.TryGetProperty("error_message", out var errorElement)
                ? errorElement.GetString()
                : null;

            System.Diagnostics.Debug.WriteLine($"Geocoding status={status}, error={errorMessage}");

            if (string.Equals(status, "ZERO_RESULTS", StringComparison.OrdinalIgnoreCase))
                return null;

            if (!string.Equals(status, "OK", StringComparison.OrdinalIgnoreCase))
                return null;

            if (!root.TryGetProperty("results", out var results) ||
                results.ValueKind != JsonValueKind.Array ||
                results.GetArrayLength() == 0)
                return null;

            var first = results[0];
            var formatted = TryGetString(first, "formatted_address");
            string? locality = null;
            string? sublocality = null;
            string? adminArea = null;
            string? country = null;

            if (first.TryGetProperty("address_components", out var components) &&
                components.ValueKind == JsonValueKind.Array)
            {
                foreach (var component in components.EnumerateArray())
                {
                    var longName = TryGetString(component, "long_name");
                    if (string.IsNullOrWhiteSpace(longName))
                        continue;

                    if (!component.TryGetProperty("types", out var types) ||
                        types.ValueKind != JsonValueKind.Array)
                        continue;

                    var tags = types.EnumerateArray()
                        .Select(x => x.GetString())
                        .Where(x => !string.IsNullOrWhiteSpace(x))
                        .Select(x => x!)
                        .ToHashSet(StringComparer.OrdinalIgnoreCase);

                    if (locality is null && tags.Contains("locality"))
                        locality = longName;
                    if (sublocality is null && (tags.Contains("sublocality") || tags.Contains("sublocality_level_1")))
                        sublocality = longName;
                    if (adminArea is null && tags.Contains("administrative_area_level_1"))
                        adminArea = longName;
                    if (country is null && tags.Contains("country"))
                        country = longName;
                }
            }

            var place = new ResolvedPlace(
                latitude,
                longitude,
                normalizedLanguage,
                locality,
                sublocality,
                adminArea,
                country,
                formatted);

            lock (_cacheLock)
            {
                _cache[key] = new CacheEntry(place, now);

                if (_cache.Count > CacheSizeLimit)
                {
                    foreach (var stale in _cache.OrderBy(x => x.Value.TimestampUtc)
                                 .Take(_cache.Count - CacheSizeLimit)
                                 .Select(x => x.Key)
                                 .ToList())
                    {
                        _cache.Remove(stale);
                    }
                }
            }

            return place;
        }
        catch (TaskCanceledException ex)
        {
            System.Diagnostics.Debug.WriteLine($"Reverse geocoding timeout/cancelled: {ex}");
            return null;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Reverse geocoding failed: {ex}");
            return null;
        }
    }

    private static string? TryGetString(JsonElement node, string propertyName)
        => node.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;

    private sealed record CacheEntry(ResolvedPlace Place, DateTimeOffset TimestampUtc);
}
