using System.Threading.Tasks;
using Avalonia.Controls;

namespace GetHymnLyricsv2.Services
{
    public interface IDialogService
    {
        Task ShowErrorAsync(string title, string message, Window parent);
        Task<string?> OpenFileAsync(Window parent, string title, params string[] extensions);
        Task<string?> SaveFileAsync(Window parent, string title, string defaultExtension, params string[] extensions);
    }
}
