﻿using Vapour.Shared.Common.Types;

namespace Vapour.Shared.Devices.Services.Reporting;

internal abstract class OutDevice : IOutDevice
{
    protected bool IsConnected { get; set; }

    public abstract void ConvertAndSendReport(InputSourceFinalReport state, int device = 0);

    public abstract void Connect();

    public abstract void Disconnect();

    public abstract void ResetState(bool submit = true);

    public abstract OutputDeviceType GetDeviceType();
}