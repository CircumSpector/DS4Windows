﻿using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

namespace DS4Windows
{
    public interface IBackingStore
    {
        string ProfilesPath { get; set; }

        string ActionsPath { get; set; }

        string LinkedProfilesPath { get; set; }

        string ControllerConfigsPath { get; set; }

        bool Ds4Mapping { get; set; }

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

        IList<string> OlderProfilePath { get; set; }

        IList<bool> DistanceProfiles { get; set; }

        IList<StickDeadZoneInfo> LSModInfo { get; set; }

        IList<StickDeadZoneInfo> RSModInfo { get; set; }

        IList<TriggerDeadZoneZInfo> L2ModInfo { get; set; }

        IList<TriggerDeadZoneZInfo> R2ModInfo { get; set; }

        IList<BezierCurve> LSOutBezierCurveObj { get; set; }

        IList<BezierCurve> RSOutBezierCurveObj { get; set; }

        IList<BezierCurve> L2OutBezierCurveObj { get; set; }

        IList<BezierCurve> R2OutBezierCurveObj { get; set; }

        IList<BezierCurve> SXOutBezierCurveObj { get; set; }

        IList<BezierCurve> SZOutBezierCurveObj { get; set; }

        IList<GyroMouseInfo> GyroMouseInfo { get; set; }
        
        IList<TouchPadAbsMouseSettings> TouchPadAbsMouse { get; set; }

        IList<TouchPadRelMouseSettings> TouchPadRelMouse { get; set; }

        IList<int> GyroMouseDeadZone { get; set; }

        IList<int> IdleDisconnectTimeout { get; set; }

        string FakeExeFileName { get; set; }

        IList<bool> ContainsCustomAction { get; set; }

        IList<bool> ContainsCustomExtras { get; set; }

        IList<TouchpadOutMode> TouchOutMode { get; set; }

        IList<IList<int>> TouchDisInvertTriggers { get; set; }

        IList<bool> SATriggerCondition { get; set; }

        IList<string> SATriggers { get; set; }

        IList<string> SAMouseStickTriggers { get; set; }

        IList<GyroMouseStickInfo> GyroMouseStickInfo { get; set; }

        IList<GyroDirectionalSwipeInfo> GyroSwipeInfo { get; set; }

        ulong LastVersionCheckedNumber { get; set; }

        int StickOutputCurveId(string name);

        bool SaTriggerCondValue(string text);

        string SaTriggerCondString(bool value);

        void RefreshExtrasButtons(int deviceNum, List<DS4Controls> devButtons);

        bool LoadControllerConfigs(DS4Device device = null);

        bool SaveControllerConfigs(DS4Device device = null);

        SpecialAction GetProfileAction(int device, string name);

        bool ContainsLinkedProfile(PhysicalAddress serial);

        string GetLinkedProfile(PhysicalAddress serial);

        void ChangeLinkedProfile(PhysicalAddress serial, string profile);

        void RemoveLinkedProfile(PhysicalAddress serial);

        int GetProfileActionIndexOf(int device, string name);

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

        string GetSAMouseStickTriggers(int device);

        GyroMouseStickInfo GetGyroMouseStickInfo(int device);

        GyroDirectionalSwipeInfo GetGyroSwipeInfo(int device);

        bool GetSATriggerCondition(int index);

        string GetSATriggers(int index);

        bool IsUsingTouchpadForControls(int index);

        bool IsUsingSAForControls(int index);

        public string AxisOutputCurveString(int id);

        int GetIdleDisconnectTimeout(int index);

        ControlSettingsGroup GetControlSettingsGroup(int deviceNum);

        TriggerDeadZoneZInfo GetL2ModInfo(int index);

        TriggerDeadZoneZInfo GetR2ModInfo(int index);

        int GetLSDeadZone(int index);

        int GetRSDeadZone(int index);

        StickDeadZoneInfo GetLSDeadInfo(int index);

        StickDeadZoneInfo GetRSDeadInfo(int index);

        SquareStickInfo GetSquareStickInfo(int device);

        StickAntiSnapbackInfo GetLSAntiSnapbackInfo(int device);

        StickAntiSnapbackInfo GetRSAntiSnapbackInfo(int device);
        
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
            string profilePath = "", bool xinputChange = true, bool postLoad = true);

        bool SaveAction(string name, string controls, int mode, string details, bool edit, string extras = "");

        void RemoveAction(string name);

        bool LoadActions();

        bool LoadLinkedProfiles();

        bool SaveLinkedProfiles();

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