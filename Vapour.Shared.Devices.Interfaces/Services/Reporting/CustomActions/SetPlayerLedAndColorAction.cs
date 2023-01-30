namespace Vapour.Shared.Devices.Services.Reporting.CustomActions;
public class SetPlayerLedAndColorAction : ICustomAction
{
    public IInputSource InputSource { get; set; }
    public byte PlayerNumber { get; set; }
}
