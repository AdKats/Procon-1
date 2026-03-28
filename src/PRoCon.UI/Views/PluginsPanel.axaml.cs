using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Threading;
using PRoCon.Core;
using PRoCon.Core.Plugin;
using PRoCon.Core.Remote;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace PRoCon.UI.Views
{
    public class PluginEntry : INotifyPropertyChanged
    {
        public string ClassName { get; set; } = "";
        public string DisplayName { get; set; } = "";
        public string Version { get; set; } = "";
        public string Author { get; set; } = "";

        private bool _isEnabled;
        public bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                _isEnabled = value;
                OnPropertyChanged(nameof(IsEnabled));
                OnPropertyChanged(nameof(EnabledText));
                OnPropertyChanged(nameof(InfoLine));
            }
        }

        public string EnabledText => IsEnabled ? "ON" : "OFF";
        public string InfoLine => $"v{Version} by {Author} - {(IsEnabled ? "Enabled" : "Disabled")}";

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public class PluginVariableEntry : INotifyPropertyChanged
    {
        public string ClassName { get; set; } = "";
        public string Name { get; set; } = "";

        private string _value = "";
        public string Value
        {
            get => _value;
            set { _value = value; OnPropertyChanged(nameof(Value)); }
        }

        public bool IsReadOnly { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public partial class PluginsPanel : UserControl
    {
        private PRoConClient _client;
        private readonly ObservableCollection<PluginEntry> _plugins = new ObservableCollection<PluginEntry>();
        private readonly ObservableCollection<PluginVariableEntry> _variables = new ObservableCollection<PluginVariableEntry>();
        private string _selectedClassName;

        public PluginsPanel()
        {
            InitializeComponent();

            var pluginList = this.FindControl<ListBox>("PluginListBox");
            if (pluginList != null)
                pluginList.ItemsSource = _plugins;

            var variablesControl = this.FindControl<ItemsControl>("VariablesItemsControl");
            if (variablesControl != null)
                variablesControl.ItemsSource = _variables;
        }

        public void SetClient(PRoConClient client)
        {
            // Unwire old events
            if (_client?.PluginsManager != null)
            {
                _client.PluginsManager.PluginLoaded -= OnPluginLoadedEvent;
                _client.PluginsManager.PluginEnabled -= OnPluginEnabledEvent;
                _client.PluginsManager.PluginDisabled -= OnPluginDisabledEvent;
                _client.PluginsManager.PluginVariableAltered -= OnPluginVariableAlteredEvent;
            }

            _client = client;
            _plugins.Clear();
            _variables.Clear();
            _selectedClassName = null;
            ClearDetails();

            if (_client?.PluginsManager != null)
            {
                _client.PluginsManager.PluginLoaded += OnPluginLoadedEvent;
                _client.PluginsManager.PluginEnabled += OnPluginEnabledEvent;
                _client.PluginsManager.PluginDisabled += OnPluginDisabledEvent;
                _client.PluginsManager.PluginVariableAltered += OnPluginVariableAlteredEvent;
                RefreshPluginList();
            }
            else if (_client != null)
            {
                // PluginsManager may not exist yet — retry with backoff
                RetryWirePluginsManager(_client, 0);
            }
        }

        private void RetryWirePluginsManager(PRoConClient client, int attempt)
        {
            int[] delays = { 1000, 2000, 4000, 8000, 15000 };
            if (attempt >= delays.Length) return;

            System.Threading.Tasks.Task.Delay(delays[attempt]).ContinueWith(_ =>
            {
                Dispatcher.UIThread.Post(() =>
                {
                    if (_client != client) return; // client changed, abort
                    if (_client?.PluginsManager != null)
                    {
                        _client.PluginsManager.PluginLoaded += OnPluginLoadedEvent;
                        _client.PluginsManager.PluginEnabled += OnPluginEnabledEvent;
                        _client.PluginsManager.PluginDisabled += OnPluginDisabledEvent;
                        _client.PluginsManager.PluginVariableAltered += OnPluginVariableAlteredEvent;
                        RefreshPluginList();
                    }
                    else
                    {
                        RetryWirePluginsManager(client, attempt + 1);
                    }
                });
            });
        }

        private void RefreshPluginList()
        {
            _plugins.Clear();

            if (_client?.PluginsManager?.Plugins == null)
                return;

            foreach (string className in _client.PluginsManager.Plugins.LoadedClassNames)
            {
                try
                {
                    PluginDetails details = _client.PluginsManager.GetPluginDetails(className);
                    bool isEnabled = _client.PluginsManager.Plugins.IsEnabled(className);

                    _plugins.Add(new PluginEntry
                    {
                        ClassName = className,
                        DisplayName = !string.IsNullOrEmpty(details.Name) ? details.Name : className,
                        Version = details.Version ?? "?",
                        Author = details.Author ?? "Unknown",
                        IsEnabled = isEnabled,
                    });
                }
                catch
                {
                    _plugins.Add(new PluginEntry
                    {
                        ClassName = className,
                        DisplayName = className,
                        Version = "?",
                        Author = "Unknown",
                        IsEnabled = false,
                    });
                }
            }
        }

        private void OnPluginLoadedEvent(string className)
        {
            Dispatcher.UIThread.Post(RefreshPluginList);
        }

        private void OnPluginEnabledEvent(string className)
        {
            Dispatcher.UIThread.Post(() =>
            {
                foreach (var entry in _plugins)
                {
                    if (entry.ClassName == className)
                    {
                        entry.IsEnabled = true;
                        break;
                    }
                }
            });
        }

        private void OnPluginDisabledEvent(string className)
        {
            Dispatcher.UIThread.Post(() =>
            {
                foreach (var entry in _plugins)
                {
                    if (entry.ClassName == className)
                    {
                        entry.IsEnabled = false;
                        break;
                    }
                }
            });
        }

        private void OnPluginVariableAlteredEvent(PluginDetails details)
        {
            Dispatcher.UIThread.Post(() =>
            {
                if (_selectedClassName == details.ClassName)
                    LoadPluginVariables(details.ClassName);
            });
        }

        private void OnPluginSelected(object sender, SelectionChangedEventArgs e)
        {
            var listBox = this.FindControl<ListBox>("PluginListBox");
            if (listBox?.SelectedItem is not PluginEntry entry)
                return;

            _selectedClassName = entry.ClassName;
            LoadPluginDetails(entry.ClassName);
            LoadPluginVariables(entry.ClassName);
        }

        private void LoadPluginDetails(string className)
        {
            if (_client?.PluginsManager == null)
            {
                ClearDetails();
                return;
            }

            try
            {
                PluginDetails details = _client.PluginsManager.GetPluginDetails(className);

                var nameText = this.FindControl<TextBlock>("DetailNameText");
                var versionText = this.FindControl<TextBlock>("DetailVersionText");
                var authorText = this.FindControl<TextBlock>("DetailAuthorText");
                var descText = this.FindControl<TextBlock>("DetailDescriptionText");

                if (nameText != null)
                    nameText.Text = !string.IsNullOrEmpty(details.Name) ? details.Name : className;
                if (versionText != null)
                    versionText.Text = $"Version: {details.Version ?? "?"}";
                if (authorText != null)
                    authorText.Text = $"Author: {details.Author ?? "Unknown"}";
                if (descText != null)
                    descText.Text = details.Description ?? "";
            }
            catch
            {
                ClearDetails();
            }
        }

        private void LoadPluginVariables(string className)
        {
            _variables.Clear();

            if (_client?.PluginsManager == null)
                return;

            try
            {
                List<CPluginVariable> vars = _client.PluginsManager.GetPluginVariables(className);
                if (vars != null)
                {
                    foreach (var v in vars)
                    {
                        _variables.Add(new PluginVariableEntry
                        {
                            ClassName = className,
                            Name = v.Name,
                            Value = v.Value ?? "",
                            IsReadOnly = v.ReadOnly,
                        });
                    }
                }
            }
            catch
            {
                // Plugin may not support variables
            }
        }

        private void ClearDetails()
        {
            var nameText = this.FindControl<TextBlock>("DetailNameText");
            var versionText = this.FindControl<TextBlock>("DetailVersionText");
            var authorText = this.FindControl<TextBlock>("DetailAuthorText");
            var descText = this.FindControl<TextBlock>("DetailDescriptionText");

            if (nameText != null) nameText.Text = "Select a plugin";
            if (versionText != null) versionText.Text = "";
            if (authorText != null) authorText.Text = "";
            if (descText != null) descText.Text = "";
        }

        private void OnTogglePlugin(object sender, RoutedEventArgs e)
        {
            if (_client?.PluginsManager == null)
                return;

            if (sender is ToggleButton toggle && toggle.DataContext is PluginEntry entry)
            {
                if (entry.IsEnabled)
                    _client.PluginsManager.EnablePlugin(entry.ClassName);
                else
                    _client.PluginsManager.DisablePlugin(entry.ClassName);
            }
        }

        private void OnVariableValueChanged(object sender, RoutedEventArgs e)
        {
            if (_client?.PluginsManager == null)
                return;

            if (sender is TextBox textBox && textBox.DataContext is PluginVariableEntry varEntry)
            {
                if (!varEntry.IsReadOnly)
                {
                    try
                    {
                        _client.PluginsManager.SetPluginVariable(
                            varEntry.ClassName, varEntry.Name, textBox.Text ?? "");
                    }
                    catch
                    {
                        // Ignore set errors
                    }
                }
            }
        }

        private void OnReloadPlugins(object sender, RoutedEventArgs e)
        {
            RefreshPluginList();

            if (!string.IsNullOrEmpty(_selectedClassName))
            {
                LoadPluginDetails(_selectedClassName);
                LoadPluginVariables(_selectedClassName);
            }
        }
    }
}
