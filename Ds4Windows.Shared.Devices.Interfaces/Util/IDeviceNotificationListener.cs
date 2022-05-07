using DS4Windows.Shared.Devices.HID;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DS4Windows.Shared.Devices.Util
{
    /// <summary>
    ///     Utility interface to add device arrival/removal notifications to WPF window.
    /// </summary>
    public interface IDeviceNotificationListener : IDeviceNotificationListenerSubscriber
    {
        void StartListen(Window window, Guid interfaceGuid);
        void EndListen();
        void StartListen(Guid interfaceGuid);
        void StopListen();
    }
}
