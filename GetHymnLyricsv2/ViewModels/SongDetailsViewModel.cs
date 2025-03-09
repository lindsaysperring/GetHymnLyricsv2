using System;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using GetHymnLyricsv2.Models;

namespace GetHymnLyricsv2.ViewModels
{
    public partial class SongDetailsViewModel : ViewModelBase
    {
        public event EventHandler? ContentChanged;

        [ObservableProperty]
        private Song? song;

        public void UpdateSong(Song? newSong)
        {
            if (Song != null)
            {
                Song.PropertyChanged -= Song_PropertyChanged;
            }

            Song = newSong;

            if (Song != null)
            {
                Song.PropertyChanged += Song_PropertyChanged;
            }
        }

        private void Song_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            ContentChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
