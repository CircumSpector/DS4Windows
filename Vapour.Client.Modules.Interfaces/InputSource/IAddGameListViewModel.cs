using Vapour.Client.Core.ViewModel;
using Vapour.Shared.Devices.Services.Configuration;

namespace Vapour.Client.Modules.InputSource;

public interface IAddGameListViewModel : IViewModel<IAddGameListViewModel>
{
    Task Initialize(string inputSourceKey, GameSource gameSource, Func<Task> onCompleted);
}