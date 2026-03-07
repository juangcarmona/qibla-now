namespace QiblaNow.Core.Models;

/// <summary>
/// Rules for handling high latitude areas where night is very long or very short
/// </summary>
public enum HighLatitudeRule
{
    /// <summary>
    /// Seventh of the night - Fajr at 1/7 of night duration
    /// </summary>
    SeventhOfNight = 0,

    /// <summary>
    /// Middle of the night - Fajr at middle of night
    /// </summary>
    MiddleOfNight = 1,

    /// <summary>
    /// One seventh of the night - Fajr at 1/7 of night duration
    /// </summary>
    OneSeventh = 2
}
