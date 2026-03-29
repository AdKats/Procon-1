using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using PRoCon.Core.Events;
using PRoCon.Core.Remote;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace PRoCon.UI.Views
{
    public class EventLogEntry : INotifyPropertyChanged
    {
        public string Timestamp { get; set; } = "";
        public string EventTypeName { get; set; } = "";
        public string EventText { get; set; } = "";
        public EventType EventType { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public partial class EventsPanel : UserControl
    {
        private PRoConClient _client;
        private readonly ObservableCollection<EventLogEntry> _allEvents = new ObservableCollection<EventLogEntry>();
        private readonly ObservableCollection<EventLogEntry> _filteredEvents = new ObservableCollection<EventLogEntry>();

        public EventsPanel()
        {
            InitializeComponent();

            var itemsControl = this.FindControl<ItemsControl>("EventItemsControl");
            if (itemsControl != null)
                itemsControl.ItemsSource = _filteredEvents;
        }

        public void SetClient(PRoConClient client)
        {
            // Unwire old events
            if (_client?.EventsLogging != null)
            {
                _client.EventsLogging.LoggedEvent -= OnLoggedEvent;
            }

            _client = client;
            _allEvents.Clear();
            _filteredEvents.Clear();

            if (_client?.EventsLogging != null)
            {
                _client.EventsLogging.LoggedEvent += OnLoggedEvent;

                // Load existing log entries
                LoadExistingEntries();
            }
        }

        private void LoadExistingEntries()
        {
            if (_client?.EventsLogging?.LogEntries == null)
                return;

            try
            {
                foreach (CapturedEvent captured in _client.EventsLogging.LogEntries)
                {
                    AddEventEntry(captured);
                }
                ApplyFilters();
            }
            catch
            {
                // Queue may be modified during iteration
            }
        }

        private void OnLoggedEvent(CapturedEvent capture)
        {
            Dispatcher.UIThread.Post(() =>
            {
                AddEventEntry(capture);

                // Keep a reasonable max
                while (_allEvents.Count > 500)
                    _allEvents.RemoveAt(0);

                if (IsEventTypeVisible(capture.EventType))
                {
                    _filteredEvents.Add(CreateLogEntry(capture));

                    while (_filteredEvents.Count > 500)
                        _filteredEvents.RemoveAt(0);

                    // Auto-scroll to bottom
                    var scroller = this.FindControl<ScrollViewer>("EventScroller");
                    scroller?.ScrollToEnd();
                }
            });
        }

        private void AddEventEntry(CapturedEvent capture)
        {
            _allEvents.Add(CreateLogEntry(capture));
        }

        private EventLogEntry CreateLogEntry(CapturedEvent capture)
        {
            string timestamp = capture.LoggedTime.ToString("yyyy-MM-dd HH:mm:ss");
            string typeName = capture.EventType.ToString();
            string text = !string.IsNullOrEmpty(capture.EventText)
                ? capture.EventText
                : capture.Event.ToString();

            if (!string.IsNullOrEmpty(capture.InstigatingAdmin))
                text = $"[{capture.InstigatingAdmin}] {text}";

            return new EventLogEntry
            {
                Timestamp = timestamp,
                EventTypeName = typeName,
                EventText = text,
                EventType = capture.EventType,
            };
        }

        private void OnFilterChanged(object sender, RoutedEventArgs e)
        {
            ApplyFilters();
        }

        private void ApplyFilters()
        {
            _filteredEvents.Clear();

            foreach (var entry in _allEvents)
            {
                if (IsEventTypeVisible(entry.EventType))
                    _filteredEvents.Add(entry);
            }
        }

        private bool IsEventTypeVisible(EventType eventType)
        {
            return eventType switch
            {
                EventType.Connection => GetFilterChecked("FilterConnection"),
                EventType.Playerlist => GetFilterChecked("FilterPlayerlist"),
                EventType.Banlist => GetFilterChecked("FilterBanlist"),
                EventType.Map => GetFilterChecked("FilterMap"),
                EventType.Plugins => GetFilterChecked("FilterPlugins"),
                EventType.Layer => GetFilterChecked("FilterLayer"),
                EventType.Game => true,
                _ => true,
            };
        }

        private bool GetFilterChecked(string name)
        {
            var cb = this.FindControl<CheckBox>(name);
            return cb?.IsChecked == true;
        }

        private void OnClearEvents(object sender, RoutedEventArgs e)
        {
            _allEvents.Clear();
            _filteredEvents.Clear();
        }
    }
}
