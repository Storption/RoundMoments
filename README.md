# RoundMoments

An [EXILED](https://github.com/ExMod-Team/EXILED) plugin for SCP: Secret Laboratory that announces the notable moments of a round - kill streaks, first blood, comebacks, revenge kills, team wipes - plus a stats summary at round end.

[![Downloads](https://img.shields.io/github/downloads/Storption/RoundMoments/total?style=for-the-badge&logo=github&color=blue)](https://github.com/Storption/RoundMoments/releases/latest)
[![Latest](https://img.shields.io/github/v/release/Storption/RoundMoments?include_prereleases&style=for-the-badge&logo=github&label=Latest%20Release&color=green)](https://github.com/Storption/RoundMoments/releases/latest)

## How it works

- **Kill streaks** - a hint shown to a player once their consecutive kills reach a configurable threshold, resetting when they die.
- **Death streaks** - the inverse: a hint shown after too many deaths in a row without landing a kill.
- **First blood** - a broadcast to everyone the moment the round's first kill happens.
- **Revenge kills** - a hint when a player kills the specific person who killed them last.
- **Comeback/underdog kills** - a hint when a player gets a kill while at critically low health.
- **Team wipes** - a CASSIE announcement plus broadcast when an entire team (SCPs, Class-D, Chaos Insurgency, or Foundation Forces) is fully eliminated.
- **Round-end summary** - a single broadcast covering the round's longest survivor, the biggest "nemesis" pairing (whoever killed each other the most), and whoever dealt the most damage without ever landing a kill.

## Requirements

- [EXILED](https://github.com/ExMod-Team/EXILED) 9.14.2 or later

## Installation

1. Download the latest `RoundMoments.dll` from the [Releases](https://github.com/Storption/RoundMoments/releases) page.
2. Place it in your server's EXILED plugins folder (`%AppData%\EXILED\Plugins` on Windows).
3. Restart your server. A default config will be generated on first load.

## Config

```yaml
# Whether the plugin is enabled.
is_enabled: true
# Whether debug messages are shown.
debug: false
# The number of consecutive kills required to trigger a kill streak announcement.
kill_streak_threshold: 3
# The number of consecutive deaths (without a kill in between) required to trigger a death streak announcement.
death_streak_threshold: 3
# How long, in seconds, hints shown to individual players stay visible.
hint_duration: 5
# How many blank lines to pad hints with, controlling their vertical position on screen. More lines pushes the hint higher up.
hint_line_padding: 10
# Whether the kill streak feature is enabled.
kill_streak_enabled: true
# Whether the death streak feature is enabled.
death_streak_enabled: true
# Whether the first blood announcement is enabled.
first_blood_enabled: true
# How long, in seconds, the first blood broadcast stays visible.
first_blood_broadcast_duration: 5
# Whether the revenge kill callout is enabled.
revenge_kill_enabled: true
# Whether the comeback/underdog callout is enabled.
comeback_enabled: true
# The maximum health percentage (0-100) a player can have been at to qualify for a comeback callout after getting a kill.
comeback_health_threshold: 20
# Whether the team wipe announcement is enabled.
team_wipe_enabled: true
# How long, in seconds, the team wipe broadcast stays visible.
team_wipe_broadcast_duration: 5
# How long, in seconds, the round-end summary broadcast stays visible.
round_summary_duration: 10
```

All hint and broadcast text is configurable via the generated translation file, including every message's exact wording.