namespace Vapour.Shared.Devices.Services.Reporting;

public class CustomActionReport
{
    public long[] Values { get; } = new long[16];

    public byte CrossStart => 0;
    public byte SquareStart => 1;
    public byte TriangleStart => 2;
    public byte CircleStart => 3;

    public byte DPadNorthStart => 4;
    public byte DPadSouthStart => 5;
    public byte DPadWestStart => 6;
    public byte DPadEastStart => 7;

    public byte OptionsStart => 8;
    public byte ShareStart => 9;
    public byte L3Start => 10;
    public byte R3Start => 11;
    public byte L1Start => 12;
    public byte L2Start => 13;
    public byte R1Start => 14;
    public byte R2Start => 15;

    public byte PSStart => 16;

    public void SetValue(byte index, long value)
    {
        Values[index] = value;
    }

    public long GetValue(byte index)
    {
        return Values[index];
    }
}
