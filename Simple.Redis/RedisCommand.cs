using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace Simple.Redis
{
    public class RedisCommand
    {
        private readonly string command;
        private readonly List<byte[]> collection;

        public RedisCommand(string command, byte[][] arguments)
        {
            this.command = command;
            collection = new List<byte[]>(arguments);
        }

        public RedisCommand AddArgument(byte[] argument)
        {
            collection.Add(argument);
            return this;
        }

        public RedisCommand AddArgument(string argument) =>
            AddArgument(Encoding.UTF8.GetBytes(argument));

        public RedisCommand AddArgument(short argument) =>
            AddArgument(argument.ToString(CultureInfo.InvariantCulture));

        public RedisCommand AddArgument(int argument) =>
            AddArgument(argument.ToString(CultureInfo.InvariantCulture));

        public RedisCommand AddArgument(long argument) =>
            AddArgument(argument.ToString(CultureInfo.InvariantCulture));

        public RedisCommand AddArgument(decimal argument) =>
            AddArgument(argument.ToString(CultureInfo.InvariantCulture));

        public RedisCommand AddArgument(float argument) =>
            AddArgument(argument.ToString(CultureInfo.InvariantCulture));

        public RedisCommand AddArgument<T>(T argument)
            where T : class =>
            AddArgument(Serializer.SerializeToBytes(argument));

        public void Execute(IRedisConnection connection)
        {
            if (connection.IsPassThrough)
                throw new InvalidOperationException($"Connection is marked as Pass-Through and will not execute command \"{command}\".");
            if (!connection.IsOpen)
                throw new InvalidOperationException($"Connection is not open or has been closed. Command \"{command}\" will not be executed.");

            var bytes = GenerateCommand(collection.ToArray());
            var channel = connection.RedisChannel;

            try
            {
                channel.Write(bytes, 0, bytes.Length);
                channel.Flush();


            }
            catch (Exception ex)
            {
                connection.Dispose();
                throw ex;
            }
        }

        public static RedisCommand Create(string command)
        {
            if (!RedisCommandMapping.IsValidCommand(command))
                throw new InvalidOperationException($"Command \"{command}\" is not a valid Redis Command or is not supported.");

            return RedisCommandMapping.GenerateCommand(command);
        }

        private static byte[] GenerateCommand(byte[][] arguments)
        {
            using (var stream = new MemoryStream())
            {
                var header = CreateMarker('*', arguments.Length);
                stream.Write(header, 0, header.Length);
                stream.Write(new byte[] { 13, 10 }, 0, 2);

                foreach (var argument in arguments)
                {
                    var payload = CreateMarker('$', argument.Length);
                    stream.Write(payload, 0, payload.Length);
                    stream.Write(new byte[] { 13, 10 }, 0, 2);
                    stream.Write(argument, 0, argument.Length);
                    stream.Write(new byte[] { 13, 10 }, 0, 2);
                }

                return stream.ToArray();
            }
        }

        private static byte[] CreateMarker(char marker, int count)
        {
            var value = count.ToString(CultureInfo.InvariantCulture);
            var length = value.Length;
            var bytes = new byte[length + 1];

            bytes[0] = (byte)marker;
            for (int index = 0; index < length; index++)
                bytes[index + 1] = (byte)value[index];

            return bytes;
        }
    }
}