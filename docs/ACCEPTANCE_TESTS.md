# Acceptance Tests

Tolerance:
- Prayer times: ±1 minute
- Qibla bearing: ±2 degrees

## PT-1 London
2026-03-02
51.5074, -0.1278
MWL, Shafi, SeventhOfNight

Expected:
Fajr 05:04
Dhuhr 12:18
Asr 15:06
Maghrib 17:46
Isha 19:12

## QB-1 London
Expected bearing ≈ 119°

Accept range: 117–121°

## Alarm Scenario

Enable Dhuhr only
PreReminder 10

Expect:
- PRE_REMINDER 12:08
- PRAYER 12:18

## Sensors Missing

Compass unavailable:
- Bearing still shown
- Heading shown as "—"
- No crash