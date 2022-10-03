using System;
using JetBrains.Annotations;
using PropertyChanged;

namespace Vapour.Shared.Common.Types
{
    [AddINotifyPropertyChangedInterface]
    public class TriggerDeadZoneZInfo
    {
        // Trigger deadzone is expressed in axis units (bad old convention)
        public byte DeadZone { get; set; }

        public int AntiDeadZone { get; set; }

        public int MaxZone { get; set; } = 100;

        public double MaxOutput { get; set; } = 100.0;

        [UsedImplicitly]
        private void OnDeadZoneChanged()
        {
            DeadZoneChanged?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler DeadZoneChanged;

        [UsedImplicitly]
        private void OnMaxZoneChanged()
        {
            MaxZoneChanged?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler MaxZoneChanged;

        [UsedImplicitly]
        private void OnMaxOutputChanged()
        {
            MaxOutputChanged?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler MaxOutputChanged;

        public void Reset()
        {
            DeadZone = 0;
            AntiDeadZone = 0;
            MaxZone = 100;
            MaxOutput = 100.0;
        }

        public void ResetEvents()
        {
            MaxZoneChanged = null;
            MaxOutputChanged = null;
            DeadZoneChanged = null;
        }
    }
}