using System.Collections.ObjectModel;
using System.Linq;
using Vapour.Shared.Devices.HID;
using Vapour.Shared.Devices.HID.Devices;
using Vapour.Shared.Devices.Interfaces.HID;
using Vapour.Shared.Devices.Types;

namespace Vapour.Shared.Devices.Services;

public interface IInputSourceService
{
    ReadOnlyObservableCollection<IInputSource> InputSources { get; }

    void ControllerArrived(int slot, ICompatibleHidDevice device);

    void ControllerDeparted(int slot, ICompatibleHidDevice device);
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

    public void ControllerArrived(int slot, ICompatibleHidDevice device)
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

    public void ControllerDeparted(int slot, ICompatibleHidDevice device)
    {
        if (device is JoyConCompatibleHidDevice)
        {
            var primary = inputSources
                .OfType<ICompositeInputSource>()
                .Cast<CompositeInputSource>()
                .FirstOrDefault(source => Equals(source.SecondarySourceDevice, device));

            if (primary is not null)
            {
                inputSources.Remove(primary);
                inputSources.Add(new CompositeInputSource(primary.PrimarySourceDevice));
                return;
            }

            var secondary = inputSources
                .OfType<ICompositeInputSource>()
                .Cast<CompositeInputSource>()
                .FirstOrDefault(source => Equals(source.PrimarySourceDevice, device));

            if (secondary is not null)
            {
                inputSources.Remove(secondary);
                inputSources.Add(new CompositeInputSource(secondary.SecondarySourceDevice));
                return;
            }
        }

        inputSources.Remove(inputSources.First(source => Equals(source.PrimarySourceDevice, device)));
    }
}