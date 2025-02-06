using CommunityToolkit.Mvvm.ComponentModel;
using System.Text.Json.Serialization;

namespace GetHymnLyricsv2.Models
{
    public class UserSettings : ObservableObject
    {
        private string _lastSectionSymbol = "Ω";
        private SectionSymbolLocation _lastSectionSymbolLocation = SectionSymbolLocation.None;

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

        [JsonConstructor]
        public UserSettings() { }
    }
}
