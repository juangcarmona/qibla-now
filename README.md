# Qibla Now

![App Icon](https://github.com/juangcarmona/qibla-now/blob/main/assets/ic_launcher_512.png)

Qibla Now is an offline-first Islamic utility app for Android, iOS, Windows, and Mac, built with .NET MAUI.

## Features
- **Prayer Times:** Accurate for any location, using multiple calculation methods and madhab options.
- **Qibla Direction:** Compass-based, with true Qibla azimuth and error display.
- **Map:** Shows Qibla as a long-curve (great circle) path, not a straight line, for true geodesic accuracy.
- **Adhan Alarms:** Fine-grained configurable notifications for each prayer.
- **Localization:** Supports 9 languages (English, Arabic, Spanish, French, Urdu, Bengali, Indonesian, Turkish, Persian). All translations are maintained in RESX files.
- **RTL Support:** Full right-to-left layout for Arabic-script languages.
- **Privacy:** No location or personal data is sent to any server. All calculations are done locally. No ads, no analytics, no tracking.

## Translations
All visible app text is localized. See `src/QiblaNow.App/Resources/Localization/AppResources.*.resx` for full language support. Prayer names use standard terminology for each language.

## Supported Languages

| Culture | Language | Native Name | Flag |
|--------|---------|-------------|------|
| en | English | English | 🇬🇧 |
| es | Spanish | Español | 🇪🇸 |
| fr | French | Français | 🇫🇷 |
| ar | Arabic | العربية | 🇸🇦 |
| ur | Urdu | اردو | 🇵🇰 |
| bn | Bengali | বাংলা | 🇧🇩 |
| id | Indonesian | Bahasa Indonesia | 🇮🇩 |
| tr | Turkish | Türkçe | 🇹🇷 |
| fa | Persian | فارسی | 🇮🇷 |

## Map Curves
The Qibla path on the map is shown as a curved line (great circle), not a straight line, because the shortest path on a sphere is a curve. This ensures the Qibla direction is accurate for all locations.

## Privacy
Qibla Now does not collect or transmit any user data. Location is used only for prayer time and Qibla calculations, and never leaves your device.

## Open Source
Qibla Now is free and open source. Contributions and translations are welcome.
