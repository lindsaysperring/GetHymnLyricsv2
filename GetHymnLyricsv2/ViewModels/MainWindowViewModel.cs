﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GetHymnLyricsv2.Models;
using GetHymnLyricsv2.Models.PreviewFormats.Interfaces;
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

        private readonly IEnumerable<IPreviewFormat> _previewFormats = Array.Empty<IPreviewFormat>();

        [ObservableProperty]
        private IPreviewFormat? selectedFormat;

        [ObservableProperty]
        private DataPacket? dataPacket;

        public IEnumerable<IPreviewFormat> PreviewFormats => _previewFormats;

        [ObservableProperty]
        private ObservableCollection<Song> songs = new();

        [ObservableProperty]
        private string searchText = string.Empty;

        [ObservableProperty]
        private ObservableCollection<MenuItem> _formatMenuItems = new();

        partial void OnSelectedFormatChanged(IPreviewFormat? value)
        {
            if (value != null)
            {
                _settingsService.Settings.LastPreviewFormat = value.Name;
                _settingsService.SaveSettings();
            }
        }

        [RelayCommand]
        private void OnFormatSelected(IPreviewFormat format)
        {
            SelectedFormat = format;
        }

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
            IUpdateService updateService,
            IEnumerable<IPreviewFormat> previewFormats)
        {
            _fileService = fileService;
            _songService = songService;
            _dialogService = dialogService;
            _settingsService = settingsService;
            SongDetails = songDetails;
            SongSections = songSections;
            _updateService = updateService;
            _previewFormats = previewFormats;

            // Initialize selected format
            SelectedFormat = _previewFormats.FirstOrDefault(f => f.Name == _settingsService.Settings.LastPreviewFormat)
                ?? _previewFormats.First();

            // Subscribe to content change events
            SongDetails.ContentChanged += (s, e) => HasUnsavedChanges = true;
            SongSections.ContentChanged += (s, e) => HasUnsavedChanges = true;

            BuildFormatMenu();
        }

        public async Task InitializeAsync()
        {
            #if DEBUG
                var samplePath = Path.Combine("Data", "sample.xml");
                if (File.Exists(samplePath))
                {
                    await LoadFileAsync(samplePath);
                }
            #else
                string filePath = OperatingSystem.IsMacOS()
                    ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "com.lindsaysperring.gethymnlyricsv2", "Data", SongsFileName)
                    : Path.Combine(AppContext.BaseDirectory, "Data", SongsFileName);
                
                if (File.Exists(filePath))
                {
                    await LoadFileAsync(filePath);
                }
                else if (OperatingSystem.IsMacOS())
                {
                    string appFolder = Path.GetDirectoryName(filePath)!;
                    Directory.CreateDirectory(appFolder);
                    File.Copy(Path.Combine(AppContext.BaseDirectory, "Data", SongsFileName), filePath);
                    await LoadFileAsync(filePath);
                }
            #endif

            await CheckForUpdatesInternal(false);
        }

        [RelayCommand]
        private async Task OpenFile()
        {
            try
            {
                var filePath = await _dialogService.OpenFileAsync("Open Hymn File", "*.xml");
                if (filePath != null)
                {
                    await LoadFileAsync(filePath);
                }
            }
            catch (Exception ex)
            {
                await _dialogService.ShowErrorAsync("Error", $"Failed to open file: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task SaveFile()
        {
            if (DataPacket == null) return;

            try
            {
                if (string.IsNullOrEmpty(_currentFilePath))
                {
                    _currentFilePath = await _dialogService.SaveFileAsync("Save Hymn File", ".xml", "*.xml");
                    if (_currentFilePath == null) return;
                }

                await _fileService.SaveFileAsync(_currentFilePath, DataPacket);
                HasUnsavedChanges = false;
                await _dialogService.ShowInfoAsync("Saved", "File has been saved.");
            }
            catch (Exception ex)
            {
                await _dialogService.ShowErrorAsync("Error", $"Failed to save file: {ex.Message}");
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
        private async Task CopyToClipboard()
        {
            if (SongSections == null || SelectedFormat == null || !SelectedFormat.SupportsCopy) return;

            try
            {
                if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop && desktop.MainWindow?.Clipboard is { } clipboard && SelectedSong != null)
                {
                    await clipboard.SetTextAsync(SelectedFormat.FormatForCopy(SelectedSong, SongSections.SongOrder));
                    await _dialogService.ShowInfoAsync("Copied", $"Text has been copied to clipboard in {SelectedFormat.Name} format.");
                }
            }
            catch (Exception ex)
            {
                await _dialogService.ShowErrorAsync("Error", $"Failed to copy to clipboard: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task ExportToFile()
        {
            if (SongSections == null || SelectedFormat == null || !SelectedFormat.SupportsExport) return;

            try
            {
                if (SelectedSong != null)
                {
                    var fileType = SelectedFormat.SupportedFileExtensions[0];
                    var suggestedFileName = SelectedFormat.GetSuggestedFileName(SelectedSong);
                    var filePath = await _dialogService.SaveFileAsync("Export Song", fileType, suggestedFileName, SelectedFormat.Name, $"*{fileType}");
                    if (filePath == null) return;

                    await SelectedFormat.ExportToFileAsync(SelectedSong, SongSections.SongOrder, filePath);
                    await _dialogService.ShowInfoAsync("Exported", $"Song has been exported to {filePath}");
                }
                else
                {
                    await _dialogService.ShowInfoAsync("Export", "Please select a song to export.");
                }
            }
            catch (Exception ex)
            {
                await _dialogService.ShowErrorAsync("Error", $"Failed to export: {ex.Message}");
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

        public async Task<bool> ConfirmCloseWithUnsavedChanges()
        {
            return await _dialogService.ShowConfirmationAsync(
                "Unsaved Changes",
                "You have unsaved changes. Do you want to close without saving?"
            );
        }

        [RelayCommand]
        private async Task OpenSettings()
        {
            var settingsWindow = new SettingsWindow
            {
                DataContext = new SettingsViewModel(_settingsService)
            };

            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop && desktop.MainWindow != null)
            {
                await settingsWindow.ShowDialog(desktop.MainWindow);
            }
        }

        [RelayCommand]
        private Task CheckForUpdates()
        {
            return CheckForUpdatesInternal(true);
        }

        private async Task CheckForUpdatesInternal(bool isUserInitiated)
        {
            try
            {
                if (!isUserInitiated && !await _updateService.ShouldCheckForUpdates())
                {
                    return;
                }

                var updateInfo = await _updateService.CheckForUpdatesAsync();
                if (updateInfo == null)
                {
                    if (isUserInitiated)
                    {
                        await _dialogService.ShowInfoAsync("Update Check", "Failed to check for updates.");
                    }
                    return;
                }

                var currentVersion = typeof(MainWindowViewModel).Assembly.GetName().Version?.ToString() ?? "1.0.0";
                if (_updateService.IsUpdateAvailable(currentVersion, updateInfo.Version))
                {
                    _updateInfo = updateInfo;
                    UpdateVersion = updateInfo.Version;
                    UpdateAvailable = true;

                    if (isUserInitiated)
                    {
                        var result = await _dialogService.ShowConfirmationAsync(
                            "Update Available",
                            $"Version {updateInfo.Version} is available. Would you like to view the release notes?");

                        if (result)
                        {
                            await OpenUpdateInfo();
                        }
                    }
                }
                else
                {
                    if (isUserInitiated)
                    {
                        await _dialogService.ShowInfoAsync("Update Check", "You are using the latest version.");
                    }
                }

                await _updateService.UpdateLastCheckTime();
            }
            catch (Exception ex)
            {
                if (isUserInitiated)
                {
                    await _dialogService.ShowErrorAsync("Error", $"Failed to check for updates: {ex.Message}");
                }
            }
        }

        [RelayCommand]
        private async Task OpenUpdateInfo()
        {
            if (_updateInfo != null)
            {
                var result = await _dialogService.ShowConfirmationAsync(
                    $"Update {_updateInfo.Version}",
                    $"Release Notes:\n{_updateInfo.ReleaseNotes}\n\nWould you like to download the update?");

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
                        await _dialogService.ShowErrorAsync("Error", $"Failed to open download URL: {ex.Message}");
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

        private void BuildFormatMenu()
        {
            foreach (var format in PreviewFormats)
            {
                var menuItem = new MenuItem
                {
                    Header = format.Name,
                    Command = new RelayCommand<IPreviewFormat>(OnFormatSelected, f => f == format),
                    CommandParameter = format
                };
                FormatMenuItems.Add(menuItem);
            }
        }
    }
}
