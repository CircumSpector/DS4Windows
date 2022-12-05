using System.Collections.ObjectModel;

using Vapour.Shared.Devices.HID;

namespace Vapour.Shared.Devices.Services;
/// <summary>
///     Handles currently active devices and occupied slots and takes care that profiles are loaded when required.
/// </summary>
public interface IControllerManagerService
{
    /// <summary>
    ///     Gets the state of compatible input devices currently present.
    /// </summary>
    ReadOnlyObservableCollection<CompatibleHidDeviceSlot> ActiveControllers { get; }

    /// <summary>
    ///     Call when a new <see cref="ICompatibleHidDevice" /> has arrived and is ready to occupy a free slot.
    /// </summary>
    /// <param name="device">The <see cref="ICompatibleHidDevice" /> that arrived.</param>
    /// <returns>Zero-based slot index on success, -1 if no free slot available.</returns>
    int AssignFreeSlotWith(ICompatibleHidDevice device);

    /// <summary>
    ///     Call when a <see cref="ICompatibleHidDevice" /> had departed and its slot can be marked available.
    /// </summary>
    /// <param name="device">The <see cref="ICompatibleHidDevice" /> that departed.</param>
    /// <returns>The zero-based slot index it has previously occupied.</returns>
    int FreeSlotContaining(ICompatibleHidDevice device);

    /// <summary>
    ///     Gets invoked when a slot got occupied.
    /// </summary>
    event Action<CompatibleHidDeviceSlot> ControllerSlotOccupied;

    /// <summary>
    ///     Gets invoked when a slot got freed.
    /// </summary>
    event Action<CompatibleHidDeviceSlot> ControllerSlotFreed;
}