using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using DS4Windows;
using DS4Windows.Shared.Common.Core;
using DS4Windows.Shared.Common.Types;
using Nefarius.ViGEm.Client;

namespace DS4WinWPF.DS4Control.IoC.Services
{
    //
    // TODO: to be continued
    // 
    public sealed class OutputSlotService
    {
        private readonly IList<OutSlotDevice> outputSlots =
            new List<OutSlotDevice>(Enumerable.Range(0, Constants.MaxControllers).Select(i => new OutSlotDevice(i)));

        public OutputSlotService(ViGEmClient client)
        {
            Emulator = client;
        }

        public IReadOnlyCollection<OutSlotDevice> OutputSlots => outputSlots.ToImmutableList();

        public ViGEmClient Emulator { get; }

        public OutputDevice AllocateController(OutputDeviceType contType)
        {
            switch (contType)
            {
                case OutputDeviceType.X360:
                    return new Xbox360OutDevice(Emulator);
                case OutputDeviceType.DS4:
                    return DS4OutDeviceFactory.CreateDS4Device(Emulator, Global.ViGEmBusVersionInfo);
                case OutputDeviceType.None:
                default:
                    break;
            }

            return null;
        }
    }
}