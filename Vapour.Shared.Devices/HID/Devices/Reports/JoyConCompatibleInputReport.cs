using Vapour.Shared.Devices.HID.InputTypes;

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
        if (IsLeft)
        {
            SetStick(input.Slice(6, 3));
            LeftThumbX = _finalStickData[0];
            LeftThumbY = _finalStickData[1];
        }
        else
        {
            SetStick(input.Slice(9, 3));
            RightThumbX = _finalStickData[0];
            RightThumbY = _finalStickData[1];
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
    }

    private static short CastStickValue(float value)
    {
        return (short)Math.Max(short.MinValue, Math.Min(short.MaxValue, value * (value > 0 ? short.MaxValue : -short.MinValue)));
    }
}