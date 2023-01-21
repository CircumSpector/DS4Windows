namespace Vapour.Shared.Devices.Services.Configuration;
public class InputSourceConfigurationController
{
    public string DeviceKey { get; set; }

    public MultiControllerConfigurationType MultiControllerConfigurationType { get; set; } =
        MultiControllerConfigurationType.None;
    public int Index { get; set; }
}

public enum MultiControllerConfigurationType
{
    Left,
    Right,
    None,
    Custom
}
