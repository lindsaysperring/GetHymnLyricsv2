using System;
using CommunityToolkit.Mvvm.ComponentModel;
using GetHymnLyricsv2.Models;
using GetHymnLyricsv2.Services;
using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.Input;
using Avalonia.Controls;

namespace GetHymnLyricsv2.ViewModels
{
    public partial class SettingsViewModel : ViewModelBase
    {
        private readonly ISettingsService _settingsService;

        [ObservableProperty]
        private string _lastSectionSymbol;

        [ObservableProperty]
        private SectionSymbolLocation _lastSectionSymbolLocation;

        public IEnumerable<SectionSymbolLocation> SymbolLocations => 
            Enum.GetValues(typeof(SectionSymbolLocation))
                .Cast<SectionSymbolLocation>();

        public SettingsViewModel(ISettingsService settingsService)
        {
            _settingsService = settingsService;
            LoadSettings();
        }

        private void LoadSettings()
        {
            LastSectionSymbol = _settingsService.Settings.LastSectionSymbol;
            LastSectionSymbolLocation = _settingsService.Settings.LastSectionSymbolLocation;
        }

        [RelayCommand]
        private void Save(Window window)
        {
            _settingsService.Settings.LastSectionSymbol = LastSectionSymbol;
            _settingsService.Settings.LastSectionSymbolLocation = LastSectionSymbolLocation;
            _settingsService.SaveSettings();
            window.Close();
        }

        [RelayCommand]
        private void Cancel(Window window)
        {
            LoadSettings();
            window.Close();
        }
    }
}
