using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace PRoCon.Core.Layer
{
    /// <summary>
    /// SignalR Hub that replaces <see cref="PRoCon.Core.Remote.Layer.LayerInstance"/>
    /// and <see cref="PRoCon.Core.Remote.Layer.LayerClient"/> for admin-to-PRoCon
    /// communication.  All connected admins join the "Admins" group upon login so
    /// events can be broadcast efficiently.
    /// </summary>
    public class LayerHub : Hub
    {
        /// <summary>
        /// SignalR group name for all authenticated admin connections.
        /// </summary>
        public const string AdminGroupName = "Admins";

        private readonly LayerHubClientRegistry _registry;
        private readonly LayerAuthService _auth;
        private readonly ILogger<LayerHub> _logger;

        /// <summary>
        /// Delegate invoked when a validated command should be forwarded to the
        /// game server.  The host application wires this up during startup so the
        /// hub does not depend directly on <c>PRoConClient</c>.
        /// </summary>
        public static Func<string, string[], Task<LayerResponse>> CommandExecutor { get; set; }

        /// <summary>Callback for when a client connects and authenticates.</summary>
        public static Action<string, string> OnClientConnected { get; set; }

        /// <summary>Callback for when a client disconnects.</summary>
        public static Action<string> OnClientDisconnected { get; set; }

        public LayerHub(LayerHubClientRegistry registry, LayerAuthService auth, ILogger<LayerHub> logger)
        {
            _registry = registry ?? throw new ArgumentNullException(nameof(registry));
            _auth = auth ?? throw new ArgumentNullException(nameof(auth));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // -----------------------------------------------------------------
        //  Connection lifecycle
        // -----------------------------------------------------------------

        public override Task OnConnectedAsync()
        {
            _registry.GetOrAdd(Context.ConnectionId);
            OnClientConnected?.Invoke(Context.ConnectionId, null);
            _logger.LogInformation("Layer client connected: {ConnectionId}", Context.ConnectionId);
            return base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            if (_registry.TryRemove(Context.ConnectionId, out var client))
            {
                if (client.IsLoggedIn)
                {
                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, AdminGroupName);
                    await BroadcastAccountLogout(client.Username);
                    _logger.LogInformation("Layer client disconnected (user {Username}): {ConnectionId}",
                        client.Username, Context.ConnectionId);
                }
                else
                {
                    _logger.LogInformation("Layer client disconnected (unauthenticated): {ConnectionId}",
                        Context.ConnectionId);
                }
            }

            OnClientDisconnected?.Invoke(Context.ConnectionId);
            await base.OnDisconnectedAsync(exception);
        }

        // -----------------------------------------------------------------
        //  Authentication
        // -----------------------------------------------------------------

        /// <summary>
        /// Authenticates the connection using a JWT obtained from
        /// <see cref="LayerAuthService.GenerateToken"/>.
        /// </summary>
        public async Task<LayerResponse> Login(LayerRequest request)
        {
            var client = _registry.GetOrAdd(Context.ConnectionId);
            client.Touch();

            if (request?.Args == null || request.Args.Length < 1)
            {
                return MakeResponse(request, "InvalidArguments");
            }

            var token = request.Args[0];
            var principal = _auth.ValidateToken(token);

            if (principal == null)
            {
                _logger.LogWarning("Login failed (invalid token) for {ConnectionId}", Context.ConnectionId);
                return MakeResponse(request, "InvalidPassword");
            }

            var username = LayerAuthService.GetUsername(principal);
            var privileges = LayerAuthService.GetPrivileges(principal);

            if (string.IsNullOrEmpty(username) || !privileges.CanLogin)
            {
                return MakeResponse(request, "InsufficientPrivileges");
            }

            client.Username = username;
            client.Privileges = privileges;
            client.IsLoggedIn = true;

            await Groups.AddToGroupAsync(Context.ConnectionId, AdminGroupName);

            // Notify all admins about the new login
            await Clients.Group(AdminGroupName).SendAsync("ReceiveEvent", new LayerEvent
            {
                EventName = "procon.account.onLogin",
                Data = new { Username = username, PrivilegesFlags = privileges.PrivilegesFlags }
            });

            _logger.LogInformation("User {Username} logged in via {ConnectionId}", username, Context.ConnectionId);

            return MakeResponse(request, "OK", username, privileges.PrivilegesFlags.ToString());
        }

        /// <summary>
        /// Logs out the current connection without disconnecting.
        /// </summary>
        public async Task<LayerResponse> Logout(LayerRequest request)
        {
            var client = _registry.GetOrAdd(Context.ConnectionId);
            client.Touch();

            if (!client.IsLoggedIn)
            {
                return MakeResponse(request, "OK");
            }

            var username = client.Username;
            client.IsLoggedIn = false;
            client.Username = string.Empty;
            client.Privileges = new CPrivileges();

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, AdminGroupName);
            await BroadcastAccountLogout(username);

            return MakeResponse(request, "OK");
        }

        // -----------------------------------------------------------------
        //  Command execution
        // -----------------------------------------------------------------

        /// <summary>
        /// Executes an arbitrary game-server command. The caller must be
        /// logged in; privilege enforcement happens per-command category.
        /// </summary>
        public async Task<LayerResponse> ExecuteCommand(LayerRequest request)
        {
            var client = _registry.GetOrAdd(Context.ConnectionId);
            client.Touch();

            var loginCheck = client.RequireLogin();
            if (loginCheck != null)
                return MakeResponse(request, loginCheck);

            if (request == null || string.IsNullOrEmpty(request.Command))
                return MakeResponse(request, "InvalidArguments");

            // Privilege gate — map commands to required privileges,
            // mirroring the old LayerPacketDispatcher categories.
            var privError = CheckCommandPrivilege(client, request.Command);
            if (privError != null)
                return MakeResponse(request, privError);

            if (CommandExecutor == null)
            {
                _logger.LogWarning("CommandExecutor not wired — cannot execute {Command}", request.Command);
                return MakeResponse(request, "UnknownCommand");
            }

            try
            {
                var response = await CommandExecutor(request.Command, request.Args ?? Array.Empty<string>());
                response.Id = request?.Id ?? string.Empty;
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing command {Command}", request.Command);
                return MakeResponse(request, "InternalError");
            }
        }

        /// <summary>
        /// Sends a chat message to the game server. Convenience wrapper around
        /// the "admin.say" command.
        /// </summary>
        public Task<LayerResponse> SendChat(LayerRequest request)
        {
            var wrapped = new LayerRequest
            {
                Id = request?.Id ?? string.Empty,
                Command = "admin.say",
                Args = request?.Args ?? Array.Empty<string>()
            };
            return ExecuteCommand(wrapped);
        }

        /// <summary>
        /// Retrieves current server information. No special privileges required
        /// beyond being logged in (mirrors the old "unsecure safe listed" category).
        /// </summary>
        public Task<LayerResponse> GetServerInfo(LayerRequest request)
        {
            var wrapped = new LayerRequest
            {
                Id = request?.Id ?? string.Empty,
                Command = "serverInfo",
                Args = request?.Args ?? Array.Empty<string>()
            };
            return ExecuteCommand(wrapped);
        }

        /// <summary>
        /// Retrieves the current player list. Requires login only.
        /// </summary>
        public Task<LayerResponse> GetPlayerList(LayerRequest request)
        {
            var wrapped = new LayerRequest
            {
                Id = request?.Id ?? string.Empty,
                Command = "admin.listPlayers",
                Args = request?.Args ?? Array.Empty<string>()
            };
            return ExecuteCommand(wrapped);
        }

        // -----------------------------------------------------------------
        //  Event subscription
        // -----------------------------------------------------------------

        /// <summary>
        /// Toggles event broadcasting for this connection. When enabled the
        /// client receives server events via the "ReceiveEvent" callback.
        /// </summary>
        public Task<LayerResponse> SetEventsEnabled(LayerRequest request)
        {
            var client = _registry.GetOrAdd(Context.ConnectionId);
            client.Touch();

            var loginCheck = client.RequireLogin();
            if (loginCheck != null)
                return Task.FromResult(MakeResponse(request, loginCheck));

            if (request?.Args == null || request.Args.Length < 1 ||
                !bool.TryParse(request.Args[0], out var enabled))
            {
                return Task.FromResult(MakeResponse(request, "InvalidArguments"));
            }

            client.EventsEnabled = enabled;
            return Task.FromResult(MakeResponse(request, "OK"));
        }

        /// <summary>
        /// Registers a UID for this connection (mirrors procon.registerUid).
        /// </summary>
        public async Task<LayerResponse> RegisterUid(LayerRequest request)
        {
            var client = _registry.GetOrAdd(Context.ConnectionId);
            client.Touch();

            var loginCheck = client.RequireLogin();
            if (loginCheck != null)
                return MakeResponse(request, loginCheck);

            if (request?.Args == null || request.Args.Length < 1)
                return MakeResponse(request, "InvalidArguments");

            client.ProconEventsUid = request.Args[0];

            // Broadcast UID registration to other admins
            await Clients.Group(AdminGroupName).SendAsync("ReceiveEvent", new LayerEvent
            {
                EventName = "procon.account.onUidRegistered",
                Data = new { Uid = client.ProconEventsUid, Username = client.Username }
            });

            return MakeResponse(request, "OK");
        }

        // -----------------------------------------------------------------
        //  Broadcasting helpers
        // -----------------------------------------------------------------

        /// <summary>
        /// Pushes a server event to all authenticated admin connections that
        /// have events enabled.
        /// </summary>
        public async Task BroadcastEvent(LayerEvent layerEvent)
        {
            await Clients.Group(AdminGroupName).SendAsync("ReceiveEvent", layerEvent);
        }

        private async Task BroadcastAccountLogout(string username)
        {
            if (string.IsNullOrEmpty(username)) return;

            await Clients.Group(AdminGroupName).SendAsync("ReceiveEvent", new LayerEvent
            {
                EventName = "procon.account.onLogout",
                Data = new { Username = username }
            });

            _logger.LogInformation("Broadcast logout for {Username}", username);
        }

        // -----------------------------------------------------------------
        //  Privilege enforcement
        // -----------------------------------------------------------------

        /// <summary>
        /// Maps a command to the required privilege, mirroring the dispatch
        /// categories in the old <see cref="PRoCon.Core.Remote.Layer.LayerPacketDispatcher"/>.
        /// Returns null if the client has sufficient privileges, or an error string otherwise.
        /// </summary>
        private static string CheckCommandPrivilege(LayerHubClient client, string command)
        {
            // Commands that require no special privileges beyond being logged in
            // (the "unsecure safe listed" and "secure safe listed" categories)
            if (IsReadOnlyCommand(command))
                return null;

            // Map/round control
            if (IsMapFunctionCommand(command))
                return client.RequirePrivilege(p => p.CanUseMapFunctions);

            // Server variable changes
            if (command.StartsWith("vars.", StringComparison.OrdinalIgnoreCase) ||
                command.StartsWith("levelVars.", StringComparison.OrdinalIgnoreCase))
                return client.RequirePrivilege(p => p.CanAlterServerSettings);

            // Ban list modifications
            if (command.StartsWith("banList.", StringComparison.OrdinalIgnoreCase) &&
                !command.Equals("banList.list", StringComparison.OrdinalIgnoreCase))
                return client.RequirePrivilege(p => p.CanEditBanList);

            // Map list modifications
            if (command.StartsWith("mapList.", StringComparison.OrdinalIgnoreCase) &&
                !command.Equals("mapList.list", StringComparison.OrdinalIgnoreCase))
                return client.RequirePrivilege(p => p.CanEditMapList);

            // Reserved slots
            if (command.StartsWith("reservedSlotsList.", StringComparison.OrdinalIgnoreCase) &&
                !command.Equals("reservedSlotsList.list", StringComparison.OrdinalIgnoreCase))
                return client.RequirePrivilege(p => p.CanEditReservedSlotsList);

            // Text chat moderation
            if (command.StartsWith("textChatModerationList.", StringComparison.OrdinalIgnoreCase) &&
                !command.Equals("textChatModerationList.list", StringComparison.OrdinalIgnoreCase))
                return client.RequirePrivilege(p => p.CanEditTextChatModerationList);

            // PunkBuster
            if (command.StartsWith("punkBuster.", StringComparison.OrdinalIgnoreCase))
                return client.RequirePrivilege(p => p.CanIssueAllPunkbusterCommands);

            // Player punishment
            if (command.Equals("admin.kickPlayer", StringComparison.OrdinalIgnoreCase))
                return client.RequirePrivilege(p => p.CanKickPlayers);

            if (command.Equals("admin.killPlayer", StringComparison.OrdinalIgnoreCase))
                return client.RequirePrivilege(p => p.CanKillPlayers);

            if (command.Equals("admin.movePlayer", StringComparison.OrdinalIgnoreCase))
                return client.RequirePrivilege(p => p.CanMovePlayers);

            // Server shutdown
            if (command.Equals("admin.shutDown", StringComparison.OrdinalIgnoreCase))
                return client.RequirePrivilege(p => p.CanShutdownServer);

            // PRoCon-specific commands
            if (command.StartsWith("procon.", StringComparison.OrdinalIgnoreCase))
                return client.RequirePrivilege(p => p.CanIssueAllProconCommands);

            // Default: allow logged-in users for unrecognised commands
            // (the old dispatcher forwarded unknowns as-is)
            return null;
        }

        private static bool IsReadOnlyCommand(string command)
        {
            return command.Equals("serverInfo", StringComparison.OrdinalIgnoreCase) ||
                   command.Equals("version", StringComparison.OrdinalIgnoreCase) ||
                   command.Equals("admin.listPlayers", StringComparison.OrdinalIgnoreCase) ||
                   command.Equals("listPlayers", StringComparison.OrdinalIgnoreCase) ||
                   command.Equals("admin.currentLevel", StringComparison.OrdinalIgnoreCase) ||
                   command.Equals("admin.supportedMaps", StringComparison.OrdinalIgnoreCase) ||
                   command.Equals("admin.getPlaylist", StringComparison.OrdinalIgnoreCase) ||
                   command.Equals("admin.getPlaylists", StringComparison.OrdinalIgnoreCase) ||
                   command.Equals("banList.list", StringComparison.OrdinalIgnoreCase) ||
                   command.Equals("mapList.list", StringComparison.OrdinalIgnoreCase) ||
                   command.Equals("reservedSlotsList.list", StringComparison.OrdinalIgnoreCase) ||
                   command.Equals("textChatModerationList.list", StringComparison.OrdinalIgnoreCase) ||
                   command.Equals("levelVars.list", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsMapFunctionCommand(string command)
        {
            return command.Equals("admin.runNextLevel", StringComparison.OrdinalIgnoreCase) ||
                   command.Equals("admin.restartMap", StringComparison.OrdinalIgnoreCase) ||
                   command.Equals("admin.endRound", StringComparison.OrdinalIgnoreCase) ||
                   command.Equals("admin.runNextRound", StringComparison.OrdinalIgnoreCase) ||
                   command.Equals("admin.restartRound", StringComparison.OrdinalIgnoreCase) ||
                   command.Equals("mapList.nextLevelIndex", StringComparison.OrdinalIgnoreCase);
        }

        // -----------------------------------------------------------------
        //  Helpers
        // -----------------------------------------------------------------

        private static LayerResponse MakeResponse(LayerRequest request, string status, params object[] data)
        {
            return new LayerResponse
            {
                Id = request?.Id ?? string.Empty,
                Status = status,
                Data = data
            };
        }
    }
}
