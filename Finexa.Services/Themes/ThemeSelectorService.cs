using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Finexa.Core.Services;
using Finexa.Infrastructure.Database;
using Finexa.Domain.Entities;

namespace Finexa.Services.Themes
{
    public class ThemeSelectorService : IThemeSelectorService
    {
        private readonly FinexaDbContext _context;
        private string _currentTheme = "Dark";

        public string CurrentTheme => _currentTheme;

        public event EventHandler<string>? ThemeChanged;

        public ThemeSelectorService(FinexaDbContext context)
        {
            _context = context;
        }

        public async Task InitializeAsync()
        {
            var themeSetting = await _context.Settings
                .FirstOrDefaultAsync(s => s.Key == "AppTheme");

            if (themeSetting != null)
            {
                _currentTheme = themeSetting.Value;
            }
            else
            {
                _currentTheme = "Dark";
            }
        }

        public async Task SetThemeAsync(string themeName)
        {
            _currentTheme = themeName;

            var themeSetting = await _context.Settings
                .FirstOrDefaultAsync(s => s.Key == "AppTheme");

            if (themeSetting == null)
            {
                themeSetting = new Setting { Key = "AppTheme", Value = themeName, Group = "Theme" };
                await _context.Settings.AddAsync(themeSetting);
            }
            else
            {
                themeSetting.Value = themeName;
                _context.Settings.Update(themeSetting);
            }

            await _context.SaveChangesAsync();

            ThemeChanged?.Invoke(this, themeName);
        }
    }
}
