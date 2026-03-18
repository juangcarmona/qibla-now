using QiblaNow.Core.Models;

namespace QiblaNow.App.Platforms.Android;

/// <summary>
/// Thread-safe static holder for a prayer-alert navigation request.
/// Used when the alarm fires while the app is not in the foreground:
/// the <see cref="PrayerAlarmReceiver"/> sets a pending prayer type here,
/// and <see cref="MainActivity"/> reads and clears it once the Shell is ready.
/// </summary>
internal static class PrayerNavigationRequest
{
    private static volatile int _pendingPrayerType = -1;

    /// <summary>Extra key written into the notification content intent.</summary>
    internal const string ExtraPrayerAlert = "prayer_alert";

    /// <summary>Extra key for the prayer type int value.</summary>
    internal const string ExtraPrayerType = "prayer_type";

    public static void Set(PrayerType prayerType)
    {
        _pendingPrayerType = (int)prayerType;
        System.Diagnostics.Debug.WriteLine($"PrayerNavigationRequest: pending set to {prayerType}");
    }

    /// <summary>
    /// Returns the pending prayer type (if any) and clears the pending value atomically.
    /// </summary>
    public static PrayerType? TakeAndClear()
    {
        var val = System.Threading.Interlocked.Exchange(ref _pendingPrayerType, -1);
        return val < 0 ? null : (PrayerType)val;
    }
}
