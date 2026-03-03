# M5 — Qibla Bearing Engine

## Decision Locks

* Great-circle initial bearing
* True north reference
* Output range: 0–359
* Precision: 1 decimal

Locked.

---

## Inputs

* docs/ACCEPTANCE_TESTS.md (QB-1, QB-2)

---

## Scope

* QiblaService
* Bearing mode UI
* Unit tests for London + NYC

---

## Non-Scope

* Sensors

---

## Verification

```
dotnet test
```

Tolerance:
±2°

---

## Definition of Done

* QB tests pass
* Bearing UI renders numeric + arrow
* Output token: COMPLETE

---

## Files Allowed

* Core.Qibla
* Core.Tests
* Compass ViewModel