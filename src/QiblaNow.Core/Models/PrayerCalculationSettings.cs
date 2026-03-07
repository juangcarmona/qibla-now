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

        var fajr = source.GetPrayer(PrayerType.Fajr);
        if (fajr != null)
            result.Prayers.Add(new PrayerTime(PrayerType.Fajr, fajr.DateTime.AddMinutes(FajrOffsetMinutes), FajrOffsetMinutes));

        var dhuhr = source.GetPrayer(PrayerType.Dhuhr);
        if (dhuhr != null)
            result.Prayers.Add(new PrayerTime(PrayerType.Dhuhr, dhuhr.DateTime.AddMinutes(DhuhrOffsetMinutes), DhuhrOffsetMinutes));

        var asr = source.GetPrayer(PrayerType.Asr);
        if (asr != null)
            result.Prayers.Add(new PrayerTime(PrayerType.Asr, asr.DateTime.AddMinutes(AsrOffsetMinutes), AsrOffsetMinutes));

        var maghrib = source.GetPrayer(PrayerType.Maghrib);
        if (maghrib != null)
            result.Prayers.Add(new PrayerTime(PrayerType.Maghrib, maghrib.DateTime.AddMinutes(MaghribOffsetMinutes), MaghribOffsetMinutes));

        var isha = source.GetPrayer(PrayerType.Isha);
        if (isha != null)
            result.Prayers.Add(new PrayerTime(PrayerType.Isha, isha.DateTime.AddMinutes(IshaOffsetMinutes), IshaOffsetMinutes));

        return result;
    }
}
