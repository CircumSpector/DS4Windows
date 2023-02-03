﻿namespace Vapour.Shared.Devices.HID.InputTypes.DualShock4.In;

[Flags]
public enum DualShock4Buttons1 : byte
{
    Square = 0x10,
    Cross = 0x20,
    Circle = 0x40,
    Triangle = 0x80
}

[Flags]
public enum DualShock4Buttons2 : byte
{
    L1 = 0x01,
    R1 = 0x02,
    L2 = 0x04,
    R2 = 0x08,
    Create = 0x10,
    Options = 0x20,
    L3 = 0x40,
    R3 = 0x80
}

[Flags]
public enum DualShock4Buttons3 : byte
{
    Home = 0x01,
    Pad = 0x02,
}