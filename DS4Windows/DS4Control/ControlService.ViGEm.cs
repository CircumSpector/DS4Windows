using System;
using System.Threading;
using DS4WinWPF.Translations;

namespace DS4Windows
{
    public partial class ControlService
    {
        [Obsolete]
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
    }
}