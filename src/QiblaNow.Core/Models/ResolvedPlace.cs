namespace QiblaNow.Core.Models;

public sealed class ResolvedPlace
{
    public double Latitude { get; }
    public double Longitude { get; }
    public string Language { get; }
    public string? Locality { get; }
    public string? Sublocality { get; }
    public string? AdminArea { get; }
    public string? Country { get; }
    public string? FormattedAddress { get; }

    public ResolvedPlace(
        double latitude,
        double longitude,
        string language,
        string? locality = null,
        string? sublocality = null,
        string? adminArea = null,
        string? country = null,
        string? formattedAddress = null)
    {
        Latitude = latitude;
        Longitude = longitude;
        Language = language;
        Locality = locality;
        Sublocality = sublocality;
        AdminArea = adminArea;
        Country = country;
        FormattedAddress = formattedAddress;
    }
}
