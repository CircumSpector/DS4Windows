using Vapour.Client.Core.ViewModel;
using Vapour.Shared.Common.Types;

namespace Vapour.Client.Modules.Profiles.Edit
{
    public interface IStickEditViewModel : IViewModel<IStickEditViewModel>
    {
        StickMode OutputSettings { get; set; }
        IStickControlModeSettingsViewModel ControlModeSettings { get; }
        double FlickRealWorldCalibtration { get; set; }
        double FlickThreshold { get; set; }
        double FlickTime { get; set; }
        double FlickMinAngleThreshold { get; set; }
    }
}
