# M3 — Prayer Engine + Persistent Notification Scheduling

## Objective

Deliver the first end-to-end vertical slice for prayer times and user-selected prayer notifications.

The app must:

- compute prayer times from user location and settings
- determine the next prayer event relevant to the user
- persist all required configuration and runtime state
- schedule the next notification/alarm so it works even when the app is closed
- restore scheduling when the app is opened again
- restore scheduling after device reboot on supported platforms

This milestone targets Android-first behavior, while keeping domain and application logic platform-independent.

---

## Decision Locks

- Prayer calculation engine is pure .NET and platform-independent
- Internal temporal representation uses DateTimeOffset
- TimeZoneInfo is used for conversions
- Kaaba coordinates are locked to (place it in a constant in CORE library):
  - 21.422487, 39.826206
- Rounding rule: nearest minute
- Offsets are applied before rounding
- ViewModels belong to QiblaNow.Presentation
- Core must remain independent from MAUI and platform APIs
- Notification scheduling must be abstracted behind platform-neutral interfaces
- Android implementation must not rely on a permanently running in-process background loop
- Scheduling model is “compute next event -> schedule one next trigger -> recalculate and reschedule”
- Default notification state may be fully disabled
- No decision lock may be changed during the milestone

---

## Inputs

- docs/PRD.md
- docs/ACCEPTANCE_TESTS.md
- current solution structure
- current settings mechanism

---

## Product Clarifications Locked For This Milestone

### Prayer events

The daily schedule may include solar events such as Sunrise, but “next prayer” and notification scheduling operate on user-relevant prayer events only.

At minimum, the configurable prayer events are:

- Fajr
- Dhuhr
- Asr
- Maghrib
- Isha

Sunrise is not treated as a prayer notification target unless the PRD explicitly says otherwise.

### Notification behavior

- Users can enable or disable notifications per prayer using simple checkbox/toggle settings
- Default values may be all disabled
- Notification enablement is persisted in settings
- The app schedules only the next due enabled prayer notification
- When that notification fires, the app recalculates and schedules the following one
- If all prayer notifications are disabled, no future alarm is scheduled

### Location behavior

- Prayer time calculation depends on location and calculation settings
- Scheduling uses the best available persisted context
- Last known valid location must be persisted and reused if live location is unavailable at scheduling time
- If no valid location exists, scheduling must fail safely and expose a diagnosable state

### Lifecycle behavior

- When app starts, resumes, or settings change, scheduling state is reconciled
- On Android, scheduling should be restored after device reboot
- The design must tolerate process death; correctness must not depend on an always-alive app process

---

## Scope

### 1. QiblaNow.Core

Implement pure domain/application logic for:

- PrayerTimesCalculator
- prayer time models
- calculation settings models
- next-prayer resolution
- next-notification-candidate resolution
- daily schedule resolution
- countdown target resolution
- persisted scheduling state model if needed

### 2. QiblaNow.Core.Tests

Implement and encode acceptance and behavior tests for:

- PT-1 London
- PT-2 Reykjavik
- next prayer resolution
- post-Isha rollover to next-day Fajr
- disabled-prayer filtering for notification selection
- settings-driven notification candidate selection

Acceptance vectors must be explicit in tests.

### 3. QiblaNow.Presentation

Implement:

- PrayerTimesViewModel
- real computed prayer schedule binding
- next prayer exposure
- countdown updating every second while page is active
- settings-facing models/commands for prayer notification toggles
- state exposure for scheduling diagnostics if needed

### 4. Settings Integration

Extend the current settings mechanism to persist, at minimum:

- prayer calculation method/settings required by the PRD
- per-prayer notification enabled flags
- any prayer offsets required by the PRD
- last known valid location needed for scheduling fallback
- any scheduling metadata strictly required to reconcile next pending event

Do not introduce redundant wrapper abstractions around the existing settings mechanism.

### 5. Platform Scheduling Abstraction

Define platform-neutral interfaces for scheduling the next prayer notification/alarm, for example:

- schedule next prayer notification
- cancel scheduled prayer notifications
- restore/reconcile schedule on app startup
- react to fired notification/alarm event
- react to device reboot where supported

These interfaces must live outside the platform-specific implementation boundary.

### 6. Android Implementation

Implement Android-specific scheduling so that:

- the next enabled prayer can trigger even when the app is closed
- scheduling does not depend on a continuously running process
- the next prayer is recalculated and rescheduled after a trigger
- scheduling can be restored after reboot on Android
- implementation remains replaceable and isolated from Core

---

## Non-Scope

- Qibla compass
- ad integration
- cross-platform parity guarantees for background execution
- cloud sync
- server-side scheduling
- wearable integrations
- advanced notification UX beyond basic correctness
- geofencing or continuous background location tracking

---

## Architecture Rules

- Domain logic stays out of MAUI pages
- Core contains no Android, iOS, or MAUI APIs
- Presentation contains no platform scheduling implementation
- Platform-specific scheduling must consume persisted settings/state, not hidden in-memory state
- Do not rely on a forever-running service/thread/timer
- Reconciliation logic must be idempotent
- Scheduling must tolerate duplicated startup/recovery calls safely
- Any Android boot recovery wiring must be explicit and minimal
- Only store what is necessary for deterministic recovery

---

## Verification

Run:

```bash
dotnet test
````

Must pass:

* PT-1 London
* PT-2 Reykjavik
* next prayer behavior tests
* notification candidate selection tests

Manual verification must demonstrate:

* PrayerTimes page shows real computed values
* Countdown updates each second
* User can enable/disable notifications per prayer from Settings
* Closing the app does not lose the next scheduled prayer notification
* Reopening the app reconciles scheduling state correctly
* On Android, reboot recovery path is implemented

Tolerance for prayer time acceptance vectors:

* ±1 minute

---

## Definition of Done

* All tests green
* PrayerTimes page shows real computed values
* Next prayer resolution is correct
* Countdown updates correctly
* Prayer notification toggles exist in Settings and persist correctly
* Android schedules the next enabled prayer without depending on an always-alive process
* Scheduling is reconciled on app startup
* Android reboot recovery path exists
* Output token: COMPLETE

---

## Files Allowed

* QiblaNow.Core
* QiblaNow.Core.Tests
* QiblaNow.Presentation
* QiblaNow.App only for app wiring
* Android platform project/files only where required for scheduling and boot recovery wiring

---

## Working Mode

1. Inspect the current solution structure and existing settings mechanism
2. Produce a short implementation plan
3. Implement incrementally
4. Keep all milestone decisions locked
5. Do not invent requirements outside this file
6. Output token at the end: COMPLETE