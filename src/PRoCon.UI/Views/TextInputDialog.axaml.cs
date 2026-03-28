using Avalonia.Controls;
using Avalonia.Interactivity;

namespace PRoCon.UI.Views
{
    public partial class TextInputDialog : Window
    {
        public string ResultText { get; private set; }
        public bool Confirmed { get; private set; }

        public TextInputDialog()
        {
            InitializeComponent();
        }

        public TextInputDialog(string title, string prompt, string watermark = "Type here...") : this()
        {
            Title = title;
            var promptText = this.FindControl<TextBlock>("PromptText");
            var inputBox = this.FindControl<TextBox>("InputBox");
            if (promptText != null) promptText.Text = prompt;
            if (inputBox != null) inputBox.Watermark = watermark;
        }

        private void OnOK(object sender, RoutedEventArgs e)
        {
            var inputBox = this.FindControl<TextBox>("InputBox");
            ResultText = inputBox?.Text?.Trim() ?? "";
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
