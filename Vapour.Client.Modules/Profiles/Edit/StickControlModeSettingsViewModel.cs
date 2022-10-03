using System;
using System.ComponentModel;
using System.Diagnostics;
using Vapour.Client.Core;
using Vapour.Client.Core.ViewModel;
using Vapour.Shared.Common;
using Vapour.Shared.Common.Types;
using Vapour.Shared.Devices.Interfaces.Services;
using Microsoft.Toolkit.Mvvm.Input;

namespace Vapour.Client.Modules.Profiles.Edit
{
    public class StickControlModeSettingsViewModel : ViewModel<IStickControlModeSettingsViewModel>, IStickControlModeSettingsViewModel
    {
        private readonly IDeviceValueConverters deviceValueConverters;

        public StickControlModeSettingsViewModel(IDeviceValueConverters deviceValueConverters)
        {
            this.deviceValueConverters = deviceValueConverters;
            ShowCustomCurveCommand = new RelayCommand(OnShowCustomCurve);
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

        private CurveMode outputCurve;
        public CurveMode OutputCurve
        {
            get => outputCurve;
            set => SetProperty(ref outputCurve, value);
        }

        private BezierCurve customCurve;
        public BezierCurve CustomCurve
        {
            get => customCurve;
            set => SetProperty(ref customCurve, value);
        }

        public bool IsCustomCurveSelected => OutputCurve == CurveMode.Custom;

        public RelayCommand ShowCustomCurveCommand { get; }
        private void OnShowCustomCurve()
        {
            var processStartInfo = new ProcessStartInfo(Constants.BezierCurveEditorPath);
            processStartInfo.UseShellExecute = true;
            Process.Start(processStartInfo);
        }

        private bool isSquareStick;
        public bool IsSquareStick
        {
            get => isSquareStick;
            set => SetProperty(ref isSquareStick, value);
        }

        private double squareStickRoundness;
        public double SquareStickRoundness
        {
            get => squareStickRoundness;
            set => SetProperty(ref squareStickRoundness, Math.Round(value, 0));
        }

        public double Rotation { get; set; }
        public double RotationConverted
        {
            get => deviceValueConverters.RotationConvertFrom(Rotation);
            set => Rotation = deviceValueConverters.RotationConvertTo(value);
        }

        private int fuzz;
        public int Fuzz
        {
            get => fuzz;
            set => SetProperty(ref fuzz, value);
        }

        private bool isAntiSnapback;
        public bool IsAntiSnapback
        {
            get => isAntiSnapback;
            set => SetProperty(ref isAntiSnapback, value);
        }

        private int antiSnapbackDelta;
        public int AntiSnapbackDelta
        {
            get => antiSnapbackDelta;
            set => SetProperty(ref antiSnapbackDelta, value);
        }

        private int antiSnapbackTimeout;
        public int AntiSnapbackTimeout
        {
            get => antiSnapbackTimeout;
            set => SetProperty(ref antiSnapbackTimeout, value);
        }

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
            else if (e.PropertyName == nameof(OutputCurve))
            {
                if (OutputCurve != CurveMode.Custom)
                {
                    CustomCurve = null;
                }

                OnPropertyChanged(nameof(IsCustomCurveSelected));
            }
            else if (e.PropertyName == nameof(IsSquareStick) && !IsSquareStick)
            {
                SquareStickRoundness = SquareStickInfo.DefaultSquareStickRoundness;
            }
            else if (e.PropertyName == nameof(Rotation))
            {
                OnPropertyChanged(nameof(RotationConverted));
            }
        }
    }
}
