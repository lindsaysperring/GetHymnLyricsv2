using GetHymnLyricsv2.Models;

namespace GetHymnLyricsv2.Services
{
    public interface ISettingsService
    {
        UserSettings Settings { get; }
        void LoadSettings();
        void SaveSettings();
    }
}
