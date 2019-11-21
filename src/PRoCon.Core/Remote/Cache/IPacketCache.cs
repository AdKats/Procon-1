using System;

namespace PRoCon.Core.Remote.Cache
{
    public interface IPacketCache
    {
        /// <summary>
        /// When this cache should expire
        /// </summary>
        DateTime Expiry { get; set; }

        /// <summary>
        /// The packet that was originally sent
        /// </summary>
        Packet Request { get; set; }

        /// <summary>
        /// The response to the packet that was sent
        /// </summary>
        Packet Response { get; set; }
    }
}
