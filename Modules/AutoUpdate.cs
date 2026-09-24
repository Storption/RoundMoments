namespace RoundMoments.Modules
{
    using System;
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Security.Cryptography;
    using System.Threading;
    using System.Threading.Tasks;
    using Exiled.API.Features;

    /// <summary>
    /// Checks GitHub for a newer release of this plugin and, if found, downloads it and restarts
    /// the server once the current round finishes to apply it.
    /// </summary>
    public static class AutoUpdate
    {
        private const string GitHubOwner = "Storption";
        private const string GitHubRepo = "RoundMoments";
        private const string DllFileName = "RoundMoments.dll";

        private static int downloadingFlag;
        private static bool restartPending;

        private static Config Config => Plugin.Instance!.Config;
        private static string CurrentDllPath => Path.Combine(Paths.Plugins, DllFileName);

        public static void RegisterEvents()
        {
            Exiled.Events.Handlers.Server.WaitingForPlayers += OnWaitingForPlayers;
            Exiled.Events.Handlers.Server.RoundStarted += OnRoundStarted;
        }

        public static void UnregisterEvents()
        {
            Exiled.Events.Handlers.Server.WaitingForPlayers -= OnWaitingForPlayers;
            Exiled.Events.Handlers.Server.RoundStarted -= OnRoundStarted;
            restartPending = false;
            Interlocked.Exchange(ref downloadingFlag, 0);
        }

        private static void OnWaitingForPlayers()
        {
            if (!Config.AutoUpdateEnabled)
                return;

            Config config = Config;
            Version currentVersion = Plugin.Instance!.Version;
            SynchronizationContext? mainContext = SynchronizationContext.Current;

            _ = Task.Run(() => CheckAsync(config, currentVersion, mainContext));
        }

        private static async Task CheckAsync(Config config, Version currentVersion, SynchronizationContext? mainContext)
        {
            Log.Info("[AutoUpdate] Checking for a newer release...");

            using HttpClient client = new();
            client.DefaultRequestHeaders.UserAgent.ParseAdd($"{GitHubRepo}-AutoUpdate");

            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;

            (string Tag, string DownloadUrl, string? Sha256)? latestRelease;
            try
            {
                latestRelease = await GetLatestReleaseAsync(client);
            }
            catch (Exception ex)
            {
                Log.Error($"[AutoUpdate] Failed to check for updates - {ex.Message}");
                return;
            }

            if (latestRelease is null)
            {
                if (config.Debug)
                    Log.Debug("[AutoUpdate] Could not find a release or a matching .dll asset.");

                return;
            }

            if (!TryParseVersion(latestRelease.Value.Tag, out Version? latestVersion) || latestVersion is null)
            {
                if (config.Debug)
                    Log.Debug($"[AutoUpdate] Could not parse version from tag '{latestRelease.Value.Tag}'.");

                return;
            }

            if (latestVersion <= currentVersion)
            {
                Log.Info($"[AutoUpdate] Already up to date (current=v{currentVersion}, latest=v{latestVersion}).");
                return;
            }

            if (latestRelease.Value.Sha256 is null)
            {
                Log.Warn($"[AutoUpdate] v{latestVersion} is available but the release has no SHA-256 digest to verify against - skipping the update.");
                return;
            }

            Log.Info($"[AutoUpdate] Newer release found - v{currentVersion} -> v{latestVersion}. Downloading...");

            if (Interlocked.CompareExchange(ref downloadingFlag, 1, 0) != 0)
                return;

            try
            {
                byte[] newDllBytes = await client.GetByteArrayAsync(latestRelease.Value.DownloadUrl);

                using SHA256 sha256 = SHA256.Create();
                string actualHash = BitConverter.ToString(sha256.ComputeHash(newDllBytes)).Replace("-", string.Empty);
                if (!string.Equals(actualHash, latestRelease.Value.Sha256, StringComparison.OrdinalIgnoreCase))
                {
                    Log.Error("[AutoUpdate] The downloaded file failed its SHA-256 check - update aborted.");
                    return;
                }

                if (config.AutoUpdateBackup && File.Exists(CurrentDllPath))
                    File.Copy(CurrentDllPath, CurrentDllPath + ".backup", overwrite: true);

                File.WriteAllBytes(CurrentDllPath, newDllBytes);

                Log.Info("[AutoUpdate] Update downloaded and applied to disk.");

                if (config.AutoUpdateRestart)
                    ScheduleRestart(mainContext);
            }
            catch (Exception ex)
            {
                Log.Error($"[AutoUpdate] Failed to download or apply the update - {ex.Message}");
            }
            finally
            {
                Interlocked.Exchange(ref downloadingFlag, 0);
            }
        }

        private static void ScheduleRestart(SynchronizationContext? mainContext)
        {
            if (mainContext is null)
                RestartAfterRound();
            else
                mainContext.Post(_ => RestartAfterRound(), null);
        }

        private static void RestartAfterRound()
        {
            if (ServerStatic.StopNextRound != ServerStatic.NextRoundAction.DoNothing)
            {
                Log.Info("[AutoUpdate] A restart or shutdown is already scheduled - the update will load on the next start.");
                return;
            }

            Log.Info("[AutoUpdate] Restarting after this round to load the new version...");
            Server.ExecuteCommand("rnr");

            if (Round.IsStarted)
                AnnounceRestart();
            else
                restartPending = true;
        }

        private static void OnRoundStarted()
        {
            if (!restartPending)
                return;

            restartPending = false;
            AnnounceRestart();
        }

        private static void AnnounceRestart()
        {
            if (Plugin.Instance is null)
                return;

            string message = Plugin.Instance.Translation.AutoUpdateRestartBroadcast;
            if (string.IsNullOrWhiteSpace(message))
                return;

            Map.Broadcast(15, string.Format(message, GitHubRepo));
        }

        private static async Task<(string Tag, string DownloadUrl, string? Sha256)?> GetLatestReleaseAsync(HttpClient client)
        {
            string url = $"https://api.github.com/repos/{GitHubOwner}/{GitHubRepo}/releases/latest";
            string json = await client.GetStringAsync(url);
            GitHubRelease? release = UnityEngine.JsonUtility.FromJson<GitHubRelease>(json);

            if (release is null || string.IsNullOrWhiteSpace(release.tag_name))
                return null;

            GitHubAsset? asset = release.assets?.FirstOrDefault(a => string.Equals(a.name, DllFileName, StringComparison.OrdinalIgnoreCase));
            if (asset is null || string.IsNullOrWhiteSpace(asset.browser_download_url))
                return null;

            string? sha256 = asset.digest is not null && asset.digest.StartsWith("sha256:", StringComparison.OrdinalIgnoreCase)
                ? asset.digest.Substring("sha256:".Length)
                : null;

            return (release.tag_name!, asset.browser_download_url!, sha256);
        }

        private static bool TryParseVersion(string tag, out Version? version)
        {
            string cleaned = tag.TrimStart('v', 'V');
            return Version.TryParse(cleaned, out version);
        }

        [Serializable]
        private sealed class GitHubRelease
        {
            public string? tag_name = null;
            public GitHubAsset[]? assets = null;
        }

        [Serializable]
        private sealed class GitHubAsset
        {
            public string? name = null;
            public string? browser_download_url = null;
            public string? digest = null;
        }
    }
}