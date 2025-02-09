using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using GetHymnLyricsv2.Models;
using Octokit;

namespace GetHymnLyricsv2.Services
{
    public class UpdateService : IUpdateService
    {
        private readonly ISettingsService _settingsService;
        private readonly GitHubClient _gitHubClient;
        private const string Owner = "lindsaysperring";
        private const string Repository = "GetHymnLyricsv2";

        public UpdateService(ISettingsService settingsService)
        {
            _settingsService = settingsService;
            _gitHubClient = new GitHubClient(new ProductHeaderValue("GetHymnLyricsv2"));
        }

        public async Task<UpdateInfo?> CheckForUpdatesAsync()
        {
            try
            {
                var releases = await _gitHubClient.Repository.Release.GetAll(Owner, Repository);
                var latestRelease = releases.FirstOrDefault();

                if (latestRelease == null)
                    return null;

                return new UpdateInfo
                {
                    Version = latestRelease.TagName.TrimStart('v'),
                    ReleaseNotes = latestRelease.Body,
                    DownloadUrl = latestRelease.HtmlUrl,
                    PublishedAt = latestRelease.PublishedAt?.UtcDateTime ?? DateTime.UtcNow
                };
            }
            catch (Exception)
            {
                return null;
            }
        }

        public bool IsUpdateAvailable(string currentVersion, string latestVersion)
        {
            if (!Version.TryParse(currentVersion, out Version? current) ||
                !Version.TryParse(latestVersion, out Version? latest))
            {
                return false;
            }

            return latest > current;
        }

        public async Task<bool> ShouldCheckForUpdates()
        {
            var timeSinceLastCheck = DateTime.UtcNow - _settingsService.Settings.LastUpdateCheck;
            return timeSinceLastCheck.TotalHours >= _settingsService.Settings.UpdateCheckFrequencyHours;
        }

        public async Task UpdateLastCheckTime()
        {
            _settingsService.Settings.LastUpdateCheck = DateTime.UtcNow;
            _settingsService.SaveSettings();
        }
    }
}
