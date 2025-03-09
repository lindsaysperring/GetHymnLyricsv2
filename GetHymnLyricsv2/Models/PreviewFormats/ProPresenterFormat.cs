using GetHymnLyricsv2.Models.PreviewFormats.Base;
using GetHymnLyricsv2.Services;

namespace GetHymnLyricsv2.Models.PreviewFormats
{
    public class ProPresenterFormat : PreviewFormatBase
    {
        public ProPresenterFormat(ISettingsService settingsService) : base(settingsService)
        {
        }

        public override string Name => "ProPresenter";
        public override string Description => "Export to ProPresenter 6/7 format";
        public override bool SupportsExport => false;
        // public override string[] SupportedFileExtensions => new[] { ".pro", ".pro6" };

        // Override export to implement ProPresenter-specific format
    //     public override async Task ExportToFileAsync(Song song, IEnumerable<SongSection> sections, string filePath)
    //     {
    //         ValidateFileExtension(filePath);
    //         var xml = GenerateProPresenterXml(song, sections);
    //         await File.WriteAllTextAsync(filePath, xml);
    //     }

    //     private string GenerateProPresenterXml(Song song, IEnumerable<SongSection> sections)
    //     {
            // ProPresenter-specific XML generation
    //     }
    }
}