using System.Linq;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using GetHymnLyricsv2.ViewModels;
using GetHymnLyricsv2.Views;
using GetHymnLyricsv2.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System;
using GetHymnLyricsv2.Models.PreviewFormats.Interfaces;
using GetHymnLyricsv2.Models.PreviewFormats;

namespace GetHymnLyricsv2
{
    public partial class App : Application
    {
        private ServiceProvider? _serviceProvider;
        
        public static readonly IReadOnlyList<SectionType> SectionTypes = Array.AsReadOnly(Enum.GetValues<SectionType>());

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);

            var services = new ServiceCollection();

            // Register services
            services.AddSingleton<IFileService, FileService>();
            services.AddSingleton<ISongService, SongService>();
            services.AddSingleton<IDialogService, DialogService>();
            services.AddSingleton<ISettingsService, SettingsService>();
            services.AddSingleton<IUpdateService, UpdateService>();

            // Register preview formats
            services.AddSingleton<IPreviewFormat, PlainTextFormat>();
            services.AddSingleton<IPreviewFormat, ProPresenterFormat>();
            services.AddSingleton<IPreviewFormat, FreeShowFormat>();

            // Register view models
            services.AddTransient<SongDetailsViewModel>();
            services.AddTransient<SongSectionsViewModel>();
            services.AddTransient<MainWindowViewModel>();

            _serviceProvider = services.BuildServiceProvider();
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                // Avoid duplicate validations from both Avalonia and the CommunityToolkit. 
                // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
                DisableAvaloniaDataAnnotationValidation();

                var mainViewModel = _serviceProvider?.GetRequiredService<MainWindowViewModel>();
                desktop.MainWindow = new MainWindow
                {
                    DataContext = mainViewModel,
                };
            }

            base.OnFrameworkInitializationCompleted();
        }

        private void DisableAvaloniaDataAnnotationValidation()
        {
            // Get an array of plugins to remove
            var dataValidationPluginsToRemove =
                BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

            // remove each entry found
            foreach (var plugin in dataValidationPluginsToRemove)
            {
                BindingPlugins.DataValidators.Remove(plugin);
            }
        }
    }
}
