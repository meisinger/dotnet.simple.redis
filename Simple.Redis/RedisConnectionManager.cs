using System;
using System.Collections.Generic;
using System.Linq;

namespace Simple.Redis
{
    internal static class RedisConnectionManager
    {
        private static readonly Dictionary<string, RedisConnectionPool> dictionary = 
            new Dictionary<string, RedisConnectionPool>();

        internal static RedisConnectionPool IdentifyConnectionPool(string host, int port, int size, TimeSpan timeout)
        {
            lock (dictionary)
            {
                var key = $"{host}:{port}";
                if (!dictionary.TryGetValue(key, out RedisConnectionPool pool))
                {
                    pool = new RedisConnectionPool(host, port, size, timeout);
                    dictionary[key] = pool;
                }

                return pool;
            }
        }

        internal static void DisposeConnectionPool()
        {
            lock (dictionary)
            {
                var pools = dictionary.Values.ToArray();
                for (int index = 0; index < pools.Length; index++)
                    pools[index].Dispose();
            }
        }
    }
}