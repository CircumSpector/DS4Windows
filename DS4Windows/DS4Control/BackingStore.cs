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

        AppThemeChoice ThemeChoice { get; set; }

        ControlServiceDeviceOptions DeviceOptions { get; }

        List<OutContType> OutputDeviceType { get; set; }

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