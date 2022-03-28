using DS4Windows.Client.Core.ViewModel;
using DS4Windows.Shared.Common.Types;
using System;
using System.ComponentModel;

namespace DS4Windows.Client.Modules.Profiles.Controls
{
    public class StickEditViewModel : ViewModel<IStickEditViewModel>, IStickEditViewModel
    {
        public StickEditViewModel(IViewModelFactory viewModelFactory)
        {
            ControlModeSettings = viewModelFactory.Create<IStickControlModeSettingsViewModel, IStickControlModeSettingsView>();
        }

        private StickMode outputSettings;
        public StickMode OutputSettings
        {
            get => outputSettings;
            set => SetProperty(ref outputSettings, value);
        }

        public bool IsControlModeSet => OutputSettings == StickMode.Controls;
        public bool IsFlickStickSet => OutputSettings == StickMode.FlickStick;
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

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.PropertyName == nameof(OutputSettings))
            {
                OnPropertyChanged(nameof(IsControlModeSet));
                OnPropertyChanged(nameof(IsFlickStickSet));
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
