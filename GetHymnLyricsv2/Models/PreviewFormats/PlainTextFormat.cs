using GetHymnLyricsv2.Models.PreviewFormats.Base;
using GetHymnLyricsv2.Services;

namespace GetHymnLyricsv2.Models.PreviewFormats;

public class PlainTextFormat : PreviewFormatBase
{
    public PlainTextFormat(ISettingsService settingsService) : base(settingsService)
    {
    }

    public override string Name => "Plain Text";
    public override string Description => "Simple text format without formatting";

    // Uses all the default implementations
}