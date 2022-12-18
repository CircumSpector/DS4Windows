using Vapour.Client.Core.ViewModel;
using Vapour.Shared.Devices.Services.Configuration;

namespace Vapour.Client.Modules.Controllers;

public interface IAddGameListViewModel : IViewModel<IAddGameListViewModel>
{
    Task Initialize(string controllerKey, GameSource gameSource, Func<Task> onCompleted);
}