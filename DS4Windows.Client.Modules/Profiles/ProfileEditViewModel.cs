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
        private IProfile profile;

        public ProfileEditViewModel(IViewModelFactory viewModelFactory, IMapper mapper)
        {
            leftStick = viewModelFactory.Create<IStickEditViewModel, IStickEditView>();
            rightStick = viewModelFactory.Create<IStickEditViewModel, IStickEditView>();
            l2Button = viewModelFactory.Create<ITriggerButtonsEditViewModel, ITriggerButtonsEditView>();
            r2Button = viewModelFactory.Create<ITriggerButtonsEditViewModel, ITriggerButtonsEditView>();
            this.mapper = mapper;
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

        private ITriggerButtonsEditViewModel l2Button;
        public ITriggerButtonsEditViewModel L2Button
        {
            get => l2Button;
            private set => SetProperty(ref l2Button, value);
        }

        private ITriggerButtonsEditViewModel r2Button;
        public ITriggerButtonsEditViewModel R2Button
        {
            get => r2Button;
            private set => SetProperty(ref r2Button, value);
        }

        public void SetProfile(IProfile profile, bool isNew = false)
        {
            this.profile = profile.Clone();
            IsNew = isNew;
            mapper.Map(this.profile, this);
        }

        public IProfile GetUpdatedProfile()
        {
            mapper.Map(this, profile);
            return profile;
        }
    }
}
