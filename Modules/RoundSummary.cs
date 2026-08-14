namespace RoundMoments.Modules
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Exiled.API.Features;
    using Exiled.Events.EventArgs.Player;
    using Exiled.Events.EventArgs.Server;
    using PlayerRoles;

    /// <summary>
    /// Tracks survival time, nemesis pairings, and damage-without-a-kill, reporting all three in one broadcast at round end.
    /// </summary>
    public static class RoundSummary
    {
        private static readonly Dictionary<int, DateTime> LifeStartTimes = new();
        private static readonly Dictionary<(int, int), int> KillPairs = new();
        private static readonly Dictionary<int, float> DamageDealt = new();
        private static readonly Dictionary<int, bool> HasKilled = new();

        private static int? bestSurvivorId;
        private static TimeSpan bestSurvivalTime;
        private static int? firstBloodKillerId;

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
            bestSurvivorId = null;
            bestSurvivalTime = TimeSpan.Zero;
            firstBloodKillerId = null;
        }

        private static void OnSpawned(SpawnedEventArgs ev)
        {
            LifeStartTimes[ev.Player.Id] = DateTime.Now;
        }

        private static void OnPlayerHurting(HurtingEventArgs ev)
        {
            if (ev.Attacker is null || ev.Player is null || ev.Attacker == ev.Player)
                return;

            int attackerId = ev.Attacker.Id;
            DamageDealt.TryGetValue(attackerId, out float currentDamage);
            DamageDealt[attackerId] = currentDamage + ev.Amount;
        }

        private static void OnPlayerDied(DiedEventArgs ev)
        {
            if (ev.Player is null)
                return;

            int victimId = ev.Player.Id;

            if (LifeStartTimes.TryGetValue(victimId, out DateTime lifestart))
            {
                TimeSpan survived = DateTime.Now - lifestart;
                if (survived > bestSurvivalTime)
                {
                    bestSurvivalTime = survived;
                    bestSurvivorId = victimId;
                }
            }

            if (ev.Attacker is null || ev.Attacker == ev.Player)
                return;

            int killerId = ev.Attacker.Id;
            firstBloodKillerId ??= killerId;

            HasKilled[killerId] = true;

            (int, int) pairKey = killerId < victimId ? (killerId, victimId) : (victimId, killerId);

            KillPairs.TryGetValue(pairKey, out int currentPairKills);
            KillPairs[pairKey] = currentPairKills + 1;
        }

        private static async void OnRoundEnded(RoundEndedEventArgs ev)
        {
            foreach (Player player in Player.List.Where(p => p.IsAlive))
            {
                int id = player.Id;
                if (!LifeStartTimes.TryGetValue(id, out DateTime lifeStart))
                    continue;

                TimeSpan survived = DateTime.Now - lifeStart;
                if (survived > bestSurvivalTime)
                {
                    bestSurvivalTime = survived;
                    bestSurvivorId = id;
                }
            }

            string text = string.Empty;

            Player? firstBloodPlayer = firstBloodKillerId.HasValue ? Player.Get(firstBloodKillerId.Value) : null;
            if (firstBloodPlayer is not null)
                text += string.Format(Translation.FirstBloodLine, PlayerColor.GetColoredName(firstBloodPlayer)) + "\n";

            Team? firstWipeTeam = TeamWipe.FirstWipeTeam;
            if (firstWipeTeam.HasValue)
                text += string.Format(Translation.FirstTeamWipeLine, GetTeamDisplayName(firstWipeTeam.Value)) + "\n";

            Player? bestSurvivor = bestSurvivorId.HasValue ? Player.Get(bestSurvivorId.Value) : null;
            if (bestSurvivor is not null)
                text += string.Format(Translation.LongestSurvivalLine, PlayerColor.GetColoredName(bestSurvivor), bestSurvivalTime.Minutes, bestSurvivalTime.Seconds) + "\n";

            KeyValuePair<(int, int), int> topPair = KillPairs.OrderByDescending(kv => kv.Value).FirstOrDefault();
            if (topPair.Value > 0)
            {
                Player? nemesisPlayer1 = Player.Get(topPair.Key.Item1);
                Player? nemesisPlayer2 = Player.Get(topPair.Key.Item2);
                if (nemesisPlayer1 is not null && nemesisPlayer2 is not null)
                    text += string.Format(Translation.NemesisLine, PlayerColor.GetColoredName(nemesisPlayer1), PlayerColor.GetColoredName(nemesisPlayer2), topPair.Value) + "\n";
            }

            KeyValuePair<int, float> topDamageNoKill = DamageDealt
                .Where(kv => !HasKilled.ContainsKey(kv.Key))
                .OrderByDescending(kv => kv.Value)
                .FirstOrDefault();

            Player? topDamageNoKillPlayer = topDamageNoKill.Value > 0 ? Player.Get(topDamageNoKill.Key) : null;
            if (topDamageNoKillPlayer is not null)
                text += string.Format(Translation.MostDamageWithoutKillLine, PlayerColor.GetColoredName(topDamageNoKillPlayer), (int)topDamageNoKill.Value) + "\n";

            if (Config.Debug)
                Log.Debug($"Round summary: survivor={bestSurvivor?.Nickname ?? "none"} ({bestSurvivalTime.Minutes}m {bestSurvivalTime.Seconds}s), nemesis pair kills={topPair.Value}, top damage-no-kill={topDamageNoKill.Value}.");

            if (!string.IsNullOrEmpty(text))
            {
                await Task.Delay(TimeSpan.FromSeconds(6));
                string sizedText = $"<size={Config.RoundSummaryTextSizePercent}%>{text}</size>";
                Map.Broadcast((ushort)Config.RoundSummaryDuration, sizedText, global::Broadcast.BroadcastFlags.Normal, true);
            }
        }

        private static string GetTeamDisplayName(Team team) => team switch
        {
            Team.SCPs => "SCPs",
            Team.ClassD => "Class D",
            Team.ChaosInsurgency => "Chaos Insurgency",
            Team.FoundationForces => "Foundation Forces",
            Team.Scientists => "Scientists",
            _ => team.ToString(),
        };
    }
}