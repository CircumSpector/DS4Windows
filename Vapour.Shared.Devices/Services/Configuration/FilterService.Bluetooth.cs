using System.Net.NetworkInformation;

using Microsoft.Extensions.Logging;

using Nefarius.Utilities.Bluetooth;
using Nefarius.Utilities.Bluetooth.SDP;
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
    /// <param name="shouldRestartBtHost">True to cycle the radio, default is not to.</param>
    private void FilterBtController(string instanceId, bool shouldRestartBtHost = false)
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

        /* TODO: doesn't appear to work :(
        if (Devcon.FindInDeviceClassByHardwareId(
                Guid.Parse("{e0cbf06c-cd8b-4647-bb8a-263b43f0f974}"),
                @$"BTHENUM\Dev_{remoteAddressString}", 
                out IEnumerable<string> genInstances)
           )
        {
            PnPDevice genericBthDevice = PnPDevice.GetDeviceByInstanceId(genInstances.First());

            genericBthDevice.Restart();
        }
        */

        if (shouldRestartBtHost)
        {
            using HostRadio radio = new();
            radio.RestartRadio();
        }
    }

    /// <summary>
    ///     Reverts the patched SDP records of a given wireless device to original state and optionally restarts the Bluetooth
    ///     host radio.
    /// </summary>
    /// <param name="instanceId">The Instance ID of the HID device connected via Bluetooth.</param>
    /// <param name="shouldRestartBtHost">True to cycle the radio, default is not to.</param>
    private void UnfilterBtController(string instanceId, bool shouldRestartBtHost = false)
    {
        PnPDevice hidDevice = PnPDevice.GetDeviceByInstanceId(instanceId);

        string parentId = hidDevice.GetProperty<string>(DevicePropertyKey.Device_Parent);

        PnPDevice parentDevice = PnPDevice.GetDeviceByInstanceId(parentId);

        PhysicalAddress remoteAddress =
            PhysicalAddress.Parse(parentDevice.GetProperty<string>(BluetoothDeviceAddressProperty));

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

        if (shouldRestartBtHost)
        {
            using HostRadio radio = new();
            radio.RestartRadio();
        }
    }
}
