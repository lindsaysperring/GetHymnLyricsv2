using Avalonia.Controls;
using GetHymnLyricsv2.ViewModels;

namespace GetHymnLyricsv2.Views
{
    public partial class MainWindow : Window
    {
        private MainWindowViewModel? ViewModel => DataContext as MainWindowViewModel;

        public MainWindow()
        {
            InitializeComponent();
            Closing += MainWindow_Closing;
        }

        private async void MainWindow_Closing(object? sender, WindowClosingEventArgs e)
        {
            if (ViewModel?.HasUnsavedChanges == true)
            {
                e.Cancel = true; // Prevent immediate closing
                
                if (await ViewModel.ConfirmCloseWithUnsavedChanges(this))
                {
                    ViewModel.HasUnsavedChanges = false; // Prevent re-triggering the dialog
                    Close(); // User confirmed they want to close
                }
            }
        }
    }
}
