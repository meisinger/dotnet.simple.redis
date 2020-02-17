using System;
using System.IO;
using System.Text;

namespace Simple.Redis
{
    internal class RedisResponseParser
    {
        private readonly StringBuilder builder;
        private readonly Stream stream;

        internal RedisResponseParser(Stream stream)
        {
            this.stream = stream;
            builder = new StringBuilder();
        }

        internal RedisResponse Parse()
        {
            var line = ParseLine(out char marker);
            if (marker.Equals('-'))
                throw new Exception(line);
            
            var buffer = new RedisResponseBuffer();
            switch (marker)
            {
                case '$':
                    ParseBulkReply(buffer, line);
                    break;
                case '*':
                    ParseMultiBulkReply(buffer, line);
                    break;
                case ':':
                case '+':
                    buffer.Push(line);
                    break;
                default:
                    throw new InvalidDataException($"Unknown Response Marker received. Response Marker \"{marker}\" is not supported.");
            }
            
            return buffer.ToResponse();
        }

        private void ParseBulkReply(RedisResponseBuffer buffer, string line)
        {
            if (!int.TryParse(line, out int length))
                throw new InvalidDataException($"Expected Bulk Reply to indicate stream length. Received \"{line}\"");

            if (length.Equals(-1))
                buffer.PushEmpty();
            else
                buffer.Push(ParseLine(length));
        }

        private void ParseMultiBulkReply(RedisResponseBuffer buffer, string line)
        {
            if (!int.TryParse(line, out int count))
                throw new InvalidDataException($"Expected Multi Bulk Reply to indicate result count. Received \"{line}\"");

            for (int index = 0; index < count; index++)
            {
                var multiline = ParseLine(out char marker);
                if (!marker.Equals(':') && !marker.Equals('$') && !marker.Equals('*'))
                {
                    buffer.PushEmpty(index);
                    continue;
                }

                if (marker.Equals(':'))
                {
                    buffer.Push(index, multiline);
                    continue;
                }

                if (marker.Equals('*'))
                {
                    ParseMultiBulkReply(buffer, multiline);
                    continue;
                }

                if (!int.TryParse(multiline, out int length) || length.Equals(-1))
                    buffer.PushEmpty(index);
                else
                    buffer.Push(index, ParseLine(length));
            }
        }

        private string ParseLine()
        {
            builder.Length = 0;

            int character;
            while ((character = stream.ReadByte()) != -1)
            {
                if (character.Equals(13))
                    continue;
                if (character.Equals(10))
                    break;
                builder.Append((char)character);
            }

            return builder.ToString();
        }

        private string ParseLine(int length)
        {
            var bytes = new byte[length];
            var parsed = 0;

            do
            {
                var read = stream.Read(bytes, parsed, (length - parsed));
                if (read < 1)
                    throw new EndOfStreamException();
                
                parsed += read;
            } while (parsed < length);

            if (stream.ReadByte() != 13 || stream.ReadByte() != 10)
                throw new InvalidOperationException();

            return Encoding.UTF8.GetString(bytes);
        }

        private string ParseLine(out char marker)
        {
            var line = ParseLine();
            if (string.IsNullOrEmpty(line))
            {
                marker = '?';
                return string.Empty;
            }

            marker = line[0];
            return line.Substring(1);
        }
        
    }
}