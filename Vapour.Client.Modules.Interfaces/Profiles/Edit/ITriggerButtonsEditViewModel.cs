using Vapour.Client.Core.ViewModel;
using Vapour.Shared.Common.Types;

namespace Vapour.Client.Modules.Profiles.Edit
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
