using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml;
using DS4Windows.DS4Control;

namespace DS4Windows
{
    public partial class Global
    {
        // Use 15 minutes for default Idle Disconnect when initially enabling the option
        public const int DEFAULT_ENABLE_IDLE_DISCONN_MINS = 15;
        public const int MAX_DS4_CONTROLLER_COUNT = 8;
        public const int TEST_PROFILE_ITEM_COUNT = MAX_DS4_CONTROLLER_COUNT + 1;
        public const int TEST_PROFILE_INDEX = TEST_PROFILE_ITEM_COUNT - 1;
        public const int OLD_XINPUT_CONTROLLER_COUNT = 4;
        public const byte DS4_STICK_AXIS_MIDPOINT = 128;

        private const string BLANK_VIGEMBUS_VERSION = "0.0.0.0";
        private const string MIN_SUPPORTED_VIGEMBUS_VERSION = "1.16.112.0";
        public const string BLANK_FAKERINPUT_VERSION = "0.0.0.0";

        public const int CONFIG_VERSION = 5;
        public const int APP_CONFIG_VERSION = 2;
        public const string ASSEMBLY_RESOURCE_PREFIX = "pack://application:,,,/DS4Windows;";
        public const string RESOURCES_PREFIX = "/DS4Windows;component/Resources";
        public const string CUSTOM_EXE_CONFIG_FILENAME = "custom_exe_name.txt";
        public const string XML_EXTENSION = ".xml";
        private static readonly Lazy<Global> LazyInstance = new(() => new Global());

        protected static int m_IdleTimeout = 600000;

        public static string[] TempProfileNames = new string[TEST_PROFILE_ITEM_COUNT]
        {
            string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty,
            string.Empty, string.Empty
        };

        public static bool[] UseTempProfiles = new bool[TEST_PROFILE_ITEM_COUNT]
            { false, false, false, false, false, false, false, false, false };

        public static bool[] TempProfileDistance = new bool[TEST_PROFILE_ITEM_COUNT]
            { false, false, false, false, false, false, false, false, false };

        public static bool[] UseDirectInputOnly = new bool[TEST_PROFILE_ITEM_COUNT]
            { true, true, true, true, true, true, true, true, true };

        public static bool[] LinkedProfileCheck = new bool[MAX_DS4_CONTROLLER_COUNT]
            { false, false, false, false, false, false, false, false };

        public static bool[] TouchpadActive = new bool[TEST_PROFILE_ITEM_COUNT]
            { true, true, true, true, true, true, true, true, true };

        

        public static bool hidHideInstalled = IsHidHideInstalled;
        public static bool fakerInputInstalled = IsFakerInputInstalled;
        public static string fakerInputVersion = FakerInputVersion();

        public static VirtualKBMBase outputKBMHandler;
        public static VirtualKBMMapping outputKBMMapping;
        
        // Create mapping array at runtime
        public static DS4Controls[] ReverseX360ButtonMapping = new Func<DS4Controls[]>(() =>
        {
            var temp = new DS4Controls[DefaultButtonMapping.Length];
            for (int i = 0, arlen = DefaultButtonMapping.Length; i < arlen; i++)
            {
                var mapping = DefaultButtonMapping[i];
                if (mapping != X360Controls.None) temp[(int)mapping] = (DS4Controls)i;
            }

            return temp;
        })();

        public static Dictionary<TrayIconChoice, string> IconChoiceResources = new()
        {
            [TrayIconChoice.Default] = "/DS4Windows;component/Resources/DS4W.ico",
            [TrayIconChoice.Colored] = "/DS4Windows;component/Resources/DS4W.ico",
            [TrayIconChoice.White] = "/DS4Windows;component/Resources/DS4W - White.ico",
            [TrayIconChoice.Black] = "/DS4Windows;component/Resources/DS4W - Black.ico"
        };

        private readonly BackingStore _config = new();

        private Global()
        {
        }

        /// <summary>
        ///     Singleton instance of <see cref="Global"/>.
        /// </summary>
        public static Global Instance => LazyInstance.Value;

        /// <summary>
        ///     Configuration data which gets persisted to disk.
        /// </summary>
        public IBackingStore Config => _config;

