namespace DS4Windows.Shared.Common.Util
{
    public static class MathsUtils
    {
        public static double Clamp(double min, double value, double max)
        {
            return value < min ? min : value > max ? max : value;
        }
    }
}