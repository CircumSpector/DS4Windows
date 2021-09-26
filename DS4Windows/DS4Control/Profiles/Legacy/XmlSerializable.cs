using System;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using ExtendedXmlSerializer;

namespace DS4WinWPF.DS4Control.Profiles.Legacy
{
    public abstract class XmlSerializable<T>
    {
        public static Task<IExtendedXmlSerializer> GetSerializerAsync()
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

        public static async Task<T> DeserializeAsync(Stream stream)
        {
            return await Task.Run(async () => (await GetSerializerAsync()).Deserialize<T>(stream));
        }
    }
}