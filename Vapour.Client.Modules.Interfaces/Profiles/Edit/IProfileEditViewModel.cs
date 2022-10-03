using Vapour.Client.Core.ViewModel;
using Vapour.Shared.Configuration.Profiles.Schema;

namespace Vapour.Client.Modules.Profiles.Edit
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
