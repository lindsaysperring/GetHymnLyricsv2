﻿﻿﻿﻿﻿﻿﻿using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
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
        private ObservableCollection<OrderItem> songOrder = new();

        [ObservableProperty]
        private SongSection? selectedSection;

        [ObservableProperty]
        private int nextSectionId;

        public class OrderItem
        {
            public required SongSection Section { get; init; }
            public required SectionOrder OrderEntry { get; init; }
        }

        private string? currentFilePath;

        private void UpdateNextSectionId()
        {
            if (DataPacket?.RowData?.Row?.SongSections?.Items != null)
            {
                NextSectionId = DataPacket.RowData.Row.SongSections.Items.Max(s => s.SectionId) + 1;
            }
            else
            {
                NextSectionId = 1;
            }
        }

        [RelayCommand]
        private void AddSection()
        {
            if (SelectedSong == null || DataPacket?.RowData?.Row?.SongSections?.Items == null) return;

            var newSection = new SongSection
            {
                SongId = SelectedSong.SongId,
                SectionId = NextSectionId,
                SectionName = "New Section",
                SectionText = "",
                SectionComments = ""
            };

            DataPacket.RowData.Row.SongSections.Items.Add(newSection);

            // Add to section order
            var maxOrder = DataPacket.RowData.Row.SongSectionOrder.Items
                .Where(o => o.SongId == SelectedSong.SongId)
                .DefaultIfEmpty()
                .Max(o => o?.Order ?? -1) + 1;

            DataPacket.RowData.Row.SongSectionOrder.Items.Add(new SectionOrder
            {
                SongId = SelectedSong.SongId,
                SectionId = NextSectionId,
                Order = maxOrder
            });

            NextSectionId++;
            OnSelectedSongChanged(SelectedSong);
        }

        [RelayCommand]
        private void RemoveSection(SongSection section)
        {
            if (DataPacket?.RowData?.Row?.SongSections?.Items == null) return;

            // Remove section
            DataPacket.RowData.Row.SongSections.Items.Remove(section);

            // Remove from order and reorder remaining sections
            var ordersToRemove = DataPacket.RowData.Row.SongSectionOrder.Items
                .Where(o => o.SongId == section.SongId && o.SectionId == section.SectionId)
                .ToList();

            foreach (var order in ordersToRemove)
            {
                DataPacket.RowData.Row.SongSectionOrder.Items.Remove(order);
            }

            // Update UI
            OnSelectedSongChanged(SelectedSong);
        }

        [RelayCommand]
        private void AddToOrder()
        {
            if (SelectedSong == null || SelectedSection == null || 
                DataPacket?.RowData?.Row?.SongSectionOrder?.Items == null) return;

            // Get the next order number
            var maxOrder = DataPacket.RowData.Row.SongSectionOrder.Items
                .Where(o => o.SongId == SelectedSong.SongId)
                .DefaultIfEmpty()
                .Max(o => o?.Order ?? -1) + 1;

            // Create new order entry
            var newOrder = new SectionOrder
            {
                SongId = SelectedSong.SongId,
                SectionId = SelectedSection.SectionId,
                Order = maxOrder
            };

            DataPacket.RowData.Row.SongSectionOrder.Items.Add(newOrder);
            UpdateSongOrder();
        }

        [RelayCommand]
        private void MoveOrderUp(OrderItem orderItem)
        {
            if (DataPacket?.RowData?.Row?.SongSectionOrder?.Items == null || SelectedSong == null) return;

            var previousOrder = DataPacket.RowData.Row.SongSectionOrder.Items
                .Where(o => o.SongId == orderItem.OrderEntry.SongId && o.Order < orderItem.OrderEntry.Order)
                .OrderByDescending(o => o.Order)
                .FirstOrDefault();

            if (previousOrder != null)
            {
                // Swap orders
                (orderItem.OrderEntry.Order, previousOrder.Order) = (previousOrder.Order, orderItem.OrderEntry.Order);
                UpdateSongOrder();
            }
        }

        [RelayCommand]
        private void MoveOrderDown(OrderItem orderItem)
        {
            if (DataPacket?.RowData?.Row?.SongSectionOrder?.Items == null || SelectedSong == null) return;

            var nextOrder = DataPacket.RowData.Row.SongSectionOrder.Items
                .Where(o => o.SongId == orderItem.OrderEntry.SongId && o.Order > orderItem.OrderEntry.Order)
                .OrderBy(o => o.Order)
                .FirstOrDefault();

            if (nextOrder != null)
            {
                // Swap orders
                (orderItem.OrderEntry.Order, nextOrder.Order) = (nextOrder.Order, orderItem.OrderEntry.Order);
                UpdateSongOrder();
            }
        }

        [RelayCommand]
        private void RemoveFromOrder(OrderItem orderItem)
        {
            if (DataPacket?.RowData?.Row?.SongSectionOrder?.Items == null) return;

            // Remove the order entry
            DataPacket.RowData.Row.SongSectionOrder.Items.Remove(orderItem.OrderEntry);

            // Reorder remaining entries
            var remainingOrders = DataPacket.RowData.Row.SongSectionOrder.Items
                .Where(o => o.SongId == orderItem.OrderEntry.SongId)
                .OrderBy(o => o.Order)
                .ToList();

            for (int i = 0; i < remainingOrders.Count; i++)
            {
                remainingOrders[i].Order = i;
            }

            UpdateSongOrder();
        }

        private void UpdateSongOrder()
        {
            if (SelectedSong == null || DataPacket?.RowData?.Row?.SongSections?.Items == null) return;

            var orders = DataPacket.RowData.Row.SongSectionOrder.Items
                .Where(o => o.SongId == SelectedSong.SongId)
                .OrderBy(o => o.Order)
                .ToList();

            SongOrder.Clear();
            foreach (var order in orders)
            {
                var section = DataPacket.RowData.Row.SongSections.Items
                    .FirstOrDefault(s => s.SongId == order.SongId && s.SectionId == order.SectionId);
                
                if (section != null)
                {
                    SongOrder.Add(new OrderItem
                    {
                        Section = section,
                        OrderEntry = order
                    });
                }
            }
        }

        public MainWindowViewModel()
        {
            LoadSampleData();
            UpdateNextSectionId();
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
                var settings = new XmlReaderSettings
                {
                    IgnoreWhitespace = false
                };

                var serializer = new XmlSerializer(typeof(DataPacket));
                using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                using var reader = XmlReader.Create(fileStream, settings);
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
                var settings = new System.Xml.XmlWriterSettings
                {
                    Indent = true,
                    IndentChars = "  ",
                    NewLineChars = Environment.NewLine,
                    NewLineHandling = System.Xml.NewLineHandling.Replace
                };

                using var writer = System.Xml.XmlWriter.Create(filePath, settings);
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
                // Load sections
                var sections = DataPacket.RowData.Row.SongSections.Items
                    .Where(s => s.SongId == value.SongId)
                    .ToList();

                CurrentSongSections.Clear();
                foreach (var section in sections)
                {
                    CurrentSongSections.Add(section);
                }

                // Load order
                UpdateSongOrder();
            }
            else
            {
                CurrentSongSections.Clear();
                SongOrder.Clear();
            }
        }
    }
}
