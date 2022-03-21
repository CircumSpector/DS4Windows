using DS4Windows.Client.Core.ViewModel;
using DS4Windows.Client.Modules.Profiles.Controls;
using DS4Windows.Shared.Configuration.Profiles.Schema;

namespace DS4Windows.Client.Modules.Profiles
{
    public interface IProfileEditViewModel : IViewModel<IProfileEditViewModel>
    {
        bool IsNew { get; }
        string Name { get; set; }
        IStickEditViewModel LeftStick { get; }
        IStickEditViewModel RightStick { get; }

        void SetProfile(IProfile profile, bool isNew = false);

        IProfile GetUpdatedProfile();
    }
}
