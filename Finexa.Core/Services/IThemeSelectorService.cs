using System.Threading.Tasks;

namespace Finexa.Core.Services
{
    public interface IThemeSelectorService
    {
        string CurrentTheme { get; }
        Task InitializeAsync();
        Task SetThemeAsync(string themeName);
    }
}
