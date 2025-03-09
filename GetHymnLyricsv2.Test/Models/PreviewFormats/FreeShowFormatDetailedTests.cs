using System.Text.Json;
using System.Xml.Serialization;
using GetHymnLyricsv2.Models;
using GetHymnLyricsv2.Models.FreeShow;
using GetHymnLyricsv2.Models.PreviewFormats;
using GetHymnLyricsv2.Services;
using Moq;
using static GetHymnLyricsv2.ViewModels.MainWindowViewModel;

namespace GetHymnLyricsv2.Test.Models.PreviewFormats
{
    [TestFixture]
    public class FreeShowFormatDetailedTests
    {
        private DataPacket? _dataPacket;
        private Song? _song;
        private List<OrderItem>? _sectionOrders;
        private FreeShowFormat? _freeShowFormat;
        private ISettingsService? _settingsService;
        private string _tempFilePath;
        private string _sampleXmlPath;
        private ShowRoot? _exportedShowRoot;

        [SetUp]
        public async Task Setup()
        {
            // Setup mock SettingsService
            var mockSettings = new Mock<ISettingsService>();
            var settings = new UserSettings
            {
                LastSectionSymbol = "§",
                LastSectionSymbolLocation = SectionSymbolLocation.Start
            };
            mockSettings.Setup(s => s.Settings).Returns(settings);
            mockSettings.Setup(s => s.LoadSettings()).Callback(() => { }); // Do nothing when LoadSettings is called

            _settingsService = mockSettings.Object;
            _freeShowFormat = new FreeShowFormat(_settingsService);
            
            // Create a temporary file path for testing export
            _tempFilePath = Path.Combine(Path.GetTempPath(), $"test_export_{Guid.NewGuid()}.show");
            
            // Copy sample.xml to the test output directory
            CopySampleXmlToTestDirectory();
            
            // Load the sample data
            LoadSampleData();
            
            // Export the song to FreeShow format for testing
            if (_song != null && _sectionOrders != null)
            {
                await _freeShowFormat.ExportToFileAsync(_song, _sectionOrders, _tempFilePath);
                var fileContent = await File.ReadAllTextAsync(_tempFilePath);
                var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
                
                // Deserialize as JSON array
                var jsonArray = JsonSerializer.Deserialize<object[]>(fileContent, options);
                Assert.That(jsonArray, Is.Not.Null);
                Assert.That(jsonArray.Length, Is.EqualTo(2), "JSON should contain 2 elements: id and show");

                // Second element should be the show object
                var showJson = JsonSerializer.Serialize(jsonArray[1], options);
                var show = JsonSerializer.Deserialize<Show>(showJson, options);
                
                // Create ShowRoot for compatibility with existing tests
                _exportedShowRoot = new ShowRoot { Show = show };
            }
        }

        [TearDown]
        public void TearDown()
        {
            // Clean up the temporary file if it exists
            if (File.Exists(_tempFilePath))
            {
                File.Delete(_tempFilePath);
            }
        }

        [Test]
        public void ExportedFile_ShouldHaveCorrectShowStructure()
        {
            // Assert
            Assert.That(_exportedShowRoot, Is.Not.Null);
            Assert.That(_exportedShowRoot!.Id, Is.Not.Null.Or.Empty);
            Assert.That(_exportedShowRoot.Show, Is.Not.Null);
            
            // Check show properties
            var show = _exportedShowRoot.Show;
            Assert.That(show.Name, Is.EqualTo($"{_song!.Number} - {_song.Title}"));
            Assert.That(show.Category, Is.EqualTo("song"));
            Assert.That(show.Settings, Is.Not.Null);
            Assert.That(show.Settings.Template, Is.EqualTo("default"));
            Assert.That(show.Timestamps, Is.Not.Null);
            Assert.That(show.Timestamps.Created, Is.GreaterThan(0));
            Assert.That(show.Timestamps.Modified, Is.GreaterThan(0));
            Assert.That(show.Timestamps.Used, Is.GreaterThan(0));
        }

