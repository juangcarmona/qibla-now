# M10 — Store Readiness + Privacy Verification

## Decision Locks

* No telemetry
* Privacy statement included
* Play Store compliance reviewed

Locked.

---

## Inputs

* docs/PRD.md
* docs/NFR.md
* docs/ADS_POLICY.md

---

## Scope

* Add in-app privacy screen
* Review application permissions
* Verify platform manifest entries
* Remove debug logging
* Produce Release build

---

## Non-Scope

* Feature changes
* UI redesign
* New integrations

---

## Implementation Rules

* No analytics or tracking libraries allowed
* Permissions must match actual features only
* Location permission used only for prayer calculation
* Ads must follow NPA policy
* Core logic must remain offline-first

---

## Verification

Checklist:

* No analytics packages in dependencies
* No unnecessary permissions declared
* Release build succeeds
* Manual smoke test on device
* Privacy screen accessible from Settings

Build:

```

dotnet build -c Release

```

---

## Definition of Done

* Release build succeeds
* Privacy statement present in app
* Manifest permissions validated
* App passes manual smoke test
* Output token: COMPLETE

---

## Files Allowed

* QiblaNow.App (privacy page, settings)
* QiblaNow.App.Droid (manifest verification)
* QiblaNow.App.iOS / WinUI / Mac (manifest checks if needed)

