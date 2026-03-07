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

    public PrayerCalculationSettings GetCalculationSettings()
    {
        try
        {
            var method = (CalculationMethod)Preferences.Default.Get("calculation_method", (int)CalculationMethod.MuslimWorldLeague);
            var madhab = (Madhab)Preferences.Default.Get("madhab", (int)Madhab.Shafi);
            var highLatitudeRule = (HighLatitudeRule)Preferences.Default.Get("high_latitude_rule", (int)HighLatitudeRule.SeventhOfNight);
            var fajrOffset = Preferences.Default.Get("fajr_offset_minutes", 0);
            var dhuhrOffset = Preferences.Default.Get("dhuhr_offset_minutes", 0);
            var asrOffset = Preferences.Default.Get("asr_offset_minutes", 0);
            var maghribOffset = Preferences.Default.Get("maghrib_offset_minutes", 0);
            var ishaOffset = Preferences.Default.Get("isha_offset_minutes", 0);

            var settings = new PrayerCalculationSettings
            {
                Method = method,
                Madhab = madhab,
                HighLatitudeRule = highLatitudeRule,
                FajrOffsetMinutes = fajrOffset,
                DhuhrOffsetMinutes = dhuhrOffset,
                AsrOffsetMinutes = asrOffset,
                MaghribOffsetMinutes = maghribOffset,
                IshaOffsetMinutes = ishaOffset
            };
            return settings;
        }
        catch
        {
            return new PrayerCalculationSettings();
        }
    }

    public void SaveCalculationSettings(PrayerCalculationSettings settings)
    {
        try
        {
            Preferences.Default.Set("calculation_method", (int)settings.Method);
            Preferences.Default.Set("madhab", (int)settings.Madhab);
            Preferences.Default.Set("high_latitude_rule", (int)settings.HighLatitudeRule);
            Preferences.Default.Set("fajr_offset_minutes", settings.FajrOffsetMinutes);
            Preferences.Default.Set("dhuhr_offset_minutes", settings.DhuhrOffsetMinutes);
            Preferences.Default.Set("asr_offset_minutes", settings.AsrOffsetMinutes);
            Preferences.Default.Set("maghrib_offset_minutes", settings.MaghribOffsetMinutes);
            Preferences.Default.Set("isha_offset_minutes", settings.IshaOffsetMinutes);
        }
        catch
        {
            // Ignore storage errors
        }
    }

    public PrayerNotificationSettings GetNotificationSettings()
    {
        try
        {
            var settings = new PrayerNotificationSettings
            {
                FajrEnabled = Preferences.Default.Get("fajr_enabled", false),
                DhuhrEnabled = Preferences.Default.Get("dhuhr_enabled", false),
                AsrEnabled = Preferences.Default.Get("asr_enabled", false),
                MaghribEnabled = Preferences.Default.Get("maghrib_enabled", false),
                IshaEnabled = Preferences.Default.Get("isha_enabled", false)
            };
            return settings;
        }
        catch
        {
            return new PrayerNotificationSettings();
        }
    }

    public void SaveNotificationSettings(PrayerNotificationSettings settings)
    {
        try
        {
            Preferences.Default.Set("fajr_enabled", settings.FajrEnabled);
            Preferences.Default.Set("dhuhr_enabled", settings.DhuhrEnabled);
            Preferences.Default.Set("asr_enabled", settings.AsrEnabled);
            Preferences.Default.Set("maghrib_enabled", settings.MaghribEnabled);
            Preferences.Default.Set("isha_enabled", settings.IshaEnabled);
        }
        catch
        {
            // Ignore storage errors
        }
    }

    public LocationSnapshot? GetLastValidLocation()
    {
        return GetLastSnapshot();
    }

    public void SaveLastValidLocation(LocationSnapshot location)
    {
        SaveSnapshot(location);
    }
}
