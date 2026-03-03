# M7 — Offline Map Rendering

## Decision Locks

* Use GraphicsView
* Equirectangular projection
* Static bitmap only
* No MAUI Maps

Locked.

---

## Scope

* Bundle world map image
* Implement projection
* Draw markers + bearing line

---

## Verification

Manual:

* Change manual location
* Markers move correctly

Build must pass.

---

## Definition of Done

* Fully offline
* No network calls
* Output token: COMPLETE

---

## Files Allowed

* Map Page
* GraphicsView logic
* Assets
