using DS4Windows.Client.Core.ViewModel;
using DS4Windows.Shared.Configuration.Profiles.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS4Windows.Client.Modules.Profiles
{
    public class ProfileEditViewModel : ViewModel<IProfileEditViewModel>, IProfileEditViewModel
    {
        private IProfile profile;
        public IProfile Profile
        {
            get => profile;
            private set => SetProperty(ref profile, value);
        } 


        public void SetProfile(IProfile profile)
        {
            Profile = profile;
        }
    }
}
