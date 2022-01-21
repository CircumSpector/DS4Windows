using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DS4Windows.Shared.Configuration.Common.Util
{
    /// <summary>
    ///     Adds JSON (de-)serialization helper methods to <see cref="T" />.
    /// </summary>
    /// <typeparam name="T">The type to (de-)serialize.</typeparam>
    public abstract class JsonSerializable<T>
    {
        public void Serialize(Stream stream)
        {
            var serializer = new JsonSerializer()
            {
                Formatting = Formatting.Indented
            };

            using var sw = new StreamWriter(stream);
            using var jtw = new JsonTextWriter(sw);

            serializer.Serialize(jtw, this);
        }

        public async Task SerializeAsync(Stream stream)
        {
            await Task.Run(() => Serialize(stream));
        }

        public static T Deserialize(Stream stream)
        {
            var serializer = new JsonSerializer()
            {
                Formatting = Formatting.Indented
            };

            using var sr = new StreamReader(stream);
            using var jtr = new JsonTextReader(sr);

            return serializer.Deserialize<T>(jtr);
        }

        public static async Task<T> DeserializeAsync(Stream stream)
        {
            return await Task.Run(() => Deserialize(stream));
        }
    }
}