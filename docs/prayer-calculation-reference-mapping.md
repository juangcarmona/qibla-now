# Prayer Calculation Reference Mapping

This document describes the exact mapping between QiblaNow's .NET enums / settings
and the [PrayTimes v3.2](https://praytimes.org) JavaScript reference implementation.

The reference is committed at:
`src/QiblaNow.Core.Tests/Reference/praytimes/praytime.js` (v3.2, MIT)

---

## CalculationMethod → PrayTimes method key

| C# enum value                  | PrayTimes key | Fajr angle | Isha rule                     |
|--------------------------------|--------------|------------|-------------------------------|
| `MuslimWorldLeague`            | `MWL`        | 18°        | 17° angle                    |
| `EgyptianGeneralAuthority`     | `Egypt`      | 19.5°      | 17.5° angle                  |
| `ISNA`                         | `ISNA`       | 15°        | 15° angle                    |
| `Karachi`                      | `Karachi`    | 18°        | 18° angle                    |
| `Kuwait`                       | `Kuwait`     | 17.5°      | 17.5° angle (not in PrayTimes defaults; added by QiblaNow) |
| `UmmAlQura`                    | `Makkah`     | 18.5°      | **90 min** after sunset       |

### Maghrib

All supported methods inherit `defaults.maghrib = '1 min'` from PrayTimes, meaning:

```
Maghrib = Sunset + 1 minute
```

PrayTimes processes maghrib as an angle-based time through `processTimes`, then
`updateTimes` overrides it with `sunset + 1 min`. Our implementation computes
`maghrib = sunset + 1/60 h` directly, which produces identical rounded output.

### Dhuhr

All method defaults include `dhuhr = '0 min'` (no adjustment beyond solar noon).
Our implementation uses solar noon directly as Dhuhr, which is equivalent.

---

## Madhab → Asr shadow factor

| C# enum value | PrayTimes `asr` | Shadow factor |
|---------------|----------------|---------------|
| `Shafi`       | `Standard`      | 1× (object shadow + own length) |
| `Hanafi`      | `Hanafi`        | 2× (object shadow + 2× own length) |

Asr time is computed via the inverse-cotangent formula:

```
asrAngle = -arccot(shadowFactor + tan(|lat − decl|))
```

---

## HighLatitudeRule → PrayTimes `highLats` mode

| C# enum value    | PrayTimes `highLats` | Night fraction |
|------------------|---------------------|----------------|
| `SeventhOfNight` | `OneSeventh`        | night / 7      |
| `MiddleOfNight`  | `NightMiddle`       | night / 2      |
| `OneSeventh`     | `OneSeventh`        | night / 7      |

`SeventhOfNight` and `OneSeventh` are equivalent; both map to `OneSeventh`.

### When the rule applies

PrayTimes applies the high-latitude rule when:
1. The angle-based time is **NaN** (sun never reaches the angle), **or**
2. The prayer falls **farther from its anchor** (sunrise/sunset) than `night × fraction`.

```
night = 24 + sunrise − sunset   (PrayTimes formula)
```

- Fajr: capped at `sunrise − night/N`
- Isha: capped at `sunset + night/N`

Our prior implementation only applied the rule on NaN; it now matches the full
PrayTimes condition.

---

## Offset / tuning semantics

| Our field               | PrayTimes equivalent            | Behaviour                     |
|-------------------------|---------------------------------|-------------------------------|
| `FajrOffsetMinutes`     | `tune.fajr`                     | Added to computed Fajr time   |
| `DhuhrOffsetMinutes`    | `tune.dhuhr`                    | Added to Dhuhr                |
| `AsrOffsetMinutes`      | `tune.asr`                      | Added to Asr                  |
| `MaghribOffsetMinutes`  | `tune.maghrib`                  | Added on top of `sunset+1min` |
| `IshaOffsetMinutes`     | `tune.isha`                     | Added to Isha                 |

Offsets (tuning) are applied **after** all algorithmic corrections (high-latitude
rule, method defaults), matching PrayTimes `tuneTimes()` which runs last in the
pipeline.

---

## Rounding assumptions

PrayTimes pipeline:
1. Compute fractional UTC hours (sub-second precision).
2. `convertTimes`: adds `floor(hours × 3 600 000 ms)` to UTC midnight.
3. `roundTime('nearest')`: `round(timestamp / 60 000) × 60 000` → nearest minute.

Our implementation:
```csharp
int totalMinutes = (int)Math.Round(hours * 60);
```

Both produce **nearest-minute** results and agree to the minute for all known
test cases. The intermediate `Math.floor` in the JS reference operates at
millisecond precision; there is no sub-minute discrepancy for any case tested.

---

## Timezone assumptions

Our calculator **always outputs UTC** (`DateTimeOffset` with `TimeSpan.Zero`
offset). Times are stored and compared as UTC.

The PrayTimes reference computes UTC timestamps internally and formats them in the
requested timezone. When the CLI is invoked with `--timezone UTC`, its output
matches our UTC output directly.

---

## Solar position formula

The calculator uses the PrayTimes v3.2 solar-position formula evaluated at
**Julian Day from J2000.0**:

```
D = (Unix_ms / 86_400_000) − 10_957.5 + seedHour / 24 − lng / 360
g = 357.529 + 0.98560028 × D   (mod 360)
q = 280.459 + 0.98564736 × D   (mod 360)
L = q + 1.915 × sin(g) + 0.020 × sin(2g)   (mod 360)
e = 23.439 − 0.00000036 × D
RA = arctan2(cos(e) × sin(L), cos(L)) / 15   (mod 24)
decl = arcsin(sin(e) × sin(L))
eqt = q / 15 − RA
```

### Per-prayer seed hours

PrayTimes evaluates the solar position separately for each prayer using the
approximate time of that prayer as the `seedHour`:

| Prayer  | Seed hour |
|---------|-----------|
| Fajr    | 5         |
| Sunrise | 6         |
| Dhuhr   | 12        |
| Asr     | 13        |
| Sunset  | 18        |
| Maghrib | 18        |
| Isha    | 18        |

Our previous implementation used `day = date.DayOfYear` instead of the J2000-based
`D`. The day-of-year value differed by ~9 500 days, yielding ~1-minute errors in
Fajr, Sunrise, and Maghrib for typical dates.

---

## Known gaps / intentional divergences

| Feature                | PrayTimes support      | Our support        | Notes                           |
|------------------------|------------------------|--------------------|---------------------------------|
| Tehran method          | Yes (`Tehran`)         | No                 | Not in our CalculationMethod    |
| Jafari method          | Yes (`Jafari`)         | No                 | Not in our CalculationMethod    |
| France / Russia method | Yes                    | No                 | Not in our CalculationMethod    |
| Angle-based Maghrib    | Tehran, Jafari only    | No                 | All our methods use +1 min      |
| Jafari midnight        | Yes                    | No                 | Midnight not exposed            |
| `AngleBased` high-lat  | Yes                    | No                 | Not in our HighLatitudeRule     |
| `None` high-lat        | Yes                    | Not enumerated     | Falls to default (SeventhOfNight) |
| Iterations > 1         | Configurable           | Fixed at 1         | Matches reference default       |
