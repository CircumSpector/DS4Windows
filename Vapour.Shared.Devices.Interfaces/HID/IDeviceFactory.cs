using Vapour.Shared.Devices.HID.DeviceInfos;

namespace Vapour.Shared.Devices.HID;

public interface IDeviceFactory
{
    IDeviceInfo IsKnownDevice(int vid, int pid);
    ICompatibleHidDevice CreateDevice(IDeviceInfo deviceInfo, IHidDevice hidDevice);
}