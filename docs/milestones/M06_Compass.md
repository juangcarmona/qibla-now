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
* Qibla page rotates arrow according to heading
* Calibration guidance UI

---

## Non-Scope

* Prayer time calculation
* Ads
* Map features

---

## Implementation Rules

* Sensor integration implemented only in the Android project
* Core project must remain platform-independent
* Heading reported in degrees (0–359°)
* ViewModel translates heading + Qibla bearing into arrow rotation
* No continuous background services

---

## Verification

Manual:

* Rotate device → heading updates
* Move device in figure-8 → calibration improves
* Disable sensor (emulator / device without magnetometer) → fallback state shown

Build must pass.

---

## Definition of Done

* No crash without magnetometer
* Heading updates smoothly
* Calibration UX shown when accuracy is low
* Qibla arrow rotates correctly
* Output token: COMPLETE

---

## Files Allowed

* QiblaNow.App.Droid (SensorManager integration)
* QiblaNow.App (QiblaViewModel and page)