        public bool IsFirstRun { get; set; }

        public bool HasMultipleSaveSpots { get; set; }

        public bool RunHotPlug { get; set; } = false;

        public static bool IsViGEmInstalled { get; private set; }

        public static string ViGEmBusVersion { get; private set; } = BLANK_VIGEMBUS_VERSION;

        public static Version ViGEmBusVersionInfo => new(ViGEmBusVersion);

        private static Version MinimumSupportedViGEmBusVersionInfo => new(MIN_SUPPORTED_VIGEMBUS_VERSION);
        
        public static bool IsHidHideInstalled => CheckForSysDevice(@"root\HidHide");

        public static bool IsFakerInputInstalled => CheckForSysDevice(@"root\FakerInput");

        public static bool IsRunningSupportedViGEmBus => IsViGEmInstalled &&
                                                         MinimumSupportedViGEmBusVersionInfo.CompareTo(
                                                             ViGEmBusVersionInfo) <= 0;

        public string LastVersionChecked
        {
            get => _config.lastVersionChecked;
            set
            {
                _config.lastVersionChecked = value;
                _config.LastVersionCheckedNumber = CompileVersionNumberFromString(value);
            }
        }

        public ulong LastVersionCheckedNumber => _config.LastVersionCheckedNumber;

        public double UDPServerSmoothingMincutoff
        {
            get => _config.UdpSmoothingMincutoff;
            set
            {
                var temp = _config.UdpSmoothingMincutoff;
                if (temp == value) return;
                _config.UdpSmoothingMincutoff = value;
                UDPServerSmoothingMincutoffChanged?.Invoke(null, EventArgs.Empty);
            }
        }

        public double UDPServerSmoothingBeta
        {
            get => _config.UdpSmoothingBeta;
            set
            {
                var temp = _config.UdpSmoothingBeta;
                if (temp == value) return;
                _config.UdpSmoothingBeta = value;
                UDPServerSmoothingBetaChanged?.Invoke(null, EventArgs.Empty);
            }
        }

        public bool AutoProfileRevertDefaultProfile
        {
            set => _config.AutoProfileRevertDefaultProfile = value;
            get => _config.AutoProfileRevertDefaultProfile;
        }

        /// <summary>
        ///     Fake name used for user copy of DS4Windows.exe
        /// </summary>
        public string FakeExeName
        {
            get => _config.FakeExeFileName;
            set
            {
                var valid = !(value.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0);
                if (valid) _config.FakeExeFileName = value;
            }
        }

        public bool[] TouchActive => TouchpadActive;
        
        public GyroOutMode[] GyroOutputMode => _config.GyroOutMode;

        public bool[] GyroMouseStickTriggerTurns => _config.GyroMouseStickTriggerTurns;

        public GyroMouseStickInfo[] GyroMouseStickInf => _config.GyroMStickInfo;

        public GyroDirectionalSwipeInfo[] GyroSwipeInf => _config.GyroSwipeInfo;

        public bool[] GyroMouseStickToggle => _config.GyroMouseStickToggle;

        public GyroControlsInfo[] GyroControlsInf => _config.gyroControlsInf;

        public int[] SAWheelFuzzValues => _config.saWheelFuzzValues;

        public static string GetX360ControlString(X360Controls key, OutContType conType)
        {
            var result = string.Empty;

            switch (conType)
            {
                case OutContType.X360:
                    XboxDefaultNames.TryGetValue(key, out result);
                    break;
                case OutContType.DS4:
                    Ds4DefaultNames.TryGetValue(key, out result);
                    break;
            }

            return result;
        }

        public void SaveWhere(string path)
        {
            RuntimeAppDataPath = path;
            _config.ProfilesPath = Path.Combine(RuntimeAppDataPath, Constants.ProfilesFileName);
            _config.ActionsPath = Path.Combine(RuntimeAppDataPath, Constants.ActionsFileName);
            _config.LinkedProfilesPath = Path.Combine(RuntimeAppDataPath, Constants.LinkedProfilesFileName);
            _config.ControllerConfigsPath = Path.Combine(RuntimeAppDataPath, Constants.ControllerConfigsFileName);
        }

