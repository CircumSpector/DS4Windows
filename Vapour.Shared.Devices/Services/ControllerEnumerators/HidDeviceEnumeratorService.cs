using System.Diagnostics;
using System.Runtime.InteropServices;

using Windows.Win32;
using Windows.Win32.Devices.HumanInterfaceDevice;
using Windows.Win32.Foundation;
using Windows.Win32.Storage.FileSystem;

using Microsoft.Extensions.Logging;
using Microsoft.Win32.SafeHandles;

using Nefarius.Utilities.DeviceManagement.PnP;

using Vapour.Shared.Common.Telemetry;
using Vapour.Shared.Devices.HID;

namespace Vapour.Shared.Devices.Services.ControllerEnumerators;

/// <summary>
///     Single point of truth of states for all connected and handled HID devices.
/// </summary>
internal sealed class HidDeviceEnumeratorService : IHidDeviceEnumeratorService<HidDevice>
{
    private readonly ActivitySource _coreActivity = new(TracingSources.AssemblyName);

    // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
    private readonly IDeviceNotificationListener _deviceNotificationListener;
    private readonly Guid _hidClassInterfaceGuid;

    private readonly ILogger<HidDeviceEnumeratorService> _logger;

    public HidDeviceEnumeratorService(IDeviceNotificationListener deviceNotificationListener,
        ILogger<HidDeviceEnumeratorService> logger)
    {
        _deviceNotificationListener = deviceNotificationListener;
        _logger = logger;

        PInvoke.HidD_GetHidGuid(out Guid interfaceGuid);

        _hidClassInterfaceGuid = interfaceGuid;

        _deviceNotificationListener.RegisterDeviceArrived(DeviceNotificationListenerOnDeviceArrived,
            _hidClassInterfaceGuid);
        _deviceNotificationListener.RegisterDeviceRemoved(DeviceNotificationListenerOnDeviceRemoved,
            _hidClassInterfaceGuid);
    }

    /// <summary>
    ///     HID Device Class GUID.
    /// </summary>
    public static Guid HidDeviceClassGuid => Guid.Parse("{745a17a0-74d3-11d0-b6fe-00a0c90f57da}");

    /// <inheritdoc />
    public event Action<HidDevice> DeviceArrived;

    public event Action<string> DeviceRemoved;

