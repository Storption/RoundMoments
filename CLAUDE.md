# RoundMoments

Announces notable moments of a round and a round-end stats summary. Current version: **v1.4.0**.

See `../CLAUDE.md` for shared plugin conventions and the `AutoUpdate` module design.

## Structure

- `Plugin.cs`, `Config.cs`, `Translation.cs`
- `Modules/` — `TeamWipe.cs`, `KillTracking.cs`, `RoundSummary.cs`, `PlayerColor.cs`, `AutoUpdate.cs` (shared, see `../CLAUDE.md`)

Each module tracks its own state independently rather than sharing data — e.g. `RoundSummary` re-tracks kills/damage itself rather than reading `KillTracking`'s data. The one deliberate exception: `TeamWipe` exposes a public `FirstWipeTeam` property that `RoundSummary` reads, since the wipe-detection logic can only live in one place.

## Known behavior worth knowing before changing

- **`TeamWipe`**: only an actual death counts as a wipe - checks `ev.NewRole == RoleTypeId.Spectator`, *not* just team membership changing, since escaping/being recruited into another team also changes team membership but isn't a wipe. Cancelled role changes (`!ev.IsAllowed`) are ignored. `Team.FoundationForces` covers both NTF and Facility Guard, so the CASSIE line says generic "FOUNDATION", never "Nine-Tailed Fox" specifically. `ClassD`'s CASSIE token is `"CLASSD"` (no space) - the official token, confirmed working. Scientists wipes are included by default (`TeamWipeIncludeScientists`, on by default) but can be turned off - originally hardcoded-skipped. SCP wipes specifically wait for the vanilla "SCP terminated" CASSIE announcement to finish (`Cassie.IsSpeaking`, ~1s initial delay + 500ms polling, 15s safety cap) before playing the wipe announcement, to avoid the two overlapping.
- **Teamkills** are excluded everywhere (first blood, streaks, revenge, comeback, nemesis, damage-without-a-kill) using the game's own `HitboxIdentity.IsEnemy(attackerRole, victimRole)`. Use `ev.TargetOldRole` for the victim - by `Died` the victim's role is already Spectator.
- **Event order matters**: the game changes a dying player's role to Spectator (firing `Spawned`) *before* `Died` fires. `RoundSummary` therefore ignores `Spawned` for dead roles and only starts a life timer if none exists, so escaping doesn't reset survival time.
- **`RoundSummary`**: reports longest survivor, nemesis pair (2+ kills), most-damage-without-a-kill, first blood, and first team wiped. The round-end broadcast fires after `round_summary_delay_seconds` (default 6) with `shouldClearPrevious:true` - this is specifically to *outlast* `UsefulHints`' own round-end broadcast rather than get wiped by it. Config values are read before the delay so disabling the plugin mid-wait can't throw.
- **Hints**: several callouts from one kill (streak/revenge/comeback) are combined into ONE hint, because it's unverified whether the game stacks or replaces simultaneous hints. The death streak message is a personal broadcast, not a hint, so it can't clash with `DeathRecap`'s hint. `hint_line_padding` default is `15`, matching `DeathRecap`'s default position; more padding pushes a hint *higher* on screen.