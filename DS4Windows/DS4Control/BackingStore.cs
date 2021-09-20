using System;
using System.Collections.Generic;

namespace DS4Windows
{
    public interface IBackingStore
    {
        string ProfilesPath { get; set; }

        string ActionsPath { get; set; }

        string LinkedProfilesPath { get; set; }

        string ControllerConfigsPath { get; set; }

        int UdpServerPort { get; set; }

        /// <summary>
        ///     127.0.0.1=IPAddress.Loopback (default), 0.0.0.0=IPAddress.Any as all interfaces, x.x.x.x = Specific ipv4 interface
        ///     address or hostname
        /// </summary>
        string UdpServerListenAddress { get; set; }

        bool UseUdpSmoothing { get; set; }

        string CustomSteamFolder { get; set; }

        public AppThemeChoice ThemeChoice { get; set; }

        public ControlServiceDeviceOptions DeviceOptions { get; }

        public IList<OutContType> OutputDeviceType { get; set; }

        public int CheckWhen { get; set; }

        public DateTime LastChecked { get; set; }

        public bool DisconnectBluetoothAtStop { get; set; }

        public int Notifications { get; set; }

        public bool Ds4Mapping { get; set; }

        public bool QuickCharge { get; set; }

        public bool CloseMini { get; set; }

        public bool StartMinimized { get; set; }

        public bool MinToTaskBar { get; set; }

        public int FormWidth { get; set; }

        public int FormHeight { get; set; }

        public int FormLocationX { get; set; }

        public int FormLocationY { get; set; }

        public string UseLang { get; set; }

        public bool DownloadLang { get; set; }

        public TrayIconChoice UseIconChoice { get; set; }

        public bool FlashWhenLate { get; set; }

        public bool UseCustomSteamFolder { get; set; }

        public IList<string> LaunchProgram { get; set; }

        public IList<string> ProfilePath { get; set; }

        public IList<string>[] ProfileActions { get; set; }

        public IList<SquareStickInfo> SquStickInfo { get; set; }

        public IList<SpecialAction> Actions { get; set; }

        public IList<StickAntiSnapbackInfo> LSAntiSnapbackInfo { get; set; }

        public IList<StickAntiSnapbackInfo> RSAntiSnapbackInfo { get; set; }

        public IList<StickOutputSetting> LSOutputSettings { get; set; }

        public IList<StickOutputSetting> RSOutputSettings { get; set; }

        public IList<TriggerOutputSettings> L2OutputSettings { get; set; }

        public IList<TriggerOutputSettings> R2OutputSettings { get; set; }

        public IList<SteeringWheelSmoothingInfo> WheelSmoothInfo { get; set; }

        public IList<ButtonMouseInfo> ButtonMouseInfos { get; set; }

        public IList<string> OlderProfilePath { get; set; }

        public IList<bool> DistanceProfiles { get; set; }

        public IList<byte> RumbleBoost { get; set; }

        public IList<byte> TouchSensitivity { get; set; }

        public IList<StickDeadZoneInfo> LSModInfo { get; set; }

        public IList<StickDeadZoneInfo> RSModInfo { get; set; }

        public IList<TriggerDeadZoneZInfo> L2ModInfo { get; set; }

        public IList<TriggerDeadZoneZInfo> R2ModInfo { get; set; }

        public IList<double> L2Sens { get; set; }

        public IList<double> R2Sens { get; set; }

        public IList<double> SXSens { get; set; }

        public IList<double> SZSens { get; set; }

        public IList<double> SXDeadzone { get; set; }

        public IList<double> SXMaxzone { get; set; }

        public IList<double> SXAntiDeadzone { get; set; }

        public IList<double> SZDeadzone { get; set; }

        public IList<double> SZAntiDeadzone { get; set; }

        public IList<double> SZMaxzone { get; set; }

        public IList<double> LSSens { get; set; }

        public IList<double> RSSens { get; set; }

        public IList<bool> LowerRCOn { get; set; }

