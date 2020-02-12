using System;

namespace Simple.Redis
{
    public interface IRedisCacheManager
    {
        T Cache<T>(IRedisCommand<T> command, Func<T> miss);
        T Execute<T>(IRedisGet<T> command) where T : class;
        bool TryExecute<T>(IRedisGet<T> command, out T item);
        void Execute<T>(IRedisSet<T> command, T item);
        void Execute(IRedisCommand command);
    }
}