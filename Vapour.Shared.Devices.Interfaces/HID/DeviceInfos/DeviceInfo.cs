using Vapour.Shared.Devices.Services.ControllerEnumerators;

namespace Vapour.Shared.Devices.HID.DeviceInfos;
public abstract class DeviceInfo : IDeviceInfo
{
    public abstract int Vid { get; }
    public abstract int Pid { get; }
    public abstract string Name { get; }
    public abstract InputDeviceType DeviceType { get; }
    public virtual CompatibleHidDeviceFeatureSet FeatureSet { get; } = CompatibleHidDeviceFeatureSet.Default;
    public virtual HidDeviceOverWinUsbEndpoints WinUsbEndpoints { get; } = null;
    public virtual bool IsDongle { get; } = false;
}
