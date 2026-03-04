# M01 — App Foundation

<promise>COMPLETE</promise>

## Implementation Summary

Established the initial application foundation and project structure.

The solution now contains:

- **QiblaNow.App** – Main MAUI application (UI, pages, view models, navigation)
- **QiblaNow.App.\*** – Platform entry points (Android, iOS, Mac, WinUI)
- **QiblaNow.Core** – Pure calculation logic (prayer times, qibla direction)
- **Tests projects** – Basic test scaffolding for App and Core

This milestone establishes the base architecture on which the rest of the features will be built.

## Key Features Implemented

1. **Application Shell**

   - MAUI Shell configured
   - Root navigation structure established

2. **Page Structure**

   Initial pages created:

   - Home
   - Prayer Times
   - Qibla
   - Map
   - Settings

3. **ViewModel Layer**

   - ViewModels defined for each page
   - Clear separation between UI and logic

4. **Dependency Injection**

   - Application services and ViewModels registered
   - Pages resolve dependencies through DI

5. **Core Library**

   - Dedicated project for calculation logic
   - No platform dependencies

## Outcome

The application starts successfully and displays the initial UI.

Navigation, page structure, dependency injection, and the core project layout are now in place.

This provides a stable base for implementing the functional milestones that follow.

## Next Step

M02 — Location acquisition and persistence.