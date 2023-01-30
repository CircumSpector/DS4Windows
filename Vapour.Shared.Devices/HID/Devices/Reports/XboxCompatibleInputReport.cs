using System.Runtime.InteropServices;

using Vapour.Shared.Devices.HID.InputTypes;
using Vapour.Shared.Devices.HID.InputTypes.Xbox;

namespace Vapour.Shared.Devices.HID.Devices.Reports;

public class XboxCompatibleInputReport : InputSourceReport
{
    public override InputAxisType AxisScaleInputType => InputAxisType.Xbox;

    public override void Parse(ReadOnlySpan<byte> input)
    {
        XboxButtons buttons = (XboxButtons)MemoryMarshal.Read<ushort>(input.Slice(11, 2));

        Triangle = buttons.HasFlag(XboxButtons.Y);
        Circle = buttons.HasFlag(XboxButtons.B);
        Cross = buttons.HasFlag(XboxButtons.A);
        Square = buttons.HasFlag(XboxButtons.X);

        LeftShoulder = buttons.HasFlag(XboxButtons.L1);
        RightShoulder = buttons.HasFlag(XboxButtons.R1);

        Options = buttons.HasFlag(XboxButtons.Start);
        Share = buttons.HasFlag(XboxButtons.Back);
        PS = buttons.HasFlag(XboxButtons.Xbox);

        SetDPad(buttons);

        LeftThumb = buttons.HasFlag(XboxButtons.L3);
        RightThumb = buttons.HasFlag(XboxButtons.R3);

        LeftTrigger = input[13];
        RightTrigger = input[14];

        LeftThumbX = MemoryMarshal.Read<short>(input.Slice(15, 2));
        LeftThumbY = MemoryMarshal.Read<short>(input.Slice(17, 2));
        RightThumbX = MemoryMarshal.Read<short>(input.Slice(19, 2));
        RightThumbY = MemoryMarshal.Read<short>(input.Slice(21, 2));
    }

    private void SetDPad(XboxButtons buttons0)
    {
        if (buttons0.HasFlag(XboxButtons.DpadDown))
        {
            if (buttons0.HasFlag(XboxButtons.DpadLeft))
            {
                DPad = DPadDirection.SouthWest;
            }
            else if (buttons0.HasFlag(XboxButtons.DpadRight))
            {
                DPad = DPadDirection.SouthEast;
            }
            else
            {
                DPad = DPadDirection.South;
            }
        }
        else if (buttons0.HasFlag(XboxButtons.DpadLeft))
        {
            if (buttons0.HasFlag(XboxButtons.DpadUp))
            {
                DPad = DPadDirection.NorthWest;
            }
            else
            {
                DPad = DPadDirection.West;
            }
        }
        else if (buttons0.HasFlag(XboxButtons.DpadUp))
        {
            if (buttons0.HasFlag(XboxButtons.DpadRight))
            {
                DPad = DPadDirection.NorthEast;
            }
            else
            {
                DPad = DPadDirection.North;
            }
        }
        else if (buttons0.HasFlag(XboxButtons.DpadRight))
        {
            DPad = DPadDirection.East;
        }
        else
        {
            DPad = DPadDirection.Default;
        }
    }
}