        public static bool SaveDefault(string path)
        {
            var Saved = true;
            var m_Xdoc = new XmlDocument();
            try
            {
                XmlNode Node;

                m_Xdoc.RemoveAll();

                Node = m_Xdoc.CreateXmlDeclaration("1.0", "utf-8", string.Empty);
                m_Xdoc.AppendChild(Node);

                Node = m_Xdoc.CreateComment(string.Format(" Profile Configuration Data. {0} ", DateTime.Now));
                m_Xdoc.AppendChild(Node);

                Node = m_Xdoc.CreateWhitespace("\r\n");
                m_Xdoc.AppendChild(Node);

                Node = m_Xdoc.CreateNode(XmlNodeType.Element, "Profile", null);

                m_Xdoc.AppendChild(Node);

                m_Xdoc.Save(path);
            }
            catch
            {
                Saved = false;
            }

            return Saved;
        }

        public static bool CheckForDevice(string guid)
        {
            var result = false;
            var deviceGuid = Guid.Parse(guid);
            var deviceInfoData =
                new NativeMethods.SP_DEVINFO_DATA();
            deviceInfoData.cbSize =
                Marshal.SizeOf(deviceInfoData);

            var deviceInfoSet = NativeMethods.SetupDiGetClassDevs(ref deviceGuid, null, 0,
                NativeMethods.DIGCF_DEVICEINTERFACE);
            result = NativeMethods.SetupDiEnumDeviceInfo(deviceInfoSet, 0, ref deviceInfoData);

            if (deviceInfoSet.ToInt64() != NativeMethods.INVALID_HANDLE_VALUE)
                NativeMethods.SetupDiDestroyDeviceInfoList(deviceInfoSet);

            return result;
        }

        private static void FindViGEmDeviceInfo()
        {
            var result = false;
            var deviceGuid = Constants.ViGemBusInterfaceGuid;
            var deviceInfoData =
                new NativeMethods.SP_DEVINFO_DATA();
            deviceInfoData.cbSize =
                Marshal.SizeOf(deviceInfoData);

            var dataBuffer = new byte[4096];
            ulong propertyType = 0;
            var requiredSize = 0;

            // Properties to retrieve
            NativeMethods.DEVPROPKEY[] lookupProperties =
            {
                NativeMethods.DEVPKEY_Device_DriverVersion, NativeMethods.DEVPKEY_Device_InstanceId,
                NativeMethods.DEVPKEY_Device_Manufacturer, NativeMethods.DEVPKEY_Device_Provider,
                NativeMethods.DEVPKEY_Device_DeviceDesc
            };

            var tempViGEmBusInfoList = new List<ViGEmBusInfo>();

            var deviceInfoSet = NativeMethods.SetupDiGetClassDevs(ref deviceGuid, null, 0,
                NativeMethods.DIGCF_DEVICEINTERFACE);
            for (var i = 0; !result && NativeMethods.SetupDiEnumDeviceInfo(deviceInfoSet, i, ref deviceInfoData); i++)
            {
                var tempBusInfo = new ViGEmBusInfo();

                foreach (var currentDevKey in lookupProperties)
                {
                    var tempKey = currentDevKey;
                    if (NativeMethods.SetupDiGetDeviceProperty(deviceInfoSet, ref deviceInfoData,
                        ref tempKey, ref propertyType,
                        dataBuffer, dataBuffer.Length, ref requiredSize, 0))
                    {
                        var temp = dataBuffer.ToUTF16String();
                        if (currentDevKey.fmtid == NativeMethods.DEVPKEY_Device_DriverVersion.fmtid &&
                            currentDevKey.pid == NativeMethods.DEVPKEY_Device_DriverVersion.pid)
                            try
                            {
                                tempBusInfo.deviceVersion = new Version(temp);
                                tempBusInfo.deviceVersionStr = temp;
                            }
                            catch (ArgumentException)
                            {
                                // Default to unknown version
                                tempBusInfo.deviceVersionStr = BLANK_VIGEMBUS_VERSION;
                                tempBusInfo.deviceVersion = new Version(tempBusInfo.deviceVersionStr);
                            }
                        else if (currentDevKey.fmtid == NativeMethods.DEVPKEY_Device_InstanceId.fmtid &&
                                 currentDevKey.pid == NativeMethods.DEVPKEY_Device_InstanceId.pid)
                            tempBusInfo.instanceId = temp;
                        else if (currentDevKey.fmtid == NativeMethods.DEVPKEY_Device_Manufacturer.fmtid &&
                                 currentDevKey.pid == NativeMethods.DEVPKEY_Device_Manufacturer.pid)
                            tempBusInfo.manufacturer = temp;
                        else if (currentDevKey.fmtid == NativeMethods.DEVPKEY_Device_Provider.fmtid &&
                                 currentDevKey.pid == NativeMethods.DEVPKEY_Device_Provider.pid)
                            tempBusInfo.driverProviderName = temp;
                        else if (currentDevKey.fmtid == NativeMethods.DEVPKEY_Device_DeviceDesc.fmtid &&
                                 currentDevKey.pid == NativeMethods.DEVPKEY_Device_DeviceDesc.pid)
                            tempBusInfo.deviceName = temp;
                    }
                }

                tempViGEmBusInfoList.Add(tempBusInfo);
            }

            if (deviceInfoSet.ToInt64() != NativeMethods.INVALID_HANDLE_VALUE)
                NativeMethods.SetupDiDestroyDeviceInfoList(deviceInfoSet);

            // Iterate over list and find most recent version number
            //IEnumerable<ViGEmBusInfo> tempResults = tempViGEmBusInfoList.Where(item => MinimumSupportedViGEmBusVersionInfo.CompareTo(item.deviceVersion) <= 0);
            var latestKnown = new Version(BLANK_VIGEMBUS_VERSION);
            var deviceInstanceId = string.Empty;
            foreach (var item in tempViGEmBusInfoList)
                if (latestKnown.CompareTo(item.deviceVersion) <= 0)
                {
                    latestKnown = item.deviceVersion;
                    deviceInstanceId = item.instanceId;
                }

            // Get bus info for most recent version found and save info
            var latestBusInfo =
                tempViGEmBusInfoList.SingleOrDefault(item => item.instanceId == deviceInstanceId);
            PopulateFromViGEmBusInfo(latestBusInfo);
        }

