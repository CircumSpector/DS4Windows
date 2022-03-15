using PropertyChanged;

namespace DS4Windows.Shared.Common.Types
{
    [AddINotifyPropertyChangedInterface]
    public class SquareStickInfo
    {
        public bool LSMode { get; set; }

        public bool RSMode { get; set; }

        public double LSRoundness { get; set; } = 5.0;

        public double RSRoundness { get; set; } = 5.0;
    }
}