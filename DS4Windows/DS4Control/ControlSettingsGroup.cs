using System.Collections.Generic;

namespace DS4Windows
{
    public class ControlSettingsGroup
    {
        private readonly IList<DS4ControlSettings> settingsList;

        public ControlSettingsGroup(IList<DS4ControlSettings> settingsList)
        {
            LS.Add(settingsList[(int)DS4Controls.LSOuter - 1]);
            for (var i = (int)DS4Controls.LXNeg; i <= (int)DS4Controls.LYPos; i++) LS.Add(settingsList[i - 1]);

            LS.Add(settingsList[(int)DS4Controls.RSOuter - 1]);
            for (var i = (int)DS4Controls.RXNeg; i <= (int)DS4Controls.RYPos; i++) RS.Add(settingsList[i - 1]);

            L2 = settingsList[(int)DS4Controls.L2 - 1];
            R2 = settingsList[(int)DS4Controls.R2 - 1];

            L2FullPull = settingsList[(int)DS4Controls.L2FullPull - 1];
            R2FullPull = settingsList[(int)DS4Controls.R2FullPull - 1];

            GyroSwipeLeft = settingsList[(int)DS4Controls.GyroSwipeLeft - 1];
            GyroSwipeRight = settingsList[(int)DS4Controls.GyroSwipeRight - 1];
            GyroSwipeUp = settingsList[(int)DS4Controls.GyroSwipeUp - 1];
            GyroSwipeDown = settingsList[(int)DS4Controls.GyroSwipeDown - 1];

            ControlButtons.Add(settingsList[(int)DS4Controls.L1 - 1]);
            ControlButtons.Add(settingsList[(int)DS4Controls.L3 - 1]);
            ControlButtons.Add(settingsList[(int)DS4Controls.R1 - 1]);
            ControlButtons.Add(settingsList[(int)DS4Controls.R3 - 1]);

            for (var i = (int)DS4Controls.Square; i <= (int)DS4Controls.SwipeDown; i++)
                ControlButtons.Add(settingsList[i - 1]);

            this.settingsList = settingsList;
        }

        public List<DS4ControlSettings> ControlButtons => new();

        public List<DS4ControlSettings> ExtraDeviceButtons => new();

        public DS4ControlSettings GyroSwipeDown { get; set; }

        public DS4ControlSettings GyroSwipeLeft { get; set; }

        public DS4ControlSettings GyroSwipeRight { get; set; }

        public DS4ControlSettings GyroSwipeUp { get; set; }

        public DS4ControlSettings L2 { get; set; }

        public DS4ControlSettings L2FullPull { get; set; }

        public List<DS4ControlSettings> LS => new();

        public DS4ControlSettings R2 { get; set; }

        public DS4ControlSettings R2FullPull { get; set; }

        public List<DS4ControlSettings> RS => new();

        public void EstablishExtraButtons(List<DS4Controls> buttonList)
        {
            foreach (var control in buttonList) ExtraDeviceButtons.Add(settingsList[(int)control - 1]);
        }

        public void ResetExtraButtons()
        {
            ExtraDeviceButtons.Clear();
        }
    }
}