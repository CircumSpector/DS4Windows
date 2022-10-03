namespace Vapour.Shared.Common.Types
{
    public enum TriggerEffects : ushort
    {
        None,
        Rigid,
        Pulse,
        FullClick
    }

    public enum TriggerId : ushort
    {
        Trigger0,
        LeftTrigger = Trigger0,
        Trigger1,
        RightTrigger = Trigger1
    }

    public class TriggerEffectSettings
    {
        public byte MaxValue { get; set; }

        public byte StartValue { get; set; }
    }
}