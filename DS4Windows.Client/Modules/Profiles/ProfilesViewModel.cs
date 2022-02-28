using DS4Windows.Client.Core.ViewModel;
using DS4Windows.Client.Modules.Profiles.Interfaces;

namespace DS4Windows.Client.Modules.Profiles
{
    public class ProfilesViewModel : ViewModel<ProfilesViewModel>, IProfilesViewModel
    {
        //TODO: Change to pull localization values
        public string? Header => "Profiles";
    }
}
