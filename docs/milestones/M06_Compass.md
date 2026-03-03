# M6 — Compass Sensors

## Decision Locks

* Use Android SensorManager
* Use accelerometer + magnetometer fusion
* No gyroscope dependency
* Show calibration card when accuracy low

Locked.

---

## Inputs

* docs/PRD.md (Compass section)

---

## Scope

* AndroidCompassService
* Heading + accuracy exposure
* UI delta guidance

---

## Non-Scope

* Prayer
* Ads

---

## Verification

Manual:

* Rotate device → heading updates
* Disable sensor → fallback state

Build must pass.

---

## Definition of Done

* No crash without magnetometer
* Calibration UX shown correctly
* Output token: COMPLETE

---

## Files Allowed

* Infra.Android/Sensors
* Compass ViewModel
* Compass Page
