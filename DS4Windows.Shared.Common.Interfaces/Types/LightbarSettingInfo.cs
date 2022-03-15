using System;
using JetBrains.Annotations;
using PropertyChanged;

namespace DS4Windows.Shared.Common.Types
{
    /// <summary>
    ///     Lightbar behaviour settings.
    /// </summary>
    [AddINotifyPropertyChangedInterface]
    public class LightbarSettingInfo
    {
        public LightbarMode Mode { get; set; } = LightbarMode.DS4Win;

        public LightbarDS4WinInfo Ds4WinSettings { get; set; } = new();

        public event EventHandler ModeChanged;

        [UsedImplicitly]
        private void OnModeChanged()
        {
            ModeChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
