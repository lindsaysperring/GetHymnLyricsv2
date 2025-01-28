﻿using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GetHymnLyricsv2.Models;

namespace GetHymnLyricsv2.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        [ObservableProperty]
        private DataPacket? dataPacket;

        [ObservableProperty]
        private ObservableCollection<Song> songs = new();

        [ObservableProperty]
        private Song? selectedSong;

        [ObservableProperty]
        private ObservableCollection<SongSection> currentSongSections = new();

        [ObservableProperty]
        private SongSection? selectedSection;

        private string? currentFilePath;

        public MainWindowViewModel()
        {
            LoadSampleData();
        }

        private void LoadSampleData()
        {
            var samplePath = Path.Combine("Data", "sample.xml");
            if (File.Exists(samplePath))
            {
                LoadFile(samplePath);
            }
        }

        [RelayCommand]
        private async Task OpenFile(Window window)
        {
            var options = new FilePickerOpenOptions
            {
                Title = "Open Hymn File",
                AllowMultiple = false,
                FileTypeFilter = new[]
                {
                    new FilePickerFileType("XML Files")
                    {
                        Patterns = new[] { "*.xml" }
                    }
                }
            };

            var result = await window.StorageProvider.OpenFilePickerAsync(options);
            if (result.Count > 0)
            {
                LoadFile(result[0].Path.LocalPath);
            }
        }

        [RelayCommand]
        private async Task SaveFile(Window window)
        {
            if (DataPacket == null) return;

            if (string.IsNullOrEmpty(currentFilePath))
            {
                var options = new FilePickerSaveOptions
                {
                    Title = "Save Hymn File",
                    DefaultExtension = ".xml",
                    FileTypeChoices = new[]
                    {
                        new FilePickerFileType("XML Files")
                        {
                            Patterns = new[] { "*.xml" }
                        }
                    }
                };

                var result = await window.StorageProvider.SaveFilePickerAsync(options);
                if (result != null)
                {
                    currentFilePath = result.Path.LocalPath;
                }
                else
                {
                    return;
                }
            }

            SaveToFile(currentFilePath);
        }

        private void LoadFile(string filePath)
        {
            try
            {
                var serializer = new XmlSerializer(typeof(DataPacket));
                using var reader = new StreamReader(filePath);
                DataPacket = (DataPacket)serializer.Deserialize(reader)!;
                currentFilePath = filePath;

                Songs.Clear();
                if (DataPacket?.RowData?.Row?.Songs?.Items != null)
                {
                    foreach (var song in DataPacket.RowData.Row.Songs.Items.OrderBy(s => s.Number))
                    {
                        Songs.Add(song);
                    }
                }
            }
            catch (Exception ex)
            {
                // TODO: Show error dialog
                Console.WriteLine($"Error loading file: {ex.Message}");
            }
        }

        private void SaveToFile(string filePath)
        {
            try
            {
                if (DataPacket == null) return;

                var serializer = new XmlSerializer(typeof(DataPacket));
                using var writer = new StreamWriter(filePath);
                serializer.Serialize(writer, DataPacket);
            }
            catch (Exception ex)
            {
                // TODO: Show error dialog
                Console.WriteLine($"Error saving file: {ex.Message}");
            }
        }

        partial void OnSelectedSongChanged(Song? value)
        {
            if (value != null && DataPacket?.RowData?.Row?.SongSections?.Items != null)
            {
                var sections = DataPacket.RowData.Row.SongSections.Items
                    .Where(s => s.SongId == value.SongId)
                    .OrderBy(s => DataPacket.RowData.Row.SongSectionOrder.Items
                        .FirstOrDefault(o => o.SongId == value.SongId && o.SectionId == s.SectionId)?.Order ?? 0)
                    .ToList();

                CurrentSongSections.Clear();
                foreach (var section in sections)
                {
                    CurrentSongSections.Add(section);
                }
            }
            else
            {
                CurrentSongSections.Clear();
            }
        }
    }
}
