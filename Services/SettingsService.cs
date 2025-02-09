using System;
using System.IO;
using System.Text.Json;
using GetHymnLyricsv2.Models;

namespace GetHymnLyricsv2.Services
{
    public class SettingsService : ISettingsService
    {
        private readonly string _settingsPath;
        private UserSettings _settings = default!;

        public UserSettings Settings => _settings;

        public SettingsService()
        {
            string appDataPath;

            if (OperatingSystem.IsMacOS())
            {
                appDataPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "com.lindsaysperring.gethymnlyricsv2", "Data", "GetHymnLyricsv2"
                );
            }
            else
            {
                appDataPath = Path.Combine(AppContext.BaseDirectory, "Data");
            }
            
            if (!Directory.Exists(appDataPath))
            {
                Directory.CreateDirectory(appDataPath);
            }

            _settingsPath = Path.Combine(appDataPath, "settings.json");
            LoadSettings();
        }

        public void LoadSettings()
        {
            try
            {
                if (File.Exists(_settingsPath))
                {
                    string jsonString = File.ReadAllText(_settingsPath);
                    _settings = JsonSerializer.Deserialize<UserSettings>(jsonString) ?? new UserSettings();
                }
                else
                {
                    _settings = new UserSettings();
                    SaveSettings(); // Create default settings file
                }
            }
            catch (Exception)
            {
                _settings = new UserSettings();
            }
        }

        public void SaveSettings()
        {
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                string jsonString = JsonSerializer.Serialize(_settings, options);
                File.WriteAllText(_settingsPath, jsonString);
            }
            catch (Exception)
            {
                // Log error or handle appropriately
            }
        }
    }
}
