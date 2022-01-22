using System.Collections.Generic;
using DS4Windows.Shared.Common.Types;

namespace DS4Windows.Shared.Common.Legacy
{
    public class ControlSettingsGroup
    {
        private readonly IList<DS4ControlSettingsV3> settingsList;

        public ControlSettingsGroup(IList<DS4ControlSettingsV3> settingsList)
        {
            LS.Add(settingsList[(int)DS4ControlItem.LSOuter - 1]);
            for (var i = (int)DS4ControlItem.LXNeg; i <= (int)DS4ControlItem.LYPos; i++) LS.Add(settingsList[i - 1]);

            LS.Add(settingsList[(int)DS4ControlItem.RSOuter - 1]);
            for (var i = (int)DS4ControlItem.RXNeg; i <= (int)DS4ControlItem.RYPos; i++) RS.Add(settingsList[i - 1]);

            L2 = settingsList[(int)DS4ControlItem.L2 - 1];
            R2 = settingsList[(int)DS4ControlItem.R2 - 1];

            L2FullPull = settingsList[(int)DS4ControlItem.L2FullPull - 1];
            R2FullPull = settingsList[(int)DS4ControlItem.R2FullPull - 1];

            GyroSwipeLeft = settingsList[(int)DS4ControlItem.GyroSwipeLeft - 1];
            GyroSwipeRight = settingsList[(int)DS4ControlItem.GyroSwipeRight - 1];
            GyroSwipeUp = settingsList[(int)DS4ControlItem.GyroSwipeUp - 1];
            GyroSwipeDown = settingsList[(int)DS4ControlItem.GyroSwipeDown - 1];

            ControlButtons.Add(settingsList[(int)DS4ControlItem.L1 - 1]);
            ControlButtons.Add(settingsList[(int)DS4ControlItem.L3 - 1]);
            ControlButtons.Add(settingsList[(int)DS4ControlItem.R1 - 1]);
            ControlButtons.Add(settingsList[(int)DS4ControlItem.R3 - 1]);

            for (var i = (int)DS4ControlItem.Square; i <= (int)DS4ControlItem.SwipeDown; i++)
                ControlButtons.Add(settingsList[i - 1]);

            this.settingsList = settingsList;
        }

        public DS4ControlSettingsV3 this[DS4ControlItem control] => settingsList[(int)control - 1];

        public List<DS4ControlSettingsV3> ControlButtons => new();

        public List<DS4ControlSettingsV3> ExtraDeviceButtons => new();

        public DS4ControlSettingsV3 GyroSwipeDown { get; set; }

        public DS4ControlSettingsV3 GyroSwipeLeft { get; set; }

        public DS4ControlSettingsV3 GyroSwipeRight { get; set; }

        public DS4ControlSettingsV3 GyroSwipeUp { get; set; }

        public DS4ControlSettingsV3 L2 { get; set; }

        public DS4ControlSettingsV3 L2FullPull { get; set; }

        public List<DS4ControlSettingsV3> LS => new();

        public DS4ControlSettingsV3 R2 { get; set; }

        public DS4ControlSettingsV3 R2FullPull { get; set; }

        public List<DS4ControlSettingsV3> RS => new();

        public void EstablishExtraButtons(List<DS4ControlItem> buttonList)
        {
            foreach (var control in buttonList) ExtraDeviceButtons.Add(settingsList[(int)control - 1]);
        }

        public void ResetExtraButtons()
        {
            ExtraDeviceButtons.Clear();
        }
    }
}