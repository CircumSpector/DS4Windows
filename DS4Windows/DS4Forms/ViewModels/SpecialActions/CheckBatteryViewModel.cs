using System;
using System.Collections.Generic;
using System.Windows.Media;
using DS4Windows;
using DS4WinWPF.DS4Forms.ViewModels.Util;

namespace DS4WinWPF.DS4Forms.ViewModels.SpecialActions
{
    public class CheckBatteryViewModel : NotifyDataErrorBase
    {
        private Color emptyColor = new() {A = 255, R = 255, G = 0, B = 0};

        private Color fullColor = new() {A = 255, R = 0, G = 255, B = 0};

        private bool lightbar = true;
        private bool notification;

        public double Delay { get; set; }

        public bool Notification
        {
            get => notification;
            set => notification = value;
        }

        public bool Lightbar
        {
            get => lightbar;
            set => lightbar = value;
        }

        public Color EmptyColor
        {
            get => emptyColor;
            set
            {
                if (emptyColor == value) return;
                emptyColor = value;
                EmptyColorChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public Color FullColor
        {
            get => fullColor;
            set
            {
                if (fullColor == value) return;
                fullColor = value;
                FullColorChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public event EventHandler EmptyColorChanged;
        public event EventHandler FullColorChanged;

        public void UpdateForcedColor(Color color, int device)
        {
            if (device < ControlService.CURRENT_DS4_CONTROLLER_LIMIT)
            {
                var dcolor = new DS4Color(color);
                DS4LightBarV3.forcedColor[device] = dcolor;
                DS4LightBarV3.forcedFlash[device] = 0;
                DS4LightBarV3.forcelight[device] = true;
            }
        }

        public void StartForcedColor(Color color, int device)
        {
            if (device < ControlService.CURRENT_DS4_CONTROLLER_LIMIT)
            {
                var dcolor = new DS4Color(color);
                DS4LightBarV3.forcedColor[device] = dcolor;
                DS4LightBarV3.forcedFlash[device] = 0;
                DS4LightBarV3.forcelight[device] = true;
            }
        }

        public void EndForcedColor(int device)
        {
            if (device < ControlService.CURRENT_DS4_CONTROLLER_LIMIT)
            {
                DS4LightBarV3.forcedColor[device] = new DS4Color(0, 0, 0);
                DS4LightBarV3.forcedFlash[device] = 0;
                DS4LightBarV3.forcelight[device] = false;
            }
        }

        public void LoadAction(SpecialActionV3 action)
        {
            var details = action.Details.Split(',');
            Delay = action.DelayTime;
            bool.TryParse(details[1], out notification);
            bool.TryParse(details[2], out lightbar);
            emptyColor = Color.FromArgb(255, byte.Parse(details[3]), byte.Parse(details[4]), byte.Parse(details[5]));
            fullColor = Color.FromArgb(255, byte.Parse(details[6]), byte.Parse(details[7]), byte.Parse(details[8]));
        }

        public void SaveAction(SpecialActionV3 action, bool edit = false)
        {
            var details =
                $"{Delay.ToString("#.##", Global.ConfigFileDecimalCulture)}|{notification}|{lightbar}|{emptyColor.R}|{emptyColor.G}|{emptyColor.B}|" +
                $"{fullColor.R}|{fullColor.G}|{fullColor.B}";

            Global.Instance.SaveAction(action.Name, action.Controls, 6, details, edit);
        }

        public override bool IsValid(SpecialActionV3 action)
        {
            ClearOldErrors();

            var valid = true;
            var notificationErrors = new List<string>();
            var lightbarErrors = new List<string>();

            if (!notification && !lightbar)
            {
                notificationErrors.Add("Need status option");
                lightbarErrors.Add("Need status option");
            }
            else if (lightbar)
            {
                if (emptyColor == fullColor) lightbarErrors.Add("Need to set two different colors");
            }

            if (notificationErrors.Count > 0)
            {
                errors["Notification"] = notificationErrors;
                RaiseErrorsChanged("Notification");
            }

            if (lightbarErrors.Count > 0)
            {
                errors["Lightbar"] = lightbarErrors;
                RaiseErrorsChanged("Lightbar");
            }

            return valid;
        }

        public override void ClearOldErrors()
        {
            if (errors.Count > 0)
            {
                errors.Clear();
                RaiseErrorsChanged("Notification");
                RaiseErrorsChanged("Lightbar");
            }
        }
    }
}