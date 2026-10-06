namespace RoundMoments.API
{
    using System;
    using System.Collections.Generic;
    using Exiled.API.Features;

    /// <summary>
    /// Lets other plugins register their own teams with RoundMoments.
    /// </summary>
    public static class CustomTeams
    {
        private static readonly List<CustomTeam> Teams = new();

        /// <summary>
        /// Gets every registered custom team.
        /// </summary>
        public static IReadOnlyList<CustomTeam> All => Teams;

        /// <summary>
        /// Registers a custom team. Registering the same team twice does nothing.
        /// </summary>
        public static void Register(CustomTeam team)
        {
            if (team is null)
                throw new ArgumentNullException(nameof(team));

            if (!Teams.Contains(team))
                Teams.Add(team);
        }

        /// <summary>
        /// Unregisters a custom team.
        /// </summary>
        /// <returns>Whether the team was registered.</returns>
        public static bool Unregister(CustomTeam team) => Teams.Remove(team);

        /// <summary>
        /// Gets the custom team a player is on, or null if they aren't on one.
        /// </summary>
        public static CustomTeam? Get(Player player)
        {
            foreach (CustomTeam team in Teams)
            {
                if (IsMember(team, player))
                    return team;
            }

            return null;
        }

        // Another plugin's membership check failing mustn't break RoundMoments' own handlers.
        internal static bool IsMember(CustomTeam team, Player player)
        {
            try
            {
                return team.IsMember(player);
            }
            catch (Exception ex)
            {
                Log.Error($"The custom team '{team.Name}' failed to check {player.Nickname}: {ex.Message}");
                return false;
            }
        }
    }
}