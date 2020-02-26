using System;
using System.IO;
using System.Net.Sockets;

namespace Simple.Redis
{
    public class RedisConnectionFactory : IRedisConnectionFactory
    {
        private readonly string host;
        private readonly string password;
        private readonly int port;
        private readonly int space;
        private readonly int size;
        private readonly TimeSpan timeout;
        private bool enabled;

        public bool IsEnabled => enabled;

        public RedisConnectionFactory(string host)
            : this(true, host, string.Empty, 6379, 0, 100, 2)
        {
        }

        public RedisConnectionFactory(bool enabled, string host, int port)
            : this(enabled, host, string.Empty, port, 0, 100, 2)
        {
        }

        public RedisConnectionFactory(bool enabled, string host, string password, int port)
            : this(enabled, host, password, port, 0, 100, 2)
        {
        }

        public RedisConnectionFactory(bool enabled, string host, int port, int space)
            : this(enabled, host, string.Empty, port, space, 100, 2)
        {
        }

        public RedisConnectionFactory(bool enabled, string host, string password, int port, int space, int size, int timeout)
        {
            this.enabled = enabled;
            this.host = host;
            this.password = password;
            this.port = port;
            this.space = space;
            this.size = size;
            this.timeout = TimeSpan.FromSeconds(timeout);
        }

        public void Dispose()
        {
            if (!enabled)
                return;

            RedisConnectionManager.DisposeConnectionPool();
        }

        public IRedisConnection GetConnection()
        {
            var pool = RedisConnectionManager
                .IdentifyConnectionPool(host, port, size, timeout);

            return pool.GetConnection();
        }

        public IRedisConnection Open()
        {
            if (!enabled)
                return RedisConnection.CreatePassThrough();

            var connection = GetConnection();
            if (connection.InError)
            {
                enabled = false;
                return connection;
            }

            if (!string.IsNullOrWhiteSpace(password))
            {
                var auth = RedisCommand.Create(RedisCommands.AUTH)
                    .AddArgument(password)
                    .Execute(connection);

                if (auth.IsEmpty)
                {
                    connection.Dispose();
                    throw new Exception("Redis Authentication failed.");
                }

                var auth_status = auth[0].AsString();
                if (!auth_status.Equals("OK", StringComparison.Ordinal))
                {
                    connection.Dispose();
                    throw new Exception("Redis Authentication failed.");
                }
            }

            if (space == 0)
                return connection;

            var selected_space = RedisCommand.Create(RedisCommands.SELECT)
                .AddArgument(space)
                .Execute(connection);

            if (selected_space.IsEmpty)
            {
                connection.Dispose();
                throw new Exception($"Redis Keyspace {space} does not exist or is not available.");
            }

            var selected_status = selected_space[0].AsString();
            if (!selected_status.Equals("OK", StringComparison.Ordinal))
            {
                connection.Dispose();
                throw new Exception($"Redis Keyspace {space} does not exist or is not available.");
            }

            return connection;
        }

        public static IRedisConnection CreateConnection(string host, int port)
        {
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
            {
                NoDelay = true,
                SendTimeout = -1
            };

            try
            {
                socket.Connect(host, port);
                if (!socket.Connected)
                {
                    socket.Dispose();
                    throw new IOException();
                }

                var buffer = new BufferedStream(new NetworkStream(socket), 16 * 1024);
                return new RedisConnection(socket, buffer);
            }
            catch (Exception ex)
            {
                return new RedisConnectionInError(ex);
            }
        }

        public static IRedisConnection CreatePassThroughConnection()
        {
            return RedisConnection.CreatePassThrough();
        }
    }
}