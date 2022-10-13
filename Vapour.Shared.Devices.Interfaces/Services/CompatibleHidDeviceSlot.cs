using JetBrains.Annotations;

using Vapour.Shared.Devices.HID;

namespace Vapour.Shared.Devices.Services;
/// <summary>
///     Describes a controller slot and the <see cref="CompatibleHidDevice" /> associated with it.
/// </summary>
public sealed class CompatibleHidDeviceSlot
{
    public CompatibleHidDeviceSlot(int slotIndex)
    {
        SlotIndex = slotIndex;
    }

    /// <summary>
    ///     The zero-based slot index.
    /// </summary>
    public int SlotIndex { get; init; }

    /// <summary>
    ///     Is this slot occupied with a <see cref="Device" />?
    /// </summary>
    public bool IsOccupied { get; internal set; }

    /// <summary>
    ///     The <see cref="CompatibleHidDevice" /> occupying this slot.
    /// </summary>
    [CanBeNull]
    public ICompatibleHidDevice Device { get; internal set; }
}
