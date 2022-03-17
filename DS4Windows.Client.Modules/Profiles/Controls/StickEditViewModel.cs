using DS4Windows.Client.Core.ViewModel;

namespace DS4Windows.Client.Modules.Profiles.Controls
{
    public class StickEditViewModel : ViewModel<IStickEditViewModel>, IStickEditViewModel
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
    }
}
