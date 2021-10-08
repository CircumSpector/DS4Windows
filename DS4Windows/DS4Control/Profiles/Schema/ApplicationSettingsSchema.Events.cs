using System;

namespace DS4WinWPF.DS4Control.Profiles.Legacy
{
    public partial class DS4WindowsAppSettings
    {
        public event Action IsTracingEnabledChanged;

        private void OnIsTracingEnabledChanged()
        {
            IsTracingEnabledChanged?.Invoke();
        }
    }
}