        private static bool CheckForSysDevice(string searchHardwareId)
        {
            var result = false;
            var sysGuid = Constants.SystemDeviceClassGuid;
            var deviceInfoData =
                new NativeMethods.SP_DEVINFO_DATA();
            deviceInfoData.cbSize =
                Marshal.SizeOf(deviceInfoData);
            var dataBuffer = new byte[4096];
            ulong propertyType = 0;
            var requiredSize = 0;
            var deviceInfoSet = NativeMethods.SetupDiGetClassDevs(ref sysGuid, null, 0, 0);
            for (var i = 0; !result && NativeMethods.SetupDiEnumDeviceInfo(deviceInfoSet, i, ref deviceInfoData); i++)
                if (NativeMethods.SetupDiGetDeviceProperty(deviceInfoSet, ref deviceInfoData,
                    ref NativeMethods.DEVPKEY_Device_HardwareIds, ref propertyType,
                    dataBuffer, dataBuffer.Length, ref requiredSize, 0))
                {
                    var hardwareId = dataBuffer.ToUTF16String();
                    //if (hardwareIds.Contains("Virtual Gamepad Emulation Bus"))
                    //    result = true;
                    if (hardwareId.Equals(searchHardwareId))
                        result = true;
                }

            if (deviceInfoSet.ToInt64() != NativeMethods.INVALID_HANDLE_VALUE)
                NativeMethods.SetupDiDestroyDeviceInfoList(deviceInfoSet);

            return result;
        }

