using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Nefarius.ViGEm.Client;

namespace DS4WinWPF.DS4Control.IoC.Services
{
    public sealed class OutputSlotService
    {
        private readonly IList<OutSlotDevice> outputSlots =
            new List<OutSlotDevice>(Enumerable.Range(0, 8).Select(i => new OutSlotDevice(i)));

        public OutputSlotService(ViGEmClient client)
        {
            Emulator = client;
        }

        public IReadOnlyCollection<OutSlotDevice> OutputSlots => outputSlots.ToImmutableList();

        public ViGEmClient Emulator { get; }
    }
}