namespace QiblaNow.Core.Models;

public enum LocationMode
{
    /// <summary>
    /// Automatically determine location via GPS
    /// </summary>
    GPS = 0,

    /// <summary>
    /// Manually specify latitude and longitude
    /// </summary>
    Manual = 1,
}
