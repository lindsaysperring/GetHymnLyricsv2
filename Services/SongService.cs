using System.Collections.Generic;
using System.Linq;
using GetHymnLyricsv2.Models;

namespace GetHymnLyricsv2.Services
{
    public class SongService : ISongService
    {
        public IEnumerable<Song> GetSongs(DataPacket dataPacket)
        {
            return dataPacket.RowData.Row.Songs.Items.OrderBy(s => s.Number);
        }

        public IEnumerable<SongSection> GetSongSections(DataPacket dataPacket, int songId)
        {
            return dataPacket.RowData.Row.SongSections.Items
                .Where(s => s.SongId == songId);
        }

        public void AddSection(DataPacket dataPacket, Song song, string sectionName)
        {
            var nextSectionId = GetNextSectionId(dataPacket);
            var newSection = new SongSection
            {
                SongId = song.SongId,
                SectionId = nextSectionId,
                SectionName = sectionName,
                SectionText = string.Empty,
                SectionComments = string.Empty
            };

            dataPacket.RowData.Row.SongSections.Items.Add(newSection);

            // Add to section order
            var maxOrder = dataPacket.RowData.Row.SongSectionOrder.Items
                .Where(o => o.SongId == song.SongId)
                .DefaultIfEmpty()
                .Max(o => o?.Order ?? -1) + 1;

            dataPacket.RowData.Row.SongSectionOrder.Items.Add(new SectionOrder
            {
                SongId = song.SongId,
                SectionId = nextSectionId,
                Order = maxOrder
            });
        }

        public void RemoveSection(DataPacket dataPacket, SongSection section)
        {
            dataPacket.RowData.Row.SongSections.Items.Remove(section);

            var ordersToRemove = dataPacket.RowData.Row.SongSectionOrder.Items
                .Where(o => o.SongId == section.SongId && o.SectionId == section.SectionId)
                .ToList();

            foreach (var order in ordersToRemove)
            {
                dataPacket.RowData.Row.SongSectionOrder.Items.Remove(order);
            }

            ReorderSections(dataPacket, section.SongId);
        }

        public int GetNextSectionId(DataPacket dataPacket)
        {
            return dataPacket.RowData.Row.SongSections.Items.Any()
                ? dataPacket.RowData.Row.SongSections.Items.Max(s => s.SectionId) + 1
                : 1;
        }

        public IEnumerable<SectionOrder> GetSongOrder(DataPacket dataPacket, int songId)
        {
            return dataPacket.RowData.Row.SongSectionOrder.Items
                .Where(o => o.SongId == songId)
                .OrderBy(o => o.Order);
        }

        public void AddToOrder(DataPacket dataPacket, Song song, SongSection section)
        {
            var maxOrder = dataPacket.RowData.Row.SongSectionOrder.Items
                .Where(o => o.SongId == song.SongId)
                .DefaultIfEmpty()
                .Max(o => o?.Order ?? -1) + 1;

            var newOrder = new SectionOrder
            {
                SongId = song.SongId,
                SectionId = section.SectionId,
                Order = maxOrder
            };

            dataPacket.RowData.Row.SongSectionOrder.Items.Add(newOrder);
        }

        public void RemoveFromOrder(DataPacket dataPacket, SectionOrder order)
        {
            dataPacket.RowData.Row.SongSectionOrder.Items.Remove(order);
            ReorderSections(dataPacket, order.SongId);
        }

        public void MoveOrderUp(DataPacket dataPacket, SectionOrder order)
        {
            var previousOrder = dataPacket.RowData.Row.SongSectionOrder.Items
                .Where(o => o.SongId == order.SongId && o.Order < order.Order)
                .OrderByDescending(o => o.Order)
                .FirstOrDefault();

            if (previousOrder != null)
            {
                (order.Order, previousOrder.Order) = (previousOrder.Order, order.Order);
            }
        }

        public void MoveOrderDown(DataPacket dataPacket, SectionOrder order)
        {
            var nextOrder = dataPacket.RowData.Row.SongSectionOrder.Items
                .Where(o => o.SongId == order.SongId && o.Order > order.Order)
                .OrderBy(o => o.Order)
                .FirstOrDefault();

            if (nextOrder != null)
            {
                (order.Order, nextOrder.Order) = (nextOrder.Order, order.Order);
            }
        }

        private static void ReorderSections(DataPacket dataPacket, int songId)
        {
            var remainingOrders = dataPacket.RowData.Row.SongSectionOrder.Items
                .Where(o => o.SongId == songId)
                .OrderBy(o => o.Order)
                .ToList();

            for (int i = 0; i < remainingOrders.Count; i++)
            {
                remainingOrders[i].Order = i;
            }
        }
    }
}
