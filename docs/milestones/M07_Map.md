# M7 — Offline Map Rendering

## Decision Locks

* Use GraphicsView
* Equirectangular projection
* Static bitmap only
* No MAUI Maps
* No network calls

Locked.

---

## Inputs

* docs/PRD.md (Map section)

---

## Scope

* Bundle world map image asset
* Implement simple equirectangular projection
* Render current location marker
* Render Kaaba marker
* Draw bearing line between both points

---

## Non-Scope

* Interactive maps
* GPS tracking
* Online tiles
* Navigation integration

---

## Implementation Rules

* Map rendering implemented entirely in the App project
* Core provides coordinates, Qibla bearing, and optional projection helpers.
* Projection math must remain deterministic
* Rendering must work fully offline

---

## Verification

Manual:

* Change manual location
* Map marker updates correctly
* Kaaba marker remains fixed
* Bearing line connects both points

Build must pass.

---

## Definition of Done

* Map renders correctly offline
* Location and Kaaba markers displayed
* Bearing line drawn correctly
* No network calls
* Output token: COMPLETE

---

## Files Allowed

* QiblaNow.App (MapPage, GraphicsView renderer, MapViewModel)
* QiblaNow.App Resources (map image asset)
* QiblaNow.Core (optional projection helpers)