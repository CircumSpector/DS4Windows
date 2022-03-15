using System;
using System.Collections.ObjectModel;
using System.Linq;
using DS4Windows.Shared.Devices.HID;
using DS4Windows.Shared.Devices.HID.Devices;
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
    private readonly ObservableCollection<IInputSource> inputSources;

    public InputSourceService()
    {
        inputSources = new ObservableCollection<IInputSource>();

        InputSources = new ReadOnlyObservableCollection<IInputSource>(inputSources);
    }

    public ReadOnlyObservableCollection<IInputSource> InputSources { get; }

    public void ControllerArrived(int slot, CompatibleHidDevice device)
    {
        if (device is JoyConCompatibleHidDevice)
        {
            var composite = inputSources
                .OfType<ICompositeInputSource>()
                .Cast<CompositeInputSource>()
                .FirstOrDefault(source => source.SecondarySourceDevice is null);

            if (composite is not null)
                composite.SecondarySourceDevice = device;
            else
                inputSources.Add(new CompositeInputSource(device));

            return;
        }

        inputSources.Add(new InputSource(device));
    }

    public void ControllerDeparted(int slot, CompatibleHidDevice device)
    {
        throw new NotImplementedException();
    }
}