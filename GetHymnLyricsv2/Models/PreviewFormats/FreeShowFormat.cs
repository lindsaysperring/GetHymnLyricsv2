using GetHymnLyricsv2.Models.FreeShow;
using GetHymnLyricsv2.Models.PreviewFormats.Base;
using GetHymnLyricsv2.Services;
using GetHymnLyricsv2.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using static GetHymnLyricsv2.ViewModels.MainWindowViewModel;

namespace GetHymnLyricsv2.Models.PreviewFormats
{
    public class FreeShowFormat : PreviewFormatBase
    {
        public override string Name => "FreeShow";
        public override string Description => "Export to FreeShow format";
        public override bool SupportsExport => true;
        public override string[] SupportedFileExtensions => new[] { ".show" };
        private const int linesPerSlide = 2;

        public FreeShowFormat(ISettingsService settingsService) : base(settingsService)
        {
        }

        // Override export to implement FreeShow-specific format
        public override async Task ExportToFileAsync(Song song, IEnumerable<OrderItem> sectionOrders, string filePath)
        {
            ValidateFileExtension(filePath);
            var xml = GenerateFreeShowJson(song, sectionOrders);
            await File.WriteAllTextAsync(filePath, xml);
        }

        private string GenerateFreeShowJson(Song song, IEnumerable<OrderItem> sectionOrders)
        {
            _settingsService.LoadSettings();
            var hasEndSymbol = _settingsService.Settings.LastSectionSymbolLocation != SectionSymbolLocation.None && !string.IsNullOrWhiteSpace(_settingsService.Settings.LastSectionSymbol);
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var layoutId = UidGenerator.Generate();
            var showRoot = new ShowRoot()
            {
                Show = new Show
                {
                    Name = BuildSongHeader(song),
                    Category = "song",
                    Settings = new ShowSettings()
                    {
                        ActiveLayout = layoutId,
                        Template = "default"
                    },
                    Timestamps = new Timestamps()
                    {
                        Created = timestamp,
                        Modified = timestamp,
                        Used = timestamp
                    },
                }
            };

            var slides = new Dictionary<string, Slide>();

            var titleSlide = new Slide
            {
                Group = "Tag",
                Color = GenerateRandomHexColor(),
                GlobalGroup = "tag",
                Settings = new Dictionary<string, object>(),
                Notes = string.Empty,
                Items = new List<SlideItem>()
                {
                    new SlideItem
                    {
                        Lines = new List<Line>()
                        {
                            new Line
                            {
                                Text = new List<TextSegment>
                                {
                                    new TextSegment
                                    {
                                        Value = BuildSongHeader(song)
                                    }
                                }
                            }
                        }
                    }
                },
                Children = new List<string>()
            };

            slides.Add(UidGenerator.Generate(), titleSlide);

            foreach (var sectionOrder in sectionOrders)
            {
                var section = sectionOrder.Section;
                var tag = section.SectionName.Equals("Refrain", StringComparison.OrdinalIgnoreCase)
                    ? "Chorus"
                    : "Verse";

                var slide = new Slide
                {
                    Group = tag,
                    Color = GenerateRandomHexColor(),
                    GlobalGroup = tag.ToLower(),
                    Settings = new Dictionary<string, object>(),
                    Notes = string.Empty,
                    Items = new List<SlideItem>(),
                    Children = new List<string>()
                };

                slides.Add(UidGenerator.Generate(), slide);

                var isLastSection = sectionOrder == sectionOrders.Last();

                var lines = section.SectionText.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

                var endSymbolItem = new SlideItem
                {
                    Style = "top:940.03px;left:1411.55px;height:139.97px;width:487.45px;",
                    Lines = new List<Line>
                    {
                        new Line
                        {
                            Align = "text-align: right",
                            Text = new List<TextSegment>
                            {
                                new TextSegment
                                {
                                    Value = _settingsService.Settings.LastSectionSymbol
                                }
                            }
                        }
                    }

                };

                for (int i = 0; i < lines.Length; i += linesPerSlide)
                {
                    var slideItem = new SlideItem
                    {
                        Lines = new List<Line>()
                    };
                    for (int j = 0; j < linesPerSlide && i + j < lines.Length; j++)
                    {
                        slideItem.Lines.Add(new Line
                        {
                            Text = new List<TextSegment>
                            {
                                new TextSegment
                                {
                                    Value = lines[i + j]
                                }
                            }
                        });
                    }
                    if (i == 0)
                    {
                        slide.Items.Add(slideItem);
                        if (hasEndSymbol && isLastSection)
                        {
                            slide.Items.Add(endSymbolItem);
                        }
                    }
                    else
                    {
                        var childSlideId = UidGenerator.Generate();
                        var childSlide = new Slide()
                        {
                            Items = new List<SlideItem>()
                            {
                                slideItem
                            }
                        };
                        slides.Add(childSlideId, childSlide);
                        slide.Children.Add(childSlideId);
                        if (hasEndSymbol && isLastSection)
                        {
                            childSlide.Items.Add(endSymbolItem);
                        }
                    }
                }
            }

            showRoot.Show.Slides = slides;
            showRoot.Show.Layouts = new Dictionary<string, Layout>
            {
                {
                    layoutId,
                    new Layout
                    {
                        Name = "default",
                        Slides = slides.Where(k => k.Value.Group != null).Select(x => new LayoutSlide{Id = x.Key}).ToList()
                    }
                }
            };

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var output = new List<object>();
            output.Add(showRoot.Id);
            output.Add(showRoot.Show);

            return JsonSerializer.Serialize(output, options);
        }

        private string GenerateRandomHexColor()
        {
            Random random = new Random();
            return $"#{random.Next(0x1000000):X6}";
        }
    }
}