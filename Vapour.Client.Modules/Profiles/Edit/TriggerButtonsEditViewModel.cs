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
    public class TriggerButtonsEditViewModel : ViewModel<ITriggerButtonsEditViewModel>, ITriggerButtonsEditViewModel
    {
        private readonly IDeviceValueConverters deviceValueConverters;

        public TriggerButtonsEditViewModel(IDeviceValueConverters deviceValueConverters)
        {
            this.deviceValueConverters = deviceValueConverters;
            ShowCustomCurveCommand = new RelayCommand(OnShowCustomCurve);
        }

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

        private double sensitivity;
        public double Sensitivity
        {
            get => sensitivity;
            set => SetProperty(ref sensitivity, Math.Round(value, 1));
            //Math round needed because wpf slider control when binding to double and tick increment
            //of 0.1 when it hits values like 1.2 and 2.2 it sends 1.20000000002 or something like that
        }

        private int hipFireDelay;
        public int HipFireDelay
        {
            get => hipFireDelay;
            set => SetProperty(ref hipFireDelay, value);
        }

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

        private TwoStageTriggerMode twoStageTriggerMode;
        public TwoStageTriggerMode TwoStageTriggerMode
        {
            get => twoStageTriggerMode;
            set => SetProperty(ref twoStageTriggerMode, value);
        }

        private TriggerEffects triggerEffect;
        public TriggerEffects TriggerEffect
        {
            get => triggerEffect;
            set => SetProperty(ref triggerEffect, value);
        }

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.PropertyName == nameof(OutputCurve))
            {
                if (OutputCurve != CurveMode.Custom)
                {
                    CustomCurve = null;
                }

                OnPropertyChanged(nameof(IsCustomCurveSelected));
            }
        }
    }
}
