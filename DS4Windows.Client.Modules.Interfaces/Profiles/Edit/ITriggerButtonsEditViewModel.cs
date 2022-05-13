using DS4Windows.Client.Core.ViewModel;
using DS4Windows.Shared.Common.Types;

namespace DS4Windows.Client.Modules.Profiles.Edit
{
    public interface ITriggerButtonsEditViewModel : IViewModel<ITriggerButtonsEditViewModel>
    {
        double DeadZoneConverted { get; set; }
        int AntiDeadZone { get; set; }
        int MaxZone { get; set; }
        int MaxOutput { get; set; }
        double Sensitivity { get; set; }
        int HipFireDelay { get; set; }
        CurveMode OutputCurve { get; set; }
        BezierCurve CustomCurve { get; set; }
        TwoStageTriggerMode TwoStageTriggerMode { get; set; }
        TriggerEffects TriggerEffect { get; set; }
        int DeadZone { get; set; }
    }
}
