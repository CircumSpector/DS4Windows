﻿using Vapour.Shared.Devices.HID;

namespace Vapour.Shared.Devices.Output;

public abstract class OutDevice : IOutDevice
{
    protected bool IsConnected;

    public abstract void ConvertAndSendReport(CompatibleHidDeviceInputReport state, int device = 0);

    public abstract void Connect();

    public abstract void Disconnect();

    public abstract void ResetState(bool submit = true);

    public abstract string GetDeviceType();

    public abstract void RemoveFeedbacks();

    public abstract void RemoveFeedback(int inIdx);
}