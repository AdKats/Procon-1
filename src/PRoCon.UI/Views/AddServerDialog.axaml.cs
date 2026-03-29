using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;

namespace PRoCon.UI.Views
{
    public partial class AddServerDialog : Window
    {
        public string Host { get; private set; }
        public ushort Port { get; private set; }
        public string Username { get; private set; }
        public string Password { get; private set; }
        public bool IsLayerConnection { get; private set; }
        public bool Confirmed { get; private set; }

        public AddServerDialog()
        {
            InitializeComponent();
        }

        private void OnModeDirectClick(object sender, RoutedEventArgs e)
        {
            IsLayerConnection = false;
            UpdateMode();
        }

        private void OnModeLayerClick(object sender, RoutedEventArgs e)
        {
            IsLayerConnection = true;
            UpdateMode();
        }

        private void UpdateMode()
        {
            var directBtn = this.FindControl<Button>("ModeDirectBtn");
            var layerBtn = this.FindControl<Button>("ModeLayerBtn");
            var title = this.FindControl<TextBlock>("DialogTitle");
            var desc = this.FindControl<TextBlock>("DialogDescription");
            var usernameLabel = this.FindControl<TextBlock>("UsernameLabel");
            var usernameInput = this.FindControl<TextBox>("UsernameInput");
            var passwordInput = this.FindControl<TextBox>("PasswordInput");
            var portInput = this.FindControl<TextBox>("PortInput");

            if (IsLayerConnection)
            {
                if (directBtn != null) { directBtn.Background = new SolidColorBrush(Color.Parse("#2a3a4a")); directBtn.Foreground = new SolidColorBrush(Color.Parse("#8899aa")); }
                if (layerBtn != null) { layerBtn.Background = new SolidColorBrush(Color.Parse("#4fc3f7")); layerBtn.Foreground = new SolidColorBrush(Color.Parse("#0f1923")); }
                if (title != null) title.Text = "Connect to PRoCon Layer";
                if (desc != null) desc.Text = "Connect to a remote PRoCon instance via its SignalR layer service. Requires a PRoCon account.";
                if (usernameLabel != null) usernameLabel.IsVisible = true;
                if (usernameInput != null) usernameInput.IsVisible = true;
                if (passwordInput != null) passwordInput.Watermark = "PRoCon account password";
                if (portInput != null) portInput.Watermark = "e.g. 27260";
            }
            else
            {
                if (directBtn != null) { directBtn.Background = new SolidColorBrush(Color.Parse("#4fc3f7")); directBtn.Foreground = new SolidColorBrush(Color.Parse("#0f1923")); }
                if (layerBtn != null) { layerBtn.Background = new SolidColorBrush(Color.Parse("#2a3a4a")); layerBtn.Foreground = new SolidColorBrush(Color.Parse("#8899aa")); }
                if (title != null) title.Text = "Add Game Server";
                if (desc != null) desc.Text = "Connect directly to a game server's RCON port.";
                if (usernameLabel != null) usernameLabel.IsVisible = false;
                if (usernameInput != null) usernameInput.IsVisible = false;
                if (passwordInput != null) passwordInput.Watermark = "RCON password";
                if (portInput != null) portInput.Watermark = "e.g. 47300";
            }
        }

        private void OnAdd(object sender, RoutedEventArgs e)
        {
            var hostInput = this.FindControl<TextBox>("HostInput");
            var portInput = this.FindControl<TextBox>("PortInput");
            var usernameInput = this.FindControl<TextBox>("UsernameInput");
            var passwordInput = this.FindControl<TextBox>("PasswordInput");
            var errorText = this.FindControl<TextBlock>("ErrorText");

            string host = hostInput?.Text?.Trim() ?? "";
            string portStr = portInput?.Text?.Trim() ?? "";
            string username = usernameInput?.Text?.Trim() ?? "";
            string password = passwordInput?.Text?.Trim() ?? "";

            if (string.IsNullOrEmpty(host))
            {
                ShowError(errorText, "Host is required");
                return;
            }
            if (!ushort.TryParse(portStr, out ushort port) || port == 0)
            {
                ShowError(errorText, "Valid port number is required");
                return;
            }
            if (IsLayerConnection && string.IsNullOrEmpty(username))
            {
                ShowError(errorText, "Username is required for layer connections");
                return;
            }

            Host = host;
            Port = port;
            Username = username;
            Password = password;
            Confirmed = true;
            Close();
        }

        private void ShowError(TextBlock errorText, string message)
        {
            if (errorText != null) { errorText.Text = message; errorText.IsVisible = true; }
        }

        private void OnCancel(object sender, RoutedEventArgs e)
        {
            Confirmed = false;
            Close();
        }
    }
}
