﻿﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GetHymnLyricsv2.Models;
using GetHymnLyricsv2.Services;
using GetHymnLyricsv2.Views;

namespace GetHymnLyricsv2.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        private readonly IFileService _fileService;
        private readonly ISongService _songService;
        private readonly IDialogService _dialogService;
        private readonly ISettingsService _settingsService;
        private readonly IUpdateService _updateService;
        private string? _currentFilePath;
        private UpdateInfo? _updateInfo;
        private const string SongsFileName = "Songs.xml";

        [ObservableProperty]
        private DataPacket? dataPacket;

        [ObservableProperty]
        private ObservableCollection<Song> songs = new();

        [ObservableProperty]
        private string searchText = string.Empty;

        [ObservableProperty]
        private Song? selectedSong;

        [ObservableProperty]
        private bool hasUnsavedChanges;

        [ObservableProperty]
        private bool updateAvailable;

        [ObservableProperty]
        private string? updateVersion;

        public static string BaseDirectory => AppContext.BaseDirectory;

        public IEnumerable<Song> FilteredSongs => string.IsNullOrWhiteSpace(SearchText) 
            ? Songs 
            : Songs.Where(s => 
                (s.Title?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false) || 
                s.Number.ToString().Contains(SearchText));

        partial void OnSearchTextChanged(string value)
        {
            OnPropertyChanged(nameof(FilteredSongs));
        }

        public SongDetailsViewModel SongDetails { get; }
        public SongSectionsViewModel SongSections { get; }

        public class OrderItem
        {
            public required SongSection Section { get; init; }
            public required SectionOrder OrderEntry { get; init; }
        }

        public MainWindowViewModel(
            IFileService fileService,
            ISongService songService,
            IDialogService dialogService,
            ISettingsService settingsService,
            SongDetailsViewModel songDetails,
            SongSectionsViewModel songSections,
            IUpdateService updateService)
        {
            _fileService = fileService;
            _songService = songService;
            _dialogService = dialogService;
            _settingsService = settingsService;
            SongDetails = songDetails;
            SongSections = songSections;
            _updateService = updateService;

            // Subscribe to content change events
            SongDetails.ContentChanged += (s, e) => HasUnsavedChanges = true;
            SongSections.ContentChanged += (s, e) => HasUnsavedChanges = true;

            #if DEBUG
                LoadSampleData();
            #else
                LoadData();
            #endif

            _ = CheckForUpdates(null);
        }

        private async void LoadSampleData()
        {
            var samplePath = Path.Combine("Data", "sample.xml");
            if (File.Exists(samplePath))
            {
                await LoadFileAsync(samplePath);
            }
        }

        private async void LoadData()
        {
            string filePath = OperatingSystem.IsMacOS()
                ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "com.lindsaysperring.gethymnlyricsv2", "Data", SongsFileName)
                : Path.Combine(AppContext.BaseDirectory, "Data", SongsFileName);
            
            if (File.Exists(filePath))
            {
                await LoadFileAsync(filePath);
                return;
            }
            
            if (OperatingSystem.IsMacOS())
            {
                string appFolder = Path.GetDirectoryName(filePath)!;
                Directory.CreateDirectory(appFolder);
                File.Copy(Path.Combine(AppContext.BaseDirectory, "Data", SongsFileName), filePath);
                await LoadFileAsync(filePath);
            }
        }

        [RelayCommand]
        private async Task OpenFile(Window window)
        {
            try
            {
                var filePath = await _dialogService.OpenFileAsync(window, "Open Hymn File", "*.xml");
                if (filePath != null)
                {
                    await LoadFileAsync(filePath);
                }
            }
            catch (Exception ex)
            {
                await _dialogService.ShowErrorAsync("Error", $"Failed to open file: {ex.Message}", window);
            }
        }

        [RelayCommand]
        private async Task SaveFile(Window window)
        {
            if (DataPacket == null) return;

            try
            {
                if (string.IsNullOrEmpty(_currentFilePath))
                {
                    _currentFilePath = await _dialogService.SaveFileAsync(window, "Save Hymn File", ".xml", "*.xml");
                    if (_currentFilePath == null) return;
                }

                await _fileService.SaveFileAsync(_currentFilePath, DataPacket);
                HasUnsavedChanges = false;
                await _dialogService.ShowInfoAsync("Saved", "File has been saved.", window);
            }
            catch (Exception ex)
            {
                await _dialogService.ShowErrorAsync("Error", $"Failed to save file: {ex.Message}", window);
            }
        }

        [RelayCommand]
        private void AddSong()
        {
            if (DataPacket?.RowData?.Row?.Songs == null) return;

            var maxNumber = Songs.Count > 0 ? Songs.Max(s => s.Number) : 0;
            var newSong = new Song
            {
                Number = maxNumber + 1,
                Title = "New Song",
                WordsPublicDomain = false,
                WordsLicenseCovered = false,
                MusicPublicDomain = false,
                MusicLicenseCovered = false
            };

            // Add to the DataPacket
            DataPacket.RowData.Row.Songs.Items ??= new List<Song>();
            DataPacket.RowData.Row.Songs.Items.Add(newSong);

            // Add to the observable collection
            Songs.Add(newSong);
            SelectedSong = newSong;
            HasUnsavedChanges = true;
        }

        [RelayCommand]
        private void DeleteSong()
        {
            if (SelectedSong == null || DataPacket?.RowData?.Row?.Songs?.Items == null) return;

            // Remove from the DataPacket
            DataPacket.RowData.Row.Songs.Items.Remove(SelectedSong);

            // Remove from the observable collection
            Songs.Remove(SelectedSong);
            SelectedSong = null;
            HasUnsavedChanges = true;
        }

        [RelayCommand]
        private async Task CopyToClipboard(Window window)
        {
            if (SongSections != null)
            {
                var clipboard = window.Clipboard;
                if (clipboard != null)
                {
                    await clipboard.SetTextAsync(SongSections.FormatSongText());
                    await _dialogService.ShowInfoAsync("Copied", "Text has been copied to clipboard.", window);
                }
            }
        }

        private async Task LoadFileAsync(string filePath)
        {
            try
            {
                DataPacket = await _fileService.LoadFileAsync(filePath);
                _currentFilePath = filePath;
                HasUnsavedChanges = false;

                Songs.Clear();
                if (DataPacket?.RowData?.Row?.Songs?.Items != null)
                {
                    foreach (var song in _songService.GetSongs(DataPacket))
                    {
                        Songs.Add(song);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to load file: {ex.Message}", ex);
            }
        }

        public async Task<bool> ConfirmCloseWithUnsavedChanges(Window window)
        {
            return await _dialogService.ShowConfirmationAsync(
                "Unsaved Changes",
                "You have unsaved changes. Do you want to close without saving?",
                window
            );
        }

        [RelayCommand]
        private async Task OpenSettings(Window window)
        {
            var settingsWindow = new SettingsWindow
            {
                DataContext = new SettingsViewModel(_settingsService)
            };

            await settingsWindow.ShowDialog(window);
        }

        [RelayCommand]
        private async Task CheckForUpdates(Window? window)
        {
            try
            {
                var force = window != null; // user clicked the button

                if (!force && !await _updateService.ShouldCheckForUpdates())
                {
                    if (window != null)
                    {
                        await _dialogService.ShowInfoAsync("Update Check", "Already checked for updates recently.", window);
                    }
                    return;
                }

                var updateInfo = await _updateService.CheckForUpdatesAsync();
                if (updateInfo == null)
                {
                    if (window != null)
                    {
                        await _dialogService.ShowInfoAsync("Update Check", "Failed to check for updates.", window);
                    }
                    return;
                }

                var currentVersion = typeof(MainWindowViewModel).Assembly.GetName().Version?.ToString() ?? "1.0.0";
                if (_updateService.IsUpdateAvailable(currentVersion, updateInfo.Version))
                {
                    _updateInfo = updateInfo;
                    UpdateVersion = updateInfo.Version;
                    UpdateAvailable = true;

                    if (window != null)
                    {
                        var result = await _dialogService.ShowConfirmationAsync(
                            "Update Available",
                            $"Version {updateInfo.Version} is available. Would you like to view the release notes?",
                            window);

                        if (result)
                        {
                            await OpenUpdateInfo(window);
                        }
                    }
                }
                else if (window != null)
                {
                    await _dialogService.ShowInfoAsync("Update Check", "You are using the latest version.", window);
                }

                await _updateService.UpdateLastCheckTime();
            }
            catch (Exception ex)
            {
                if (window != null)
                {
                    await _dialogService.ShowErrorAsync("Error", $"Failed to check for updates: {ex.Message}", window);
                }
            }
        }

        [RelayCommand]
        private async Task OpenUpdateInfo(Window window)
        {
            if (_updateInfo != null)
            {
                var result = await _dialogService.ShowConfirmationAsync(
                    $"Update {_updateInfo.Version}",
                    $"Release Notes:\n{_updateInfo.ReleaseNotes}\n\nWould you like to download the update?",
                    window);

                if (result)
                {
                    try
                    {
                        if (OperatingSystem.IsWindows())
                        {
                            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                            {
                                FileName = _updateInfo.DownloadUrl,
                                UseShellExecute = true
                            });
                        }
                        else if (OperatingSystem.IsMacOS())
                        {
                            System.Diagnostics.Process.Start("open", _updateInfo.DownloadUrl);
                        }
                        else if (OperatingSystem.IsLinux())
                        {
                            System.Diagnostics.Process.Start("xdg-open", _updateInfo.DownloadUrl);
                        }
                    }
                    catch (Exception ex)
                    {
                        await _dialogService.ShowErrorAsync("Error", $"Failed to open download URL: {ex.Message}", window);
                    }
                }
            }
        }

        partial void OnSelectedSongChanged(Song? value)
        {
            SongDetails.UpdateSong(value);

            if (value != null && DataPacket != null)
            {
                SongSections.Initialize(DataPacket, value);
            }
            else
            {
                SongSections.Clear();
            }
        }
    }
}
