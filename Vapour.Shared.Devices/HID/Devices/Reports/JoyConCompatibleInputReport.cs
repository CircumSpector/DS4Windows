using Nefarius.Utilities.HID.Devices;

using Vapour.Shared.Common.Util;
using Vapour.Shared.Devices.HID.InputTypes;
using Vapour.Shared.Devices.HID.InputTypes.JoyCon.In;
using Vapour.Shared.Devices.Services.Configuration;

namespace Vapour.Shared.Devices.HID.Devices.Reports;

public sealed class JoyConCompatibleInputReport : InputSourceReport, IRawInputSourceReport
{
    private readonly short[] _finalStickData = new short[2];
    private readonly float[] _tempStickData = new float[2];

    public override InputAxisType AxisScaleInputType => InputAxisType.Xbox;

    public ushort[] StickCalibration { get; } = new ushort[6];
    public ushort DeadZone { get; set; }
    public bool IsLeft { get; set; }

    /// <inheritdoc />
    public void Parse(ReadOnlySpan<byte> input)
    {
        var inputReport = input.ToStruct<InputReport>();

        SetButtons(inputReport.ReportData);
        SetSticks(inputReport.ReportData);
    }

    private void SetButtons(InputReportData reportData)
    {
        var sharedButtons = reportData.SharedButtons;
        var leftButtons = reportData.LeftButtons;
        var rightButtons = reportData.RightButtons;

        if (MultiControllerConfigurationType != MultiControllerConfigurationType.None)
        {
            if (IsLeft)
            {
                SetDPad(leftButtons);
                Share = sharedButtons.HasFlag(JoyConButtonsShared.Minus);
                LeftThumb = sharedButtons.HasFlag(JoyConButtonsShared.LStick);
                LeftShoulder = leftButtons.HasFlag(JoyConButtonsLeft.L1);
                LeftTrigger = reportData.LeftTrigger;
            }
            else
            {
                PS = sharedButtons.HasFlag(JoyConButtonsShared.Home);
                Options = sharedButtons.HasFlag(JoyConButtonsShared.Plus);
                Cross = rightButtons.HasFlag(JoyConButtonsRight.A);
                Circle = rightButtons.HasFlag(JoyConButtonsRight.B);
                Square = rightButtons.HasFlag(JoyConButtonsRight.X);
                Triangle = rightButtons.HasFlag(JoyConButtonsRight.Y);
                RightShoulder = rightButtons.HasFlag(JoyConButtonsRight.R1);
                RightTrigger = reportData.RightTrigger;
            }
        }
        else
        {
            if (IsLeft)
            {
                Options = sharedButtons.HasFlag(JoyConButtonsShared.Capture);
                Share = sharedButtons.HasFlag(JoyConButtonsShared.Minus);
                LeftShoulder = leftButtons.HasFlag(JoyConButtonsLeft.SL);
                LeftTrigger = reportData.L1AsTrigger;
                RightShoulder = leftButtons.HasFlag(JoyConButtonsLeft.SR);
                RightTrigger = reportData.LeftTrigger;
                Cross = leftButtons.HasFlag(JoyConButtonsLeft.DpadDown);
                Circle = leftButtons.HasFlag(JoyConButtonsLeft.DpadLeft);
                Square = leftButtons.HasFlag(JoyConButtonsLeft.DpadRight);
                Triangle = leftButtons.HasFlag(JoyConButtonsLeft.DpadUp);
            }
            else
            {
                Options = sharedButtons.HasFlag(JoyConButtonsShared.Plus);
                Share = sharedButtons.HasFlag(JoyConButtonsShared.Home);
                LeftShoulder = rightButtons.HasFlag(JoyConButtonsRight.SL);
                LeftTrigger = reportData.R1AsTrigger;
                RightShoulder = rightButtons.HasFlag(JoyConButtonsRight.SR);
                RightTrigger = reportData.RightTrigger;
                Cross = rightButtons.HasFlag(JoyConButtonsRight.X);
                Circle = rightButtons.HasFlag(JoyConButtonsRight.A);
                Square = rightButtons.HasFlag(JoyConButtonsRight.Y);
                Triangle = rightButtons.HasFlag(JoyConButtonsRight.B);
            }
        }
    }

