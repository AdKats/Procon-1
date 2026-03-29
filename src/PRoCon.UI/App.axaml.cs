using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using PRoCon.Themes;
using PRoCon.UI.Views;

namespace PRoCon.UI
{
    public partial class App : Application
    {
        public static ThemeManager ThemeManager { get; private set; }

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
            ThemeManager = new ThemeManager(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow();
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}
