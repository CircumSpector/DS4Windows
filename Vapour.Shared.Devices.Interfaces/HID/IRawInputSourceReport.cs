namespace Vapour.Shared.Devices.HID;

public interface IRawInputSourceReport : IInputSourceReport
{
    public void Parse(ReadOnlySpan<byte> input);
}