# Architecture — Qibla Now (.NET MAUI Android)

## Stack

- .NET MAUI
- MVVM
- CommunityToolkit.Mvvm
- Microsoft.Extensions.DependencyInjection
- Pure .NET Core engines
- Android AlarmManager
- AdMob SDK (Android)

---

## Solution Structure

qibla-now/

src/
- QiblaNow.App/
- QiblaNow.Core.Prayer/
- QiblaNow.Core.Qibla/
- QiblaNow.Core.Abstractions/
- QiblaNow.Infra/
- QiblaNow.Infra.Android/

tests/
- QiblaNow.Core.Tests/

---

## MVVM Pattern

- Each tab has ViewModel
- ViewModels use ObservableObject
- No code-behind logic

---

## Dependency Injection

Configured in MauiProgram:

Singleton:
- PrayerTimesService
- QiblaService
- SettingsStore
- AlarmPlanService
- AdBannerService

Transient:
- ViewModels

---

## Core Engines

### QiblaNow.Core.Prayer

- Deterministic prayer time engine
- Computes PrayerTimesDay
- Fully unit-tested

### QiblaNow.Core.Qibla

- Great-circle initial bearing
- Kaaba constant:
  Lat: 21.422487
  Lon: 39.826206

---

## Android Infrastructure

### Alarm System

- AlarmManager
- BroadcastReceivers:
  - BootReceiver
  - TimeChangeReceiver
  - AlarmReceiver
- WorkManager daily repair

Exact alarm permission:
- Request capability
- Fallback gracefully

---

### Sensors

- SensorManager
- Accelerometer + magnetometer
- Expose heading + accuracy

---

### Map Rendering

- MAUI GraphicsView
- Static world map bitmap
- Custom projection

---

### Ads

Abstraction:
- IAdBannerService

Android implementation:
- AdMob SDK
- NPA-only requests
- UMP consent flow
- One banner per tab
- No app-driven refresh loops

Ads may show even without location configured.