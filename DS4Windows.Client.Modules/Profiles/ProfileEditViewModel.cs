using AutoMapper;
using DS4Windows.Client.Core.ViewModel;
using DS4Windows.Client.Modules.Profiles.Controls;
using DS4Windows.Shared.Configuration.Profiles.Schema;
using FastDeepCloner;

namespace DS4Windows.Client.Modules.Profiles
{
    public class ProfileEditViewModel : ViewModel<IProfileEditViewModel>, IProfileEditViewModel
    {
        private readonly IMapper mapper;


        public ProfileEditViewModel(IViewModelFactory viewModelFactory, IMapper mapper)
        {
            leftStick = viewModelFactory.Create<IStickEditViewModel, IStickEditView>();
            rightStick = viewModelFactory.Create<IStickEditViewModel, IStickEditView>();
            this.mapper = mapper;
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
            mapper.Map(Profile, this);
        }

        public IProfile GetUpdatedProfile()
        {
            var profile = Profile;
            mapper.Map(this, profile);
            return profile;
        }
    }
}
