using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DS4Windows.Shared.Common.Types;
using DS4Windows.Shared.Devices.Output;
using Nefarius.ViGEm.Client;

namespace DS4Windows.Shared.Devices.Services
{
    public interface IOutputSlotManager
    {
        ViGEmClient Client { get; }

        IList<IOutSlotDevice> OutputSlots { get; }

        bool RunningQueue { get; }

        public delegate void SlotAssignedDelegate(IOutputSlotManager sender,
            int slotNum, IOutSlotDevice outSlotDev);

        public delegate void SlotUnassignedDelegate(IOutputSlotManager sender,
            int slotNum, IOutSlotDevice outSlotDev);

        event SlotAssignedDelegate SlotAssigned;

        event SlotUnassignedDelegate SlotUnassigned;

        event EventHandler ViGEmFailure;

        void Stop(bool immediate = false);

        void ShutDown();

        IOutDevice AllocateController(OutputDeviceType contType);

        void DeferredPlugin(IOutDevice outputDevice, int inIdx, IOutDevice[] outdevs, OutputDeviceType contType);

        void DeferredRemoval(IOutDevice outputDevice, int inIdx,
            IOutDevice[] outdevs, bool immediate = false);

        IOutSlotDevice FindOpenSlot();

        IOutSlotDevice GetOutSlotDevice(IOutDevice outputDevice);

        IOutSlotDevice FindExistUnboundSlotType(OutputDeviceType contType);

        void UnplugRemainingControllers(bool immediate = false);
    }
}
