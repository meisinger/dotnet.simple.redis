using System;
using System.IO;
using System.Net.Sockets;

namespace Simple.Redis
{
    public class RedisConnection : IRedisConnection
    {
        private readonly Socket socket;
        private readonly BufferedStream buffer;

        private bool active;
        private bool pass_through;
        private RedisConnectionPool connection_pool;

        public bool InError => false;
        public bool IsActive => active;
        public bool IsOpen => socket.Connected;
        public Stream RedisChannel => buffer;
        public Exception Exception => null;

        public bool IsPassThrough
        {
            get => pass_through;
            internal set => pass_through = value;
        }

        public RedisConnection(Socket socket, BufferedStream buffer)
            : this(false)
        {
            this.socket = socket;
            this.buffer = buffer;
        }

        public RedisConnection(bool pass_through) => 
            this.pass_through = pass_through;

        public void Dispose()
        {
            if (connection_pool != null)
                connection_pool.DisposeConnection(this);
            else
                DisposeConnection();
        }

        internal void DisposeConnection()
        {
            buffer.Close();
            socket.Shutdown(SocketShutdown.Both);
            socket.Close();
        }

        internal void SetConnectionActive() => 
            active = true;
        internal void SetConnectionInactive() => 
            active = false;

        internal void SetConnectionPool(RedisConnectionPool pool) =>
            connection_pool = pool;

        public static IRedisConnection CreatePassThrough() =>
            new RedisConnection(true);
    }

    public class RedisConnectionInError : IRedisConnection
    {
        private readonly Exception exception;

        public bool IsPassThrough => true;
        public bool InError => true;
        public bool IsActive => false;
        public bool IsOpen => false;
        public Stream RedisChannel => throw new IOException();
        public Exception Exception => exception;

        public RedisConnectionInError()
            : this(null)
        {
        }

        public RedisConnectionInError(Exception exception)
        {
            this.exception = exception;
        }

        public void Dispose()
        {
        }
    }
}