namespace QiblaNow.Core.Models;

/// <summary>
/// Represents a location snapshot with coordinates and metadata
/// </summary>
public sealed class LocationSnapshot
{
    public LocationMode Mode { get; }
    public double Latitude { get; }
    public double Longitude { get; }
    public string? Label { get; }
    public DateTimeOffset Timestamp { get; }

    public LocationSnapshot(
        LocationMode mode,
        double latitude,
        double longitude,
        DateTimeOffset timestamp,
        string? label = null)
    {
        if (latitude < -90 || latitude > 90)
            throw new ArgumentOutOfRangeException(nameof(latitude), "Latitude must be between -90 and 90.");

        if (longitude < -180 || longitude > 180)
            throw new ArgumentOutOfRangeException(nameof(longitude), "Longitude must be between -180 and 180.");

        Mode = mode;
        Latitude = latitude;
        Longitude = longitude;
        Label = label;
        Timestamp = timestamp;
    }

    public bool IsValidLatitude => Latitude >= -90 && Latitude <= 90;
    public bool IsValidLongitude => Longitude >= -180 && Longitude <= 180;
    public bool AreCoordinatesValid => true;
}