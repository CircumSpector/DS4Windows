namespace Vapour.Shared.Devices.Services.Reporting;

public class CustomActionReport
{
    public long[] Values { get; } = new long[16];

    public byte CrossStart { get; } = 0;
    public byte SquareStart { get; } = 1;
    public byte TriangleStart { get; } = 2;
    public byte CircleStart { get; } = 3;
           
    public byte DPadNorthStart { get; } = 4;
    public byte DPadSouthStart { get; } = 5;
    public byte DPadWestStart { get; } = 6;
    public byte DPadEastStart { get; } = 7;
           
    public byte OptionsStart { get; } = 8;
    public byte ShareStart { get; } = 9;
    public byte L3Start { get; } = 10;
    public byte R3Start { get; } = 11;
    public byte L1Start { get; } = 12;
    public byte L2Start { get; } = 13;
    public byte R1Start { get; } = 14;
    public byte R2Start { get; } = 15;

    public void SetValue(byte index, long value)
    {
        Values[index] = value;
    }

    public long GetValue(byte index)
    {
        return Values[index];
    }
}
