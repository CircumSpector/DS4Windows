using System.Collections.ObjectModel;
using Vapour.Shared.Devices.HID;

namespace Vapour.Shared.Devices.Services;

public interface IInputSourceService
{
    ReadOnlyObservableCollection<IInputSource> InputSources { get; }

    void ControllerArrived(int slot, ICompatibleHidDevice device);

    void ControllerDeparted(int slot, ICompatibleHidDevice device);
}