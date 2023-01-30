namespace Vapour.Shared.Devices.Services.Configuration;
public class ProcessorWatchItem
{
    public GameSource GameSource { get; set; }

    public string GameId { get; init; }

    public string ImagePath { get; init; }

    public int Count { get; set; }
}
