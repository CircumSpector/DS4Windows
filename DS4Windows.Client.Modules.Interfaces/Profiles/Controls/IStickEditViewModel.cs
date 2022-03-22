using DS4Windows.Client.Core.ViewModel;
using DS4Windows.Shared.Common.Types;

namespace DS4Windows.Client.Modules.Profiles.Controls
{
    public interface IStickEditViewModel : IViewModel<IStickEditViewModel>
    {
        StickMode OutputSettings { get; set; }
        IStickControlModeSettingsViewModel ControlModeSettings { get; }
    }
}
