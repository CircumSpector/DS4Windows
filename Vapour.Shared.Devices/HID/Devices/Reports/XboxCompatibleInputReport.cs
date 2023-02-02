using System.Runtime.InteropServices;

using Vapour.Shared.Devices.HID.InputTypes;
using Vapour.Shared.Devices.HID.InputTypes.Xbox.In;

namespace Vapour.Shared.Devices.HID.Devices.Reports;

public class XboxCompatibleInputReport : InputSourceReport
{
    public override InputAxisType AxisScaleInputType => InputAxisType.Xbox;

    public override void Parse(ReadOnlySpan<byte> input)
    {
        var inputReportData = MemoryMarshal.AsRef<InputReportData>(input.Slice(11));
       
        Triangle = inputReportData.Buttons.HasFlag(XboxButtons.Y);
        Circle = inputReportData.Buttons.HasFlag(XboxButtons.B);
        Cross = inputReportData.Buttons.HasFlag(XboxButtons.A);
        Square = inputReportData.Buttons.HasFlag(XboxButtons.X);

        LeftShoulder = inputReportData.Buttons.HasFlag(XboxButtons.L1);
        RightShoulder = inputReportData.Buttons.HasFlag(XboxButtons.R1);

        Options = inputReportData.Buttons.HasFlag(XboxButtons.Start);
        Share = inputReportData.Buttons.HasFlag(XboxButtons.Back);
        PS = inputReportData.Buttons.HasFlag(XboxButtons.Xbox);

        SetDPad(inputReportData.Buttons);

        LeftThumb = inputReportData.Buttons.HasFlag(XboxButtons.L3);
        RightThumb = inputReportData.Buttons.HasFlag(XboxButtons.R3);

        LeftTrigger = inputReportData.LeftTrigger;
        RightTrigger = inputReportData.RightTrigger;

        LeftThumbX = inputReportData.LeftThumbX;
        LeftThumbY = inputReportData.LeftThumbY;
        RightThumbX = inputReportData.RightThumbX;
        RightThumbY = inputReportData.RightThumbY;
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
