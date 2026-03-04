# Qibla Now — Ads Policy

## Goals

Ads must:

* Provide modest monetization without harming usability.
* Never interfere with prayer actions.
* Respect Islamic user expectations and privacy.
* Remain predictable and minimal.

The app must always feel like a **religious utility first**, not an advertising surface.

---

# Ad Format

**Single banner format only**

* Google AdMob banner
* Standard adaptive banner size
* No interstitials
* No rewarded ads
* No full-screen takeovers

Reason:

Prayer moments require calm interaction. Any disruptive ad format would violate the core product goal.

---

# Ad Placement

Ads appear only on informational pages, never on critical prayer interaction flows.

Allowed placements:

### PrayerTimesPage

* Banner anchored at the **bottom of the page**
* Visible when scrolling the schedule

### QiblaPage

* Banner anchored at the **bottom of the screen**
* Must not overlap the compass or Qibla indicator

### MapPage

* Banner anchored at the **bottom**
* Must not obscure the map viewport

Rules:

* **One banner instance per page**
* Banner loads only once per page view
* No ad refresh during active interaction

---

# Pages Without Ads

Ads must **never appear** on:

* HomePage (Prayer Now screen)
* SettingsPage
* First-launch onboarding (if implemented later)

Reason:

These screens are considered **core interaction surfaces** and must remain clean.

---

# Privacy Rules

The app enforces strict privacy behavior.

Ads must run in **non-personalized mode (NPA)**.

The application must **never send** the following data to advertising networks:

* GPS location
* Prayer habits
* Device usage analytics
* Religious activity
* User identifiers

No analytics SDKs are permitted.

AdMob configuration must explicitly enable:

```
NonPersonalizedAds = true
```

---

# Data Collection

The application collects **no user data** for advertising purposes.

Allowed signals:

* Device type
* Screen size
* Generic contextual signals provided by AdMob

Forbidden:

* Tracking identifiers
* Behavioral profiling
* Cross-app tracking

---

# Performance Rules

Ads must not degrade the app experience.

Constraints:

* Banner load timeout ≤ 2 seconds
* If the ad fails to load, the UI must continue normally
* No retry loops
* No blocking UI while ads initialize

If AdMob fails, the banner container simply remains empty.

---

# Visual Behavior

Ads must integrate quietly into the layout.

Rules:

* No flashing or animated containers
* No custom frames around ads
* Banner sits flush with the page bottom
* Respect safe area insets on iOS

---

# Future Expansion Policy

Future versions **may** introduce:

* Paid version removing ads
* Donation option
* Sponsor banner (static)

But the following will **never** be introduced:

* Interstitial ads
* Video ads
* Rewarded ads
* Tracking SDKs
* Behavioral targeting

---

# Compliance Principles

The advertising policy must always respect:

* Religious sensitivity
* Privacy-first design
* Non-disruptive UX
* Offline-first philosophy

If monetization conflicts with user trust, **monetization loses**.
