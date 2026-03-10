namespace QiblaNow.Core.Models;

/// <summary>
/// Result of calculating the next prayer
/// </summary>
public sealed class NextPrayerResult
{
    public PrayerType Type { get; }
    public DateTimeOffset Time { get; }
    public TimeSpan Remaining { get; }
    public bool IsToday { get; }

    public NextPrayerResult(PrayerType type, DateTimeOffset time, TimeSpan remaining, bool isToday)
    {
        Type = type;
        Time = time;
        Remaining = remaining;
        IsToday = isToday;
    }

    /// <summary>
    /// Gets remaining minutes for display
    /// </summary>
    public int RemainingMinutes => (int)Math.Ceiling(Remaining.TotalMinutes);

    /// <summary>
    /// Gets remaining seconds for countdown
    /// </summary>
    public int RemainingSeconds => (int)Remaining.TotalSeconds;
}
