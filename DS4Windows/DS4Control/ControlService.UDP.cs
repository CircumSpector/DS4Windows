using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;
using DS4Windows.Shared.Common.Types;
using DS4WinWPF.DS4Control.Logging;

namespace DS4Windows
{
    public partial class ControlService
    {
        public bool changingUDPPort;
        private bool udpChangeStatus;

        public async void UseUDPPort()
        {
            changingUDPPort = true;
            var devices = ds4devices.GetDs4Controllers().ToList();
            foreach (var dev in devices)
                dev.QueueEvent(() =>
                {
                    if (dev.MotionEvent != null) dev.Report -= dev.MotionEvent;
                });

            await Task.Delay(100);

            var udpServerPort = appSettings.Settings.UDPServerPort;
            var udpServerListenAddress = appSettings.Settings.UDPServerListenAddress;

            try
            {
                _udpServer.Start(udpServerPort, udpServerListenAddress);
                foreach (var dev in devices)
                    dev.QueueEvent(() =>
                    {
                        if (dev.MotionEvent != null) dev.Report += dev.MotionEvent;
                    });
                LogDebug($"UDP server listening on address {udpServerListenAddress} port {udpServerPort}");
            }
            catch (SocketException ex)
            {
                var errMsg =
                    $"Couldn't start UDP server on address {udpServerListenAddress}:{udpServerPort}, outside applications won't be able to access pad data ({ex.SocketErrorCode})";

                LogDebug(errMsg, true);
                AppLogger.Instance.LogToTray(errMsg, true, true);
            }

            changingUDPPort = false;
        }

        public void ResetUdpSmoothingFilters(int idx)
        {
            if (idx >= UdpServer.NUMBER_SLOTS) return;

            var temp = udpEuroPairAccel[idx] = new OneEuroFilter3D();
            temp.SetFilterAttrs(appSettings.Settings.UDPServerSmoothingOptions.MinCutoff,
                appSettings.Settings.UDPServerSmoothingOptions.Beta);

            temp = udpEuroPairGyro[idx] = new OneEuroFilter3D();
            temp.SetFilterAttrs(appSettings.Settings.UDPServerSmoothingOptions.MinCutoff,
                appSettings.Settings.UDPServerSmoothingOptions.Beta);
        }

        public void ChangeUDPStatus(bool state, bool openPort = true)
        {
            if (state && _udpServer == null)
            {
                udpChangeStatus = true;
                TestQueueBus(() =>
                {
                    _udpServer = new UdpServer(GetPadDetailForIdx);
                    if (openPort)
                        // Change thread affinity of object to have normal priority
                        Task.Run(() =>
                        {
                            var udpServerPort = appSettings.Settings.UDPServerPort;
                            var udpServerListenAddress = appSettings.Settings.UDPServerListenAddress;

                            try
                            {
                                _udpServer.Start(udpServerPort, udpServerListenAddress);
                                LogDebug(
                                    $"UDP server listening on address {udpServerListenAddress} port {udpServerPort}");
                            }
                            catch (SocketException ex)
                            {
                                var errMsg =
                                    $"Couldn't start UDP server on address {udpServerListenAddress}:{udpServerPort}, outside applications won't be able to access pad data ({ex.SocketErrorCode})";

                                LogDebug(errMsg, true);
                                AppLogger.Instance.LogToTray(errMsg, true, true);
                            }
                        }).Wait();

                    udpChangeStatus = false;
                });
            }
            else if (!state && _udpServer != null)
            {
                TestQueueBus(() =>
                {
                    udpChangeStatus = true;
                    _udpServer.Stop();
                    _udpServer = null;
                    AppLogger.Instance.LogToGui("Closed UDP server", false);
                    udpChangeStatus = false;

                    for (var i = 0; i < UdpServer.NUMBER_SLOTS; i++) ResetUdpSmoothingFilters(i);
                });
            }
        }

        private void ChangeUdpSmoothingAttrs()
        {
            foreach (var filter3D in udpEuroPairAccel)
                filter3D.SetFilterAttrs(appSettings.Settings.UDPServerSmoothingOptions.MinCutoff,
                    appSettings.Settings.UDPServerSmoothingOptions.Beta);

            foreach (var filter3D in udpEuroPairGyro)
                filter3D.SetFilterAttrs(appSettings.Settings.UDPServerSmoothingOptions.MinCutoff,
                    appSettings.Settings.UDPServerSmoothingOptions.Beta);
        }
    }
}