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
            WireEvents();
        }

        private void WireEvents()
        {
            if (_application == null) return;

            _application.HttpServerOnline += OnHttpServerOnline;
            _application.HttpServerOffline += OnHttpServerOffline;
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

            // Language
            var currentLang = _application.CurrentLanguage;
            var langText = this.FindControl<TextBlock>("CurrentLanguageText");
            if (langText != null && currentLang != null)
                langText.Text = $"Current: {currentLang.FileName}";

            // HTTP Server
            var httpServer = _application.HttpWebServer;
            if (httpServer != null)
            {
                var portInput = this.FindControl<TextBox>("HttpPortInput");
                if (portInput != null) portInput.Text = httpServer.ListeningPort.ToString();

                var bindingInput = this.FindControl<TextBox>("HttpBindingAddressInput");
                if (bindingInput != null) bindingInput.Text = httpServer.BindingAddress ?? "";

                UpdateHttpStatus(httpServer.IsOnline);
            }

            // Runtime info
            var runtimeInfo = this.FindControl<TextBlock>("RuntimeInfoText");
            if (runtimeInfo != null)
            {
                runtimeInfo.Text = $"OS: {RuntimeInformation.OSDescription} | " +
                                   $"Framework: {RuntimeInformation.FrameworkDescription}";
            }
        }

        // --- Event handlers ---

        private void OnHttpServerOnline(PRoCon.Core.HttpServer.HttpWebServer sender)
        {
            Dispatcher.UIThread.Post(() =>
            {
                UpdateHttpStatus(true);
                SetStatus("HTTP server is now online.");
            });
        }

        private void OnHttpServerOffline(PRoCon.Core.HttpServer.HttpWebServer sender)
        {
            Dispatcher.UIThread.Post(() =>
            {
                UpdateHttpStatus(false);
                SetStatus("HTTP server is now offline.");
            });
        }

        // --- UI Actions ---

        private void OnAutoCheckUpdatesToggle(object sender, RoutedEventArgs e)
        {
            if (_application?.OptionsSettings == null) return;
            var cb = this.FindControl<CheckBox>("AutoCheckUpdatesCheck");
            if (cb != null) _application.OptionsSettings.AutoCheckDownloadUpdates = cb.IsChecked == true;
            SetStatus("Update check setting saved.");
        }

        private void OnAutoApplyUpdatesToggle(object sender, RoutedEventArgs e)
        {
            if (_application?.OptionsSettings == null) return;
            var cb = this.FindControl<CheckBox>("AutoApplyUpdatesCheck");
            if (cb != null) _application.OptionsSettings.AutoApplyUpdates = cb.IsChecked == true;
            SetStatus("Auto-apply updates setting saved.");
        }

        private void OnShowTrayIconToggle(object sender, RoutedEventArgs e)
        {
            if (_application?.OptionsSettings == null) return;
            var cb = this.FindControl<CheckBox>("ShowTrayIconCheck");
            if (cb != null) _application.OptionsSettings.ShowTrayIcon = cb.IsChecked == true;
        }

        private void OnCloseToTrayToggle(object sender, RoutedEventArgs e)
        {
            if (_application?.OptionsSettings == null) return;
            var cb = this.FindControl<CheckBox>("CloseToTrayCheck");
            if (cb != null) _application.OptionsSettings.CloseToTray = cb.IsChecked == true;
        }

        private void OnMinimizeToTrayToggle(object sender, RoutedEventArgs e)
        {
            if (_application?.OptionsSettings == null) return;
            var cb = this.FindControl<CheckBox>("MinimizeToTrayCheck");
            if (cb != null) _application.OptionsSettings.MinimizeToTray = cb.IsChecked == true;
        }

        private void OnLanguageChanged(object sender, SelectionChangedEventArgs e)
        {
            // Language selection would load localization files.
            // For now, note the selection.
            var combo = this.FindControl<ComboBox>("LanguageComboBox");
            if (combo?.SelectedItem is string langName)
            {
                SetStatus($"Language selection changed to: {langName}");
            }
        }

        private void OnStartHttpServer(object sender, RoutedEventArgs e)
        {
            if (_application?.HttpWebServer == null) return;

            var portInput = this.FindControl<TextBox>("HttpPortInput");
            if (portInput != null && ushort.TryParse(portInput.Text, out ushort port))
                _application.HttpWebServer.ListeningPort = port;

            var bindingInput = this.FindControl<TextBox>("HttpBindingAddressInput");
            if (bindingInput != null)
                _application.HttpWebServer.BindingAddress = bindingInput.Text ?? "";

            try
            {
                _application.HttpWebServer.Start();
                SetStatus("Starting HTTP server...");
            }
            catch (Exception ex)
            {
                SetStatus($"Failed to start HTTP server: {ex.Message}");
            }
        }

        private void OnStopHttpServer(object sender, RoutedEventArgs e)
        {
            if (_application?.HttpWebServer == null) return;

            _application.HttpWebServer.Shutdown();
            SetStatus("Stopping HTTP server...");
        }

        // --- Helpers ---

        private void SetCheck(string controlName, bool value)
        {
            var cb = this.FindControl<CheckBox>(controlName);
            if (cb != null) cb.IsChecked = value;
        }

        private void UpdateHttpStatus(bool isOnline)
        {
            var indicator = this.FindControl<Ellipse>("HttpStatusIndicator");
            var statusText = this.FindControl<TextBlock>("HttpStatusText");

            if (indicator != null)
                indicator.Fill = new SolidColorBrush(Color.Parse(isOnline ? "#66bb6a" : "#ef5350"));
            if (statusText != null)
                statusText.Text = isOnline ? "Online" : "Offline";
        }

        private void SetStatus(string message)
        {
            var status = this.FindControl<TextBlock>("OptionsStatusText");
            if (status != null) status.Text = message;
        }
    }
}
