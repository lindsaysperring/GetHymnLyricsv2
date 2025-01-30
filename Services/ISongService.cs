using System.Collections.Generic;
using System.Threading.Tasks;
using GetHymnLyricsv2.Models;

namespace GetHymnLyricsv2.Services
{
    public interface ISongService
    {
        IEnumerable<Song> GetSongs(DataPacket dataPacket);
        IEnumerable<SongSection> GetSongSections(DataPacket dataPacket, int songId);
        void AddSection(DataPacket dataPacket, Song song, string sectionName);
        void RemoveSection(DataPacket dataPacket, SongSection section);
        int GetNextSectionId(DataPacket dataPacket);
        IEnumerable<SectionOrder> GetSongOrder(DataPacket dataPacket, int songId);
        void AddToOrder(DataPacket dataPacket, Song song, SongSection section);
        void RemoveFromOrder(DataPacket dataPacket, SectionOrder order);
        void MoveOrderUp(DataPacket dataPacket, SectionOrder order);
        void MoveOrderDown(DataPacket dataPacket, SectionOrder order);
    }
}
