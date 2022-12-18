namespace Vapour.Shared.Devices.Services.Configuration;
public class GameInfo
{
    public string GameId { get; set; }
    public string GameName { get; set; }
    public GameSource GameSource { get; set; }
}

public enum GameSource
{
    Steam,
    UWP
}