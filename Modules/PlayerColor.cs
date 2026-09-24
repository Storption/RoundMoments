namespace RoundMoments.Modules
{
    using Exiled.API.Extensions;
    using Exiled.API.Features;

    /// <summary>
    /// Shared helper for showing a player's name in their current role's color.
    /// </summary>
    public static class PlayerColor
    {
        /// <summary>
        /// Gets the player's name wrapped in their current role's color.
        /// </summary>
        public static string GetColoredName(Player player)
        {
            return $"<color={player.Role.Type.GetColor().ToHex()}>{player.Nickname}</color>";
        }
    }
}