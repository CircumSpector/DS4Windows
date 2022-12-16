namespace Vapour.Shared.Devices.Services.Configuration;

public class ControllerConfigurationChangedEventArgs : EventArgs
{
    public string ControllerKey { get; set; }

    public ControllerConfiguration ControllerConfiguration { get; set; }
}