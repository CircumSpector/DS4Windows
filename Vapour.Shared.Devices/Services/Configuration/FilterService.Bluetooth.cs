using Microsoft.Extensions.Logging;

using Nefarius.Utilities.Bluetooth;
using Nefarius.Utilities.Bluetooth.SDP;

using Vapour.Shared.Common.Util;
using Vapour.Shared.Devices.HID;

namespace Vapour.Shared.Devices.Services.Configuration;

public partial class FilterService
{
    private void FilterBtController(ICompatibleHidDevice device, bool shouldRestartBtHost)
    {
        BthPortDevice bthDevice = BthPort.Devices.FirstOrDefault(d => d.RemoteAddress.Equals(device.Serial));

        if (bthDevice is null)
        {
            throw new FilterServiceException(
                $"Wireless device with address {device.Serial.ToFriendlyName()} not found.");
        }

        if (bthDevice.IsCachedServicesPatched)
        {
            _logger.LogWarning("Device {Address} is already patched, nothing to do", device.Serial);
            return;
        }

        if (!SdpPatcher.AlterHidDeviceToVenderDefined(bthDevice.CachedServices, out byte[] patched))
        {
            throw new FilterServiceException(
                $"Failed to patch device with address {device.Serial.ToFriendlyName()}.");
        }

        bthDevice.CachedServices = patched;

        if (shouldRestartBtHost)
        {
            using HostRadio radio = new();
            radio.RestartRadio();
        }
    }

    private void UnfilterBtController(ICompatibleHidDevice device, bool shouldRestartBtHost)
    {
        BthPortDevice bthDevice = BthPort.Devices.FirstOrDefault(d => d.RemoteAddress.Equals(device.Serial));

        if (bthDevice is null)
        {
            throw new FilterServiceException(
                $"Wireless device with address {device.Serial.ToFriendlyName()} not found.");
        }

        if (!bthDevice.IsCachedServicesPatched)
        {
            _logger.LogWarning("Device {Address} is not patched, nothing to do", device.Serial);
            return;
        }

        bthDevice.CachedServices = bthDevice.OriginalCachedServices;
        bthDevice.DeleteOriginalCachedServices();

        if (shouldRestartBtHost)
        {
            using HostRadio radio = new();
            radio.RestartRadio();
        }
    }
}
