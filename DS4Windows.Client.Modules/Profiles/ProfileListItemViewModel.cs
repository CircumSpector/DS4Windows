using AutoMapper;
using DS4Windows.Client.Core.ViewModel;
using DS4Windows.Shared.Configuration.Profiles.Schema;
using DS4Windows.Shared.Configuration.Profiles.Types;
using System;
using System.Windows.Media;

namespace DS4Windows.Client.Modules.Profiles
{
    public class ProfileListItemViewModel : ViewModel<IProfileListItemViewModel>, IProfileListItemViewModel
    {
        private IProfile profile;
        private readonly IMapper mapper;

        public ProfileListItemViewModel(IMapper mapper)
        {
            this.mapper = mapper;
        }

        #region Properties

        private Guid id;
        public Guid Id
        {
            get => id;
            private set => SetProperty(ref id, value);
        }

        private string name;
        public string Name
        {
            get => name;
            private set => SetProperty(ref name, value);
        }

        private string outputControllerType;
        public string OutputControllerType
        {
            get => outputControllerType;
            private set => SetProperty(ref outputControllerType, value);
        }

        private SolidColorBrush lightbarColor;
        public SolidColorBrush LightbarColor
        {
            get => lightbarColor;
            private set => SetProperty(ref lightbarColor, value);
        }

        private string touchpadMode;
        public string TouchpadMode
        {
            get => touchpadMode;
            private set => SetProperty(ref touchpadMode, value);
        }

        private string gyroMode;
        public string GyroMode
        {
            get => gyroMode;
            private set => SetProperty(ref gyroMode, value);
        }

        #endregion

        public void SetProfile(IProfile profile)
        {
            this.profile = profile;
            this.profile.ProfilePropertyChanged += Profile_ProfilePropertyChanged;

            mapper.Map(profile, this);
        }

        private void Profile_ProfilePropertyChanged(object sender, ProfilePropertyChangedEventArgs e)
        {
            mapper.Map(profile, this);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                profile.ProfilePropertyChanged -= Profile_ProfilePropertyChanged;
                profile = null;
            }

            base.Dispose(disposing);    
        }
    }
}
