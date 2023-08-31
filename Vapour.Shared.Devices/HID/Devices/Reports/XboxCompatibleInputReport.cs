using Nefarius.Utilities.HID.Devices;

using Vapour.Shared.Devices.HID.InputTypes;
using Vapour.Shared.Devices.HID.InputTypes.Xbox.In;

namespace Vapour.Shared.Devices.HID.Devices.Reports;

public class XboxCompatibleInputReport : InputSourceReport, IStructInputSourceReport<InputReport>
{
    public override InputAxisType AxisScaleInputType => InputAxisType.Xbox;

    public void Parse(ref InputReport inputReport)
    {
        var inputReportData = inputReport.InputReportData;

        var buttons = inputReportData.Buttons;
        Triangle = buttons.HasFlag(XboxButtons.Y);
        Circle = buttons.HasFlag(XboxButtons.B);
        Cross = buttons.HasFlag(XboxButtons.A);
        Square = buttons.HasFlag(XboxButtons.X);

        LeftShoulder = buttons.HasFlag(XboxButtons.L1);
        RightShoulder = buttons.HasFlag(XboxButtons.R1);

        Options = buttons.HasFlag(XboxButtons.Start);
        Share = buttons.HasFlag(XboxButtons.Back);
        PS = buttons.HasFlag(XboxButtons.Xbox);

        SetDPad(inputReportData.Buttons);

        LeftThumb = buttons.HasFlag(XboxButtons.L3);
        RightThumb = buttons.HasFlag(XboxButtons.R3);

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
