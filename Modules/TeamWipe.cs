namespace RoundMoments.Modules
{
    using System.Linq;
    using Exiled.API.Features;
    using Exiled.Events.EventArgs.Player;
    using PlayerRoles;

    /// <summary>
    /// Announces when an entire team has been wiped out.
    /// </summary>
    public static class TeamWipe
    {
        private static Config Config => Plugin.Instance!.Config;
        private static Translation Translation => Plugin.Instance!.Translation;

        public static void RegisterEvents()
        {
            Exiled.Events.Handlers.Player.ChangingRole += OnChangingRole;
        }

        public static void UnregisterEvents()
        {
            Exiled.Events.Handlers.Player.ChangingRole -= OnChangingRole;
        }

        private static void OnChangingRole(ChangingRoleEventArgs ev)
        {
            if (!Config.TeamWipeEnabled)
                return;

            if (ev.Player.Role.Team == Team.Dead)
                return;

            if (Player.List.Count(p => p.Role.Team == ev.Player.Role.Team) > 1)
                return;

            string message = string.Format(Translation.TeamWipeBroadcast, ev.Player.Role.Team);

            string? cassieText = ev.Player.Role.Team switch
            {
                Team.SCPs => "ALL SCPSUBJECTS HAVE BEEN SECURED .",
                Team.ClassD => "ALL CLASS D PERSONNEL HAVE BEEN SECURED .",
                Team.ChaosInsurgency => "ALL CHAOSINSURGENCY PERSONNEL TERMINATED .",
                Team.FoundationForces => "ALL FOUNDATION PERSONNEL TERMINATED .",

                _ => null,
            };

            if (cassieText is not null)
                Cassie.MessageTranslated(cassieText, message);

            Map.Broadcast((ushort)Config.TeamWipeBroadcastDuration, message);
        }
    }
}