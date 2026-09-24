namespace RoundMoments.Modules
{
    using System.Collections.Generic;
    using System.Linq;
    using Exiled.API.Features;
    using Exiled.Events.EventArgs.Player;
    using MEC;
    using PlayerRoles;

    /// <summary>
    /// Announces when an entire team has been wiped out.
    /// </summary>
    public static class TeamWipe
    {
        private const string CoroutineTag = "RoundMoments.TeamWipe";

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

            Timing.KillCoroutines(CoroutineTag);
        }

        private static void OnWaitingForPlayers()
        {
            Timing.KillCoroutines(CoroutineTag);
            firstWipeTeam = null;
        }

        private static void OnChangingRole(ChangingRoleEventArgs ev)
        {
            if (!Config.TeamWipeEnabled)
                return;

            if (!ev.IsAllowed)
                return;

            if (ev.Player.Role.Team == Team.Dead)
                return;

            if (ev.Player.Role.Team == Team.Scientists && !Config.TeamWipeIncludeScientists)
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
                Team.Scientists => ("ALL SCIENTIST PERSONNEL TERMINATED .", "All Scientist personnel terminated."),

                _ => null,
            };

            if (cassieAnnouncement is null)
                return;

            firstWipeTeam ??= wipedTeam;

            if (Config.Debug)
                Log.Debug($"Team wipe detected for {wipedTeam}. Cassie phrase: {cassieAnnouncement.Value.Cassie}.");

            Timing.RunCoroutine(Announce(wipedTeam, cassieAnnouncement.Value.Cassie, cassieAnnouncement.Value.Subtitle), CoroutineTag);
        }

        private static IEnumerator<float> Announce(Team wipedTeam, string cassie, string subtitle)
        {
            if (wipedTeam == Team.SCPs)
            {
                yield return Timing.WaitForSeconds(1f);

                float waited = 0f;
                while (Cassie.IsSpeaking && waited < 15f)
                {
                    yield return Timing.WaitForSeconds(0.5f);
                    waited += 0.5f;
                }
            }

            Cassie.MessageTranslated(cassie, subtitle, true);
        }
    }
}