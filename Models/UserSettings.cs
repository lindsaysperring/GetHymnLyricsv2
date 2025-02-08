using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Text.Json.Serialization;

namespace GetHymnLyricsv2.Models
{
    public class UserSettings : ObservableObject
    {
        private string _lastSectionSymbol = "Ω";
        private SectionSymbolLocation _lastSectionSymbolLocation = SectionSymbolLocation.None;
        private DateTime _lastUpdateCheck = DateTime.MinValue;
        private int _updateCheckFrequencyHours = 24;

        public string LastSectionSymbol
        {
            get => _lastSectionSymbol;
            set => SetProperty(ref _lastSectionSymbol, value);
        }

        public SectionSymbolLocation LastSectionSymbolLocation
        {
            get => _lastSectionSymbolLocation;
            set => SetProperty(ref _lastSectionSymbolLocation, value);
        }

        public DateTime LastUpdateCheck
        {
            get => _lastUpdateCheck;
            set => SetProperty(ref _lastUpdateCheck, value);
        }

        public int UpdateCheckFrequencyHours
        {
            get => _updateCheckFrequencyHours;
            set => SetProperty(ref _updateCheckFrequencyHours, value);
        }

        [JsonConstructor]
        public UserSettings() { }
    }
}
