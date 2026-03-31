using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Threading;
using PRoCon.Core;
using PRoCon.Core.Plugin;
using PRoCon.Core.Remote;

namespace PRoCon.UI.Views
{
    public enum PluginState
    {
        Disabled,
        Enabling,
        Enabled
    }

    public class PluginEntry : INotifyPropertyChanged
    {
        private static Avalonia.Media.ISolidColorBrush ResolveBrush(string key, string fallback)
        {
            if (Avalonia.Application.Current != null &&
                Avalonia.Application.Current.TryFindResource(key, Avalonia.Application.Current.ActualThemeVariant, out var value) &&
                value is Avalonia.Media.ISolidColorBrush brush)
                return brush;
            return new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse(fallback));
        }
        private static Avalonia.Media.ISolidColorBrush EnabledBrush => ResolveBrush("SuccessBrush", "#00ff88");
        private static Avalonia.Media.ISolidColorBrush DisabledBrush => ResolveBrush("ErrorBrush", "#ff3c3c");
        private static Avalonia.Media.ISolidColorBrush EnablingBrush => ResolveBrush("WarningBrush", "#ffaa00");

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
                if (value) State = PluginState.Enabled;
                else State = PluginState.Disabled;
                OnPropertyChanged(nameof(IsEnabled));
                OnPropertyChanged(nameof(EnabledText));
                OnPropertyChanged(nameof(InfoLine));
            }
        }

        private PluginState _state = PluginState.Disabled;
        public PluginState State
        {
            get => _state;
            set
            {
                _state = value;
                OnPropertyChanged(nameof(State));
                OnPropertyChanged(nameof(StatusColor));
                OnPropertyChanged(nameof(StatusText));
                OnPropertyChanged(nameof(EnabledText));
            }
        }

        public Avalonia.Media.ISolidColorBrush StatusColor => _state switch
        {
            PluginState.Enabled => EnabledBrush,
            PluginState.Enabling => EnablingBrush,
            _ => DisabledBrush,
        };

        public string StatusText => _state switch
        {
            PluginState.Enabled => "Enabled",
            PluginState.Enabling => "Enabling...",
            _ => "Disabled",
        };

        public string EnabledText => _state switch
        {
            PluginState.Enabled => "ON",
            PluginState.Enabling => "...",
            _ => "OFF",
        };

        public string InfoLine => $"v{Version} by {Author} - {StatusText}";

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public class PluginVariableEntry : INotifyPropertyChanged
    {
        public string ClassName { get; set; } = "";
        public string Name { get; set; } = "";
        public string FullName { get; set; } = "";
        public string VariableType { get; set; } = "";

        private string _value = "";
        public string Value
        {
            get => _value;
            set { _value = value; OnPropertyChanged(nameof(Value)); }
        }

        public bool IsReadOnly { get; set; }

        // Type-based UI helpers
        public bool IsHeader => VariableType == "_header";
        public bool IsNotHeader => VariableType != "_header";
        public bool IsBool => VariableType == "bool";
        public bool IsOnOff => VariableType == "onoff";
        public bool IsYesNo => VariableType == "yesno";
        public bool IsEnum => VariableType.StartsWith("enum.", StringComparison.OrdinalIgnoreCase);
        public bool IsMultiline => VariableType == "multiline" || VariableType == "stringarray";
        public bool IsDropdown => IsBool || IsOnOff || IsYesNo || IsEnum;
        public bool IsTextBox => !IsDropdown && !IsMultiline;

        public List<string> DropdownOptions
        {
            get
            {
                if (IsBool) return new List<string> { "True", "False" };
                if (IsOnOff) return new List<string> { "On", "Off" };
                if (IsYesNo) return new List<string> { "Yes", "No" };
                if (IsEnum)
                {
                    // Parse "enum.TypeName(Option1|Option2|...)"
                    int parenStart = VariableType.IndexOf('(');
                    int parenEnd = VariableType.LastIndexOf(')');
                    if (parenStart >= 0 && parenEnd > parenStart)
                    {
                        string options = VariableType.Substring(parenStart + 1, parenEnd - parenStart - 1);
                        return new List<string>(options.Split('|'));
                    }
                }
                return new List<string>();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public partial class PluginsPanel : UserControl
    {
        private PRoConClient _client;
        private readonly ObservableCollection<PluginEntry> _plugins = new ObservableCollection<PluginEntry>();
        private readonly ObservableCollection<PluginVariableEntry> _variables = new ObservableCollection<PluginVariableEntry>();
        private readonly StringBuilder _outputBuffer = new StringBuilder();
        private const int MaxOutputLines = 500;
        private int _outputLineCount;
        private bool _outputVisible = true;
        private string _selectedClassName;
        private bool _isLoadingPlugin;

        public PluginsPanel()
        {
            InitializeComponent();

            var pluginList = this.FindControl<ListBox>("PluginListBox");
            if (pluginList != null)
                pluginList.ItemsSource = _plugins;

            var variablesControl = this.FindControl<ListBox>("VariablesItemsControl");
            if (variablesControl != null)
                variablesControl.ItemsSource = _variables;

            // Subscribe to pre-compilation output (fires before any server connects)
            PluginManager.PreCompileOutput += OnPluginOutput;
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
                _client.PluginsManager.PluginOutput -= OnPluginOutput;
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
                _client.PluginsManager.PluginOutput += OnPluginOutput;
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
                        _client.PluginsManager.PluginOutput += OnPluginOutput;
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
            var client = _client;
            if (client?.PluginsManager?.Plugins == null)
            {
                _plugins.Clear();
                return;
            }

            // Move plugin detail queries off UI thread
            System.Threading.Tasks.Task.Run(() =>
            {
                var entries = new List<PluginEntry>();
                foreach (string className in client.PluginsManager.Plugins.LoadedClassNames)
                {
                    try
                    {
                        PluginDetails details = client.PluginsManager.GetPluginDetails(className);
                        bool isEnabled = client.PluginsManager.Plugins.IsEnabled(className);

                        entries.Add(new PluginEntry
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
                        entries.Add(new PluginEntry
                        {
                            ClassName = className,
                            DisplayName = className,
                            Version = "?",
                            Author = "Unknown",
                            IsEnabled = false,
                        });
                    }
                }

                entries.Sort((a, b) => string.Compare(a.DisplayName, b.DisplayName, StringComparison.OrdinalIgnoreCase));

                Dispatcher.UIThread.Post(() =>
                {
                    _plugins.Clear();
                    foreach (var entry in entries)
                        _plugins.Add(entry);
                });
            });
        }

        private void OnPluginLoadedEvent(string className)
        {
            Dispatcher.UIThread.Post(() =>
            {
                WriteOutput($"Plugin loaded: {className}");
                RefreshPluginList();
            });
        }

        private void OnPluginEnabledEvent(string className)
        {
            Dispatcher.UIThread.Post(() =>
            {
                WriteOutput($"^2Plugin enabled: {className}");
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
                WriteOutput($"Plugin disabled: {className}");
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
                if (_selectedClassName == details.ClassName && !_isLoadingPlugin)
                    LoadPluginAsync(details.ClassName);
            });
        }

        private static readonly string _debugLogPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "procon-plugin-debug.log");

        private void PluginDebug(string msg)
        {
            string line = $"[{DateTime.Now:HH:mm:ss.fff}] [PluginUI] {msg}";
            Console.Error.WriteLine(line);
            try { System.IO.File.AppendAllText(_debugLogPath, line + Environment.NewLine); } catch { }
        }

        private void OnPluginSelected(object sender, SelectionChangedEventArgs e)
        {
            var listBox = this.FindControl<ListBox>("PluginListBox");
            if (listBox?.SelectedItem is not PluginEntry entry)
                return;

            PluginDebug($"Selected: {entry.ClassName}");
            _selectedClassName = entry.ClassName;
            LoadPluginAsync(entry.ClassName);
        }

        private void LoadPluginAsync(string className)
        {
            if (_isLoadingPlugin) return;

            PluginDebug($"LoadPluginAsync called: {className}");

            if (_client?.PluginsManager == null)
            {
                ClearDetails();
                _variables.Clear();
                return;
            }

            _isLoadingPlugin = true;
            PluginDebug($"LoadPluginAsync start: {className}");

            // Move all plugin calls off the UI thread
            var client = _client;
            System.Threading.Tasks.Task.Run(() =>
            {
                PluginDebug($"Background thread started for {className}");

                PluginDetails details = null;
                List<CPluginVariable> vars = null;

                PluginDebug($"Calling GetPluginDetails...");
                try { details = client.PluginsManager.GetPluginDetails(className); } catch (Exception ex) { PluginDebug($"GetPluginDetails error: {ex.Message}"); }
                PluginDebug($"GetPluginDetails done: {details?.Name ?? "null"}");

                PluginDebug($"Calling GetDisplayPluginVariables...");
                try { vars = client.PluginsManager.GetDisplayPluginVariables(className); } catch (Exception ex) { PluginDebug($"GetDisplayPluginVariables error: {ex.Message}"); }
                PluginDebug($"GetDisplayPluginVariables done: {vars?.Count ?? 0} vars");

                // Build entries on background thread, parsing group|name format
                var entries = new List<PluginVariableEntry>();
                if (vars != null)
                {
                    string lastGroup = null;
                    foreach (var v in vars)
                    {
                        string displayName = v.Name;
                        string fullName = v.Name;
                        string group = null;

                        // Parse "Group|Variable Name" format
                        int pipeIdx = v.Name.IndexOf('|');
                        if (pipeIdx >= 0)
                        {
                            group = v.Name.Substring(0, pipeIdx).Trim();
                            displayName = v.Name.Substring(pipeIdx + 1).Trim();
                        }

                        // Add group header when group changes
                        if (group != null && group != lastGroup)
                        {
                            entries.Add(new PluginVariableEntry
                            {
                                ClassName = className,
                                Name = group,
                                Value = "",
                                VariableType = "_header",
                                IsReadOnly = true,
                            });
                            lastGroup = group;
                        }

                        entries.Add(new PluginVariableEntry
                        {
                            ClassName = className,
                            Name = displayName,
                            FullName = fullName,
                            Value = v.Value ?? "",
                            VariableType = v.Type ?? "",
                            IsReadOnly = v.ReadOnly,
                        });
                    }
                }

                PluginDebug($"Built {entries.Count} entries, posting to UI thread");

                // Update UI on dispatcher
                Dispatcher.UIThread.Post(() =>
                {
                    PluginDebug($"UI thread post executing for {className}");
                    if (_selectedClassName != className) { PluginDebug("Aborted — user switched away"); return; }

                    // Details
                    if (details != null)
                    {
                        var nt = this.FindControl<TextBlock>("DetailNameText");
                        var vt = this.FindControl<TextBlock>("DetailVersionText");
                        var at = this.FindControl<TextBlock>("DetailAuthorText");

                        if (nt != null) nt.Text = !string.IsNullOrEmpty(details.Name) ? details.Name : className;
                        if (vt != null) vt.Text = $"Version: {details.Version ?? "?"}";
                        if (at != null) at.Text = $"Author: {details.Author ?? "Unknown"}";
                        RenderHtmlDescription(details.Description ?? "");
                    }
                    else
                    {
                        ClearDetails();
                    }

                    PluginDebug($"Details updated, loading {entries.Count} variables into list");

                    // Variables — bulk load
                    _variables.Clear();
                    foreach (var entry in entries)
                        _variables.Add(entry);
                    PluginDebug($"Variables added to collection");

                    var vc = this.FindControl<ListBox>("VariablesItemsControl");
                    if (vc != null) vc.ItemsSource = _variables;
                    PluginDebug($"ItemsSource reassigned — done");

                    // Release the loading guard AFTER the UI has processed the new items
                    // This prevents ComboBox SelectionChanged from triggering during population
                    Dispatcher.UIThread.Post(() =>
                    {
                        _isLoadingPlugin = false;
                    }, Avalonia.Threading.DispatcherPriority.Background);
                });
            });
        }

        private void ClearDetails()
        {
            var nameText = this.FindControl<TextBlock>("DetailNameText");
            var versionText = this.FindControl<TextBlock>("DetailVersionText");
            var authorText = this.FindControl<TextBlock>("DetailAuthorText");
            if (nameText != null) nameText.Text = "Select a plugin";
            if (versionText != null) versionText.Text = "";
            if (authorText != null) authorText.Text = "";
            var descPanel = this.FindControl<StackPanel>("DescriptionPanel");
            descPanel?.Children.Clear();
        }

        private void RenderHtmlDescription(string html)
        {
            var panel = this.FindControl<StackPanel>("DescriptionPanel");
            if (panel == null) return;

            panel.Children.Clear();

            if (string.IsNullOrEmpty(html))
                return;

            // Split HTML into blocks by major block elements
            // Replace <br> with newlines, normalize whitespace
            string processed = html;
            processed = System.Text.RegularExpressions.Regex.Replace(processed, @"<br\s*/?>", "\n");
            processed = System.Text.RegularExpressions.Regex.Replace(processed, @"<hr\s*/?>", "\n---\n");

            // Split on block elements
            var blocks = System.Text.RegularExpressions.Regex.Split(processed,
                @"(?=<(?:h[1-6]|p|ul|ol|li|table|blockquote|pre|div)[^>]*>)|(?<=</(?:h[1-6]|p|ul|ol|table|blockquote|pre|div)>)");

            foreach (string rawBlock in blocks)
            {
                string block = rawBlock.Trim();
                if (string.IsNullOrWhiteSpace(block)) continue;

                // Detect block type
                var hMatch = System.Text.RegularExpressions.Regex.Match(block, @"<h([1-6])[^>]*>(.*?)</h\1>",
                    System.Text.RegularExpressions.RegexOptions.Singleline);
                if (hMatch.Success)
                {
                    int level = int.Parse(hMatch.Groups[1].Value);
                    string text = StripTags(hMatch.Groups[2].Value);
                    panel.Children.Add(new TextBlock
                    {
                        Text = text,
                        FontSize = level <= 2 ? 18 : level == 3 ? 16 : 14,
                        FontWeight = Avalonia.Media.FontWeight.Bold,
                        Foreground = FindBrush("TextPrimaryBrush"),
                        TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                        Margin = new Avalonia.Thickness(0, level <= 2 ? 12 : 8, 0, 4),
                    });
                    continue;
                }

                // List items
                if (block.Contains("<li"))
                {
                    var items = System.Text.RegularExpressions.Regex.Matches(block, @"<li[^>]*>(.*?)</li>",
                        System.Text.RegularExpressions.RegexOptions.Singleline);
                    foreach (System.Text.RegularExpressions.Match item in items)
                    {
                        string text = StripTags(item.Groups[1].Value).Trim();
                        if (string.IsNullOrWhiteSpace(text)) continue;
                        panel.Children.Add(new TextBlock
                        {
                            Text = "  \u2022  " + text,
                            FontSize = 12,
                            Foreground = FindBrush("TextPrimaryBrush"),
                            TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                            Margin = new Avalonia.Thickness(8, 1, 0, 1),
                        });
                    }
                    continue;
                }

                // Code/pre blocks
                if (block.Contains("<pre") || block.Contains("<code"))
                {
                    string text = StripTags(block);
                    if (string.IsNullOrWhiteSpace(text)) continue;
                    panel.Children.Add(new Border
                    {
                        Background = FindBrush("SurfaceElevatedBrush"),
                        CornerRadius = new Avalonia.CornerRadius(4),
                        Padding = new Avalonia.Thickness(12, 8),
                        Margin = new Avalonia.Thickness(0, 4),
                        Child = new TextBlock
                        {
                            Text = text,
                            FontFamily = new Avalonia.Media.FontFamily("Cascadia Mono,Consolas,Courier New,monospace"),
                            FontSize = 11,
                            Foreground = FindBrush("TextSecondaryBrush"),
                            TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                        }
                    });
                    continue;
                }

                // Default: paragraph text
                string paraText = StripTags(block);
                if (string.IsNullOrWhiteSpace(paraText)) continue;
                panel.Children.Add(new TextBlock
                {
                    Text = paraText,
                    FontSize = 12,
                    Foreground = FindBrush("TextPrimaryBrush"),
                    TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                    Margin = new Avalonia.Thickness(0, 2),
                });
            }
        }

        private static string StripTags(string html)
        {
            if (string.IsNullOrEmpty(html)) return "";
            string text = System.Text.RegularExpressions.Regex.Replace(html, @"<[^>]+>", " ");
            text = System.Net.WebUtility.HtmlDecode(text);
            text = System.Text.RegularExpressions.Regex.Replace(text, @"[ \t]+", " ");
            text = System.Text.RegularExpressions.Regex.Replace(text, @"\n\s*\n", "\n\n");
            return text.Trim();
        }

        private Avalonia.Media.IBrush FindBrush(string key)
        {
            if (Avalonia.Application.Current.TryFindResource(key, Avalonia.Application.Current.ActualThemeVariant, out object value)
                && value is Avalonia.Media.IBrush brush)
                return brush;
            return Avalonia.Media.Brushes.White;
        }

        private void OnTogglePlugin(object sender, RoutedEventArgs e)
        {
            if (_client?.PluginsManager == null)
                return;

            if (sender is ToggleButton toggle && toggle.DataContext is PluginEntry entry)
            {
                if (entry.IsEnabled)
                {
                    entry.State = PluginState.Enabling;
                    WriteOutput($"Enabling plugin: {entry.DisplayName}...");
                    try
                    {
                        _client.PluginsManager.EnablePlugin(entry.ClassName);
                        entry.State = PluginState.Enabled;
                        WriteOutput($"Plugin enabled: {entry.DisplayName}");
                    }
                    catch (Exception ex)
                    {
                        entry.State = PluginState.Disabled;
                        entry.IsEnabled = false;
                        WriteOutput($"^1Failed to enable {entry.DisplayName}: {ex.Message}");
                    }
                }
                else
                {
                    WriteOutput($"Disabling plugin: {entry.DisplayName}...");
                    try
                    {
                        _client.PluginsManager.DisablePlugin(entry.ClassName);
                        entry.State = PluginState.Disabled;
                        WriteOutput($"Plugin disabled: {entry.DisplayName}");
                    }
                    catch (Exception ex)
                    {
                        WriteOutput($"^1Failed to disable {entry.DisplayName}: {ex.Message}");
                    }
                }
            }
        }

        private void OnVariableValueChanged(object sender, RoutedEventArgs e)
        {
            if (_client?.PluginsManager == null || _isLoadingPlugin)
                return;

            if (sender is TextBox textBox && textBox.DataContext is PluginVariableEntry varEntry)
            {
                if (!varEntry.IsReadOnly)
                {
                    string varName = !string.IsNullOrEmpty(varEntry.FullName) ? varEntry.FullName : varEntry.Name;
                    PluginDebug($"SetVariable: {varName} = {textBox.Text}");

                    try
                    {
                        _client.PluginsManager.SetPluginVariable(varEntry.ClassName, varName, textBox.Text ?? "");
                    }
                    catch (Exception ex)
                    {
                        PluginDebug($"SetVariable error: {ex.Message}");
                    }
                }
            }
        }

        private void OnDropdownChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_client?.PluginsManager == null || _isLoadingPlugin)
                return;

            if (sender is ComboBox combo && combo.DataContext is PluginVariableEntry varEntry)
            {
                if (!varEntry.IsReadOnly && combo.SelectedItem is string selected)
                {
                    varEntry.Value = selected;
                    string varName = !string.IsNullOrEmpty(varEntry.FullName) ? varEntry.FullName : varEntry.Name;
                    PluginDebug($"SetVariable (dropdown): {varName} = {selected}");

                    try
                    {
                        _client.PluginsManager.SetPluginVariable(varEntry.ClassName, varName, selected);
                    }
                    catch (Exception ex)
                    {
                        PluginDebug($"SetVariable error: {ex.Message}");
                    }
                }
            }
        }

        private void OnReloadPlugins(object sender, RoutedEventArgs e)
        {
            if (_client?.PluginsManager != null)
            {
                // Trigger actual recompilation of plugins
                System.Threading.Tasks.Task.Run(() =>
                {
                    try
                    {
                        _client.PluginsManager.CompilePlugins();
                        Dispatcher.UIThread.Post(() =>
                        {
                            RefreshPluginList();
                            if (!string.IsNullOrEmpty(_selectedClassName))
                            {
                                LoadPluginAsync(_selectedClassName);
                            }
                        });
                    }
                    catch { }
                });
            }
            else
            {
                RefreshPluginList();
            }
        }

        private void WriteOutput(string message)
        {
            OnPluginOutput(message);
        }

        // --- Plugin Output Console ---

        private void OnPluginOutput(string output)
        {
            Dispatcher.UIThread.Post(() =>
            {
                // Strip PRoCon color codes (^0-^9, ^b, ^n, ^i)
                string clean = System.Text.RegularExpressions.Regex.Replace(
                    output ?? "", @"\^[0-9bniB]", "");

                string timestamp = DateTime.Now.ToString("HH:mm:ss");
                string line = $"[{timestamp}] {clean}\n";

                if (_outputLineCount >= MaxOutputLines)
                {
                    // Trim oldest lines
                    string text = _outputBuffer.ToString();
                    int cutIdx = text.IndexOf('\n');
                    if (cutIdx >= 0)
                    {
                        _outputBuffer.Remove(0, cutIdx + 1);
                        _outputLineCount--;
                    }
                }

                _outputBuffer.Append(line);
                _outputLineCount++;

                var outputText = this.FindControl<TextBox>("PluginOutputText");
                if (outputText != null)
                {
                    outputText.Text = _outputBuffer.ToString();
                    outputText.CaretIndex = outputText.Text?.Length ?? 0;
                }
            });
        }

        private void OnClearOutput(object sender, RoutedEventArgs e)
        {
            _outputBuffer.Clear();
            _outputLineCount = 0;
            var outputText = this.FindControl<TextBox>("PluginOutputText");
            if (outputText != null)
                outputText.Text = "";
        }

        private async void OnCopyOutput(object sender, RoutedEventArgs e)
        {
            var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
            if (clipboard != null && _outputBuffer.Length > 0)
            {
                await clipboard.SetTextAsync(_outputBuffer.ToString());
                var btn = this.FindControl<Button>("BtnCopyOutput");
                if (btn != null)
                {
                    btn.Content = "Copied!";
                    await System.Threading.Tasks.Task.Delay(1500);
                    btn.Content = "Copy";
                }
            }
        }

        private void OnToggleOutput(object sender, RoutedEventArgs e)
        {
            _outputVisible = !_outputVisible;
            var outputText = this.FindControl<TextBox>("PluginOutputText");
            var toggleBtn = this.FindControl<Button>("BtnToggleOutput");

            if (outputText != null)
                outputText.IsVisible = _outputVisible;
            if (toggleBtn != null)
                toggleBtn.Content = _outputVisible ? "Hide" : "Show";
        }
    }
}
