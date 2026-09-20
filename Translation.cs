namespace RoundMoments
{
    using System.ComponentModel;
    using Exiled.API.Interfaces;

    /// <summary>
    /// The plugin's user-facing messages.
    /// </summary>
    public class Translation : ITranslation
    {
        /// <summary>
        /// Gets or sets the hint shown to a player when they reach a kill streak.
        /// </summary>
        [Description("The hint shown to a player when they reach a kill streak. {0} is the streak count.")]
        public string KillStreakHint { get; set; } = "<color=#FFA500>You're on a {0}-kill streak!</color>";

        /// <summary>
        /// Gets or sets the line in the round-end summary naming who drew first blood.
        /// </summary>
        [Description("The line in the round-end summary naming who drew first blood. {0} is the player's name.")]
        public string FirstBloodLine { get; set; } = "<color=#FF0000>First blood:</color> {0}";

        /// <summary>
        /// Gets or sets the line in the round-end summary naming which team was wiped out first.
        /// </summary>
        [Description("The line in the round-end summary naming which team was wiped out first. {0} is the team name.")]
        public string FirstTeamWipeLine { get; set; } = "<color=#FF0000>First team wiped out:</color> <color=yellow>{0}</color>";

        /// <summary>
        /// Gets or sets the hint shown to a player when they reach a death streak.
        /// </summary>
        [Description("The message shown to a player when they reach a death streak. Sent as a broadcast so it doesn't clash with other plugins' hints. {0} is the death count.")]
        public string DeathStreakHint { get; set; } = "<color=#FF0000>You've died {0} times in a row without a kill.</color>";

        /// <summary>
        /// Gets or sets the broadcast shown to everyone for the round's first kill.
        /// </summary>
        [Description("The broadcast shown to everyone for the round's first kill. {0} is the killer's name.")]
        public string FirstBloodBroadcast { get; set; } = "<color=#FF0000>First blood!</color> {0} drew first blood this round.";

        /// <summary>
        /// Gets or sets the hint shown to a player who gets a revenge kill.
        /// </summary>
        [Description("The hint shown to a player who gets a revenge kill. {0} is the name of the player they got revenge on.")]
        public string RevengeKillHint { get; set; } = "<color=#FFA500>Revenge!</color> You got payback on {0}.";

        /// <summary>
        /// Gets or sets the hint shown to a player who gets a kill while at critically low health.
        /// </summary>
        [Description("The hint shown to a player who gets a kill while at critically low health. {0} is their current health.")]
        public string ComebackHint { get; set; } = "<color=#00FF00>Incredible!</color> You got a kill at only {0} HP.";

        /// <summary>
        /// Gets or sets the line in the round-end summary reporting who survived the longest.
        /// </summary>
        [Description("The line in the round-end summary reporting who survived the longest. {0} is the player's name, {1} is minutes, {2} is seconds.")]
        public string LongestSurvivalLine { get; set; } = "<color=#FFA500>Longest survivor:</color> {0} <color=yellow>({1}m {2}s)</color>";

        /// <summary>
        /// Gets or sets the line in the round-end summary reporting the biggest nemesis pairing.
        /// </summary>
        [Description("The line in the round-end summary reporting the pair of players who killed each other the most. {0} and {1} are their names, {2} is the combined kill count between them.")]
        public string NemesisLine { get; set; } = "<color=#FF0000>Nemeses of the round:</color> {0} and {1} <color=yellow>({2} kills between them)</color>";

        /// <summary>
        /// Gets or sets the line in the round-end summary reporting who dealt the most damage without a kill.
        /// </summary>
        [Description("The line in the round-end summary reporting who dealt the most damage without getting a kill. {0} is the player's name, {1} is the damage amount.")]
        public string MostDamageWithoutKillLine { get; set; } = "<color=#00FFFF>So close:</color> {0} dealt <color=yellow>{1}</color> damage without a single kill.";

        /// <summary>
        /// Gets or sets the SCP team's name as shown in the round-end summary.
        /// </summary>
        [Description("The SCP team's name as shown in the round-end summary.")]
        public string TeamNameScps { get; set; } = "SCPs";

        /// <summary>
        /// Gets or sets the Class D team's name as shown in the round-end summary.
        /// </summary>
        [Description("The Class D team's name as shown in the round-end summary.")]
        public string TeamNameClassD { get; set; } = "Class D";

        /// <summary>
        /// Gets or sets the Chaos Insurgency team's name as shown in the round-end summary.
        /// </summary>
        [Description("The Chaos Insurgency team's name as shown in the round-end summary.")]
        public string TeamNameChaosInsurgency { get; set; } = "Chaos Insurgency";

        /// <summary>
        /// Gets or sets the Foundation Forces team's name as shown in the round-end summary.
        /// </summary>
        [Description("The Foundation Forces team's name (NTF and Facility Guards) as shown in the round-end summary.")]
        public string TeamNameFoundationForces { get; set; } = "Foundation Forces";

        /// <summary>
        /// Gets or sets the Scientists team's name as shown in the round-end summary.
        /// </summary>
        [Description("The Scientists team's name as shown in the round-end summary.")]
        public string TeamNameScientists { get; set; } = "Scientists";

        /// <summary>
        /// Gets or sets the broadcast shown to everyone when a plugin update has been installed and the server will restart once the round ends.
        /// </summary>
        [Description("The broadcast shown to everyone when a plugin update has been installed and the server will restart once the round ends. {0} is the plugin's name. Leave empty to disable.")]
        public string AutoUpdateRestartBroadcast { get; set; } = "<color=orange>[Update]</color> {0} was updated - the server will restart after this round to apply it.";
    }
}