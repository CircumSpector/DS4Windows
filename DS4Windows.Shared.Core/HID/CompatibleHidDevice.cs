using System;
using System.Linq;
using System.Net.NetworkInformation;
using DS4Windows.Shared.Core.HID.Devices;
using DS4Windows.Shared.Core.Util;
using MethodTimer;
using Nefarius.Utilities.DeviceManagement.PnP;

namespace DS4Windows.Shared.Core.HID
{
    /// <summary>
    ///     Represents a <see cref="HidDevice" /> which is a compatible input device.
    /// </summary>
    public abstract class CompatibleHidDevice : HidDevice
    {
        protected const string SonyWirelessAdapterFriendlyName = "DUALSHOCK®4 USB Wireless Adaptor";
        protected static readonly Guid UsbDeviceClassGuid = Guid.Parse("{88BAE032-5A81-49f0-BC3D-A4FF138216D6}");

        protected static readonly Guid UsbCompositeDeviceClassGuid =
            Guid.Parse("{36fc9e60-c465-11cf-8056-444553540000}");

        protected static readonly Guid BluetoothDeviceClassGuid = Guid.Parse("{e0cbf06c-cd8b-4647-bb8a-263b43f0f974}");

        protected CompatibleHidDevice(HidDevice source)
        {
            source.DeepCloneTo(this);

            var connection = LookupConnectionType();

            if (!connection.HasValue)
                throw new ArgumentException("Couldn't determine connection type.");

            Connection = connection.Value;

            PopulateSerial();
        }

        /// <summary>
        ///     The <see cref="InputDeviceType" /> of this <see cref="CompatibleHidDevice" />.
        /// </summary>
        public InputDeviceType DeviceType { get; init; }

        /// <summary>
        ///     The <see cref="ConnectionType" /> of this <see cref="CompatibleHidDevice" />.
        /// </summary>
        public ConnectionType Connection { get; init; }

        /// <summary>
        ///     The serial number (MAC address) of this <see cref="CompatibleHidDevice" />.
        /// </summary>
        public PhysicalAddress Serial { get; protected set; }

        /// <summary>
        ///     Determine the connection type of this device.
        /// </summary>
        /// <returns>The <see cref="ConnectionType" /> detected, or null otherwise.</returns>
        [Time]
        private ConnectionType? LookupConnectionType()
        {
            var device = PnPDevice.GetDeviceByInterfaceId(Path);

            //
            // Walk up device tree
            // 
            while (device is not null)
            {
                var deviceClass = device.GetProperty<Guid>(DevicePropertyDevice.ClassGuid);

                //
                // Parent is Bluetooth device
                // 
                if (Equals(deviceClass, BluetoothDeviceClassGuid))
                    return ConnectionType.Bluetooth;

                //
                // USB or via Sony Wireless Adapter
                // 
                if (Equals(deviceClass, UsbCompositeDeviceClassGuid))
                {
                    //
                    // Check if we find the composite audio device
                    // 
                    var children = device.GetProperty<string[]>(DevicePropertyDevice.Children).ToList();

                    if (children.Count == 1)
                        return ConnectionType.Usb;

                    var audioDevice = PnPDevice.GetDeviceByInstanceId(children.First());

                    var friendlyName = audioDevice.GetProperty<string>(DevicePropertyDevice.FriendlyName);

                    //
                    // Match friendly name reported by Wireless Adapter
                    // 
                    return friendlyName.Equals(SonyWirelessAdapterFriendlyName, StringComparison.OrdinalIgnoreCase)
                        ? ConnectionType.SonyWirelessAdapter
                        : ConnectionType.Usb;
                }

                var parentId = device.GetProperty<string>(DevicePropertyDevice.Parent);

                if (parentId is null)
                    break;

                device = PnPDevice.GetDeviceByInstanceId(parentId);
            }

            return null;
        }

        /// <summary>
        ///     Retrieves <see cref="Serial" /> with device-specific methods.
        /// </summary>
        public abstract void PopulateSerial();

