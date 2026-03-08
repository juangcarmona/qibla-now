namespace QiblaNow.Core.Models;

/// <summary>
/// Complete settings for prayer time calculation
/// </summary>
public sealed class PrayerCalculationSettings
{
    public CalculationMethod Method { get; set; }
    public Madhab Madhab { get; set; }
    public HighLatitudeRule HighLatitudeRule { get; set; }
    public int FajrOffsetMinutes { get; set; }
    public int DhuhrOffsetMinutes { get; set; }
    public int AsrOffsetMinutes { get; set; }
    public int MaghribOffsetMinutes { get; set; }
    public int IshaOffsetMinutes { get; set; }

    public PrayerCalculationSettings()
    {
        Method = CalculationMethod.MuslimWorldLeague;
        Madhab = Madhab.Shafi;
        HighLatitudeRule = HighLatitudeRule.SeventhOfNight;
        FajrOffsetMinutes = 0;
        DhuhrOffsetMinutes = 0;
        AsrOffsetMinutes = 0;
        MaghribOffsetMinutes = 0;
        IshaOffsetMinutes = 0;
    }

    /// <summary>
    /// Creates a new schedule with offsets applied
    /// </summary>
    public DailyPrayerSchedule ApplyOffsets(DailyPrayerSchedule source)
    {
        var result = new DailyPrayerSchedule(source.Date, source.TimeZone);

        foreach (var prayer in source.Prayers.OrderBy(p => p.DateTime))
        {
            var offsetMinutes = GetOffsetMinutes(prayer.Type);

            result.Prayers.Add(new PrayerTime(
                prayer.Type,
                prayer.DateTime.AddMinutes(offsetMinutes),
                offsetMinutes));
        }

        return result;
    }

    private int GetOffsetMinutes(PrayerType type)
    {
        return type switch
        {
            PrayerType.Fajr => FajrOffsetMinutes,
            PrayerType.Dhuhr => DhuhrOffsetMinutes,
            PrayerType.Asr => AsrOffsetMinutes,
            PrayerType.Maghrib => MaghribOffsetMinutes,
            PrayerType.Isha => IshaOffsetMinutes,
            _ => 0
        };
    }
}