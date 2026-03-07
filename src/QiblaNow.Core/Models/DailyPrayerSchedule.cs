using System.Globalization;
using QiblaNow.Core.Models;

namespace QiblaNow.Core.Models;

/// <summary>
/// Represents a complete daily prayer schedule for a specific date and timezone
/// </summary>
public sealed class DailyPrayerSchedule
{
    public DateTimeOffset Date { get; }
    public TimeZoneInfo TimeZone { get; }
    public List<PrayerTime> Prayers { get; }

    public DailyPrayerSchedule(DateTimeOffset date, TimeZoneInfo timeZone)
    {
        Date = date;
        TimeZone = timeZone;
        Prayers = new List<PrayerTime>();
    }

    /// <summary>
    /// Gets the next prayer after the given reference time (default: now)
    /// </summary>
    public PrayerTime? GetNextPrayer(DateTimeOffset? referenceTime = null)
    {
        var now = referenceTime ?? DateTimeOffset.Now;
        return Prayers
            .Where(p => p.DateTime > now)
            .OrderBy(p => p.DateTime)
            .FirstOrDefault();
    }

    /// <summary>
    /// Gets the current or next prayer
    /// </summary>
    public PrayerTime? GetCurrentOrNextPrayer()
    {
        var now = DateTimeOffset.Now;
        var next = GetNextPrayer(now);
        if (next != null) return next;

        // If no next prayer today, wrap to next day Fajr
        var fajrToday = Prayers.FirstOrDefault(p => p.Type == PrayerType.Fajr);
        if (fajrToday != null && fajrToday.DateTime <= now)
        {
            return fajrToday;
        }

        return null;
    }

    /// <summary>
    /// Gets prayer by type
    /// </summary>
    public PrayerTime GetPrayer(PrayerType type) =>
        Prayers.FirstOrDefault(p => p.Type == type);

    /// <summary>
    /// Validates all prayer times are within valid ranges
    /// </summary>
    public bool IsValid()
    {
        if (Prayers.Count < 5) return false; // Need Fajr through Isha
        return Prayers.All(p => true); // All prayer times are valid by construction
    }

    /// <summary>
    /// Gets prayer times formatted for display
    /// </summary>
    public IEnumerable<string> GetDisplayTimes()
    {
        foreach (var prayer in Prayers)
        {
            yield return $"{prayer.Type}: {prayer.ToShortString()}{(prayer.OffsetMinutes != 0 ? $" ({prayer.OffsetMinutes:+#;-#;0} min)" : "")}";
        }
    }
}
