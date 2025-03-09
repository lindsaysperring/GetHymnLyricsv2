using System.Collections.Generic;
using System.Text.Json.Serialization;
using GetHymnLyricsv2.Utilities;

namespace GetHymnLyricsv2.Models.FreeShow
{
    public class ShowRoot
    {
        public string Id { get; set; } = UidGenerator.Generate();
        public Show Show { get; set; } = default!;
    }

    public class Show
    {
        public string Name { get; set; } = default!;
        public bool Private { get; set; }
        public string Category { get; set; } = "show";
        public ShowSettings Settings { get; set; } = default!;
        public Timestamps Timestamps { get; set; } = default!;
        public Dictionary<string, object> QuickAccess { get; set; } = new();
        public Dictionary<string, object> Meta { get; set; } = new();
        public Dictionary<string, Slide> Slides { get; set; } = new();
        public Dictionary<string, Layout> Layouts { get; set; } = new();
        public Dictionary<string, object> Media { get; set; } = new();
    }

    public class ShowSettings
    {
        public string ActiveLayout { get; set; } = default!;
        public string Template { get; set; } = default!;
    }

    public class Timestamps
    {
        public long Created { get; set; }
        public long Modified { get; set; }
        public long Used { get; set; }
    }

    // Other classes remain unchanged

    public class Slide
    {
        public string? Group { get; set; }
        public string? Color { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? GlobalGroup { get; set; }
        public Dictionary<string, object> Settings { get; set; } = new();
        public string Notes { get; set; } = string.Empty;
        public List<SlideItem> Items { get; set; } = new();

        [JsonIgnore]
        public List<string> Children { get; set; } = new();

        [JsonPropertyName("children")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<string>? ChildrenForSerialization => Children.Count > 0 ? Children : null;
    }

    public class SlideItem
    {
        public string Type { get; set; } = "text";
        public List<Line> Lines { get; set; } = new();
        public string Style { get; set; } = "top:120px;left:50px;height:840px;width:1820px;";
        public string Align { get; set; } = string.Empty;
        public bool Auto { get; set; }
    }

    public class Line
    {
        public string Align { get; set; } = string.Empty;
        public List<TextSegment> Text { get; set; } = new();
    }

    public class TextSegment
    {
        public string Value { get; set; } = default!;
        public string Style { get; set; } = "font-size: 100px;";
    }

    public class Layout
    {
        public string Name { get; set; } = "Default";
        public string Notes { get; set; } = string.Empty;
        public List<LayoutSlide> Slides { get; set; } = new();
    }

    public class LayoutSlide
    {
        public string Id { get; set; } = default!;
    }
}
