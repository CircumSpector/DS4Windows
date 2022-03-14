using System.Collections.ObjectModel;
using DS4Windows.Shared.Devices.HID;
using DS4Windows.Shared.Devices.Types;

namespace DS4Windows.Shared.Devices.Services;

public interface IInputSourceService
{
    ReadOnlyObservableCollection<IInputSource> InputSources { get; }

    void ControllerArrived(int slot, CompatibleHidDevice device);

    void ControllerDeparted(int slot, CompatibleHidDevice device);
}

public class InputSourceService : IInputSourceService
{
    private readonly IControllerManagerService controllerManagerService;
    private readonly ObservableCollection<IInputSource> inputSources;

    public InputSourceService(IControllerManagerService controllerManagerService)
    {
        this.controllerManagerService = controllerManagerService;

        inputSources = new ObservableCollection<IInputSource>();

        InputSources = new ReadOnlyObservableCollection<IInputSource>(inputSources);
    }

    public ReadOnlyObservableCollection<IInputSource> InputSources { get; }

    public void ControllerArrived(int slot, CompatibleHidDevice device)
    {
    }

    public void ControllerDeparted(int slot, CompatibleHidDevice device)
    {
    }
}