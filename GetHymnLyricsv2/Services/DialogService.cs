using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Platform.Storage;

namespace GetHymnLyricsv2.Services
{
    public enum DialogType
    {
        Error,
        Warning,
        Info
    }

    public class DialogService : IDialogService
    {
        private Window GetMainWindow()
        {
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                return desktop.MainWindow ?? throw new InvalidOperationException("Main window is not set.");
            }
            throw new InvalidOperationException("Application lifetime is not IClassicDesktopStyleApplicationLifetime.");
        }

        private async Task<object> ShowDialogAsync(string title, string message, DialogType type)
        {
            var parent = GetMainWindow();
            var icon = type switch
            {
                DialogType.Error => "❌",
                DialogType.Warning => "⚠️",
                _ => "ℹ️"
            };

            var iconBitmap = new Bitmap(AssetLoader.Open(new Uri($"avares://{nameof(GetHymnLyricsv2)}/Assets/gethymnlyrics-logo.ico")));

            var dialog = new Window
            {
                Title = title,
                Width = 300,
                Height = 150,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Icon = new WindowIcon(iconBitmap),
                Content = new StackPanel
                {
                    Margin = new Thickness(20),
                    Children =
                    {
                        new TextBlock
                        {
                            Text = $"{icon} {message}",
                            TextWrapping = TextWrapping.Wrap,
                            Margin = new Thickness(0, 0, 0, 20)
                        },
                        new Button
                        {
                            Content = "OK",
                            HorizontalAlignment = HorizontalAlignment.Center
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
            return await tcs.Task;
        }

        public Task ShowErrorAsync(string title, string message)
            => ShowDialogAsync(title, message, DialogType.Error);

        public Task ShowWarningAsync(string title, string message)
            => ShowDialogAsync(title, message, DialogType.Warning);

        public Task ShowInfoAsync(string title, string message)
            => ShowDialogAsync(title, message, DialogType.Info);

        public async Task<bool> ShowConfirmationAsync(string title, string message)
        {
            var parent = GetMainWindow();
            var iconBitmap = new Bitmap(AssetLoader.Open(new Uri($"avares://{nameof(GetHymnLyricsv2)}/Assets/gethymnlyrics-logo.ico")));
            var tcs = new TaskCompletionSource<bool>();

            var dialog = new Window
            {
                Title = title,
                Width = 300,
                Height = 150,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Icon = new WindowIcon(iconBitmap),
                Content = new ScrollViewer
                {
                    Content = new StackPanel
                    {
                        Margin = new Thickness(20),
                        Children =
                        {
                            new TextBlock
                            {
                                Text = message,
                                TextWrapping = TextWrapping.Wrap,
                                Margin = new Thickness(0, 0, 0, 20)
                            },
                            new StackPanel
                            {
                                Orientation = Orientation.Horizontal,
                                HorizontalAlignment = HorizontalAlignment.Center,
                                Spacing = 10,
                                Children =
                                {
                                    new Button
                                    {
                                        Content = "Yes",
                                        Width = 80
                                    },
                                    new Button
                                    {
                                        Content = "No",
                                        Width = 80
                                    }
                                }
                            }
                        }
                    }
                }
            };

            var scrollViewer = (ScrollViewer)dialog.Content;
            if (scrollViewer.Content is not StackPanel stackPanel)
            {
                throw new InvalidOperationException("ScrollViewer content is not a StackPanel.");
            }
            var buttonPanel = (StackPanel)stackPanel.Children[1];
            var yesButton = (Button)buttonPanel.Children[0];
            var noButton = (Button)buttonPanel.Children[1];

            yesButton.Click += (s, e) =>
            {
                dialog.Close();
                tcs.SetResult(true);
            };

            noButton.Click += (s, e) =>
            {
                dialog.Close();
                tcs.SetResult(false);
            };

            await dialog.ShowDialog(parent);
            return await tcs.Task;
        }

        public async Task<string?> OpenFileAsync(string title, params string[] extensions)
        {
            var parent = GetMainWindow();
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

        public async Task<string?> SaveFileAsync(string title, string defaultExtension, string? suggestedFileName = null, string? fileTypeName = null, params string[] extensions)
        {
            var parent = GetMainWindow();
            var options = new FilePickerSaveOptions
            {
                Title = title,
                DefaultExtension = defaultExtension,
                FileTypeChoices = new[]
                {
                    new FilePickerFileType(fileTypeName ?? title)
                    {
                        Patterns = extensions
                    }
                }
            };

            if (suggestedFileName != null)
            {
                options.SuggestedFileName = suggestedFileName;
            }

            var result = await parent.StorageProvider.SaveFilePickerAsync(options);
            return result?.Path.LocalPath;
        }
    }
}
