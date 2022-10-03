using PropertyChanged;

namespace Vapour.Shared.Common.Types
{
    [AddINotifyPropertyChangedInterface]
    public class StickAntiSnapbackInfo
    {
        public const double DefaultDelta = 135;
        public const int DefaultTimeout = 50;
        public const bool DefaultEnabled = false;

        public bool Enabled { get; set; } = DefaultEnabled;

        public double Delta { get; set; } = DefaultDelta;

        public int Timeout { get; set; } = DefaultTimeout;
    }
}