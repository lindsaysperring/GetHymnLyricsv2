using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls.Documents;
using static GetHymnLyricsv2.ViewModels.MainWindowViewModel;

namespace GetHymnLyricsv2.Models.PreviewFormats.Interfaces;

public interface IPreviewFormat
{
    string Name { get; }  // Display name for the format
    string Description { get; }  // Description of what the format does
    InlineCollection FormatPreview(Song song, IEnumerable<OrderItem> sectionOrders);  // For preview display

    // Export capabilities
    bool SupportsExport { get; }  // Whether this format supports file export
    bool SupportsCopy { get; }    // Whether this format supports clipboard copy
    string[] SupportedFileExtensions { get; }  // File extensions this format supports (e.g. [".pro", ".pro6"])

    // Export methods
    string FormatForCopy(Song song, IEnumerable<OrderItem> sectionOrders);  // Format for clipboard
    Task ExportToFileAsync(Song song, IEnumerable<OrderItem> sectionOrders, string filePath);  // Handles file creation

    string GetSuggestedFileName(Song song);  // Suggests a file name for the format
}