namespace Vapour.Shared.Devices.Services;
public class ControllerConfigurationChangedEventArgs : EventArgs
{
    public string ControllerKey { get; set; }
    public ControllerConfiguration ControllerConfiguration { get; set; }
}
