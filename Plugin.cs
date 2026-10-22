namespace RoundMoments
{
    using System;
    using Exiled.API.Features;

    public class Plugin : Plugin<Config, Translation>
    {
        private const string LegacyKillStreakHint = "You're on a {0}-kill streak!";

        public static Plugin? Instance { get; private set; }

        public override string Author => "Storption";

        public override string Name => "RoundMoments";

        public override string Prefix => "RoundMoments";

        public override Version RequiredExiledVersion { get; } = new Version(9, 14, 2);

        public override Version Version { get; } = new Version(1, 6, 0);

        public override void OnEnabled()
        {
            Instance = this;

            if (Translation.KillStreakHint == LegacyKillStreakHint)
                Log.Warn("Translation file reset recommended: it still contains the plain, uncolored messages from before v1.2.0. Delete RoundMoments' translation file (EXILED/Configs/Translations/RoundMoments/<port>.yml) and restart to regenerate it with the colored defaults. If you use a single merged translations file, delete only the RoundMoments section.");

            Modules.KillCredit.RegisterEvents();
            Modules.TeamWipe.RegisterEvents();
            Modules.KillTracking.RegisterEvents();
            Modules.RoundSummary.RegisterEvents();
            Modules.AutoUpdate.RegisterEvents();

            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            Modules.KillCredit.UnregisterEvents();
            Modules.TeamWipe.UnregisterEvents();
            Modules.KillTracking.UnregisterEvents();
            Modules.RoundSummary.UnregisterEvents();
            Modules.AutoUpdate.UnregisterEvents();

            Instance = null;

            base.OnDisabled();
        }
    }
}