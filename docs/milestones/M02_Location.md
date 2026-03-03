# M2 — Location System

## Decision Locks

* LocationMode enum: GPS | Manual
* No background tracking
* Manual lat/lon validation enforced
* Storage via Preferences wrapper

Locked for this milestone.

---

## Inputs

* docs/PRD.md (Location section)
* docs/DATA_MODEL.md (Location fields)

---

## Scope

* ILocationService abstraction
* Android GPS implementation (single-shot request)
* Manual location entry page
* Store last snapshot
* Times tab displays location label

---

## Non-Scope

* Prayer computation
* Alarms
* Qibla
* Ads

---

## Implementation Rules

* No continuous GPS updates
* Manual coordinates validated strictly:

  * Lat: -90..90
  * Lon: -180..180
* All persistence via ISettingsStore

---

## Verification

```
dotnet build
dotnet test
```

Unit tests required:

* Manual coordinate validation
* Snapshot persistence

---

## Definition of Done

* Manual location works offline
* GPS single-shot request works
* Location state persists
* Output token: COMPLETE

---

## Files Allowed

* Core.Abstractions (ILocationService)
* Infra
* Infra.Android
* App ViewModels
* Tests