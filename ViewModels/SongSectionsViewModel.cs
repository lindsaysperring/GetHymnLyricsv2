using System.Collections.ObjectModel;
using System.Linq;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GetHymnLyricsv2.Models;
using GetHymnLyricsv2.Services;
using System;
namespace GetHymnLyricsv2.ViewModels
{
    public enum SectionType
    {
        Verse,
        Refrain
    }
    
    public class SectionViewModel : INotifyPropertyChanged
    {
        private SongSection _section;
        internal SectionType _sectionType;
        private readonly SongSectionsViewModel _parent;

        public event PropertyChangedEventHandler? PropertyChanged;

        public SectionViewModel(SongSection section, SongSectionsViewModel parent)
        {
            _section = section;
            _parent = parent;
            _sectionType = section.SectionName.StartsWith("Verse", StringComparison.OrdinalIgnoreCase) 
                ? SectionType.Verse 
                : SectionType.Refrain;
        }

        public string SectionName
        {
            get => _section.SectionName;
            set
            {
                if (_section.SectionName != value)
                {
                    _section.SectionName = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SectionName)));
                }
            }
        }

        public string SectionText
        {
            get => _section.SectionText;
            set
            {
                if (_section.SectionText != value)
                {
                    _section.SectionText = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SectionText)));
                }
            }
        }

        public string SectionComments
        {
            get => _section.SectionComments;
            set
            {
                if (_section.SectionComments != value)
                {
                    _section.SectionComments = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SectionComments)));
                }
            }
        }

        public SectionType SectionType
        {
            get => _sectionType;
            set
            {
                if (_sectionType != value)
                {
                    _sectionType = value;
                    _parent.UpdateSectionName(this);
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SectionType)));
                }
            }
        }

        public SongSection Section => _section;
    }

    public partial class SongSectionsViewModel : ViewModelBase
    {
        private readonly ISongService _songService;
        private DataPacket? _dataPacket;
        private Song? _currentSong;

        [ObservableProperty]
        private ObservableCollection<SectionViewModel> sections = new();

        [ObservableProperty]
        private SectionViewModel? selectedSection;

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
                Sections.Add(new SectionViewModel(section, this));
            }
            UpdateAllSectionNames();
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
            _songService.AddSection(_dataPacket, _currentSong, "Verse 1");
            UpdateSections();
        }

        public void UpdateSectionName(SectionViewModel section)
        {
            if (section.SectionType == SectionType.Verse)
            {
                var currentVerseNumber = 1;
                foreach (var s in Sections.Where(s => s.SectionType == SectionType.Verse).OrderBy(s => s.Section.SectionId))
                {
                    s.SectionName = $"Verse {currentVerseNumber}";
                    currentVerseNumber++;
                }
            }
            else // Refrain
            {
                section.SectionName = "Refrain";
            }
        }

        private void UpdateAllSectionNames()
        {
            var currentVerseNumber = 1;
            foreach (var section in Sections.Where(s => s.SectionType == SectionType.Verse).OrderBy(s => s.Section.SectionId))
            {
                section.SectionName = $"Verse {currentVerseNumber}";
                currentVerseNumber++;
            }

            foreach (var section in Sections.Where(s => s.SectionType == SectionType.Refrain))
            {
                section.SectionName = "Refrain";
            }
        }

        [RelayCommand]
        private void RemoveSection(SectionViewModel section)
        {
            if (_dataPacket == null) return;
            _songService.RemoveSection(_dataPacket, section.Section);
            UpdateSections();
            UpdateOrder();
        }

        [RelayCommand]
        private void AddToOrder()
        {
            if (_dataPacket == null || _currentSong == null || SelectedSection == null) return;
            _songService.AddToOrder(_dataPacket, _currentSong, SelectedSection.Section);
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
