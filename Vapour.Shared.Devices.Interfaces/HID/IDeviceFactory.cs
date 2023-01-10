using Vapour.Shared.Devices.HID.DeviceInfos;

namespace Vapour.Shared.Devices.HID;

public interface IDeviceFactory
{
    DeviceInfo IsKnownDevice(int vid, int pid);
    ICompatibleHidDevice CreateDevice(DeviceInfo deviceInfo, IHidDevice hidDevice);
}