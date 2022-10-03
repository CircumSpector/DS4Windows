using PropertyChanged;

namespace Vapour.Shared.Common.Types
{
    [AddINotifyPropertyChangedInterface]
    public class GyroDirectionalSwipeInfo
    {
        public enum XAxisSwipe : ushort
        {
            Yaw,
            Roll
        }

        public const string DefaultTriggers = "-1";
        public const int DefaultGyroDirSpeed = 80; // degrees per second
        public const bool DefaultTriggerCond = true;
        public const bool DefaultTriggerTurns = true;
        public const XAxisSwipe DefaultXAxis = XAxisSwipe.Yaw;
        public const int DefaultDelayTime = 0;

        public int DeadZoneX { get; set; } = DefaultGyroDirSpeed;

        public int DeadZoneY { get; set; } = DefaultGyroDirSpeed;

        public string Triggers { get; set; } = DefaultTriggers;

        public bool TriggerCondition { get; set; } = DefaultTriggerCond;

        public bool TriggerTurns { get; set; } = DefaultTriggerTurns;

        public XAxisSwipe XAxis { get; set; } = DefaultXAxis;

        public int DelayTime { get; set; } = DefaultDelayTime;

        public void Reset()
        {
            DeadZoneX = DefaultGyroDirSpeed;
            DeadZoneY = DefaultGyroDirSpeed;
            Triggers = DefaultTriggers;
            TriggerCondition = DefaultTriggerCond;
            TriggerTurns = DefaultTriggerTurns;
            XAxis = DefaultXAxis;
            DelayTime = DefaultDelayTime;
        }
    }
}