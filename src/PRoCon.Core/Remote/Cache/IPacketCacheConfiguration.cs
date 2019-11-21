using System;
using System.Text.RegularExpressions;

namespace PRoCon.Core.Remote.Cache
{
    public interface IPacketCacheConfiguration
    {
        /// <summary>
        /// How long this packet should live before being destroyed
        /// </summary>
        TimeSpan Ttl { get; set; }

        /// <summary>
        /// The words that must match the request packet.
        /// </summary>
        Regex Matching { get; set; }
    }
}