        internal static string GetDeviceProperty(string deviceInstanceId,
            NativeMethods.DEVPROPKEY prop)
        {
            var result = string.Empty;
            var deviceInfoData = new NativeMethods.SP_DEVINFO_DATA();
            deviceInfoData.cbSize = Marshal.SizeOf(deviceInfoData);
            var dataBuffer = new byte[4096];
            ulong propertyType = 0;
            var requiredSize = 0;

            var hidGuid = new Guid();
            NativeMethods.HidD_GetHidGuid(ref hidGuid);
            var deviceInfoSet = NativeMethods.SetupDiGetClassDevs(ref hidGuid, deviceInstanceId, 0,
                NativeMethods.DIGCF_PRESENT | NativeMethods.DIGCF_DEVICEINTERFACE);
            NativeMethods.SetupDiEnumDeviceInfo(deviceInfoSet, 0, ref deviceInfoData);
            if (NativeMethods.SetupDiGetDeviceProperty(deviceInfoSet, ref deviceInfoData, ref prop, ref propertyType,
                dataBuffer, dataBuffer.Length, ref requiredSize, 0))
                result = dataBuffer.ToUTF16String();

            if (deviceInfoSet.ToInt64() != NativeMethods.INVALID_HANDLE_VALUE)
                NativeMethods.SetupDiDestroyDeviceInfoList(deviceInfoSet);

            return result;
        }

        public static bool CheckHidHideAffectedStatus(string deviceInstanceId,
            HashSet<string> affectedDevs, HashSet<string> exemptedDevices, bool force = false)
        {
            var result = false;
            var tempDeviceInstanceId = deviceInstanceId.ToUpper();
            result = affectedDevs.Contains(tempDeviceInstanceId);
            return result;
        }

        public static bool IsViGEmBusInstalled()
        {
            return IsViGEmInstalled;
        }

        public static void RefreshViGEmBusInfo()
        {
            FindViGEmDeviceInfo();
        }

        public static void RefreshHidHideInfo()
        {
            hidHideInstalled = IsHidHideInstalled;
        }

        private static void PopulateFromViGEmBusInfo(ViGEmBusInfo busInfo)
        {
            if (busInfo != null)
            {
                IsViGEmInstalled = true;
                ViGEmBusVersion = busInfo.deviceVersionStr;
            }
            else
            {
                IsViGEmInstalled = false;
                ViGEmBusVersion = BLANK_VIGEMBUS_VERSION;
            }
        }

