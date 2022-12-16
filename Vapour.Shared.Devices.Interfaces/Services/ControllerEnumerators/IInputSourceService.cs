using System.Collections.ObjectModel;

namespace Vapour.Shared.Devices.Services.ControllerEnumerators;

public interface IInputSourceService
{
    ReadOnlyObservableCollection<IInputSource> InputSources { get; }
}