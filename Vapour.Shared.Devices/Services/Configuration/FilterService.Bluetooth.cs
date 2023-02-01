using System.Net.NetworkInformation;

using Windows.Win32.Devices.DeviceAndDriverInstallation;

using Microsoft.Extensions.Logging;

using Nefarius.Utilities.Bluetooth.SDP;
using Nefarius.Utilities.DeviceManagement.Exceptions;
using Nefarius.Utilities.DeviceManagement.PnP;

using Vapour.Shared.Common.Util;

namespace Vapour.Shared.Devices.Services.Configuration;

public partial class FilterService
{
    /// <summary>
    ///     The DEVPKEY_Bluetooth_DeviceAddress which stores the remote device address as hex string.
    /// </summary>
    private static DevicePropertyKey BluetoothDeviceAddressProperty => CustomDeviceProperty.CreateCustomDeviceProperty(
        Guid.Parse("{0x2bd67d8b, 0x8beb, 0x48d5, {0x87, 0xe0, 0x6c, 0xda, 0x34, 0x28, 0x04, 0x0a}}"), 1,
        typeof(string));

    /// <summary>
    ///     Patches the SDP record of a given wireless device and optionally restarts the Bluetooth host radio.
    /// </summary>
    /// <param name="instanceId">The Instance ID of the HID device connected via Bluetooth.</param>
    /// <param name="ct">Optional cancellation token.</param>
    private async Task FilterBtController(string instanceId, CancellationToken ct = default)
    {
        PnPDevice hidDevice = PnPDevice.GetDeviceByInstanceId(instanceId);

        string parentId = hidDevice.GetProperty<string>(DevicePropertyKey.Device_Parent);

        PnPDevice parentDevice = PnPDevice.GetDeviceByInstanceId(parentId);

        string remoteAddressString = parentDevice.GetProperty<string>(BluetoothDeviceAddressProperty);

        PhysicalAddress remoteAddress = PhysicalAddress.Parse(remoteAddressString);

        BthPortDevice bthDevice = BthPort.Devices.FirstOrDefault(d => d.RemoteAddress.Equals(remoteAddress));

        if (bthDevice is null)
        {
            throw new FilterServiceException(
                $"Wireless device with address {remoteAddress.ToFriendlyName()} not found.");
        }

        if (bthDevice.IsCachedServicesPatched)
        {
            _logger.LogWarning("Device {Address} is already patched, nothing to do", remoteAddress.ToFriendlyName());
            return;
        }

        if (!SdpPatcher.AlterHidDeviceToVenderDefined(bthDevice.CachedServices, out byte[] patched))
        {
            throw new FilterServiceException(
                $"Failed to patch device with address {remoteAddress.ToFriendlyName()}.");
        }

        // overwrite patched record
        bthDevice.CachedServices = patched;

        int maxRetries = 5;

        while (!ct.IsCancellationRequested && maxRetries-- > 0)
        {
            // enforces reloading patched records from registry
            try
            {
                parentDevice.Disable();
                await Task.Delay(TimeSpan.FromSeconds(1), ct);
                parentDevice.Enable();
            }
            catch (ConfigManagerException cme)
            {
                if (cme.Value != (uint)CONFIGRET.CR_REMOVE_VETOED)
                {
                    // unexpected error
                    throw;
                }

                await Task.Delay(TimeSpan.FromSeconds(1), ct);
            }
        }
    }

    /// <summary>
    ///     Reverts the patched SDP records of a given wireless device to original state and optionally restarts the Bluetooth
    ///     host radio.
    /// </summary>
    /// <param name="instanceId">The Instance ID of the HID device connected via Bluetooth.</param>
    /// <param name="ct">Optional cancellation token.</param>
    private async Task UnfilterBtController(string instanceId, CancellationToken ct = default)
    {
        PnPDevice hidDevice = PnPDevice.GetDeviceByInstanceId(instanceId);

        string parentId = hidDevice.GetProperty<string>(DevicePropertyKey.Device_Parent);

        PnPDevice parentDevice = PnPDevice.GetDeviceByInstanceId(parentId);

        string remoteAddressString = parentDevice.GetProperty<string>(BluetoothDeviceAddressProperty);

        PhysicalAddress remoteAddress = PhysicalAddress.Parse(remoteAddressString);

        BthPortDevice bthDevice = BthPort.Devices.FirstOrDefault(d => d.RemoteAddress.Equals(remoteAddress));

        if (bthDevice is null)
        {
            throw new FilterServiceException(
                $"Wireless device with address {remoteAddress.ToFriendlyName()} not found.");
        }

        if (!bthDevice.IsCachedServicesPatched)
        {
            _logger.LogWarning("Device {Address} is not patched, nothing to do", remoteAddress);
            return;
        }

        // restore original record, remove backup copy
        bthDevice.CachedServices = bthDevice.OriginalCachedServices;
        bthDevice.DeleteOriginalCachedServices();

        int maxRetries = 5;

        while (!ct.IsCancellationRequested && maxRetries-- > 0)
        {
            // enforces reloading patched records from registry
            try
            {
                parentDevice.Disable();
                await Task.Delay(TimeSpan.FromSeconds(1), ct);
                parentDevice.Enable();
            }
            catch (ConfigManagerException cme)
            {
                if (cme.Value != (uint)CONFIGRET.CR_REMOVE_VETOED)
                {
                    // unexpected error
                    throw;
                }

                await Task.Delay(TimeSpan.FromSeconds(1), ct);
            }
        }
    }
}
