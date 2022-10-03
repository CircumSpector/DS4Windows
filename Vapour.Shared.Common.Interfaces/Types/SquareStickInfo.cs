using PropertyChanged;

namespace Vapour.Shared.Common.Types
{
    [AddINotifyPropertyChangedInterface]
    public class SquareStickInfo
    {
        public const double DefaultSquareStickRoundness = 5.0;

        public bool LSMode { get; set; }

        public bool RSMode { get; set; }

        public double LSRoundness { get; set; } = DefaultSquareStickRoundness;

        public double RSRoundness { get; set; } = DefaultSquareStickRoundness;
    }
}