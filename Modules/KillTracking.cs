namespace RoundMoments.Modules
{
    using System.Collections.Generic;
    using Exiled.API.Features;
    using Exiled.Events.EventArgs.Player;

    /// <summary>
    /// Tracks kill streaks, death streaks, first blood, revenge kills, and comeback callouts.
    /// </summary>
    public static class KillTracking
    {
        private static readonly Dictionary<int, int> KillStreaks = new();
        private static readonly Dictionary<int, int> DeathStreaks = new();
        private static readonly Dictionary<int, int> LastKilledBy = new();
        private static bool firstBloodHappened;

        private static Config Config => Plugin.Instance!.Config;
        private static Translation Translation => Plugin.Instance!.Translation;

        public static void RegisterEvents()
        {
            Exiled.Events.Handlers.Player.Died += OnPlayerDied;
            Exiled.Events.Handlers.Server.WaitingForPlayers += OnWaitingForPlayers;
        }

        public static void UnregisterEvents()
        {
            Exiled.Events.Handlers.Player.Died -= OnPlayerDied;
            Exiled.Events.Handlers.Server.WaitingForPlayers -= OnWaitingForPlayers;
        }

        private static void OnWaitingForPlayers()
        {
            KillStreaks.Clear();
            DeathStreaks.Clear();
            LastKilledBy.Clear();
            firstBloodHappened = false;
        }

        private static void OnPlayerDied(DiedEventArgs ev)
        {
            if (ev.Player is null)
                return;

            int victimId = ev.Player.Id;

            if (KillStreaks.ContainsKey(victimId))
                KillStreaks[victimId] = 0;

            if (ev.Attacker is null || ev.Attacker == ev.Player)
                return;

            Player killer = ev.Attacker;
            Player victim = ev.Player;
            int killerId = killer.Id;

            if (Config.DeathStreakEnabled)
            {
                DeathStreaks.TryGetValue(victimId, out int currentDeathStreak);
                currentDeathStreak++;
                DeathStreaks[victimId] = currentDeathStreak;

                if (currentDeathStreak >= Config.DeathStreakThreshold)
                {
                    victim.Broadcast((ushort)Math.Ceiling(Config.HintDuration), string.Format(Translation.DeathStreakHint, currentDeathStreak));

                    if (Config.Debug)
                        Log.Debug($"{victim.Nickname} reached a {currentDeathStreak}-death streak.");
                }
            }

            if (!HitboxIdentity.IsEnemy(killer.Role.Type, ev.TargetOldRole))
            {
                if (Config.Debug)
                    Log.Debug($"{killer.Nickname} killed {victim.Nickname} but they aren't enemies - not counted.");

                return;
            }

            List<string> killerHints = new();

            if (Config.FirstBloodEnabled && !firstBloodHappened)
            {
                firstBloodHappened = true;
                string coloredName = PlayerColor.GetColoredName(killer);
                string message = string.Format(Translation.FirstBloodBroadcast, coloredName);
                Map.Broadcast((ushort)Config.FirstBloodBroadcastDuration, message);

                if (Config.Debug)
                    Log.Debug($"First blood: {killer.Nickname} killed {victim.Nickname}.");
            }

            if (Config.KillStreakEnabled)
            {
                KillStreaks.TryGetValue(killerId, out int currentStreak);
                currentStreak++;
                KillStreaks[killerId] = currentStreak;

                if (Config.Debug)
                    Log.Debug($"{killer.Nickname} (id={killerId}) kill streak now {currentStreak} (victim={victim.Nickname}, id={victimId}).");

                if (currentStreak >= Config.KillStreakThreshold)
                {
                    killerHints.Add(string.Format(Translation.KillStreakHint, currentStreak));

                    if (Config.Debug)
                        Log.Debug($"{killer.Nickname} reached a {currentStreak}-kill streak.");
                }
            }

            DeathStreaks[killerId] = 0;

            if (Config.RevengeKillEnabled && LastKilledBy.TryGetValue(killerId, out int killersLastKillerId) && killersLastKillerId == victimId)
            {
                killerHints.Add(string.Format(Translation.RevengeKillHint, PlayerColor.GetColoredName(victim)));
                LastKilledBy.Remove(killerId);

                if (Config.Debug)
                    Log.Debug($"{killer.Nickname} got a revenge kill on {victim.Nickname}.");
            }

            LastKilledBy[victimId] = killerId;

            if (Config.ComebackEnabled)
            {
                double killerHealthPercent = killer.MaxHealth > 0 ? killer.Health / killer.MaxHealth * 100.0 : 100.0;
                if (killerHealthPercent <= Config.ComebackHealthThreshold)
                {
                    killerHints.Add(string.Format(Translation.ComebackHint, (int)killer.Health));

                    if (Config.Debug)
                        Log.Debug($"{killer.Nickname} got a comeback kill at {killer.Health} HP ({killerHealthPercent:F1}%).");
                }
            }

            if (killerHints.Count > 0)
                ShowPositionedHint(killer, string.Join("\n", killerHints));
        }

        private static void ShowPositionedHint(Player player, string message)
        {
            string padding = new('\n', Config.HintLinePadding);
            player.ShowHint($"{padding}{message}", Config.HintDuration);
        }
    }
}