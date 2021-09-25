using DS4Windows;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using DS4WinWPF.DS4Control.Profiles.Legacy;
using ExtendedXmlSerializer;
using ExtendedXmlSerializer.Configuration;

namespace DS4WinWPF.DS4Control
{
    public static class OutputSlotPersist
    {
        private static async Task<IExtendedXmlSerializer> GetOutputSlotsSerializerAsync()
        {
            return await Task.Run(GetOutputSlotsSerializer);
        }

        private static IExtendedXmlSerializer GetOutputSlotsSerializer()
        {
            return new ConfigurationContainer()
                .UseOptimizedNamespaces()
                .EnableImplicitTyping(typeof(OutputSlots))
                .Type<Slot>().EnableReferences(c => c.Idx)
                .Create();
        }

        [ConfigurationSystemComponent]
        public static async Task<bool> ReadConfig(OutputSlotManager slotManager)
        {
            bool result = false;
            string output_path = Path.Combine(Global.RuntimeAppDataPath, Constants.OutputSlotsFileName);
            if (File.Exists(output_path))
            {
                OutputSlots settings;

                await using (var stream = File.OpenRead(output_path))
                {
                    var serializer = await GetOutputSlotsSerializerAsync();

                    settings = await Task.Run(() => serializer.Deserialize<OutputSlots>(stream));
                }

                foreach (var slot in settings.Slots)
                {
                    slotManager.OutputSlots[slot.Idx].PermanentType = slot.DeviceType;
                }

                result = true;
            }

            return result;
        }

        [ConfigurationSystemComponent]
        public static async Task<bool> WriteConfig(OutputSlotManager slotManager)
        {
            bool result = false;

            var serializer = await GetOutputSlotsSerializerAsync();

            var settings = new OutputSlots()
            {
                AppVersion = Global.ExecutableProductVersion
            };
            /*
            settings.Slots = slotManager.OutputSlots
                .Where(s => s.CurrentReserveStatus == OutSlotDevice.ReserveStatus.Permanent)
                .Select(s => new Slot()
                {
                    Idx = slotManager.OutputSlots
                });
            
            var document = await Task.Run(() =>
                serializer.Serialize(new XmlWriterSettings { Indent = true }, profileObject));

            var betaPath = Path.Combine(
                RuntimeAppDataPath,
                Constants.ProfilesSubDirectory,
                $"{proName}-BETA{XML_EXTENSION}"
            );

            await File.WriteAllTextAsync(betaPath, document);
            */


            XmlDocument m_Xdoc = new XmlDocument();
            XmlNode rootNode;
            rootNode = m_Xdoc.CreateXmlDeclaration("1.0", "utf-8", string.Empty);
            m_Xdoc.AppendChild(rootNode);

            rootNode = m_Xdoc.CreateComment(string.Format(" Made with DS4Windows version {0} ", Global.ExecutableProductVersion));
            m_Xdoc.AppendChild(rootNode);

            rootNode = m_Xdoc.CreateWhitespace("\r\n");
            m_Xdoc.AppendChild(rootNode);

            XmlElement baseElement = m_Xdoc.CreateElement("OutputSlots", null);
            baseElement.SetAttribute("app_version", Global.ExecutableProductVersion);

            int idx = 0;
            foreach (OutSlotDevice dev in slotManager.OutputSlots)
            {
                if (dev.CurrentReserveStatus == OutSlotDevice.ReserveStatus.Permanent)
                {
                    XmlElement slotElement = m_Xdoc.CreateElement("Slot");
                    slotElement.SetAttribute("idx", idx.ToString());

                    XmlElement propElement;
                    propElement = m_Xdoc.CreateElement("DeviceType");
                    propElement.InnerText = dev.PermanentType.ToString();
                    slotElement.AppendChild(propElement);

                    baseElement.AppendChild(slotElement);
                }

                idx++;
            }

            m_Xdoc.AppendChild(baseElement);

            string output_path = Path.Combine(Global.RuntimeAppDataPath, Constants.OutputSlotsFileName);
            try { m_Xdoc.Save(output_path); result = true; }
            catch (UnauthorizedAccessException) { result = false; }

            return result;
        }
    }
}