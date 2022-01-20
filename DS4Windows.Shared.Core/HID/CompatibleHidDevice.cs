using System;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using DS4Windows.Shared.Core.Util;
using JetBrains.Annotations;
using MethodTimer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Nefarius.Utilities.DeviceManagement.PnP;
using PInvoke;

namespace DS4Windows.Shared.Core.HID
{
    /// <summary>
    ///     Represents a <see cref="HidDevice" /> which is a compatible input device.
    /// </summary>
    public abstract partial class CompatibleHidDevice : HidDevice
    {
        protected const string SonyWirelessAdapterFriendlyName = "DUALSHOCK®4 USB Wireless Adaptor";
        protected static readonly Guid UsbDeviceClassGuid = Guid.Parse("{88BAE032-5A81-49f0-BC3D-A4FF138216D6}");

        protected static readonly Guid UsbCompositeDeviceClassGuid =
            Guid.Parse("{36fc9e60-c465-11cf-8056-444553540000}");

        protected static readonly Guid BluetoothDeviceClassGuid = Guid.Parse("{e0cbf06c-cd8b-4647-bb8a-263b43f0f974}");

        protected readonly Channel<byte[]> InputReportChannel = Channel.CreateBounded<byte[]>(5);

        /// <summary>
        ///     Managed input report array.
        /// </summary>
        protected byte[] InputReportArray;

        /// <summary>
        ///     Unmanaged input report buffer.
        /// </summary>
        protected IntPtr InputReportBuffer;

        private Task inputReportProcessor;

        private Task inputReportReader;

        private CancellationTokenSource inputReportToken = new();

        [Time]
        protected CompatibleHidDevice(InputDeviceType deviceType, HidDevice source,
            CompatibleHidDeviceFeatureSet featureSet, IServiceProvider serviceProvider)
        {
            //
            // This makes this instance independent
            // 
            source.DeepCloneTo(this);

            Services = serviceProvider;
            DeviceType = deviceType;
            FeatureSet = featureSet;

            //
            // Grab new logger
            // 
            Logger = Services.GetRequiredService<ILogger<CompatibleHidDevice>>();

            if (!Connection.HasValue)
                throw new ArgumentException("Couldn't determine connection type.");

            if (FeatureSet != CompatibleHidDeviceFeatureSet.Default)
                Logger.LogInformation("Controller {Device} is using custom feature set {Feature}",
                    this, FeatureSet);

            //
            // Open handle
            // 
            OpenDevice();
        }

        /// <summary>
        ///     The <see cref="InputDeviceType" /> of this <see cref="CompatibleHidDevice" />.
        /// </summary>
        public InputDeviceType DeviceType { get; init; }

        /// <summary>
        ///     The <see cref="ConnectionType" /> of this <see cref="CompatibleHidDevice" />.
        /// </summary>
        public ConnectionType? Connection
        {
            get
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

                        if (children.Count != 2)
                            return ConnectionType.Usb;

                        var audioDevice = PnPDevice.GetDeviceByInstanceId(children.First());

                        var friendlyName = audioDevice.GetProperty<string>(DevicePropertyDevice.FriendlyName);

                        if (string.IsNullOrEmpty(friendlyName))
                            return ConnectionType.Usb;

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
        }

        /// <summary>
        ///     The serial number (MAC address) of this <see cref="CompatibleHidDevice" />.
        /// </summary>
        public PhysicalAddress Serial { get; protected set; }

        /// <summary>
        ///     The <see cref="CompatibleHidDeviceFeatureSet" /> flags this device has been created with.
        /// </summary>
        public CompatibleHidDeviceFeatureSet FeatureSet { get; }

        /// <summary>
        ///     Service provider for injected services.
        /// </summary>
        protected IServiceProvider Services { get; }

        /// <summary>
        ///     Logger instance.
        /// </summary>
        protected ILogger<CompatibleHidDevice> Logger { get; }

        /// <summary>
        ///     Fired when this device has been disconnected/unplugged.
        /// </summary>
        public event Action Disconnected;

        /// <summary>
        ///     Process the input report read from the device.
        /// </summary>
        /// <param name="report">The raw report buffer.</param>
        protected abstract void ProcessInputReport(byte[] report);

        /// <summary>
        ///     Start the asynchronous input report reading logic.
        /// </summary>
        protected void StartInputReportReader()
        {
            if (inputReportToken.Token.IsCancellationRequested)
                inputReportToken = new CancellationTokenSource();

            inputReportReader = Task.Run(ReadInputReportLoop, inputReportToken.Token);
            inputReportProcessor = Task.Run(ProcessInputReportLoop, inputReportToken.Token);
        }

        /// <summary>
        ///      Stop the asynchronous input report reading logic.
        /// </summary>
        protected void StopInputReportReader()
        {
            inputReportToken.Cancel();

            Task.WaitAll(inputReportReader, inputReportProcessor);
        }

        /// <summary>
        ///     Continuous input report processing thread.
        /// </summary>
        protected async void ProcessInputReportLoop()
        {
            Logger.LogDebug("Started input report processing thread");

            try
            {
                while (!inputReportToken.IsCancellationRequested)
                {
                    if (!await InputReportChannel.Reader.WaitToReadAsync()) continue;

                    var buffer = await InputReportChannel.Reader.ReadAsync();

                    //
                    // Implementation depends on derived object
                    // 
                    ProcessInputReport(buffer);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Fatal failure in input report processing");
            }
        }

        /// <summary>
        ///     Continuous input report reader thread.
        /// </summary>
        protected async void ReadInputReportLoop()
        {
            Logger.LogDebug("Started input report reading thread");

            try
            {
                while (!inputReportToken.IsCancellationRequested)
                {
                    ReadInputReport(InputReportBuffer, InputReportArray.Length, out var written);

                    Marshal.Copy(InputReportBuffer, InputReportArray, 0, written);

                    await InputReportChannel.Writer.WriteAsync(InputReportArray, inputReportToken.Token);
                }
            }
            catch (Win32Exception win32)
            {
                if (win32.NativeErrorCode != Win32ErrorCode.ERROR_DEVICE_NOT_CONNECTED) throw;

                inputReportToken.Cancel();

                Disconnected?.Invoke();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Fatal failure in input report reading");
            }
        }

        [Time]
        [CanBeNull]
        protected PhysicalAddress ReadSerial(byte featureId)
        {
            PhysicalAddress serial = null;

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

        public override void Dispose()
        {
            StopInputReportReader();

            base.Dispose();
        }

        public override string ToString()
        {
            return $"{DisplayName} ({Serial}) via {Connection}";
        }
    }
}