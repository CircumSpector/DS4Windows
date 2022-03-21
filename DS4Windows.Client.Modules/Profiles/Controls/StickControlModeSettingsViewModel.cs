using DS4Windows.Client.Core.ViewModel;
using DS4Windows.Shared.Common.Types;
using System.ComponentModel;
using System.Windows;

namespace DS4Windows.Client.Modules.Profiles.Controls
{
    public class StickControlModeSettingsViewModel : ViewModel<IStickControlModeSettingsViewModel>, IStickControlModeSettingsViewModel
    {
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

        private StickDeadZoneInfo.DeadZoneType deadZooneType;
        public StickDeadZoneInfo.DeadZoneType DeadZoneType
        {
            get => deadZooneType;
            set => SetProperty(ref deadZooneType, value);
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

        public Visibility IsRadialSet => DeadZoneType == StickDeadZoneInfo.DeadZoneType.Radial ? Visibility.Visible : Visibility.Collapsed;

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
