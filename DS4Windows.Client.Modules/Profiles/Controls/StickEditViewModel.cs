using DS4Windows.Client.Core.ViewModel;
using DS4Windows.Shared.Common.Types;
using System.ComponentModel;
using System.Windows;

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

        public Visibility IsControlModeSet => OutputSettings == StickMode.Controls ? Visibility.Visible : Visibility.Collapsed;
        public IStickControlModeSettingsViewModel ControlModeSettings { get; }

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.PropertyName == nameof(OutputSettings))
            {
                OnPropertyChanged(nameof(IsControlModeSet));
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
