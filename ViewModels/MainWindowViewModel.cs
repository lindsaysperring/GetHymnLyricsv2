﻿using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GetHymnLyricsv2.Models;
using GetHymnLyricsv2.Services;

namespace GetHymnLyricsv2.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        private readonly IFileService _fileService;
        private readonly ISongService _songService;
        private readonly IDialogService _dialogService;
        private string? _currentFilePath;

        [ObservableProperty]
        private DataPacket? dataPacket;

        [ObservableProperty]
        private ObservableCollection<Song> songs = new();

        [ObservableProperty]
        private Song? selectedSong;

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
            SongDetailsViewModel songDetails,
            SongSectionsViewModel songSections)
        {
            _fileService = fileService;
            _songService = songService;
            _dialogService = dialogService;
            SongDetails = songDetails;
            SongSections = songSections;

            LoadSampleData();
        }

        private async void LoadSampleData()
        {
            var samplePath = Path.Combine("Data", "sample.xml");
            if (File.Exists(samplePath))
            {
                await LoadFileAsync(samplePath);
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
            }
            catch (Exception ex)
            {
                await _dialogService.ShowErrorAsync("Error", $"Failed to save file: {ex.Message}", window);
            }
        }

        private async Task LoadFileAsync(string filePath)
        {
            try
            {
                DataPacket = await _fileService.LoadFileAsync(filePath);
                _currentFilePath = filePath;

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
