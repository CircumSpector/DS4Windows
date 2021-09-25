using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using DS4Windows;
using DS4WinWPF.DS4Control.Profiles.Legacy;
using ExtendedXmlSerializer;
using ExtendedXmlSerializer.Configuration;

namespace DS4WinWPF.DS4Control
{
    public sealed class OutputSlotPersist
    {
        private static readonly Lazy<OutputSlotPersist> LazyInstance = new(() => new OutputSlotPersist());

        private OutputSlotPersist()
        {
        }

        public static OutputSlotPersist Instance => LazyInstance.Value;

        private static async Task<IExtendedXmlSerializer> GetOutputSlotsSerializerAsync()
        {
            return await Task.Run(GetOutputSlotsSerializer);
        }

        private static IExtendedXmlSerializer GetOutputSlotsSerializer()
        {
            return new ConfigurationContainer()
                .UseOptimizedNamespaces()
                .EnableImplicitTyping(typeof(OutputSlots), typeof(Slot))
                .Type<Slot>().EnableReferences(c => c.Idx)
                .EnableMemberExceptionHandling()
                .Create();
        }

        [ConfigurationSystemComponent]
        public async Task<bool> ReadConfig(OutputSlotManager slotManager)
        {
            var outputPath = Path.Combine(Global.RuntimeAppDataPath, Constants.OutputSlotsFileName);

            if (!File.Exists(outputPath))
                return false;

            OutputSlots settings;

            await using (var stream = File.OpenRead(outputPath))
            {
                var serializer = await GetOutputSlotsSerializerAsync();

                settings = await Task.Run(() =>
                {
                    try
                    {
                        return serializer.Deserialize<OutputSlots>(stream);
                    }
                    catch (InvalidOperationException)
                    {
                        return new OutputSlots();
                    }
                });
            }

            foreach (var slot in settings.Slot)
            {
                slotManager.OutputSlots[slot.Idx].CurrentReserveStatus = OutSlotDevice.ReserveStatus.Permanent;
                slotManager.OutputSlots[slot.Idx].PermanentType = slot.DeviceType;
            }

            return true;
        }

        [ConfigurationSystemComponent]
        public bool WriteConfig(OutputSlotManager slotManager)
        {
            var result = false;

            var serializer = GetOutputSlotsSerializer();

            var settings = new OutputSlots
            {
                AppVersion = Global.ExecutableProductVersion,
                Slot = slotManager.OutputSlots
                    .Where(s => s.CurrentReserveStatus == OutSlotDevice.ReserveStatus.Permanent)
                    .Select(s => new Slot
                    {
                        Idx = slotManager.OutputSlots.IndexOf(s),
                        DeviceType = s.PermanentType
                    }).ToList()
            };

            var document = serializer.Serialize(new XmlWriterSettings { Indent = true }, settings);

            var outputPath = Path.Combine(Global.RuntimeAppDataPath, Constants.OutputSlotsFileName);

            try
            {
                File.WriteAllText(outputPath, document);
                result = true;
            }
            catch (UnauthorizedAccessException)
            {
                result = false;
            }

            return result;
        }
    }
}