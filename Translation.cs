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
        public string KillStreakHint { get; set; } = "You're on a {0}-kill streak!";

        /// <summary>
        /// Gets or sets the hint shown to a player when they reach a death streak.
        /// </summary>
        [Description("The hint shown to a player when they reach a death streak. {0} is the death count.")]
        public string DeathStreakHint { get; set; } = "You've died {0} times in a row without a kill.";

        /// <summary>
        /// Gets or sets the broadcast shown to everyone for the round's first kill.
        /// </summary>
        [Description("The broadcast shown to everyone for the round's first kill. {0} is the killer's name.")]
        public string FirstBloodBroadcast { get; set; } = "First blood! {0} drew first blood this round.";

        /// <summary>
        /// Gets or sets the hint shown to a player who gets a revenge kill.
        /// </summary>
        [Description("The hint shown to a player who gets a revenge kill. {0} is the name of the player they got revenge on.")]
        public string RevengeKillHint { get; set; } = "Revenge! You got payback on {0}.";

        /// <summary>
        /// Gets or sets the hint shown to a player who gets a kill while at critically low health.
        /// </summary>
        [Description("The hint shown to a player who gets a kill while at critically low health. {0} is their current health.")]
        public string ComebackHint { get; set; } = "Incredible! You got a kill at only {0} HP.";

        /// <summary>
        /// Gets or sets the line in the round-end summary reporting who survived the longest.
        /// </summary>
        [Description("The line in the round-end summary reporting who survived the longest. {0} is the player's name, {1} is minutes, {2} is seconds.")]
        public string LongestSurvivalLine { get; set; } = "Longest survivor: {0} ({1}m {2}s)";

        /// <summary>
        /// Gets or sets the line in the round-end summary reporting the biggest nemesis pairing.
        /// </summary>
        [Description("The line in the round-end summary reporting the pair of players who killed each other the most. {0} and {1} are their names, {2} is the combined kill count between them.")]
        public string NemesisLine { get; set; } = "Nemeses of the round: {0} and {1} ({2} kills between them)";

        /// <summary>
        /// Gets or sets the line in the round-end summary reporting who dealt the most damage without a kill.
        /// </summary>
        [Description("The line in the round-end summary reporting who dealt the most damage without getting a kill. {0} is the player's name, {1} is the damage amount.")]
        public string MostDamageWithoutKillLine { get; set; } = "So close: {0} dealt {1} damage without a single kill.";
    }
}