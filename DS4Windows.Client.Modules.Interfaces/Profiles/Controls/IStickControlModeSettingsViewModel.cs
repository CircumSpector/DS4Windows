using DS4Windows.Client.Core.ViewModel;
using DS4Windows.Shared.Common.Types;

namespace DS4Windows.Client.Modules.Profiles.Controls
{
    public interface IStickControlModeSettingsViewModel : IViewModel<IStickControlModeSettingsViewModel>
    {
        int DeadZone { get; set; }
        int AntiDeadZone { get; set; }
        int MaxZone { get; set; }
        double MaxOutput { get; set; }
        StickDeadZoneInfo.DeadZoneType DeadZoneType { get; set; }
        int Sensitivity { get; set; }
        double VerticalScale { get; set; }
    }
}
