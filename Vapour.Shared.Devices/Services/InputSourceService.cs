using System.Collections.ObjectModel;

using Vapour.Shared.Devices.HID;
using Vapour.Shared.Devices.HID.Devices;
using Vapour.Shared.Devices.Types;

namespace Vapour.Shared.Devices.Services;

internal sealed class InputSourceService : IInputSourceService
{
    private readonly ObservableCollection<IInputSource> _inputSources;

    public InputSourceService()
    {
        _inputSources = new ObservableCollection<IInputSource>();

        InputSources = new ReadOnlyObservableCollection<IInputSource>(_inputSources);
    }

    public ReadOnlyObservableCollection<IInputSource> InputSources { get; }

    public void ControllerArrived(int slot, ICompatibleHidDevice device)
    {
        // attempt to merge a JoyCons pair
        if (device is JoyConCompatibleHidDevice)
        {
            CompositeInputSource composite = _inputSources
                .OfType<ICompositeInputSource>()
                .Cast<CompositeInputSource>()
                .FirstOrDefault(source => source.SecondarySourceDevice is null);

            if (composite is not null)
            {
                // primary device already exists, add this as secondary
                composite.SecondarySourceDevice = device;
            }
            else
            {
                // add new primary device
                _inputSources.Add(new CompositeInputSource(device));
            }

            return;
        }

        _inputSources.Add(new InputSource(device));
    }

    public void ControllerDeparted(int slot, ICompatibleHidDevice device)
    {
        if (device is JoyConCompatibleHidDevice)
        {
            CompositeInputSource primary = _inputSources
                .OfType<ICompositeInputSource>()
                .Cast<CompositeInputSource>()
                .FirstOrDefault(source => Equals(source.SecondarySourceDevice, device));

            if (primary is not null)
            {
                _inputSources.Remove(primary);
                _inputSources.Add(new CompositeInputSource(primary.PrimarySourceDevice));
                return;
            }

            CompositeInputSource secondary = _inputSources
                .OfType<ICompositeInputSource>()
                .Cast<CompositeInputSource>()
                .FirstOrDefault(source => Equals(source.PrimarySourceDevice, device));

            if (secondary is not null)
            {
                _inputSources.Remove(secondary);
                _inputSources.Add(new CompositeInputSource(secondary.SecondarySourceDevice));
                return;
            }
        }

        _inputSources.Remove(_inputSources.First(source => Equals(source.PrimarySourceDevice, device)));
    }
}