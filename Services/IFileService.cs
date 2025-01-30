using System.Threading.Tasks;
using GetHymnLyricsv2.Models;

namespace GetHymnLyricsv2.Services
{
    public interface IFileService
    {
        Task<DataPacket?> LoadFileAsync(string filePath);
        Task SaveFileAsync(string filePath, DataPacket dataPacket);
    }
}
