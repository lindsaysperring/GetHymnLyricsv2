using Avalonia.Controls.Documents;
using GetHymnLyricsv2.Models;
using GetHymnLyricsv2.Models.PreviewFormats.Base;
using GetHymnLyricsv2.Services;
using static GetHymnLyricsv2.ViewModels.MainWindowViewModel;

namespace GetHymnLyricsv2.Test.Models.PreviewFormats.Base
{
    [TestFixture]
    public class PreviewFormatBaseTests
    {
        private TestPreviewFormat? _previewFormat;
        private Song? _song;
        private List<OrderItem>? _sectionOrders;
        private SettingsService? _settingsService;

        [SetUp]
        public void Setup()
        {
            _settingsService = new SettingsService();
            _settingsService.Settings.LastSectionSymbol = "§";
            _settingsService.Settings.LastSectionSymbolLocation = SectionSymbolLocation.Start;
            _previewFormat = new TestPreviewFormat(_settingsService);
            
            // Create a test song
            _song = new Song
            {
                SongId = 1,
                Number = 123,
                Title = "Test Song",
                WordsAuthor = "Test Author"
            };
            
            // Create test sections
            var sections = new List<SongSection>
            {
                new SongSection
                {
                    SongId = 1,
                    SectionId = 1,
                    SectionName = "Verse 1",
                    SectionText = "This is verse 1\nSecond line of verse 1"
                },
                new SongSection
                {
                    SongId = 1,
                    SectionId = 2,
                    SectionName = "Refrain",
                    SectionText = "This is the refrain\nSecond line of refrain"
                }
            };

            var orders = new List<SectionOrder>()
            {
                new SectionOrder()
                {
                    SongId = 1,
                    SectionId = 1,
                    Order = 1
                },
                new SectionOrder()
                {
                    SongId = 1,
                    SectionId = 2,
                    Order = 2
                }
            };

            _sectionOrders = new List<OrderItem>();

            foreach (var order in orders)
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
        }

        [Test]
        public void BuildSongHeader_ShouldFormatCorrectly()
        {
            // Act
            var result = _previewFormat!.TestBuildSongHeader(_song!);
            
            // Assert
            Assert.That(result, Is.EqualTo("123 - Test Song"));
        }

        [Test]
        public void FormatSectionName_ShouldConvertRefrainToChorus()
        {
            // Act
            var result = _previewFormat!.TestFormatSectionName("Refrain");
            
            // Assert
            Assert.That(result, Is.EqualTo("Chorus"));
        }

        [Test]
        public void FormatSectionName_ShouldNotChangeOtherSectionNames()
        {
            // Arrange
            var sectionNames = new[] { "Verse 1", "Bridge", "Ending" };
            
            // Act & Assert
            foreach (var name in sectionNames)
            {
                var result = _previewFormat!.TestFormatSectionName(name);
                Assert.That(result, Is.EqualTo(name));
            }
        }

        [Test]
        public void FormatForCopy_ShouldIncludeSongHeaderAndAllSections()
        {
            // Act
            var result = _previewFormat!.FormatForCopy(_song!, _sectionOrders!);
            
            // Assert
            Assert.That(result, Does.Contain("123 - Test Song"));
            Assert.That(result, Does.Contain("Verse 1"));
            Assert.That(result, Does.Contain("This is verse 1"));
            Assert.That(result, Does.Contain("Chorus")); // Converted from "Refrain"
            Assert.That(result, Does.Contain("This is the refrain"));
        }

        [Test]
        public void FormatPreview_ShouldReturnNonEmptyInlineCollection()
        {
            // Act
            var result = _previewFormat!.FormatPreview(_song!, _sectionOrders!);
            
            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.GreaterThan(0));
        }

        [Test]
        public void FormatPreview_WithLastSection_ShouldAddLastSectionSymbol()
        {
            // Arrange
            _settingsService!.Settings.LastSectionSymbol = "§";
            _settingsService.Settings.LastSectionSymbolLocation = SectionSymbolLocation.Both;

            // Act
            var result = _previewFormat!.FormatPreview(_song!, _sectionOrders!);

            // Convert InlineCollection to string for easier assertion
            var resultString = string.Join("", result.Select(inline =>
                inline is Run run ? run.Text :
                inline is LineBreak ? Environment.NewLine :
                string.Empty
            ));

            // Assert
            // Should contain the symbol at both start and end of the last section (Refrain/Chorus)
            Assert.That(resultString, Does.Contain($"Chorus{Environment.NewLine}§"));
            Assert.That(resultString, Does.EndWith("§"));
        }

        [Test]
        public void ValidateFileExtension_WithInvalidExtension_ShouldThrowArgumentException()
        {
            // Arrange
            var invalidPath = "test.txt";
            
            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => 
                _previewFormat!.TestValidateFileExtension(invalidPath));
            
            Assert.That(exception!.Message, Does.Contain("Invalid file extension"));
        }

        [Test]
        public void ValidateFileExtension_WithValidExtension_ShouldNotThrow()
        {
            // Arrange
            var validPath = "test.testformat";
            
            // Act & Assert
            Assert.DoesNotThrow(() => _previewFormat!.TestValidateFileExtension(validPath));
        }

        [Test]
        public void ExportToFileAsync_WhenNotSupported_ShouldThrowNotSupportedException()
        {
            // Arrange
            var nonExportingFormat = new NonExportingTestFormat(_settingsService);
            
            // Act & Assert
            var exception = Assert.ThrowsAsync<NotSupportedException>(async () => 
                await nonExportingFormat.ExportToFileAsync(_song!, _sectionOrders!, "test.txt"));
            
            Assert.That(exception!.Message, Does.Contain("does not support file export"));
        }

        // Test implementation of PreviewFormatBase for testing protected methods
        private class TestPreviewFormat : PreviewFormatBase
        {
            public TestPreviewFormat(SettingsService settingsService) : base(settingsService)
            {
            }

            public override string Name => "TestFormat";
            public override string Description => "Test Format Description";
            public override bool SupportsExport => true;
            public override string[] SupportedFileExtensions => new[] { ".testformat" };

            public override Task ExportToFileAsync(Song song, IEnumerable<OrderItem> sectionOrders, string filePath)
            {
                ValidateFileExtension(filePath);
                return Task.CompletedTask;
            }

            // Expose protected methods for testing
            public string TestFormatSectionName(string sectionName)
            {
                return FormatSectionName(sectionName);
            }

            public string TestBuildSongHeader(Song song)
            {
                return BuildSongHeader(song);
            }

            public void TestValidateFileExtension(string filePath)
            {
                ValidateFileExtension(filePath);
            }
        }

        // Test implementation that doesn't support export
        private class NonExportingTestFormat : PreviewFormatBase
        {
            public NonExportingTestFormat(SettingsService settingsService) : base(settingsService)
            {
            }

            public override string Name => "NonExportingFormat";
            public override string Description => "Format that doesn't support export";
            public override bool SupportsExport => false;
        }
    }
}
