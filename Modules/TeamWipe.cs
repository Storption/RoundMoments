namespace RoundMoments.Modules
{
    using System.Collections.Generic;
    using System.Linq;
    using Exiled.API.Features;
    using Exiled.Events.EventArgs.Player;
    using MEC;
    using PlayerRoles;
    using RoundMoments.API;

    /// <summary>
    /// Announces when an entire team, vanilla or registered through <see cref="CustomTeams"/>, has been wiped out.
    /// </summary>
    public static class TeamWipe
    {
        private const string CoroutineTag = "RoundMoments.TeamWipe";

        private static string? firstWipeTeamName;

        /// <summary>
        /// Gets the display name of the team that was wiped out first this round, or null if no team has been wiped yet.
        /// </summary>
        public static string? FirstWipeTeamName => firstWipeTeamName;

        private static Config Config => Plugin.Instance!.Config;
        private static Translation Translation => Plugin.Instance!.Translation;

        public static void RegisterEvents()
        {
            Exiled.Events.Handlers.Player.ChangingRole += OnChangingRole;
            Exiled.Events.Handlers.Player.Dying += OnDying;
            Exiled.Events.Handlers.Server.WaitingForPlayers += OnWaitingForPlayers;
        }

        public static void UnregisterEvents()
        {
            Exiled.Events.Handlers.Player.ChangingRole -= OnChangingRole;
            Exiled.Events.Handlers.Player.Dying -= OnDying;
            Exiled.Events.Handlers.Server.WaitingForPlayers -= OnWaitingForPlayers;

            Timing.KillCoroutines(CoroutineTag);
        }

        private static void OnWaitingForPlayers()
        {
            Timing.KillCoroutines(CoroutineTag);
            firstWipeTeamName = null;
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

            firstWipeTeamName ??= GetTeamDisplayName(wipedTeam);

            if (Config.Debug)
                Log.Debug($"Team wipe detected for {wipedTeam}. Cassie phrase: {cassieAnnouncement.Value.Cassie}.");

            Timing.RunCoroutine(Announce(cassieAnnouncement.Value.Cassie, cassieAnnouncement.Value.Subtitle, waitForCassie: wipedTeam == Team.SCPs), CoroutineTag);
        }

        // Custom teams are judged on Dying, while the dying player's role is still intact: by ChangingRole, the plugin
        // that owns the team may already have taken them off it.
        private static void OnDying(DyingEventArgs ev)
        {
            if (!Config.TeamWipeEnabled || !ev.IsAllowed || CustomTeams.Get(ev.Player) is not CustomTeam team)
                return;

            if (Player.List.Any(player => player != ev.Player && player.IsAlive && CustomTeams.IsMember(team, player)))
                return;

            firstWipeTeamName ??= team.Name;

            if (Config.Debug)
                Log.Debug($"Team wipe detected for the custom team {team.Name}.");

            if (!string.IsNullOrEmpty(team.CassieMessage))
                Timing.RunCoroutine(Announce(team.CassieMessage, team.CassieSubtitles, waitForCassie: false), CoroutineTag);
        }

        private static IEnumerator<float> Announce(string cassie, string subtitle, bool waitForCassie)
        {
            // The game announces each SCP's termination itself; let that finish first.
            if (waitForCassie)
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