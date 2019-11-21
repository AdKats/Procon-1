using System;

namespace PRoCon.Core.Remote.Cache
{
    public class PacketCache : IPacketCache
    {
        public DateTime Expiry { get; set; }

        public Packet Request { get; set; }

        public Packet Response { get; set; }
    }
}
