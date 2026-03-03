# Non-Functional Requirements

## Privacy

- No analytics
- No crash reporting
- No telemetry
- No cloud services
- Location used only locally
- No personal data transmitted

## Offline

All features must function without internet:
- Prayer times
- Qibla
- Compass
- Map
- Alarms

Ads require network but must fail gracefully.

## Performance

- Cold start <= 1.0s
- Recompute prayer times <= 200ms
- No frame > 32ms during navigation

## Reliability

- Exact alarms when permitted
- Inexact fallback when denied
- Restore alarms after reboot
- Repair scheduling daily

## Battery

- No continuous GPS
- No background polling
- WorkManager only for daily repair

## Accessibility

- 48dp minimum touch targets
- Content descriptions on icons
- High contrast
- No color-only state encoding

## Formatting Rules

- Use DateTimeOffset internally
- Use TimeZoneInfo for conversions
- Display formatted via CurrentUICulture
- Degree format: 0–359.0 with 1 decimal