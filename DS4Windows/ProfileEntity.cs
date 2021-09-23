using System;
using System.IO;
using System.Threading.Tasks;
using DS4Windows;

namespace DS4WinWPF
{
    public class ProfileEntity
    {
        private string name;

        public string Name
        {
            get => name;
            set
            {
                if (name == value) return;
                name = value;
                NameChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public event EventHandler NameChanged;
        public event EventHandler ProfileSaved;
        public event EventHandler ProfileDeleted;

        [ConfigurationSystemComponent]
        public void DeleteFile()
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                var filepath = Global.RuntimeAppDataPath + @"\Profiles\" + name + ".xml";
                if (File.Exists(filepath))
                {
                    File.Delete(filepath);
                    ProfileDeleted?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public async Task SaveProfileAsync(int deviceNum)
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                await Global.Instance.Config.SaveProfile(deviceNum, name);
                Global.Instance.Config.CacheExtraProfileInfo(deviceNum);
            }
        }

        public void FireSaved()
        {
            ProfileSaved?.Invoke(this, EventArgs.Empty);
        }

        [ConfigurationSystemComponent]
        public void RenameProfile(string newProfileName)
        {
            var oldFilePath = Path.Combine(Global.RuntimeAppDataPath,
                "Profiles", $"{name}.xml");

            var newFilePath = Path.Combine(Global.RuntimeAppDataPath,
                "Profiles", $"{newProfileName}.xml");

            if (File.Exists(oldFilePath) && !File.Exists(newFilePath))
            {
                File.Move(oldFilePath, newFilePath);
                // Send NameChanged event so controls get updated with new name
                Name = newProfileName;
            }
        }
    }
}