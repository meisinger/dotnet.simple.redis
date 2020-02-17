using System;

namespace Simple.Redis
{
    public class RedisCacheManager : IRedisCacheManager
    {
        private readonly IRedisConnectionFactory factory;

        public RedisCacheManager(IRedisConnectionFactory factory)
        {
            this.factory = factory;
        }

        public T Cache<T>(IRedisCommand<T> command, Func<T> miss)
        {
            if (!factory.IsEnabled)
                return miss();
            
            using (var connection = factory.Open())
            {
                if (connection.IsPassThrough)
                    return miss();

                if (command.Get(connection, out T item))
                    return item;

                item = miss();
                command.Set(connection, item);

                return item;
            }
        }

        public T Execute<T>(IRedisGet<T> command) where T : class
        {
            if (!factory.IsEnabled)
                return null;
            
            using (var connection = factory.Open())
            {
                if (connection.IsPassThrough)
                    return null;

                if (command.Get(connection, out T item))
                    return item;

                return null;
            }
        }

        public void Execute<T>(IRedisSet<T> command, T item)
        {
            if (!factory.IsEnabled)
                return;

            using (var connection = factory.Open())
            {
                if (connection.IsPassThrough)
                    return;

                command.Set(connection, item);
            }
        }

        public void Execute(IRedisCommand command)
        {
            if (!factory.IsEnabled)
                return;

            using (var connection = factory.Open())
            {
                if (connection.IsPassThrough)
                    return;

                command.Execute(connection);
            }
        }

        public bool TryExecute<T>(IRedisGet<T> command, out T item)
        {
            item = default;
            if (!factory.IsEnabled)
                return false;

            using (var connection = factory.Open())
            {
                if (connection.IsPassThrough)
                    return false;
                
                return command.Get(connection, out item);
            }
        }
    }
}