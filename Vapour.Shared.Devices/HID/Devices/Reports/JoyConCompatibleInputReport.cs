using Vapour.Shared.Devices.HID.InputTypes;
using Vapour.Shared.Devices.HID.InputTypes.JoyCon;
using Vapour.Shared.Devices.Services.Configuration;

namespace Vapour.Shared.Devices.HID.Devices.Reports;

public sealed class JoyConCompatibleInputReport : InputSourceReport
{
    private readonly float[] _tempStickData = new float[2];
    private readonly short[] _finalStickData = new short[2];

    public override InputAxisType AxisScaleInputType => InputAxisType.Xbox;
    
    public ushort[] StickCalibration { get; } = new ushort[6];
    public ushort DeadZone { get; set; }
    public bool IsLeft { get; set; }

    /// <inheritdoc />
    public override void Parse(ReadOnlySpan<byte> input)
    {
        SetButtons(input);
        SetSticks(input);
    }

    private void SetButtons(ReadOnlySpan<byte> buttonData)
    {
        var sharedButtons = (JoyConButtonsShared)buttonData[4];
        var leftButtons = (JoyConButtonsLeft)buttonData[5];
        var rightButtons = (JoyConButtonsRight)buttonData[3];

        if (MultiControllerConfigurationType == MultiControllerConfigurationType.None)
        {
            if (IsLeft)
            {
                Options = sharedButtons.HasFlag(JoyConButtonsShared.Capture);
                Share = sharedButtons.HasFlag(JoyConButtonsShared.Minus);
                LeftShoulder = leftButtons.HasFlag(JoyConButtonsLeft.SL);
                LeftTrigger = leftButtons.HasFlag(JoyConButtonsLeft.L1) ? byte.MaxValue : byte.MinValue;
                RightShoulder = leftButtons.HasFlag(JoyConButtonsLeft.SR);
                RightTrigger = leftButtons.HasFlag(JoyConButtonsLeft.L2) ? byte.MaxValue : byte.MinValue;
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
                LeftTrigger = rightButtons.HasFlag(JoyConButtonsRight.R1) ? byte.MaxValue : byte.MinValue;
                RightShoulder = rightButtons.HasFlag(JoyConButtonsRight.SR);
                RightTrigger = rightButtons.HasFlag(JoyConButtonsRight.R2) ? byte.MaxValue : byte.MinValue;
                Cross = rightButtons.HasFlag(JoyConButtonsRight.X);
                Circle = rightButtons.HasFlag(JoyConButtonsRight.A);
                Square = rightButtons.HasFlag(JoyConButtonsRight.Y);
                Triangle = rightButtons.HasFlag(JoyConButtonsRight.B);
            }
        }
        else if (MultiControllerConfigurationType == MultiControllerConfigurationType.Left)
        {
            SetDPad(leftButtons);
            Share = sharedButtons.HasFlag(JoyConButtonsShared.Minus);
            LeftThumb = sharedButtons.HasFlag(JoyConButtonsShared.LStick);
            LeftShoulder = leftButtons.HasFlag(JoyConButtonsLeft.L1);
            LeftTrigger = leftButtons.HasFlag(JoyConButtonsLeft.L2) ? byte.MaxValue : byte.MinValue;
        }
        else if (MultiControllerConfigurationType == MultiControllerConfigurationType.Right)
        {
            PS = sharedButtons.HasFlag(JoyConButtonsShared.Home);
            Options = sharedButtons.HasFlag(JoyConButtonsShared.Plus);
            Cross = rightButtons.HasFlag(JoyConButtonsRight.A);
            Circle = rightButtons.HasFlag(JoyConButtonsRight.B);
            Square = rightButtons.HasFlag(JoyConButtonsRight.X);
            Triangle = rightButtons.HasFlag(JoyConButtonsRight.Y);
            RightShoulder = rightButtons.HasFlag(JoyConButtonsRight.R1);
            RightTrigger = rightButtons.HasFlag(JoyConButtonsRight.R2) ? byte.MaxValue : byte.MinValue;
        }
    }

    private void SetSticks(ReadOnlySpan<byte> input)
    {
        if (IsLeft)
        {
            SetStick(input.Slice(6, 3));
            LeftThumbX = _finalStickData[0];
            LeftThumbY = _finalStickData[1];
        }
        else
        {
            SetStick(input.Slice(9, 3));

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

    private void SetStick(ReadOnlySpan<byte> stickData)
    {
        ushort tempX = (ushort)(stickData[0] | ((stickData[1] & 0xf) << 8));
        ushort tempY = (ushort)((stickData[1] >> 4) | (stickData[2] << 4));

        CenterSticks(tempX, tempY);
        
        _finalStickData[0] = CastStickValue(_tempStickData[0]);
        _finalStickData[1] = CastStickValue(_tempStickData[1]);
    }

    private void CenterSticks(ushort x, ushort y)
    {
        _tempStickData[0] = 0;
        _tempStickData[1] = 0;
        float dx = x - StickCalibration[2], dy = y - StickCalibration[3];
        if (Math.Abs(dx * dx + dy * dy) < DeadZone * DeadZone)
            return;

        _tempStickData[0] = dx / (dx > 0 ? StickCalibration[0] : StickCalibration[4]);
        _tempStickData[1] = dy / (dy > 0 ? StickCalibration[1] : StickCalibration[5]);

        if (MultiControllerConfigurationType == MultiControllerConfigurationType.None)
        {
            var firstValue = _tempStickData[0];
            _tempStickData[0] = _tempStickData[1] * (IsLeft ? -1f : 1f);
            _tempStickData[1] = firstValue * (IsLeft ? 1f : -1f);
        }
    }

    private static short CastStickValue(float value)
    {
        return (short)Math.Max(short.MinValue, Math.Min(short.MaxValue, value * (value > 0 ? short.MaxValue : -short.MinValue)));
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