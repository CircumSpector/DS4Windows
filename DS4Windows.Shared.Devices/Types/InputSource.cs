using System;
using DS4Windows.Shared.Devices.HID;
using JetBrains.Annotations;
using PropertyChanged;

namespace DS4Windows.Shared.Devices.Types;

/// <summary>
///     Represents a logical input source baked by a hardware <see cref="CompatibleHidDevice" />.
/// </summary>
public interface IInputSource
{
    CompatibleHidDevice PrimarySourceDevice { get; }
}

/// <summary>
///     Represents a logical input source baked by a two hardware <see cref="CompatibleHidDevice" />s.
/// </summary>
public interface ICompositeInputSource : IInputSource
{
    CompatibleHidDevice SecondarySourceDevice { get; }

    event Action SecondarySourceDeviceArrived;
}

public class InputSource : IInputSource
{
    internal InputSource(CompatibleHidDevice primarySource)
    {
        PrimarySourceDevice = primarySource;
    }

    /// <inheritdoc />
    public CompatibleHidDevice PrimarySourceDevice { get; }
}

[AddINotifyPropertyChangedInterface]
public class CompositeInputSource : InputSource, ICompositeInputSource
{
    internal CompositeInputSource(CompatibleHidDevice primarySource)
        : base(primarySource)
    {
    }

    /// <inheritdoc />
    public CompatibleHidDevice SecondarySourceDevice { get; internal set; }

    public event Action SecondarySourceDeviceArrived;

    [UsedImplicitly]
    private void OnSecondarySourceDeviceChanged()
    {
        SecondarySourceDeviceArrived?.Invoke();
    }
}