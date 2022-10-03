using System;
using JetBrains.Annotations;
using PropertyChanged;

namespace Vapour.Shared.Common.Types
{
    public enum TriggerMode : uint
    {
        Normal,
        TwoStage
    }

    public enum TwoStageTriggerMode : uint
    {
        Disabled,
        Normal,
        ExclusiveButtons,
        HairTrigger,
        HipFire,
        HipFireExclusiveButtons
    }

    [AddINotifyPropertyChangedInterface]
    public class TriggerOutputSettings
    {
        private const TwoStageTriggerMode DEFAULT_TRIG_MODE = TwoStageTriggerMode.Disabled;
        private const int DEFAULT_HIP_TIME = 100;
        private const TriggerEffects DEFAULT_TRIGGER_EFFECT = TriggerEffects.None;

        public TwoStageTriggerMode TwoStageMode { get; set; } = DEFAULT_TRIG_MODE;

        public TriggerEffectSettings EffectSettings { get; set; } = new();

        public int HipFireMs { get; set; } = DEFAULT_HIP_TIME;

        public TriggerEffects TriggerEffect { get; set; } = DEFAULT_TRIGGER_EFFECT;

        [UsedImplicitly]
        private void OnTwoStageModeChanged()
        {
            TwoStageModeChanged?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler TwoStageModeChanged;

        [UsedImplicitly]
        private void OnTriggerEffectChanged()
        {
            TriggerEffectChanged?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler TriggerEffectChanged;

        public void ResetSettings()
        {
            //mode = TriggerMode.Normal;
            TwoStageMode = DEFAULT_TRIG_MODE;
            HipFireMs = DEFAULT_HIP_TIME;
            TriggerEffect = DEFAULT_TRIGGER_EFFECT;
            TwoStageModeChanged?.Invoke(this, EventArgs.Empty);
            TriggerEffectChanged?.Invoke(this, EventArgs.Empty);
        }

        public void ResetEvents()
        {
            TwoStageModeChanged = null;
            TriggerEffectChanged = null;
        }
    }
}