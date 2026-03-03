# M1 — MAUI Shell + DI Foundation

## Decision Locks

* Architecture: MVVM + CommunityToolkit.Mvvm
* DI: Microsoft.Extensions.DependencyInjection
* Navigation: Shell with 3 tabs (Times, Compass, Map)
* No business logic yet
* Android target only

These decisions cannot change inside M1.

---

## Inputs

* docs/VISION.md
* docs/ARCHITECTURE.md (Stack + Solution Structure sections)

---

## Scope

* Create MAUI solution structure
* Configure DI container
* Create:

  * AppShell with 3 tabs
  * Placeholder pages
  * Placeholder ViewModels
* Wire ViewModels via DI

---

## Non-Scope

* Prayer logic
* Location
* Alarms
* Ads
* Sensors
* Map rendering

---

## Implementation Rules

* No code-behind logic
* All ViewModels inherit from ObservableObject
* No static service singletons outside DI
* App builds with zero errors

---

## Verification

Run every iteration:

```
dotnet build
```

Expected:

* Build succeeds
* No errors

---

## Definition of Done

* Shell renders 3 tabs
* Each tab binds to its ViewModel
* DI resolves ViewModels correctly
* Output token: COMPLETE

---

## Files Allowed to Change

* src/QiblaNow.App/*
* src/QiblaNow.Core.Abstractions/* (if needed)