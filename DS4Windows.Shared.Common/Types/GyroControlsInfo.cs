namespace DS4Windows.Shared.Common.Types
{
    public class GyroControlsInfo
    {
        public const string DefaultTriggers = "-1";
        public const bool DefaultTriggerCond = true;
        public const bool DefaultTriggerTurns = true;
        public const bool DefaultTriggerToggle = false;

        public string Triggers { get; set; } = DefaultTriggers;

        public bool TriggerCond { get; set; } = DefaultTriggerCond;

        public bool TriggerTurns { get; set; } = DefaultTriggerTurns;

        public bool TriggerToggle { get; set; } = DefaultTriggerToggle;

        public void Reset()
        {
            Triggers = DefaultTriggers;
            TriggerCond = DefaultTriggerCond;
            TriggerTurns = DefaultTriggerTurns;
            TriggerToggle = DefaultTriggerToggle;
        }
    }
}
