using System;
using System.Threading;
using DS4WinWPF.Translations;
using Nefarius.ViGEm.Client;
using static DS4Windows.Global;

namespace DS4Windows
{
    public partial class ControlService
    {
        private void OutputslotMan_ViGEmFailure(object sender, EventArgs e)
        {
            EventDispatcher.BeginInvoke((Action)(() =>
            {
                loopControllers = false;
                while (inServiceTask)
                    Thread.SpinWait(1000);

                LogDebug(Strings.ViGEmPluginFailure, true);
                Stop();
            }));
        }

        private void CheckViGEmConnectivity()
        {
            // Refresh internal ViGEmBus info
            RefreshViGEmBusInfo();
            if (IsRunningSupportedViGEmBus)
            {
                tempThread = new Thread(() =>
                {
                    try
                    {
                        vigemTestClient = new ViGEmClient();
                    }
                    catch
                    {
                    }
                });
                tempThread.Priority = ThreadPriority.AboveNormal;
                tempThread.IsBackground = true;
                tempThread.Start();
                while (tempThread.IsAlive) Thread.SpinWait(500);
            }

            tempThread = null;
        }

        private void StopViGEm()
        {
            if (vigemTestClient != null)
            {
                vigemTestClient.Dispose();
                vigemTestClient = null;
            }
        }
    }
}
