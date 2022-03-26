using DS4Windows.Client.Core.ViewModel;
using DS4Windows.Shared.Common.Types;
using Microsoft.Toolkit.Mvvm.Input;
using System;
using System.ComponentModel;
using System.Diagnostics;

namespace DS4Windows.Client.Modules.Profiles.Controls
{
    public class StickEditViewModel : ViewModel<IStickEditViewModel>, IStickEditViewModel
    {
        private string path = $"file:///{AppContext.BaseDirectory.Replace('\\', '/')}BezierCurveEditor/index.html";

        public StickEditViewModel(IViewModelFactory viewModelFactory)
        {
            ControlModeSettings = viewModelFactory.Create<IStickControlModeSettingsViewModel, IStickControlModeSettingsView>();
            ShowCustomCurveCommand = new RelayCommand(OnShowCustomCurve);
        }

        private StickMode outputSettings;
        public StickMode OutputSettings
        {
            get => outputSettings;
            set => SetProperty(ref outputSettings, value);
        }

        public bool IsControlModeSet => OutputSettings == StickMode.Controls;
        public IStickControlModeSettingsViewModel ControlModeSettings { get; }

        private double flickRealWorldCalibration;
        public double FlickRealWorldCalibtration
        {
            get => flickRealWorldCalibration;
            set => SetProperty(ref flickRealWorldCalibration, value);
        }

        private double flickThreshold;
        public double FlickThreshold
        {
            get => flickThreshold;
            set => SetProperty(ref flickThreshold, Math.Round(value, 1));
        }

        private double flickTime;
        public double FlickTime
        {
            get => flickTime;
            set => SetProperty(ref flickTime, Math.Round(value, 1));
        }

        private double flickMinAngleThreshold;
        public double FlickMinAngleThreshold
        {
            get => flickMinAngleThreshold;
            set => SetProperty(ref flickMinAngleThreshold, Math.Round(value, 1));
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
            var processStartInfo = new ProcessStartInfo(path);
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

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.PropertyName == nameof(OutputSettings))
            {
                OnPropertyChanged(nameof(IsControlModeSet));
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
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ControlModeSettings.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
