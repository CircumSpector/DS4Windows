using System.Collections.Generic;
using System.Collections.ObjectModel;
using DS4Windows.Shared.Devices.HID;

namespace DS4Windows.Shared.Devices.Types;

/// <summary>
///     Virtual composite input device with one or more source <see cref="CompatibleHidDevice" />s.
/// </summary>
public interface IInputSource
{
    /// <summary>
    ///     One or more <see cref="CompatibleHidDevice" />s.
    /// </summary>
    IReadOnlyList<CompatibleHidDevice> SourceDevices { get; }
}

/// <summary>
///     Virtual composite input device with one or more source <see cref="CompatibleHidDevice" />s.
/// </summary>
public class InputSource : IInputSource
{
    private readonly List<CompatibleHidDevice> sourceDevices;

    internal InputSource()
    {
        sourceDevices = new List<CompatibleHidDevice>();

        SourceDevices = new ReadOnlyCollection<CompatibleHidDevice>(sourceDevices);
    }

    /// <inheritdoc />
    public IReadOnlyList<CompatibleHidDevice> SourceDevices { get; }
}