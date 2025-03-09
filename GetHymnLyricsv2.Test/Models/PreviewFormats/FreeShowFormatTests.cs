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
    public class FreeShowFormatTests
    {
        private DataPacket? _dataPacket;
        private Song? _song;
        private List<OrderItem>? _sectionOrders;
        private FreeShowFormat? _freeShowFormat;
        private ISettingsService? _settingsService;
        private string _tempFilePath;
        private string _sampleXmlPath;

        [SetUp]
        public void Setup()
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
        }

        [TearDown]
        public void TearDown()
        {
            // Clean up the temporary file if it exists
            //if (File.Exists(_tempFilePath))
            //{
            //    File.Delete(_tempFilePath);
            //}
        }

        [Test]
        public void FreeShowFormat_Properties_ShouldHaveCorrectValues()
        {
            // Assert
            Assert.That(_freeShowFormat!.Name, Is.EqualTo("FreeShow"));
            Assert.That(_freeShowFormat.Description, Is.EqualTo("Export to FreeShow format"));
            Assert.That(_freeShowFormat.SupportsExport, Is.True);
            Assert.That(_freeShowFormat.SupportedFileExtensions, Is.EquivalentTo(new[] { ".show" }));
        }

        [Test]
        public async Task ExportToFileAsync_WithValidSong_ShouldCreateValidFreeShowFile()
        {
            // Arrange
            Assert.That(_song, Is.Not.Null, "Sample song should be loaded");
            Assert.That(_sectionOrders, Is.Not.Null, "Sample sections should be loaded");
            
            // Act
            await _freeShowFormat!.ExportToFileAsync(_song!, _sectionOrders!, _tempFilePath);
            
            // Assert
            Assert.That(File.Exists(_tempFilePath), Is.True, "Export file should be created");
            
            // Verify the file contains valid JSON array
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
            var showRoot = new ShowRoot { Show = show };
            Assert.That(showRoot, Is.Not.Null);
            Assert.That(showRoot!.Show, Is.Not.Null);
            Assert.That(showRoot.Show.Name, Is.EqualTo($"{_song!.Number} - {_song.Title}"));
            Assert.That(showRoot.Show.Category, Is.EqualTo("song"));
            
            // Verify slides were created
            Assert.That(showRoot.Show.Slides, Is.Not.Empty);
            
            // Verify there's a title slide
            var titleSlide = showRoot.Show.Slides.Values.FirstOrDefault(s =>
                s.Group == "Tag" && s.GlobalGroup == "tag");
            Assert.That(titleSlide, Is.Not.Null, "Title slide should exist");
            Assert.That(titleSlide!.Items.Count, Is.GreaterThan(0), "Title slide should have items");
            Assert.That(titleSlide.Items[0].Lines[0].Text[0].Value, Is.EqualTo($"{_song.Number} - {_song.Title}"));
            Assert.That(titleSlide.Color, Does.Match("^#[0-9A-F]{6}$"), "Title slide should have valid hex color");
            
            // Verify section slides were created
            var sectionSlides = showRoot.Show.Slides.Values.Where(s =>
                s.Group == "Verse" || s.Group == "Chorus").ToList();
            Assert.That(sectionSlides, Is.Not.Empty, "Section slides should exist");

            // Get the last section slide
            var lastSectionSlide = sectionSlides.Last();
            
            // Verify either there are child slides or all sections fit in one slide
            var slideWithChildren = sectionSlides.FirstOrDefault(s => s.Children.Count > 0);
            if (slideWithChildren != null)
            {
                // If we have child slides, verify they exist in slides dictionary
                var childSlideId = slideWithChildren.Children[0];
                Assert.That(showRoot.Show.Slides.ContainsKey(childSlideId), Is.True, "Child slide should exist in slides dictionary");
                
                // If this is the last section slide and last section symbol is enabled, verify child slides have the symbol
                if (slideWithChildren == lastSectionSlide && _settingsService!.Settings.LastSectionSymbolLocation != SectionSymbolLocation.None
                    && !string.IsNullOrWhiteSpace(_settingsService.Settings.LastSectionSymbol))
                {
                    foreach (var childId in slideWithChildren.Children)
                    {
                        var childSlide = showRoot.Show.Slides[childId];
                        var lastItem = childSlide.Items.Last();
                        Assert.That(lastItem.Lines[0].Text[0].Value, Is.EqualTo(_settingsService.Settings.LastSectionSymbol),
                            "Child slides of last section should have the last section symbol");
                    }
                }
            }
            
            // Verify last section symbol on main slide if enabled
            if (_settingsService.Settings.LastSectionSymbolLocation != SectionSymbolLocation.None
                && !string.IsNullOrWhiteSpace(_settingsService.Settings.LastSectionSymbol))
            {
                var lastSectionItems = lastSectionSlide.Items;
                Assert.That(lastSectionItems.Count, Is.GreaterThan(1), "Last section should have symbol item");
                var lastItem = lastSectionItems.Last();
                Assert.That(lastItem.Lines[0].Text[0].Value, Is.EqualTo(_settingsService.Settings.LastSectionSymbol),
                    "Last section should have the last section symbol");
                Assert.That(lastItem.Lines[0].Align, Does.Contain("text-align: right"), "Last section symbol should be right-aligned");
            }
            
            // Verify layouts were created
            Assert.That(showRoot.Show.Layouts, Is.Not.Empty);
            var layout = showRoot.Show.Layouts.Values.First();
            Assert.That(layout.Slides.Count,
                Is.LessThan(showRoot.Show.Slides.Count), "Layout should only include slides with Group property");
            Assert.That(layout.Name, Is.EqualTo("default"));
        }

        [Test]
        public void ExportToFileAsync_WithInvalidExtension_ShouldThrowArgumentException()
        {
            // Arrange
            var invalidPath = Path.ChangeExtension(_tempFilePath, ".txt");
            
            // Act & Assert
            var exception = Assert.ThrowsAsync<ArgumentException>(async () => 
                await _freeShowFormat!.ExportToFileAsync(_song!, _sectionOrders!, invalidPath));
            
            Assert.That(exception!.Message, Does.Contain("Invalid file extension"));
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
