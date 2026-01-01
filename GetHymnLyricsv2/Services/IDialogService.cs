using System.Threading.Tasks;
using Avalonia.Controls;

namespace GetHymnLyricsv2.Services
{
    public interface IDialogService
    {
        Task ShowErrorAsync(string title, string message);
        Task ShowWarningAsync(string title, string message);
        Task ShowInfoAsync(string title, string message);
        Task<string?> OpenFileAsync(string title, params string[] extensions);
        Task<string?> SaveFileAsync(string title, string defaultExtension, string? suggestedFileName = null, string? fileTypeName = null, params string[] extensions);
        Task<bool> ShowConfirmationAsync(string title, string message);
    }
}
