using DS4Windows.Client.Core.ViewModel;
using DS4Windows.Shared.Common.Types;
using DS4Windows.Shared.Devices.Services;
using System;
using System.ComponentModel;
using System.Windows;

namespace DS4Windows.Client.Modules.Profiles.Controls
{
    public class StickControlModeSettingsViewModel : ViewModel<IStickControlModeSettingsViewModel>, IStickControlModeSettingsViewModel
    {
        private readonly IDeviceValueConverters deviceValueConverters;

        public StickControlModeSettingsViewModel(IDeviceValueConverters deviceValueConverters)
        {
            this.deviceValueConverters = deviceValueConverters;
        }

        #region Radial Properties

        public int DeadZone { get; set; }
        public double DeadZoneConverted
        {
            get => deviceValueConverters.DeadZoneIntToDouble(DeadZone);
            set => DeadZone = deviceValueConverters.DeadZoneDoubleToInt(value);
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

        private int maxOutput;
        public int MaxOutput
        {
            get => maxOutput;
            set => SetProperty(ref maxOutput, value);
        }

        private bool forceMaxOutput;
        public bool ForceMaxOutput
        {
            get => forceMaxOutput;
            set => SetProperty(ref forceMaxOutput, value);
        } 

        private int verticalScale;
        public int VerticalScale
        {
            get => verticalScale;
            set => SetProperty(ref verticalScale, value);
        }

        private double sensitivity;
        public double Sensitivity
        {
            get => sensitivity;
            set => SetProperty(ref sensitivity, Math.Round(value,1)); 
            //Math round needed because wpf slider control when binding to double and tick increment
            //of 0.1 when it hits values like 1.2 and 2.2 it sends 1.20000000002 or something like that
        }

        #endregion

        #region X Properties

        public int XDeadZone { get; set; }
        public double XDeadZoneConverted
        {
            get => deviceValueConverters.DeadZoneIntToDouble(XDeadZone);
            set => XDeadZone = deviceValueConverters.DeadZoneDoubleToInt(value);
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

        public int YDeadZone { get; set; }
        public double YDeadZoneConverted
        {
            get => deviceValueConverters.DeadZoneIntToDouble(YDeadZone);
            set => YDeadZone = deviceValueConverters.DeadZoneDoubleToInt(value);
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
            else if (e.PropertyName == nameof(DeadZone))
            {
                OnPropertyChanged(nameof(DeadZoneConverted));
            }
            else if (e.PropertyName == nameof(XDeadZone))
            {
                OnPropertyChanged(nameof(XDeadZoneConverted));
            }
            else if (e.PropertyName == nameof(YDeadZone))
            {
                OnPropertyChanged(nameof(YDeadZoneConverted));
            }
        }
    }
}
