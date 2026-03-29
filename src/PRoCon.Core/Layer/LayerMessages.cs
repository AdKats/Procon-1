using System;

namespace PRoCon.Core.Layer
{
    /// <summary>
    /// Represents a command request sent from a connected admin client to the layer.
    /// Replaces the Frostbite binary Packet for request semantics.
    /// </summary>
    public class LayerRequest
    {
        /// <summary>
        /// The command name (e.g. "admin.say", "serverInfo", "banList.add").
        /// Mirrors the first word in the old Frostbite Packet.
        /// </summary>
        public string Command { get; set; } = string.Empty;

        /// <summary>
        /// Command arguments, corresponding to Words[1..n] in the old Packet.
        /// </summary>
        public string[] Args { get; set; } = Array.Empty<string>();

        /// <summary>
        /// Client-generated correlation identifier so the caller can match
        /// a <see cref="LayerResponse"/> back to its originating request.
        /// </summary>
        public string Id { get; set; } = string.Empty;
    }

    /// <summary>
    /// Represents a response sent from the layer back to the requesting admin client.
    /// Replaces the Frostbite binary Packet for response semantics.
    /// </summary>
    public class LayerResponse
    {
        /// <summary>
        /// Correlation identifier matching the original <see cref="LayerRequest.Id"/>.
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Status string compatible with the legacy protocol responses:
        /// "OK", "InvalidPassword", "InsufficientPrivileges", "InvalidArguments",
        /// "UnknownCommand", "LogInRequired", etc.
        /// </summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// Response payload. The shape depends on the command — for example,
        /// serverInfo returns server details, listPlayers returns player arrays, etc.
        /// </summary>
        public object[] Data { get; set; } = Array.Empty<object>();
    }

    /// <summary>
    /// Represents a server-initiated event pushed to connected admin clients.
    /// Replaces the old eventsEnabled / event Packet dispatch.
    /// </summary>
    public class LayerEvent
    {
        /// <summary>
        /// The event name (e.g. "player.onChat", "player.onKill", "server.onRoundOver").
        /// </summary>
        public string EventName { get; set; } = string.Empty;

        /// <summary>
        /// Event-specific payload data.
        /// </summary>
        public object Data { get; set; }
    }
}
