using Vapour.Shared.Devices.HID;

namespace Vapour.Shared.Devices.Services;

/// <summary>
///     Represents a logical input source baked by a hardware <see cref="ICompatibleHidDevice" />.
/// </summary>
public interface IInputSource
{
    /// <summary>
    ///     The primary source <see cref="ICompatibleHidDevice" />.
    /// </summary>
    ICompatibleHidDevice PrimarySourceDevice { get; }
}

/// <summary>
///     Represents a logical input source baked by a two hardware <see cref="ICompatibleHidDevice" />s.
/// </summary>
public interface ICompositeInputSource : IInputSource
{
    /// <summary>
    ///     The secondary source <see cref="ICompatibleHidDevice" />.
    /// </summary>
    ICompatibleHidDevice SecondarySourceDevice { get; }
}