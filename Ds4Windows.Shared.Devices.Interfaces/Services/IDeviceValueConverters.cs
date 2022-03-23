namespace DS4Windows.Shared.Devices.Services
{
    public interface IDeviceValueConverters
    {
        int DeadZoneDoubleToInt(double val);
        double DeadZoneIntToDouble(int inVal);
    }
}