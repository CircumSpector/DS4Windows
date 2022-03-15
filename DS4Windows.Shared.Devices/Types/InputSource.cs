using System.ComponentModel;
using System.Runtime.CompilerServices;
using DS4Windows.Shared.Devices.HID;
using JetBrains.Annotations;

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

public class CompositeInputSource : InputSource, ICompositeInputSource, INotifyPropertyChanged
{
    internal CompositeInputSource(CompatibleHidDevice primarySource)
        : base(primarySource)
    {
    }

    /// <inheritdoc />
    public CompatibleHidDevice SecondarySourceDevice { get; internal set; }

    public event PropertyChangedEventHandler PropertyChanged;

    [UsedImplicitly]
    [NotifyPropertyChangedInvocator]
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}