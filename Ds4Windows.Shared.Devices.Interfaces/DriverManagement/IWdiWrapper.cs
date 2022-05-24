namespace Ds4Windows.Shared.Devices.Interfaces.DriverManagement;

public interface IWdiWrapper
{
    PrepareDriverResult PrepareDriver(string controllerInstanceId);
}

public class PrepareDriverResult
{
    public string HardwareId { get; set; }
    public string InfPath { get; set; }
}