        [Test]
        public void ExportedFile_ShouldHaveCorrectSlideStructure()
        {
            // Assert
            Assert.That(_exportedShowRoot, Is.Not.Null);
            Assert.That(_exportedShowRoot!.Show.Slides, Is.Not.Empty);
            
            // Check that we have the expected number of slides
            // Title slide + one slide per section (at minimum)
            int expectedMinimumSlides = 1 + _sectionOrders!.Count;
            Assert.That(_exportedShowRoot.Show.Slides.Count, Is.GreaterThanOrEqualTo(expectedMinimumSlides));
            
            // Check title slide
            var titleSlide = _exportedShowRoot.Show.Slides.Values.FirstOrDefault(s => 
                s.Group == "Tag" && s.GlobalGroup == "tag");
            Assert.That(titleSlide, Is.Not.Null);
            Assert.That(titleSlide!.Items, Has.Count.GreaterThanOrEqualTo(1));
            Assert.That(titleSlide.Items[0].Lines, Has.Count.GreaterThanOrEqualTo(1));
            Assert.That(titleSlide.Items[0].Lines[0].Text, Has.Count.GreaterThanOrEqualTo(1));
            Assert.That(titleSlide.Items[0].Lines[0].Text[0].Value, Is.EqualTo($"{_song!.Number} - {_song.Title}"));
            
            // Check verse slides
            var verseSlides = _exportedShowRoot.Show.Slides.Values.Where(s => s.Group == "Verse").ToList();
            var verseSections = _sectionOrders!.Select(so => so.Section).Where(s => !s.SectionName.Equals("Refrain", StringComparison.OrdinalIgnoreCase)).ToList();
            Assert.That(verseSlides.Count, Is.GreaterThanOrEqualTo(verseSections.Count));
            
            // Check chorus slides
            var chorusSlides = _exportedShowRoot.Show.Slides.Values.Where(s => s.Group == "Chorus").ToList();
            var chorusSections = _sectionOrders!.Select(so => so.Section).Where(s => s.SectionName.Equals("Refrain", StringComparison.OrdinalIgnoreCase)).ToList();
            Assert.That(chorusSlides.Count, Is.GreaterThanOrEqualTo(chorusSections.Count));
        }

        [Test]
        public void ExportedFile_ShouldHaveCorrectSectionNameConversion()
        {
            // Find all refrain sections
            var refrainSections = _sectionOrders!.Select(so => so.Section).Where(s => 
                s.SectionName.Equals("Refrain", StringComparison.OrdinalIgnoreCase)).ToList();
            
            if (refrainSections.Any())
            {
                // Check that all refrain sections are converted to "Chorus" in the FreeShow format
                var chorusSlides = _exportedShowRoot!.Show.Slides.Values
                    .Where(s => s.Group == "Chorus" && s.GlobalGroup == "chorus").ToList();
                
                Assert.That(chorusSlides, Is.Not.Empty, "Refrain sections should be converted to Chorus slides");
            }
        }

        [Test]
        public void ExportedFile_ShouldHaveCorrectLayoutStructure()
        {
            // Assert
            Assert.That(_exportedShowRoot, Is.Not.Null);
            Assert.That(_exportedShowRoot!.Show.Layouts, Is.Not.Empty);
            
            // Get the active layout
            var activeLayoutId = _exportedShowRoot.Show.Settings.ActiveLayout;
            Assert.That(activeLayoutId, Is.Not.Null.Or.Empty);
            Assert.That(_exportedShowRoot.Show.Layouts.ContainsKey(activeLayoutId), Is.True);
            
            var activeLayout = _exportedShowRoot.Show.Layouts[activeLayoutId];
            Assert.That(activeLayout.Name, Is.EqualTo("default"));
            // Only verify that the layout includes all slides that have a Group property
            var slidesWithGroup = _exportedShowRoot.Show.Slides.Values.Count(s => s.Group != null);
            Assert.That(activeLayout.Slides, Has.Count.EqualTo(slidesWithGroup));
            
            // Check that all slide IDs in the layout match keys in the slides dictionary
            foreach (var layoutSlide in activeLayout.Slides)
            {
                var slide = _exportedShowRoot.Show.Slides[layoutSlide.Id];
                Assert.That(slide.Group, Is.Not.Null, "Layout should only include slides with Group property");
                Assert.That(slide.Color, Does.Match("^#[0-9A-F]{6}$"), "Slide should have valid hex color");
            }
        }

