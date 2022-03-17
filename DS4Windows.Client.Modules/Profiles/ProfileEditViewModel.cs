using DS4Windows.Client.Core.ViewModel;
using DS4Windows.Client.Modules.Profiles.Controls;
using DS4Windows.Shared.Configuration.Profiles.Schema;

namespace DS4Windows.Client.Modules.Profiles
{
    public class ProfileEditViewModel : ViewModel<IProfileEditViewModel>, IProfileEditViewModel
    {
        public ProfileEditViewModel(IViewModelFactory viewModelFactory)
        {
            leftStick = viewModelFactory.Create<IStickEditViewModel, IStickEditView>();
            rightStick = viewModelFactory.Create<IStickEditViewModel, IStickEditView>();
        }

        private IProfile profile;
        public IProfile Profile
        {
            get => profile;
            private set => SetProperty(ref profile, value);
        }

        private IStickEditViewModel leftStick;
        public IStickEditViewModel LeftStick
        {
            get => leftStick;
            private set => SetProperty(ref leftStick, value);
        }

        private IStickEditViewModel rightStick;
        public IStickEditViewModel RightStick
        {
            get => rightStick;
            private set => SetProperty(ref rightStick, value);
        }

        public void SetProfile(IProfile profile)
        {
            Profile = profile;

            #region Set Left Stick

            leftStick.DeadZone = profile.LSModInfo.DeadZone;
            leftStick.AntiDeadZone = profile.LSModInfo.AntiDeadZone;
            leftStick.MaxZone = profile.LSModInfo.MaxZone;
            leftStick.MaxOutput = profile.LSModInfo.MaxOutput;

            #endregion  

            #region Set Right Stick

            rightStick.DeadZone = profile.RSModInfo.DeadZone;
            rightStick.AntiDeadZone = profile.RSModInfo.AntiDeadZone;
            rightStick.MaxZone = profile.RSModInfo.MaxZone;
            rightStick.MaxOutput = profile.RSModInfo.MaxOutput;

            #endregion  
        }
    }
}
