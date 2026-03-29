using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PRoCon.Core.Accounts;
using PRoCon.Core.Remote;
using PRoCon.Core.Remote.Layer;

namespace PRoCon.Core.Layer
{
    /// <summary>
    /// SignalR-based layer service implementing ILayerInstance.
    /// Replaces the old TCP LayerInstance with Kestrel + SignalR Hub.
    /// </summary>
    public class LayerHostService : ILayerInstance, IDisposable
    {
        private WebApplication _app;
        private PRoConApplication _application;
        private PRoConClient _client;
        private readonly LayerHubClientRegistry _registry = new();
        private readonly ConcurrentDictionary<string, SignalRLayerClientAdapter> _clientAdapters = new();

        // ILayerInstance properties
        public Dictionary<string, ILayerClient> Clients
        {
            get
            {
                var dict = new Dictionary<string, ILayerClient>();
                foreach (var kvp in _clientAdapters)
                    dict[kvp.Key] = kvp.Value;
                return dict;
            }
        }

        public AccountPrivilegeDictionary AccountPrivileges { get; private set; }
        public string BindingAddress { get; set; } = "0.0.0.0";
        public ushort ListeningPort { get; set; } = 27260;
        public string NameFormat { get; set; } = "PRoCon[%servername%]";
        public bool IsEnabled { get; set; }
        public bool IsOnline => _app != null;

        // ILayerInstance events
        public event Action LayerStarted;
        public event Action LayerShutdown;
        public event Action<SocketException> SocketError;
        public event Action<ILayerClient> ClientConnected;

        public void Initialize(PRoConApplication application, PRoConClient client)
        {
            _application = application;
            _client = client;

            // Sync account privileges
            AccountPrivileges = new AccountPrivilegeDictionary();
            foreach (Account account in application.AccountsList)
            {
                var priv = new AccountPrivilege(account, new CPrivileges());
                AccountPrivileges.Add(priv);
            }

            // Listen for account changes
            application.AccountsList.AccountAdded += item =>
            {
                if (!AccountPrivileges.Contains(item.Name))
                    AccountPrivileges.Add(new AccountPrivilege(item, new CPrivileges()));
            };

            application.AccountsList.AccountRemoved += item =>
            {
                if (AccountPrivileges.Contains(item.Name))
                    AccountPrivileges.Remove(item.Name);
                // Force disconnect removed accounts
                ForcefullyDisconnectAccount(item.Name);
            };

            // Wire CommandExecutor for the LayerHub
            LayerHub.CommandExecutor = async (command, args) =>
            {
                if (_client?.Game?.Connection == null)
                    return new LayerResponse { Status = "Error", Data = new[] { "Not connected to game server" } };

                try
                {
                    var words = new List<string> { command };
                    words.AddRange(args ?? Array.Empty<string>());

                    var tcs = new TaskCompletionSource<LayerResponse>(TaskCreationOptions.RunContinuationsAsynchronously);

                    // Send via the connection and capture response through the sequence system
                    uint seqNum = _client.Game.Connection.AcquireSequenceNumber;
                    var packet = new Packet(false, false, seqNum, words);

                    // Hook response temporarily
                    FrostbiteConnection.PacketDispatchHandler responseHandler = null;
                    responseHandler = (sender, isHandled, receivedPacket) =>
                    {
                        if (receivedPacket.IsResponse && receivedPacket.SequenceNumber == seqNum)
                        {
                            _client.Game.Connection.PacketReceived -= responseHandler;
                            var data = receivedPacket.Words.ToArray();
                            tcs.TrySetResult(new LayerResponse
                            {
                                Status = data.Length > 0 ? data[0] : "OK",
                                Data = data.Skip(1).ToArray()
                            });
                        }
                    };
                    _client.Game.Connection.PacketReceived += responseHandler;
                    _client.Game.Connection.SendQueued(packet);

                    // Timeout after 10 seconds
                    var completed = await Task.WhenAny(tcs.Task, Task.Delay(10000));
                    if (completed != tcs.Task)
                    {
                        _client.Game.Connection.PacketReceived -= responseHandler;
                        return new LayerResponse { Status = "Timeout", Data = new[] { "Command timed out" } };
                    }

                    return await tcs.Task;
                }
                catch (Exception ex)
                {
                    return new LayerResponse { Status = "Error", Data = new[] { ex.Message } };
                }
            };
        }

