using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Simple.Redis
{
    public class RedisResponse : IEnumerator<RedisResponseEntry>, IEnumerable<RedisResponseEntry>
    {
        private readonly RedisResponseEntry[] entries;
        private int currentIndex;

        public RedisResponseEntry this[int index]
        {
            get
            {
                var entry = entries.FirstOrDefault(x => x.Index.Equals(index));
                if (entry == null)
                    throw new IndexOutOfRangeException();
                
                return entry;
            }
        }

        public bool IsEmpty => (entries.Length == 0);
        public int Length => entries.Length;

        RedisResponseEntry IEnumerator<RedisResponseEntry>.Current =>
            entries[currentIndex];
        object IEnumerator.Current =>
            entries[currentIndex];

        internal RedisResponse(RedisResponseEntry[] entries)
        {
            this.entries = entries;
            currentIndex = -1;
        }

        public void Dispose() =>
            currentIndex = -1;

        public void Map(string field, Action<bool> action)
        {
            var index = FindFieldIndex(field);
            if (!index.HasValue)
                return;
            
            entries[index.Value + 1].Map(action);
        }

        public void Map(string field, Action<short> action)
        {
            var index = FindFieldIndex(field);
            if (!index.HasValue)
                return;
            
            entries[index.Value + 1].Map(action);
        }

        public void Map(string field, Action<int> action)
        {
            var index = FindFieldIndex(field);
            if (!index.HasValue)
                return;
            
            entries[index.Value + 1].Map(action);
        }

        public void Map(string field, Action<long> action)
        {
            var index = FindFieldIndex(field);
            if (!index.HasValue)
                return;
            
            entries[index.Value + 1].Map(action);
        }

        public void Map(string field, Action<decimal> action)
        {
            var index = FindFieldIndex(field);
            if (!index.HasValue)
                return;
            
            entries[index.Value + 1].Map(action);
        }

        public void Map(string field, Action<float> action)
        {
            var index = FindFieldIndex(field);
            if (!index.HasValue)
                return;
            
            entries[index.Value + 1].Map(action);
        }

        public void Map(string field, Action<string> action)
        {
            var index = FindFieldIndex(field);
            if (!index.HasValue)
                return;

            entries[index.Value + 1].Map(action);
        }

        IEnumerator<RedisResponseEntry> IEnumerable<RedisResponseEntry>.GetEnumerator() =>
            this;
 
        IEnumerator IEnumerable.GetEnumerator() =>
            this;

        void IEnumerator.Reset() =>
            currentIndex = -1;
        
        bool IEnumerator.MoveNext()
        {
            currentIndex++;
            return currentIndex < entries.Length;
        }

        private int? FindFieldIndex(string field)
        {
            var entry = entries
                .Where(x => !x.IsNil)
                .FirstOrDefault(x => x.Value.Equals(field, StringComparison.Ordinal));
            
            if (entry == null)
                return null;
            
            var index = Array.IndexOf(entries, entry);
            return (index == -1) ? (int?)null : index;
        }

        public static RedisResponse Empty() =>
            new RedisResponse(new RedisResponseEntry[0]);
    }
}