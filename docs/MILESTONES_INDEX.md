# Qibla Now — Milestones Index

Milestones must be completed strictly in order.

A milestone is complete only when it outputs:
<promise>COMPLETE</promise>

If it outputs:
<promise>BLOCKED</promise>
resolve before continuing.

---

## Order

1. M01_Shell_DI.md
2. M02_Location.md
3. M03_PrayerEngine.md
4. M04_Alarms.md
5. M05_Qibla.md
6. M06_Compass.md
7. M07_Map.md
8. M08_i18n.md
9. M09_Ads.md
10. M10_Store.md

---

## Dependency Chain

M01 → M02 → M03 → M04 → M05 → M06 → M07 → M08 → M09 → M10

---

## Ralph Rule

When executing a milestone:
- Load only this file + the target milestone file.
- Do not modify files outside its allowlist.