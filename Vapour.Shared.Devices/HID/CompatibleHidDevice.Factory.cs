using Vapour.Shared.Devices.HID.DeviceInfos;

namespace Vapour.Shared.Devices.HID;

public class DeviceFactory : IDeviceFactory
{
    private readonly List<DeviceInfo> _deviceInfos;
    private readonly List<ICompatibleHidDevice> _dummyDevices;
    private readonly IServiceProvider _serviceProvider;

    public DeviceFactory(
        List<DeviceInfo> deviceInfos,
        List<ICompatibleHidDevice> dummyDevices,
        IServiceProvider serviceProvider)
    {
        _deviceInfos = deviceInfos;
        _dummyDevices = dummyDevices;
        _serviceProvider = serviceProvider;
    }

    public DeviceInfo IsKnownDevice(int vid, int pid)
    {
        return _deviceInfos.SingleOrDefault(i => i.VendorId == vid && i.ProductId == pid);
    }

    public ICompatibleHidDevice CreateDevice(DeviceInfo deviceInfo, IHidDevice hidDevice)
    {
        ICompatibleHidDevice dummyDevice = _dummyDevices.SingleOrDefault(d =>
            d.KnownDevices.Any(i => i.VendorId == deviceInfo.VendorId && i.ProductId == deviceInfo.ProductId));

        ICompatibleHidDevice device = null;

        if (dummyDevice != null)
        {
            Type type = dummyDevice.GetType();
            device = (ICompatibleHidDevice)_serviceProvider.GetService(type);
            device?.Initialize(hidDevice, deviceInfo);
        }

        return device;
    }

    public static string DeviceInfoKey(int vid, int pid)
    {
        return $"{vid}::{pid}";
    }
}