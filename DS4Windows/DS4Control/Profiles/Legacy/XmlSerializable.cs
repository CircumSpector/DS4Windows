using System;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using ExtendedXmlSerializer;

namespace DS4WinWPF.DS4Control.Profiles.Legacy
{
    /// <summary>
    ///     Adds XML (de-)serialization helper methods to <see cref="T"/>.
    /// </summary>
    /// <typeparam name="T">The type to (de-)serialize.</typeparam>
    public abstract class XmlSerializable<T>
    {
        public static async Task<IExtendedXmlSerializer> GetSerializerAsync()
        {
            return await Task.Run(GetSerializer);
        }

        public static IExtendedXmlSerializer GetSerializer()
        {
            throw new NotImplementedException(
                $"Please override {nameof(GetSerializerAsync)} with your own implementation!");
        }

        public async Task SerializeAsync(Stream stream)
        {
            var document = await Task.Run(async () =>
                (await GetSerializerAsync()).Serialize(new XmlWriterSettings { Indent = true }, this));

            await using var writer = new StreamWriter(stream);

            await writer.WriteAsync(document);
        }

        public void Serialize(Stream stream)
        {
            var document = GetSerializer().Serialize(new XmlWriterSettings { Indent = true }, this);

            using var writer = new StreamWriter(stream);

            writer.Write(document);
        }

        public static async Task<T> DeserializeAsync(Stream stream)
        {
            return await Task.Run(async () => (await GetSerializerAsync()).Deserialize<T>(stream));
        }

        public static T Deserialize(Stream stream)
        {
            return GetSerializer().Deserialize<T>(stream);
        }
    }
}