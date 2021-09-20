using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace DS4Windows
{
    public partial class Global
    {
        public void SaveWhere(string path)
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
                SaveWhere(ExecutableDirectory);
            }
            //else if (localAppDataAutoProfilesExists)
            //{
            //    SaveWhere(localAppDataPpath);
            //}
            else if (appDataAutoProfilesExists)
            {
                SaveWhere(RoamingAppDataPath);
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

        public bool getUseExclusiveMode()
        {
            return _config.UseExclusiveMode;
        }

        public bool GetMinToTaskbar()
        {
            return _config.MinToTaskBar;
        }

        public bool getFlashWhenLate()
        {
            return _config.FlashWhenLate;
        }

        public int getFlashWhenLateAt()
        {
            return _config.FlashWhenLateAt;
        }

        public bool isUsingUDPServer()
        {
            return _config.UseUdpServer;
        }

        public void setUsingUDPServer(bool state)
        {
            _config.UseUdpServer = state;
        }

        public string getUDPServerListenAddress()
        {
            return _config.UdpServerListenAddress;
        }

        public void setUDPServerListenAddress(string value)
        {
            _config.UdpServerListenAddress = value.Trim();
        }

        public bool IsUsingUDPServerSmoothing()
        {
            return _config.UseUdpSmoothing;
        }

        public byte getRumbleBoost(int index)
        {
            return _config.RumbleBoost[index];
        }

        public void setRumbleAutostopTime(int index, int value)
        {
            _config.RumbleAutostopTime[index] = value;

            var tempDev = Program.rootHub.DS4Controllers[index];
            if (tempDev != null && tempDev.isSynced())
                tempDev.RumbleAutostopTime = value;
        }

        public int getRumbleAutostopTime(int index)
        {
            return _config.RumbleAutostopTime[index];
        }

        public bool getEnableTouchToggle(int index)
        {
            return _config.EnableTouchToggle[index];
        }

        public int getIdleDisconnectTimeout(int index)
        {
            return _config.IdleDisconnectTimeout[index];
        }

        public bool getEnableOutputDataToDS4(int index)
        {
            return _config.EnableOutputDataToDS4[index];
        }

        public byte getTouchSensitivity(int index)
        {
            return _config.TouchSensitivity[index];
        }

        public bool GetTouchActive(int index)
        {
            return TouchpadActive[index];
        }

        public LightbarSettingInfo getLightbarSettingsInfo(int index)
        {
            return _config.LightbarSettingInfo[index];
        }

        public bool getDInputOnly(int index)
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

        public string getSATriggers(int index)
        {
            return _config.SATriggers[index];
        }

        public bool getSATriggerCond(int index)
        {
            return _config.SATriggerCond[index];
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

        public int getGyroMouseStickHorizontalAxis(int index)
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


        public int getGyroSensitivity(int index)
        {
            return _config.GyroSensitivity[index];
        }

        public int getGyroSensVerticalScale(int index)
        {
            return _config.GyroSensVerticalScale[index];
        }

        public int getGyroInvert(int index)
        {
            return _config.GyroInvert[index];
        }

        public bool getGyroTriggerTurns(int index)
        {
            return _config.GyroTriggerTurns[index];
        }

        public int getGyroMouseHorizontalAxis(int index)
        {
            return _config.GyroMouseHorizontalAxis[index];
        }

        public int GetGyroMouseDeadZone(int index)
        {
            return _config.GyroMouseDeadZone[index];
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

        //public static DS4Color[] MainColor => m_Config.m_Leds;
        public ref DS4Color getMainColor(int index)
        {
            return ref _config.LightbarSettingInfo[index].ds4winSettings.m_Led;
            //return ref m_Config.m_Leds[index];
        }

        //public static DS4Color[] LowColor => m_Config.m_LowLeds;
        public ref DS4Color getLowColor(int index)
        {
            return ref _config.LightbarSettingInfo[index].ds4winSettings.m_LowLed;
            //return ref m_Config.m_LowLeds[index];
        }

        //public static DS4Color[] ChargingColor => m_Config.m_ChargingLeds;
        public ref DS4Color getChargingColor(int index)
        {
            return ref _config.LightbarSettingInfo[index].ds4winSettings.m_ChargingLed;
            //return ref m_Config.m_ChargingLeds[index];
        }

        //public static DS4Color[] CustomColor => m_Config.m_CustomLeds;
        public ref DS4Color getCustomColor(int index)
        {
            return ref _config.LightbarSettingInfo[index].ds4winSettings.m_CustomLed;
            //return ref m_Config.m_CustomLeds[index];
        }

        //public static bool[] UseCustomLed => m_Config.useCustomLeds;
        public bool getUseCustomLed(int index)
        {
            return _config.LightbarSettingInfo[index].ds4winSettings.useCustomLed;
            //return m_Config.useCustomLeds[index];
        }

        //public static DS4Color[] FlashColor => m_Config.m_FlashLeds;
        public ref DS4Color getFlashColor(int index)
        {
            return ref _config.LightbarSettingInfo[index].ds4winSettings.m_FlashLed;
            //return ref m_Config.m_FlashLeds[index];
        }

        public byte getTapSensitivity(int index)
        {
            return _config.TapSensitivity[index];
        }

        public bool getDoubleTap(int index)
        {
            return _config.DoubleTap[index];
        }

        public int getScrollSensitivity(int index)
        {
            return _config.ScrollSensitivity[index];
        }

        public bool getTouchpadJitterCompensation(int index)
        {
            return _config.TouchpadJitterCompensation[index];
        }

        public int getTouchpadInvert(int index)
        {
            return _config.TouchPadInvert[index];
        }

        public TriggerDeadZoneZInfo GetL2ModInfo(int index)
        {
            return _config.L2ModInfo[index];
        }

        //public static byte[] L2Deadzone => m_Config.l2Deadzone;
        public byte getL2Deadzone(int index)
        {
            return _config.L2ModInfo[index].deadZone;
            //return m_Config.l2Deadzone[index];
        }

        public TriggerDeadZoneZInfo GetR2ModInfo(int index)
        {
            return _config.R2ModInfo[index];
        }

        //public static byte[] R2Deadzone => m_Config.r2Deadzone;
        public byte getR2Deadzone(int index)
        {
            return _config.R2ModInfo[index].deadZone;
            //return m_Config.r2Deadzone[index];
        }

        public double getSXDeadzone(int index)
        {
            return _config.SXDeadzone[index];
        }

        public double getSZDeadzone(int index)
        {
            return _config.SZDeadzone[index];
        }

        //public static int[] LSDeadzone => m_Config.LSDeadzone;
        public int getLSDeadzone(int index)
        {
            return _config.LSModInfo[index].deadZone;
            //return m_Config.LSDeadzone[index];
        }

        //public static int[] RSDeadzone => m_Config.RSDeadzone;
        public int getRSDeadzone(int index)
        {
            return _config.RSModInfo[index].deadZone;
            //return m_Config.RSDeadzone[index];
        }

        //public static int[] LSAntiDeadzone => m_Config.LSAntiDeadzone;
        public int getLSAntiDeadzone(int index)
        {
            return _config.LSModInfo[index].antiDeadZone;
            //return m_Config.LSAntiDeadzone[index];
        }

        //public static int[] RSAntiDeadzone => m_Config.RSAntiDeadzone;
        public int getRSAntiDeadzone(int index)
        {
            return _config.RSModInfo[index].antiDeadZone;
            //return m_Config.RSAntiDeadzone[index];
        }

        public StickDeadZoneInfo GetLSDeadInfo(int index)
        {
            return _config.LSModInfo[index];
        }

        public StickDeadZoneInfo GetRSDeadInfo(int index)
        {
            return _config.RSModInfo[index];
        }

        public double getSXAntiDeadzone(int index)
        {
            return _config.SXAntiDeadzone[index];
        }

        public double getSZAntiDeadzone(int index)
        {
            return _config.SZAntiDeadzone[index];
        }

        //public static int[] LSMaxzone => m_Config.LSMaxzone;
        public int getLSMaxzone(int index)
        {
            return _config.LSModInfo[index].maxZone;
            //return m_Config.LSMaxzone[index];
        }

        //public static int[] RSMaxzone => m_Config.RSMaxzone;
        public int getRSMaxzone(int index)
        {
            return _config.RSModInfo[index].maxZone;
            //return m_Config.RSMaxzone[index];
        }

        public double getSXMaxzone(int index)
        {
            return _config.SXMaxzone[index];
        }

        public double getSZMaxzone(int index)
        {
            return _config.SZMaxzone[index];
        }

        //public static int[] L2AntiDeadzone => m_Config.l2AntiDeadzone;
        public int getL2AntiDeadzone(int index)
        {
            return _config.L2ModInfo[index].antiDeadZone;
            //return m_Config.l2AntiDeadzone[index];
        }

        //public static int[] R2AntiDeadzone => m_Config.r2AntiDeadzone;
        public int getR2AntiDeadzone(int index)
        {
            return _config.R2ModInfo[index].antiDeadZone;
            //return m_Config.r2AntiDeadzone[index];
        }

        //public static int[] L2Maxzone => m_Config.l2Maxzone;
        public int getL2Maxzone(int index)
        {
            return _config.L2ModInfo[index].maxZone;
            //return m_Config.l2Maxzone[index];
        }

        //public static int[] R2Maxzone => m_Config.r2Maxzone;
        public int getR2Maxzone(int index)
        {
            return _config.R2ModInfo[index].maxZone;
            //return m_Config.r2Maxzone[index];
        }

        public double getLSRotation(int index)
        {
            return _config.LSRotation[index];
        }

        public double getRSRotation(int index)
        {
            return _config.RSRotation[index];
        }

        public double getL2Sens(int index)
        {
            return _config.L2Sens[index];
        }

        public double getR2Sens(int index)
        {
            return _config.R2Sens[index];
        }

        public double getSXSens(int index)
        {
            return _config.SXSens[index];
        }

        public double getSZSens(int index)
        {
            return _config.SZSens[index];
        }

        public double getLSSens(int index)
        {
            return _config.LSSens[index];
        }

        public double getRSSens(int index)
        {
            return _config.RSSens[index];
        }

        public int getBTPollRate(int index)
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

        public void setLsOutCurveMode(int index, int value)
        {
            _config.setLsOutCurveMode(index, value);
        }

        public int getLsOutCurveMode(int index)
        {
            return _config.getLsOutCurveMode(index);
        }

        public void setRsOutCurveMode(int index, int value)
        {
            _config.setRsOutCurveMode(index, value);
        }

        public int getRsOutCurveMode(int index)
        {
            return _config.getRsOutCurveMode(index);
        }

        public void setL2OutCurveMode(int index, int value)
        {
            _config.setL2OutCurveMode(index, value);
        }

        public int getL2OutCurveMode(int index)
        {
            return _config.getL2OutCurveMode(index);
        }

        public void setR2OutCurveMode(int index, int value)
        {
            _config.setR2OutCurveMode(index, value);
        }

        public int getR2OutCurveMode(int index)
        {
            return _config.getR2OutCurveMode(index);
        }

        public void setSXOutCurveMode(int index, int value)
        {
            _config.setSXOutCurveMode(index, value);
        }

        public int getSXOutCurveMode(int index)
        {
            return _config.getSXOutCurveMode(index);
        }

        public void setSZOutCurveMode(int index, int value)
        {
            _config.setSZOutCurveMode(index, value);
        }

        public int getSZOutCurveMode(int index)
        {
            return _config.getSZOutCurveMode(index);
        }

        public bool getTrackballMode(int index)
        {
            return _config.TrackballMode[index];
        }

        public double getTrackballFriction(int index)
        {
            return _config.TrackballFriction[index];
        }

        public int getProfileActionCount(int index)
        {
            return _config.profileActionCount[index];
        }

        public void CalculateProfileActionCount(int index)
        {
            _config.CalculateProfileActionCount(index);
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

        public ControlActionData GetDS4Action(int deviceNum, string buttonName, bool shift)
        {
            return _config.GetDs4Action(deviceNum, buttonName, shift);
        }

        public ControlActionData GetDS4Action(int deviceNum, DS4Controls control, bool shift)
        {
            return _config.GetDs4Action(deviceNum, control, shift);
        }

        public DS4KeyType GetDS4KeyType(int deviceNum, string buttonName, bool shift)
        {
            return _config.GetDs4KeyType(deviceNum, buttonName, shift);
        }

        public string GetDS4Extra(int deviceNum, string buttonName, bool shift)
        {
            return _config.GetDs4Extra(deviceNum, buttonName, shift);
        }

        public int GetDS4STrigger(int deviceNum, string buttonName)
        {
            return _config.GetDs4STrigger(deviceNum, buttonName);
        }

        public int GetDS4STrigger(int deviceNum, DS4Controls control)
        {
            return _config.GetDs4STrigger(deviceNum, control);
        }

        public DS4ControlSettings GetDS4CSetting(int deviceNum, string control)
        {
            return _config.GetDs4ControllerSetting(deviceNum, control);
        }

        public DS4ControlSettings GetDS4CSetting(int deviceNum, DS4Controls control)
        {
            return _config.GetDs4ControllerSetting(deviceNum, control);
        }

        public ControlSettingsGroup GetControlSettingsGroup(int deviceNum)
        {
            return _config.ds4controlSettings[deviceNum];
        }

        public bool HasCustomActions(int deviceNum)
        {
            return _config.HasCustomActions(deviceNum);
        }

        public bool HasCustomExtras(int deviceNum)
        {
            return _config.HasCustomExtras(deviceNum);
        }

        public bool containsCustomAction(int deviceNum)
        {
            return _config.ContainsCustomAction[deviceNum];
        }

        public bool containsCustomExtras(int deviceNum)
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

        public void CalculateProfileActionDicts(int device)
        {
            _config.CalculateProfileActionDicts(device);
        }

        public void CacheProfileCustomsFlags(int device)
        {
            _config.CacheProfileCustomsFlags(device);
        }

        public void CacheExtraProfileInfo(int device)
        {
            _config.CacheExtraProfileInfo(device);
        }

        public X360Controls GetX360ControlsByName(string key)
        {
            return _config.GetX360ControlsByName(key);
        }

        public string GetX360ControlString(X360Controls key)
        {
            return _config.GetX360ControlString(key);
        }

        public DS4Controls GetDS4ControlsByName(string key)
        {
            return _config.GetDs4ControlsByName(key);
        }

        public X360Controls GetDefaultX360ControlBinding(DS4Controls dc)
        {
            return DefaultButtonMapping[(int)dc];
        }

        public bool containsLinkedProfile(string serial)
        {
            var tempSerial = serial.Replace(":", string.Empty);
            return _config.linkedProfiles.ContainsKey(tempSerial);
        }

        public string getLinkedProfile(string serial)
        {
            var temp = string.Empty;
            var tempSerial = serial.Replace(":", string.Empty);
            if (_config.linkedProfiles.ContainsKey(tempSerial)) temp = _config.linkedProfiles[tempSerial];

            return temp;
        }

        public void changeLinkedProfile(string serial, string profile)
        {
            var tempSerial = serial.Replace(":", string.Empty);
            _config.linkedProfiles[tempSerial] = profile;
        }

        public void removeLinkedProfile(string serial)
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