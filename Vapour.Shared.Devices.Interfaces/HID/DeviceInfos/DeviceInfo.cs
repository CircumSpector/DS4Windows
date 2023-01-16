using Vapour.Shared.Devices.Services.ControllerEnumerators;

namespace Vapour.Shared.Devices.HID.DeviceInfos;
public abstract class DeviceInfo
{
    public abstract int Vid { get; }
    public abstract int Pid { get; }
    public abstract string Name { get; }
    public abstract InputDeviceType DeviceType { get; }
    public virtual CompatibleHidDeviceFeatureSet FeatureSet => CompatibleHidDeviceFeatureSet.Default;
    public virtual HidDeviceOverWinUsbEndpoints WinUsbEndpoints { get; } = null;
    public virtual bool IsDongle => false;
    public virtual bool IsLeftDevice => false;
    public virtual bool IsRightDevice => false;
}
