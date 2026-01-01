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

        [ObservableProperty]
        private int _linesPerSlide;

        public IEnumerable<SectionSymbolLocation> SymbolLocations => 
            Enum.GetValues(typeof(SectionSymbolLocation))
                .Cast<SectionSymbolLocation>();

        public event EventHandler? RequestClose;

        public SettingsViewModel(ISettingsService settingsService)
        {
            _settingsService = settingsService;
            LoadSettings();
        }

        private void LoadSettings()
        {
            LastSectionSymbol = _settingsService.Settings.LastSectionSymbol;
            LastSectionSymbolLocation = _settingsService.Settings.LastSectionSymbolLocation;
            LinesPerSlide = _settingsService.Settings.LinesPerSlide;
        }

        [RelayCommand]
        private void Save()
        {
            _settingsService.Settings.LastSectionSymbol = LastSectionSymbol;
            _settingsService.Settings.LastSectionSymbolLocation = LastSectionSymbolLocation;
            _settingsService.Settings.LinesPerSlide = LinesPerSlide;
            _settingsService.SaveSettings();
            RequestClose?.Invoke(this, EventArgs.Empty);
        }

        [RelayCommand]
        private void Cancel()
        {
            LoadSettings();
            RequestClose?.Invoke(this, EventArgs.Empty);
        }
    }
}
