# Data Model

Storage: existing settings mechanism exposed through `ISettingsStore`

schemaVersion: int = 1

## Language

- language.mode: system | override
- language.tag: string?

## Location

- location.mode: gps | manual
- manual.lat: double?
- manual.lon: double?
- last.lat: double?
- last.lon: double?
- last.timestampUtc: long?

## Calculation

- calculation.method
- calculation.madhab
- calculation.highLatitudeRule
- calculation.offsets.<prayer>: int
- calculation.showSunrise: bool

## Notifications

- notifications.enabled.fajr: bool
- notifications.enabled.dhuhr: bool
- notifications.enabled.asr: bool
- notifications.enabled.maghrib: bool
- notifications.enabled.isha: bool

## Reminder Policy

Use only if required by PRD/UI:

- reminders.global.preReminderMinutes: int?
- reminders.override.<prayer>.preReminderMinutes: int?

Avoid extra `mode` fields unless the PRD defines multiple reminder modes.

## Scheduling Recovery Metadata

Persist only what is strictly required for deterministic reconciliation:

- scheduling.lastPlannedPrayer: string?
- scheduling.lastPlannedTriggerUtc: long?
- scheduling.lastPlannedRequestCode: int?
- scheduling.lastPlannedVersion: int?
- scheduling.lastReconciledUtc: long?

Do not persist derived schedule data unless recovery correctness actually requires it.

## Explicit Non-Goals

- no ad frequency cap storage
- no analytics state
- no server sync state
- Sunrise is display-only unless PRD explicitly promotes it to a notification target