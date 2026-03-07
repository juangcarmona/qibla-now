using System.Globalization;

namespace QiblaNow.Core.Models;

/// <summary>
/// Represents a single prayer time with its type, exact time, and offset
/// </summary>
public struct PrayerTime : IEquatable<PrayerTime>
{
    public PrayerType Type { get; }
    public DateTimeOffset DateTime { get; }
    public int OffsetMinutes { get; }

    public PrayerTime(PrayerType type, DateTimeOffset dateTime, int offsetMinutes = 0)
    {
        Type = type;
        DateTime = dateTime;
        OffsetMinutes = offsetMinutes;
    }

    public bool Equals(PrayerTime other) =>
        Type == other.Type &&
        DateTime == other.DateTime &&
        OffsetMinutes == other.OffsetMinutes;

    public override bool Equals(object? obj) => obj is PrayerTime other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(Type, DateTime, OffsetMinutes);

    public override string ToString() =>
        $"{Type}: {DateTime:HH:mm} (offset: {OffsetMinutes:+#;-#;0} min)";

    public string ToShortString() => DateTime.ToString("HH:mm", CultureInfo.InvariantCulture);
}
