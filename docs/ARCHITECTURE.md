# Qibla Now – Architecture

## Overview

Qibla Now is an **Android-first, offline-first Islamic utility application** built with **.NET MAUI**.

The architecture prioritizes:

* **Local computation** (no backend)
* **Deterministic prayer time calculations**
* **Minimal platform dependencies**
* **Clear separation between UI and domain logic**

The solution is intentionally **simple and pragmatic**. Only one shared domain library exists (`QiblaNow.Core`). All UI and application logic lives in `QiblaNow.App`.

---

# Solution Structure

```
src/
 ├─ QiblaNow.App
 │   ├─ Pages
 │   ├─ ViewModels
 │   ├─ Resources
 │   ├─ App.xaml
 │   ├─ AppShell.xaml
 │   └─ MauiProgramExtensions.cs
 │
 ├─ QiblaNow.App.Droid
 │   ├─ AndroidManifest.xml
 │   ├─ MainActivity.cs
 │   ├─ MainApplication.cs
 │   └─ MauiProgram.cs
 │
 ├─ QiblaNow.App.iOS
 ├─ QiblaNow.App.Mac
 ├─ QiblaNow.App.WinUI
 │
 └─ QiblaNow.Core
     ├─ Domain
     ├─ Services
     └─ Math
```

Tests:

```
tests/
 ├─ QiblaNow.Core.Tests
 └─ QiblaNow.App.Tests
```

---

# Responsibilities

## QiblaNow.App

Contains **all UI and application logic**.

Responsibilities:

* Pages
* ViewModels
* Navigation
* Dependency Injection
* Settings persistence
* Binding between UI and Core services

Structure:

```
Pages/
  HomePage
  PrayerTimesPage
  QiblaPage
  MapPage
  SettingsPage

ViewModels/
  HomeViewModel
  PrayerTimesViewModel
  QiblaViewModel
  MapViewModel
  SettingsViewModel
```

ViewModels interact with the **Core services** to obtain prayer times and Qibla direction.

No heavy business logic lives here.

---

## QiblaNow.Core

`QiblaNow.Core` contains **pure domain logic**.

This project:

* Has **no MAUI dependencies**
* Has **no platform dependencies**
* Can be tested independently
* Runs in any .NET environment

It contains the **calculation engines** used by the application.

Structure:

```
Domain/
  Coordinates
  PrayerName
  PrayerTime
  CalculationMethod
  Madhab
  HighLatitudeRule
  PrayerCalculationSettings

Services/
  IPrayerTimesCalculator
  PrayerTimesCalculator

  IQiblaCalculator
  QiblaCalculator

  PrayerScheduleService

Math/
  SolarMath
  GeoMath
  AngleHelpers
  ProjectionHelpers (optional map helpers)
```

Responsibilities:

### Prayer Times Engine

Implements deterministic prayer time calculations.

Inputs:

```
date
coordinates
calculation method
madhab
high latitude rule
timezone offset
```

Outputs:

```
Fajr
Sunrise
Dhuhr
Asr
Maghrib
Isha
```

---

### Qibla Engine

Computes the **great-circle bearing to the Kaaba**.

Constant coordinates:

```
Latitude: 21.4225
Longitude: 39.8262
```

Returns:

```
bearing (0-360°)
```

---

## Platform Projects

Platform projects provide **integration with operating system APIs**.

Examples:

### Android (`QiblaNow.App.Droid`)

Responsible for:

* Prayer alarms (AlarmManager)
* Compass sensors (SensorManager)
* Location services
* Notifications
* AdMob integration

These services are exposed to the shared application through interfaces and registered in DI.

---

# Application Architecture

The application follows a **simple MVVM architecture**.

```
Page
  ↓
ViewModel
  ↓
Core Services
```

Example flow:

```
HomePage
  ↓
HomeViewModel
  ↓
PrayerScheduleService
  ↓
PrayerTimesCalculator
```

ViewModels never implement calculation logic themselves.

---

# Dependency Injection

All services are registered during application startup.

Location:

```
QiblaNow.App/MauiProgramExtensions.cs
```

Example:

```csharp
builder.Services.AddSingleton<IPrayerTimesCalculator, PrayerTimesCalculator>();
builder.Services.AddSingleton<IQiblaCalculator, QiblaCalculator>();

builder.Services.AddTransient<HomeViewModel>();
builder.Services.AddTransient<QiblaViewModel>();
builder.Services.AddTransient<MapViewModel>();
builder.Services.AddTransient<PrayerTimesViewModel>();
builder.Services.AddTransient<SettingsViewModel>();
```

Platform services are registered conditionally.

Example (Android):

```
ILocationService → AndroidLocationService
ICompassService → AndroidCompassService
IAlarmService → AndroidAlarmService
```

---

# Navigation Model

Navigation is **Shell-based** with a Home entry point.

Primary routes:

```
Home
Qibla
PrayerTimes
Map
Settings
```

Typical flow:

```
Notification → open Qibla page
User opens app → Home
Home → Qibla / Map / PrayerTimes
Settings → configuration
```

---

# Offline-First Model

All functionality works **without internet connectivity**.

Local data includes:

* user settings
* last known location
* calculation parameters

Persistence is implemented using **Preferences** or local storage.

No backend exists.

---

# Testing Strategy

## Core Tests

Located in:

```
tests/QiblaNow.Core.Tests
```

Tests verify:

* Prayer time calculations
* Qibla direction accuracy
* High latitude edge cases
* Calculation method correctness

These tests do **not require MAUI**.

---

## App Tests

Located in:

```
tests/QiblaNow.App.Tests
```

Focus:

* ViewModel behavior
* Dependency injection wiring
* basic navigation flows

---

# Design Principles

### Deterministic

Prayer calculations must produce the same results for the same inputs.

### Local First

All calculations occur on device.

### Minimal Dependencies

The project avoids unnecessary libraries and frameworks.

### Clear Boundaries

```
UI logic → App
Domain logic → Core
Platform integration → Platform projects
```

---

# Architecture Evolution

The architecture is intentionally minimal for the initial release.

Future changes may include:

* additional calculation methods
* offline map improvements
* enhanced sensor filtering for compass
* advanced alarm scheduling

However, the core design goal remains:

**Simple, reliable, and fully local.**
