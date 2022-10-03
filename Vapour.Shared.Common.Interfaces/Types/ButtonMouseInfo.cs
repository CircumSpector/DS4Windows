using System;
using JetBrains.Annotations;
using PropertyChanged;

namespace Vapour.Shared.Common.Types
{
    [AddINotifyPropertyChangedInterface]
    public class ButtonMouseInfo
    {
        //public const double MOUSESTICKANTIOFFSET = 0.0128;
        public const double MouseStickAntiOffset = 0.008;
        public const int DefaultButtonSens = 25;
        public const double DefaultButtonVerticalScale = 1.0;
        public const int DefaultTempSens = -1;

        public ButtonMouseInfo()
        {
            ButtonMouseInfoChanged += ButtonMouseInfo_ButtonMouseInfoChanged;
        }

        public int ButtonSensitivity { get; set; } = DefaultButtonSens;

        public bool MouseAcceleration { get; set; }

        public int ActiveButtonSensitivity { get; set; } = DefaultButtonSens;

        public int TempButtonSensitivity { get; set; } = DefaultTempSens;

        public double MouseVelocityOffset { get; set; } = MouseStickAntiOffset;

        public double ButtonVerticalScale { get; set; } = DefaultButtonVerticalScale;

        [UsedImplicitly]
        private void OnButtonSensitivityChanged()
        {
            ButtonMouseInfoChanged?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler ButtonMouseInfoChanged;

        private void ButtonMouseInfo_ButtonMouseInfoChanged(object sender, EventArgs e)
        {
            if (TempButtonSensitivity == DefaultTempSens) ActiveButtonSensitivity = ButtonSensitivity;
        }

        public void SetActiveButtonSensitivity(int sens)
        {
            ActiveButtonSensitivity = sens;
        }

        public void Reset()
        {
            ButtonSensitivity = DefaultButtonSens;
            MouseAcceleration = false;
            ActiveButtonSensitivity = DefaultButtonSens;
            TempButtonSensitivity = DefaultTempSens;
            MouseVelocityOffset = MouseStickAntiOffset;
            ButtonVerticalScale = DefaultButtonVerticalScale;
        }
    }
}
