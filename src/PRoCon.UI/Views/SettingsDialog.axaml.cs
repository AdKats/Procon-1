using Avalonia.Controls;
using Avalonia.Interactivity;

namespace PRoCon.UI.Views
{
    public partial class SettingsDialog : Window
    {
        public SettingsDialog()
        {
            InitializeComponent();
        }

        public void SetContent(OptionsPanel panel)
        {
            var content = this.FindControl<ContentControl>("SettingsContent");
            if (content != null) content.Content = panel;
        }

        private void OnClose(object sender, RoutedEventArgs e)
        {
            Close();
        }

        protected override void OnClosed(System.EventArgs e)
        {
            // Detach the panel so it can be reused
            var content = this.FindControl<ContentControl>("SettingsContent");
            if (content != null) content.Content = null;
            base.OnClosed(e);
        }
    }
}
