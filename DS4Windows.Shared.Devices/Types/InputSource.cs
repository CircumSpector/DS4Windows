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
    /// <summary>
    ///     The primary source <see cref="CompatibleHidDevice"/>.
    /// </summary>
    ICompatibleHidDevice PrimarySourceDevice { get; }
}

/// <summary>
///     Represents a logical input source baked by a two hardware <see cref="CompatibleHidDevice" />s.
/// </summary>
public interface ICompositeInputSource : IInputSource
{
    /// <summary>
    ///     The secondary source <see cref="CompatibleHidDevice"/>.
    /// </summary>
    ICompatibleHidDevice SecondarySourceDevice { get; }
}

/// <summary>
///     Represents a logical input source baked by a hardware <see cref="CompatibleHidDevice" />.
/// </summary>
public class InputSource : IInputSource
{
    internal InputSource(ICompatibleHidDevice primarySource)
    {
        PrimarySourceDevice = primarySource;
    }

    /// <inheritdoc />
    public ICompatibleHidDevice PrimarySourceDevice { get; }
}

/// <summary>
///     Represents a logical input source baked by a two hardware <see cref="CompatibleHidDevice" />s.
/// </summary>
public class CompositeInputSource : InputSource, ICompositeInputSource, INotifyPropertyChanged
{
    internal CompositeInputSource(ICompatibleHidDevice primarySource)
        : base(primarySource)
    {
    }

    /// <inheritdoc />
    public ICompatibleHidDevice SecondarySourceDevice { get; internal set; }

    public event PropertyChangedEventHandler PropertyChanged;

    [UsedImplicitly]
    [NotifyPropertyChangedInvocator]
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}