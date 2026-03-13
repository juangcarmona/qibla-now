# Qibla Now

![App Icon](../assets/qibla-now-icon.png)

Qibla Now is an offline-first Islamic utility app for Android, iOS, Windows, and Mac. It provides accurate prayer times, Qibla direction, and location-based features, all with full privacy and no ads.

## Features
- **Prayer Times:** Accurate for any location, using multiple calculation methods and madhab options.
- **Qibla Direction:** Compass-based, with true Qibla azimuth and error display.
- **Map:** Shows Qibla as a long-curve (great circle) path, not a straight line, for true geodesic accuracy.
- **Localization:** Supports 9 languages (English, Arabic, Spanish, French, Urdu, Bengali, Indonesian, Turkish, Persian). All translations are maintained in RESX files.
- **Privacy:** No location or personal data is sent to any server. All calculations are done locally. No ads, no analytics, no tracking.

## Translations
All visible app text is localized. See `Resources/Localization/AppResources.*.resx` for full language support. Prayer names use standard terminology for each language.

## Map Curves
The Qibla path on the map is shown as a curved line (great circle), not a straight line, because the shortest path on a sphere is a curve. This ensures the Qibla direction is accurate for all locations.

## Privacy
Qibla Now does not collect or transmit any user data. Location is used only for prayer time and Qibla calculations, and never leaves your device.

## Open Source
Qibla Now is free and open source. Contributions and translations are welcome.

---

For more details, see the [docs](../docs/).