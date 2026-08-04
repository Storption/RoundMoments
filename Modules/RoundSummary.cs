namespace RoundMoments.Modules
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Exiled.API.Features;
    using Exiled.Events.EventArgs.Player;
    using Exiled.Events.EventArgs.Server;

    /// <summary>
    /// Tracks survival time, nemesis pairings, and damage-without-a-kill, reporting all three in one broadcast at round end.
    /// </summary>
    public static class RoundSummary
    {
        private static readonly Dictionary<Player, DateTime> LifeStartTimes = new();
        private static readonly Dictionary<(Player, Player), int> KillPairs = new();
        private static readonly Dictionary<Player, float> DamageDealt = new();
        private static readonly Dictionary<Player, bool> HasKilled = new();

        private static Player? bestSurvivor;
        private static TimeSpan bestSurvivalTime;

        private static Config Config => Plugin.Instance!.Config;
        private static Translation Translation => Plugin.Instance!.Translation;

        public static void RegisterEvents()
        {
            Exiled.Events.Handlers.Player.Spawned += OnSpawned;
            Exiled.Events.Handlers.Player.Died += OnPlayerDied;
            Exiled.Events.Handlers.Player.Hurting += OnPlayerHurting;
            Exiled.Events.Handlers.Server.RoundEnded += OnRoundEnded;
            Exiled.Events.Handlers.Server.WaitingForPlayers += OnWaitingForPlayers;
        }

        public static void UnregisterEvents()
        {
            Exiled.Events.Handlers.Player.Spawned -= OnSpawned;
            Exiled.Events.Handlers.Player.Died -= OnPlayerDied;
            Exiled.Events.Handlers.Player.Hurting -= OnPlayerHurting;
            Exiled.Events.Handlers.Server.RoundEnded -= OnRoundEnded;
            Exiled.Events.Handlers.Server.WaitingForPlayers -= OnWaitingForPlayers;
        }

        private static void OnWaitingForPlayers()
        {
            LifeStartTimes.Clear();
            KillPairs.Clear();
            DamageDealt.Clear();
            HasKilled.Clear();
            bestSurvivor = null;
            bestSurvivalTime = TimeSpan.Zero;
        }

        private static void OnSpawned(SpawnedEventArgs ev)
        {
            LifeStartTimes[ev.Player] = DateTime.Now;
        }

        private static void OnPlayerHurting(HurtingEventArgs ev)
        {
            if (ev.Attacker is null || ev.Player is null || ev.Attacker == ev.Player)
                return;

            DamageDealt.TryGetValue(ev.Attacker, out float currentDamage);
            DamageDealt[ev.Attacker] = currentDamage + ev.Amount;
        }

        private static void OnPlayerDied(DiedEventArgs ev)
        {
            if (ev.Player is null)
                return;

            if (LifeStartTimes.TryGetValue(ev.Player, out DateTime lifestart))
            {
                TimeSpan survived = DateTime.Now - lifestart;
                if (survived > bestSurvivalTime)
                {
                    bestSurvivalTime = survived;
                    bestSurvivor = ev.Player;
                }
            }

            if (ev.Attacker is null || ev.Attacker == ev.Player)
                return;

            HasKilled[ev.Attacker] = true;

            (Player, Player) pairKey = ev.Attacker.GetHashCode() < ev.Player.GetHashCode()
                ? (ev.Attacker, ev.Player)
                : (ev.Player, ev.Attacker);

            KillPairs.TryGetValue(pairKey, out int currentPairKills);
            KillPairs[pairKey] = currentPairKills + 1;
        }

        private static void OnRoundEnded(RoundEndedEventArgs ev)
        {
            foreach (Player player in Player.List.Where(p => p.IsAlive))
            {
                if (!LifeStartTimes.TryGetValue(player, out DateTime lifeStart))
                    continue;

                TimeSpan survived = DateTime.Now - lifeStart;
                if (survived > bestSurvivalTime)
                {
                    bestSurvivalTime = survived;
                    bestSurvivor = player;
                }    
            }

            string text = string.Empty;

            if (bestSurvivor is not null)
                text += string.Format(Translation.LongestSurvivalLine, bestSurvivor.Nickname, bestSurvivalTime.Minutes, bestSurvivalTime.Seconds) + "\n";

            KeyValuePair<(Player, Player), int> topPair = KillPairs.OrderByDescending(kv => kv.Value).FirstOrDefault();
            if (topPair.Value > 0)
                text += string.Format(Translation.NemesisLine, topPair.Key.Item1.Nickname, topPair.Key.Item2.Nickname, topPair.Value) + "\n";

            KeyValuePair<Player, float> topDamageNoKill = DamageDealt
                .Where(kv => !HasKilled.ContainsKey(kv.Key))
                .OrderByDescending(kv => kv.Value)
                .FirstOrDefault();
            if (topDamageNoKill.Key is not null && topDamageNoKill.Value > 0)
                text += string.Format(Translation.MostDamageWithoutKillLine, topDamageNoKill.Key.Nickname, (int)topDamageNoKill.Value) + "\n";

            if (!string.IsNullOrEmpty(text))
                Map.Broadcast((ushort)Config.RoundSummaryDuration, text);
        }
    }
}