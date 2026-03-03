# Data Model

Storage: MAUI Preferences wrapped by ISettingsStore

schemaVersion: int (default 1)

Language:
- language.mode: system|override
- language.tag: string

Location:
- location.mode: gps|manual
- manual.lat: double?
- manual.lon: double?
- last.lat: double?
- last.lon: double?
- last.timestamp: long?

Calculation:
- method
- madhhab
- highLatRule
- offsets (per prayer)
- showSunrise

Alarms:
- enabled.<prayer>
- global.mode
- global.preReminder
- override.<prayer>.mode?
- override.<prayer>.preReminder?

No ad caps stored (no frequency limiting).