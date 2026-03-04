# M9 — AdMob Integration

## Decision Locks

* Banner ads only
* NPA-only requests (non-personalized ads)
* UMP consent flow
* One banner per page
* No app-side frequency caps

Locked.

---

## Inputs

* docs/ADS_POLICY.md

---

## Scope

* IAdBannerService interface
* Android AdMob implementation
* Banner placement on key pages

Banner locations:

* PrayerTimesPage
* QiblaPage
* MapPage

---

## Non-Scope

* Mediation
* Interstitial ads
* Rewarded ads
* Analytics or tracking

---

## Implementation Rules

* Android integration implemented only in the Android project
* App project exposes banner placeholders in UI
* Ads must respect privacy policy (NPA only)
* Ads must not block core interactions
* Ads must not depend on location data

---

## Verification

Manual:

* Banner renders on supported pages
* App does not crash when offline
* Banner does not appear during permission dialogs
* Banner does not block alarm UI

Build must pass.

---

## Definition of Done

* Ads render correctly
* Privacy constraints respected
* App remains fully functional without ads
* Output token: COMPLETE

---

## Files Allowed

* QiblaNow.App.Droid (AdMob integration)
* QiblaNow.App (UI placement and DI wiring)