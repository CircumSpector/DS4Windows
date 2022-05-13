using DS4Windows.Client.Core.ViewModel;
using DS4Windows.Shared.Common.Types;

namespace DS4Windows.Client.Modules.Profiles.Edit
{
    public interface ISixAxisEditViewModel : IViewModel<ISixAxisEditViewModel>
    {
        double AntiDeadZone { get; set; }
        double MaxZone { get; set; }
        CurveMode OutputCurve { get; set; }
        BezierCurve CustomCurve { get; set; }
        double DeadZone { get; set; }
        double Sensitivity { get; set; }
    }
}
