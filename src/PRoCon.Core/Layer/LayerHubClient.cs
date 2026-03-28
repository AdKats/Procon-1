using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace PRoCon.Core.Layer
{
    /// <summary>
    /// Tracks per-connection state for an admin connected via the SignalR LayerHub.
    /// Replaces the state-management portions of
    /// <see cref="PRoCon.Core.Remote.Layer.LayerClient"/> (IsLoggedIn, Username,
    /// Privileges, EventsEnabled, etc.) without the TCP/packet plumbing.
    /// </summary>
    public class LayerHubClient
    {
        /// <summary>
        /// SignalR connection identifier (maps to Context.ConnectionId in the hub).
        /// </summary>
        public string ConnectionId { get; }

        /// <summary>
        /// The username the client authenticated with, or empty if not yet logged in.
        /// </summary>
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// Whether this connection has successfully authenticated.
        /// </summary>
        public bool IsLoggedIn { get; set; }

        /// <summary>
        /// The privileges of the authenticated user. Defaults to the lowest
        /// privilege set (no permissions).
        /// </summary>
        public CPrivileges Privileges { get; set; } = new CPrivileges();

        /// <summary>
        /// Whether the client has opted in to receiving server events.
        /// </summary>
        public bool EventsEnabled { get; set; }

        /// <summary>
        /// Optional UID the client registers to identify itself for event routing.
        /// Mirrors ProconEventsUid from the legacy LayerClient.
        /// </summary>
        public string ProconEventsUid { get; set; } = string.Empty;

        /// <summary>
        /// UTC timestamp of the last activity on this connection, used for
        /// idle-timeout detection (replaces the old Poke mechanism).
        /// </summary>
        public DateTime LastActivityUtc { get; set; } = DateTime.UtcNow;

        public LayerHubClient(string connectionId)
        {
            ConnectionId = connectionId ?? throw new ArgumentNullException(nameof(connectionId));
        }

        // -----------------------------------------------------------------
        //  Privilege helpers — mirror the checks scattered through the old
        //  LayerClient dispatch methods so callers get a single, clear API.
        // -----------------------------------------------------------------

        /// <summary>
        /// Returns true when the client is logged in AND holds the CanLogin flag.
        /// </summary>
        public bool HasLoginPrivilege => IsLoggedIn && Privileges.CanLogin;

        /// <summary>
        /// Checks whether the client holds a specific privilege flag.
        /// </summary>
        public bool HasPrivilege(Func<CPrivileges, bool> check)
        {
            if (!IsLoggedIn) return false;
            return check(Privileges);
        }

        /// <summary>
        /// Verifies the client is logged in. Returns the appropriate error
        /// status string if not, or null when the check passes.
        /// </summary>
        public string RequireLogin()
        {
            return IsLoggedIn ? null : "LogInRequired";
        }

        /// <summary>
        /// Verifies the client is logged in AND passes a privilege check.
        /// Returns the appropriate error status string if either condition
        /// fails, or null when the check passes.
        /// </summary>
        public string RequirePrivilege(Func<CPrivileges, bool> check)
        {
            if (!IsLoggedIn) return "LogInRequired";
            return check(Privileges) ? null : "InsufficientPrivileges";
        }

        /// <summary>
        /// Bumps the last-activity timestamp to now. Call on every inbound request.
        /// </summary>
        public void Touch()
        {
            LastActivityUtc = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Thread-safe registry of all currently connected LayerHub clients,
    /// keyed by SignalR ConnectionId.
    /// </summary>
    public class LayerHubClientRegistry
    {
        private readonly ConcurrentDictionary<string, LayerHubClient> _clients = new();

        public LayerHubClient GetOrAdd(string connectionId)
        {
            return _clients.GetOrAdd(connectionId, id => new LayerHubClient(id));
        }

        public bool TryGet(string connectionId, out LayerHubClient client)
        {
            return _clients.TryGetValue(connectionId, out client);
        }

        public bool TryRemove(string connectionId, out LayerHubClient client)
        {
            return _clients.TryRemove(connectionId, out client);
        }

        /// <summary>
        /// Returns a snapshot of all currently tracked clients.
        /// </summary>
        public LayerHubClient[] GetAll()
        {
            return _clients.Values.ToArray();
        }

        /// <summary>
        /// Returns usernames of all logged-in clients (deduplicated).
        /// </summary>
        public string[] GetLoggedInUsernames()
        {
            var names = new System.Collections.Generic.HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var client in _clients.Values)
            {
                if (client.IsLoggedIn && !string.IsNullOrEmpty(client.Username))
                {
                    names.Add(client.Username);
                }
            }
            return names.ToArray();
        }
    }
}
