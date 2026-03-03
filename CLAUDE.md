# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Qibla Now is an Android-first, offline-first Islamic utility app built with .NET MAUI. All calculations are performed locally on-device. The project follows a strict spec-driven development approach where architecture and milestones are defined before implementation.

## Tech Stack

- .NET MAUI
- MVVM with CommunityToolkit.Mvvm
- Microsoft.Extensions.DependencyInjection
- Pure .NET Core calculation engines
- Android native integrations (AlarmManager, SensorManager, AdMob)

## Project Structure

```
qibla-now/
├── src/                          # App projects
│   ├── QiblaNow.App/             # MAUI shell and views
│   ├── QiblaNow.Core.Prayer/     # Prayer time calculation engine
│   ├── QiblaNow.Core.Qibla/      # Qibla direction calculation engine
│   ├── QiblaNow.Core.Abstractions/ # Core interfaces
│   ├── QiblaNow.Infra/           # Shared infrastructure
│   └── QiblaNow.Infra.Android/   # Android-specific infrastructure
└── docs/                         # Specification documents
```

## Architecture Principles

### Spec-Driven Development
- Milestones must be completed strictly in order (see MILESTONES_INDEX.md)
- Each milestone has an allowlist of files it can modify
- A milestone is complete only when it outputs `<promise>COMPLETE</promise>`
- If blocked, resolve before proceeding to the next milestone

### Dependency Injection (DI)
- Configure in `MauiProgram.cs`
- Singleton services: `PrayerTimesService`, `QiblaService`, `SettingsStore`, `AlarmPlanService`, `AdBannerService`
- ViewModels are transient

### MVVM Pattern
- No code-behind logic
- ViewModels inherit from `ObservableObject`
- Each tab has a dedicated ViewModel

### Core Engines
- **Prayer Engine**: Deterministic prayer times using pure .NET
- **Qibla Engine**: Great-circle initial bearing calculations
- Both engines must be fully unit-tested

### Android Integrations
- **Alarm System**: Exact alarms via `AlarmManager` with fallback to inexact; handles boot/time-change/timezone changes
- **Sensors**: `SensorManager` for accelerometer + magnetometer; expose heading and accuracy
- **Map**: Static world map bitmap with custom projection (no online tiles)
- **Ads**: `AdMob` SDK with NPA-only requests, UMP consent flow, one banner per tab

## Development Commands

### Build
```bash
dotnet build
```

### Run on Android
```bash
dotnet run --platform android
```

### Test Projects
```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test /p:Project="/home/juan/dev/qibla-now/tests/QiblaNow.Core.Tests"
```

## Key Constraints

### Performance
- Cold start ≤ 1.0s
- Prayer recomputation ≤ 200ms
- No frame > 32ms during navigation

### Privacy
- No analytics, telemetry, crash reporting, or cloud services
- Location used only locally
- No personal data transmitted
- All features work offline; ads require network but fail gracefully

### Formatting
- DateTimeOffset for all time values
- TimeZoneInfo for conversions
- Display via `CurrentUICulture`
- Degrees: 0–359.0 with 1 decimal

### Accessibility
- 48dp minimum touch targets
- Content descriptions on all icons
- High contrast colors
- No color-only state encoding

## Milestone Dependencies

```
M01 Shell/DI → M02 Location → M03 PrayerEngine → M04 Alarms →
M05 Qibla → M06 Compass → M07 Map → M08 i18n → M09 Ads → M10 Store
```

Each milestone file in `docs/milestones/` defines specific requirements and deliverables.
