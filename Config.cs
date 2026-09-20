namespace RoundMoments
{
    using System.ComponentModel;
    using Exiled.API.Interfaces;

    /// <summary>
    /// The plugin's configuration.
    /// </summary>
    public class Config : IConfig
    {
        /// <inheritdoc />
        [Description("Whether the plugin is enabled.")]
        public bool IsEnabled { get; set; } = true;

        /// <inheritdoc />
        [Description("Whether debug messages are shown.")]
        public bool Debug { get; set; } = false;

        /// <summary>
        /// Gets or sets the number of consecutive kills required to trigger a kill streak announcement.
        /// </summary>
        [Description("The number of consecutive kills required to trigger a kill streak announcement.")]
        public int KillStreakThreshold { get; set; } = 3;

        /// <summary>
        /// Gets or sets the number of consecutive deaths (without a kill in between) required to trigger a death streak announcement.
        /// </summary>
        [Description("The number of consecutive deaths (without a kill in between) required to trigger a death streak announcement.")]
        public int DeathStreakThreshold { get; set; } = 3;

        /// <summary>
        /// Gets or sets how long, in seconds, hints shown to individual players stay visible.
        /// </summary>
        [Description("How long, in seconds, hints (and the death streak message) shown to individual players stay visible.")]
        public float HintDuration { get; set; } = 5;

        /// <summary>
        /// Gets or sets how many blank lines to pad hints with, controlling their vertical position on screen. More lines pushes the hint higher up.
        /// </summary>
        [Description("How many blank lines to pad hints with, controlling their vertical position on screen. More lines pushes the hint higher up.")]
        public int HintLinePadding { get; set; } = 15;

        /// <summary>
        /// Gets or sets whether the kill streak feature is enabled.
        /// </summary>
        [Description("Whether the kill streak feature is enabled.")]
        public bool KillStreakEnabled { get; set; } = true;

        /// <summary>
        /// Gets or sets whether the death streak feature is enabled.
        /// </summary>
        [Description("Whether the death streak feature is enabled.")]
        public bool DeathStreakEnabled { get; set; } = true;

        /// <summary>
        /// Gets or sets whether the first blood announcement is enabled.
        /// </summary>
        [Description("Whether the first blood announcement is enabled.")]
        public bool FirstBloodEnabled { get; set; } = true;

        /// <summary>
        /// Gets or sets how long, in seconds, the first blood broadcast stays visible.
        /// </summary>
        [Description("How long, in seconds, the first blood broadcast stays visible.")]
        public int FirstBloodBroadcastDuration { get; set; } = 5;

        /// <summary>
        /// Gets or sets whether the revenge kill callout is enabled.
        /// </summary>
        [Description("Whether the revenge kill callout is enabled.")]
        public bool RevengeKillEnabled { get; set; } = true;

        /// <summary>
        /// Gets or sets whether the comeback/underdog callout is enabled.
        /// </summary>
        [Description("Whether the comeback/underdog callout is enabled.")]
        public bool ComebackEnabled { get; set; } = true;

        /// <summary>
        /// Gets or sets the maximum health percentage (0-100) a player can have been at to qualify for a comeback callout after getting a kill.
        /// </summary>
        [Description("The maximum health percentage (0-100) a player can have been at to qualify for a comeback callout after getting a kill.")]
        public int ComebackHealthThreshold { get; set; } = 20;

        /// <summary>
        /// Gets or sets whether the team wipe announcement is enabled.
        /// </summary>
        [Description("Whether the team wipe announcement is enabled.")]
        public bool TeamWipeEnabled { get; set; } = true;

        /// <summary>
        /// Gets or sets how long, in seconds, the round-end summary broadcast stays visible.
        /// </summary>
        [Description("How long, in seconds, the round-end summary broadcast stays visible.")]
        public int RoundSummaryDuration { get; set; } = 10;

        /// <summary>
        /// Gets or sets how long, in seconds, to wait after the round ends before showing the round-end summary.
        /// </summary>
        [Description("How long, in seconds, to wait after the round ends before showing the round-end summary. The delay keeps other round-end broadcasts from clearing it.")]
        public int RoundSummaryDelaySeconds { get; set; } = 6;

        /// <summary>
        /// Gets or sets the round-end summary broadcasts text size, as a percentage of the default size.
        /// </summary>
        [Description("The round-end summary broadcast's text size, as a percentage of the default size.")]
        public int RoundSummaryTextSizePercent { get; set; } = 65;

        /// <summary>
        /// Gets or sets whether to check for and automatically install updates.
        /// </summary>
        [Description("Whether to check for and automatically install updates.")]
        public bool AutoUpdateEnabled { get; set; } = true;

        /// <summary>
        /// Gets or sets whether to keep a backup of the previous .dll before replacing it with an update.
        /// </summary>
        [Description("Whether to keep a backup of the previous .dll before replacing it with an update.")]
        public bool AutoUpdateBackup { get; set; } = true;

        /// <summary>
        /// Gets or sets whether to automatically restart the server once the current round ends, to apply a downloaded update.
        /// </summary>
        [Description("Whether to automatically restart the server once the current round ends, to apply a downloaded update. Never restarts mid-round.")]
        public bool AutoUpdateRestart { get; set; } = true;
    }
}