using Nefarius.Utilities.DeviceManagement.Extensions;
using Nefarius.Utilities.DeviceManagement.PnP;
using Vapour.Shared.Devices.Interfaces.DriverManagement;

namespace Vapour.Shared.Devices.DriverManagement;

public class ControllerDriverManagementService : IControllerDriverManagementService
{
    private const string tempDriverPath = "c:\\temp\\";
    private const string tempDriverInf = "existingcontroller.inf";
    private const string tempDriverFullPath = $"{tempDriverPath}{tempDriverInf}";
    private const int expectedErrorCode = 122; //expected overflow of some sort when asking without required length
    private readonly IWdiWrapper wdiWrapper;

    public ControllerDriverManagementService(IWdiWrapper wdiWrapper)
    {
        this.wdiWrapper = wdiWrapper;
    }

    public void HideController(string controllerInstanceId)
    {
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
}