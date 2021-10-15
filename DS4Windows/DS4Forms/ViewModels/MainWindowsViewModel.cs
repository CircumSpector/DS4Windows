using System;

namespace DS4WinWPF.DS4Forms.ViewModels
{
    public class MainWindowsViewModel
    {
        private bool fullTabsEnabled = true;

        public string updaterExe = Environment.Is64BitProcess ? "DS4Updater.exe" : "DS4Updater_x86.exe";

        public bool FullTabsEnabled
        {
            get => fullTabsEnabled;
            set
            {
                fullTabsEnabled = value;
                FullTabsEnabledChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public event EventHandler FullTabsEnabledChanged;
    }
}