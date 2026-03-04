# M3 — Prayer Engine (Pure .NET)

## Decision Locks

* Engine is pure .NET (no platform bindings)
* Kaaba coords locked:

  * 21.422487, 39.826206

* Methods enum exactly as spec
* Rounding: nearest minute
* Offset applied before rounding

No changes allowed during milestone.

---

## Inputs

* docs/PRD.md (Prayer section)
* docs/ACCEPTANCE_TESTS.md (PT-1, PT-2)

---

## Scope

* PrayerTimesCalculator
* CalculationSettings model
* PT-1 + PT-2 unit tests
* PrayerTimes page displays daily schedule + countdown

---

## Non-Scope

* Alarm scheduling
* Qibla compass
* Ads

---

## Implementation Rules

* Internal representation: DateTimeOffset
* TimeZoneInfo used for conversions
* No UI logic inside Core project
* Tests must encode acceptance vectors
* Core must remain independent from MAUI and platform APIs

---

## Verification

```

dotnet test

```

Must pass:

* PT-1 London
* PT-2 Reykjavik

Tolerance:
±1 minute

---

## Definition of Done

* All tests green
* PrayerTimes page shows real computed values
* Countdown updates each second
* Output token: COMPLETE

---

## Files Allowed

* QiblaNow.Core (PrayerTimesCalculator and models)
* QiblaNow.Core.Tests
* QiblaNow.App (PrayerTimesViewModel and page)
