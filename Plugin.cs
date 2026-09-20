namespace RoundMoments
{
    using System;
    using Exiled.API.Features;

    /// <summary>
    /// The main plugin class.
    /// </summary>
    public class Plugin : Plugin<Config, Translation>
    {
        private const string LegacyKillStreakHint = "You're on a {0}-kill streak!";

        /// <summary>
        /// Get the only existing instance of the <see cref="Plugin"/> class.
        /// </summary>
        public static Plugin? Instance { get; private set; }

        /// <inheritdoc/>
        public override string Author => "Storption";

        /// <inheritdoc/>
        public override string Name => "RoundMoments";

        /// <inheritdoc/>
        public override string Prefix => "RoundMoments";

        /// <inheritdoc/>
        public override Version RequiredExiledVersion { get; } = new Version(9, 14, 2);

        /// <inheritdoc/>
        public override Version Version { get; } = new Version(1, 3, 0);

        /// <inheritdoc/>
        public override void OnEnabled()
        {
            Instance = this;

            if (Translation.KillStreakHint == LegacyKillStreakHint)
                Log.Warn("Translation file reset recommended: it still contains the plain, uncolored messages from before v1.2.0. Delete RoundMoments' translation file (EXILED/Configs/Translations/RoundMoments/<port>.yml) and restart to regenerate it with the colored defaults. If you use a single merged translations file, delete only the RoundMoments section.");

            Modules.TeamWipe.RegisterEvents();
            Modules.KillTracking.RegisterEvents();
            Modules.RoundSummary.RegisterEvents();
            Modules.AutoUpdate.RegisterEvents();

            base.OnEnabled();
        }

        /// <inheritdoc/>
        public override void OnDisabled()
        {
            Modules.TeamWipe.UnregisterEvents();
            Modules.KillTracking.UnregisterEvents();
            Modules.RoundSummary.UnregisterEvents();
            Modules.AutoUpdate.UnregisterEvents();

            Instance = null;

            base.OnDisabled();
        }
    }
}