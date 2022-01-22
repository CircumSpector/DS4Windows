namespace DS4Windows.Shared.Common.Types
{
    public class OneEuroFilterPair
    {
        public const double DEFAULT_WHEEL_CUTOFF = 0.1;
        public const double DEFAULT_WHEEL_BETA = 0.1;

        public OneEuroFilter Axis1Filter { get; } = new(DEFAULT_WHEEL_CUTOFF, DEFAULT_WHEEL_BETA);
        public OneEuroFilter Axis2Filter { get; } = new(DEFAULT_WHEEL_CUTOFF, DEFAULT_WHEEL_BETA);
    }
}