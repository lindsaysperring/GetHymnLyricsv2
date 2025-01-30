using CommunityToolkit.Mvvm.ComponentModel;
using GetHymnLyricsv2.Models;

namespace GetHymnLyricsv2.ViewModels
{
    public partial class SongDetailsViewModel : ViewModelBase
    {
        [ObservableProperty]
        private Song? song;

        public void UpdateSong(Song? newSong)
        {
            Song = newSong;
        }
    }
}
