using Vapour.Client.Core.ViewModel;
using Vapour.Shared.Common.Types;

namespace Vapour.Client.Modules.Profiles.Edit
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
