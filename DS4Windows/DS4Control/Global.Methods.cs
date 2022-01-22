using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using DS4Windows.Shared.Common.Attributes;
using DS4Windows.Shared.Common.Core;
using DS4WinWPF.DS4Control.IoC.Services;

namespace DS4Windows
{
    public partial class Global
    {
        public void SaveTo(string path)
        {
            RuntimeAppDataPath = path;
            _config.ProfilesPath = Path.Combine(RuntimeAppDataPath, Constants.LegacyProfilesFileName);
            _config.ActionsPath = Path.Combine(RuntimeAppDataPath, Constants.LegacyActionsFileName);
            _config.LinkedProfilesPath = Path.Combine(RuntimeAppDataPath, Constants.LegacyLinkedProfilesFileName);
            _config.ControllerConfigsPath = Path.Combine(RuntimeAppDataPath, Constants.LegacyControllerConfigsFileName);
        }

        public void FindConfigLocation()
        {
            var programFolderAutoProfilesExists =
                File.Exists(Path.Combine(ExecutableDirectory, Constants.LegacyAutoProfilesFileName));
            var appDataAutoProfilesExists =
                File.Exists(Path.Combine(RoamingAppDataPath, Constants.LegacyAutoProfilesFileName));
            //bool localAppDataAutoProfilesExists = File.Exists(Path.Combine(localAppDataPpath, "Auto Profiles.xml"));
            //bool systemAppConfigExists = appDataAutoProfilesExists || localAppDataAutoProfilesExists;
            var systemAppConfigExists = appDataAutoProfilesExists;
            var isSameFolder = appDataAutoProfilesExists && ExecutableDirectory == RoamingAppDataPath;

            if (programFolderAutoProfilesExists && appDataAutoProfilesExists &&
                !isSameFolder)
            {
                Instance.IsFirstRun = true;
                Instance.HasMultipleSaveSpots = true;
            }
            else if (programFolderAutoProfilesExists)
            {
                SaveTo(ExecutableDirectory);
            }
            //else if (localAppDataAutoProfilesExists)
            //{
            //    SaveWhere(localAppDataPpath);
            //}
            else if (appDataAutoProfilesExists)
            {
                SaveTo(RoamingAppDataPath);
            }
            else if (!programFolderAutoProfilesExists && !appDataAutoProfilesExists)
            {
                Instance.IsFirstRun = true;
                Instance.HasMultipleSaveSpots = false;
            }
        }

