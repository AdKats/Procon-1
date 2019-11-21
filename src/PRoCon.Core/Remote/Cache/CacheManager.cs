using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace PRoCon.Core.Remote.Cache
{
    public class CacheManager : ICacheManager
    {
        public List<IPacketCacheConfiguration> Configurations { get; set; }

        /// <summary>
        /// The cached packets, waiting for a response or completed and yet to expire.
        /// </summary>
        public Dictionary<String, IPacketCache> Cache { get; set; }

        /// <summary>
        /// Lock used whenever we interact with the cache.
        /// </summary>
        public readonly Object CacheLock = new Object();

        /// <summary>
        /// Ticks every second, checking if any cached items have expired and removing them.
        /// </summary>
        public Timer ExpiryCheck { get; set; }

        public CacheManager()
        {
            this.Cache = new Dictionary<String, IPacketCache>();

            this.ExpiryCheck = new Timer(state =>
            {
                lock (this.CacheLock)
                {
                    List<String> keys = (this.Cache.Where(item => item.Value.Expiry < DateTime.Now).Select(item => item.Key)).ToList();

                    keys.ForEach(item => this.Cache.Remove(item));
                }
            }, null, 1000, 1000);
        }

        /// <summary>
        /// Checks if a packet should be cached, if so it will cache on a key
        /// </summary>
        /// <param name="key">The key to cache on</param>
        /// <param name="request">The request being made to the server</param>
        /// <returns>A new cache object if the request was cachable, false otherwise.</returns>
        protected IPacketCache CacheIfApplicable(String key, Packet request)
        {
            IPacketCache cache = null;

            var config = this.Configurations.FirstOrDefault(c => c.Matching.IsMatch(key));

            if (config != null)
            {
                cache = new PacketCache()
                {
                    Request = request,
                    Expiry = DateTime.Now.Add(config.Ttl)
                };

                this.Cache.Add(key, cache);
            }

            return cache;
        }

        public IPacketCache Request(Packet request)
        {
            IPacketCache cache = null;

            if (request.IsResponse == false)
            {
                var key = request.ToString();

                lock (this.CacheLock)
                {
                    // Have we got it cached and is it valid?
                    if (this.Cache.ContainsKey(key) == true)
                    {
                        if (this.Cache[key].Response != null)
                        {
                            // Yes, we do. Return this.
                            cache = this.Cache[key];
                        }
                        // else return null
                    }
                    // No, check if we should cache it.
                    else
                    {
                        this.CacheIfApplicable(key, request);
                    }
                }
            }

            return cache;
        }

        public void Response(Packet response)
        {
            if (response.IsResponse == true)
            {
                lock (this.CacheLock)
                {
                    IPacketCache cache = this.Cache.Where(item => item.Value.Request.SequenceNumber == response.SequenceNumber).Select(item => item.Value).FirstOrDefault();

                    if (cache != null)
                    {
                        cache.Response = response;
                    }
                }
            }
        }
    }
}
