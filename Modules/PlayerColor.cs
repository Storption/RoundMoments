namespace RoundMoments.Modules
{
    using Exiled.API.Extensions;
    using PlayerRoles;
    using RoundMoments.API;

    public static class PlayerColor
    {
        public static string Get(RoleTypeId role, CustomTeam? team) => team?.Color ?? role.GetColor().ToHex();

        public static string GetColoredName(string name, string color) => $"<color={color}>{name}</color>";
    }
}