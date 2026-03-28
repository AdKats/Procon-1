using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using PRoCon.Core;
using PRoCon.Core.Remote;
using System;
using System.Collections.Generic;

namespace PRoCon.UI.Views
{
    public partial class LayerPanel : UserControl
    {
        private PRoConClient _client;

        public LayerPanel()
        {
            InitializeComponent();
        }

        public void SetClient(PRoConClient client)
        {
            UnwireClient();
            _client = client;

            if (_client == null)
            {
                IsEnabled = false;
                return;
            }

            IsEnabled = true;
            WireClient();
            LoadCurrentState();
        }

        public void SetApplication(PRoConApplication app)
        {
            // Not needed for layer panel.
        }

        private void WireClient()
        {
            if (_client?.Layer == null) return;

            _client.Layer.LayerStarted += OnLayerStarted;
            _client.Layer.LayerShutdown += OnLayerShutdown;
            _client.Layer.ClientConnected += OnClientConnected;
        }

        private void UnwireClient()
        {
            if (_client?.Layer == null) return;

            _client.Layer.LayerStarted -= OnLayerStarted;
            _client.Layer.LayerShutdown -= OnLayerShutdown;
            _client.Layer.ClientConnected -= OnClientConnected;
        }

        private void LoadCurrentState()
        {
            if (_client?.Layer == null) return;

            var layer = _client.Layer;

            var enabledCheck = this.FindControl<CheckBox>("LayerEnabledCheck");
            if (enabledCheck != null) enabledCheck.IsChecked = layer.IsEnabled;

            var portInput = this.FindControl<TextBox>("LayerPortInput");
            if (portInput != null) portInput.Text = layer.ListeningPort.ToString();

            var bindingInput = this.FindControl<TextBox>("LayerBindingAddressInput");
            if (bindingInput != null) bindingInput.Text = layer.BindingAddress ?? "";

            UpdateStatusIndicator(layer.IsOnline);
            RefreshClientList();
        }

        // --- Events ---

        private void OnLayerStarted()
        {
            Dispatcher.UIThread.Post(() =>
            {
                UpdateStatusIndicator(true);
                SetStatus("Layer is now online.");
            });
        }

        private void OnLayerShutdown()
        {
            Dispatcher.UIThread.Post(() =>
            {
                UpdateStatusIndicator(false);
                SetStatus("Layer has gone offline.");
            });
        }

        private void OnClientConnected(PRoCon.Core.Remote.Layer.ILayerClient layerClient)
        {
            Dispatcher.UIThread.Post(() =>
            {
                RefreshClientList();
                SetStatus("A new layer client connected.");
            });
        }

        // --- UI Actions ---

        private void OnLayerEnabledToggle(object sender, RoutedEventArgs e)
        {
            if (_client?.Layer == null) return;

            var enabledCheck = this.FindControl<CheckBox>("LayerEnabledCheck");
            bool enabled = enabledCheck?.IsChecked == true;

            _client.Layer.IsEnabled = enabled;

            if (enabled)
            {
                _client.Layer.Start();
                SetStatus("Starting layer...");
            }
            else
            {
                _client.Layer.Shutdown();
                SetStatus("Stopping layer...");
            }
        }

        private void OnApplyLayerConfig(object sender, RoutedEventArgs e)
        {
            if (_client?.Layer == null) return;

            var portInput = this.FindControl<TextBox>("LayerPortInput");
            if (portInput != null && ushort.TryParse(portInput.Text, out ushort port))
            {
                _client.Layer.ListeningPort = port;
            }

            var bindingInput = this.FindControl<TextBox>("LayerBindingAddressInput");
            if (bindingInput != null)
            {
                _client.Layer.BindingAddress = bindingInput.Text ?? "";
            }

            SetStatus("Layer configuration applied. Restart the layer to take effect.");
        }

        private void OnRefreshClients(object sender, RoutedEventArgs e)
        {
            RefreshClientList();
        }

        // --- Helpers ---

        private void RefreshClientList()
        {
            var clientsList = this.FindControl<ListBox>("LayerClientsList");
            if (clientsList == null || _client?.Layer == null) return;

            var items = new List<string>();

            var loggedIn = _client.Layer.GetLoggedInAccountUsernames();
            if (loggedIn != null)
            {
                foreach (var username in loggedIn)
                    items.Add(username);
            }

            if (items.Count == 0)
                items.Add("(No clients connected)");

            clientsList.ItemsSource = items;

            // Update client count badge
            int count = loggedIn?.Count ?? 0;
            var countText = this.FindControl<TextBlock>("ClientCountText");
            if (countText != null) countText.Text = count.ToString();

            // Update connection info
            UpdateConnectionInfo();
        }

        private void UpdateConnectionInfo()
        {
            var infoText = this.FindControl<TextBlock>("LayerConnectionInfo");
            if (infoText == null || _client?.Layer == null) return;

            var layer = _client.Layer;
            if (layer.IsOnline)
            {
                string bind = string.IsNullOrEmpty(layer.BindingAddress) ? "0.0.0.0" : layer.BindingAddress;
                infoText.Text = $"Protocol: SignalR WebSocket (ASP.NET Core)\n" +
                                $"Endpoint: ws://{bind}:{layer.ListeningPort}/layer\n" +
                                $"Auth: JWT Bearer Token\n" +
                                $"Status: Listening for connections";
            }
            else
            {
                infoText.Text = "Layer is not running. Enable the layer to accept remote admin connections.";
            }
        }

        private void UpdateStatusIndicator(bool isOnline)
        {
            var indicator = this.FindControl<Ellipse>("LayerStatusIndicator");
            var statusText = this.FindControl<TextBlock>("LayerStatusText");

            if (indicator != null)
                indicator.Fill = new SolidColorBrush(Color.Parse(isOnline ? "#66bb6a" : "#ef5350"));
            if (statusText != null)
                statusText.Text = isOnline ? "Online" : "Offline";

            UpdateConnectionInfo();
        }

        private void SetStatus(string message)
        {
            var status = this.FindControl<TextBlock>("LayerStatusMessage");
            if (status != null) status.Text = message;
        }
    }
}
