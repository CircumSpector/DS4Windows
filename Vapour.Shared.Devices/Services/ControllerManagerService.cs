using System.Collections.ObjectModel;

using JetBrains.Annotations;

using Nefarius.Drivers.Identinator;
using Nefarius.Utilities.DeviceManagement.Extensions;
using Nefarius.Utilities.DeviceManagement.PnP;

using Vapour.Shared.Common.Core;
using Vapour.Shared.Devices.HID;

namespace Vapour.Shared.Devices.Services;

/// <summary>
///     Handles currently active devices and occupied slots and takes care that profiles are loaded when required.
/// </summary>
public sealed class ControllerManagerService : IControllerManagerService
{
    private readonly ObservableCollection<CompatibleHidDeviceSlot> _activeControllers;
    private readonly FilterDriver _filterDriver = new();

    public ControllerManagerService()
    {
        _filterDriver.IsEnabled = true;

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
        var slot = _activeControllers.FirstOrDefault(s => !s.IsOccupied);

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
        var slot = _activeControllers.First(s => Equals(s.Device, device));

        ControllerSlotFreed?.Invoke(slot);

        slot.IsOccupied = false;
        slot.Device = null;

        return slot.SlotIndex;
    }

    /// <inheritdoc />
    public event Action<CompatibleHidDeviceSlot> ControllerSlotOccupied;

    /// <inheritdoc />
    public event Action<CompatibleHidDeviceSlot> ControllerSlotFreed;

    /// <inheritdoc />
    public void FilterController(string instanceId)
    {
        var device = PnPDevice.GetDeviceByInstanceId(instanceId);
        var hardwareIds= device.GetProperty<string[]>(DevicePropertyKey.Device_HardwareIds);

        var entry = _filterDriver.AddOrUpdateRewriteEntry(hardwareIds[0]);
        entry.IsReplacingEnabled = true;
        entry.CompatibleIds = new[]
        {
            @"USB\MS_COMP_WINUSB",
            @"USB\Class_FF&SubClass_5D&Prot_01",
            @"USB\Class_FF&SubClass_5D",
            @"USB\Class_FF"
        };

        device.ToUsbPnPDevice().CyclePort();
    }
}