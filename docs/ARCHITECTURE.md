# Qibla Now – Architecture

## Purpose

Android-first Islamic utility app built with **.NET MAUI**.

Goals:

* deterministic prayer calculations
* clear architectural boundaries
* Android alarm reliability
* privacy-first operation
* minimal complexity

No project-owned backend.

Internet is used only for **external integrations** (maps and ads).

---

# Architecture Model

Three-layer architecture.

```
App → Presentation → Core
```

Rules:

* Core has **no platform dependencies**
* Presentation has **no Android/SDK dependencies**
* App owns **execution and integration**

---

# Solution Structure

```
src/
 ├─ QiblaNow.Core
 │   Models
 │   Services
 │   Math
 │
 ├─ QiblaNow.Presentation
 │   ViewModels
 │   DI
 │
 └─ QiblaNow.App
     Pages
     Services
     Platforms
     Resources
     App.xaml
     AppShell.xaml
     MauiProgram.cs
```

Tests:

```
tests/
 ├─ QiblaNow.Core.Tests
 └─ QiblaNow.Presentation.Tests
```

---

# Core Layer

Semantic anchor: **Pure Domain**

Responsibilities:

* prayer time calculation
* Qibla bearing calculation
* next-prayer resolution
* notification candidate selection
* alarm planning
* reusable math

Constraints:

* no MAUI
* no Android APIs
* no sensors
* no storage
* no Google Maps
* no AdMob

Core defines **what should happen**.

---

# Presentation Layer

Semantic anchor: **UI State**

Contains:

```
HomeViewModel
PrayerTimesViewModel
QiblaViewModel
MapViewModel
SettingsViewModel
```

Responsibilities:

* expose screen state
* expose commands
* translate Core results into UI models
* coordinate use cases

Constraints:

* no Android APIs
* no SDK integrations
* no heavy business logic

Optional DI extension may register **ViewModels only**.

---

# App Layer

Semantic anchor: **Execution + Integration**

Contains:

* MAUI pages
* XAML
* navigation
* dependency injection
* platform implementations
* SDK integrations

Responsibilities:

* composition root
* infrastructure services
* Android system integrations
* external SDK wiring

This layer owns **how things happen**.

---

# External Integrations

## Google Maps

Location: **App layer**

Responsibilities:

* render map
* display markers
* center on user location

Internet required.

Core only provides coordinates/bearings.

---

## AdMob

Location: **App layer**

Responsibilities:

* initialize SDK
* load ads
* display ads

Core and Presentation remain unaware of the SDK.

---

# Alarm System

Semantic anchor: **Plan vs Execute**

### Core

Computes:

* next prayer
* next notification candidate
* next alarm plan

### Android

Executes:

* schedule alarm
* receive alarm
* show notification
* recompute next alarm

Flow:

```
Core → plan next alarm
App → schedule alarm
Android → alarm fires
App → call Core again
```

---

# Device Services

Platform implementations live in **App**.

Examples:

```
LocationService
CompassService
NotificationScheduler
SettingsStore
```

Presentation interacts through these services.

Core never sees platform APIs.

---

# Dependency Direction

Allowed:

```
App → Presentation
App → Core
Presentation → Core
```

Forbidden:

```
Core → App
Core → MAUI
Presentation → Android APIs
```

---

# Application Flows

## Prayer Times

```
Page
 → ViewModel
 → PrayerTimesCalculator
 → UI
```

---

## Qibla / Compass

```
Page
 → ViewModel
 → CompassService + QiblaCalculator
 → UI rotation
```

---

## Alarm Scheduling

Triggers:

* app start
* settings change
* alarm fired
* device reboot
* timezone change

Flow:

```
Core → compute next alarm
App → schedule alarm
Android → trigger
App → recompute
```

Exactly **one alarm scheduled at a time**.

---

## Map

```
MapPage
 → MapViewModel
 → location
 → Google Maps
```

---

# Dependency Injection

Semantic anchor: **Single Composition Root**

Location:

```
QiblaNow.App/MauiProgram.cs
```

Registers:

### Core services

* prayer calculation
* Qibla calculation
* alarm planning

### Presentation

* ViewModels

### Infrastructure

* settings store
* location service
* compass service
* Android alarm scheduler

Avoid duplicate registrations.

---

# Data Model

Stored locally:

* user settings
* calculation parameters
* notification preferences
* last known location (optional)

No project backend.

---

# Privacy Model

Principles:

* calculations local
* no server dependency
* minimal outbound traffic

External traffic may occur through:

* Google Maps
* AdMob

No project-controlled telemetry pipeline required.

---

# Testing

## Core Tests

Location:

```
tests/QiblaNow.Core.Tests
```

Coverage:

* prayer calculation
* high-latitude behavior
* Qibla bearing
* next-prayer resolution
* alarm planning

No MAUI required.

---

## Presentation Tests

Location:

```
tests/QiblaNow.Presentation.Tests
```

Coverage:

* ViewModel state
* commands
* settings changes
* countdown logic

---

# Architectural Principles

**Deterministic Core**

Same input → same output.

---

**Strict Boundaries**

```
Domain → Core
UI → Presentation
Execution → App
```

---

**Single Execution Layer**

Only the App layer interacts with:

* Android APIs
* SDKs
* storage
* sensors
* external providers

---

**Pragmatic Simplicity**

Avoid unnecessary abstractions.

Move code **only when boundaries are violated**.
