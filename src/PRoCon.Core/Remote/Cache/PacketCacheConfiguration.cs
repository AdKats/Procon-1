using System;
using System.Text.RegularExpressions;

namespace PRoCon.Core.Remote.Cache
{
    public class PacketCacheConfiguration : IPacketCacheConfiguration
    {
        /// <summary>
        /// How long this packet should live before being destroyed
        /// </summary>
        public TimeSpan Ttl { get; set; }

        /// <summary>
        /// The words that must match the request packet.
        /// </summary>
        public Regex Matching { get; set; }
    }
}
