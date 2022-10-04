using System.Runtime.InteropServices;
using Windows.Win32.Foundation;
using Nefarius.Utilities.DeviceManagement.Extensions;
using Nefarius.Utilities.DeviceManagement.PnP;
using PInvoke;
using Vapour.Shared.Devices.Interfaces.DriverManagement;

namespace Vapour.Shared.Devices.DriverManagement;

public class ControllerDriverManagementService : IControllerDriverManagementService
{
    private const string tempDriverPath = "c:\\temp\\";
    private const string tempDriverInf = "existingcontroller.inf";
    private const string tempDriverFullPath = $"{tempDriverPath}{tempDriverInf}";
    private const int expectedErrorCode = 122; //expected overflow of some sort when asking without required length
    private readonly IWdiWrapper wdiWrapper;
    private Guid usbHubGuid = new("{F18A0E88-C30C-11D0-8815-00A0C906BED8}");

    public ControllerDriverManagementService(IWdiWrapper wdiWrapper)
    {
        this.wdiWrapper = wdiWrapper;
    }

    public void HideController(string controllerInstanceId)
    {
        var hubAndPort = GetHubAndPort(controllerInstanceId);
        var hubAndPath = GetHubPath(hubAndPort.HubDeviceId);

        var prepareDriverResult = PrepareDriver(controllerInstanceId);
        InstallDriver(prepareDriverResult);

        PnPDevice.GetDeviceByInstanceId(controllerInstanceId).ToUsbPnPDevice().CyclePort();
    }

    public void UnhideController(string controllerInstanceId)
    {
        //var foundDevice = GetDevice(controllerInstanceId);

        //var rebootRequired = false;
        //UpdateDriverForPlugAndPlayDevicesA(IntPtr.Zero, foundDevice.hardware_id, "c:\\windows\\inf\\input.inf",
        //    0, out rebootRequired);
    }

    private HubAndPort GetHubAndPort(string controllerInstanceId)
    {
        var hubInstanceIds = new List<string>();
        var hubIndex = 0;
        while (Devcon.FindByInterfaceGuid(usbHubGuid, out var path, out var hubInstanceId, hubIndex++))
            hubInstanceIds.Add(hubInstanceId.ToUpper());

        var hidDevice = PnPDevice.GetDeviceByInstanceId(controllerInstanceId);

        var device = hidDevice;
        var parentId = device.GetProperty<string>(DevicePropertyKey.Device_Parent);
        while (!string.IsNullOrEmpty(parentId))
        {
            device = PnPDevice.GetDeviceByInstanceId(parentId);
            if (hubInstanceIds.Contains(parentId.ToUpper()))
            {
                var hidLocationInfo = hidDevice.GetProperty<string>(DevicePropertyKey.Device_LocationInfo);
                var portNumber = Convert.ToInt32(hidLocationInfo.Split('.')[3]);
                return new HubAndPort
                {
                    HubDeviceId = device.DeviceId,
                    PortNumber = portNumber,
                    HidDevice = hidDevice
                };
            }

            parentId = device.GetProperty<string>(DevicePropertyKey.Device_Parent);
        }

        throw new Exception($"Could not find a parent hub with instance id {controllerInstanceId}");
    }

    private unsafe string GetHubPath(string hubId)
    {
        var controller = SetupApi.SetupDiGetClassDevs(usbHubGuid, null, IntPtr.Zero,
            SetupApi.GetClassDevsFlags.DIGCF_DEVICEINTERFACE | SetupApi.GetClassDevsFlags.DIGCF_PRESENT);

        var deviceInfoData = SetupApi.SP_DEVINFO_DATA.Create();
        var deviceInterfaceData = SetupApi.SP_DEVICE_INTERFACE_DATA.Create();
        SetupApi.SP_DEVICE_INTERFACE_DETAIL_DATA* deviceDetailData;

        const uint num1 = 256;
        var num2 = stackalloc char[(int)num1];
        string devicePath = null;
        var memberIndex = 0;
        while (devicePath == null && SetupApi.SetupDiEnumDeviceInfo(controller, memberIndex, ref deviceInfoData))
        {
            var success = SetupApi.SetupDiEnumDeviceInterfaces(controller, (SetupApi.SP_DEVINFO_DATA?)null,
                ref usbHubGuid, memberIndex,
                ref deviceInterfaceData);

            if (success)
            {
                var result = Windows.Win32.PInvoke.CM_Get_Device_IDW(deviceInfoData.DevInst, new PWSTR(num2), num1, 0u);
                if (result == 0)
                {
                    var deviceId = new string(num2);

                    if (deviceId == hubId)
                    {
                        deviceInterfaceData.Size = Marshal.SizeOf(deviceInterfaceData);

                        var requiredLength = Marshal.AllocHGlobal(2048);
                        success = SetupApi.SetupDiGetDeviceInterfaceDetail(
                            controller,
                            ref deviceInterfaceData,
                            IntPtr.Zero,
                            0,
                            requiredLength,
                            IntPtr.Zero);

                        var lastError = Marshal.GetLastWin32Error();

                        if (success || lastError == expectedErrorCode)
                        {
                            var requiredLengthValue = Marshal.ReadInt32(requiredLength);
                            deviceDetailData =
                                (SetupApi.SP_DEVICE_INTERFACE_DETAIL_DATA*)Marshal.AllocHGlobal(
                                    requiredLengthValue);
                            deviceDetailData->cbSize = Marshal.SizeOf<SetupApi.SP_DEVICE_INTERFACE_DETAIL_DATA>();

                            success = SetupApi.SetupDiGetDeviceInterfaceDetail(
                                controller,
                                ref deviceInterfaceData,
                                (IntPtr)deviceDetailData,
                                requiredLengthValue,
                                requiredLength,
                                IntPtr.Zero
                            );

                            if (success)
                                devicePath = SetupApi.SP_DEVICE_INTERFACE_DETAIL_DATA.GetDevicePath(deviceDetailData);

                            Marshal.DestroyStructure<SetupApi.SP_DEVICE_INTERFACE_DETAIL_DATA>(
                                (IntPtr)deviceDetailData);
                        }

                        Marshal.FreeHGlobal(requiredLength);
                    }
                }
            }

            memberIndex++;
        }

        if (devicePath == null)
            throw new Exception($"Could not get the device path to the usb hub with instance id {hubId}");

        return devicePath;
    }

    private PrepareDriverResult PrepareDriver(string controllerInstanceId)
    {
        return wdiWrapper.PrepareDriver(controllerInstanceId);
    }

    private static void InstallDriver(PrepareDriverResult prepareDriverResult)
    {
        var result = Devcon.Update(prepareDriverResult.HardwareId, prepareDriverResult.InfPath, out var rebootRequired);
        if (!result)
            throw new Exception($"Could not update the driver for hardware id {prepareDriverResult.HardwareId}");
    }

    private class HubAndPort
    {
        public string HubDeviceId { get; init; }
        public int PortNumber { get; set; }
        public PnPDevice HidDevice { get; set; }
    }
}