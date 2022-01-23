using System.Collections.ObjectModel;
using System.Linq;
using DS4Windows.Shared.Common.Core;
using DS4Windows.Shared.Devices.HID;
using JetBrains.Annotations;

namespace DS4Windows.Shared.Devices.Services
{
    public class CompatibleHidDeviceSlot
    {
        public CompatibleHidDeviceSlot(int slotIndex)
        {
            SlotIndex = slotIndex;
        }

        public int SlotIndex { get; init; }

        public bool IsOccupied { get; set; }

        [CanBeNull] public CompatibleHidDevice Device { get; set; }
    }

    public interface IControllerManagerService
    {
        /// <summary>
        ///     Gets the state of compatible input devices currently present.
        /// </summary>
        ReadOnlyObservableCollection<CompatibleHidDeviceSlot> ActiveControllers { get; }

        /// <summary>
        ///     Call when a new <see cref="CompatibleHidDevice"/> has arrived and is ready to occupy a free slot.
        /// </summary>
        /// <param name="device">The <see cref="CompatibleHidDevice"/> that arrived.</param>
        /// <returns>Zero-based slot index on success, -1 if no free slot available.</returns>
        int AssignFreeSlotWith(CompatibleHidDevice device);

        /// <summary>
        ///     Call when a <see cref="CompatibleHidDevice"/> had departed and its slot can be marked available.
        /// </summary>
        /// <param name="device">The <see cref="CompatibleHidDevice"/> that departed.</param>
        /// <returns>The slot index it has previously occupied.</returns>
        int FreeSlotContaining(CompatibleHidDevice device);
    }

    public class ControllerManagerService : IControllerManagerService
    {
        private readonly ObservableCollection<CompatibleHidDeviceSlot> activeControllers;

        public ControllerManagerService()
        {
            activeControllers = new ObservableCollection<CompatibleHidDeviceSlot>(Enumerable
                .Range(0, Constants.MaxControllers)
                .Select(i => new CompatibleHidDeviceSlot(i))
            );

            ActiveControllers = new ReadOnlyObservableCollection<CompatibleHidDeviceSlot>(activeControllers);
        }

        /// <inheritdoc />
        [ItemCanBeNull] public ReadOnlyObservableCollection<CompatibleHidDeviceSlot> ActiveControllers { get; }

        /// <inheritdoc />
        public int AssignFreeSlotWith(CompatibleHidDevice device)
        {
            var slot = activeControllers.FirstOrDefault(s => !s.IsOccupied);

            //
            // No free slot
            // 
            if (slot is null)
                return -1;

            slot.Device = device;
            slot.IsOccupied = true;

            return slot.SlotIndex;
        }

        /// <inheritdoc />
        public int FreeSlotContaining(CompatibleHidDevice device)
        {
            var slot = activeControllers.First(s => Equals(s.Device, device));

            slot.IsOccupied = false;
            slot.Device = null;

            return slot.SlotIndex;
        }
    }
}