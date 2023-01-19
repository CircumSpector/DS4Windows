﻿namespace Vapour.Shared.Devices.Services;
public class InputSourceDataSource : IInputSourceDataSource
{
    public List<IInputSource> InputSources { get; } = new();


    public event Action<IInputSource> InputSourceCreated;
    public event Action<IInputSource> InputSourceRemoved;

    public IInputSource GetByInputSourceKey(string inputSourceKey)
    {
        return InputSources.SingleOrDefault(i => i.InputSourceKey == inputSourceKey);
    }

    public void FireCreated(IInputSource inputSource)
    {
        InputSourceCreated?.Invoke(inputSource);
    }

    public void FireRemoved(IInputSource inputSource)
    {
        InputSourceRemoved?.Invoke(inputSource);
    }
}