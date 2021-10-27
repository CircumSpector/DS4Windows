using System.IO;
using Newtonsoft.Json;

namespace DS4WinWPF.DS4Control.Profiles.Schema
{
    /// <summary>
    ///     Adds JSON (de-)serialization helper methods to <see cref="T" />.
    /// </summary>
    /// <typeparam name="T">The type to (de-)serialize.</typeparam>
    public abstract class JsonSerializable<T>
    {
        public void Serialize(Stream stream)
        {
            var serializer = new JsonSerializer();

            using var sw = new StreamWriter(stream);
            using var jtw = new JsonTextWriter(sw);

            serializer.Serialize(jtw, this);
        }

        public static T Deserialize(Stream stream)
        {
            var serializer = new JsonSerializer();

            using var sr = new StreamReader(stream);
            using var jtr = new JsonTextReader(sr);

            return serializer.Deserialize<T>(jtr);
        }
    }
}