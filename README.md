# RoundMoments

An [EXILED](https://github.com/ExMod-Team/EXILED) plugin for SCP: Secret Laboratory that announces the notable moments of a round - kill streaks, first blood, comebacks, revenge kills, team wipes - plus a stats summary at round end.

[![Downloads](https://img.shields.io/github/downloads/Storption/RoundMoments/total?style=for-the-badge&logo=github&color=blue)](https://github.com/Storption/RoundMoments/releases/latest)
[![Latest](https://img.shields.io/github/v/release/Storption/RoundMoments?include_prereleases&style=for-the-badge&logo=github&label=Latest%20Release&color=green)](https://github.com/Storption/RoundMoments/releases/latest)
[![Discord](https://img.shields.io/discord/1114170053949128817?style=for-the-badge&color=5865F2&logo=discord&label=Discord&logoColor=white)](https://join.storption.com)

## How it works

- **Kill streaks** - a hint shown to a player once their consecutive kills reach a configurable threshold, resetting when they die.
- **Death streaks** - the inverse: a message shown after too many deaths in a row without landing a kill.
- **First blood** - a broadcast to everyone the moment the round's first kill happens.
- **Revenge kills** - a hint when a player kills the specific person who killed them last.
- **Comeback/underdog kills** - a hint when a player gets a kill while at critically low health.
- **Team wipes** - a CASSIE announcement, alarm and all, when an entire team (SCPs, Class-D, Scientists, Chaos Insurgency, or Foundation Forces) is fully eliminated by death - an escaping player recruited into another team doesn't count as a wipe. Scientists can be left out with `team_wipe_include_scientists`.
- **Round-end summary** - a single broadcast, shown shortly after the round ends, covering the round's first blood, the first team wiped out, the longest survivor, the biggest "nemesis" pairing (two players who killed each other at least twice), and whoever dealt the most damage without ever landing a kill.
- **Colored names** - every player name shown by this plugin uses their role's color.
- **Auto-update** - checks this plugin's own GitHub repo for a newer release, and if found, downloads and applies it automatically, restarting the server once the current round ends.

*Teamkills don't count towards any kill callout or stat, and a player's side is judged by the role they had when they dealt the damage - so a grenade that lands after its thrower died still counts. A death in the Pocket Dimension counts as a kill for the SCP-106 who sent the player there. If one kill triggers several hints, they're shown together as a single hint.*

## Requirements

- [EXILED](https://github.com/ExMod-Team/EXILED) 9.14.2 or later

## Installation

1. Download the latest `RoundMoments.dll` from the [Releases](https://github.com/Storption/RoundMoments/releases) page.
2. Place it in your server's EXILED plugins folder (`%AppData%\EXILED\Plugins` on Windows, `~/.config/EXILED/Plugins` on Linux).
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
# How long, in seconds, hints (and the death streak message) shown to individual players stay visible.
hint_duration: 5
# How many blank lines to pad hints with, controlling their vertical position on screen. More lines pushes the hint higher up.
hint_line_padding: 15
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
# Whether a Scientists team wipe triggers a team wipe announcement too. Only applies while TeamWipeEnabled is on.
team_wipe_include_scientists: true
# How long, in seconds, the round-end summary broadcast stays visible.
round_summary_duration: 10
# How long, in seconds, to wait after the round ends before showing the round-end summary. The delay keeps other round-end broadcasts from clearing it.
round_summary_delay_seconds: 6
# The round-end summary broadcast's text size, as a percentage of the default size.
round_summary_text_size_percent: 65
# Whether to check for and automatically install updates.
auto_update_enabled: true
# Whether to keep a backup of the previous .dll before replacing it with an update.
auto_update_backup: true
# Whether to automatically restart the server once the current round ends, to apply a downloaded update. Never restarts mid-round.
auto_update_restart: true
```

All hint and broadcast text is configurable via the generated translation file, including every message's exact wording.