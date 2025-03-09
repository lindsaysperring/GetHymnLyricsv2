using GetHymnLyricsv2.Utilities;

namespace GetHymnLyricsv2.Test.Utilities
{
    [TestFixture]
    public class UidGeneratorTests
    {
        [Test]
        public void Generate_WithDefaultLength_ShouldReturnStringOfLength11()
        {
            // Act
            var result = UidGenerator.Generate();
            
            // Assert
            Assert.That(result, Has.Length.EqualTo(11));
        }
        
        [Test]
        public void Generate_WithSpecifiedLength_ShouldReturnStringOfSpecifiedLength()
        {
            // Arrange
            int length = 5;
            
            // Act
            var result = UidGenerator.Generate(length);
            
            // Assert
            Assert.That(result, Has.Length.EqualTo(length));
        }
        
        [Test]
        public void Generate_CalledMultipleTimes_ShouldReturnDifferentValues()
        {
            // Arrange
            int count = 10;
            var generatedIds = new HashSet<string>();
            
            // Act
            for (int i = 0; i < count; i++)
            {
                generatedIds.Add(UidGenerator.Generate());
            }
            
            // Assert
            Assert.That(generatedIds.Count, Is.EqualTo(count), "Generated IDs should be unique");
        }
        
        [Test]
        public void Generate_WithZeroLength_ShouldReturnEmptyString()
        {
            // Act
            var result = UidGenerator.Generate(0);
            
            // Assert
            Assert.That(result, Is.Empty);
        }
        
        [Test]
        public void Generate_WithLargeLength_ShouldHandleItGracefully()
        {
            // Arrange
            int length = 100;
            
            // Act & Assert
            Assert.DoesNotThrow(() => UidGenerator.Generate(length));
            var result = UidGenerator.Generate(length);
            Assert.That(result, Has.Length.EqualTo(length));
        }
        
        [Test]
        public void Generate_OutputFormat_ShouldBeHexadecimal()
        {
            // Act
            var result = UidGenerator.Generate();
            
            // Assert
            Assert.That(result, Does.Match("[0-9a-f]+"), "Generated ID should contain only hexadecimal characters");
        }
    }
}