        public void Start()
        {
            if (_app != null) return;

            // Run Kestrel startup on a background thread to avoid blocking the UI
            Task.Run(() =>
            {
                try
                {
                    var builder = WebApplication.CreateBuilder();
                    builder.Logging.ClearProviders();
                    builder.Logging.AddConsole();
                    builder.Logging.SetMinimumLevel(LogLevel.Warning);
                    builder.WebHost.UseUrls($"http://{BindingAddress}:{ListeningPort}");
                    builder.Services.AddSignalR();
                    builder.Services.AddSingleton(_registry);
                    builder.Services.AddSingleton(new LayerAuthService());

                    _app = builder.Build();
                    _app.MapHub<LayerHub>("/layer");

                    // Wire hub connection events via the registry
                    LayerHub.OnClientConnected = (connectionId, username) =>
                    {
                        var hubClient = _registry.GetOrAdd(connectionId);
                        var adapter = new SignalRLayerClientAdapter(hubClient);
                        _clientAdapters[connectionId] = adapter;
                        ClientConnected?.Invoke(adapter);
                    };

                    LayerHub.OnClientDisconnected = connectionId =>
                    {
                        _clientAdapters.TryRemove(connectionId, out _);
                    };

                    _app.StartAsync().Wait();
                    LayerStarted?.Invoke();
                }
                catch (SocketException se)
                {
                    _app = null;
                    SocketError?.Invoke(se);
                }
                catch (Exception ex)
                {
                    _app = null;
                    System.Console.Error.WriteLine($"[LayerHostService] Failed to start: {ex.Message}");
                }
            });
        }

        public void Shutdown()
        {
            var app = _app;
            _app = null;
            if (app == null) return;

            // Run shutdown on background thread to avoid UI deadlock
            Task.Run(() =>
            {
                try
                {
                    using var cts = new System.Threading.CancellationTokenSource(TimeSpan.FromSeconds(5));
                    app.StopAsync(cts.Token).Wait();
                    app.DisposeAsync().AsTask().Wait(5000);
                }
                catch { }
            });

            _app = null;
            _clientAdapters.Clear();
            LayerShutdown?.Invoke();
        }

        public List<string> GetLoggedInAccountUsernames()
        {
            return _registry.GetLoggedInUsernames().ToList();
        }

        public List<string> GetLoggedInAccountUsernamesWithUids(bool listUids)
        {
            return GetLoggedInAccountUsernames();
        }

        public void Poke()
        {
            // SignalR handles keepalive automatically
        }

        private void ForcefullyDisconnectAccount(string username)
        {
            foreach (var kvp in _clientAdapters)
            {
                if (string.Equals(kvp.Value.Username, username, StringComparison.OrdinalIgnoreCase))
                {
                    kvp.Value.ForceDisconnect();
                    _clientAdapters.TryRemove(kvp.Key, out _);
                }
            }
        }

        public void Dispose()
        {
            Shutdown();
        }
    }

    /// <summary>
    /// Adapts a SignalR LayerHubClient to the ILayerClient interface
    /// so existing UI code (LayerPanel) can display connected clients.
    /// </summary>
    public class SignalRLayerClientAdapter : ILayerClient
    {
        private readonly LayerHubClient _hubClient;

        public SignalRLayerClientAdapter(LayerHubClient hubClient)
        {
            _hubClient = hubClient;
        }

        public string Username => _hubClient.Username ?? "";
        public string IPPort => _hubClient.ConnectionId ?? "";
        public CPrivileges Privileges => _hubClient.Privileges;
        public string ProconEventsUid => _hubClient.ProconEventsUid ?? "";

        public event Action<ILayerClient> ClientShutdown;
        public event Action<ILayerClient> Login;
        public event Action<ILayerClient> Logout;
        public event Action<ILayerClient> Quit;
        public event Action<ILayerClient> UidRegistered;

        public void SendAccountLogin(string username, CPrivileges privileges) { }
        public void SendAccountLogout(string username) { }
        public void SendRegisteredUid(string uid, string username) { }
        public void Forward(Packet packet) { }
        public void Poke() { }

        public void Shutdown()
        {
            ClientShutdown?.Invoke(this);
        }

        public void ForceDisconnect()
        {
            Shutdown();
        }
    }
}
