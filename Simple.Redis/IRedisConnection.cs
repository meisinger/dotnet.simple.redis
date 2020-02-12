using System;
using System.IO;

namespace Simple.Redis
{
    public interface IRedisConnection : IDisposable
    {
       bool InError { get; }
       bool IsPassThrough { get; }
       bool IsActive { get; }
       bool IsOpen { get; }
       Stream RedisChannel { get; }
       Exception Exception { get; }
    }
}