namespace Vapour.Shared.Devices.HID;

public interface IStructInputSourceReport<T> : IInputSourceReport
    where T : struct
{
    void Parse(ref T input);
}