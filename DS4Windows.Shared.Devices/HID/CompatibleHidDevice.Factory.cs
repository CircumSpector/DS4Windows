using System;
using System.Net.NetworkInformation;
using DS4Windows.Shared.Devices.HID.Devices;
using PInvoke;

namespace DS4Windows.Shared.Devices.HID;

public interface ICompatibleHidDevice
{
    /// <summary>
    ///     The <see cref="ConnectionType" /> of this <see cref="CompatibleHidDevice" />.
    /// </summary>
    ConnectionType? Connection { get; }

    /// <summary>
    ///     The <see cref="InputDeviceType" /> of this <see cref="CompatibleHidDevice" />.
    /// </summary>
    InputDeviceType DeviceType { get; set; }

    /// <summary>
    ///     The serial number (MAC address) of this <see cref="CompatibleHidDevice" />.
    /// </summary>
    PhysicalAddress Serial { get; }

    /// <summary>
    ///     The <see cref="CompatibleHidDeviceFeatureSet" /> flags this device has been created with.
    /// </summary>
    CompatibleHidDeviceFeatureSet FeatureSet { get; }

    /// <summary>
    ///     Metrics of how many input reports were read in a second.
    /// </summary>
    int ReportsPerSecondRead { get; }

    /// <summary>
    ///     Metrics of how many input reports were processed in a second.
    /// </summary>
    int ReportsPerSecondProcessed { get; }

    /// <summary>
    ///     Gets whether <see cref="InputReportAvailable" /> will be invoked in the processing loop.
    /// </summary>
    bool IsInputReportAvailableInvoked { get; }

    /// <summary>
    ///     True if device originates from a software device.
    /// </summary>
    bool IsVirtual { get; set; }

    /// <summary>
    ///     The Instance ID of this device.
    /// </summary>
    string InstanceId { get; set; }

    /// <summary>
    ///     The path (symbolic link) of the device instance.
    /// </summary>
    string Path { get; set; }

    /// <summary>
    ///     Device description.
    /// </summary>
    string Description { get; set; }

    /// <summary>
    ///     Device friendly name.
    /// </summary>
    string DisplayName { get; set; }

    /// <summary>
    ///     The Instance ID of the parent device.
    /// </summary>
    string ParentInstance { get; set; }

    /// <summary>
    ///     HID Device Attributes.
    /// </summary>
    Hid.HiddAttributes Attributes { get; set; }

    /// <summary>
    ///     HID Device Capabilities.
    /// </summary>
    Hid.HidpCaps Capabilities { get; set; }

    /// <summary>
    ///     The manufacturer string.
    /// </summary>
    string ManufacturerString { get; set; }

    /// <summary>
    ///     The product name.
    /// </summary>
    string ProductString { get; set; }

    /// <summary>
    ///     The serial number, if any.
    /// </summary>
    string SerialNumberString { get; set; }

    /// <summary>
    ///     Is this device currently open (for reading, writing).
    /// </summary>
    bool IsOpen { get; }

    /// <summary>
    ///     Fired when this device has been disconnected/unplugged.
    /// </summary>
    event Action<CompatibleHidDevice> Disconnected;

    /// <summary>
    ///     Fired when a new input report is read for further processing.
    /// </summary>
    event Action<CompatibleHidDevice, CompatibleHidDeviceInputReport> InputReportAvailable;

    void Dispose();
    string ToString();
    bool Equals(HidDevice other);
    bool Equals(object obj);
    int GetHashCode();

    /// <summary>
    ///     Access device and keep handle open until <see cref="HidDevice.CloseDevice" /> is called or object gets disposed.
    /// </summary>
    void OpenDevice();

    void CloseDevice();
}

public abstract partial class CompatibleHidDevice : ICompatibleHidDevice
{
    /// <summary>
    ///     Craft a new specific input device depending on supplied <see cref="InputDeviceType" />.
    /// </summary>
    /// <param name="deviceType">The <see cref="InputDeviceType" /> to base the new device on.</param>
    /// <param name="source">The source <see cref="HidDevice" /> to copy from.</param>
    /// <param name="featureSet">The <see cref="CompatibleHidDeviceFeatureSet" /> flags to use to create this device.</param>
    /// <param name="services">The <see cref="IServiceProvider" />.</param>
    /// <returns>The new <see cref="CompatibleHidDevice" /> instance.</returns>
    public static CompatibleHidDevice CreateFrom(InputDeviceType deviceType, HidDevice source,
        CompatibleHidDeviceFeatureSet featureSet, IServiceProvider services)
    {
        switch (deviceType)
        {
            case InputDeviceType.DualShock4:
                return new DualShock4CompatibleHidDevice(deviceType, source, featureSet, services);
            case InputDeviceType.DualSense:
                return new DualSenseCompatibleHidDevice(deviceType, source, featureSet, services);
            case InputDeviceType.SwitchPro:
                return new SwitchProCompatibleHidDevice(deviceType, source, featureSet, services);
            case InputDeviceType.JoyConL:
            case InputDeviceType.JoyConR:
                return new JoyConCompatibleHidDevice(deviceType, source, featureSet, services);
            default:
                throw new ArgumentOutOfRangeException(nameof(deviceType), deviceType, null);
        }
    }
}