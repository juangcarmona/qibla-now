using Microsoft.Maui.Storage;
using QiblaNow.Core.Abstractions;
using QiblaNow.Core.Models;

namespace QiblaNow.App.Services;

/// <summary>
/// Implementation of ISettingsStore using MAUI Preferences.
/// Key names follow the naming convention defined in DATA_MODEL.md.
/// </summary>
public sealed class SettingsStore : ISettingsStore
{
    // ── Location keys ────────────────────────────────────────────────────────
    private const string KeyLocationMode  = "location_mode";
    private const string KeyManualLat     = "manual.lat";
    private const string KeyManualLon     = "manual.lon";
    private const string KeyLastLat       = "last.lat";
    private const string KeyLastLon       = "last.lon";
    private const string KeyLastLabel     = "last.label";
    private const string KeyLastTimestamp = "last.timestampUtc";

    // ── Scheduling recovery keys (DATA_MODEL.md) ──────────────────────────────
    private const string KeySchedPrayer    = "scheduling.lastPlannedPrayer";
    private const string KeySchedTrigger   = "scheduling.lastPlannedTriggerUtc";
    private const string KeySchedReqCode   = "scheduling.lastPlannedRequestCode";
    private const string KeySchedReconcile = "scheduling.lastReconciledUtc";

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
            if (!Preferences.Default.ContainsKey(KeyLastLat) ||
                !Preferences.Default.ContainsKey(KeyLastLon))
                return null;

            var latitude  = Preferences.Default.Get(KeyLastLat, 0.0);
            var longitude = Preferences.Default.Get(KeyLastLon, 0.0);
            var label     = Preferences.Default.Get(KeyLastLabel, string.Empty);
            var tsStr     = Preferences.Default.Get(KeyLastTimestamp, string.Empty);

            if (!DateTimeOffset.TryParse(tsStr, out var timestamp))
                timestamp = DateTimeOffset.UtcNow;

            return new LocationSnapshot(
                LocationMode.Manual, latitude, longitude,
                string.IsNullOrEmpty(label) ? null : label)
            {
                Timestamp = timestamp
            };
        }
        catch { return null; }
    }

    public void SaveSnapshot(LocationSnapshot snapshot)
    {
        try
        {
            Preferences.Default.Set(KeyLastLat,       snapshot.Latitude);
            Preferences.Default.Set(KeyLastLon,       snapshot.Longitude);
            Preferences.Default.Set(KeyLastLabel,     snapshot.Label ?? string.Empty);
            Preferences.Default.Set(KeyLastTimestamp, snapshot.Timestamp.ToString("o"));
            SetLocationMode(snapshot.Mode);

            if (snapshot.Mode == LocationMode.Manual)
            {
                Preferences.Default.Set(KeyManualLat, snapshot.Latitude);
                Preferences.Default.Set(KeyManualLon, snapshot.Longitude);
            }
        }
        catch { /* ignore storage errors */ }
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
                FajrEnabled    = Preferences.Default.Get("fajr_enabled",    true),
                DhuhrEnabled   = Preferences.Default.Get("dhuhr_enabled",   true),
                AsrEnabled     = Preferences.Default.Get("asr_enabled",     true),
                MaghribEnabled = Preferences.Default.Get("maghrib_enabled", true),
                IshaEnabled    = Preferences.Default.Get("isha_enabled",    true)
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

    public LocationSnapshot? GetLastValidLocation() => GetLastSnapshot();

    public void SaveLastValidLocation(LocationSnapshot location) => SaveSnapshot(location);

    public SchedulingState GetSchedulingState()
    {
        try
        {
            return new SchedulingState
            {
                LastPlannedPrayer     = Preferences.Default.Get(KeySchedPrayer,    (string?)null),
                LastPlannedTriggerUtc = Preferences.Default.ContainsKey(KeySchedTrigger)
                    ? Preferences.Default.Get(KeySchedTrigger, 0L)
                    : null,
                LastPlannedRequestCode = Preferences.Default.ContainsKey(KeySchedReqCode)
                    ? Preferences.Default.Get(KeySchedReqCode, 0)
                    : null,
                LastReconciledUtc = Preferences.Default.ContainsKey(KeySchedReconcile)
                    ? Preferences.Default.Get(KeySchedReconcile, 0L)
                    : null
            };
        }
        catch { return new SchedulingState(); }
    }

    /// <summary>
    /// Persists scheduling metadata ONLY — never touches notification preference flags.
    /// </summary>
    public void SaveSchedulingState(SchedulingState state)
    {
        try
        {
            if (state.LastPlannedPrayer != null)
                Preferences.Default.Set(KeySchedPrayer, state.LastPlannedPrayer);
            if (state.LastPlannedTriggerUtc.HasValue)
                Preferences.Default.Set(KeySchedTrigger, state.LastPlannedTriggerUtc.Value);
            if (state.LastPlannedRequestCode.HasValue)
                Preferences.Default.Set(KeySchedReqCode, state.LastPlannedRequestCode.Value);
            if (state.LastReconciledUtc.HasValue)
                Preferences.Default.Set(KeySchedReconcile, state.LastReconciledUtc.Value);
        }
        catch { /* ignore */ }
    }
}
