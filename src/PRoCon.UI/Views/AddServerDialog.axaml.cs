using Avalonia.Controls;
using Avalonia.Interactivity;

namespace PRoCon.UI.Views
{
    public partial class AddServerDialog : Window
    {
        public string Host { get; private set; }
        public ushort Port { get; private set; }
        public string Password { get; private set; }
        public bool Confirmed { get; private set; }

        public AddServerDialog()
        {
            InitializeComponent();
        }

        private void OnAdd(object sender, RoutedEventArgs e)
        {
            var hostInput = this.FindControl<TextBox>("HostInput");
            var portInput = this.FindControl<TextBox>("PortInput");
            var passwordInput = this.FindControl<TextBox>("PasswordInput");
            var errorText = this.FindControl<TextBlock>("ErrorText");

            string host = hostInput?.Text?.Trim() ?? "";
            string portStr = portInput?.Text?.Trim() ?? "";
            string password = passwordInput?.Text?.Trim() ?? "";

            if (string.IsNullOrEmpty(host))
            {
                if (errorText != null) { errorText.Text = "Host is required"; errorText.IsVisible = true; }
                return;
            }
            if (!ushort.TryParse(portStr, out ushort port) || port == 0)
            {
                if (errorText != null) { errorText.Text = "Valid port number is required"; errorText.IsVisible = true; }
                return;
            }

            Host = host;
            Port = port;
            Password = password;
            Confirmed = true;
            Close();
        }

        private void OnCancel(object sender, RoutedEventArgs e)
        {
            Confirmed = false;
            Close();
        }
    }
}
