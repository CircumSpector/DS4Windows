using System.Collections.ObjectModel;
using System.Security;

using JetBrains.Annotations;

using Microsoft.Extensions.Logging;

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
    private readonly FilterDriver _filterDriver;
    // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
    private readonly ILogger<ControllerManagerService> _logger;

    public ControllerManagerService(ILogger<ControllerManagerService> logger)
    {
        _logger = logger;

        try
        {
            _filterDriver = new FilterDriver
            {
                // and allow different users of the same computer to choose whether to use it
                // that would allow for cleanup while not running
                // thinking we should enable/disable on service host stop/start and based on a config entry
                IsEnabled = true
            };
        }
        catch (SecurityException ex)
        {
            _logger.LogError(ex, "To use the rewrite feature, the service must be run as Administrator!");
            throw;
        }

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

    /// <inheritdoc />
    public void FilterController(string instanceId)
    {
        Tuple<PnPDevice, string> device = GetDeviceToFilter(instanceId);

        //TODO: filter the controller and cycle the port

        RewriteEntry entry = _filterDriver.AddOrUpdateRewriteEntry(device.Item2);
        entry.IsReplacingEnabled = true;
        entry.CompatibleIds = new[]
        {
            @"USB\MS_COMP_WINUSB", @"USB\Class_FF&SubClass_5D&Prot_01", @"USB\Class_FF&SubClass_5D", @"USB\Class_FF"
        };
        entry.Dispose();

        device.Item1.ToUsbPnPDevice().CyclePort();
    }

    /// <inheritdoc />
    public void UnfilterController(string instanceId)
    {
        Tuple<PnPDevice, string> device = GetDeviceToFilter(instanceId);

        //TODO: fill in the unfilter

        RewriteEntry entry = _filterDriver.GetRewriteEntryFor(device.Item2);
        if (entry != null)
        {
            entry.IsReplacingEnabled = false;
            entry.Dispose();
        }
    }

    private Tuple<PnPDevice, string> GetDeviceToFilter(string instanceId)
    {
        PnPDevice device = PnPDevice.GetDeviceByInstanceId(instanceId);
        string[] hardwareIds = device.GetProperty<string[]>(DevicePropertyKey.Device_HardwareIds);
        if (hardwareIds[0].StartsWith("HID"))
        {
            string parentInputDeviceId = device.GetProperty<string>(DevicePropertyKey.Device_Parent);
            device = PnPDevice.GetDeviceByInstanceId(parentInputDeviceId);
            hardwareIds = device.GetProperty<string[]>(DevicePropertyKey.Device_HardwareIds);
        }

        return new Tuple<PnPDevice, string>(device, hardwareIds[0]);
    }
}