        private static string FakerInputVersion()
        {
            // Start with BLANK_FAKERINPUT_VERSION for result
            var result = BLANK_FAKERINPUT_VERSION;
            var deviceInfoSet = NativeMethods.SetupDiGetClassDevs(ref Util.fakerInputGuid, null, 0,
                NativeMethods.DIGCF_DEVICEINTERFACE);
            var deviceInfoData = new NativeMethods.SP_DEVINFO_DATA();
            deviceInfoData.cbSize = Marshal.SizeOf(deviceInfoData);
            var foundDev = false;
            //bool success = NativeMethods.SetupDiEnumDeviceInfo(deviceInfoSet, 0, ref deviceInfoData);
            for (var i = 0; !foundDev && NativeMethods.SetupDiEnumDeviceInfo(deviceInfoSet, i, ref deviceInfoData); i++)
            {
                ulong devPropertyType = 0;
                var requiredSizeProp = 0;
                NativeMethods.SetupDiGetDeviceProperty(deviceInfoSet, ref deviceInfoData,
                    ref NativeMethods.DEVPKEY_Device_DriverVersion, ref devPropertyType, null, 0, ref requiredSizeProp,
                    0);

                if (requiredSizeProp > 0)
                {
                    var versionTextBuffer = new byte[requiredSizeProp];
                    NativeMethods.SetupDiGetDeviceProperty(deviceInfoSet, ref deviceInfoData,
                        ref NativeMethods.DEVPKEY_Device_DriverVersion, ref devPropertyType, versionTextBuffer,
                        requiredSizeProp, ref requiredSizeProp, 0);

                    var tmpitnow = Encoding.Unicode.GetString(versionTextBuffer);
                    var tempStrip = tmpitnow.TrimEnd('\0');
                    foundDev = true;
                    result = tempStrip;
                }
            }

            if (deviceInfoSet.ToInt64() != NativeMethods.INVALID_HANDLE_VALUE)
                NativeMethods.SetupDiDestroyDeviceInfoList(deviceInfoSet);

            return result;
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

        public static bool CreateAutoProfiles(string m_Profile)
        {
            var Saved = true;

            try
            {
                XmlNode Node;
                var doc = new XmlDocument();

                Node = doc.CreateXmlDeclaration("1.0", "utf-8", string.Empty);
                doc.AppendChild(Node);

                Node = doc.CreateComment(string.Format(" Auto-Profile Configuration Data. {0} ", DateTime.Now));
                doc.AppendChild(Node);

                Node = doc.CreateWhitespace("\r\n");
                doc.AppendChild(Node);

                Node = doc.CreateNode(XmlNodeType.Element, "Programs", "");
                doc.AppendChild(Node);
                doc.Save(m_Profile);
            }
            catch
            {
                Saved = false;
            }

            return Saved;
        }

        public static event EventHandler<EventArgs>
            ControllerStatusChange; // called when a controller is added/removed/battery or touchpad mode changes/etc.

        public static void ControllerStatusChanged(object sender)
        {
            if (ControllerStatusChange != null)
                ControllerStatusChange(sender, EventArgs.Empty);
        }

        public static event EventHandler<BatteryReportArgs> BatteryStatusChange;

        public static void OnBatteryStatusChange(object sender, int index, int level, bool charging)
        {
            if (BatteryStatusChange != null)
            {
                var args = new BatteryReportArgs(index, level, charging);
                BatteryStatusChange(sender, args);
            }
        }

        public static event EventHandler<ControllerRemovedArgs> ControllerRemoved;

        public static void OnControllerRemoved(object sender, int index)
        {
            if (ControllerRemoved != null)
            {
                var args = new ControllerRemovedArgs(index);
                ControllerRemoved(sender, args);
            }
        }

        public static event EventHandler<DeviceStatusChangeEventArgs> DeviceStatusChange;

        public static void OnDeviceStatusChanged(object sender, int index)
        {
            if (DeviceStatusChange != null)
            {
                var args = new DeviceStatusChangeEventArgs(index);
                DeviceStatusChange(sender, args);
            }
        }

        public static event EventHandler<SerialChangeArgs> DeviceSerialChange;

        public static void OnDeviceSerialChange(object sender, int index, string serial)
        {
            if (DeviceSerialChange != null)
            {
                var args = new SerialChangeArgs(index, serial);
                DeviceSerialChange(sender, args);
            }
        }

        public static ulong CompileVersionNumberFromString(string versionStr)
        {
            ulong result = 0;
            try
            {
                var tmpVersion = new Version(versionStr);
                result = CompileVersionNumber(tmpVersion.Major, tmpVersion.Minor,
                    tmpVersion.Build, tmpVersion.Revision);
            }
            catch (Exception)
            {
            }

            return result;
        }

        public static ulong CompileVersionNumber(int majorPart, int minorPart,
            int buildPart, int privatePart)
        {
            var result = ((ulong)majorPart << 48) | ((ulong)minorPart << 32) |
                         ((ulong)buildPart << 16) | (ushort)privatePart;
            return result;
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

        public static event EventHandler UDPServerSmoothingMincutoffChanged;
        public static event EventHandler UDPServerSmoothingBetaChanged;

        // controller/profile specfic values

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
            return _config.GyroOutMode[index] == GyroOutMode.Controls;
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
            return _config.GyroOutMode[device];
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
            return _config.GyroMStickInfo[device];
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
            return _config.gyroControlsInf[index];
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

        private static byte ApplyRatio(byte b1, byte b2, double r)
        {
            if (r > 100.0)
                r = 100.0;
            else if (r < 0.0)
                r = 0.0;

            r *= 0.01;
            return (byte)Math.Round(b1 * (1 - r) + b2 * r, 0);
        }

        public static DS4Color GetTransitionedColor(ref DS4Color c1, ref DS4Color c2, double ratio)
        {
            //Color cs = Color.FromArgb(c1.red, c1.green, c1.blue);
            var cs = new DS4Color
            {
                red = ApplyRatio(c1.red, c2.red, ratio),
                green = ApplyRatio(c1.green, c2.green, ratio),
                blue = ApplyRatio(c1.blue, c2.blue, ratio)
            };
            return cs;
        }

        private static Color ApplyRatio(Color c1, Color c2, uint r)
        {
            var ratio = r / 100f;
            var hue1 = c1.GetHue();
            var hue2 = c2.GetHue();
            var bri1 = c1.GetBrightness();
            var bri2 = c2.GetBrightness();
            var sat1 = c1.GetSaturation();
            var sat2 = c2.GetSaturation();
            var hr = hue2 - hue1;
            var br = bri2 - bri1;
            var sr = sat2 - sat1;
            Color csR;
            if (bri1 == 0)
                csR = HueToRGB(hue2, sat2, bri2 - br * ratio);
            else
                csR = HueToRGB(hue2 - hr * ratio, sat2 - sr * ratio, bri2 - br * ratio);

            return csR;
        }

        private static Color HueToRGB(float hue, float sat, float bri)
        {
            var C = (1 - Math.Abs(2 * bri) - 1) * sat;
            var X = C * (1 - Math.Abs(hue / 60 % 2 - 1));
            var m = bri - C / 2;
            float R, G, B;
            if (0 <= hue && hue < 60)
            {
                R = C;
                G = X;
                B = 0;
            }
            else if (60 <= hue && hue < 120)
            {
                R = X;
                G = C;
                B = 0;
            }
            else if (120 <= hue && hue < 180)
            {
                R = 0;
                G = C;
                B = X;
            }
            else if (180 <= hue && hue < 240)
            {
                R = 0;
                G = X;
                B = C;
            }
            else if (240 <= hue && hue < 300)
            {
                R = X;
                G = 0;
                B = C;
            }
            else if (300 <= hue && hue < 360)
            {
                R = C;
                G = 0;
                B = X;
            }
            else
            {
                R = 255;
                G = 0;
                B = 0;
            }

            R += m;
            G += m;
            B += m;
            R *= 255.0f;
            G *= 255.0f;
            B *= 255.0f;
            return Color.FromArgb((int)R, (int)G, (int)B);
        }

        public static double Clamp(double min, double value, double max)
        {
            return value < min ? min : value > max ? max : value;
        }

        private static int ClampInt(int min, int value, int max)
        {
            return value < min ? min : value > max ? max : value;
        }

        public static void InitOutputKBMHandler(string identifier)
        {
            outputKBMHandler = VirtualKBMFactory.DetermineHandler(identifier);
        }

        public static void InitOutputKBMMapping(string identifier)
        {
            outputKBMMapping = VirtualKBMFactory.GetMappingInstance(identifier);
        }

        public static void RefreshFakerInputInfo()
        {
            fakerInputInstalled = IsFakerInputInstalled;
            fakerInputVersion = FakerInputVersion();
        }

        /// <summary>
        ///     Take Windows virtual key value and refresh action alias for currently used output KB+M system
        /// </summary>
        /// <param name="setting">Instance of edited DS4ControlSettings object</param>
        /// <param name="shift">Flag to indicate if shift action is being modified</param>
        public static void RefreshActionAlias(DS4ControlSettings setting, bool shift)
        {
            if (!shift)
            {
                setting.ActionData.ActionAlias = 0;
                if (setting.ControlActionType == DS4ControlSettings.ActionType.Key)
                    setting.ActionData.ActionAlias =
                        outputKBMMapping.GetRealEventKey(Convert.ToUInt32(setting.ActionData.ActionKey));
            }
            else
            {
                setting.ShiftAction.ActionAlias = 0;
                if (setting.ShiftActionType == DS4ControlSettings.ActionType.Key)
                    setting.ShiftAction.ActionAlias =
                        outputKBMMapping.GetRealEventKey(Convert.ToUInt32(setting.ShiftAction.ActionKey));
            }
        }

        public void RefreshExtrasButtons(int deviceNum, List<DS4Controls> devButtons)
        {
            _config.ds4controlSettings[deviceNum].ResetExtraButtons();
            if (devButtons != null) _config.ds4controlSettings[deviceNum].EstablishExtraButtons(devButtons);
        }

        private class ViGEmBusInfo
        {
            public string deviceName;
            public Version deviceVersion;
            public string deviceVersionStr;

            public string driverProviderName;

            //public string path;
            public string instanceId;
            public string manufacturer;
        }
    }
}