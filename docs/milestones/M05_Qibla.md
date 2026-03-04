# M5 — Qibla Bearing Engine

## Decision Locks

* Great-circle initial bearing
* True north reference
* Output range: 0–359°
* Precision: 1 decimal

Locked.

---

## Inputs

* docs/ACCEPTANCE_TESTS.md (QB-1, QB-2)

---

## Scope

* QiblaCalculator (pure .NET)
* Numeric bearing display in Qibla page
* Unit tests for London and NYC

---

## Non-Scope

* Sensors
* Compass rotation
* Map integration

---

## Implementation Rules

* Calculation implemented in the Core project
* Use great-circle initial bearing formula
* Reference point fixed to Kaaba coordinates:

  21.422487, 39.826206

* Core must remain platform-independent
* UI must not contain calculation logic

---

## Verification

```

dotnet test

```

Must pass:

* QB-1 London
* QB-2 New York

Tolerance:

±2°

---

## Definition of Done

* All QB tests pass
* Qibla page displays numeric bearing
* Arrow indicator points to bearing value
* Output token: COMPLETE

---

## Files Allowed

* QiblaNow.Core (QiblaCalculator)
* QiblaNow.Core.Tests
* QiblaNow.App (QiblaViewModel and page)
