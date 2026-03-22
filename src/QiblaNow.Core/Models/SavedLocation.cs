namespace QiblaNow.Core.Models;

public sealed class SavedLocation
{
    public string Name { get; }
    public double Latitude { get; }
    public double Longitude { get; }
    public DateTimeOffset LastUsedUtc { get; }

    public SavedLocation(string name, double latitude, double longitude, DateTimeOffset lastUsedUtc)
    {
        Name = name;
        Latitude = latitude;
        Longitude = longitude;
        LastUsedUtc = lastUsedUtc;
    }
}
