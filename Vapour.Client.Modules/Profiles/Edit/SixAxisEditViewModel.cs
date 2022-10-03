using System;
using System.Diagnostics;
using Vapour.Client.Core;
using Vapour.Client.Core.ViewModel;
using Vapour.Shared.Common;
using Vapour.Shared.Common.Types;
using Vapour.Shared.Devices.Interfaces.Services;
using Microsoft.Toolkit.Mvvm.Input;

namespace Vapour.Client.Modules.Profiles.Edit
{
    public class SixAxisEditViewModel : ViewModel<ISixAxisEditViewModel>, ISixAxisEditViewModel
    {
        private readonly IDeviceValueConverters deviceValueConverters;

        public SixAxisEditViewModel(IDeviceValueConverters deviceValueConverters)
        {
            this.deviceValueConverters = deviceValueConverters;
            ShowCustomCurveCommand = new RelayCommand(OnShowCustomCurve);
        }

        private double deadZone;
        public double DeadZone
        {
            get => deadZone;
            set => SetProperty(ref deadZone, Math.Round(value, 1));
        } 

        private double antiDeadZone;
        public double AntiDeadZone
        {
            get => antiDeadZone;
            set => SetProperty(ref antiDeadZone, Math.Round(value, 1));
        }

        private double maxZone;
        public double MaxZone
        {
            get => maxZone;
            set => SetProperty(ref maxZone, Math.Round(value, 1));
        }

        private double sensitivity;
        public double Sensitivity
        {
            get => sensitivity;
            set => SetProperty(ref sensitivity, Math.Round(value, 1));
            //Math round needed because wpf slider control when binding to double and tick increment
            //of 0.1 when it hits values like 1.2 and 2.2 it sends 1.20000000002 or something like that
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
    }
}