        [Time]
        protected PhysicalAddress ReadSerial(byte featureId = 0x12)
        {
            var serial = new PhysicalAddress(new byte[] { 0, 0, 0, 0, 0, 0 });

            if (Capabilities.InputReportByteLength == 64)
            {
                var buffer = new byte[64];
                buffer[0] = featureId;

                if (ReadFeatureData(buffer))
                    serial = PhysicalAddress.Parse(
                        $"{buffer[6]:X02}:{buffer[5]:X02}:{buffer[4]:X02}:{buffer[3]:X02}:{buffer[2]:X02}:{buffer[1]:X02}"
                    );
            }
            else
            {
                try
                {
                    if (!string.IsNullOrEmpty(SerialNumberString)) serial = PhysicalAddress.Parse(SerialNumberString);
                }
                catch
                {
                    serial = GenerateFakeHwSerial();
                }
            }

            return serial;
        }

        /// <summary>
        ///     Generate <see cref="Serial" /> from <see cref="HidDevice.Path" />.
        /// </summary>
        /// <returns></returns>
        protected PhysicalAddress GenerateFakeHwSerial()
        {
            var address = string.Empty;

            // Substring: \\?\hid#vid_054c&pid_09cc&mi_03#7&1f882A25&0&0001#{4d1e55b2-f16f-11cf-88cb-001111000030} -> \\?\hid#vid_054c&pid_09cc&mi_03#7&1f882A25&0&0001#
            var endPos = Path.LastIndexOf('{');
            if (endPos < 0)
                endPos = Path.Length;

            // String array: \\?\hid#vid_054c&pid_09cc&mi_03#7&1f882A25&0&0001# -> [0]=\\?\hidvid_054c, [1]=pid_09cc, [2]=mi_037, [3]=1f882A25, [4]=0, [5]=0001
            var devPathItems = Path.Substring(0, endPos).Replace("#", "").Replace("-", "").Replace("{", "")
                .Replace("}", "").Split('&');

            address = devPathItems.Length switch
            {
                >= 3 => devPathItems[^3].ToUpper() // 1f882A25
                        + devPathItems[^2].ToUpper() // 0
                        + devPathItems[^1].TrimStart('0').ToUpper(),
                // Device and usb hub and port identifiers missing in devicePath string. Fallback to use vendor and product ID values and 
                // take a number from the last part of the devicePath. Hopefully the last part is a usb port number as it usually should be.
                >= 1 => Attributes.VendorId.ToString("X4") + Attributes.ProductId.ToString("X4") +
                        devPathItems[^1].TrimStart('0').ToUpper(),
                _ => address
            };

            if (string.IsNullOrEmpty(address)) return PhysicalAddress.Parse(address);

            address = address.PadRight(12, '0');
            address =
                $"{address[0]}{address[1]}:{address[2]}{address[3]}:{address[4]}{address[5]}:{address[6]}{address[7]}:{address[8]}{address[9]}:{address[10]}{address[11]}";

            return PhysicalAddress.Parse(address);
        }

        public override string ToString()
        {
            return $"{DisplayName} ({Serial}) via {Connection}";
        }

        /// <summary>
        ///     Craft a new specific input device depending on supplied <see cref="InputDeviceType" />.
        /// </summary>
        /// <param name="deviceType">The <see cref="InputDeviceType" /> to base the new device on.</param>
        /// <param name="source">The source <see cref="HidDevice" /> to copy from.</param>
        /// <returns>The new <see cref="CompatibleHidDevice" /> instance.</returns>
        [Time]
        public static CompatibleHidDevice Create(InputDeviceType deviceType, HidDevice source)
        {
            switch (deviceType)
            {
                case InputDeviceType.DualShock4:
                    return new DualShock4CompatibleHidDevice(source) { DeviceType = deviceType };
                case InputDeviceType.DualSense:
                    return new DualSenseCompatibleHidDevice(source) { DeviceType = deviceType };
                case InputDeviceType.SwitchPro:
                    return new SwitchProCompatibleHidDevice(source) { DeviceType = deviceType };
                case InputDeviceType.JoyConL:
                case InputDeviceType.JoyConR:
                    return new JoyConCompatibleHidDevice(source) { DeviceType = deviceType };
                default:
                    throw new ArgumentOutOfRangeException(nameof(deviceType), deviceType, null);
            }
        }
    }
}