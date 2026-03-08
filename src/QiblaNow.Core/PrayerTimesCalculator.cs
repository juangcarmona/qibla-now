using QiblaNow.Core.Abstractions;
using QiblaNow.Core.Models;

namespace QiblaNow.Core;

public sealed class PrayerTimesCalculator : IPrayerTimesCalculator
{
    private const double Deg2Rad = Math.PI / 180.0;
    private const double Rad2Deg = 180.0 / Math.PI;

    private static readonly Dictionary<CalculationMethod, (double fajr, double isha)> Methods =
        new()
        {
            { CalculationMethod.MuslimWorldLeague, (18,17) },
            { CalculationMethod.EgyptianGeneralAuthority, (19.5,17.5) },
            { CalculationMethod.ISNA, (15,15) },
            { CalculationMethod.Karachi, (18,18) },
            { CalculationMethod.Kuwait, (17.5,17.5) },
            { CalculationMethod.UmmAlQura, (18.5,0) }
        };

    public Task<DailyPrayerSchedule> CalculateDailyScheduleAsync(
        LocationSnapshot location,
        DateTimeOffset date,
        PrayerCalculationSettings settings)
    {
        double lat = location.Latitude;
        double lng = location.Longitude;

        int day = date.DayOfYear;

        var (decl, eqt) = SolarPosition(day);

        double noon = MidDay(lng, eqt);

        var (fajrAngle, ishaAngle) = Methods[settings.Method];

        double sunrise = SunAngleTime(-0.833, lat, decl, noon, false);
        double sunset  = SunAngleTime(-0.833, lat, decl, noon, true);

        double fajr    = SunAngleTime(-fajrAngle, lat, decl, noon, false);
        double isha;

        if (settings.Method == CalculationMethod.UmmAlQura)
            isha = sunset + (90.0 / 60.0);
        else
            isha = SunAngleTime(-ishaAngle, lat, decl, noon, true);

        double dhuhr = noon;

        double asr = AsrTime(settings.Madhab, lat, decl, noon);

        var schedule = new DailyPrayerSchedule(date, TimeZoneInfo.Utc);

        schedule.Prayers.Add(new PrayerTime(PrayerType.Fajr, ToDate(date, fajr)));
        schedule.Prayers.Add(new PrayerTime(PrayerType.Sunrise, ToDate(date, sunrise)));
        schedule.Prayers.Add(new PrayerTime(PrayerType.Dhuhr, ToDate(date, dhuhr)));
        schedule.Prayers.Add(new PrayerTime(PrayerType.Asr, ToDate(date, asr)));
        schedule.Prayers.Add(new PrayerTime(PrayerType.Maghrib, ToDate(date, sunset)));
        schedule.Prayers.Add(new PrayerTime(PrayerType.Isha, ToDate(date, isha)));

        return Task.FromResult(settings.ApplyOffsets(schedule));
    }

    private static (double declination, double equation) SolarPosition(int day)
    {
        double g = FixAngle(357.529 + 0.98560028 * day);
        double q = FixAngle(280.459 + 0.98564736 * day);

        double L = FixAngle(q + 1.915 * Sin(g) + 0.020 * Sin(2 * g));

        double e = 23.439 - 0.00000036 * day;

        double RA = Atan2(Cos(e) * Sin(L), Cos(L)) / 15.0;

        double decl = Asin(Sin(e) * Sin(L));
        double eqt = q / 15.0 - RA;

        return (decl, eqt);
    }

    private static double MidDay(double lng, double eqt)
    {
        return FixHour(12 - eqt - lng / 15.0);
    }

    private static double SunAngleTime(
        double angle,
        double lat,
        double decl,
        double noon,
        bool afterNoon)
    {
        double term =
            (Sin(angle) - Sin(lat) * Sin(decl)) /
            (Cos(lat) * Cos(decl));

        double t = Acos(term) / 15.0;

        return noon + (afterNoon ? t : -t);
    }

    private static double AsrTime(
        Madhab madhab,
        double lat,
        double decl,
        double noon)
    {
        double factor = madhab == Madhab.Hanafi ? 2 : 1;

        double angle = -Acot(factor + Tan(Math.Abs(lat - decl)));

        return SunAngleTime(angle, lat, decl, noon, true);
    }

    private static DateTimeOffset ToDate(DateTimeOffset date, double hours)
    {
        int h = (int)Math.Floor(hours);
        int m = (int)Math.Round((hours - h) * 60);

        return new DateTimeOffset(
            date.Year,
            date.Month,
            date.Day,
            h,
            m,
            0,
            TimeSpan.Zero);
    }

    private static double FixAngle(double a)
    {
        a = a % 360;
        if (a < 0) a += 360;
        return a;
    }

    private static double FixHour(double h)
    {
        h = h % 24;
        if (h < 0) h += 24;
        return h;
    }

    private static double Sin(double d) => Math.Sin(d * Deg2Rad);
    private static double Cos(double d) => Math.Cos(d * Deg2Rad);
    private static double Tan(double d) => Math.Tan(d * Deg2Rad);

    private static double Asin(double x) => Rad2Deg * Math.Asin(x);
    private static double Acos(double x) => Rad2Deg * Math.Acos(x);
    private static double Atan2(double y, double x) => Rad2Deg * Math.Atan2(y, x);
    private static double Acot(double x) => Rad2Deg * Math.Atan(1 / x);
} 


