using System.Windows.Media;

using Vapour.Client.Core.ViewModel;
using Vapour.Server.InputSource;
using Vapour.Shared.Devices.Services.Configuration;

namespace Vapour.Client.Modules.InputSource;

public interface IInputSourceItemViewModel : IViewModel<IInputSourceItemViewModel>
{
    Guid SelectedProfileId { get; set; }

    SolidColorBrush CurrentColor { get; set; }

    InputSourceConfiguration CurrentConfiguration { get; set; }

    bool ConfigurationSetFromUser { get; set; }

    string InputSourceKey { get; set; }

    List<IInputSourceControllerItemViewModel> Controllers { get; set; }

    Task SetInputSource(InputSourceMessage inputSource);
}