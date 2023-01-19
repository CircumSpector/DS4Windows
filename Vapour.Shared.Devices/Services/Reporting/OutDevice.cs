﻿using Vapour.Shared.Common.Types;
using Vapour.Shared.Devices.HID;

namespace Vapour.Shared.Devices.Services.Reporting;

internal abstract class OutDevice : IOutDevice
{
    protected bool IsConnected;

    public abstract void ConvertAndSendReport(InputSourceReport state, int device = 0);

    public abstract void Connect();

    public abstract void Disconnect();

    public abstract void ResetState(bool submit = true);

    public abstract OutputDeviceType GetDeviceType();

    public abstract void RemoveFeedbacks();

    public abstract void RemoveFeedback(int inIdx);
}