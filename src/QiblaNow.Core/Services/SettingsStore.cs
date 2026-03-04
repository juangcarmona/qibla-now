using QiblaNow.Core.Abstractions.Models;
using QiblaNow.Core.Abstractions.Services;

namespace QiblaNow.Core.Services;

/// <summary>
/// Implementation of ISettingsStore using Preferences service
/// </summary>
public class SettingsStore : ISettingsStore
{
    private readonly IPreferencesService _preferencesService;

    private const string KeyLocationMode = "location_mode";
    private const string KeyLatitude = "latitude";
    private const string KeyLongitude = "longitude";
    private const string KeyLabel = "location_label";
    private const string KeyTimestamp = "timestamp";

    public SettingsStore(IPreferencesService preferencesService)
    {
        _preferencesService = preferencesService;
    }

    public LocationMode GetLocationMode()
    {
        var modeString = _preferencesService.Get(KeyLocationMode, "0");
        return int.TryParse(modeString, out int mode) && Enum.IsDefined(typeof(LocationMode), mode)
            ? (LocationMode)mode
            : LocationMode.GPS;
    }

    public void SetLocationMode(LocationMode mode)
    {
        _preferencesService.Set(KeyLocationMode, ((int)mode).ToString());
    }

    public LocationSnapshot? GetLastSnapshot()
    {
        var latStr = _preferencesService.Get(KeyLatitude, string.Empty);
        var lonStr = _preferencesService.Get(KeyLongitude, string.Empty);
        var label = _preferencesService.Get(KeyLabel, string.Empty);
        var timestampStr = _preferencesService.Get(KeyTimestamp, string.Empty);

        if (string.IsNullOrEmpty(latStr) || string.IsNullOrEmpty(lonStr) || string.IsNullOrEmpty(timestampStr))
        {
            return null;
        }

        if (!double.TryParse(latStr, out double latitude) ||
            !double.TryParse(lonStr, out double longitude) ||
            !DateTimeOffset.TryParse(timestampStr, out DateTimeOffset timestamp))
        {
            return null;
        }

        return new LocationSnapshot(LocationMode.Manual, latitude, longitude, string.IsNullOrEmpty(label) ? null : label)
        {
            Timestamp = timestamp
        };
    }

    public void SaveSnapshot(LocationSnapshot snapshot)
    {
        _preferencesService.Set(KeyLatitude, snapshot.Latitude.ToString());
        _preferencesService.Set(KeyLongitude, snapshot.Longitude.ToString());
        _preferencesService.Set(KeyLabel, snapshot.Label ?? string.Empty);
        _preferencesService.Set(KeyTimestamp, snapshot.Timestamp.ToString("o"));
        SetLocationMode(snapshot.Mode);
    }
}
