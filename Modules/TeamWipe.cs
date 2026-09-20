namespace RoundMoments.Modules
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Exiled.API.Features;
    using Exiled.Events.EventArgs.Player;
    using PlayerRoles;

    /// <summary>
    /// Announces when an entire team has been wiped out.
    /// </summary>
    public static class TeamWipe
    {
        private static Team? firstWipeTeam;

        /// <summary>
        /// Gets the team that was wiped out first this round, or null if no team has been wiped yet.
        /// </summary>
        public static Team? FirstWipeTeam => firstWipeTeam;

        private static Config Config => Plugin.Instance!.Config;

        public static void RegisterEvents()
        {
            Exiled.Events.Handlers.Player.ChangingRole += OnChangingRole;
            Exiled.Events.Handlers.Server.WaitingForPlayers += OnWaitingForPlayers;
        }

        public static void UnregisterEvents()
        {
            Exiled.Events.Handlers.Player.ChangingRole -= OnChangingRole;
            Exiled.Events.Handlers.Server.WaitingForPlayers -= OnWaitingForPlayers;
        }

        private static void OnWaitingForPlayers()
        {
            firstWipeTeam = null;
        }

        private static async void OnChangingRole(ChangingRoleEventArgs ev)
        {
            if (!Config.TeamWipeEnabled)
                return;

            if (!ev.IsAllowed)
                return;

            bool debug = Config.Debug;

            if (ev.Player.Role.Team == Team.Dead)
                return;

            if (ev.Player.Role.Team == Team.Scientists)
                return;

            if (ev.NewRole != RoleTypeId.Spectator)
                return;

            if (Player.List.Count(p => p.Role.Team == ev.Player.Role.Team) > 1)
                return;

            Team wipedTeam = ev.Player.Role.Team;

            (string Cassie, string Subtitle)? cassieAnnouncement = wipedTeam switch
            {
                Team.SCPs => ("ALL SCPSUBJECTS HAVE BEEN SECURED .", "All SCP subjects have been secured."),
                Team.ClassD => ("ALL CLASSD PERSONNEL HAVE BEEN SECURED .", "All Class D personnel have been secured."),
                Team.ChaosInsurgency => ("ALL CHAOSINSURGENCY PERSONNEL TERMINATED .", "All Chaos Insurgency personnel terminated."),
                Team.FoundationForces => ("ALL FOUNDATION PERSONNEL TERMINATED .", "All Foundation personnel terminated."),

                _ => null,
            };

            if (cassieAnnouncement is null)
                return;

            if (wipedTeam == Team.SCPs)
            {
                await Task.Delay(TimeSpan.FromSeconds(1));

                DateTime waitStarted = DateTime.Now;
                while (Cassie.IsSpeaking && DateTime.Now - waitStarted < TimeSpan.FromSeconds(15))
                    await Task.Delay(TimeSpan.FromMilliseconds(500));
            }

            Cassie.MessageTranslated(cassieAnnouncement.Value.Cassie, cassieAnnouncement.Value.Subtitle, true);
            firstWipeTeam ??= wipedTeam;

            if (debug)
                Log.Debug($"Team wipe detected for {wipedTeam}. Cassie phrase: {cassieAnnouncement.Value.Cassie}.");
        }
    }
}