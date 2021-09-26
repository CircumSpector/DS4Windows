using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DS4Windows;
using DS4WinWPF.DS4Control.Profiles.Legacy;

namespace DS4WinWPF.DS4Control
{
    public sealed class OutputSlotPersist
    {
        private static readonly Lazy<OutputSlotPersist> LazyInstance = new(() => new OutputSlotPersist());

        private OutputSlotPersist()
        {
        }

        public static OutputSlotPersist Instance => LazyInstance.Value;

        [ConfigurationSystemComponent]
        public async Task<bool> ReadConfig(OutputSlotManager slotManager)
        {
            var outputPath = Path.Combine(Global.RuntimeAppDataPath, Constants.OutputSlotsFileName);

            if (!File.Exists(outputPath))
                return false;

            OutputSlots settings;

            await using (var stream = File.OpenRead(outputPath))
            {
                settings = await Task.Run(() =>
                {
                    try
                    {
                        return OutputSlots.Deserialize(stream);
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
            
            var outputPath = Path.Combine(Global.RuntimeAppDataPath, Constants.OutputSlotsFileName);

            try
            {
                using var stream = File.OpenWrite(outputPath);

                settings.Serialize(stream);
                
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