    /// <inheritdoc />
    public void EnumerateDevices()
    {
        using Activity activity = _coreActivity.StartActivity(
            $"{nameof(HidDeviceEnumeratorService)}:{nameof(EnumerateDevices)}");

        int deviceIndex = 0;
        
        while (Devcon.FindByInterfaceGuid(_hidClassInterfaceGuid, out string path, out _, deviceIndex++))
        {
            try
            {
                CreateNewHidDevice(path);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"Failed to create HID device for {path}");
            }
        }
    }

    private void DeviceNotificationListenerOnDeviceArrived(DeviceEventArgs args)
    {
        using Activity activity = _coreActivity.StartActivity(
            $"{nameof(HidDeviceEnumeratorService)}:{nameof(DeviceNotificationListenerOnDeviceArrived)}");

        string symLink = args.SymLink;
        activity?.SetTag("Path", symLink);

        try
        {
            _logger.LogInformation("HID Device ({Path}) arrived", symLink);

            CreateNewHidDevice(symLink);
        }
        catch (ArgumentException ae)
        {
            _logger.LogWarning(ae, "Failed to add new device");
        }
    }

    private void DeviceNotificationListenerOnDeviceRemoved(DeviceEventArgs args)
    {
        RemoveDevice(args.SymLink);
    }
    
    private void CreateNewHidDevice(string path)
    {
        using Activity activity = _coreActivity.StartActivity(
            $"{nameof(HidDeviceEnumeratorService)}:{nameof(CreateNewHidDevice)}");

        activity?.SetTag("Path", path);

        if (!TestDeviceAccess(path))
        {
            _logger.LogWarning(
                "Device {Path} couldn't be opened, it's probably in use exclusively by some other process", path);
            return;
        }

        PnPDevice device = PnPDevice.GetDeviceByInterfaceId(path);

        if (device.IsVirtual())
        {
            _logger.LogInformation("Device {Path} is virtual, exclude from processing", path);
            return;
        }

        if (!IsHidDevice(device))
        {
            _logger.LogInformation("Device {Instance} ({Path}) is not a HID device, ignoring",
                device.InstanceId, path);
            return;
        }

        //
        // Try to get friendly display name (not always there)
        // 
        string friendlyName = device.GetProperty<string>(DevicePropertyKey.Device_FriendlyName);
        string parentId = device.GetProperty<string>(DevicePropertyKey.Device_Parent);

        //
        // Grab product string from device if property is missing
        // 
        if (string.IsNullOrEmpty(friendlyName))
        {
            friendlyName = GetHidProductString(path);
        }

        GetHidAttributes(path, out HIDD_ATTRIBUTES attributes);

        GetHidCapabilities(path, out HIDP_CAPS caps);

        var hidDevice = new HidDevice
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

        DeviceArrived?.Invoke(hidDevice);
    }

    private void RemoveDevice(string symLink)
    {
        using Activity activity = _coreActivity.StartActivity(
            $"{nameof(HidDeviceEnumeratorService)}:{nameof(DeviceNotificationListenerOnDeviceRemoved)}");

        activity?.SetTag("Path", symLink);

        PnPDevice device = PnPDevice.GetDeviceByInterfaceId(symLink, DeviceLocationFlags.Phantom); 
        _logger.LogInformation("HID Device ({InstanceId} ({SymLink}) removed", device.InstanceId, symLink);
        
        if (!device.IsVirtual())
        {
            DeviceRemoved?.Invoke(device.InstanceId);
        }
    }
    
    /// <summary>
    ///     Attempts to open device.
    /// </summary>
    /// <param name="path">The symbolic link path of the device instance.</param>
    /// <returns>True if successful, false if opened exclusively already.</returns>
    /// <exception cref="Exception">Throws exception on any other unexpected error case.</exception>
    private static bool TestDeviceAccess(string path)
    {
        using SafeFileHandle handle = PInvoke.CreateFile(
            path,
            FILE_ACCESS_FLAGS.FILE_GENERIC_READ | FILE_ACCESS_FLAGS.FILE_GENERIC_WRITE,
            FILE_SHARE_MODE.FILE_SHARE_READ | FILE_SHARE_MODE.FILE_SHARE_WRITE,
            null,
            FILE_CREATION_DISPOSITION.OPEN_EXISTING,
            FILE_FLAGS_AND_ATTRIBUTES.FILE_ATTRIBUTE_NORMAL,
            null
        );

        return handle.IsInvalid switch
        {
            true when Marshal.GetLastWin32Error() == (int)WIN32_ERROR.ERROR_ACCESS_DENIED => false,
            true when Marshal.GetLastWin32Error() == (int)WIN32_ERROR.ERROR_SHARING_VIOLATION => false,
            true => throw new Exception($"Couldn't open device handle, error {Marshal.GetLastWin32Error()}"),
            _ => true
        };
    }

    private unsafe string GetHidManufacturerString(string path)
    {
        using Activity activity = _coreActivity.StartActivity(
            $"{nameof(HidDeviceEnumeratorService)}:{nameof(GetHidManufacturerString)}");
        activity?.SetTag("Path", path);

        using SafeFileHandle handle = PInvoke.CreateFile(
            path,
            FILE_ACCESS_FLAGS.FILE_GENERIC_READ | FILE_ACCESS_FLAGS.FILE_GENERIC_WRITE,
            FILE_SHARE_MODE.FILE_SHARE_READ | FILE_SHARE_MODE.FILE_SHARE_WRITE,
            null,
            FILE_CREATION_DISPOSITION.OPEN_EXISTING,
            FILE_FLAGS_AND_ATTRIBUTES.FILE_ATTRIBUTE_NORMAL,
            null
        );

        const uint bufferLength = 4093; // max allowed/possible size according to MSDN
        char* buffer = stackalloc char[(int)bufferLength];

        PInvoke.HidD_GetManufacturerString(handle, buffer, bufferLength);

        string manufacturerString = new(buffer);

        activity?.SetTag("ManufacturerString", manufacturerString);

        return manufacturerString;
    }

    private unsafe string GetHidProductString(string path)
    {
        using Activity activity = _coreActivity.StartActivity(
            $"{nameof(HidDeviceEnumeratorService)}:{nameof(GetHidProductString)}");
        activity?.SetTag("Path", path);

        using SafeFileHandle handle = PInvoke.CreateFile(
            path,
            FILE_ACCESS_FLAGS.FILE_GENERIC_READ | FILE_ACCESS_FLAGS.FILE_GENERIC_WRITE,
            FILE_SHARE_MODE.FILE_SHARE_READ | FILE_SHARE_MODE.FILE_SHARE_WRITE,
            null,
            FILE_CREATION_DISPOSITION.OPEN_EXISTING,
            FILE_FLAGS_AND_ATTRIBUTES.FILE_ATTRIBUTE_NORMAL,
            null
        );

        const uint bufferLength = 4093; // max allowed/possible size according to MSDN
        char* buffer = stackalloc char[(int)bufferLength];

        PInvoke.HidD_GetProductString(handle, buffer, bufferLength);

        string productName = new(buffer);

        activity?.SetTag("ProductString", productName);

        return productName;
    }

    private unsafe string GetHidSerialNumberString(string path)
    {
        using Activity activity = _coreActivity.StartActivity(
            $"{nameof(HidDeviceEnumeratorService)}:{nameof(GetHidSerialNumberString)}");
        activity?.SetTag("Path", path);

        using SafeFileHandle handle = PInvoke.CreateFile(
            path,
            FILE_ACCESS_FLAGS.FILE_GENERIC_READ | FILE_ACCESS_FLAGS.FILE_GENERIC_WRITE,
            FILE_SHARE_MODE.FILE_SHARE_READ | FILE_SHARE_MODE.FILE_SHARE_WRITE,
            null,
            FILE_CREATION_DISPOSITION.OPEN_EXISTING,
            FILE_FLAGS_AND_ATTRIBUTES.FILE_ATTRIBUTE_NORMAL,
            null
        );

        const uint bufferLength = 4093; // max allowed/possible size according to MSDN
        char* buffer = stackalloc char[(int)bufferLength];

        PInvoke.HidD_GetSerialNumberString(handle, buffer, bufferLength);

        string serialNumberString = new(buffer);

        activity?.SetTag("SerialNumberString", serialNumberString);

        return serialNumberString;
    }

    private bool GetHidAttributes(string path, out HIDD_ATTRIBUTES attributes)
    {
        using Activity activity = _coreActivity.StartActivity(
            $"{nameof(HidDeviceEnumeratorService)}:{nameof(GetHidAttributes)}");
        activity?.SetTag("Path", path);

        using SafeFileHandle handle = PInvoke.CreateFile(
            path,
            FILE_ACCESS_FLAGS.FILE_GENERIC_READ | FILE_ACCESS_FLAGS.FILE_GENERIC_WRITE,
            FILE_SHARE_MODE.FILE_SHARE_READ | FILE_SHARE_MODE.FILE_SHARE_WRITE,
            null,
            FILE_CREATION_DISPOSITION.OPEN_EXISTING,
            FILE_FLAGS_AND_ATTRIBUTES.FILE_ATTRIBUTE_NORMAL,
            null
        );

        BOOLEAN ret = PInvoke.HidD_GetAttributes(handle, out attributes);

        if (!ret)
        {
            return false;
        }

        activity?.SetTag("VID", attributes.VendorID.ToString("X4"));
        activity?.SetTag("PID", attributes.ProductID.ToString("X4"));

        return true;
    }

    private void GetHidCapabilities(string path, out HIDP_CAPS caps)
    {
        using Activity activity = _coreActivity.StartActivity(
            $"{nameof(HidDeviceEnumeratorService)}:{nameof(GetHidCapabilities)}");
        activity?.SetTag("Path", path);

        using SafeFileHandle handle = PInvoke.CreateFile(
            path,
            FILE_ACCESS_FLAGS.FILE_GENERIC_READ | FILE_ACCESS_FLAGS.FILE_GENERIC_WRITE,
            FILE_SHARE_MODE.FILE_SHARE_READ | FILE_SHARE_MODE.FILE_SHARE_WRITE,
            null,
            FILE_CREATION_DISPOSITION.OPEN_EXISTING,
            FILE_FLAGS_AND_ATTRIBUTES.FILE_ATTRIBUTE_NORMAL,
            null
        );

        if (handle.IsInvalid)
        {
            throw new Exception($"Couldn't open device handle, error {Marshal.GetLastWin32Error()}");
        }

        if (!PInvoke.HidD_GetPreparsedData(handle, out nint dataHandle))
        {
            throw new Exception($"HidD_GetPreparsedData failed with error {Marshal.GetLastWin32Error()}");
        }

        PInvoke.HidP_GetCaps(dataHandle, out caps);
        PInvoke.HidD_FreePreparsedData(dataHandle);

        activity?.SetTag("InputReportByteLength", caps.InputReportByteLength);
        activity?.SetTag("OutputReportByteLength", caps.OutputReportByteLength);
    }

    /// <summary>
    ///     Checks if the current device belongs to HIDClass.
    /// </summary>
    /// <param name="device">The <see cref="PnPDevice" /> to test.</param>
    /// <returns>True if HIDClass device, false otherwise.</returns>
    private static bool IsHidDevice(PnPDevice device)
    {
        Guid devClass = device.GetProperty<Guid>(DevicePropertyKey.Device_ClassGuid);

        return Equals(devClass, HidDeviceClassGuid);
    }
}