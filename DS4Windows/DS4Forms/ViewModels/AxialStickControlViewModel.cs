using System;
using DS4Windows;

namespace DS4WinWPF.DS4Forms.ViewModels
{
    public class AxialStickControlViewModel
    {
        private StickDeadZoneInfo stickInfo;

        public double DeadZoneX
        {
            get => Math.Round(stickInfo.XAxisDeadInfo.DeadZone / 127d, 2);
            set
            {
                double temp = Math.Round(stickInfo.XAxisDeadInfo.DeadZone / 127d, 2);
                if (temp == value) return;
                stickInfo.XAxisDeadInfo.DeadZone = (int)Math.Round(value * 127d);
                DeadZoneXChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler DeadZoneXChanged;

        public double DeadZoneY
        {
            get => Math.Round(stickInfo.YAxisDeadInfo.DeadZone / 127d, 2);
            set
            {
                double temp = Math.Round(stickInfo.YAxisDeadInfo.DeadZone / 127d, 2);
                if (temp == value) return;
                stickInfo.YAxisDeadInfo.DeadZone = (int)Math.Round(value * 127d);
                DeadZoneYChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler DeadZoneYChanged;

        public double MaxZoneX
        {
            get => stickInfo.XAxisDeadInfo.MaxZone / 100.0;
            set => stickInfo.XAxisDeadInfo.MaxZone = (int)(value * 100.0);
        }

        public double MaxZoneY
        {
            get => stickInfo.YAxisDeadInfo.MaxZone / 100.0;
            set => stickInfo.YAxisDeadInfo.MaxZone = (int)(value * 100.0);
        }

        public double AntiDeadZoneX
        {
            get => stickInfo.XAxisDeadInfo.AntiDeadZone / 100.0;
            set => stickInfo.XAxisDeadInfo.AntiDeadZone = (int)(value * 100.0);
        }

        public double AntiDeadZoneY
        {
            get => stickInfo.YAxisDeadInfo.AntiDeadZone / 100.0;
            set => stickInfo.YAxisDeadInfo.AntiDeadZone = (int)(value * 100.0);
        }

        public double MaxOutputX
        {
            get => stickInfo.XAxisDeadInfo.MaxOutput / 100.0;
            set => stickInfo.XAxisDeadInfo.MaxOutput = value * 100.0;
        }

        public double MaxOutputY
        {
            get => stickInfo.YAxisDeadInfo.MaxOutput / 100.0;
            set => stickInfo.YAxisDeadInfo.MaxOutput = value * 100.0;
        }

        public AxialStickControlViewModel(StickDeadZoneInfo deadInfo)
        {
            this.stickInfo = deadInfo;
        }
    }
}
