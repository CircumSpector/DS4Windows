using System;
using System.IO;
using DS4Windows;

namespace DS4WinWPF.DS4Forms.ViewModels
{
    public class RenameProfileViewModel
    {
        private string profileName;
        public string ProfileName
        {
            get => profileName;
            set
            {
                profileName = value;
                ProfileNameChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler ProfileNameChanged;

        [ConfigurationSystemComponent]
        public bool ProfileFileExists()
        {
            string filePath = Path.Combine(Global.RuntimeAppDataPath,
                "Profiles", $"{profileName}.xml");
            return File.Exists(filePath);
        }
    }
}
