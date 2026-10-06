namespace RoundMoments.API
{
    using System;
    using Exiled.API.Features;

    /// <summary>
    /// A team added by another plugin, such as a custom faction, that RoundMoments treats like a vanilla team
    /// when it's wiped out.
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
    }
}