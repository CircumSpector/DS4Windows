using System.Collections.ObjectModel;

using JetBrains.Annotations;

using Vapour.Shared.Common.Core;
using Vapour.Shared.Devices.HID;

namespace Vapour.Shared.Devices.Services;

/// <summary>
///     Handles currently active devices and occupied slots and takes care that profiles are loaded when required.
/// </summary>
public sealed class ControllerManagerService : IControllerManagerService
{
    private readonly ObservableCollection<CompatibleHidDeviceSlot> _activeControllers;

    public ControllerManagerService()
    {
        _activeControllers = new ObservableCollection<CompatibleHidDeviceSlot>(Enumerable
            .Range(0, Constants.MaxControllers)
            .Select(i => new CompatibleHidDeviceSlot(i))
        );

        ActiveControllers = new ReadOnlyObservableCollection<CompatibleHidDeviceSlot>(_activeControllers);
    }

    /// <inheritdoc />
    [ItemCanBeNull]
    public ReadOnlyObservableCollection<CompatibleHidDeviceSlot> ActiveControllers { get; }

    /// <inheritdoc />
    public int AssignFreeSlotWith(ICompatibleHidDevice device)
    {
        CompatibleHidDeviceSlot slot = _activeControllers.FirstOrDefault(s => !s.IsOccupied);

        //
        // No free slot
        // 
        if (slot is null)
        {
            return -1;
        }

        slot.Device = device;
        slot.IsOccupied = true;

        ControllerSlotOccupied?.Invoke(slot);

        return slot.SlotIndex;
    }

    /// <inheritdoc />
    public int FreeSlotContaining(ICompatibleHidDevice device)
    {
        CompatibleHidDeviceSlot slot = _activeControllers.First(s => Equals(s.Device, device));

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