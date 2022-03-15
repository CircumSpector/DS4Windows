using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using PropertyChanged;

namespace DS4Windows.Shared.Common.Types
{
    [AddINotifyPropertyChangedInterface]
    public class SteeringWheelSmoothingInfo
    {
        public delegate void SmoothingInfoEventHandler(SteeringWheelSmoothingInfo sender, EventArgs args);

        public double MinCutoff { get; set; } = OneEuroFilterPair.DEFAULT_WHEEL_CUTOFF;

        public double Beta { get; set; } = OneEuroFilterPair.DEFAULT_WHEEL_BETA;

        public bool Enabled { get; set; }

        [UsedImplicitly]
        private void OnMinCutoffChanged()
        {
            MinCutoffChanged?.Invoke(this, EventArgs.Empty);
        }

        public event SmoothingInfoEventHandler MinCutoffChanged;

        [UsedImplicitly]
        private void OnBetaChanged()
        {
            BetaChanged?.Invoke(this, EventArgs.Empty);
        }

        public event SmoothingInfoEventHandler BetaChanged;

        public void Reset()
        {
            MinCutoff = OneEuroFilterPair.DEFAULT_WHEEL_CUTOFF;
            Beta = OneEuroFilterPair.DEFAULT_WHEEL_BETA;
            Enabled = false;
        }

        public void SetFilterAttrs(OneEuroFilter euroFilter)
        {
            euroFilter.MinCutoff = MinCutoff;
            euroFilter.Beta = Beta;
        }

        public void SetRefreshEvents(OneEuroFilter euroFilter)
        {
            BetaChanged += (sender, args) => { euroFilter.Beta = Beta; };

            MinCutoffChanged += (sender, args) => { euroFilter.MinCutoff = MinCutoff; };
        }
    }

}
