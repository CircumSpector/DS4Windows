using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace DS4Windows
{
    public partial class Global
    {
        public void SaveTo(string path)
        {
            RuntimeAppDataPath = path;
            _config.ProfilesPath = Path.Combine(RuntimeAppDataPath, Constants.ProfilesFileName);
            _config.ActionsPath = Path.Combine(RuntimeAppDataPath, Constants.ActionsFileName);
            _config.LinkedProfilesPath = Path.Combine(RuntimeAppDataPath, Constants.LinkedProfilesFileName);
            _config.ControllerConfigsPath = Path.Combine(RuntimeAppDataPath, Constants.ControllerConfigsFileName);
        }

        public void FindConfigLocation()
        {
            var programFolderAutoProfilesExists =
                File.Exists(Path.Combine(ExecutableDirectory, Constants.AutoProfilesFileName));
            var appDataAutoProfilesExists =
                File.Exists(Path.Combine(RoamingAppDataPath, Constants.AutoProfilesFileName));
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
                        LoadActions();
                    }
                }
            }
            catch
            {
            }
        }

        public byte GetRumbleBoost(int index)
        {
            return _config.RumbleBoost[index];
        }

        public void SetRumbleAutostopTime(int index, int value)
        {
            _config.RumbleAutostopTime[index] = value;

            var tempDev = Program.rootHub.DS4Controllers[index];
            if (tempDev != null && tempDev.isSynced())
                tempDev.RumbleAutostopTime = value;
        }

        public int GetRumbleAutostopTime(int index)
        {
            return _config.RumbleAutostopTime[index];
        }

        public bool GetEnableTouchToggle(int index)
        {
            return _config.EnableTouchToggle[index];
        }

        public int GetIdleDisconnectTimeout(int index)
        {
            return _config.IdleDisconnectTimeout[index];
        }

        public bool GetEnableOutputDataToDS4(int index)
        {
            return _config.EnableOutputDataToDS4[index];
        }

        public byte GetTouchSensitivity(int index)
        {
            return _config.TouchSensitivity[index];
        }

        public bool GetTouchActive(int index)
        {
            return TouchpadActive[index];
        }

        public LightbarSettingInfo GetLightbarSettingsInfo(int index)
        {
            return _config.LightbarSettingInfo[index];
        }

        public bool GetDirectInputOnly(int index)
        {
            return _config.DirectInputOnly[index];
        }

        public bool IsUsingTouchpadForControls(int index)
        {
            return _config.TouchOutMode[index] == TouchpadOutMode.Controls;
        }

        public bool IsUsingSAForControls(int index)
        {
            return _config.GyroOutputMode[index] == GyroOutMode.Controls;
        }

        public string GetSATriggers(int index)
        {
            return _config.SATriggers[index];
        }

        public bool GetSATriggerCondition(int index)
        {
            return _config.SATriggerCondition[index];
        }

        public void SetSaTriggerCond(int index, string text)
        {
            _config.SetSaTriggerCond(index, text);
        }

        public GyroOutMode GetGyroOutMode(int device)
        {
            return _config.GyroOutputMode[device];
        }

        public string GetSAMouseStickTriggers(int device)
        {
            return _config.SAMouseStickTriggers[device];
        }

        public bool GetSAMouseStickTriggerCond(int device)
        {
            return _config.SAMouseStickTriggerCond[device];
        }

        public void SetSaMouseStickTriggerCond(int index, string text)
        {
            _config.SetSaMouseStickTriggerCond(index, text);
        }

        public bool GetGyroMouseStickTriggerTurns(int device)
        {
            return _config.GyroMouseStickTriggerTurns[device];
        }

        public int GetGyroMouseStickHorizontalAxis(int index)
        {
            return _config.GyroMouseStickHorizontalAxis[index];
        }

        public GyroMouseStickInfo GetGyroMouseStickInfo(int device)
        {
            return _config.GyroMouseStickInfo[device];
        }

        public GyroDirectionalSwipeInfo GetGyroSwipeInfo(int device)
        {
            return _config.GyroSwipeInfo[device];
        }

        public void SetGyroMouseStickToggle(int index, bool value, ControlService control)
        {
            _config.SetGyroMouseStickToggle(index, value, control);
        }

        public SASteeringWheelEmulationAxisType GetSASteeringWheelEmulationAxis(int index)
        {
            return _config.SASteeringWheelEmulationAxis[index];
        }

        public int GetSASteeringWheelEmulationRange(int index)
        {
            return _config.SASteeringWheelEmulationRange[index];
        }


        public int GetGyroSensitivity(int index)
        {
            return _config.GyroSensitivity[index];
        }

        public int GetGyroSensVerticalScale(int index)
        {
            return _config.GyroSensVerticalScale[index];
        }

        public int GetGyroInvert(int index)
        {
            return _config.GyroInvert[index];
        }

        public bool GetGyroTriggerTurns(int index)
        {
            return _config.GyroTriggerTurns[index];
        }

        public int GetGyroMouseHorizontalAxis(int index)
        {
            return _config.GyroMouseHorizontalAxis[index];
        }

        public void SetGyroMouseDeadZone(int index, int value, ControlService control)
        {
            _config.SetGyroMouseDZ(index, value, control);
        }

        public void SetGyroMouseToggle(int index, bool value, ControlService control)
        {
            _config.SetGyroMouseToggle(index, value, control);
        }

        public void SetGyroControlsToggle(int index, bool value, ControlService control)
        {
            _config.SetGyroControlsToggle(index, value, control);
        }

        public GyroControlsInfo GetGyroControlsInfo(int index)
        {
            return _config.GyroControlsInfo[index];
        }

        public byte GetTapSensitivity(int index)
        {
            return _config.TapSensitivity[index];
        }

        public bool GetDoubleTap(int index)
        {
            return _config.DoubleTap[index];
        }

        public bool GetTouchPadJitterCompensation(int index)
        {
            return _config.TouchpadJitterCompensation[index];
        }

        public int GetTouchPadInvert(int index)
        {
            return _config.TouchPadInvert[index];
        }

        public TriggerDeadZoneZInfo GetL2ModInfo(int index)
        {
            return _config.L2ModInfo[index];
        }

        public TriggerDeadZoneZInfo GetR2ModInfo(int index)
        {
            return _config.R2ModInfo[index];
        }

        public double GetSXDeadZone(int index)
        {
            return _config.SXDeadzone[index];
        }

        public double GetSZDeadZone(int index)
        {
            return _config.SZDeadzone[index];
        }

        public int GetLSDeadZone(int index)
        {
            return _config.LSModInfo[index].deadZone;
        }

        public int GetRSDeadZone(int index)
        {
            return _config.RSModInfo[index].deadZone;
        }

        public StickDeadZoneInfo GetLSDeadInfo(int index)
        {
            return _config.LSModInfo[index];
        }

        public StickDeadZoneInfo GetRSDeadInfo(int index)
        {
            return _config.RSModInfo[index];
        }

        public double GetSXAntiDeadZone(int index)
        {
            return _config.SXAntiDeadzone[index];
        }

        public double GetSZAntiDeadZone(int index)
        {
            return _config.SZAntiDeadzone[index];
        }
        
        public double GetSXMaxZone(int index)
        {
            return _config.SXMaxzone[index];
        }

        public double GetSZMaxZone(int index)
        {
            return _config.SZMaxzone[index];
        }

        public double GetLSRotation(int index)
        {
            return _config.LSRotation[index];
        }

        public double GetRSRotation(int index)
        {
            return _config.RSRotation[index];
        }

        public double GetL2Sens(int index)
        {
            return _config.L2Sens[index];
        }

        public double GetR2Sens(int index)
        {
            return _config.R2Sens[index];
        }

        public double GetSXSens(int index)
        {
            return _config.SXSens[index];
        }

        public double GetSZSens(int index)
        {
            return _config.SZSens[index];
        }

        public double GetLSSens(int index)
        {
            return _config.LSSens[index];
        }

        public double GetRSSens(int index)
        {
            return _config.RSSens[index];
        }

        public int GetBluetoothPollRate(int index)
        {
            return _config.BluetoothPollRate[index];
        }

        public SquareStickInfo GetSquareStickInfo(int device)
        {
            return _config.SquStickInfo[device];
        }

        public StickAntiSnapbackInfo GetLSAntiSnapbackInfo(int device)
        {
            return _config.LSAntiSnapbackInfo[device];
        }

        public StickAntiSnapbackInfo GetRSAntiSnapbackInfo(int device)
        {
            return _config.RSAntiSnapbackInfo[device];
        }

        public bool GetTrackballMode(int index)
        {
            return _config.TrackballMode[index];
        }

        public double GetTrackballFriction(int index)
        {
            return _config.TrackballFriction[index];
        }

        public int GetProfileActionCount(int index)
        {
            return _config.profileActionCount[index];
        }

        public void UpdateDS4CSetting(int deviceNum, string buttonName, bool shift, object action, string exts,
            DS4KeyType kt, int trigger = 0)
        {
            _config.UpdateDs4ControllerSetting(deviceNum, buttonName, shift, action, exts, kt, trigger);
            _config.ContainsCustomAction[deviceNum] = _config.HasCustomActions(deviceNum);
            _config.ContainsCustomExtras[deviceNum] = _config.HasCustomExtras(deviceNum);
        }

        public void UpdateDS4Extra(int deviceNum, string buttonName, bool shift, string exts)
        {
            _config.UpdateDs4ControllerExtra(deviceNum, buttonName, shift, exts);
            _config.ContainsCustomAction[deviceNum] = _config.HasCustomActions(deviceNum);
            _config.ContainsCustomExtras[deviceNum] = _config.HasCustomExtras(deviceNum);
        }

        public DS4ControlSettings GetDS4CSetting(int deviceNum, DS4Controls control)
        {
            return _config.GetDs4ControllerSetting(deviceNum, control);
        }

        public ControlSettingsGroup GetControlSettingsGroup(int deviceNum)
        {
            return _config.ds4controlSettings[deviceNum];
        }

        public bool ContainsCustomAction(int deviceNum)
        {
            return _config.ContainsCustomAction[deviceNum];
        }

        public bool ContainsCustomExtras(int deviceNum)
        {
            return _config.ContainsCustomExtras[deviceNum];
        }

        public void SaveAction(string name, string controls, int mode,
            string details, bool edit, string extras = "")
        {
            _config.SaveAction(name, controls, mode, details, edit, extras);
            Mapping.actionDone.Add(new Mapping.ActionState());
        }

        public void RemoveAction(string name)
        {
            _config.RemoveAction(name);
        }

        public bool LoadActions()
        {
            return _config.LoadActions();
        }

        public int GetActionIndexOf(string name)
        {
            return _config.GetActionIndexOf(name);
        }

        public int GetProfileActionIndexOf(int device, string name)
        {
            var index = -1;
            _config.profileActionIndexDict[device].TryGetValue(name, out index);
            return index;
        }

        public SpecialAction GetAction(string name)
        {
            return _config.GetAction(name);
        }

        public SpecialAction GetProfileAction(int device, string name)
        {
            SpecialAction sA = null;
            _config.profileActionDict[device].TryGetValue(name, out sA);
            return sA;
        }

        public void CacheProfileCustomsFlags(int device)
        {
            _config.CacheProfileCustomsFlags(device);
        }

        public void CacheExtraProfileInfo(int device)
        {
            _config.CacheExtraProfileInfo(device);
        }
        
        public bool ContainsLinkedProfile(string serial)
        {
            var tempSerial = serial.Replace(":", string.Empty);
            return _config.linkedProfiles.ContainsKey(tempSerial);
        }

        public string GetLinkedProfile(string serial)
        {
            var temp = string.Empty;
            var tempSerial = serial.Replace(":", string.Empty);
            if (_config.linkedProfiles.ContainsKey(tempSerial)) temp = _config.linkedProfiles[tempSerial];

            return temp;
        }

        public void ChangeLinkedProfile(string serial, string profile)
        {
            var tempSerial = serial.Replace(":", string.Empty);
            _config.linkedProfiles[tempSerial] = profile;
        }

        public void RemoveLinkedProfile(string serial)
        {
            var tempSerial = serial.Replace(":", string.Empty);
            if (_config.linkedProfiles.ContainsKey(tempSerial)) _config.linkedProfiles.Remove(tempSerial);
        }

        public bool Load()
        {
            return _config.Load();
        }

        public bool LoadProfile(int device, bool launchprogram, ControlService control,
            bool xinputChange = true, bool postLoad = true)
        {
            var result = _config.LoadProfile(device, launchprogram, control, "", xinputChange, postLoad);
            TempProfileNames[device] = string.Empty;
            UseTempProfiles[device] = false;
            TempProfileDistance[device] = false;

            return result;
        }

        public bool LoadTempProfile(int device, string name, bool launchprogram,
            ControlService control, bool xinputChange = true)
        {
            var result = _config.LoadProfile(device, launchprogram, control,
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

        public void LoadDefaultDS4MixedControlsProfile(int device, bool launchprogram, ControlService control,
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

        public bool Save()
        {
            return _config.Save();
        }

        public void SaveProfile(int device, string proName)
        {
            _config.SaveProfile(device, proName);
        }

        public void SaveAsNewProfile(int device, string propath)
        {
            _config.SaveAsNewProfile(device, propath);
        }

        public bool SaveLinkedProfiles()
        {
            return _config.SaveLinkedProfiles();
        }

        public bool LoadLinkedProfiles()
        {
            return _config.LoadLinkedProfiles();
        }

        public bool SaveControllerConfigs(DS4Device device = null)
        {
            if (device != null)
                return _config.SaveControllerConfigsForDevice(device);

            for (var idx = 0; idx < ControlService.MAX_DS4_CONTROLLER_COUNT; idx++)
                if (Program.rootHub.DS4Controllers[idx] != null)
                    _config.SaveControllerConfigsForDevice(Program.rootHub.DS4Controllers[idx]);

            return true;
        }

        public bool LoadControllerConfigs(DS4Device device = null)
        {
            if (device != null)
                return _config.LoadControllerConfigsForDevice(device);

            for (var idx = 0; idx < ControlService.MAX_DS4_CONTROLLER_COUNT; idx++)
                if (Program.rootHub.DS4Controllers[idx] != null)
                    _config.LoadControllerConfigsForDevice(Program.rootHub.DS4Controllers[idx]);

            return true;
        }

        public void RefreshExtrasButtons(int deviceNum, List<DS4Controls> devButtons)
        {
            _config.ds4controlSettings[deviceNum].ResetExtraButtons();
            if (devButtons != null) _config.ds4controlSettings[deviceNum].EstablishExtraButtons(devButtons);
        }
    }
}