using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Windows.Win32.Devices.HumanInterfaceDevice;
using Windows.Win32.Storage.FileSystem;
using DS4Windows.Shared.Common.Telemetry;
using DS4Windows.Shared.Devices.HID;
using Microsoft.Extensions.Logging;
using Nefarius.Utilities.DeviceManagement.PnP;

namespace DS4Windows.Shared.Devices.Services;

/// <summary>
///     Single point of truth of states for all connected and handled HID devices.
/// </summary>
public interface IHidDeviceEnumeratorService
{
    /// <summary>
    ///     List of currently available (connected) HID devices.
    /// </summary>
    ReadOnlyObservableCollection<HidDevice> ConnectedDevices { get; }

    /// <summary>
    ///     Gets fired when a new HID device has been detected.
    /// </summary>
    event Action<HidDevice> DeviceArrived;

    /// <summary>
    ///     Gets fired when an existing HID device has been removed.
    /// </summary>
    event Action<HidDevice> DeviceRemoved;

    /// <summary>
    ///     Refreshes <see cref="ConnectedDevices" />. This clears out the list and repopulates is.
    /// </summary>
    void EnumerateDevices();

    void ClearDevices();
}

/// <summary>
///     Single point of truth of states for all connected and handled HID devices.
/// </summary>
public class HidDeviceEnumeratorService : IHidDeviceEnumeratorService
{
    private readonly ObservableCollection<HidDevice> connectedDevices;
    protected readonly ActivitySource CoreActivity = new(TracingSources.DevicesAssemblyActivitySourceName);

    private readonly IDeviceNotificationListener deviceNotificationListener;
    private readonly Guid hidClassInterfaceGuid;

    private readonly ILogger<HidDeviceEnumeratorService> logger;

    public HidDeviceEnumeratorService(IDeviceNotificationListener deviceNotificationListener,
        ILogger<HidDeviceEnumeratorService> logger)
    {
        this.deviceNotificationListener = deviceNotificationListener;
        this.logger = logger;

        Windows.Win32.PInvoke.HidD_GetHidGuid(out var interfaceGuid);

        hidClassInterfaceGuid = interfaceGuid;

        Windows.Win32.PInvoke.HidD_GetHidGuid(out var hidGuid);
        this.deviceNotificationListener.RegisterDeviceArrived(DeviceNotificationListenerOnDeviceArrived, hidGuid);
        this.deviceNotificationListener.RegisterDeviceRemoved(DeviceNotificationListenerOnDeviceRemoved, hidGuid);

        connectedDevices = new ObservableCollection<HidDevice>();

        ConnectedDevices = new ReadOnlyObservableCollection<HidDevice>(connectedDevices);
    }

    /// <summary>
    ///     HID Device Class GUID.
    /// </summary>
    public static Guid HidDeviceClassGuid => Guid.Parse("{745a17a0-74d3-11d0-b6fe-00a0c90f57da}");

    /// <inheritdoc />
    public event Action<HidDevice> DeviceArrived;

    /// <inheritdoc />
    public event Action<HidDevice> DeviceRemoved;

    /// <inheritdoc />
    public ReadOnlyObservableCollection<HidDevice> ConnectedDevices { get; }

