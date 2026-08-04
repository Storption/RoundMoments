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
        private static readonly Dictionary<Player, int> KillStreaks = new();
        private static readonly Dictionary<Player, int> DeathStreaks = new();
        private static readonly Dictionary<Player, Player> LastKilledBy = new();
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

            if (KillStreaks.ContainsKey(ev.Player))
                KillStreaks[ev.Player] = 0;

            if (ev.Attacker is null || ev.Attacker == ev.Player)
                return;

            Player killer = ev.Attacker;
            Player victim = ev.Player;

            if (Config.FirstBloodEnabled && !firstBloodHappened)
            {
                firstBloodHappened = true;
                string message = string.Format(Translation.FirstBloodBroadcast, killer.Nickname);
                Map.Broadcast((ushort)Config.FirstBloodBroadcastDuration, message);
            }

            if (Config.KillStreakEnabled)
            {
                KillStreaks.TryGetValue(killer, out int currentStreak);
                currentStreak++;
                KillStreaks[killer] = currentStreak;

                if (currentStreak >= Config.KillStreakThreshold)
                    ShowPositionedHint(killer, string.Format(Translation.KillStreakHint, currentStreak));
            }

            DeathStreaks[killer] = 0;

            if (Config.DeathStreakEnabled)
            {
                DeathStreaks.TryGetValue(victim, out int currentDeathStreak);
                currentDeathStreak++;
                DeathStreaks[victim] = currentDeathStreak;

                if (currentDeathStreak >= Config.DeathStreakThreshold)
                    ShowPositionedHint(victim, string.Format(Translation.DeathStreakHint, currentDeathStreak));
            }

            if (Config.RevengeKillEnabled && LastKilledBy.TryGetValue(killer, out Player? killersLastKiller) && killersLastKiller == victim)
                ShowPositionedHint(killer, string.Format(Translation.RevengeKillHint, victim.Nickname));

            LastKilledBy[victim] = killer;

            if (Config.ComebackEnabled)
            {
                double killerHealthPercent = killer.Health / killer.MaxHealth * 100.0;
                if (killerHealthPercent <= Config.ComebackHealthThreshold)
                    ShowPositionedHint(killer, string.Format(Translation.ComebackHint, (int)killer.Health));
            }
        }

        private static void ShowPositionedHint(Player player, string message)
        {
            string padding = new string('\n', Config.HintLinePadding);
            player.ShowHint($"{padding}{message}", Config.HintDuration);
        }
    }
}