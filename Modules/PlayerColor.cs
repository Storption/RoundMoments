namespace RoundMoments.Modules
{
    using System;
    using System.Collections.Generic;
    using Exiled.API.Extensions;
    using Exiled.API.Features;

    /// <summary>
    /// Shared helper for showing a player's name colored by their badge if they have one, falling back to their role's color.
    /// </summary>
    public static class PlayerColor
    {
        private static readonly Dictionary<string, string> BadgeColorHex = new(StringComparer.OrdinalIgnoreCase)
        {
            ["pink"] = "#FF96DE",
            ["red"] = "#C50000",
            ["brown"] = "#944710",
            ["silver"] = "#A0A0A0",
            ["light_green"] = "#32CD32",
            ["crimson"] = "#DC143C",
            ["cyan"] = "#00B7EB",
            ["aqua"] = "#00FFFF",
            ["deep_pink"] = "#FF1493",
            ["tomato"] = "#FF6448",
            ["yellow"] = "#FAFF86",
            ["magenta"] = "#FF0090",
            ["blue_green"] = "#4DFFB8",
            ["orange"] = "#FF9966",
            ["lime"] = "#8FFF00",
            ["green"] = "#228B22",
            ["emerald"] = "#50C878",
            ["carmine"] = "#960018",
            ["nickel"] = "#727472",
            ["mint"] = "#98F898",
            ["army_green"] = "#4B5320",
            ["pumpkin"] = "#EE7600",
        };

        /// <summary>
        /// Gets the player's name wrapped in a color tag - their badge color if they have one, otherwise their role's color.
        /// </summary>
        public static string GetColoredName(Player player)
        {
            bool hasBadge = !string.IsNullOrEmpty(player.RankColor) && player.RankColor != "default";
            string nameColor = hasBadge && BadgeColorHex.TryGetValue(player.RankColor, out string? badgeHex)
                ? badgeHex
                : player.Role.Type.GetColor().ToHex();

            return $"<color={nameColor}>{player.Nickname}</color>";
        }
    }
}
