using System;
using Avalonia;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Styling;

namespace PRoCon.Themes
{
    public enum AppTheme
    {
        Dark,
        Light
    }

    public interface IThemeManager
    {
        AppTheme CurrentTheme { get; }
        event EventHandler<AppTheme> ThemeChanged;
        void SetTheme(AppTheme theme);
        void ToggleTheme();
    }

    public class ThemeManager : IThemeManager
    {
        private readonly Application _app;
        private readonly ResourceInclude _darkTheme;
        private readonly ResourceInclude _lightTheme;

        public AppTheme CurrentTheme { get; private set; }
        public event EventHandler<AppTheme> ThemeChanged;

        public ThemeManager(Application app)
        {
            _app = app ?? throw new ArgumentNullException(nameof(app));

            _darkTheme = new ResourceInclude(new Uri("avares://PRoCon.Themes"))
            {
                Source = new Uri("avares://PRoCon.Themes/Themes/DarkTheme.axaml")
            };

            _lightTheme = new ResourceInclude(new Uri("avares://PRoCon.Themes"))
            {
                Source = new Uri("avares://PRoCon.Themes/Themes/LightTheme.axaml")
            };

            // Default to dark theme
            CurrentTheme = AppTheme.Dark;
        }

        public void SetTheme(AppTheme theme)
        {
            if (CurrentTheme == theme) return;

            CurrentTheme = theme;

            // Remove existing theme resources
            _app.Resources.MergedDictionaries.Remove(_darkTheme);
            _app.Resources.MergedDictionaries.Remove(_lightTheme);

            // Add the selected theme
            var selectedTheme = theme == AppTheme.Dark ? _darkTheme : _lightTheme;
            _app.Resources.MergedDictionaries.Add(selectedTheme);

            ThemeChanged?.Invoke(this, theme);
        }

        public void ToggleTheme()
        {
            SetTheme(CurrentTheme == AppTheme.Dark ? AppTheme.Light : AppTheme.Dark);
        }

        public void Initialize()
        {
            // Apply the default dark theme
            _app.Resources.MergedDictionaries.Add(_darkTheme);
        }
    }
}
