namespace Vapour.Shared.Devices.Services.Reporting;
public class OutputDeviceReport
{
    public byte[] FormattedData { get; } = new byte[3];

    public byte StrongMotor
    {
        get => FormattedData[0];
        set => FormattedData[0] = value;
    }

    public byte WeakMotor
    {
        get => FormattedData[1];
        set => FormattedData[1] = value;
    }

    public byte LedNumber
    {
        get => FormattedData[2];
        set => FormattedData[2] = value;
    }
}
