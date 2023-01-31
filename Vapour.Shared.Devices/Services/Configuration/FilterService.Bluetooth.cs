﻿using System.Net.NetworkInformation;

using Microsoft.Extensions.Logging;

using Nefarius.Utilities.Bluetooth;
using Nefarius.Utilities.Bluetooth.SDP;
using Nefarius.Utilities.DeviceManagement.PnP;

using Vapour.Shared.Common.Util;

namespace Vapour.Shared.Devices.Services.Configuration;

public partial class FilterService
{
    private static DevicePropertyKey BluetoothDeviceAddressProperty => CustomDeviceProperty.CreateCustomDeviceProperty(
        Guid.Parse("{0x2bd67d8b, 0x8beb, 0x48d5, {0x87, 0xe0, 0x6c, 0xda, 0x34, 0x28, 0x04, 0x0a}}"), 1,
        typeof(string));

    public void RestartBtHost()
    {
        using HostRadio radio = new();
        radio.RestartRadio();
    }

    public bool IsBtFiltered(string instanceId)
    {
        var bthDevice = GetBthDevice(instanceId);
        return bthDevice.IsCachedServicesPatched;
    }

    private void FilterBtController(string instanceId, bool shouldRestartBtHost = false)
    {
        var bthDevice = GetBthDevice(instanceId);

        if (bthDevice.IsCachedServicesPatched)
        {
            _logger.LogWarning("Device {Address} is already patched, nothing to do", bthDevice.RemoteAddress.ToFriendlyName());
            return;
        }

        if (!SdpPatcher.AlterHidDeviceToVenderDefined(bthDevice.CachedServices, out byte[] patched))
        {
            throw new FilterServiceException(
                $"Failed to patch device with address {bthDevice.RemoteAddress.ToFriendlyName()}.");
        }

        bthDevice.CachedServices = patched;

        if (shouldRestartBtHost)
        {
            using HostRadio radio = new();
            radio.RestartRadio();
        }
    }

    private void UnfilterBtController(string instanceId, bool shouldRestartBtHost = false)
    {
        var bthDevice = GetBthDevice(instanceId);

        if (!bthDevice.IsCachedServicesPatched)
        {
            _logger.LogWarning("Device {Address} is not patched, nothing to do", bthDevice.RemoteAddress.ToFriendlyName());
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

    private static BthPortDevice GetBthDevice(string instanceId)
    {
        PnPDevice hidDevice = PnPDevice.GetDeviceByInstanceId(instanceId);

        string parentId = hidDevice.GetProperty<string>(DevicePropertyKey.Device_Parent);

        PnPDevice parentDevice = PnPDevice.GetDeviceByInstanceId(parentId);

        PhysicalAddress remoteAddress = PhysicalAddress.Parse(parentDevice.GetProperty<string>(BluetoothDeviceAddressProperty));

        BthPortDevice bthDevice = BthPort.Devices.FirstOrDefault(d => d.RemoteAddress.Equals(remoteAddress));

        if (bthDevice is null)
        {
            throw new FilterServiceException(
                $"Wireless device with address {remoteAddress.ToFriendlyName()} not found.");
        }

        return bthDevice;
    }
}
