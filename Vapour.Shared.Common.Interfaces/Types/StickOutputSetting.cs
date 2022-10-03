using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vapour.Shared.Common.Types
{
    public enum StickMode : uint
    {
        None,
        Controls,
        FlickStick
    }

    public class StickOutputSetting
    {
        public StickMode Mode { get; set; } = StickMode.Controls;

        public StickModeSettings OutputSettings { get; set; } = new();

        public void ResetSettings()
        {
            Mode = StickMode.Controls;
            OutputSettings.ControlSettings.Reset();
            OutputSettings.FlickSettings.Reset();
        }
    }
}
