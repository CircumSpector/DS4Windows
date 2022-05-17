using DS4Windows.Shared.Devices.HID;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS4Windows.Shared.Devices.Services
{
    /// <summary>
    ///     Enumerates and watches hot-plugging of supported input devices (controllers).
    /// </summary>
    public interface IControllersEnumeratorService
    {
        ReadOnlyObservableCollection<ICompatibleHidDevice> SupportedDevices { get; }

        /// <summary>
        ///     Fired when <see cref="SupportedDevices"/> has been (re-)built.
        /// </summary>
        event Action DeviceListReady;

        /// <summary>
        ///     Fired every time a supported device is found and ready.
        /// </summary>
        event Action<ICompatibleHidDevice> ControllerReady;

        /// <summary>
        ///     Fired every time a supported device has departed.
        /// </summary>
        event Action<ICompatibleHidDevice> ControllerRemoved;

        /// <summary>
        ///     Enumerate system for compatible devices. This rebuilds <see cref="SupportedDevices" />.
        /// </summary>
        void EnumerateDevices();

        void ClearCurrentControllers();
    }
}
