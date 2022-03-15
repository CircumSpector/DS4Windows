using System.ComponentModel;

namespace DS4Windows.Shared.Common.Types
{
    /// <summary>
    ///     Possible Bezier Curve Modes.
    /// </summary>
    public enum CurveMode
    {
        [Description("Linear")] Linear = 0,
        [Description("Enhanced Precision")] EnhancedPrecision,
        [Description("Quadratic")] Quadratic,
        [Description("Cubic")] Cubic,
        [Description("Easeout Quad")] EaseoutQuad,
        [Description("Easeout Cubic")] EaseoutCubic,
        [Description("Custom")] Custom
    }
}