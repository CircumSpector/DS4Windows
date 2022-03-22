using DS4Windows.Client.Core.ViewModel;
using DS4Windows.Shared.Common.Types;
using System.ComponentModel;
using System.Windows;

namespace DS4Windows.Client.Modules.Profiles.Controls
{
    public class StickControlModeSettingsViewModel : ViewModel<IStickControlModeSettingsViewModel>, IStickControlModeSettingsViewModel
    {
        #region Radial Properties

        private int deadZone;
        public int DeadZone
        {
            get => deadZone;
            set => SetProperty(ref deadZone, value);
        }

        private int antiDeadZone;
        public int AntiDeadZone
        {
            get => antiDeadZone;
            set => SetProperty(ref antiDeadZone, value);
        }

        private int maxZone;
        public int MaxZone
        {
            get => maxZone;
            set => SetProperty(ref maxZone, value);
        }

        private double maxOutput;
        public double MaxOutput
        {
            get => maxOutput;
            set => SetProperty(ref maxOutput, value);
        }

        private double verticalScale;
        public double VerticalScale
        {
            get => verticalScale;
            set => SetProperty(ref verticalScale, value);
        }

        private int sensitivity;
        public int Sensitivity
        {
            get => sensitivity;
            set => SetProperty(ref sensitivity, value);
        }

        #endregion

        #region X Properties

        private int xDeadZone;
        public int XDeadZone
        {
            get => xDeadZone;
            set => SetProperty(ref xDeadZone, value);
        }

        private int xMaxZone;
        public int XMaxZone
        {
            get => xMaxZone;
            set => SetProperty(ref xMaxZone, value);
        }
        private int xAntiDeadZone;
        public int XAntiDeadZone
        {
            get => xAntiDeadZone;
            set => SetProperty(ref xAntiDeadZone, value);
        }
        private int xMaxOutput;
        public int XMaxOutput
        {
            get => xMaxOutput;
            set => SetProperty(ref xMaxOutput, value);
        }

        #endregion

        #region Y Properties

        private int yDeadZone;
        public int YDeadZone
        {
            get => yDeadZone;
            set => SetProperty(ref yDeadZone, value);
        }

        private int yMaxZone;
        public int YMaxZone
        {
            get => yMaxZone;
            set => SetProperty(ref yMaxZone, value);
        }

        private int yAntiDeadZone;
        public int YAntiDeadZone
        {
            get => yAntiDeadZone;
            set => SetProperty(ref yAntiDeadZone, value);
        }

        private int yMaxOutput;
        public int YMaxOutput
        {
            get => yMaxOutput;
            set => SetProperty(ref yMaxOutput, value);
        }

        #endregion

        private StickDeadZoneInfo.DeadZoneType deadZooneType;
        public StickDeadZoneInfo.DeadZoneType DeadZoneType
        {
            get => deadZooneType;
            set => SetProperty(ref deadZooneType, value);
        }

        public bool IsRadialSet => DeadZoneType == StickDeadZoneInfo.DeadZoneType.Radial;

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.PropertyName == nameof(DeadZoneType))
            {
                OnPropertyChanged(nameof(IsRadialSet));
            }
        }
    }
}
