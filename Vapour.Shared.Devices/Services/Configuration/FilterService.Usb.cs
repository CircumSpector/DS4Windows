using System.Diagnostics.CodeAnalysis;

using Microsoft.Extensions.Logging;
using Nefarius.Drivers.Nssidswap;
using Nefarius.Utilities.DeviceManagement.Exceptions;
using Nefarius.Utilities.DeviceManagement.Extensions;
using Nefarius.Utilities.DeviceManagement.PnP;

using Vapour.Shared.Devices.HID;

namespace Vapour.Shared.Devices.Services.Configuration;

public partial class FilterService
{
    private void FilterUsbDevice(ICompatibleHidDevice deviceToFilter)
    {
        (PnPDevice device, string hardwareId) = GetDeviceToFilter(deviceToFilter.SourceDevice.InstanceId);
        try
        {
            UsbPnPDevice usbDevice = device.ToUsbPnPDevice();

            using RewriteEntry entry = _filterDriver.AddOrUpdateRewriteEntry(hardwareId);
            entry.IsReplacingEnabled = true;
            entry.CompatibleIds = new[]
            {
                @"USB\MS_COMP_WINUSB", @"USB\Class_FF&SubClass_5D&Prot_01", @"USB\Class_FF&SubClass_5D",
                @"USB\Class_FF"
            };

            CyclePort(usbDevice);
        }
        catch (UsbPnPDeviceRestartException ex)
        {
            _logger.LogError(ex, "Device restart failed");
            throw;
        }
    }

    private void UnfilterUsbDevice(ICompatibleHidDevice deviceToUnfilter)
    {
        (PnPDevice device, string hardwareId) = GetDeviceToFilter(deviceToUnfilter.SourceDevice.InstanceId);
        try
        {
            UsbPnPDevice usbDevice = device.ToUsbPnPDevice();

            using RewriteEntry entry = _filterDriver.GetRewriteEntryFor(hardwareId);

            if (entry is null)
            {
                return;
            }

            entry.IsReplacingEnabled = false;

            CyclePort(usbDevice);
        }
        catch (UsbPnPDeviceRestartException ex)
        {
            _logger.LogError(ex, "Device restart failed");
            throw;
        }
    }

    /// <summary>
    ///     Power-cycles the port the given device is attached to.
    /// </summary>
    /// <param name="usbDevice">The USB device to restart.</param>
    [SuppressMessage("ReSharper", "AccessToDisposedClosure")]
    private void CyclePort(UsbPnPDevice usbDevice)
    {
        ManualResetEvent wait = new(false);
        DeviceNotificationListener listener = new();

        listener.RegisterDeviceArrived(args =>
        {
            wait.Set();
        });
        listener.StartListen(FilterDriver.RewrittenDeviceInterfaceId);
        listener.StartListen(DeviceInterfaceIds.HidDevice);

        usbDevice.CyclePort();

        wait.WaitOne(TimeSpan.FromSeconds(1));

        listener.StopListen();
        listener.Dispose();
        wait.Dispose();
    }

    /// <summary>
    ///     Finds the correct parent USB instance for a HID device.
    /// </summary>
    /// <param name="instanceId">The HID (or, if rewritten already, USB) instance ID.</param>
    private static Tuple<PnPDevice, string> GetDeviceToFilter(string instanceId)
    {
        PnPDevice device = PnPDevice.GetDeviceByInstanceId(instanceId);

        string[] hardwareIds = device.GetProperty<string[]>(DevicePropertyKey.Device_HardwareIds);

        if (hardwareIds.First().StartsWith("HID"))
        {
            string parentInputDeviceId = device.GetProperty<string>(DevicePropertyKey.Device_Parent);
            device = PnPDevice.GetDeviceByInstanceId(parentInputDeviceId);
            hardwareIds = device.GetProperty<string[]>(DevicePropertyKey.Device_HardwareIds);
        }

        return new Tuple<PnPDevice, string>(device, hardwareIds.First());
    }
}