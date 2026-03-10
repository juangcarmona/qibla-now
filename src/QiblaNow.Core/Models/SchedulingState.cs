namespace QiblaNow.Core.Models;

/// <summary>
/// Persisted metadata required for deterministic alarm reconciliation after
/// process death, device reboot, or timezone change. Corresponds to the
/// scheduling.* keys defined in DATA_MODEL.md.
/// </summary>
public sealed class SchedulingState
{
    /// <summary>The prayer type name of the last planned alarm ("Fajr", "Dhuhr", etc.)</summary>
    public string? LastPlannedPrayer { get; set; }

    /// <summary>UTC trigger time as Unix milliseconds of the last planned alarm.</summary>
    public long? LastPlannedTriggerUtc { get; set; }

    /// <summary>AlarmManager request code used when setting the last alarm.</summary>
    public int? LastPlannedRequestCode { get; set; }

    /// <summary>UTC time as Unix milliseconds when reconciliation last ran.</summary>
    public long? LastReconciledUtc { get; set; }

    /// <summary>Returns the planned trigger as a DateTimeOffset, or null if not set.</summary>
    public DateTimeOffset? PlannedTriggerTime =>
        LastPlannedTriggerUtc.HasValue
            ? DateTimeOffset.FromUnixTimeMilliseconds(LastPlannedTriggerUtc.Value)
            : null;

    /// <summary>True when there is a non-expired alarm on record.</summary>
    public bool HasActivePlan(DateTimeOffset now) =>
        PlannedTriggerTime.HasValue && PlannedTriggerTime.Value > now;
}
