using DS4Windows.Client.Core.ViewModel;

namespace DS4Windows.Client.Modules.Profiles.Controls
{
    public interface IStickEditViewModel : IViewModel<IStickEditViewModel>
    {
        int DeadZone { get; set; }
        int AntiDeadZone { get; set; }
        int MaxZone { get; set; }
        double MaxOutput { get; set; }
    }
}
