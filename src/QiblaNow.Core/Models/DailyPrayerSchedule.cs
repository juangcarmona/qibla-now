using System.Globalization;

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
    /// Gets the next prayer after the given reference time
    /// </summary>
    public PrayerTime? GetNextPrayer(DateTimeOffset referenceTime)
    {
        foreach (var prayer in Prayers.OrderBy(p => p.DateTime))
        {
            if (prayer.DateTime > referenceTime)
                return prayer;
        }

        return null;
    }

    /// <summary>
    /// Gets the current or next prayer for the given reference time.
    /// If the day is finished, returns today's Fajr as the wrap marker.
    /// </summary>
    public PrayerTime? GetCurrentOrNextPrayer(DateTimeOffset referenceTime)
    {
        var next = GetNextPrayer(referenceTime);
        if (next.HasValue)
            return next;

        return GetPrayer(PrayerType.Fajr);
    }

    /// <summary>
    /// Gets prayer by type
    /// </summary>
    public PrayerTime? GetPrayer(PrayerType type)
    {
        foreach (var prayer in Prayers)
        {
            if (prayer.Type == type)
                return prayer;
        }

        return null;
    }

    /// <summary>
    /// Validates all prayer times are within valid ranges and ordered
    /// </summary>
    public bool IsValid()
    {
        if (Prayers.Count < 6)
            return false;

        var ordered = Prayers.OrderBy(p => p.DateTime).ToList();

        for (var i = 1; i < ordered.Count; i++)
        {
            if (ordered[i - 1].DateTime >= ordered[i].DateTime)
                return false;
        }

        return ordered.Any(p => p.Type == PrayerType.Fajr)
            && ordered.Any(p => p.Type == PrayerType.Sunrise)
            && ordered.Any(p => p.Type == PrayerType.Dhuhr)
            && ordered.Any(p => p.Type == PrayerType.Asr)
            && ordered.Any(p => p.Type == PrayerType.Maghrib)
            && ordered.Any(p => p.Type == PrayerType.Isha);
    }

    /// <summary>
    /// Gets prayer times formatted for display
    /// </summary>
    public IEnumerable<string> GetDisplayTimes()
    {
        foreach (var prayer in Prayers.OrderBy(p => p.DateTime))
        {
            yield return $"{prayer.Type}: {prayer.ToShortString()}{(prayer.OffsetMinutes != 0 ? $" ({prayer.OffsetMinutes:+#;-#;0} min)" : "")}";
        }
    }
}