using Microsoft.Maui.Storage;
using QiblaNow.Core.Abstractions;
using QiblaNow.Core.Models;

namespace QiblaNow.App.Services;

/// <summary>
/// Implementation of ISettingsStore using MAUI Preferences
/// </summary>
public sealed class SettingsStore : ISettingsStore
{
    private const string KeyLocationMode = "location_mode";
    private const string KeyLatitude = "latitude";
    private const string KeyLongitude = "longitude";
    private const string KeyLabel = "location_label";
    private const string KeyTimestamp = "timestamp";

    public LocationMode GetLocationMode()
    {
        try
        {
            var mode = Preferences.Default.Get(KeyLocationMode, (int)LocationMode.GPS);
            return Enum.IsDefined(typeof(LocationMode), mode)
                ? (LocationMode)mode
                : LocationMode.GPS;
        }
        catch
        {
            return LocationMode.GPS;
        }
    }

    public void SetLocationMode(LocationMode mode)
    {
        try
        {
            Preferences.Default.Set(KeyLocationMode, (int)mode);
        }
        catch
        {
            // Ignore storage errors
        }
    }

    public LocationSnapshot? GetLastSnapshot()
    {
        try
        {
            if (!Preferences.Default.ContainsKey(KeyLatitude) ||
                !Preferences.Default.ContainsKey(KeyLongitude) ||
                !Preferences.Default.ContainsKey(KeyTimestamp))
            {
                return null;
            }

            var latitude = Preferences.Default.Get(KeyLatitude, 0.0);
            var longitude = Preferences.Default.Get(KeyLongitude, 0.0);
            var label = Preferences.Default.Get(KeyLabel, string.Empty);
            var timestampStr = Preferences.Default.Get(KeyTimestamp, string.Empty);

            if (!DateTimeOffset.TryParse(timestampStr, out DateTimeOffset timestamp))
            {
                return null;
            }

            return new LocationSnapshot(LocationMode.Manual, latitude, longitude, string.IsNullOrEmpty(label) ? null : label)
            {
                Timestamp = timestamp
            };
        }
        catch
        {
            return null;
        }
    }

    public void SaveSnapshot(LocationSnapshot snapshot)
    {
        try
        {
            Preferences.Default.Set(KeyLatitude, snapshot.Latitude);
            Preferences.Default.Set(KeyLongitude, snapshot.Longitude);
            Preferences.Default.Set(KeyLabel, snapshot.Label ?? string.Empty);
            Preferences.Default.Set(KeyTimestamp, snapshot.Timestamp.ToString("o"));
            SetLocationMode(snapshot.Mode);
        }
        catch
        {
            // Ignore storage errors
        }
    }
}