        public IList<double> LSRotation { get; set; }

        public IList<double> RSRotation { get; set; }

        public IList<BezierCurve> LSOutBezierCurveObj { get; set; }

        public IList<BezierCurve> RSOutBezierCurveObj { get; set; }

        public IList<BezierCurve> L2OutBezierCurveObj { get; set; }

        public IList<BezierCurve> R2OutBezierCurveObj { get; set; }

        public IList<BezierCurve> SXOutBezierCurveObj { get; set; }

        public IList<BezierCurve> SZOutBezierCurveObj { get; set; }

        public IList<int> GyroInvert { get; set; }

        public IList<bool> GyroTriggerTurns { get; set; }

        public IList<GyroMouseInfo> GyroMouseInfo { get; set; }

        public IList<int> GyroMouseHorizontalAxis { get; set; }

        public IList<int> GyroMouseStickHorizontalAxis { get; set; }

        public IList<bool> TrackballMode { get; set; }

        public IList<double> TrackballFriction { get; set; }

        public IList<TouchpadAbsMouseSettings> TouchPadAbsMouse { get; set; }

        public IList<TouchpadRelMouseSettings> TouchPadRelMouse { get; set; }

        public IList<byte> TapSensitivity { get; set; }

        public IList<bool> DoubleTap { get; set; }

        public IList<int> ScrollSensitivity { get; set; }

        public IList<int> TouchPadInvert { get; set; }

        public IList<int> BluetoothPollRate { get; set; }

        public IList<int> GyroMouseDeadZone { get; set; }

        public IList<bool> GyroMouseToggle { get; set; }

        public IList<bool> EnableTouchToggle { get; set; }

        public IList<int> IdleDisconnectTimeout { get; set; }

        public IList<bool> EnableOutputDataToDS4 { get; set; }

        public IList<bool> TouchpadJitterCompensation { get; set; }

        public IList<bool> TouchClickPassthru { get; set; }

        public double UdpSmoothingMincutoff { get; set; }

        public double UdpSmoothingBeta { get; set; }

        public string FakeExeFileName { get; set; }

        public IList<bool> ContainsCustomAction { get; set; }

        public IList<bool> ContainsCustomExtras { get; set; }

        public IList<int> GyroSensitivity { get; set; }

        public IList<int> GyroSensVerticalScale { get; set; }

        public IList<bool> DirectInputOnly { get; set; }

        public IList<TouchpadOutMode> TouchOutMode { get; set; }

        public IList<IList<int>> TouchDisInvertTriggers { get; set; }

        public IList<LightbarSettingInfo> LightbarSettingInfo { get; set; }

        public IList<int> SASteeringWheelEmulationRange { get; set; }

        public IList<bool> SATriggerCond { get; set; }

        public IList<string> SATriggers { get; set; }

        public IList<bool> StartTouchpadOff { get; set; }

        public IList<bool> SAMouseStickTriggerCond { get; set; }

        public IList<string> SAMouseStickTriggers { get; set; }

        public IList<SASteeringWheelEmulationAxisType> SASteeringWheelEmulationAxis { get; set; }

        int getLsOutCurveMode(int index);
        void setLsOutCurveMode(int index, int value);
        int getRsOutCurveMode(int index);
        void setRsOutCurveMode(int index, int value);
        int getL2OutCurveMode(int index);
        void setL2OutCurveMode(int index, int value);
        int getR2OutCurveMode(int index);
        void setR2OutCurveMode(int index, int value);
        int getSXOutCurveMode(int index);
        void setSXOutCurveMode(int index, int value);
        int getSZOutCurveMode(int index);
        void setSZOutCurveMode(int index, int value);
        void EstablishDefaultSpecialActions(int idx);
        void CacheProfileCustomsFlags(int device);
        void CacheExtraProfileInfo(int device);
        void CalculateProfileActionCount(int index);
        void CalculateProfileActionDicts(int device);
        SpecialAction GetAction(string name);
        int GetActionIndexOf(string name);
        void SetSaTriggerCond(int index, string text);
        void SetSaMouseStickTriggerCond(int index, string text);
        void SetGyroMouseDZ(int index, int value, ControlService control);
        void SetGyroControlsToggle(int index, bool value, ControlService control);
        void SetGyroMouseToggle(int index, bool value, ControlService control);
        void SetGyroMouseStickToggle(int index, bool value, ControlService control);
        bool SaveAsNewProfile(int device, string proName);
        bool SaveProfile(int device, string proName);
        DS4Controls GetDs4ControlsByName(string key);
        X360Controls GetX360ControlsByName(string key);
        string GetX360ControlString(X360Controls key);

