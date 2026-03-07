namespace QiblaNow.Core.Models;

/// <summary>
/// Result of calculating the countdown target
/// </summary>
public sealed class CountdownTargetResult
{
    public PrayerType Type { get; }
    public DateTimeOffset TargetTime { get; }
    public int RemainingSeconds { get; }

    public CountdownTargetResult(PrayerType type, DateTimeOffset targetTime, int remainingSeconds)
    {
        Type = type;
        TargetTime = targetTime;
        RemainingSeconds = remainingSeconds;
    }

    /// <summary>
    /// Gets remaining seconds for countdown display
    /// </summary>
    public int RemainingSecondsFormatted => RemainingSeconds;

    /// <summary>
    /// Gets remaining minutes for display
    /// </summary>
    public int RemainingMinutesFormatted => RemainingSeconds / 60;
}
