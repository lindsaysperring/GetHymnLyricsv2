using System;
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

        protected override async void OnOpened(EventArgs e)
        {
            base.OnOpened(e);
            if (ViewModel != null)
            {
                await ViewModel.InitializeAsync();
            }
        }

        private async void MainWindow_Closing(object? sender, WindowClosingEventArgs e)
        {
            if (ViewModel?.HasUnsavedChanges == true)
            {
                e.Cancel = true; // Prevent immediate closing
                
                if (await ViewModel.ConfirmCloseWithUnsavedChanges())
                {
                    ViewModel.HasUnsavedChanges = false; // Prevent re-triggering the dialog
                    Close(); // User confirmed they want to close
                }
            }
        }
    }
}
