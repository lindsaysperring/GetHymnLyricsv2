using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GetHymnLyricsv2.Models;
using GetHymnLyricsv2.Services;

namespace GetHymnLyricsv2.ViewModels
{
    public partial class SongSectionsViewModel : ViewModelBase
    {
        private readonly ISongService _songService;
        private DataPacket? _dataPacket;
        private Song? _currentSong;

        [ObservableProperty]
        private ObservableCollection<SongSection> sections = new();

        [ObservableProperty]
        private SongSection? selectedSection;

        [ObservableProperty]
        private ObservableCollection<MainWindowViewModel.OrderItem> songOrder = new();

        public SongSectionsViewModel(ISongService songService)
        {
            _songService = songService;
        }

        public void Initialize(DataPacket dataPacket, Song song)
        {
            _dataPacket = dataPacket;
            _currentSong = song;
            UpdateSections();
            UpdateOrder();
        }

        private void UpdateSections()
        {
            if (_dataPacket == null || _currentSong == null) return;

            Sections.Clear();
            foreach (var section in _songService.GetSongSections(_dataPacket, _currentSong.SongId))
            {
                Sections.Add(section);
            }
        }

        private void UpdateOrder()
        {
            if (_dataPacket == null || _currentSong == null) return;

            var orders = _songService.GetSongOrder(_dataPacket, _currentSong.SongId);
            SongOrder.Clear();

            foreach (var order in orders)
            {
                var section = _dataPacket.RowData.Row.SongSections.Items
                    .FirstOrDefault(s => s.SongId == order.SongId && s.SectionId == order.SectionId);
                
                if (section != null)
                {
                    SongOrder.Add(new MainWindowViewModel.OrderItem
                    {
                        Section = section,
                        OrderEntry = order
                    });
                }
            }
        }

        [RelayCommand]
        private void AddSection()
        {
            if (_dataPacket == null || _currentSong == null) return;
            _songService.AddSection(_dataPacket, _currentSong, "New Section");
            UpdateSections();
        }

        [RelayCommand]
        private void RemoveSection(SongSection section)
        {
            if (_dataPacket == null) return;
            _songService.RemoveSection(_dataPacket, section);
            UpdateSections();
            UpdateOrder();
        }

        [RelayCommand]
        private void AddToOrder()
        {
            if (_dataPacket == null || _currentSong == null || SelectedSection == null) return;
            _songService.AddToOrder(_dataPacket, _currentSong, SelectedSection);
            UpdateOrder();
        }

        [RelayCommand]
        private void RemoveFromOrder(MainWindowViewModel.OrderItem orderItem)
        {
            if (_dataPacket == null) return;
            _songService.RemoveFromOrder(_dataPacket, orderItem.OrderEntry);
            UpdateOrder();
        }

        [RelayCommand]
        private void MoveOrderUp(MainWindowViewModel.OrderItem orderItem)
        {
            if (_dataPacket == null) return;
            _songService.MoveOrderUp(_dataPacket, orderItem.OrderEntry);
            UpdateOrder();
        }

        [RelayCommand]
        private void MoveOrderDown(MainWindowViewModel.OrderItem orderItem)
        {
            if (_dataPacket == null) return;
            _songService.MoveOrderDown(_dataPacket, orderItem.OrderEntry);
            UpdateOrder();
        }

        public void Clear()
        {
            Sections.Clear();
            SongOrder.Clear();
            SelectedSection = null;
            _dataPacket = null;
            _currentSong = null;
        }
    }
}