    private void SetSticks(InputReportData reportData)
    {
        if (IsLeft)
        {
            SetStick(reportData.LeftAxis1, reportData.LeftAxis2, reportData.LeftAxis3);
            LeftThumbX = _finalStickData[0];
            LeftThumbY = _finalStickData[1];
        }
        else
        {
            SetStick(reportData.RightAxis1, reportData.RightAxis2, reportData.RightAxis3);

            if (MultiControllerConfigurationType == MultiControllerConfigurationType.None)
            {
                LeftThumbX = _finalStickData[0];
                LeftThumbY = _finalStickData[1];
            }
            else
            {
                RightThumbX = _finalStickData[0];
                RightThumbY = _finalStickData[1];
            }
        }
    }

    private void SetStick(byte axis1, byte axis2, byte axis3)
    {
        ushort tempX = (ushort)(axis1 | ((axis2 & 0xf) << 8));
        ushort tempY = (ushort)((axis2 >> 4) | (axis3 << 4));

        CenterSticks(tempX, tempY);

        _finalStickData[0] = CastStickValue(_tempStickData[0]);
        _finalStickData[1] = CastStickValue(_tempStickData[1]);
    }

    private void CenterSticks(ushort x, ushort y)
    {
        _tempStickData[0] = 0;
        _tempStickData[1] = 0;
        float dx = x - StickCalibration[2], dy = y - StickCalibration[3];
        if (Math.Abs((dx * dx) + (dy * dy)) < DeadZone * DeadZone)
        {
            return;
        }

        _tempStickData[0] = dx / (dx > 0 ? StickCalibration[0] : StickCalibration[4]);
        _tempStickData[1] = dy / (dy > 0 ? StickCalibration[1] : StickCalibration[5]);

        if (MultiControllerConfigurationType == MultiControllerConfigurationType.None)
        {
            float firstValue = _tempStickData[0];
            _tempStickData[0] = _tempStickData[1] * (IsLeft ? -1f : 1f);
            _tempStickData[1] = firstValue * (IsLeft ? 1f : -1f);
        }
    }

    private static short CastStickValue(float value)
    {
        return (short)Math.Max(short.MinValue,
            Math.Min(short.MaxValue, value * (value > 0 ? short.MaxValue : -short.MinValue)));
    }

    private void SetDPad(JoyConButtonsLeft buttons0)
    {
        if (buttons0.HasFlag(JoyConButtonsLeft.DpadDown))
        {
            if (buttons0.HasFlag(JoyConButtonsLeft.DpadLeft))
            {
                DPad = DPadDirection.SouthWest;
            }
            else if (buttons0.HasFlag(JoyConButtonsLeft.DpadRight))
            {
                DPad = DPadDirection.SouthEast;
            }
            else
            {
                DPad = DPadDirection.South;
            }
        }
        else if (buttons0.HasFlag(JoyConButtonsLeft.DpadLeft))
        {
            if (buttons0.HasFlag(JoyConButtonsLeft.DpadUp))
            {
                DPad = DPadDirection.NorthWest;
            }
            else
            {
                DPad = DPadDirection.West;
            }
        }
        else if (buttons0.HasFlag(JoyConButtonsLeft.DpadUp))
        {
            if (buttons0.HasFlag(JoyConButtonsLeft.DpadRight))
            {
                DPad = DPadDirection.NorthEast;
            }
            else
            {
                DPad = DPadDirection.North;
            }
        }
        else if (buttons0.HasFlag(JoyConButtonsLeft.DpadRight))
        {
            DPad = DPadDirection.East;
        }
        else
        {
            DPad = DPadDirection.Default;
        }
    }
}