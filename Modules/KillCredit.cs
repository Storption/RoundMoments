namespace RoundMoments.Modules
{
    using System.Collections.Generic;
    using Exiled.API.Enums;
    using Exiled.API.Features;
    using Exiled.API.Features.DamageHandlers;
    using Exiled.Events.EventArgs.Player;
    using Footprinting;
    using PlayerRoles;

    /// <summary>
    /// Works out who gets credit for a kill: the attacker as they were when the damage was dealt,
    /// or the SCP-106 who sent the victim into the pocket dimension.
    /// </summary>
    public static class KillCredit
    {
        private static readonly Dictionary<int, int> CapturedBy = new();

        public static void RegisterEvents()
        {
            Exiled.Events.Handlers.Player.EnteringPocketDimension += OnEnteringPocketDimension;
            Exiled.Events.Handlers.Player.EscapingPocketDimension += OnEscapingPocketDimension;
            Exiled.Events.Handlers.Player.Spawned += OnSpawned;
            Exiled.Events.Handlers.Player.Left += OnLeft;
            Exiled.Events.Handlers.Server.WaitingForPlayers += OnWaitingForPlayers;
        }

        public static void UnregisterEvents()
        {
            Exiled.Events.Handlers.Player.EnteringPocketDimension -= OnEnteringPocketDimension;
            Exiled.Events.Handlers.Player.EscapingPocketDimension -= OnEscapingPocketDimension;
            Exiled.Events.Handlers.Player.Spawned -= OnSpawned;
            Exiled.Events.Handlers.Player.Left -= OnLeft;
            Exiled.Events.Handlers.Server.WaitingForPlayers -= OnWaitingForPlayers;

            CapturedBy.Clear();
        }

        /// <summary>
        /// Gets the player credited with a death, or null if nobody is. <paramref name="killerRole"/> is their role when the damage was dealt.
        /// </summary>
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

        /// <summary>
        /// Gets the attacker's role when the damage was dealt - not the same as their current role for delayed damage, like a grenade thrown before they died.
        /// </summary>
        public static RoleTypeId GetAttackerRole(CustomDamageHandler damageHandler, Player attacker)
        {
            Footprint footprint = damageHandler.AttackerFootprint;
            return footprint.IsSet ? footprint.Role : attacker.Role.Type;
        }

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
        }

        private static void OnWaitingForPlayers()
        {
            CapturedBy.Clear();
        }
    }
}