    /// <inheritdoc />
    public void EnumerateDevices()
    {
        using var activity = CoreActivity.StartActivity(
            $"{nameof(HidDeviceEnumeratorService)}:{nameof(EnumerateDevices)}");

        var deviceIndex = 0;

        connectedDevices.Clear();

        while (Devcon.FindByInterfaceGuid(hidClassInterfaceGuid, out var path, out var instanceId, deviceIndex++))
        {
            try
            {
                var entry = CreateNewHidDevice(path);

                logger.LogInformation("Discovered HID device {Device}", entry);

                connectedDevices.Add(entry);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, $"Failed to create HID device for {path}");
            }
        }
    }

    public void ClearDevices()
    {
        foreach (var connectedDevice in ConnectedDevices.ToList()) RemoveDevice(connectedDevice.Path);
    }

    /// <summary>
    ///     Create new <see cref="HidDevice" /> and initialize basic properties.
    /// </summary>
    /// <param name="path">The symbolic link path of the device instance.</param>
    /// <returns>The new <see cref="HidDevice" />.</returns>
    private HidDevice CreateNewHidDevice(string path)
    {
        using var activity = CoreActivity.StartActivity(
            $"{nameof(HidDeviceEnumeratorService)}:{nameof(CreateNewHidDevice)}");

        activity?.SetTag("Path", path);

        var device = PnPDevice.GetDeviceByInterfaceId(path);

        //
        // Try to get friendly display name (not always there)
        // 
        var friendlyName = device.GetProperty<string>(DevicePropertyKey.Device_FriendlyName);
        var parentId = device.GetProperty<string>(DevicePropertyKey.Device_Parent);

        //
        // Grab product string from device if property is missing
        // 
        if (string.IsNullOrEmpty(friendlyName)) friendlyName = GetHidProductString(path);

        GetHidAttributes(path, out var attributes);

        GetHidCapabilities(path, out var caps);

        return new HidDevice
        {
            Path = path,
            InstanceId = device.InstanceId.ToUpper(),
            Description = device.GetProperty<string>(DevicePropertyKey.Device_DeviceDesc),
            DisplayName = friendlyName,
            ParentInstance = parentId,
            Attributes = attributes,
            Capabilities = caps,
            IsVirtual = device.IsVirtual(),
            ManufacturerString = GetHidManufacturerString(path),
            ProductString = GetHidProductString(path),
            SerialNumberString = GetHidSerialNumberString(path)
        };
    }

    private unsafe string GetHidManufacturerString(string path)
    {
        using var activity = CoreActivity.StartActivity(
            $"{nameof(HidDeviceEnumeratorService)}:{nameof(GetHidManufacturerString)}");
        activity?.SetTag("Path", path);

        using var handle = Windows.Win32.PInvoke.CreateFile(
            path,
            FILE_ACCESS_FLAGS.FILE_GENERIC_READ | FILE_ACCESS_FLAGS.FILE_GENERIC_WRITE,
            FILE_SHARE_MODE.FILE_SHARE_READ | FILE_SHARE_MODE.FILE_SHARE_WRITE,
            null,
            FILE_CREATION_DISPOSITION.OPEN_EXISTING,
            FILE_FLAGS_AND_ATTRIBUTES.FILE_ATTRIBUTE_NORMAL,
            null
        );

        const uint bufferLength = 4093; // max allowed/possible size according to MSDN
        var buffer = stackalloc char[(int)bufferLength];

        Windows.Win32.PInvoke.HidD_GetManufacturerString(handle, buffer, bufferLength);

        var manufacturerString = new string(buffer);

        activity?.SetTag("ManufacturerString", manufacturerString);

        return manufacturerString;
    }

    private unsafe string GetHidProductString(string path)
    {
        using var activity = CoreActivity.StartActivity(
            $"{nameof(HidDeviceEnumeratorService)}:{nameof(GetHidProductString)}");
        activity?.SetTag("Path", path);

        using var handle = Windows.Win32.PInvoke.CreateFile(
            path,
            FILE_ACCESS_FLAGS.FILE_GENERIC_READ | FILE_ACCESS_FLAGS.FILE_GENERIC_WRITE,
            FILE_SHARE_MODE.FILE_SHARE_READ | FILE_SHARE_MODE.FILE_SHARE_WRITE,
            null,
            FILE_CREATION_DISPOSITION.OPEN_EXISTING,
            FILE_FLAGS_AND_ATTRIBUTES.FILE_ATTRIBUTE_NORMAL,
            null
        );

        const uint bufferLength = 4093; // max allowed/possible size according to MSDN
        var buffer = stackalloc char[(int)bufferLength];

        Windows.Win32.PInvoke.HidD_GetProductString(handle, buffer, bufferLength);

        var productName = new string(buffer);

        activity?.SetTag("ProductString", productName);

        return productName;
    }

    private unsafe string GetHidSerialNumberString(string path)
    {
        using var activity = CoreActivity.StartActivity(
            $"{nameof(HidDeviceEnumeratorService)}:{nameof(GetHidSerialNumberString)}");
        activity?.SetTag("Path", path);

        using var handle = Windows.Win32.PInvoke.CreateFile(
            path,
            FILE_ACCESS_FLAGS.FILE_GENERIC_READ | FILE_ACCESS_FLAGS.FILE_GENERIC_WRITE,
            FILE_SHARE_MODE.FILE_SHARE_READ | FILE_SHARE_MODE.FILE_SHARE_WRITE,
            null,
            FILE_CREATION_DISPOSITION.OPEN_EXISTING,
            FILE_FLAGS_AND_ATTRIBUTES.FILE_ATTRIBUTE_NORMAL,
            null
        );

        const uint bufferLength = 4093; // max allowed/possible size according to MSDN
        var buffer = stackalloc char[(int)bufferLength];

        Windows.Win32.PInvoke.HidD_GetSerialNumberString(handle, buffer, bufferLength);

        var serialNumberString = new string(buffer);

        activity?.SetTag("SerialNumberString", serialNumberString);

        return serialNumberString;
    }

    private bool GetHidAttributes(string path, out HIDD_ATTRIBUTES attributes)
    {
        using var activity = CoreActivity.StartActivity(
            $"{nameof(HidDeviceEnumeratorService)}:{nameof(GetHidAttributes)}");
        activity?.SetTag("Path", path);

        using var handle = Windows.Win32.PInvoke.CreateFile(
            path,
            FILE_ACCESS_FLAGS.FILE_GENERIC_READ | FILE_ACCESS_FLAGS.FILE_GENERIC_WRITE,
            FILE_SHARE_MODE.FILE_SHARE_READ | FILE_SHARE_MODE.FILE_SHARE_WRITE,
            null,
            FILE_CREATION_DISPOSITION.OPEN_EXISTING,
            FILE_FLAGS_AND_ATTRIBUTES.FILE_ATTRIBUTE_NORMAL,
            null
        );

        var ret = Windows.Win32.PInvoke.HidD_GetAttributes(handle, out attributes);

        if (!ret) return false;

        activity?.SetTag("VID", attributes.VendorID.ToString("X4"));
        activity?.SetTag("PID", attributes.ProductID.ToString("X4"));

        return true;
    }

    private void GetHidCapabilities(string path, out HIDP_CAPS caps)
    {
        using var activity = CoreActivity.StartActivity(
            $"{nameof(HidDeviceEnumeratorService)}:{nameof(GetHidCapabilities)}");
        activity?.SetTag("Path", path);

        using var handle = Windows.Win32.PInvoke.CreateFile(
            path,
            FILE_ACCESS_FLAGS.FILE_GENERIC_READ | FILE_ACCESS_FLAGS.FILE_GENERIC_WRITE,
            FILE_SHARE_MODE.FILE_SHARE_READ | FILE_SHARE_MODE.FILE_SHARE_WRITE,
            null,
            FILE_CREATION_DISPOSITION.OPEN_EXISTING,
            FILE_FLAGS_AND_ATTRIBUTES.FILE_ATTRIBUTE_NORMAL,
            null
        );

        if (handle.IsInvalid)
            throw new Exception($"Couldn't open device handle, error {Marshal.GetLastWin32Error()}");

        if (!Windows.Win32.PInvoke.HidD_GetPreparsedData(handle, out var dataHandle))
            throw new Exception($"HidD_GetPreparsedData failed with error {Marshal.GetLastWin32Error()}");

        Windows.Win32.PInvoke.HidP_GetCaps(dataHandle, out caps);
        Windows.Win32.PInvoke.HidD_FreePreparsedData(dataHandle);

        activity?.SetTag("InputReportByteLength", caps.InputReportByteLength);
        activity?.SetTag("OutputReportByteLength", caps.OutputReportByteLength);
    }

    private void DeviceNotificationListenerOnDeviceArrived(DeviceEventArgs args)
    {
        using var activity = CoreActivity.StartActivity(
            $"{nameof(HidDeviceEnumeratorService)}:{nameof(DeviceNotificationListenerOnDeviceArrived)}");

        var symLink = args.SymLink;
        activity?.SetTag("Path", symLink);

        try
        {
            var device = PnPDevice.GetDeviceByInterfaceId(symLink);

            logger.LogInformation("HID Device {Instance} ({Path}) arrived",
                device.InstanceId, symLink);

            //
            // This should never happen as we're only listening to HID Class Devices
            // changes anyway but some extra safety and logging can't hurt :)
            // 
            if (!IsHidDevice(device))
            {
                logger.LogInformation("Device {Instance} ({Path}) is not a HID device, ignoring",
                    device.InstanceId, symLink);
                return;
            }

            var entry = CreateNewHidDevice(symLink);

            if (device.IsVirtual())
                logger.LogInformation("HID Device {Instance} ({Path}) is emulated, setting flag",
                    device.InstanceId, symLink);

            if (!connectedDevices.Contains(entry))
                connectedDevices.Add(entry);

            DeviceArrived?.Invoke(entry);
        }
        catch (ArgumentException ae)
        {
            logger.LogWarning(ae, "Failed to add new device");
        }
    }

    private void DeviceNotificationListenerOnDeviceRemoved(DeviceEventArgs args)
    {
        RemoveDevice(args.SymLink);
    }

    private void RemoveDevice(string symLink)
    {
        using var activity = CoreActivity.StartActivity(
            $"{nameof(HidDeviceEnumeratorService)}:{nameof(DeviceNotificationListenerOnDeviceRemoved)}");

        activity?.SetTag("Path", symLink);

        var device = PnPDevice.GetDeviceByInterfaceId(symLink, DeviceLocationFlags.Phantom);

        logger.LogInformation("HID Device {Instance} ({Path}) removed",
            device.InstanceId, symLink);

        GetHidAttributes(symLink, out var attributes);

        var entry = new HidDevice
        {
            Path = symLink,
            IsVirtual = device.IsVirtual(),
            InstanceId = device.InstanceId.ToUpper(),
            Attributes = attributes
        };

        if (connectedDevices.Contains(entry))
            connectedDevices.Remove(entry);

        DeviceRemoved?.Invoke(entry);
    }

    /// <summary>
    ///     Checks if the current device belongs to HIDClass.
    /// </summary>
    /// <param name="device">The <see cref="PnPDevice" /> to test.</param>
    /// <returns>True if HIDClass device, false otherwise.</returns>
    private static bool IsHidDevice(PnPDevice device)
    {
        var devClass = device.GetProperty<Guid>(DevicePropertyKey.Device_ClassGuid);

        return Equals(devClass, HidDeviceClassGuid);
    }
}