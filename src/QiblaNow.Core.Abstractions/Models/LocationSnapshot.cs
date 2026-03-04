namespace QiblaNow.Core.Abstractions.Models;

/// <summary>
/// Represents a location snapshot with coordinates and metadata
/// </summary>
public sealed class LocationSnapshot
{
    public LocationMode Mode { get; }

    public double Latitude { get; }

    public double Longitude { get; }

    /// <summary>
    /// Optional human-readable location label (e.g., city name)
    /// </summary>
    public string? Label { get; }

    /// <summary>
    /// UTC timestamp when this snapshot was captured
    /// </summary>
    public DateTimeOffset Timestamp { get; }

    public LocationSnapshot(LocationMode mode, double latitude, double longitude, string? label = null)
    {
        Mode = mode;
        Latitude = latitude;
        Longitude = longitude;
        Label = label;
        Timestamp = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Validates latitude is within valid range [-90, 90]
    /// </summary>
    public bool IsValidLatitude => Latitude >= -90 && Latitude <= 90;

    /// <summary>
    /// Validates longitude is within valid range [-180, 180]
    /// </summary>
    public bool IsValidLongitude => Longitude >= -180 && Longitude <= 180;

    /// <summary>
    /// Checks if the snapshot coordinates are valid
    /// </summary>
    public bool AreCoordinatesValid => IsValidLatitude && IsValidLongitude;
}
