using System;
using System.Threading.Tasks;

namespace GetHymnLyricsv2.Services
{
    public class UpdateInfo
    {
        public string Version { get; set; } = string.Empty;
        public string ReleaseNotes { get; set; } = string.Empty;
        public string DownloadUrl { get; set; } = string.Empty;
        public DateTime PublishedAt { get; set; }
    }

    public interface IUpdateService
    {
        Task<UpdateInfo?> CheckForUpdatesAsync();
        bool IsUpdateAvailable(string currentVersion, string latestVersion);
        Task<bool> ShouldCheckForUpdates();
        Task UpdateLastCheckTime();
    }
}
