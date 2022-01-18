using System;

namespace DS4Windows.Shared.Core.HID
{
    public interface IDeviceNotificationListenerSubscriber
    {
        event Action<string> DeviceArrived;
        event Action<string> DeviceRemoved;
    }
}
