namespace Vapour.Shared.Common.Types
{
    public class OneEuroFilter3D
    {
        public const double DEFAULT_WHEEL_CUTOFF = 0.4;
        public const double DEFAULT_WHEEL_BETA = 0.2;

        public OneEuroFilter Axis1Filter { get; } = new(DEFAULT_WHEEL_CUTOFF, DEFAULT_WHEEL_BETA);
        public OneEuroFilter Axis2Filter { get; } = new(DEFAULT_WHEEL_CUTOFF, DEFAULT_WHEEL_BETA);
        public OneEuroFilter Axis3Filter { get; } = new(DEFAULT_WHEEL_CUTOFF, DEFAULT_WHEEL_BETA);

        public void SetFilterAttrs(double minCutoff, double beta)
        {
            Axis1Filter.MinCutoff = Axis2Filter.MinCutoff = Axis3Filter.MinCutoff = minCutoff;
            Axis1Filter.Beta = Axis2Filter.Beta = Axis3Filter.Beta = beta;
        }
    }
}