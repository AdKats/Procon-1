using System.Collections.Generic;

namespace PRoCon.Core.Remote.Cache
{
    public interface ICacheManager
    {
        /// <summary>
        /// What packets to capture 
        /// </summary>
        List<IPacketCacheConfiguration> Configurations { get; set; }

        /// <summary>
        /// Cache a requested packet, or pull the response from the cache.
        /// </summary>
        /// <param name="request">The request being made to the server</param>
        /// <returns>The existing cached response, if it exists.</returns>
        IPacketCache Request(Packet request);

        /// <summary>
        /// A response from the server, used to update cache for an object.
        /// </summary>
        /// <param name="response">The response from the server.</param>
        void Response(Packet response);
    }
}
