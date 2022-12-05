using System.ComponentModel;
using System.Runtime.CompilerServices;

using JetBrains.Annotations;

using Vapour.Shared.Devices.HID;
using Vapour.Shared.Devices.Services;

namespace Vapour.Shared.Devices.Types;

/// <summary>
///     Represents a logical input source baked by a hardware <see cref="CompatibleHidDevice" />.
/// </summary>
internal class InputSource : IInputSource
{
    internal InputSource(ICompatibleHidDevice primarySource)
    {
        PrimarySourceDevice = primarySource;
    }

    /// <inheritdoc />
    public ICompatibleHidDevice PrimarySourceDevice { get; }
}

/// <summary>
///     Represents a logical input source baked by two hardware <see cref="CompatibleHidDevice" />s.
/// </summary>
// ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
internal class CompositeInputSource : InputSource, ICompositeInputSource, INotifyPropertyChanged
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