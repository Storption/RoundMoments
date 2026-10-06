namespace RoundMoments.Modules
{
    using Exiled.API.Extensions;
    using Exiled.API.Features;
    using PlayerRoles;

    /// <summary>
    /// Shared helper for showing a player's name in a role's color.
    /// </summary>
    public static class PlayerColor
    {
        /// <summary>
        /// Gets the player's name wrapped in their current role's color.
        /// </summary>
        public static string GetColoredName(Player player) => GetColoredName(player.Nickname, player.Role.Type);

        /// <summary>
        /// Gets a name wrapped in a role's color, for a player described as they were at some earlier moment, like a kill or a death.
        /// </summary>
        public static string GetColoredName(string name, RoleTypeId role) => $"<color={role.GetColor().ToHex()}>{name}</color>";
    }
}