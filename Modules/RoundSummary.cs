namespace RoundMoments.Modules
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Exiled.API.Features;
    using Exiled.Events.EventArgs.Player;
    using Exiled.Events.EventArgs.Server;
    using MEC;
    using PlayerRoles;
    using PlayerStatsSystem;

    /// <summary>
    /// Tracks survival time, nemesis pairings, and damage-without-a-kill, reporting all three in one broadcast at round end.
    /// </summary>
    public static class RoundSummary
    {
        private static readonly Dictionary<int, DateTime> LifeStartTimes = new();
        private static readonly Dictionary<(int, int), int> KillPairs = new();
        private static readonly Dictionary<int, float> DamageDealt = new();
        private static readonly Dictionary<int, bool> HasKilled = new();
        private static readonly Dictionary<int, RoleTypeId> LastAttackRoles = new();

        private static (string Name, RoleTypeId Role)? bestSurvivor;
        private static TimeSpan bestSurvivalTime;
        private static (string Name, RoleTypeId Role)? firstBlood;

        private static Config Config => Plugin.Instance!.Config;
        private static Translation Translation => Plugin.Instance!.Translation;

        private static CoroutineHandle summaryBroadcast;

        public static void RegisterEvents()
        {
            Exiled.Events.Handlers.Player.Spawned += OnSpawned;
            Exiled.Events.Handlers.Player.Left += OnLeft;
            Exiled.Events.Handlers.Player.Died += OnPlayerDied;
            Exiled.Events.Handlers.Player.Hurt += OnPlayerHurt;
            Exiled.Events.Handlers.Server.RoundEnded += OnRoundEnded;
            Exiled.Events.Handlers.Server.WaitingForPlayers += OnWaitingForPlayers;
        }

        public static void UnregisterEvents()
        {
            Exiled.Events.Handlers.Player.Spawned -= OnSpawned;
            Exiled.Events.Handlers.Player.Left -= OnLeft;
            Exiled.Events.Handlers.Player.Died -= OnPlayerDied;
            Exiled.Events.Handlers.Player.Hurt -= OnPlayerHurt;
            Exiled.Events.Handlers.Server.RoundEnded -= OnRoundEnded;
            Exiled.Events.Handlers.Server.WaitingForPlayers -= OnWaitingForPlayers;

            Timing.KillCoroutines(summaryBroadcast);
        }

        private static void OnWaitingForPlayers()
        {
            Timing.KillCoroutines(summaryBroadcast);

            LifeStartTimes.Clear();
            KillPairs.Clear();
            DamageDealt.Clear();
            HasKilled.Clear();
            LastAttackRoles.Clear();
            bestSurvivor = null;
            bestSurvivalTime = TimeSpan.Zero;
            firstBlood = null;
        }

        private static void OnSpawned(SpawnedEventArgs ev)
        {
            // A death also fires Spawned (as Spectator) before Died, which still needs this life's start time.
            if (ev.Player.Role.Team == Team.Dead)
                return;

            // Only human lives count: SCPs tend to live the whole round. A human who becomes an SCP without
            // dying stops their survival time there.
            if (!ev.Player.IsHuman)
            {
                LifeStartTimes.Remove(ev.Player.Id);
                return;
            }

            if (!LifeStartTimes.ContainsKey(ev.Player.Id))
                LifeStartTimes[ev.Player.Id] = DateTime.Now;
        }

        private static void OnLeft(LeftEventArgs ev)
        {
            int id = ev.Player.Id;
            LifeStartTimes.Remove(id);
            DamageDealt.Remove(id);
            HasKilled.Remove(id);
            LastAttackRoles.Remove(id);

            foreach ((int, int) key in KillPairs.Keys.Where(k => k.Item1 == id || k.Item2 == id).ToList())
                KillPairs.Remove(key);
        }

        // Hurt, not Hurting: it fires once the damage has landed, after armor and after other plugins had their chance
        // to block it, so only damage that was really dealt counts.
        private static void OnPlayerHurt(HurtEventArgs ev)
        {
            if (ev.Attacker is null || ev.Player is null || ev.Attacker == ev.Player || ev.DamageHandler.Base is not StandardDamageHandler handler)
                return;

            RoleTypeId attackerRole = KillCredit.GetAttackerRole(ev.DamageHandler, ev.Attacker);
            if (!HitboxIdentity.IsEnemy(attackerRole, ev.Player.Role.Type))
                return;

            int attackerId = ev.Attacker.Id;
            DamageDealt.TryGetValue(attackerId, out float currentDamage);
            DamageDealt[attackerId] = currentDamage + handler.TotalDamageDealt;
            LastAttackRoles[attackerId] = attackerRole;
        }

        private static void OnPlayerDied(DiedEventArgs ev)
        {
            if (ev.Player is null)
                return;

            int victimId = ev.Player.Id;

            if (LifeStartTimes.TryGetValue(victimId, out DateTime lifeStart))
            {
                TimeSpan survived = DateTime.Now - lifeStart;
                LifeStartTimes.Remove(victimId);
                RecordSurvival(ev.Player.Nickname, ev.TargetOldRole, survived);

                if (Config.Debug)
                    Log.Debug($"{ev.Player.Nickname} (id={victimId}) died after surviving {(int)survived.TotalMinutes}m {survived.Seconds}s.");
            }

            Player? killer = KillCredit.GetKiller(ev, out RoleTypeId killerRole);
            if (killer is null || !HitboxIdentity.IsEnemy(killerRole, ev.TargetOldRole))
                return;

            int killerId = killer.Id;
            firstBlood ??= (killer.Nickname, killerRole);

            HasKilled[killerId] = true;
            LastAttackRoles[killerId] = killerRole;

            (int, int) pairKey = killerId < victimId ? (killerId, victimId) : (victimId, killerId);

            KillPairs.TryGetValue(pairKey, out int currentPairKills);
            KillPairs[pairKey] = currentPairKills + 1;
        }

        private static void OnRoundEnded(RoundEndedEventArgs ev)
        {
            foreach (Player player in Player.List.Where(p => p.IsAlive))
            {
                if (LifeStartTimes.TryGetValue(player.Id, out DateTime lifeStart))
                    RecordSurvival(player.Nickname, player.Role.Type, DateTime.Now - lifeStart);
            }

            string text = string.Empty;

            if (firstBlood is { } killer)
                text += string.Format(Translation.FirstBloodLine, PlayerColor.GetColoredName(killer.Name, killer.Role)) + "\n";

            Team? firstWipeTeam = TeamWipe.FirstWipeTeam;
            if (firstWipeTeam.HasValue)
                text += string.Format(Translation.FirstTeamWipeLine, GetTeamDisplayName(firstWipeTeam.Value)) + "\n";

            if (bestSurvivor is { } survivor)
                text += string.Format(Translation.LongestSurvivalLine, PlayerColor.GetColoredName(survivor.Name, survivor.Role), (int)bestSurvivalTime.TotalMinutes, bestSurvivalTime.Seconds) + "\n";

            KeyValuePair<(int, int), int> topPair = KillPairs.OrderByDescending(kv => kv.Value).FirstOrDefault();
            if (topPair.Value >= 2)
            {
                Player? nemesisPlayer1 = Player.Get(topPair.Key.Item1);
                Player? nemesisPlayer2 = Player.Get(topPair.Key.Item2);
                if (nemesisPlayer1 is not null && nemesisPlayer2 is not null)
                    text += string.Format(Translation.NemesisLine, GetAttackerName(nemesisPlayer1), GetAttackerName(nemesisPlayer2), topPair.Value) + "\n";
            }

            KeyValuePair<int, float> topDamageNoKill = DamageDealt
                .Where(kv => !HasKilled.ContainsKey(kv.Key))
                .OrderByDescending(kv => kv.Value)
                .FirstOrDefault();

            Player? topDamageNoKillPlayer = topDamageNoKill.Value > 0 ? Player.Get(topDamageNoKill.Key) : null;
            if (topDamageNoKillPlayer is not null)
                text += string.Format(Translation.MostDamageWithoutKillLine, GetAttackerName(topDamageNoKillPlayer), (int)topDamageNoKill.Value) + "\n";

            if (Config.Debug)
                Log.Debug($"Round summary: survivor={bestSurvivor?.Name ?? "none"} ({(int)bestSurvivalTime.TotalMinutes}m {bestSurvivalTime.Seconds}s), nemesis pair kills={topPair.Value}, top damage-no-kill={topDamageNoKill.Value}.");

            if (!string.IsNullOrEmpty(text))
            {
                string sizedText = $"<size={Config.RoundSummaryTextSizePercent}%>{text}</size>";
                ushort duration = (ushort)Config.RoundSummaryDuration;

                summaryBroadcast = Timing.CallDelayed(Math.Max(0, Config.RoundSummaryDelaySeconds), () => Map.Broadcast(duration, sizedText, global::Broadcast.BroadcastFlags.Normal, true));
            }
        }

        private static void RecordSurvival(string name, RoleTypeId role, TimeSpan survived)
        {
            if (survived <= bestSurvivalTime)
                return;

            bestSurvivalTime = survived;
            bestSurvivor = (name, role);
        }

        // Colored by the role they last fought as, since most players are spectators by the time the round ends.
        private static string GetAttackerName(Player player) => PlayerColor.GetColoredName(player.Nickname, LastAttackRoles.TryGetValue(player.Id, out RoleTypeId role) ? role : player.Role.Type);

        private static string GetTeamDisplayName(Team team) => team switch
        {
            Team.SCPs => Translation.TeamNameScps,
            Team.ClassD => Translation.TeamNameClassD,
            Team.ChaosInsurgency => Translation.TeamNameChaosInsurgency,
            Team.FoundationForces => Translation.TeamNameFoundationForces,
            Team.Scientists => Translation.TeamNameScientists,
            _ => team.ToString(),
        };
    }
}