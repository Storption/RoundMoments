namespace RoundMoments.Modules
{
    using System.Collections.Generic;
    using Exiled.API.Enums;
    using Exiled.API.Features;
    using Exiled.API.Features.DamageHandlers;
    using Exiled.Events.EventArgs.Player;
    using Footprinting;
    using PlayerRoles;
    using RoundMoments.API;

    public static class KillCredit
    {
        private static readonly Dictionary<int, int> CapturedBy = new();
        private static readonly Dictionary<int, CustomTeam?> DyingTeams = new();

        public static void RegisterEvents()
        {
            Exiled.Events.Handlers.Player.EnteringPocketDimension += OnEnteringPocketDimension;
            Exiled.Events.Handlers.Player.EscapingPocketDimension += OnEscapingPocketDimension;
            Exiled.Events.Handlers.Player.Spawned += OnSpawned;
            Exiled.Events.Handlers.Player.Dying += OnDying;
            Exiled.Events.Handlers.Player.Left += OnLeft;
            Exiled.Events.Handlers.Server.WaitingForPlayers += OnWaitingForPlayers;
        }

        public static void UnregisterEvents()
        {
            Exiled.Events.Handlers.Player.EnteringPocketDimension -= OnEnteringPocketDimension;
            Exiled.Events.Handlers.Player.EscapingPocketDimension -= OnEscapingPocketDimension;
            Exiled.Events.Handlers.Player.Spawned -= OnSpawned;
            Exiled.Events.Handlers.Player.Dying -= OnDying;
            Exiled.Events.Handlers.Player.Left -= OnLeft;
            Exiled.Events.Handlers.Server.WaitingForPlayers -= OnWaitingForPlayers;

            CapturedBy.Clear();
            DyingTeams.Clear();
        }

        public static Player? GetKiller(DiedEventArgs ev, out RoleTypeId killerRole)
        {
            killerRole = RoleTypeId.None;

            if (ev.Attacker is Player attacker)
            {
                if (attacker == ev.Player)
                    return null;

                killerRole = GetAttackerRole(ev.DamageHandler, attacker);
                return attacker;
            }

            if (ev.DamageHandler.Type != DamageType.PocketDimension || !CapturedBy.TryGetValue(ev.Player.Id, out int scp106Id))
                return null;

            Player? scp106 = Player.Get(scp106Id);
            if (scp106 is null)
                return null;

            killerRole = RoleTypeId.Scp106;
            return scp106;
        }

        public static RoleTypeId GetAttackerRole(CustomDamageHandler damageHandler, Player attacker)
        {
            Footprint footprint = damageHandler.AttackerFootprint;
            return footprint.IsSet ? footprint.Role : attacker.Role.Type;
        }

        // Only while they're on the life the damage came from: a grenade's thrower may have died since.
        public static CustomTeam? GetAttackerTeam(CustomDamageHandler damageHandler, Player attacker)
        {
            Footprint footprint = damageHandler.AttackerFootprint;
            bool sameLife = attacker.IsAlive && (!footprint.IsSet || footprint.LifeIdentifier == attacker.ReferenceHub.roleManager.CurrentRole.UniqueLifeIdentifier);
            return sameLife ? CustomTeams.Get(attacker) : null;
        }

        public static CustomTeam? GetVictimTeam(Player victim) => DyingTeams.TryGetValue(victim.Id, out CustomTeam? team) ? team : null;

        public static bool IsEnemyKill(DiedEventArgs ev, Player killer, RoleTypeId killerRole, out CustomTeam? killerTeam, out CustomTeam? victimTeam)
        {
            killerTeam = GetAttackerTeam(ev.DamageHandler, killer);
            victimTeam = GetVictimTeam(ev.Player);
            return CustomTeams.AreEnemies(killerRole, killerTeam, ev.TargetOldRole, victimTeam);
        }

        // By Died, the plugin that owns a team may already have taken the player off it.
        private static void OnDying(DyingEventArgs ev) => DyingTeams[ev.Player.Id] = CustomTeams.Get(ev.Player);

        private static void OnEnteringPocketDimension(EnteringPocketDimensionEventArgs ev)
        {
            if (ev.IsAllowed && ev.Scp106 is not null)
                CapturedBy[ev.Player.Id] = ev.Scp106.Id;
        }

        private static void OnEscapingPocketDimension(EscapingPocketDimensionEventArgs ev)
        {
            if (ev.IsAllowed)
                CapturedBy.Remove(ev.Player.Id);
        }

        private static void OnSpawned(SpawnedEventArgs ev)
        {
            // A death sets the role to Spectator before Died fires, so only forget the capture once the player is alive again.
            if (ev.Player.IsAlive)
                CapturedBy.Remove(ev.Player.Id);
        }

        private static void OnLeft(LeftEventArgs ev)
        {
            CapturedBy.Remove(ev.Player.Id);
            DyingTeams.Remove(ev.Player.Id);
        }

        private static void OnWaitingForPlayers()
        {
            CapturedBy.Clear();
            DyingTeams.Clear();
        }
    }
}