using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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

        AppThemeChoice ThemeChoice { get; set; }

        ControlServiceDeviceOptions DeviceOptions { get; }

        IList<OutContType> OutputDeviceType { get; set; }

        int CheckWhen { get; set; }

        DateTime LastChecked { get; set; }

        bool DisconnectBluetoothAtStop { get; set; }

        int Notifications { get; set; }

        bool Ds4Mapping { get; set; }

        bool QuickCharge { get; set; }

        bool CloseMini { get; set; }

        bool StartMinimized { get; set; }

        bool MinToTaskBar { get; set; }

        int FormWidth { get; set; }

        int FormHeight { get; set; }

        int FormLocationX { get; set; }

        int FormLocationY { get; set; }

        string UseLang { get; set; }

        bool DownloadLang { get; set; }

        TrayIconChoice UseIconChoice { get; set; }

        bool FlashWhenLate { get; set; }

        bool UseCustomSteamFolder { get; set; }

        IList<string> LaunchProgram { get; set; }

        IList<string> ProfilePath { get; set; }

        IList<string>[] ProfileActions { get; set; }

        IList<SquareStickInfo> SquStickInfo { get; set; }

        IList<SpecialAction> Actions { get; set; }

        IList<StickAntiSnapbackInfo> LSAntiSnapbackInfo { get; set; }

        IList<StickAntiSnapbackInfo> RSAntiSnapbackInfo { get; set; }

        IList<StickOutputSetting> LSOutputSettings { get; set; }

        IList<StickOutputSetting> RSOutputSettings { get; set; }

        IList<TriggerOutputSettings> L2OutputSettings { get; set; }

        IList<TriggerOutputSettings> R2OutputSettings { get; set; }

        IList<SteeringWheelSmoothingInfo> WheelSmoothInfo { get; set; }

        IList<ButtonMouseInfo> ButtonMouseInfos { get; set; }

        IList<string> OlderProfilePath { get; set; }

        IList<bool> DistanceProfiles { get; set; }

        IList<byte> RumbleBoost { get; set; }

        IList<int> RumbleAutostopTime { get; }

        IList<byte> TouchSensitivity { get; set; }

        IList<StickDeadZoneInfo> LSModInfo { get; set; }

        IList<StickDeadZoneInfo> RSModInfo { get; set; }

        IList<TriggerDeadZoneZInfo> L2ModInfo { get; set; }

        IList<TriggerDeadZoneZInfo> R2ModInfo { get; set; }

        IList<double> L2Sens { get; set; }

        IList<double> R2Sens { get; set; }

        IList<double> SXSens { get; set; }

        IList<double> SZSens { get; set; }

        IList<double> SXDeadzone { get; set; }

        IList<double> SXMaxzone { get; set; }

        IList<double> SXAntiDeadzone { get; set; }

        IList<double> SZDeadzone { get; set; }

        IList<double> SZAntiDeadzone { get; set; }

        IList<double> SZMaxzone { get; set; }

        IList<double> LSSens { get; set; }

        IList<double> RSSens { get; set; }

        IList<bool> LowerRCOn { get; set; }

        IList<double> LSRotation { get; set; }

        IList<double> RSRotation { get; set; }

        IList<BezierCurve> LSOutBezierCurveObj { get; set; }

        IList<BezierCurve> RSOutBezierCurveObj { get; set; }

        IList<BezierCurve> L2OutBezierCurveObj { get; set; }

        IList<BezierCurve> R2OutBezierCurveObj { get; set; }

        IList<BezierCurve> SXOutBezierCurveObj { get; set; }

        IList<BezierCurve> SZOutBezierCurveObj { get; set; }

        IList<int> GyroInvert { get; set; }

        IList<bool> GyroTriggerTurns { get; set; }

        IList<GyroMouseInfo> GyroMouseInfo { get; set; }

        IList<int> GyroMouseHorizontalAxis { get; set; }

        IList<int> GyroMouseStickHorizontalAxis { get; set; }

        IList<bool> TrackballMode { get; set; }

        IList<double> TrackballFriction { get; set; }

        IList<TouchPadAbsMouseSettings> TouchPadAbsMouse { get; set; }

        IList<TouchPadRelMouseSettings> TouchPadRelMouse { get; set; }

        IList<byte> TapSensitivity { get; set; }

        IList<bool> DoubleTap { get; set; }

        IList<int> ScrollSensitivity { get; set; }

        IList<int> TouchPadInvert { get; set; }

        IList<int> BluetoothPollRate { get; set; }

        IList<int> GyroMouseDeadZone { get; set; }

        IList<bool> GyroMouseToggle { get; set; }

        IList<bool> EnableTouchToggle { get; set; }

        IList<int> IdleDisconnectTimeout { get; set; }

        IList<bool> EnableOutputDataToDS4 { get; set; }

        IList<bool> TouchpadJitterCompensation { get; set; }

        IList<bool> TouchClickPassthru { get; set; }

        double UdpSmoothingMincutoff { get; set; }

        double UdpSmoothingBeta { get; set; }

        string FakeExeFileName { get; set; }

        IList<bool> ContainsCustomAction { get; set; }

        IList<bool> ContainsCustomExtras { get; set; }

        IList<int> GyroSensitivity { get; set; }

        IList<int> GyroSensVerticalScale { get; set; }

        IList<bool> DirectInputOnly { get; set; }

        IList<TouchpadOutMode> TouchOutMode { get; set; }

        IList<IList<int>> TouchDisInvertTriggers { get; set; }

        IList<LightbarSettingInfo> LightbarSettingInfo { get; set; }

        IList<int> SASteeringWheelEmulationRange { get; set; }

        IList<bool> SATriggerCondition { get; set; }

        IList<string> SATriggers { get; set; }

        IList<bool> StartTouchpadOff { get; set; }

        IList<bool> SAMouseStickTriggerCond { get; set; }

        IList<string> SAMouseStickTriggers { get; set; }

        IList<SASteeringWheelEmulationAxisType> SASteeringWheelEmulationAxis { get; set; }

        bool UseExclusiveMode { get; set; }

        bool SwipeProfiles { get; set; }

        int FlashWhenLateAt { get; set; }

        bool AutoProfileRevertDefaultProfile { get; set; }

        IList<GyroOutMode> GyroOutputMode { get; set; }

        IList<bool> GyroMouseStickTriggerTurns { get; set; }

        IList<GyroMouseStickInfo> GyroMouseStickInfo { get; set; }

        IList<bool> GyroMouseStickToggle { get; set; }

        IList<GyroDirectionalSwipeInfo> GyroSwipeInfo { get; set; }

        IList<GyroControlsInfo> GyroControlsInfo { get; set; }

        IList<int> SAWheelFuzzValues { get; set; }

        ulong LastVersionCheckedNumber { get; set; }

        bool IsUdpServerEnabled { get; set; }

        int StickOutputCurveId(string name);

        bool SaTriggerCondValue(string text);

        string SaTriggerCondString(bool value);

        void RefreshExtrasButtons(int deviceNum, List<DS4Controls> devButtons);

        bool LoadControllerConfigs(DS4Device device = null);

        bool SaveControllerConfigs(DS4Device device = null);

        SpecialAction GetProfileAction(int device, string name);

        bool ContainsLinkedProfile(string serial);

        string GetLinkedProfile(string serial);

        void ChangeLinkedProfile(string serial, string profile);

        void RemoveLinkedProfile(string serial);

        int GetProfileActionIndexOf(int device, string name);

        DS4Color GetMainColor(int index);

        DS4Color GetLowColor(int index);

        DS4Color GetChargingColor(int index);

        DS4Color GetFlashColor(int index);

        int GetLsOutCurveMode(int index);

        void SetLsOutCurveMode(int index, int value);

        int GetRsOutCurveMode(int index);

        void SetRsOutCurveMode(int index, int value);

        int GetL2OutCurveMode(int index);

        void SetL2OutCurveMode(int index, int value);

        int GetR2OutCurveMode(int index);

        void SetR2OutCurveMode(int index, int value);

        int GetSXOutCurveMode(int index);

        void SetSXOutCurveMode(int index, int value);

        int GetSZOutCurveMode(int index);

        void SetSZOutCurveMode(int index, int value);

        GyroOutMode GetGyroOutMode(int device);

        string GetSAMouseStickTriggers(int device);

        bool GetSAMouseStickTriggerCond(int device);

        bool GetGyroMouseStickTriggerTurns(int device);

        int GetGyroMouseStickHorizontalAxis(int index);

        GyroMouseStickInfo GetGyroMouseStickInfo(int device);

        GyroDirectionalSwipeInfo GetGyroSwipeInfo(int device);

        bool GetSATriggerCondition(int index);

        string GetSATriggers(int index);

        LightbarSettingInfo GetLightbarSettingsInfo(int index);

        bool GetDirectInputOnly(int index);

        bool IsUsingTouchpadForControls(int index);

        bool IsUsingSAForControls(int index);

        SASteeringWheelEmulationAxisType GetSASteeringWheelEmulationAxis(int index);

        public string AxisOutputCurveString(int id);

        int GetSASteeringWheelEmulationRange(int index);

        int GetGyroSensitivity(int index);

        int GetGyroSensVerticalScale(int index);

        int GetGyroInvert(int index);

        bool GetGyroTriggerTurns(int index);

        int GetGyroMouseHorizontalAxis(int index);

        byte GetRumbleBoost(int index);

        int GetRumbleAutostopTime(int index);

        bool GetEnableTouchToggle(int index);

        int GetIdleDisconnectTimeout(int index);

        bool GetEnableOutputDataToDS4(int index);

        byte GetTouchSensitivity(int index);

        ControlSettingsGroup GetControlSettingsGroup(int deviceNum);

        GyroControlsInfo GetGyroControlsInfo(int index);

        byte GetTapSensitivity(int index);

        bool GetDoubleTap(int index);

        bool GetTouchPadJitterCompensation(int index);

        int GetTouchPadInvert(int index);
        TriggerDeadZoneZInfo GetL2ModInfo(int index);

        TriggerDeadZoneZInfo GetR2ModInfo(int index);

        double GetSXDeadZone(int index);

        double GetSZDeadZone(int index);

        int GetLSDeadZone(int index);

        int GetRSDeadZone(int index);

        StickDeadZoneInfo GetLSDeadInfo(int index);

        StickDeadZoneInfo GetRSDeadInfo(int index);

        double GetSXAntiDeadZone(int index);

        double GetSZAntiDeadZone(int index);

        double GetSXMaxZone(int index);

        double GetSZMaxZone(int index);

        double GetLSRotation(int index);

        double GetRSRotation(int index);

        double GetL2Sens(int index);

        double GetR2Sens(int index);

        double GetSXSens(int index);

        double GetSZSens(int index);

        double GetLSSens(int index);

        double GetRSSens(int index);

        int GetBluetoothPollRate(int index);

        SquareStickInfo GetSquareStickInfo(int device);

        StickAntiSnapbackInfo GetLSAntiSnapbackInfo(int device);

        StickAntiSnapbackInfo GetRSAntiSnapbackInfo(int device);

        bool GetTrackballMode(int index);

        double GetTrackballFriction(int index);

        int GetProfileActionCount(int index);

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

        Task<bool> SaveAsNewProfile(int device, string proName);

        Task<bool> SaveProfile(int device, string proName);

        DS4Controls GetDs4ControlsByName(string key);

        X360Controls GetX360ControlsByName(string key);

        string StickOutputCurveString(int id);

        string GetX360ControlString(X360Controls key);

        Task<bool> LoadProfile(int device, bool launchprogram, ControlService control,
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