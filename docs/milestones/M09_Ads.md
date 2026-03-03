# M9 — AdMob Integration

## Decision Locks

* Banner only
* NPA-only requests
* UMP consent flow
* One banner per tab
* No app-side frequency caps

Locked.

---

## Scope

* IAdBannerService
* Android AdMob implementation
* Banner placement in 3 tabs

---

## Non-Scope

* Mediation
* Interstitial
* Rewarded

---

## Verification

Manual:

* Banner appears
* No crash offline
* Does not show during permission dialogs
* Does not block alarm UI

Build must pass.

---

## Definition of Done

* Ads render correctly
* Privacy constraints respected
* Output token: COMPLETE

---

## Files Allowed

* Infra.Android/Ads
* App UI
* DI wiring