using DS4Windows.Client.Core.ViewModel;
using DS4Windows.Client.Modules.Profiles.Controls;
using DS4Windows.Shared.Configuration.Profiles.Schema;
using FastDeepCloner;

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

        private bool isNew;
        public bool IsNew
        {
            get => isNew;
            private set => SetProperty(ref isNew, value);
        } 

        private string name;
        public string Name
        {
            get => name;
            set => SetProperty(ref name, value);
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

        public void SetProfile(IProfile profile, bool isNew = false)
        {
            Profile = profile.Clone();

            IsNew = isNew;
            Name = profile.DisplayName;
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

        public IProfile GetUpdatedProfile()
        {
            var profile = Profile;

            profile.DisplayName = Name;

            #region Set Left Stick

            profile.LSModInfo.DeadZone = leftStick.DeadZone;
            profile.LSModInfo.AntiDeadZone = leftStick.AntiDeadZone;
            profile.LSModInfo.MaxZone = leftStick.MaxZone;
            profile.LSModInfo.MaxOutput = leftStick.MaxOutput;

            #endregion  

            #region Set Right Stick

            profile.RSModInfo.DeadZone = rightStick.DeadZone;
            profile.RSModInfo.AntiDeadZone = rightStick.AntiDeadZone;
            profile.RSModInfo.MaxZone = rightStick.MaxZone;
            profile.RSModInfo.MaxOutput = rightStick.MaxOutput;

            #endregion  

            return profile;
        }
    }
}
