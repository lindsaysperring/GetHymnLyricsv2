using System;
using Avalonia.Controls;
using GetHymnLyricsv2.ViewModels;

namespace GetHymnLyricsv2.Views
{
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
        }

        protected override void OnDataContextChanged(EventArgs e)
        {
            base.OnDataContextChanged(e);
            if (DataContext is SettingsViewModel vm)
            {
                vm.RequestClose += (_, _) => Close();
            }
        }
    }
}
