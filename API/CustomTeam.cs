namespace RoundMoments.API
{
    using System;
    using Exiled.API.Features;
    using PlayerRoles;

    /// <summary>
    /// A team added by another plugin, such as a custom faction or role, that RoundMoments treats like a vanilla team:
    /// for teamkills, name colors and when it's wiped out.
    /// </summary>
    public sealed class CustomTeam
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CustomTeam"/> class.
        /// </summary>
        /// <param name="name">The team's name, shown in the round summary's first-team-wiped line.</param>
        /// <param name="isMember">Gets whether a living player is on this team.</param>
        /// <param name="cassieMessage">The CASSIE announcement when the team is wiped out, or empty for none.</param>
        /// <param name="cassieSubtitles">The subtitles for <paramref name="cassieMessage"/>.</param>
        public CustomTeam(string name, Func<Player, bool> isMember, string cassieMessage, string cassieSubtitles)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            IsMember = isMember ?? throw new ArgumentNullException(nameof(isMember));
            CassieMessage = cassieMessage ?? string.Empty;
            CassieSubtitles = cassieSubtitles ?? string.Empty;
        }

        /// <summary>
        /// Gets the team's name, shown in the round summary's first-team-wiped line.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets whether a living player is on this team. It's checked when a player is dying, before their role changes.
        /// </summary>
        public Func<Player, bool> IsMember { get; }

        /// <summary>
        /// Gets the CASSIE announcement when the team is wiped out, or empty for none.
        /// </summary>
        public string CassieMessage { get; }

        /// <summary>
        /// Gets the subtitles for <see cref="CassieMessage"/>.
        /// </summary>
        public string CassieSubtitles { get; }

        /// <summary>
        /// Gets or sets the color of members' names, as a hex code like <c>#FF96DE</c>, or null to use their role's color.
        /// </summary>
        public string? Color { get; set; }

        /// <summary>
        /// Gets or sets the vanilla faction this team fights alongside, or null if it's only on its own side. Kills between
        /// members, or between a member and an ally, count as teamkills; any other kill involving a member is an enemy kill.
        /// </summary>
        public Faction? Allies { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the team being wiped out is announced and can be the round summary's
        /// first team wiped. Turn it off for a team that's really a single role.
        /// </summary>
        public bool TracksWipe { get; set; } = true;
    }
}