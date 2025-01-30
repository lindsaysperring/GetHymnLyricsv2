using System;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using GetHymnLyricsv2.Models;

namespace GetHymnLyricsv2.Services
{
    public class FileService : IFileService
    {
        public async Task<DataPacket?> LoadFileAsync(string filePath)
        {
            try
            {
                var settings = new XmlReaderSettings
                {
                    IgnoreWhitespace = false
                };

                var serializer = new XmlSerializer(typeof(DataPacket));
                await using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                using var reader = XmlReader.Create(fileStream, settings);
                return (DataPacket)serializer.Deserialize(reader)!;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task SaveFileAsync(string filePath, DataPacket dataPacket)
        {
            try
            {
                var settings = new XmlWriterSettings
                {
                    Indent = true,
                    IndentChars = "  ",
                    NewLineChars = Environment.NewLine,
                    NewLineHandling = NewLineHandling.Replace,
                    Async = true
                };

                var serializer = new XmlSerializer(typeof(DataPacket));
                await using var writer = XmlWriter.Create(filePath, settings);
                serializer.Serialize(writer, dataPacket);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
