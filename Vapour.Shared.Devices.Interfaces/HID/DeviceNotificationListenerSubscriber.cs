﻿namespace Vapour.Shared.Devices.Interfaces.HID;

public interface IDeviceNotificationListenerSubscriber
{
    event Action<string> DeviceArrived;
    event Action<string> DeviceRemoved;
}