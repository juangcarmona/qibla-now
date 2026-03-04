# Qibla Now – Product Requirements Document (PRD)

Version: 1.1  
Status: Active  
Scope: MVP (Android-first, offline-first)

---

# 1. Product Overview

**Qibla Now** is a lightweight Islamic utility mobile application designed to help Muslims:

- Know **when** to pray
- Know **where** to face (Qibla)
- Receive **prayer notifications**
- Access prayer information **instantly and offline**

The application is designed for **speed, reliability, and minimal distraction**.

Core principles:

- Offline-first
- No user tracking
- No accounts
- No analytics
- No cloud dependency
- Fast launch (<1 second)

The app should feel like a **tool**, not a social platform.

---

# 2. Target Platform

Primary platform:

- **Android**

Secondary platforms (same codebase via MAUI):

- iOS
- Mac Catalyst
- Windows

However:

- **Android is the only platform guaranteed in MVP**
- Other platforms are best-effort

---

# 3. Design Philosophy

The app is optimized for **real-life prayer situations**.

Typical user scenario:

1. User hears the prayer notification.
2. User opens the app.
3. The app immediately shows **what matters now**.

This leads to the following UX principles:

- Minimal navigation
- One primary screen
- Fast access to Qibla
- Clear prayer schedule
- Large readable elements
- One-handed usability

---

# 4. Navigation Structure

The app uses **a Home-centered navigation model**, not fixed tabs.

## Root Screen

**Home ("Prayer Now")**

Displays:

- Next prayer
- Countdown to next prayer
- Quick actions for:
  - Qibla
  - Map
  - Prayer Times
  - Settings

The Home screen is the **default landing screen**.

## Pages

The application contains the following main pages:

### HomePage

Primary dashboard.

Shows:

- Next prayer name
- Next prayer time
- Countdown
- Today's prayer schedule (summary)
- Location and calculation method
- Quick actions

Quick actions:

- Open Qibla
- Open Map
- Open Prayer Times
- Open Settings

---

### QiblaPage

Shows the **direction to the Kaaba**.

Features:

- Compass with Qibla bearing
- Numeric bearing (degrees)
- Visual alignment indicator

Requirements:

- Works offline
- Uses device sensors
- No GPS required if last location exists

---

### PrayerTimesPage

Shows the **complete daily prayer schedule**.

Displays:

- Fajr
- Sunrise
- Dhuhr
- Asr
- Maghrib
- Isha

Additional features:

- Current prayer highlight
- Next prayer highlight
- Countdown indicator

---

### MapPage

Shows the **user location and Qibla direction on a map**.

Features:

- User location marker
- Qibla direction line
- Kaaba reference direction
- Zoom / pan

Map requirements:

- Minimal dependencies
- No external tracking

---

### SettingsPage

Allows configuration of:

- Calculation method
- Madhab (Asr method)
- High latitude rule
- Notification enable/disable
- Notification offsets

Settings are stored **locally only**.

---

# 5. Notification Behaviour

Prayer notifications:

- Triggered locally
- Scheduled using platform alarms
- Do not require internet

Each prayer can trigger:

- Notification
- Optional sound
- Optional vibration

Notification tap behaviour:

- Opens **HomePage**
- Home shows the **current or next prayer**

Future option:

- Open directly to QiblaPage.

---

# 6. Location Strategy

Location is required for:

- Prayer time calculation
- Qibla direction
- Map display

The app uses:

- Single-shot GPS acquisition
- Stored location fallback

If location is unavailable:

- Last known location is used.

The app does **not continuously track location**.

---

# 7. Prayer Calculation

All prayer calculations are performed **locally on device**.

Supported parameters:

Calculation method examples:

- Muslim World League
- Egyptian General Authority
- Umm Al-Qura
- ISNA
- Karachi

Madhab:

- Shafi
- Hanafi

High latitude rules:

- Middle of the night
- Seventh of the night
- Angle based

The calculation engine is implemented in **QiblaNow.Core**.

---

# 8. Qibla Calculation

Qibla direction is computed locally using:

- User coordinates
- Kaaba coordinates

Kaaba coordinates:

```

21.4225° N
39.8262° E

```

Calculation uses **great-circle bearing**.

No internet required.

---

# 9. Data Storage

All data is stored locally.

Stored items:

- User location
- Calculation settings
- Notification preferences
- Last known prayer schedule

Storage technology:

- Platform preferences / local storage

No user data leaves the device.

---

# 10. Ads Policy

Ads are supported to keep the app free.

Rules:

- Only **AdMob banner ads**
- **Non-personalized ads only**
- No tracking
- No analytics
- No user profiling

Ads are displayed on:

- PrayerTimesPage
- QiblaPage
- MapPage

Ads are **never shown on HomePage**.

Ad frequency must remain minimal.

---

# 11. Offline Behaviour

The app must function **fully offline** after installation.

Offline capabilities:

- Prayer time calculation
- Qibla direction
- Compass
- Notifications
- Settings

Internet is only required for:

- Ad loading (optional)

If ads cannot load:

- App functionality must remain unaffected.

---

# 12. Privacy Requirements

The application must follow strict privacy rules:

- No analytics
- No user accounts
- No telemetry
- No remote logging
- No background tracking
- No location sharing

Location data never leaves the device.

AdMob must run in **non-personalized mode**.

---

# 13. Performance Requirements

Startup time:

- < 1 second

Prayer calculation:

- < 5 ms

Qibla calculation:

- < 1 ms

UI must remain responsive on low-end Android devices.

---

# 14. Future Features (Post-MVP)

Potential future additions:

- Widgets
- Lock screen prayer countdown
- Wear OS companion
- Mosque finder
- Hijri calendar
- Prayer tracking
- Multiple locations

These are **not part of MVP**.

---

# 15. Success Criteria

MVP is successful if:

- App launches reliably
- Prayer times are accurate
- Notifications fire correctly
- Qibla direction works
- App works fully offline
- UI is simple and fast