        private void CopySampleXmlToTestDirectory()
        {
            // Source path in the project
            var sourcePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "GetHymnLyricsv2", "Data", "sample.xml");
            sourcePath = Path.GetFullPath(sourcePath);
            
            // Ensure the source file exists
            if (!File.Exists(sourcePath))
            {
                Assert.Fail($"Source sample XML file not found at {sourcePath}");
            }
            
            // Destination path in the test output directory
            var testDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
            Directory.CreateDirectory(testDirectory); // Ensure directory exists
            
            _sampleXmlPath = Path.Combine(testDirectory, "sample.xml");
            
            // Copy the file
            File.Copy(sourcePath, _sampleXmlPath, true);
            
            // Verify the file was copied
            Assert.That(File.Exists(_sampleXmlPath), Is.True, "Failed to copy sample.xml to test directory");
        }

        [Test]
        public void ExportedFile_ShouldHaveLastSectionSymbol()
        {
            // Assert show root exists
            Assert.That(_exportedShowRoot, Is.Not.Null);
            Assert.That(_exportedShowRoot!.Show.Slides, Is.Not.Empty);
            
            // Get all section slides
            var sectionSlides = _exportedShowRoot.Show.Slides.Values
                .Where(s => s.Group == "Verse" || s.Group == "Chorus")
                .ToList();
            
            // Get the last section slide
            var lastSectionSlide = sectionSlides.Last();
            Assert.That(lastSectionSlide, Is.Not.Null, "Last section slide should exist");
            
            // Verify last section symbol exists on main slide
            Assert.That(lastSectionSlide.Items.Count, Is.GreaterThan(1), "Last section should have symbol item");
            var lastItem = lastSectionSlide.Items.Last();
            Assert.That(lastItem.Lines[0].Text[0].Value, Is.EqualTo("§"), "Last section should have the section symbol");
            Assert.That(lastItem.Lines[0].Align, Is.EqualTo("text-align: right"), "Section symbol should be right-aligned");
            
            // If there are child slides, verify they also have the symbol
            if (lastSectionSlide.Children.Any())
            {
                foreach (var childId in lastSectionSlide.Children)
                {
                    var childSlide = _exportedShowRoot.Show.Slides[childId];
                    Assert.That(childSlide.Items.Count, Is.GreaterThan(1), "Child slide should have symbol item");
                    var childLastItem = childSlide.Items.Last();
                    Assert.That(childLastItem.Lines[0].Text[0].Value, Is.EqualTo("§"),
                        "Child slides of last section should have the section symbol");
                    Assert.That(childLastItem.Lines[0].Align, Is.EqualTo("text-align: right"),
                        "Section symbol in child slides should be right-aligned");
                }
            }
        }

        private void LoadSampleData()
        {
            // Ensure the file exists
            Assert.That(File.Exists(_sampleXmlPath), Is.True, $"Sample XML file not found at {_sampleXmlPath}");
            
            // Deserialize the XML
            var serializer = new XmlSerializer(typeof(DataPacket));
            using var fileStream = new FileStream(_sampleXmlPath, FileMode.Open);
            _dataPacket = (DataPacket)serializer.Deserialize(fileStream)!;
            
            // Get the first song
            _song = _dataPacket.RowData.Row.Songs.Items.FirstOrDefault();
            Assert.That(_song, Is.Not.Null, "No songs found in sample data");

            // Get the sections for the song in the correct order
            var sectionOrders = _dataPacket.RowData.Row.SongSectionOrder.Items
                .Where(so => so.SongId == _song!.SongId)
                .OrderBy(so => so.Order)
                .ToList();

            var sections = _dataPacket.RowData.Row.SongSections.Items.Where(s => s.SongId == _song.SongId);

            _sectionOrders = new List<OrderItem>();

            foreach (var order in sectionOrders)
            {
                var section = sections.FirstOrDefault(s => s.SectionId == order.SectionId);

                if (section == null)
                    continue;

                _sectionOrders.Add(new OrderItem()
                {
                    OrderEntry = order,
                    Section = section
                });
            }

            // Ensure we have sections
            Assert.That(_sectionOrders, Is.Not.Empty, "No sections found for the song");
        }
    }
}
