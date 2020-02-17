using System.Collections.Generic;

namespace Simple.Redis
{
    internal class RedisResponseBuffer
    {
        private readonly List<RedisResponseEntry> entries;

        internal RedisResponseBuffer() =>
            entries = new List<RedisResponseEntry>();

        internal void Push(string value) =>
            Push(0, value);

        internal void Push(int index, string value) =>
            entries.Add(RedisResponseEntry.Entry(index, value));

        internal void PushEmpty() =>
            PushEmpty(0);

        internal void PushEmpty(int index) =>
            entries.Add(RedisResponseEntry.Nil(index));
        
        internal RedisResponse ToResponse() =>
            new RedisResponse(entries.ToArray());
    }
}