        bool LoadProfile(int device, bool launchprogram, ControlService control,
            string propath = "", bool xinputChange = true, bool postLoad = true);

        bool Load();
        bool Save();
        bool SaveAction(string name, string controls, int mode, string details, bool edit, string extras = "");
        void RemoveAction(string name);
        bool LoadActions();
        bool CreateLinkedProfiles();
        bool LoadLinkedProfiles();
        bool SaveLinkedProfiles();
        bool CreateControllerConfigs();
        bool LoadControllerConfigsForDevice(DS4Device device);
        bool SaveControllerConfigsForDevice(DS4Device device);

        void UpdateDs4ControllerSetting(int deviceNum, string buttonName, bool shift, object action, string exts,
            DS4KeyType kt, int trigger = 0);

        void UpdateDs4ControllerExtra(int deviceNum, string buttonName, bool shift, string exts);
        ControlActionData GetDs4Action(int deviceNum, string buttonName, bool shift);
        ControlActionData GetDs4Action(int deviceNum, DS4Controls dc, bool shift);
        string GetDs4Extra(int deviceNum, string buttonName, bool shift);
        DS4KeyType GetDs4KeyType(int deviceNum, string buttonName, bool shift);
        int GetDs4STrigger(int deviceNum, string buttonName);
        int GetDs4STrigger(int deviceNum, DS4Controls dc);
        DS4ControlSettings GetDs4ControllerSetting(int deviceNum, string buttonName);
        DS4ControlSettings GetDs4ControllerSetting(int deviceNum, DS4Controls dc);
        bool HasCustomActions(int deviceNum);
        bool HasCustomExtras(int deviceNum);

        void LoadBlankDs4Profile(int device, bool launchprogram, ControlService control,
            string propath = "", bool xinputChange = true, bool postLoad = true);

        void LoadBlankProfile(int device, bool launchprogram, ControlService control,
            string propath = "", bool xinputChange = true, bool postLoad = true);

        void LoadDefaultGamepadGyroProfile(int device, bool launchprogram, ControlService control,
            string propath = "", bool xinputChange = true, bool postLoad = true);

        void LoadDefaultDS4GamepadGyroProfile(int device, bool launchprogram, ControlService control,
            string propath = "", bool xinputChange = true, bool postLoad = true);

        void LoadDefaultMixedGyroMouseProfile(int device, bool launchprogram, ControlService control,
            string propath = "", bool xinputChange = true, bool postLoad = true);

        void LoadDefaultDs4MixedGyroMouseProfile(int device, bool launchprogram, ControlService control,
            string propath = "", bool xinputChange = true, bool postLoad = true);

        void LoadDefaultDS4MixedControlsProfile(int device, bool launchprogram, ControlService control,
            string propath = "", bool xinputChange = true, bool postLoad = true);

        void LoadDefaultMixedControlsProfile(int device, bool launchprogram, ControlService control,
            string propath = "", bool xinputChange = true, bool postLoad = true);

        void LoadDefaultKBMProfile(int device, bool launchprogram, ControlService control,
            string propath = "", bool xinputChange = true, bool postLoad = true);

        void LoadDefaultKBMGyroMouseProfile(int device, bool launchprogram, ControlService control,
            string propath = "", bool xinputChange = true, bool postLoad = true);
    }
}