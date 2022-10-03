using System.Collections.ObjectModel;
using Vapour.Shared.Common.Core;
using Vapour.Shared.Devices.HID;
using Vapour.Shared.Devices.Interfaces.HID;
using JetBrains.Annotations;

namespace Vapour.Shared.Devices.Services;

/// <summary>
///     Describes a controller slot and the <see cref="CompatibleHidDevice" /> associated with it.
/// </summary>
public class CompatibleHidDeviceSlot
{
    public CompatibleHidDeviceSlot(int slotIndex)
    {
        SlotIndex = slotIndex;
    }

    /// <summary>
    ///     The zero-based slot index.
    /// </summary>
    public int SlotIndex { get; init; }

    /// <summary>
    ///     Is this slot occupied with a <see cref="Device" />?
    /// </summary>
    public bool IsOccupied { get; internal set; }

    /// <summary>
    ///     The <see cref="CompatibleHidDevice" /> occupying this slot.
    /// </summary>
    [CanBeNull]
    public ICompatibleHidDevice Device { get; internal set; }
}

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
    ///     Call when a new <see cref="CompatibleHidDevice" /> has arrived and is ready to occupy a free slot.
    /// </summary>
    /// <param name="device">The <see cref="CompatibleHidDevice" /> that arrived.</param>
    /// <returns>Zero-based slot index on success, -1 if no free slot available.</returns>
    int AssignFreeSlotWith(ICompatibleHidDevice device);

    /// <summary>
    ///     Call when a <see cref="CompatibleHidDevice" /> had departed and its slot can be marked available.
    /// </summary>
    /// <param name="device">The <see cref="CompatibleHidDevice" /> that departed.</param>
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

/// <summary>
///     Handles currently active devices and occupied slots and takes care that profiles are loaded when required.
/// </summary>
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
    [ItemCanBeNull]
    public ReadOnlyObservableCollection<CompatibleHidDeviceSlot> ActiveControllers { get; }

    /// <inheritdoc />
    public int AssignFreeSlotWith(ICompatibleHidDevice device)
    {
        var slot = activeControllers.FirstOrDefault(s => !s.IsOccupied);

        //
        // No free slot
        // 
        if (slot is null)
            return -1;

        slot.Device = device;
        slot.IsOccupied = true;

        ControllerSlotOccupied?.Invoke(slot);

        return slot.SlotIndex;
    }

    /// <inheritdoc />
    public int FreeSlotContaining(ICompatibleHidDevice device)
    {
        var slot = activeControllers.First(s => Equals(s.Device, device));

        ControllerSlotFreed?.Invoke(slot);

        slot.IsOccupied = false;
        slot.Device = null;

        return slot.SlotIndex;
    }

    /// <inheritdoc />
    public event Action<CompatibleHidDeviceSlot> ControllerSlotOccupied;

    /// <inheritdoc />
    public event Action<CompatibleHidDeviceSlot> ControllerSlotFreed;
}