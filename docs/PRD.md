# Product Requirements Document — Qibla Now (v1 Android)

## 1. Navigation Structure

Shell-based bottom navigation with three fixed tabs:

1. Times
2. Compass
3. Map

Times is default tab on launch.

App must start and render first frame within 1.0 second on mid-tier Android device.

---

## 2. Prayer Times (Salah)

### 2.1 Daily Display (Times Tab)

Display:

- Gregorian date (device local)
- Location label (GPS / Manual)
- Calculation method
- Fajr
- Dhuhr
- Asr
- Maghrib
- Isha
- Optional Sunrise
- Next prayer countdown (hh:mm:ss)

Rules:

- Times computed locally using pure .NET engine.
- Rounded to nearest minute.
- Offsets applied before rounding.
- Time zone derived from device.

If location unavailable:
- Show Location Required state.
- Provide Set Location action.

---

### 2.2 Calculation Settings

User configurable:

Calculation Method:
- MWL (default)
- Egyptian
- Umm al-Qura
- Karachi
- ISNA
- Moonsighting Committee

Asr Madhhab:
- Shafi (default)
- Hanafi

High Latitude Rule:
- SeventhOfNight (default)
- MiddleOfNight
- TwilightAngle

Per-prayer offsets:
- Range: -30 to +30 minutes
- Default: 0

Sunrise display:
- Default: ON

Settings must persist locally.

---

## 3. Alarms / Notifications

### 3.1 Per-Prayer Enablement

Each prayer has:
- Enable toggle (default OFF)

### 3.2 Alarm Behavior

Global alarm mode:
- NotificationOnly (default)
- SoundAndNotification
- VibrateAndNotification
- SoundVibrateAndNotification

Pre-reminder:
- 0 (default)
- 5
- 10
- 15 minutes before prayer

Per-prayer override allowed.

---

### 3.3 Exact Alarm Policy (Android)

The app requests exact alarms only for prayer notifications.

If exact alarms are denied:
- Schedule inexact alarms.
- Show warning in Alarm Settings:
  "Exact alarms disabled. Notifications may be delayed by the system."

Rescheduling triggers:
- Date change
- Location change
- Calculation change
- Timezone change
- DST change
- Device reboot

---

## 4. Qibla Direction

### 4.1 Bearing Mode (Fallback)

Displays:

- Qibla bearing (true north reference)
- Numeric degrees (0–359, one decimal)
- Arrow indicating direction
- Cardinal hint (N/NE/E/etc.)

Works without sensors.

---

### 4.2 Compass Mode

Displays:

- Qibla bearing (true)
- Device heading (sensor-based)
- Turn delta (shortest signed angle)

Rules:

- Bearing always true north.
- Heading may be magnetic.
- UI must clearly distinguish:
  - "Qibla bearing (true)"
  - "Device heading"

Calibration card shown:
- On first entry
- When accuracy low for >5 seconds

---

### 4.3 Map Mode (Offline Static)

- Bundled equirectangular world map image
- Non-interactive
- Overlays:
  - User marker
  - Kaaba marker
  - Initial bearing arrow/line

No online tiles.
No MAUI Maps control.

---

## 5. Location

LocationMode:
- GPS (default)
- Manual

Manual entry:
- Latitude (-90..90)
- Longitude (-180..180)

Last known snapshot stored.

No background tracking.

---

## 6. Internationalization

Languages in v1:

- English (default)
- Arabic (RTL)
- Spanish
- French
- Turkish
- Indonesian
- Urdu (RTL)

Language mode:
- Follow system
- Override in app

RTL requirements:
- Use Start/End layout properties
- Mirror directional icons
- No clipped text at 200% font scaling