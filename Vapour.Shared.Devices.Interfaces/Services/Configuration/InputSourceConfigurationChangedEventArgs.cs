namespace Vapour.Shared.Devices.Services.Configuration;

public class InputSourceConfigurationChangedEventArgs : EventArgs
{
    public string InputSourceKey { get; set; }

    public InputSourceConfiguration InputSourceConfiguration { get; set; }
}