        [ConfigurationSystemComponent]
        public void CreateStdActions()
        {
            var xDoc = new XmlDocument();
            try
            {
                var profiles = Directory.GetFiles(RuntimeAppDataPath + @"\Profiles\");
                var s = string.Empty;
                //foreach (string s in profiles)
                for (int i = 0, proflen = profiles.Length; i < proflen; i++)
                {
                    s = profiles[i];
                    if (Path.GetExtension(s) == ".xml")
                    {
                        xDoc.Load(s);
                        var el = xDoc.SelectSingleNode("DS4Windows/ProfileActions");
                        if (el != null)
                        {
                            if (string.IsNullOrEmpty(el.InnerText))
                                el.InnerText = "Disconnect Controller";
                            else
                                el.InnerText += "/Disconnect Controller";
                        }
                        else
                        {
                            var Node = xDoc.SelectSingleNode("DS4Windows");
                            el = xDoc.CreateElement("ProfileActions");
                            el.InnerText = "Disconnect Controller";
                            Node.AppendChild(el);
                        }

                        xDoc.Save(s);
                        Config.LoadActions();
                    }
                }
            }
            catch
            {
            }
        }

        public bool GetTouchActive(int index)
        {
            return TouchpadActive[index];
        }

        public void SaveAction(string name, string controls, int mode,
            string details, bool edit, string extras = "")
        {
            _config.SaveAction(name, controls, mode, details, edit, extras);
            Mapping.actionDone.Add(new Mapping.ActionState());
        }

        public async Task<bool> LoadProfile(int device, bool launchprogram, ControlService control,
            bool xinputChange = true, bool postLoad = true)
        {
            var result = await _config.LoadProfile(device, launchprogram, control, "", xinputChange, postLoad);
            TempProfileNames[device] = string.Empty;
            UseTempProfiles[device] = false;
            TempProfileDistance[device] = false;

            return result;
        }

        public async Task<bool> LoadTempProfile(int device, string name, bool launchprogram,
            ControlService control, bool xinputChange = true)
        {
            var result = await _config.LoadProfile(device, launchprogram, control,
                RuntimeAppDataPath + @"\Profiles\" + name + ".xml");
            TempProfileNames[device] = name;
            UseTempProfiles[device] = true;
            TempProfileDistance[device] = name.ToLower().Contains("distance");

            return result;
        }

        public void LoadBlankDevProfile(int device, bool launchprogram, ControlService control,
            bool xinputChange = true, bool postLoad = true)
        {
            _config.LoadBlankProfile(device, launchprogram, control, "", xinputChange, postLoad);
            _config.EstablishDefaultSpecialActions(device);
            _config.CacheExtraProfileInfo(device);

            TempProfileNames[device] = string.Empty;
            UseTempProfiles[device] = false;
            TempProfileDistance[device] = false;
        }

        public void LoadBlankDS4Profile(int device, bool launchprogram, ControlService control,
            bool xinputChange = true, bool postLoad = true)
        {
            _config.LoadBlankDs4Profile(device, launchprogram, control, "", xinputChange, postLoad);
            _config.EstablishDefaultSpecialActions(device);
            _config.CacheExtraProfileInfo(device);

            TempProfileNames[device] = string.Empty;
            UseTempProfiles[device] = false;
            TempProfileDistance[device] = false;
        }

        public void LoadDefaultGamepadGyroProfile(int device, bool launchprogram, ControlService control,
            bool xinputChange = true, bool postLoad = true)
        {
            _config.LoadDefaultGamepadGyroProfile(device, launchprogram, control, "", xinputChange, postLoad);
            _config.EstablishDefaultSpecialActions(device);
            _config.CacheExtraProfileInfo(device);

            TempProfileNames[device] = string.Empty;
            UseTempProfiles[device] = false;
            TempProfileDistance[device] = false;
        }

        public void LoadDefaultDS4GamepadGyroProfile(int device, bool launchprogram, ControlService control,
            bool xinputChange = true, bool postLoad = true)
        {
            _config.LoadDefaultDS4GamepadGyroProfile(device, launchprogram, control, "", xinputChange, postLoad);
            _config.EstablishDefaultSpecialActions(device);
            _config.CacheExtraProfileInfo(device);

            TempProfileNames[device] = string.Empty;
            UseTempProfiles[device] = false;
            TempProfileDistance[device] = false;
        }

        public void LoadDefaultMixedControlsProfile(int device, bool launchprogram, ControlService control,
            bool xinputChange = true, bool postLoad = true)
        {
            _config.LoadDefaultMixedControlsProfile(device, launchprogram, control, "", xinputChange, postLoad);
            _config.EstablishDefaultSpecialActions(device);
            _config.CacheExtraProfileInfo(device);

            TempProfileNames[device] = string.Empty;
            UseTempProfiles[device] = false;
            TempProfileDistance[device] = false;
        }

        public void LoadDefaultMixedGyroMouseProfile(int device, bool launchprogram, ControlService control,
            bool xinputChange = true, bool postLoad = true)
        {
            _config.LoadDefaultMixedGyroMouseProfile(device, launchprogram, control, "", xinputChange, postLoad);
            _config.EstablishDefaultSpecialActions(device);
            _config.CacheExtraProfileInfo(device);

            TempProfileNames[device] = string.Empty;
            UseTempProfiles[device] = false;
            TempProfileDistance[device] = false;
        }

        public void LoadDefaultDS4MixedGyroMouseProfile(int device, bool launchprogram, ControlService control,
            bool xinputChange = true, bool postLoad = true)
        {
            _config.LoadDefaultDs4MixedGyroMouseProfile(device, launchprogram, control, "", xinputChange, postLoad);
            _config.EstablishDefaultSpecialActions(device);
            _config.CacheExtraProfileInfo(device);

            TempProfileNames[device] = string.Empty;
            UseTempProfiles[device] = false;
            TempProfileDistance[device] = false;
        }

        public void LoadDefaultKBMProfile(int device, bool launchprogram, ControlService control,
            bool xinputChange = true, bool postLoad = true)
        {
            _config.LoadDefaultKBMProfile(device, launchprogram, control, "", xinputChange, postLoad);
            _config.EstablishDefaultSpecialActions(device);
            _config.CacheExtraProfileInfo(device);

            TempProfileNames[device] = string.Empty;
            UseTempProfiles[device] = false;
            TempProfileDistance[device] = false;
        }

        public void LoadDefaultKBMGyroMouseProfile(int device, bool launchprogram, ControlService control,
            bool xinputChange = true, bool postLoad = true)
        {
            _config.LoadDefaultKBMGyroMouseProfile(device, launchprogram, control, "", xinputChange, postLoad);
            _config.EstablishDefaultSpecialActions(device);
            _config.CacheExtraProfileInfo(device);

            TempProfileNames[device] = string.Empty;
            UseTempProfiles[device] = false;
            TempProfileDistance[device] = false;
        }
    }
}