using Vapour.Client.Core.ViewModel;
using Vapour.Shared.Common.Types;
using Vapour.Shared.Devices.Services.Configuration;

namespace Vapour.Client.Modules.InputSource;

public interface IGameConfigurationItemViewModel : IViewModel<IGameConfigurationItemViewModel>
{
    string GameId { get; }

    string GameName { get; }

    string GameSource { get; }

    bool IsPassThru { get; set; }

    OutputDeviceType OutputDeviceType { get; set; }

    string OutputGroupName { get; }

    void SetGameConfiguration(string inputSourceKey, InputSourceConfiguration configuration);
}