using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using PRoCon.Core;
using PRoCon.Core.Remote;
using System;
using System.Runtime.InteropServices;

namespace PRoCon.UI.Views
{
    public partial class OptionsPanel : UserControl
    {
        private PRoConApplication _application;

        public OptionsPanel()
        {
            InitializeComponent();
        }

        public void SetClient(PRoConClient client)
        {
            // Options panel works at the application level, not per-client.
        }

        public void SetApplication(PRoConApplication app)
        {
            _application = app;

            if (_application == null)
            {
                IsEnabled = false;
                return;
            }

            IsEnabled = true;
            LoadCurrentState();
        }

        private void LoadCurrentState()
        {
            if (_application == null) return;

            var options = _application.OptionsSettings;
            if (options != null)
            {
                SetCheck("AutoCheckUpdatesCheck", options.AutoCheckDownloadUpdates);
                SetCheck("AutoApplyUpdatesCheck", options.AutoApplyUpdates);
                SetCheck("ShowTrayIconCheck", options.ShowTrayIcon);
                SetCheck("CloseToTrayCheck", options.CloseToTray);
                SetCheck("MinimizeToTrayCheck", options.MinimizeToTray);
            }

            // ProxyCheck API key
            var apiKeyInput = this.FindControl<TextBox>("ProxyCheckApiKeyInput");
            if (apiKeyInput != null && options != null)
                apiKeyInput.Text = options.ProxyCheckApiKey ?? "";

            // Language
            var currentLang = _application.CurrentLanguage;
            var langText = this.FindControl<TextBlock>("CurrentLanguageText");
            if (langText != null && currentLang != null)
                langText.Text = $"Current: {currentLang.FileName}";

            // Runtime info
            var runtimeInfo = this.FindControl<TextBlock>("RuntimeInfoText");
            if (runtimeInfo != null)
            {
                runtimeInfo.Text = $"OS: {RuntimeInformation.OSDescription} | " +
                                   $"Framework: {RuntimeInformation.FrameworkDescription}";
            }
        }

        // --- UI Actions ---

        private void OnAutoCheckUpdatesToggle(object sender, RoutedEventArgs e)
        {
            if (_application?.OptionsSettings == null) return;
            _application.OptionsSettings.AutoCheckDownloadUpdates = (sender as CheckBox)?.IsChecked == true;
        }

        private void OnAutoApplyUpdatesToggle(object sender, RoutedEventArgs e)
        {
            if (_application?.OptionsSettings == null) return;
            _application.OptionsSettings.AutoApplyUpdates = (sender as CheckBox)?.IsChecked == true;
        }

        private void OnShowTrayIconToggle(object sender, RoutedEventArgs e)
        {
            if (_application?.OptionsSettings == null) return;
            _application.OptionsSettings.ShowTrayIcon = (sender as CheckBox)?.IsChecked == true;
        }

        private void OnCloseToTrayToggle(object sender, RoutedEventArgs e)
        {
            if (_application?.OptionsSettings == null) return;
            _application.OptionsSettings.CloseToTray = (sender as CheckBox)?.IsChecked == true;
        }

        private void OnMinimizeToTrayToggle(object sender, RoutedEventArgs e)
        {
            if (_application?.OptionsSettings == null) return;
            _application.OptionsSettings.MinimizeToTray = (sender as CheckBox)?.IsChecked == true;
        }

        private void OnLanguageChanged(object sender, SelectionChangedEventArgs e)
        {
            var combo = this.FindControl<ComboBox>("LanguageComboBox");
            if (combo?.SelectedItem is string langName)
            {
                SetStatus($"Language selection changed to: {langName}");
            }
        }

        // --- Helpers ---

        private void SetCheck(string controlName, bool value)
        {
            var cb = this.FindControl<CheckBox>(controlName);
            if (cb != null) cb.IsChecked = value;
        }

        private void SetStatus(string message)
        {
            var status = this.FindControl<TextBlock>("OptionsStatusText");
            if (status != null) status.Text = message;
        }

        private void OnSaveProxyCheckKey(object sender, RoutedEventArgs e)
        {
            if (_application?.OptionsSettings == null) return;

            var input = this.FindControl<TextBox>("ProxyCheckApiKeyInput");
            var status = this.FindControl<TextBlock>("ProxyCheckStatus");

            string key = input?.Text?.Trim() ?? "";
            _application.OptionsSettings.ProxyCheckApiKey = key;

            if (status != null)
                status.Text = string.IsNullOrEmpty(key)
                    ? "Using free tier (1,000 queries/day). Key cleared."
                    : "API key saved. Using paid tier.";
        }
    }
}
