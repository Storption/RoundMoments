namespace RoundMoments.Modules
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using Exiled.API.Features;
    using Newtonsoft.Json.Linq;

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

        private static Config Config => Plugin.Instance!.Config;
        private static string CurrentDllPath => Path.Combine(Paths.Plugins, DllFileName);

        public static void RegisterEvents()
        {
            Exiled.Events.Handlers.Server.WaitingForPlayers += OnWaitingForPlayers;
        }

        public static void UnregisterEvents()
        {
            Exiled.Events.Handlers.Server.WaitingForPlayers -= OnWaitingForPlayers;
            Interlocked.Exchange(ref downloadingFlag, 0);
        }

        private static void OnWaitingForPlayers()
        {
            if (!Config.AutoUpdateEnabled)
                return;

            _ = Task.Run(CheckAsync);
        }

        private static async Task CheckAsync()
        {
            Log.Info("[AutoUpdate] Checking for a newer release...");

            using HttpClient client = new();
            client.DefaultRequestHeaders.UserAgent.ParseAdd($"{GitHubRepo}-AutoUpdate");

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            (string Version, string DownloadUrl)? latestRelease;
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
                if (Config.Debug)
                    Log.Debug("[AutoUpdate] Could not find a release or a matching .dll asset.");

                return;
            }

            if (!TryParseVersion(latestRelease.Value.Version, out Version? latestVersion) || latestVersion is null)
            {
                if (Config.Debug)
                    Log.Debug($"[AutoUpdate] Could not parse version from tag '{latestRelease.Value.Version}'.");

                return;
            }

            Version currentVersion = Plugin.Instance!.Version;
            if (latestVersion <= currentVersion)
            {
                Log.Info($"[AutoUpdate] Already up to date (current=v{currentVersion}, latest=v{latestVersion}).");
                return;
            }

            Log.Info($"[AutoUpdate] Newer release found - v{currentVersion} -> v{latestVersion}. Downloading...");

            if (Interlocked.CompareExchange(ref downloadingFlag, 1, 0) != 0)
                return;

            try
            {
                byte[] newDllBytes = await client.GetByteArrayAsync(latestRelease.Value.DownloadUrl);

                if (Config.AutoUpdateBackup && File.Exists(CurrentDllPath))
                    File.Copy(CurrentDllPath, CurrentDllPath + ".backup", overwrite: true);

                File.WriteAllBytes(CurrentDllPath, newDllBytes);

                Log.Info("[AutoUpdate] Update downloaded and applied to disk.");

                if (Config.AutoUpdateRestart)
                {
                    Log.Info("[AutoUpdate] Restarting to load the new version...");
                    Server.ExecuteCommand("rnr");
                }
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

        private static async Task<(string Version, string DownloadUrl)?> GetLatestReleaseAsync(HttpClient client)
        {
            string url = $"https://api.github.com/repos/{GitHubOwner}/{GitHubRepo}/releases/latest";
            string json = await client.GetStringAsync(url);
            JObject release = JObject.Parse(json);

            string? tagName = release["tag_name"]?.ToString();
            if (string.IsNullOrWhiteSpace(tagName))
                return null;

            string? downloadUrl = (release["assets"] as JArray)
                ?.FirstOrDefault(a =>
                    string.Equals(a["name"]?.ToString(), DllFileName, StringComparison.OrdinalIgnoreCase))
                ?["browser_download_url"]?.ToString();

            if (string.IsNullOrWhiteSpace(downloadUrl))
                return null;

            return (tagName!, downloadUrl!);
        }

        private static bool TryParseVersion(string tag, out Version? version)
        {
            string cleaned = tag.TrimStart('v', 'V');
            return Version.TryParse(cleaned, out version);
        }
    }
}