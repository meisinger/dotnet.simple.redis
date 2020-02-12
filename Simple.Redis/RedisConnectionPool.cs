using System;
using System.Threading;

namespace Simple.Redis
{
    internal class RedisConnectionPool : IDisposable
    {
        private readonly RedisConnection[] connections;
        private readonly string host;
        private readonly int port;
        private readonly TimeSpan timeout;
        private int dispose_counter;

        public RedisConnectionPool(string host, int port)
            : this(host, port, TimeSpan.FromSeconds(2))
        {
        }

        public RedisConnectionPool(string host, int port, TimeSpan timeout)
            : this(host, port, 100, timeout)
        {
        }

        public RedisConnectionPool(string host, int port, int size, TimeSpan timeout)
        {
            this.host = host;
            this.port = port;
            this.timeout = timeout;
            connections = new RedisConnection[size];
        }

        ~RedisConnectionPool() => Dispose();

        public void Dispose()
        {
            Disposing();
            GC.SuppressFinalize(this);
        }

        public void DisposeConnection(RedisConnection connection)
        {
            lock (connections)
            {
                connection.SetConnectionInactive();
                Monitor.PulseAll(connections);
            }
        }

        public IRedisConnection GetConnection()
        {
            lock (connections)
            {
                IRedisConnection instance;
                while ((instance = FindAvailableConnection()) == null)
                {
                    if (Monitor.Wait(connections, timeout))
                    {
                        var exception = new TimeoutException("Unable to establish connection");
                        instance = new RedisConnectionInError(exception);
                    }
                }

                if (instance.InError)
                    return instance;

                var connection = instance as RedisConnection;
                connection.SetConnectionActive();

                return connection;
            }
        }

        public IRedisConnection FindAvailableConnection()
        {
            for (int index = 0; index < connections.Length; index++)
            {
                var connection = connections[index];
                if (connection != null && connection.IsOpen && !connection.IsActive && !connection.InError)
                    return connection;
                
                if (connection == null || !connection.IsOpen)
                {
                    if (connection != null)
                        connection.DisposeConnection();

                    var connection_instance = RedisConnectionFactory.CreateConnection(host, port);
                    if (connection_instance.InError)
                        return connection_instance;

                    var instance = connection_instance as RedisConnection;
                    instance.SetConnectionPool(this);

                    connections[index] = instance;
                    return instance;
                }
            }

            return null;
        }

        private void Disposing()
        {
            if (Interlocked.Increment(ref dispose_counter) > 1)
                return;

            for (int index = 0; index < connections.Length; index++)
            {
                var connection = connections[index];
                if (connection == null)
                    continue;

                connection.DisposeConnection();
                connections[index] = null;
            }
        }
    }
}