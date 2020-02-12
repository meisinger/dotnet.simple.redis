using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace Simple.Redis
{
    internal static class Serializer
    {
        private static readonly JsonSerializerSettings settings
            = new JsonSerializerSettings
            {
                Formatting = Formatting.None,
                DateFormatHandling = DateFormatHandling.IsoDateFormat,
                MissingMemberHandling = MissingMemberHandling.Ignore,
                NullValueHandling = NullValueHandling.Ignore,
                TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
                TypeNameHandling = TypeNameHandling.None
            };

        internal static T Deserialize<T>(string content) =>
            JsonConvert.DeserializeObject<T>(content);

        internal static T Deserialize<T>(byte[] bytes) =>
            Deserialize<T>(Encoding.UTF8.GetString(bytes));

        internal static TType DeserializeType<TType>(string content, TType anonymousType) =>
            JsonConvert.DeserializeAnonymousType(content, anonymousType);

        internal static TType DeserializeType<TType>(byte[] bytes, TType anonymousType) =>
            DeserializeType(Encoding.UTF8.GetString(bytes), anonymousType);

        internal static byte[] SerializeToBytes<T>(T item)
        {
            using (var stream = new MemoryStream())
            using (var writer = new StreamWriter(stream))
            using (var json_writer = new JsonTextWriter(writer))
            {
                var serializer = JsonSerializer.Create(settings);
                serializer.Serialize(json_writer, item);

                writer.Flush();
                return stream.ToArray();
            }
        }

        internal static string SerializeToString<T>(T item)
        {
            using (var writer = new StringWriter())
            using (var json_writer = new JsonTextWriter(writer))
            {
                var serializer = JsonSerializer.Create(settings);
                serializer.Serialize(json_writer, item);

                writer.Flush();
                return writer.ToString();
            }
        }
    }
}