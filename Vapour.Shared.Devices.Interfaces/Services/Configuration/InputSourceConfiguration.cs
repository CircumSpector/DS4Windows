using System.Text.Json.Serialization;

using Vapour.Shared.Common.Types;
using Vapour.Shared.Configuration.Profiles.Schema;

namespace Vapour.Shared.Devices.Services.Configuration;

public class InputSourceConfiguration
{
    public const string MultiControllerKeySeparator = "::::";

    public List<InputSourceConfigurationController> Controllers { get; set; } = new();
    [JsonInclude]
    public Guid ProfileId { get; set; }

    public OutputDeviceType OutputDeviceType { get; set; }
    public string Lightbar { get; set; }
    public bool IsRumbleEnabled { get; set; }
    public bool IsPassthru { get; set; }

    [JsonIgnore]
    public IProfile Profile { get; set; }

    public bool IsGameConfiguration => GameInfo != null;
    public GameInfo GameInfo { get; set; }

    public string InputSourceKey
    {
        get
        {
            return string.Join("::::", Controllers.OrderBy(c => c.Index).Select(c => c.DeviceKey));
        }
    }
}