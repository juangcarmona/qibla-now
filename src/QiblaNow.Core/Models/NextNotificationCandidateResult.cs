namespace QiblaNow.Core.Models;

/// <summary>
/// Result of calculating the next notification candidate
/// This is used to determine what prayer to schedule an alarm for
/// </summary>
public sealed class NextNotificationCandidateResult
{
    public PrayerType Type { get; }
    public DateTimeOffset ScheduledTime { get; }

    public NextNotificationCandidateResult(PrayerType type, DateTimeOffset scheduledTime)
    {
        Type = type;
        ScheduledTime = scheduledTime;
    }

    /// <summary>
    /// Gets scheduled time as formatted string
    /// </summary>
    public string GetFormattedTime() => ScheduledTime.ToString("HH:mm", System.Globalization.CultureInfo.InvariantCulture);
}
