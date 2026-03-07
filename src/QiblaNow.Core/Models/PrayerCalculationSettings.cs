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
    /// Applies all configured offsets to prayer times
    /// </summary>
    public void ApplyOffsets(DailyPrayerSchedule schedule)
    {
        var fajr = schedule.GetPrayer(PrayerType.Fajr);
        var dhuhr = schedule.GetPrayer(PrayerType.Dhuhr);
        var asr = schedule.GetPrayer(PrayerType.Asr);
        var maghrib = schedule.GetPrayer(PrayerType.Maghrib);
        var isha = schedule.GetPrayer(PrayerType.Isha);

        if (fajr != null) fajr = new PrayerTime(PrayerType.Fajr, fajr.DateTime.AddMinutes(FajrOffsetMinutes), FajrOffsetMinutes);
        if (dhuhr != null) dhuhr = new PrayerTime(PrayerType.Dhuhr, dhuhr.DateTime.AddMinutes(DhuhrOffsetMinutes), DhuhrOffsetMinutes);
        if (asr != null) asr = new PrayerTime(PrayerType.Asr, asr.DateTime.AddMinutes(AsrOffsetMinutes), AsrOffsetMinutes);
        if (maghrib != null) maghrib = new PrayerTime(PrayerType.Maghrib, maghrib.DateTime.AddMinutes(MaghribOffsetMinutes), MaghribOffsetMinutes);
        if (isha != null) isha = new PrayerTime(PrayerType.Isha, isha.DateTime.AddMinutes(IshaOffsetMinutes), IshaOffsetMinutes);

        // Rebuild schedule with offset times
        schedule.Prayers.Clear();
        if (fajr != null) schedule.Prayers.Add(fajr);
        schedule.Prayers.Add(new PrayerTime(PrayerType.Sunrise, fajr.DateTime.AddMinutes(15), 0)); // Sunrise is 15 min after Fajr (approximate)
        if (dhuhr != null) schedule.Prayers.Add(dhuhr);
        if (asr != null) schedule.Prayers.Add(asr);
        if (maghrib != null) schedule.Prayers.Add(maghrib);
        if (isha != null) schedule.Prayers.Add(isha);
    }
}
