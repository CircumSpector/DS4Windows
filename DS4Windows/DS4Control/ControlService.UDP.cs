using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using DS4WinWPF.DS4Control.Logging;
using static DS4Windows.Global;

namespace DS4Windows
{
    public partial class ControlService
    {
        private bool udpChangeStatus;
        public bool changingUDPPort;

        public async void UseUDPPort()
        {
            changingUDPPort = true;
            var devices = DS4Devices.GetDS4Controllers();
            foreach (var dev in devices)
                dev.QueueEvent(() =>
                {
                    if (dev.MotionEvent != null) dev.Report -= dev.MotionEvent;
                });

            await Task.Delay(100);

            var UDP_SERVER_PORT = Instance.Config.UdpServerPort;
            var UDP_SERVER_LISTEN_ADDRESS = appSettings.Settings.UDPServerListenAddress;

            try
            {
                _udpServer.Start(UDP_SERVER_PORT, UDP_SERVER_LISTEN_ADDRESS);
                foreach (var dev in devices)
                    dev.QueueEvent(() =>
                    {
                        if (dev.MotionEvent != null) dev.Report += dev.MotionEvent;
                    });
                LogDebug($"UDP server listening on address {UDP_SERVER_LISTEN_ADDRESS} port {UDP_SERVER_PORT}");
            }
            catch (SocketException ex)
            {
                var errMsg =
                    string.Format(
                        "Couldn't start UDP server on address {0}:{1}, outside applications won't be able to access pad data ({2})",
                        UDP_SERVER_LISTEN_ADDRESS, UDP_SERVER_PORT, ex.SocketErrorCode);

                LogDebug(errMsg, true);
                AppLogger.Instance.LogToTray(errMsg, true, true);
            }

            changingUDPPort = false;
        }

        public void ResetUdpSmoothingFilters(int idx)
        {
            if (idx < UdpServer.NUMBER_SLOTS)
            {
                var temp = udpEuroPairAccel[idx] = new OneEuroFilter3D();
                temp.SetFilterAttrs(Instance.UDPServerSmoothingMincutoff, Instance.UDPServerSmoothingBeta);

                temp = udpEuroPairGyro[idx] = new OneEuroFilter3D();
                temp.SetFilterAttrs(Instance.UDPServerSmoothingMincutoff, Instance.UDPServerSmoothingBeta);
            }
        }

        private void ChangeUdpSmoothingAttrs(object sender, EventArgs e)
        {
            for (var i = 0; i < udpEuroPairAccel.Length; i++)
            {
                var temp = udpEuroPairAccel[i];
                temp.SetFilterAttrs(Instance.UDPServerSmoothingMincutoff, Instance.UDPServerSmoothingBeta);
            }

            for (var i = 0; i < udpEuroPairGyro.Length; i++)
            {
                var temp = udpEuroPairGyro[i];
                temp.SetFilterAttrs(Instance.UDPServerSmoothingMincutoff, Instance.UDPServerSmoothingBeta);
            }
        }
    }
}
