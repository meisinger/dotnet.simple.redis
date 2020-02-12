namespace Simple.Redis
{
    public interface IRedisConnectionFactory
    {
       bool IsEnabled { get; }
       IRedisConnection Open();
    }
}