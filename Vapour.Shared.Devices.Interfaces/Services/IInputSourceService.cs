﻿namespace Vapour.Shared.Devices.Services;

public interface IInputSourceService
{
    void Stop();
    Task Start();
    event Action InputSourceListReady;
}