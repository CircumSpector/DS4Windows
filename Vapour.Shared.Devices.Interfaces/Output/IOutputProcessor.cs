﻿using Vapour.Shared.Devices.HID;

namespace Vapour.Shared.Devices.Output;

public interface IOutputProcessor
{
    ICompatibleHidDevice HidDevice { get; }
    void StartOutputProcessing();
    void StopOutputProcessing();
}