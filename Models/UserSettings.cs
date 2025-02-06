using System.Text.Json.Serialization;

namespace GetHymnLyricsv2.Models
{
    public class UserSettings
    {
        public string LastSectionSymbol { get; set; } = "Ω";
        public SectionSymbolLocation LastSectionSymbolLocation { get; set; } = SectionSymbolLocation.None;

        [JsonConstructor]
        public UserSettings() { }
    }
}
