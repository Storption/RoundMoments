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

            if (ev.Player.Role.Team == Team.Scientists)
                return;

            if (Player.List.Count(p => p.Role.Team == ev.Player.Role.Team) > 1)
                return;

            (string Cassie, string Subtitle)? cassieAnnouncement = ev.Player.Role.Team switch
            {
                Team.SCPs => ("ALL SCPSUBJECTS HAVE BEEN SECURED .", "All SCP subjects have been secured."),
                Team.ClassD => ("ALL CLASS D PERSONNEL HAVE BEEN SECURED .", "All Class D personnel have been secured."),
                Team.ChaosInsurgency => ("ALL CHAOSINSURGENCY PERSONNEL TERMINATED .", "All Chaos Insurgency personnel terminated."),
                Team.FoundationForces => ("ALL FOUNDATION PERSONNEL TERMINATED .", "All Foundation personnel terminated."),

                _ => null,
            };

            if (cassieAnnouncement is not null)
                Cassie.MessageTranslated(cassieAnnouncement.Value.Cassie, cassieAnnouncement.Value.Subtitle);

            if (Config.Debug)
                Log.Debug($"Team wipe detected for {ev.Player.Role.Team}. Cassie phrase: {cassieAnnouncement?.Cassie ?? "none"}.");
        }
    }
}