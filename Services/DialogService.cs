using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform.Storage;

namespace GetHymnLyricsv2.Services
{
    public class DialogService : IDialogService
    {
        public async Task ShowErrorAsync(string title, string message, Window parent)
        {
            var dialog = new Window
            {
                Title = title,
                Width = 300,
                Height = 150,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Content = new StackPanel
                {
                    Margin = new Avalonia.Thickness(20),
                    Children =
                    {
                        new TextBlock
                        {
                            Text = message,
                            TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                            Margin = new Avalonia.Thickness(0, 0, 0, 20)
                        },
                        new Button
                        {
                            Content = "OK",
                            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
                        }
                    }
                }
            };

            var stackPanel = (StackPanel)dialog.Content;
            var button = (Button)stackPanel.Children[1];
            var tcs = new TaskCompletionSource<object>();
            button.Click += (s, e) =>
            {
                dialog.Close();
                tcs.SetResult(new object());
            };

            await dialog.ShowDialog(parent);
            await tcs.Task;
        }

        public async Task<string?> OpenFileAsync(Window parent, string title, params string[] extensions)
        {
            var options = new FilePickerOpenOptions
            {
                Title = title,
                AllowMultiple = false,
                FileTypeFilter = new[]
                {
                    new FilePickerFileType(title)
                    {
                        Patterns = extensions
                    }
                }
            };

            var result = await parent.StorageProvider.OpenFilePickerAsync(options);
            return result.Count > 0 ? result[0].Path.LocalPath : null;
        }

        public async Task<string?> SaveFileAsync(Window parent, string title, string defaultExtension, params string[] extensions)
        {
            var options = new FilePickerSaveOptions
            {
                Title = title,
                DefaultExtension = defaultExtension,
                FileTypeChoices = new[]
                {
                    new FilePickerFileType(title)
                    {
                        Patterns = extensions
                    }
                }
            };

            var result = await parent.StorageProvider.SaveFilePickerAsync(options);
            return result?.Path.LocalPath;
        }
    }
}
