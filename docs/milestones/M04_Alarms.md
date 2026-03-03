# M4 — Alarm System

## Decision Locks

* Android AlarmManager
* Exact alarm requested
* Inexact fallback supported
* BroadcastReceivers:

  * BootReceiver
  * TimeChangeReceiver
  * AlarmReceiver
* No foreground service

Locked.

---

## Inputs

* docs/PRD.md (Alarms section)
* docs/NFR.md (Reliability)

---

## Scope

* AlarmPlanService
* AndroidAlarmScheduler
* Notification channel
* Rescheduling triggers
* Alarm settings UI

---

## Non-Scope

* Qibla
* Ads

---

## Implementation Rules

* Stable request codes
* Cancel stale alarms before rescheduling
* No busy background loops
* Show warning if exact alarms denied

---

## Verification

Manual:

* Enable Dhuhr + 10 min reminder
* Confirm scheduled entries
* Reboot emulator → alarms restored
* Change timezone → alarms recalculated

Build:

```
dotnet build
```

---

## Definition of Done

* AL-1 scenario works
* Reboot restore works
* No crashes
* Output token: COMPLETE

---

## Files Allowed

* Infra.Android/Alarms
* Core.Abstractions (IAlarmScheduler)
* App Settings