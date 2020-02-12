namespace Simple.Redis
{
    public interface IRedisCommand
    {
        void Execute(IRedisConnection connection);
    }

    public interface IRedisGet<T>
    {
        bool Get(IRedisConnection connection, out T item);
    }

    public interface IRedisSet<in T>
    {
        void Set(IRedisConnection connection, T item);
    }

    public interface IRedisCommand<T> : IRedisGet<T>, IRedisSet<T>
    {
    }
}