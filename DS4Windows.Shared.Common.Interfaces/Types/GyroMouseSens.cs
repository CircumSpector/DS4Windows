namespace DS4Windows.Shared.Common.Types
{
    public class GyroMouseSens
    {
        public double MouseCoefficient { get; set; } = 0.012;

        public double MouseOffset { get; set; } = 0.2;

        public double MouseSmoothOffset { get; set; } = 0.2;
    }
}