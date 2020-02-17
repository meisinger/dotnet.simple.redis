using System;

namespace Simple.Redis
{
    public class RedisResponseEntry
    {
        private readonly string value;
        private readonly int index;
        private readonly bool nil;

        public int Index => index;
        public bool IsNil => nil;
        public string Value => value;

        private RedisResponseEntry(int index, string value, bool nil)
        {
            this.index = index;
            this.value = value;
            this.nil = nil;
        }

        public string AsString() => value;

        public T As<T>()
        {
            if (nil || string.IsNullOrWhiteSpace(value))
                throw new InvalidCastException();

            return Serializer.Deserialize<T>(value);
        }

        public bool AsBoolean()
        {
            var result = AsInteger();
            if (result != 0 && result != 1)
                throw new InvalidCastException();

            return (result == 1);
        }

        public short AsShort()
        {
            if (short.TryParse(value, out short result))
                return result;

            throw new InvalidCastException();
        }

        public int AsInteger()
        {
            if (int.TryParse(value, out int result))
                return result;
            
            throw new InvalidCastException();
        }

        public long AsLong()
        {
            if (long.TryParse(value, out long result))
                return result;

            throw new InvalidCastException();
        }

        public decimal AsDecimal()
        {
            if (decimal.TryParse(value, out decimal result))
                return result;

            throw new InvalidCastException();
        }

        public float AsFloat()
        {
            if (float.TryParse(value, out float result))
                return result;

            throw new InvalidCastException();
        }

        public void Map(Action<bool> action) => action(AsBoolean());
        public void Map(Action<short> action) => action(AsShort());
        public void Map(Action<int> action) => action(AsInteger());
        public void Map(Action<long> action) => action(AsLong());
        public void Map(Action<decimal> action) => action(AsDecimal());
        public void Map(Action<float> action) => action(AsFloat());
        public void Map(Action<string> action) => action(AsString());

        internal static RedisResponseEntry Nil(int index) => 
            new RedisResponseEntry(index, string.Empty, true);

        internal static RedisResponseEntry Entry(int index, string value) =>
            new RedisResponseEntry(index, value, false);
    }
}