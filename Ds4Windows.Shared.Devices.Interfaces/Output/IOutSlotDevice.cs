using System;
using DS4Windows.Shared.Common.Types;

namespace DS4Windows.Shared.Devices.Output;

public interface IOutSlotDevice
{
    public enum AttachedStatus : uint
    {
        UnAttached = 0,
        Attached = 1
    }

    public enum InputBound : uint
    {
        Unbound = 0,
        Bound = 1
    }

    public enum ReserveStatus : uint
    {
        Dynamic = 0,
        Permanent = 1
    }

    /// <summary>
    ///     Desired device type for a permanently connected slot
    /// </summary>
    OutputDeviceType PermanentType { get; set; }

    int Index { get; }

    /// <summary>
    ///     Connection status of virtual output controller
    /// </summary>
    AttachedStatus CurrentAttachedStatus { get; }

    /// <summary>
    ///     Reference to output controller
    /// </summary>
    IOutDevice OutputDevice { get; }

    /// <summary>
    ///     Flag stating the connection preference of an output controller
    /// </summary>
    ReserveStatus CurrentReserveStatus { get; set; }

    /// <summary>
    ///     Whether an input controller is associated with the slot
    /// </summary>
    InputBound CurrentInputBound { get; set; }

    /// <summary>
    ///     Device type of the current output controller
    /// </summary>
    OutputDeviceType CurrentType { get; set; }

    event EventHandler CurrentReserveStatusChanged;
    event EventHandler CurrentInputBoundChanged;
    event EventHandler PermanentTypeChanged;
    void AttachedDevice(IOutDevice outputDevice, OutputDeviceType contType);
    void DetachDevice();
}