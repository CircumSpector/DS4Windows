﻿namespace Vapour.Shared.Devices.HID;

public interface IDeviceNotificationListenerSubscriber
{
    event Action<string> DeviceArrived;
    event Action<string> DeviceRemoved;
}