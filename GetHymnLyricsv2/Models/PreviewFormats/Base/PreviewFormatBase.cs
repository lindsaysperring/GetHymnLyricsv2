using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls.Documents;
using GetHymnLyricsv2.Models.PreviewFormats.Interfaces;
using GetHymnLyricsv2.Services;
using static GetHymnLyricsv2.ViewModels.MainWindowViewModel;

namespace GetHymnLyricsv2.Models.PreviewFormats.Base
{
    public abstract class PreviewFormatBase : IPreviewFormat
    {
        // Required properties that must be implemented by derived classes
        public abstract string Name { get; }
        public abstract string Description { get; }
        
        // Default implementation for capabilities
        public virtual bool SupportsExport => false;
        public virtual bool SupportsCopy => true;
        public virtual string[] SupportedFileExtensions => Array.Empty<string>();

        protected readonly ISettingsService _settingsService;

        protected PreviewFormatBase(ISettingsService settingsService)
        {
            _settingsService = settingsService;
        }

        // Helper method to format section name consistently
        protected virtual string FormatSectionName(string sectionName)
        {
            return sectionName.Equals("Refrain", StringComparison.OrdinalIgnoreCase)
                ? "Chorus"
                : sectionName;
        }

        // Helper method to build song header
        protected virtual string BuildSongHeader(Song song)
        {
            return $"{song.Number} - {song.Title}";
        }

        // Default preview implementation that most formats can use
        public virtual InlineCollection FormatPreview(Song song, IEnumerable<OrderItem> sectionOrders)
        {
            var inlineCollection = new InlineCollection();

            if (song == null) return inlineCollection;

            inlineCollection.Add(new Run($"{song.Number} - {song.Title}") { FontWeight = Avalonia.Media.FontWeight.Bold });
            inlineCollection.Add(new LineBreak());

            foreach (var order in sectionOrders)
            {
                var section = order.Section;
                var isLastSection = sectionOrders.Last() == order;

                if (inlineCollection.Count > 0)
                {
                    inlineCollection.Add(new LineBreak());
                }

                string sectionName = section.SectionName.Equals("Refrain", StringComparison.OrdinalIgnoreCase)
                    ? "Chorus"
                    : section.SectionName;
                inlineCollection.Add(new Run(sectionName) { FontWeight = Avalonia.Media.FontWeight.Bold });
                inlineCollection.Add(new LineBreak());

                if (isLastSection && (_settingsService.Settings.LastSectionSymbolLocation == SectionSymbolLocation.Start || _settingsService.Settings.LastSectionSymbolLocation == SectionSymbolLocation.Both))
                {
                    inlineCollection.Add(new Run(_settingsService.Settings.LastSectionSymbol) { FontStyle = Avalonia.Media.FontStyle.Italic });
                    inlineCollection.Add(new LineBreak());
                }

                inlineCollection.Add(new Run(section.SectionText));

                if (isLastSection && (_settingsService.Settings.LastSectionSymbolLocation == SectionSymbolLocation.End || _settingsService.Settings.LastSectionSymbolLocation == SectionSymbolLocation.Both))
                {
                    inlineCollection.Add(new Run(_settingsService.Settings.LastSectionSymbol) { FontStyle = Avalonia.Media.FontStyle.Italic });
                }
            }

            return inlineCollection;
        }

        // Default copy implementation
        public virtual string FormatForCopy(Song song, IEnumerable<OrderItem> sectionOrders)
        {
            var stringBuilder = new System.Text.StringBuilder();

            var collection = FormatPreview(song, sectionOrders);

            foreach (var inline in collection)
            {
                if (inline is Run run)
                {
                    stringBuilder.Append(run.Text);
                }
                else if (inline is LineBreak)
                {
                    stringBuilder.AppendLine();
                }
            }

            return stringBuilder.ToString().Trim();
        }

        // Default export implementation that throws if not overridden
        public virtual Task ExportToFileAsync(Song song, IEnumerable<OrderItem> sectionOrders, string filePath)
        {
            throw new NotSupportedException($"{Name} format does not support file export.");
        }

        // Helper method to ensure file extension matches supported types
        protected virtual void ValidateFileExtension(string filePath)
        {
            if (!SupportsExport)
            {
                throw new NotSupportedException($"{Name} format does not support file export.");
            }

            var extension = System.IO.Path.GetExtension(filePath);
            if (!Array.Exists(SupportedFileExtensions, ext => ext.Equals(extension, StringComparison.OrdinalIgnoreCase)))
            {
                throw new ArgumentException(
                    $"Invalid file extension. Supported extensions are: {string.Join(", ", SupportedFileExtensions)}");
            }
        }

        // Default file name suggestion
        public virtual string GetSuggestedFileName(Song song)
        {
            if (SupportsExport && SupportedFileExtensions.Length > 0)
            {
                return $"{song.Number} - {song.Title}{SupportedFileExtensions[0]}";
            }
            return $"{song.Number} - {song.Title}";
        }
    }
}