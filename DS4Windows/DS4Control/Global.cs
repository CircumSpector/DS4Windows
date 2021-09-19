using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Xml;
using DS4Windows.DS4Control;

namespace DS4Windows
{
    public class Global
    {
        public const int MAX_DS4_CONTROLLER_COUNT = 8;
        public const int TEST_PROFILE_ITEM_COUNT = MAX_DS4_CONTROLLER_COUNT + 1;
        public const int TEST_PROFILE_INDEX = TEST_PROFILE_ITEM_COUNT - 1;
        public const int OLD_XINPUT_CONTROLLER_COUNT = 4;
        public const byte DS4_STICK_AXIS_MIDPOINT = 128;

        /// <summary>
        ///     Loading and Saving decimal values in configuration files should always use en-US decimal format (ie. dot char as
        ///     decimal separator char, not comma char)
        /// </summary>
        public static CultureInfo ConfigFileDecimalCulture => new("en-US");

        protected static BackingStore m_Config = new BackingStore();

        protected static Int32 m_IdleTimeout = 600000;

        public static string ExecutableLocation => Process.GetCurrentProcess().MainModule.FileName;

        public static string ExecutableDirectory => Directory.GetParent(ExecutableLocation).FullName;

        public static string ExecutableFileName => Path.GetFileName(ExecutableLocation);

        public static FileVersionInfo ExecutableFileVersion => FileVersionInfo.GetVersionInfo(ExecutableLocation);

        public static string ExecutableProductVersion => ExecutableFileVersion.ProductVersion;

        public static ulong ExecutableVersionLong => (ulong)ExecutableFileVersion.ProductMajorPart << 48 |
            (ulong)ExecutableFileVersion.ProductMinorPart << 32 | (ulong)ExecutableFileVersion.ProductBuildPart << 16;

        public static bool IsWin8OrGreater
        {
            get
            {
                var result = false;

                switch (Environment.OSVersion.Version.Major)
                {
                    case > 6:
                    case 6 when Environment.OSVersion.Version.Minor >= 2:
                        result = true;
                        break;
                }

                return result;
            }
        }

        public static bool IsWin10OrGreater => Environment.OSVersion.Version.Major >= 10;

        public static string RuntimeAppDataPath { get; set; }

        public static bool IsFirstRun { get; set; } = false;

        public static bool HasMultipleSaveSpots { get; set; } = false;

        public static string RoamingAppDataPath =>
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "DS4Windows");
        
        public static bool RunHotPlug { get; set; } = false;

        public static string[] TempProfileNames = new string[TEST_PROFILE_ITEM_COUNT]
        {
            string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty,
            string.Empty, string.Empty
        };

        public static bool[] UseTempProfiles = new bool[TEST_PROFILE_ITEM_COUNT]
            {false, false, false, false, false, false, false, false, false};

        public static bool[] TempProfileDistance = new bool[TEST_PROFILE_ITEM_COUNT]
            {false, false, false, false, false, false, false, false, false};

        public static bool[] UseDirectInputOnly = new bool[TEST_PROFILE_ITEM_COUNT]
            {true, true, true, true, true, true, true, true, true};

        public static bool[] LinkedProfileCheck = new bool[MAX_DS4_CONTROLLER_COUNT]
            {false, false, false, false, false, false, false, false};

        public static bool[] TouchpadActive = new bool[TEST_PROFILE_ITEM_COUNT]
            {true, true, true, true, true, true, true, true, true};

        /// <summary>
        ///     Used to hold device type desired from Profile Editor
        /// </summary>
        public static OutContType[] OutDevTypeTemp = new OutContType[TEST_PROFILE_ITEM_COUNT]
        {
            DS4Windows.OutContType.X360, DS4Windows.OutContType.X360,
            DS4Windows.OutContType.X360, DS4Windows.OutContType.X360,
            DS4Windows.OutContType.X360, DS4Windows.OutContType.X360,
            DS4Windows.OutContType.X360, DS4Windows.OutContType.X360,
            DS4Windows.OutContType.X360
        };

        /// <summary>
        ///     Used to hold the currently active controller output type in use for a slot.
        /// </summary>
        public static OutContType[] ActiveOutDevType = new OutContType[TEST_PROFILE_ITEM_COUNT]
        {
            DS4Windows.OutContType.None, DS4Windows.OutContType.None,
            DS4Windows.OutContType.None, DS4Windows.OutContType.None,
            DS4Windows.OutContType.None, DS4Windows.OutContType.None,
            DS4Windows.OutContType.None, DS4Windows.OutContType.None,
            DS4Windows.OutContType.None
        };

        private const string BLANK_VIGEMBUS_VERSION = "0.0.0.0";
        private const string MIN_SUPPORTED_VIGEMBUS_VERSION = "1.16.112.0";

        public static bool IsViGEmInstalled { get; private set; } = false;

        public static string ViGEmBusVersion { get; private set; } = BLANK_VIGEMBUS_VERSION;

        public static Version ViGEmBusVersionInfo => new(ViGEmBusVersion);

        private static Version MinimumSupportedViGEmBusVersionInfo => new(MIN_SUPPORTED_VIGEMBUS_VERSION);

        public static bool hidHideInstalled = IsHidHideInstalled;
        public static bool fakerInputInstalled = IsFakerInputInstalled;
        public const string BLANK_FAKERINPUT_VERSION = "0.0.0.0";
        public static string fakerInputVersion = FakerInputVersion();

        public static VirtualKBMBase outputKBMHandler = null;
        public static VirtualKBMMapping outputKBMMapping = null;

        public const int CONFIG_VERSION = 5;
        public const int APP_CONFIG_VERSION = 2;
        public const string ASSEMBLY_RESOURCE_PREFIX = "pack://application:,,,/DS4Windows;";
        public const string RESOURCES_PREFIX = "/DS4Windows;component/Resources";
        public const string CUSTOM_EXE_CONFIG_FILENAME = "custom_exe_name.txt";
        public const string XML_EXTENSION = ".xml";

        public static X360Controls[] DefaultButtonMapping = {
            X360Controls.None, // DS4Controls.None
            X360Controls.LXNeg, // DS4Controls.LXNeg
            X360Controls.LXPos, // DS4Controls.LXPos
            X360Controls.LYNeg, // DS4Controls.LYNeg
            X360Controls.LYPos, // DS4Controls.LYPos
            X360Controls.RXNeg, // DS4Controls.RXNeg
            X360Controls.RXPos, // DS4Controls.RXPos
            X360Controls.RYNeg, // DS4Controls.RYNeg
            X360Controls.RYPos, // DS4Controls.RYPos
            X360Controls.LB, // DS4Controls.L1
            X360Controls.LT, // DS4Controls.L2
            X360Controls.LS, // DS4Controls.L3
            X360Controls.RB, // DS4Controls.R1
            X360Controls.RT, // DS4Controls.R2
            X360Controls.RS, // DS4Controls.R3
            X360Controls.X, // DS4Controls.Square
            X360Controls.Y, // DS4Controls.Triangle
            X360Controls.B, // DS4Controls.Circle
            X360Controls.A, // DS4Controls.Cross
            X360Controls.DpadUp, // DS4Controls.DpadUp
            X360Controls.DpadRight, // DS4Controls.DpadRight
            X360Controls.DpadDown, // DS4Controls.DpadDown
            X360Controls.DpadLeft, // DS4Controls.DpadLeft
            X360Controls.Guide, // DS4Controls.PS
            X360Controls.LeftMouse, // DS4Controls.TouchLeft
            X360Controls.MiddleMouse, // DS4Controls.TouchUpper
            X360Controls.RightMouse, // DS4Controls.TouchMulti
            X360Controls.LeftMouse, // DS4Controls.TouchRight
            X360Controls.Back, // DS4Controls.Share
            X360Controls.Start, // DS4Controls.Options
            X360Controls.None, // DS4Controls.Mute
            X360Controls.None, // DS4Controls.GyroXPos
            X360Controls.None, // DS4Controls.GyroXNeg
            X360Controls.None, // DS4Controls.GyroZPos
            X360Controls.None, // DS4Controls.GyroZNeg
            X360Controls.None, // DS4Controls.SwipeLeft
            X360Controls.None, // DS4Controls.SwipeRight
            X360Controls.None, // DS4Controls.SwipeUp
            X360Controls.None, // DS4Controls.SwipeDown
            X360Controls.None, // DS4Controls.L2FullPull
            X360Controls.None, // DS4Controls.R2FullPull
            X360Controls.None, // DS4Controls.GyroSwipeLeft
            X360Controls.None, // DS4Controls.GyroSwipeRight
            X360Controls.None, // DS4Controls.GyroSwipeUp
            X360Controls.None, // DS4Controls.GyroSwipeDown
            X360Controls.None, // DS4Controls.Capture
            X360Controls.None, // DS4Controls.SideL
            X360Controls.None, // DS4Controls.SideR
            X360Controls.None, // DS4Controls.LSOuter
            X360Controls.None, // DS4Controls.RSOuter
        };

        // Create mapping array at runtime
        public static DS4Controls[] ReverseX360ButtonMapping = new Func<DS4Controls[]>(() =>
        {
            var temp = new DS4Controls[DefaultButtonMapping.Length];
            for (int i = 0, arlen = DefaultButtonMapping.Length; i < arlen; i++)
            {
                var mapping = DefaultButtonMapping[i];
                if (mapping != X360Controls.None) temp[(int) mapping] = (DS4Controls) i;
            }

            return temp;
        })();

        public static Dictionary<X360Controls, string> XboxDefaultNames =>
            new()
            {
                [X360Controls.LXNeg] = "Left X-Axis-",
                [X360Controls.LXPos] = "Left X-Axis+",
                [X360Controls.LYNeg] = "Left Y-Axis-",
                [X360Controls.LYPos] = "Left Y-Axis+",
                [X360Controls.RXNeg] = "Right X-Axis-",
                [X360Controls.RXPos] = "Right X-Axis+",
                [X360Controls.RYNeg] = "Right Y-Axis-",
                [X360Controls.RYPos] = "Right Y-Axis+",
                [X360Controls.LB] = "Left Bumper",
                [X360Controls.LT] = "Left Trigger",
                [X360Controls.LS] = "Left Stick",
                [X360Controls.RB] = "Right Bumper",
                [X360Controls.RT] = "Right Trigger",
                [X360Controls.RS] = "Right Stick",
                [X360Controls.X] = "X Button",
                [X360Controls.Y] = "Y Button",
                [X360Controls.B] = "B Button",
                [X360Controls.A] = "A Button",
                [X360Controls.DpadUp] = "Up Button",
                [X360Controls.DpadRight] = "Right Button",
                [X360Controls.DpadDown] = "Down Button",
                [X360Controls.DpadLeft] = "Left Button",
                [X360Controls.Guide] = "Guide",
                [X360Controls.Back] = "Back",
                [X360Controls.Start] = "Start",
                [X360Controls.TouchpadClick] = "Touchpad Click",
                [X360Controls.LeftMouse] = "Left Mouse Button",
                [X360Controls.RightMouse] = "Right Mouse Button",
                [X360Controls.MiddleMouse] = "Middle Mouse Button",
                [X360Controls.FourthMouse] = "4th Mouse Button",
                [X360Controls.FifthMouse] = "5th Mouse Button",
                [X360Controls.WUP] = "Mouse Wheel Up",
                [X360Controls.WDOWN] = "Mouse Wheel Down",
                [X360Controls.MouseUp] = "Mouse Up",
                [X360Controls.MouseDown] = "Mouse Down",
                [X360Controls.MouseLeft] = "Mouse Left",
                [X360Controls.MouseRight] = "Mouse Right",
                [X360Controls.Unbound] = "Unbound",
                [X360Controls.None] = "Unassigned"
            };

        public static Dictionary<X360Controls, string> Ds4DefaultNames => new()
        {
            [X360Controls.LXNeg] = "Left X-Axis-",
            [X360Controls.LXPos] = "Left X-Axis+",
            [X360Controls.LYNeg] = "Left Y-Axis-",
            [X360Controls.LYPos] = "Left Y-Axis+",
            [X360Controls.RXNeg] = "Right X-Axis-",
            [X360Controls.RXPos] = "Right X-Axis+",
            [X360Controls.RYNeg] = "Right Y-Axis-",
            [X360Controls.RYPos] = "Right Y-Axis+",
            [X360Controls.LB] = "L1",
            [X360Controls.LT] = "L2",
            [X360Controls.LS] = "L3",
            [X360Controls.RB] = "R1",
            [X360Controls.RT] = "R2",
            [X360Controls.RS] = "R3",
            [X360Controls.X] = "Square",
            [X360Controls.Y] = "Triangle",
            [X360Controls.B] = "Circle",
            [X360Controls.A] = "Cross",
            [X360Controls.DpadUp] = "Dpad Up",
            [X360Controls.DpadRight] = "Dpad Right",
            [X360Controls.DpadDown] = "Dpad Down",
            [X360Controls.DpadLeft] = "Dpad Left",
            [X360Controls.Guide] = "PS",
            [X360Controls.Back] = "Share",
            [X360Controls.Start] = "Options",
            [X360Controls.TouchpadClick] = "Touchpad Click",
            [X360Controls.LeftMouse] = "Left Mouse Button",
            [X360Controls.RightMouse] = "Right Mouse Button",
            [X360Controls.MiddleMouse] = "Middle Mouse Button",
            [X360Controls.FourthMouse] = "4th Mouse Button",
            [X360Controls.FifthMouse] = "5th Mouse Button",
            [X360Controls.WUP] = "Mouse Wheel Up",
            [X360Controls.WDOWN] = "Mouse Wheel Down",
            [X360Controls.MouseUp] = "Mouse Up",
            [X360Controls.MouseDown] = "Mouse Down",
            [X360Controls.MouseLeft] = "Mouse Left",
            [X360Controls.MouseRight] = "Mouse Right",
            [X360Controls.Unbound] = "Unbound"
        };

        public static string GetX360ControlString(X360Controls key, OutContType conType)
        {
            var result = string.Empty;

            switch (conType)
            {
                case DS4Windows.OutContType.X360:
                    XboxDefaultNames.TryGetValue(key, out result);
                    break;
                case DS4Windows.OutContType.DS4:
                    Ds4DefaultNames.TryGetValue(key, out result);
                    break;
            }

            return result;
        }

        public static Dictionary<DS4Controls, string> Ds4InputNames => new()
        {
            [DS4Controls.LXNeg] = "Left X-Axis-",
            [DS4Controls.LXPos] = "Left X-Axis+",
            [DS4Controls.LYNeg] = "Left Y-Axis-",
            [DS4Controls.LYPos] = "Left Y-Axis+",
            [DS4Controls.RXNeg] = "Right X-Axis-",
            [DS4Controls.RXPos] = "Right X-Axis+",
            [DS4Controls.RYNeg] = "Right Y-Axis-",
            [DS4Controls.RYPos] = "Right Y-Axis+",
            [DS4Controls.L1] = "L1",
            [DS4Controls.L2] = "L2",
            [DS4Controls.L3] = "L3",
            [DS4Controls.R1] = "R1",
            [DS4Controls.R2] = "R2",
            [DS4Controls.R3] = "R3",
            [DS4Controls.Square] = "Square",
            [DS4Controls.Triangle] = "Triangle",
            [DS4Controls.Circle] = "Circle",
            [DS4Controls.Cross] = "Cross",
            [DS4Controls.DpadUp] = "Dpad Up",
            [DS4Controls.DpadRight] = "Dpad Right",
            [DS4Controls.DpadDown] = "Dpad Down",
            [DS4Controls.DpadLeft] = "Dpad Left",
            [DS4Controls.PS] = "PS",
            [DS4Controls.Share] = "Share",
            [DS4Controls.Options] = "Options",
            [DS4Controls.Mute] = "Mute",
            [DS4Controls.Capture] = "Capture",
            [DS4Controls.SideL] = "Side L",
            [DS4Controls.SideR] = "Side R",
            [DS4Controls.TouchLeft] = "Left Touch",
            [DS4Controls.TouchUpper] = "Upper Touch",
            [DS4Controls.TouchMulti] = "Multitouch",
            [DS4Controls.TouchRight] = "Right Touch",
            [DS4Controls.GyroXPos] = "Gyro X+",
            [DS4Controls.GyroXNeg] = "Gyro X-",
            [DS4Controls.GyroZPos] = "Gyro Z+",
            [DS4Controls.GyroZNeg] = "Gyro Z-",
            [DS4Controls.SwipeLeft] = "Swipe Left",
            [DS4Controls.SwipeRight] = "Swipe Right",
            [DS4Controls.SwipeUp] = "Swipe Up",
            [DS4Controls.SwipeDown] = "Swipe Down",
            [DS4Controls.L2FullPull] = "L2 Full Pull",
            [DS4Controls.R2FullPull] = "R2 Full Pull",
            [DS4Controls.LSOuter] = "LS Outer",
            [DS4Controls.RSOuter] = "RS Outer",

            [DS4Controls.GyroSwipeLeft] = "Gyro Swipe Left",
            [DS4Controls.GyroSwipeRight] = "Gyro Swipe Right",
            [DS4Controls.GyroSwipeUp] = "Gyro Swipe Up",
            [DS4Controls.GyroSwipeDown] = "Gyro Swipe Down"
        };

        public static Dictionary<DS4Controls, int> MacroDs4Values => new()
        {
            [DS4Controls.Cross] = 261, [DS4Controls.Circle] = 262,
            [DS4Controls.Square] = 263, [DS4Controls.Triangle] = 264,
            [DS4Controls.Options] = 265, [DS4Controls.Share] = 266,
            [DS4Controls.DpadUp] = 267, [DS4Controls.DpadDown] = 268,
            [DS4Controls.DpadLeft] = 269, [DS4Controls.DpadRight] = 270,
            [DS4Controls.PS] = 271, [DS4Controls.L1] = 272,
            [DS4Controls.R1] = 273, [DS4Controls.L2] = 274,
            [DS4Controls.R2] = 275, [DS4Controls.L3] = 276,
            [DS4Controls.R3] = 277, [DS4Controls.LXPos] = 278,
            [DS4Controls.LXNeg] = 279, [DS4Controls.LYPos] = 280,
            [DS4Controls.LYNeg] = 281, [DS4Controls.RXPos] = 282,
            [DS4Controls.RXNeg] = 283, [DS4Controls.RYPos] = 284,
            [DS4Controls.RYNeg] = 285,
            [DS4Controls.TouchLeft] = 286, [DS4Controls.TouchRight] = 286,
            [DS4Controls.TouchUpper] = 286, [DS4Controls.TouchMulti] = 286
        };

        public static Dictionary<TrayIconChoice, string> IconChoiceResources = new()
        {
            [TrayIconChoice.Default] = "/DS4Windows;component/Resources/DS4W.ico",
            [TrayIconChoice.Colored] = "/DS4Windows;component/Resources/DS4W.ico",
            [TrayIconChoice.White] = "/DS4Windows;component/Resources/DS4W - White.ico",
            [TrayIconChoice.Black] = "/DS4Windows;component/Resources/DS4W - Black.ico"
        };

        public static void SaveWhere(string path)
        {
            RuntimeAppDataPath = path;
            m_Config.ProfilesPath = Path.Combine(RuntimeAppDataPath, Constants.ProfilesFileName);
            m_Config.ActionsPath = Path.Combine(RuntimeAppDataPath, Constants.ActionsFileName);
            m_Config.LinkedProfilesPath = Path.Combine(RuntimeAppDataPath, Constants.LinkedProfilesFileName);
            m_Config.ControllerConfigsPath = Path.Combine(RuntimeAppDataPath, Constants.ControllerConfigsFileName);
        }

        public static bool SaveDefault(string path)
        {
            Boolean Saved = true;
            XmlDocument m_Xdoc = new XmlDocument();
            try
            {
                XmlNode Node;

                m_Xdoc.RemoveAll();

                Node = m_Xdoc.CreateXmlDeclaration("1.0", "utf-8", String.Empty);
                m_Xdoc.AppendChild(Node);

                Node = m_Xdoc.CreateComment(string.Format(" Profile Configuration Data. {0} ", DateTime.Now));
                m_Xdoc.AppendChild(Node);

                Node = m_Xdoc.CreateWhitespace("\r\n");
                m_Xdoc.AppendChild(Node);

                Node = m_Xdoc.CreateNode(XmlNodeType.Element, "Profile", null);

                m_Xdoc.AppendChild(Node);

                m_Xdoc.Save(path);
            }
            catch { Saved = false; }

            return Saved;
        }

        /// <summary>
        /// Check if Admin Rights are needed to write in Application Directory
        /// </summary>
        /// <value></value>
        public static bool IsAdminNeeded
        {
            get
            {
                try
                {
                    using (var fs = File.Create(
                        Path.Combine(
                            ExecutableDirectory,
                            Path.GetRandomFileName()
                        ),
                        1,
                        FileOptions.DeleteOnClose)
                    )
                    {
                    }

                    return false;
                }
                catch
                {
                    return true;
                }
            }
        }

        public static bool IsAdministrator
        {
            get
            {
                var identity = WindowsIdentity.GetCurrent();
                var principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
        }

        public static bool CheckForDevice(string guid)
        {
            bool result = false;
            Guid deviceGuid = Guid.Parse(guid);
            NativeMethods.SP_DEVINFO_DATA deviceInfoData =
                new NativeMethods.SP_DEVINFO_DATA();
            deviceInfoData.cbSize =
                System.Runtime.InteropServices.Marshal.SizeOf(deviceInfoData);

            IntPtr deviceInfoSet = NativeMethods.SetupDiGetClassDevs(ref deviceGuid, null, 0,
                NativeMethods.DIGCF_DEVICEINTERFACE);
            result = NativeMethods.SetupDiEnumDeviceInfo(deviceInfoSet, 0, ref deviceInfoData);

            if (deviceInfoSet.ToInt64() != NativeMethods.INVALID_HANDLE_VALUE)
            {
                NativeMethods.SetupDiDestroyDeviceInfoList(deviceInfoSet);
            }

            return result;
        }

        private class ViGEmBusInfo
        {
            //public string path;
            public string instanceId;
            public string deviceName;
            public string deviceVersionStr;
            public Version deviceVersion;
            public string manufacturer;
            public string driverProviderName;
        }

        private static void FindViGEmDeviceInfo()
        {
            bool result = false;
            Guid deviceGuid = Constants.ViGemBusInterfaceGuid;
            NativeMethods.SP_DEVINFO_DATA deviceInfoData =
                new NativeMethods.SP_DEVINFO_DATA();
            deviceInfoData.cbSize =
                System.Runtime.InteropServices.Marshal.SizeOf(deviceInfoData);

            var dataBuffer = new byte[4096];
            ulong propertyType = 0;
            var requiredSize = 0;

            // Properties to retrieve
            NativeMethods.DEVPROPKEY[] lookupProperties = new NativeMethods.DEVPROPKEY[]
            {
                NativeMethods.DEVPKEY_Device_DriverVersion, NativeMethods.DEVPKEY_Device_InstanceId,
                NativeMethods.DEVPKEY_Device_Manufacturer, NativeMethods.DEVPKEY_Device_Provider,
                NativeMethods.DEVPKEY_Device_DeviceDesc,
            };

            List<ViGEmBusInfo> tempViGEmBusInfoList = new List<ViGEmBusInfo>();

            IntPtr deviceInfoSet = NativeMethods.SetupDiGetClassDevs(ref deviceGuid, null, 0,
                NativeMethods.DIGCF_DEVICEINTERFACE);
            for (int i = 0; !result && NativeMethods.SetupDiEnumDeviceInfo(deviceInfoSet, i, ref deviceInfoData); i++)
            {
                ViGEmBusInfo tempBusInfo = new ViGEmBusInfo();

                foreach (NativeMethods.DEVPROPKEY currentDevKey in lookupProperties)
                {
                    NativeMethods.DEVPROPKEY tempKey = currentDevKey;
                    if (NativeMethods.SetupDiGetDeviceProperty(deviceInfoSet, ref deviceInfoData,
                        ref tempKey, ref propertyType,
                        dataBuffer, dataBuffer.Length, ref requiredSize, 0))
                    {
                        string temp = dataBuffer.ToUTF16String();
                        if (currentDevKey.fmtid == NativeMethods.DEVPKEY_Device_DriverVersion.fmtid &&
                            currentDevKey.pid == NativeMethods.DEVPKEY_Device_DriverVersion.pid)
                        {
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
                        }
                        else if (currentDevKey.fmtid == NativeMethods.DEVPKEY_Device_InstanceId.fmtid &&
                            currentDevKey.pid == NativeMethods.DEVPKEY_Device_InstanceId.pid)
                        {
                            tempBusInfo.instanceId = temp;
                        }
                        else if (currentDevKey.fmtid == NativeMethods.DEVPKEY_Device_Manufacturer.fmtid &&
                            currentDevKey.pid == NativeMethods.DEVPKEY_Device_Manufacturer.pid)
                        {
                            tempBusInfo.manufacturer = temp;
                        }
                        else if (currentDevKey.fmtid == NativeMethods.DEVPKEY_Device_Provider.fmtid &&
                            currentDevKey.pid == NativeMethods.DEVPKEY_Device_Provider.pid)
                        {
                            tempBusInfo.driverProviderName = temp;
                        }
                        else if (currentDevKey.fmtid == NativeMethods.DEVPKEY_Device_DeviceDesc.fmtid &&
                            currentDevKey.pid == NativeMethods.DEVPKEY_Device_DeviceDesc.pid)
                        {
                            tempBusInfo.deviceName = temp;
                        }
                    }
                }

                tempViGEmBusInfoList.Add(tempBusInfo);
            }

            if (deviceInfoSet.ToInt64() != NativeMethods.INVALID_HANDLE_VALUE)
            {
                NativeMethods.SetupDiDestroyDeviceInfoList(deviceInfoSet);
            }

            // Iterate over list and find most recent version number
            //IEnumerable<ViGEmBusInfo> tempResults = tempViGEmBusInfoList.Where(item => MinimumSupportedViGEmBusVersionInfo.CompareTo(item.deviceVersion) <= 0);
            Version latestKnown = new Version(BLANK_VIGEMBUS_VERSION);
            string deviceInstanceId = string.Empty;
            foreach (ViGEmBusInfo item in tempViGEmBusInfoList)
            {
                if (latestKnown.CompareTo(item.deviceVersion) <= 0)
                {
                    latestKnown = item.deviceVersion;
                    deviceInstanceId = item.instanceId;
                }
            }

            // Get bus info for most recent version found and save info
            ViGEmBusInfo latestBusInfo =
                tempViGEmBusInfoList.SingleOrDefault(item => item.instanceId == deviceInstanceId);
            PopulateFromViGEmBusInfo(latestBusInfo);
        }

        private static bool CheckForSysDevice(string searchHardwareId)
        {
            bool result = false;
            Guid sysGuid = Constants.SystemDeviceClassGuid;
            NativeMethods.SP_DEVINFO_DATA deviceInfoData =
                new NativeMethods.SP_DEVINFO_DATA();
            deviceInfoData.cbSize =
                System.Runtime.InteropServices.Marshal.SizeOf(deviceInfoData);
            var dataBuffer = new byte[4096];
            ulong propertyType = 0;
            var requiredSize = 0;
            IntPtr deviceInfoSet = NativeMethods.SetupDiGetClassDevs(ref sysGuid, null, 0, 0);
            for (int i = 0; !result && NativeMethods.SetupDiEnumDeviceInfo(deviceInfoSet, i, ref deviceInfoData); i++)
            {
                if (NativeMethods.SetupDiGetDeviceProperty(deviceInfoSet, ref deviceInfoData,
                    ref NativeMethods.DEVPKEY_Device_HardwareIds, ref propertyType,
                    dataBuffer, dataBuffer.Length, ref requiredSize, 0))
                {
                    string hardwareId = dataBuffer.ToUTF16String();
                    //if (hardwareIds.Contains("Virtual Gamepad Emulation Bus"))
                    //    result = true;
                    if (hardwareId.Equals(searchHardwareId))
                        result = true;
                }
            }

            if (deviceInfoSet.ToInt64() != NativeMethods.INVALID_HANDLE_VALUE)
            {
                NativeMethods.SetupDiDestroyDeviceInfoList(deviceInfoSet);
            }

            return result;
        }

        internal static string GetDeviceProperty(string deviceInstanceId,
            NativeMethods.DEVPROPKEY prop)
        {
            string result = string.Empty;
            NativeMethods.SP_DEVINFO_DATA deviceInfoData = new NativeMethods.SP_DEVINFO_DATA();
            deviceInfoData.cbSize = System.Runtime.InteropServices.Marshal.SizeOf(deviceInfoData);
            var dataBuffer = new byte[4096];
            ulong propertyType = 0;
            var requiredSize = 0;

            Guid hidGuid = new Guid();
            NativeMethods.HidD_GetHidGuid(ref hidGuid);
            IntPtr deviceInfoSet = NativeMethods.SetupDiGetClassDevs(ref hidGuid, deviceInstanceId, 0, NativeMethods.DIGCF_PRESENT | NativeMethods.DIGCF_DEVICEINTERFACE);
            NativeMethods.SetupDiEnumDeviceInfo(deviceInfoSet, 0, ref deviceInfoData);
            if (NativeMethods.SetupDiGetDeviceProperty(deviceInfoSet, ref deviceInfoData, ref prop, ref propertyType,
                    dataBuffer, dataBuffer.Length, ref requiredSize, 0))
            {
                result = dataBuffer.ToUTF16String();
            }

            if (deviceInfoSet.ToInt64() != NativeMethods.INVALID_HANDLE_VALUE)
            {
                NativeMethods.SetupDiDestroyDeviceInfoList(deviceInfoSet);
            }

            return result;
        }

        public static bool CheckHidHideAffectedStatus(string deviceInstanceId,
            HashSet<string> affectedDevs, HashSet<string> exemptedDevices, bool force = false)
        {
            bool result = false;
            string tempDeviceInstanceId = deviceInstanceId.ToUpper();
            result = affectedDevs.Contains(tempDeviceInstanceId);
            return result;
        }

        public static bool IsHidHideInstalled => CheckForSysDevice(@"root\HidHide");

        public static bool IsFakerInputInstalled => CheckForSysDevice(@"root\FakerInput");

        
        public static bool IsViGEmBusInstalled()
        {
            return IsViGEmInstalled;
        }

        public static bool IsRunningSupportedViGEmBus => IsViGEmInstalled &&
                                                         MinimumSupportedViGEmBusVersionInfo.CompareTo(
                                                             ViGEmBusVersionInfo) <= 0;

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
            string result = BLANK_FAKERINPUT_VERSION;
            IntPtr deviceInfoSet = NativeMethods.SetupDiGetClassDevs(ref Util.fakerInputGuid, null, 0, NativeMethods.DIGCF_DEVICEINTERFACE);
            NativeMethods.SP_DEVINFO_DATA deviceInfoData = new NativeMethods.SP_DEVINFO_DATA();
            deviceInfoData.cbSize = System.Runtime.InteropServices.Marshal.SizeOf(deviceInfoData);
            bool foundDev = false;
            //bool success = NativeMethods.SetupDiEnumDeviceInfo(deviceInfoSet, 0, ref deviceInfoData);
            for (int i = 0; !foundDev && NativeMethods.SetupDiEnumDeviceInfo(deviceInfoSet, i, ref deviceInfoData); i++)
            {
                ulong devPropertyType = 0;
                int requiredSizeProp = 0;
                NativeMethods.SetupDiGetDeviceProperty(deviceInfoSet, ref deviceInfoData,
                    ref NativeMethods.DEVPKEY_Device_DriverVersion, ref devPropertyType, null, 0, ref requiredSizeProp, 0);

                if (requiredSizeProp > 0)
                {
                    var versionTextBuffer = new byte[requiredSizeProp];
                    NativeMethods.SetupDiGetDeviceProperty(deviceInfoSet, ref deviceInfoData,
                        ref NativeMethods.DEVPKEY_Device_DriverVersion, ref devPropertyType, versionTextBuffer, requiredSizeProp, ref requiredSizeProp, 0);

                    string tmpitnow = System.Text.Encoding.Unicode.GetString(versionTextBuffer);
                    string tempStrip = tmpitnow.TrimEnd('\0');
                    foundDev = true;
                    result = tempStrip;
                }
            }

            if (deviceInfoSet.ToInt64() != NativeMethods.INVALID_HANDLE_VALUE)
            {
                NativeMethods.SetupDiDestroyDeviceInfoList(deviceInfoSet);
            }

            return result;
        }

        public static void FindConfigLocation()
        {
            bool programFolderAutoProfilesExists = File.Exists(Path.Combine(ExecutableDirectory, Constants.AutoProfilesFileName));
            bool appDataAutoProfilesExists = File.Exists(Path.Combine(RoamingAppDataPath, Constants.AutoProfilesFileName));
            //bool localAppDataAutoProfilesExists = File.Exists(Path.Combine(localAppDataPpath, "Auto Profiles.xml"));
            //bool systemAppConfigExists = appDataAutoProfilesExists || localAppDataAutoProfilesExists;
            bool systemAppConfigExists = appDataAutoProfilesExists;
            bool isSameFolder = appDataAutoProfilesExists && ExecutableDirectory == RoamingAppDataPath;

            if (programFolderAutoProfilesExists && appDataAutoProfilesExists &&
                !isSameFolder)
            {
                Global.IsFirstRun = true;
                Global.HasMultipleSaveSpots = true;
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
                Global.IsFirstRun = true;
                Global.HasMultipleSaveSpots = false;
            }
        }

        public static void CreateStdActions()
        {
            XmlDocument xDoc = new XmlDocument();
            try
            {
                string[] profiles = Directory.GetFiles(RuntimeAppDataPath + @"\Profiles\");
                string s = string.Empty;
                //foreach (string s in profiles)
                for (int i = 0, proflen = profiles.Length; i < proflen; i++)
                {
                    s = profiles[i];
                    if (Path.GetExtension(s) == ".xml")
                    {
                        xDoc.Load(s);
                        XmlNode el = xDoc.SelectSingleNode("DS4Windows/ProfileActions");
                        if (el != null)
                        {
                            if (string.IsNullOrEmpty(el.InnerText))
                                el.InnerText = "Disconnect Controller";
                            else
                                el.InnerText += "/Disconnect Controller";
                        }
                        else
                        {
                            XmlNode Node = xDoc.SelectSingleNode("DS4Windows");
                            el = xDoc.CreateElement("ProfileActions");
                            el.InnerText = "Disconnect Controller";
                            Node.AppendChild(el);
                        }

                        xDoc.Save(s);
                        LoadActions();
                    }
                }
            }
            catch { }
        }

        public static bool CreateAutoProfiles(string m_Profile)
        {
            bool Saved = true;

            try
            {
                XmlNode Node;
                XmlDocument doc = new XmlDocument();

                Node = doc.CreateXmlDeclaration("1.0", "utf-8", String.Empty);
                doc.AppendChild(Node);

                Node = doc.CreateComment(string.Format(" Auto-Profile Configuration Data. {0} ", DateTime.Now));
                doc.AppendChild(Node);

                Node = doc.CreateWhitespace("\r\n");
                doc.AppendChild(Node);

                Node = doc.CreateNode(XmlNodeType.Element, "Programs", "");
                doc.AppendChild(Node);
                doc.Save(m_Profile);
            }
            catch { Saved = false; }

            return Saved;
        }

        public static event EventHandler<EventArgs> ControllerStatusChange; // called when a controller is added/removed/battery or touchpad mode changes/etc.
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
                BatteryReportArgs args = new BatteryReportArgs(index, level, charging);
                BatteryStatusChange(sender, args);
            }
        }

        public static event EventHandler<ControllerRemovedArgs> ControllerRemoved;
        public static void OnControllerRemoved(object sender, int index)
        {
            if (ControllerRemoved != null)
            {
                ControllerRemovedArgs args = new ControllerRemovedArgs(index);
                ControllerRemoved(sender, args);
            }
        }

        public static event EventHandler<DeviceStatusChangeEventArgs> DeviceStatusChange;
        public static void OnDeviceStatusChanged(object sender, int index)
        {
            if (DeviceStatusChange != null)
            {
                DeviceStatusChangeEventArgs args = new DeviceStatusChangeEventArgs(index);
                DeviceStatusChange(sender, args);
            }
        }

        public static event EventHandler<SerialChangeArgs> DeviceSerialChange;
        public static void OnDeviceSerialChange(object sender, int index, string serial)
        {
            if (DeviceSerialChange != null)
            {
                SerialChangeArgs args = new SerialChangeArgs(index, serial);
                DeviceSerialChange(sender, args);
            }
        }

        public static ulong CompileVersionNumberFromString(string versionStr)
        {
            ulong result = 0;
            try
            {
                Version tmpVersion = new Version(versionStr);
                result = CompileVersionNumber(tmpVersion.Major, tmpVersion.Minor,
                    tmpVersion.Build, tmpVersion.Revision);
            }
            catch (Exception) { }

            return result;
        }

        public static ulong CompileVersionNumber(int majorPart, int minorPart,
            int buildPart, int privatePart)
        {
            ulong result = (ulong)majorPart << 48 | (ulong)minorPart << 32 |
                (ulong)buildPart << 16 | (ushort)privatePart;
            return result;
        }

        // general values
        // -- Re-Enable Exclusive Mode Starts Here --
        public static bool UseExclusiveMode
        {
            set { m_Config.useExclusiveMode = value; }
            get { return m_Config.useExclusiveMode; }
        } // -- Re-Enable Ex Mode Ends here

        public static bool getUseExclusiveMode()
        {
            return m_Config.useExclusiveMode;
        }
        public static DateTime LastChecked
        {
            set { m_Config.lastChecked = value; }
            get { return m_Config.lastChecked; }
        }

        public static int CheckWhen
        {
            set { m_Config.CheckWhen = value; }
            get { return m_Config.CheckWhen; }
        }

        public static string LastVersionChecked
        {
            get { return m_Config.lastVersionChecked; }
            set
            {
                m_Config.lastVersionChecked = value;
                m_Config.lastVersionCheckedNum = CompileVersionNumberFromString(value);
            }
        }

        public static ulong LastVersionCheckedNum
        {
            get { return m_Config.lastVersionCheckedNum; }
        }

        public static int Notifications
        {
            set { m_Config.notifications = value; }
            get { return m_Config.notifications; }
        }

        public static bool DCBTatStop
        {
            set { m_Config.disconnectBTAtStop = value; }
            get { return m_Config.disconnectBTAtStop; }
        }

        public static bool SwipeProfiles
        {
            set { m_Config.swipeProfiles = value; }
            get { return m_Config.swipeProfiles; }
        }

        public static bool DS4Mapping
        {
            set { m_Config.ds4Mapping = value; }
            get { return m_Config.ds4Mapping; }
        }

        public static bool QuickCharge
        {
            set { m_Config.quickCharge = value; }
            get { return m_Config.quickCharge; }
        }

        public static bool getQuickCharge()
        {
            return m_Config.quickCharge;
        }

        public static bool CloseMini
        {
            set { m_Config.closeMini = value; }
            get { return m_Config.closeMini; }
        }

        public static bool StartMinimized
        {
            set { m_Config.startMinimized = value; }
            get { return m_Config.startMinimized; }
        }

        public static bool MinToTaskbar
        {
            set { m_Config.minToTaskbar = value; }
            get { return m_Config.minToTaskbar; }
        }

        public static bool GetMinToTaskbar()
        {
            return m_Config.minToTaskbar;
        }

        public static int FormWidth
        {
            set { m_Config.formWidth = value; }
            get { return m_Config.formWidth; }
        }

        public static int FormHeight
        {
            set { m_Config.formHeight = value; }
            get { return m_Config.formHeight; }
        }

        public static int FormLocationX
        {
            set { m_Config.formLocationX = value; }
            get { return m_Config.formLocationX; }
        }

        public static int FormLocationY
        {
            set { m_Config.formLocationY = value; }
            get { return m_Config.formLocationY; }
        }

        public static string UseLang
        {
            set { m_Config.useLang = value; }
            get { return m_Config.useLang; }
        }

        public static bool DownloadLang
        {
            set { m_Config.downloadLang = value; }
            get { return m_Config.downloadLang; }
        }

        public static bool FlashWhenLate
        {
            set { m_Config.flashWhenLate = value; }
            get { return m_Config.flashWhenLate; }
        }

        public static bool getFlashWhenLate()
        {
            return m_Config.flashWhenLate;
        }

        public static int FlashWhenLateAt
        {
            set { m_Config.flashWhenLateAt = value; }
            get { return m_Config.flashWhenLateAt; }
        }

        public static int getFlashWhenLateAt()
        {
            return m_Config.flashWhenLateAt;
        }

        public static bool isUsingUDPServer()
        {
            return m_Config.useUDPServ;
        }
        public static void setUsingUDPServer(bool state)
        {
            m_Config.useUDPServ = state;
        }

        public static int getUDPServerPortNum()
        {
            return m_Config.udpServPort;
        }
        public static void setUDPServerPort(int value)
        {
            m_Config.udpServPort = value;
        }

        public static string getUDPServerListenAddress()
        {
            return m_Config.udpServListenAddress;
        }
        public static void setUDPServerListenAddress(string value)
        {
            m_Config.udpServListenAddress = value.Trim();
        }

        public static bool UseUDPSeverSmoothing
        {
            get => m_Config.useUdpSmoothing;
            set => m_Config.useUdpSmoothing = value;
        }

        public static bool IsUsingUDPServerSmoothing()
        {
            return m_Config.useUdpSmoothing;
        }

        public static double UDPServerSmoothingMincutoff
        {
            get => m_Config.udpSmoothingMincutoff;
            set
            {
                double temp = m_Config.udpSmoothingMincutoff;
                if (temp == value) return;
                m_Config.udpSmoothingMincutoff = value;
                UDPServerSmoothingMincutoffChanged?.Invoke(null, EventArgs.Empty);
            }
        }
        public static event EventHandler UDPServerSmoothingMincutoffChanged;

        public static double UDPServerSmoothingBeta
        {
            get => m_Config.udpSmoothingBeta;
            set
            {
                double temp = m_Config.udpSmoothingBeta;
                if (temp == value) return;
                m_Config.udpSmoothingBeta = value;
                UDPServerSmoothingBetaChanged?.Invoke(null, EventArgs.Empty);
            }
        }
        public static event EventHandler UDPServerSmoothingBetaChanged;

        public static TrayIconChoice UseIconChoice
        {
            get => m_Config.useIconChoice;
            set => m_Config.useIconChoice = value;
        }

        public static AppThemeChoice UseCurrentTheme
        {
            get => m_Config.useCurrentTheme;
            set => m_Config.useCurrentTheme = value;
        }

        public static bool UseCustomSteamFolder
        {
            set { m_Config.useCustomSteamFolder = value; }
            get { return m_Config.useCustomSteamFolder; }
        }

        public static string CustomSteamFolder
        {
            set { m_Config.customSteamFolder = value; }
            get { return m_Config.customSteamFolder; }
        }

        public static bool AutoProfileRevertDefaultProfile
        {
            set { m_Config.autoProfileRevertDefaultProfile = value; }
            get { return m_Config.autoProfileRevertDefaultProfile; }
        }

        /// <summary>
        /// Fake name used for user copy of DS4Windows.exe
        /// </summary>
        public static string FakeExeName
        {
            get => m_Config.fakeExeFileName;
            set
            {
                bool valid = !(value.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0);
                if (valid)
                {
                    m_Config.fakeExeFileName = value;
                }
            }
        }

        // controller/profile specfic values
        public static ButtonMouseInfo[] ButtonMouseInfos => m_Config.buttonMouseInfos;

        public static byte[] RumbleBoost => m_Config.rumble;
        public static byte getRumbleBoost(int index)
        {
            return m_Config.rumble[index];
        }

        public static void setRumbleAutostopTime(int index, int value)
        {
            m_Config.rumbleAutostopTime[index] = value;
            
            DS4Device tempDev = Program.rootHub.DS4Controllers[index];
            if (tempDev != null && tempDev.isSynced())
                tempDev.RumbleAutostopTime = value;
        }

        public static int getRumbleAutostopTime(int index)
        {
            return m_Config.rumbleAutostopTime[index];
        }

        public static bool[] EnableTouchToggle => m_Config.enableTouchToggle;
        public static bool getEnableTouchToggle(int index)
        {
            return m_Config.enableTouchToggle[index];
        }

        public static int[] IdleDisconnectTimeout => m_Config.idleDisconnectTimeout;
        public static int getIdleDisconnectTimeout(int index)
        {
            return m_Config.idleDisconnectTimeout[index];
        }

        public static bool[] EnableOutputDataToDS4 => m_Config.enableOutputDataToDS4;
        public static bool getEnableOutputDataToDS4(int index)
        {
            return m_Config.enableOutputDataToDS4[index];
        }

        public static byte[] TouchSensitivity => m_Config.touchSensitivity;
        public static byte[] getTouchSensitivity()
        {
            return m_Config.touchSensitivity;
        }

        public static byte getTouchSensitivity(int index)
        {
            return m_Config.touchSensitivity[index];
        }

        public static bool[] TouchActive => TouchpadActive;
        public static bool GetTouchActive(int index)
        {
            return TouchpadActive[index];
        }

        public static LightbarSettingInfo[] LightbarSettingsInfo => m_Config.lightbarSettingInfo;
        public static LightbarSettingInfo getLightbarSettingsInfo(int index)
        {
            return m_Config.lightbarSettingInfo[index];
        }

        public static bool[] DinputOnly => m_Config.dinputOnly;
        public static bool getDInputOnly(int index)
        {
            return m_Config.dinputOnly[index];
        }

        public static bool[] StartTouchpadOff => m_Config.startTouchpadOff;

        public static bool IsUsingTouchpadForControls(int index)
        {
            return m_Config.touchOutMode[index] == TouchpadOutMode.Controls;
        }

        public static TouchpadOutMode[] TouchOutMode = m_Config.touchOutMode;

        public static bool IsUsingSAForControls(int index)
        {
            return m_Config.gyroOutMode[index] == GyroOutMode.Controls;
        }

        public static string[] SATriggers => m_Config.sATriggers;
        public static string getSATriggers(int index)
        {
            return m_Config.sATriggers[index];
        }

        public static bool[] SATriggerCond => m_Config.sATriggerCond;
        public static bool getSATriggerCond(int index)
        {
            return m_Config.sATriggerCond[index];
        }
        public static void SetSaTriggerCond(int index, string text)
        {
            m_Config.SetSaTriggerCond(index, text);
        }


        public static GyroOutMode[] GyroOutputMode => m_Config.gyroOutMode;
        public static GyroOutMode GetGyroOutMode(int device)
        {
            return m_Config.gyroOutMode[device];
        }

        public static string[] SAMousestickTriggers => m_Config.sAMouseStickTriggers;
        public static string GetSAMouseStickTriggers(int device)
        {
            return m_Config.sAMouseStickTriggers[device];
        }

        public static bool[] SAMouseStickTriggerCond => m_Config.sAMouseStickTriggerCond;
        public static bool GetSAMouseStickTriggerCond(int device)
        {
            return m_Config.sAMouseStickTriggerCond[device];
        }
        public static void SetSaMouseStickTriggerCond(int index, string text)
        {
            m_Config.SetSaMouseStickTriggerCond(index, text);
        }

        public static bool[] GyroMouseStickTriggerTurns = m_Config.gyroMouseStickTriggerTurns;
        public static bool GetGyroMouseStickTriggerTurns(int device)
        {
            return m_Config.gyroMouseStickTriggerTurns[device];
        }

        public static int[] GyroMouseStickHorizontalAxis =>
            m_Config.gyroMouseStickHorizontalAxis;
        public static int getGyroMouseStickHorizontalAxis(int index)
        {
            return m_Config.gyroMouseStickHorizontalAxis[index];
        }

        public static GyroMouseStickInfo[] GyroMouseStickInf => m_Config.gyroMStickInfo;
        public static GyroMouseStickInfo GetGyroMouseStickInfo(int device)
        {
            return m_Config.gyroMStickInfo[device];
        }

        public static GyroDirectionalSwipeInfo[] GyroSwipeInf => m_Config.gyroSwipeInfo;
        public static GyroDirectionalSwipeInfo GetGyroSwipeInfo(int device)
        {
            return m_Config.gyroSwipeInfo[device];
        }

        public static bool[] GyroMouseStickToggle => m_Config.gyroMouseStickToggle;
        public static void SetGyroMouseStickToggle(int index, bool value, ControlService control)
            => m_Config.SetGyroMouseStickToggle(index, value, control);

        public static SASteeringWheelEmulationAxisType[] SASteeringWheelEmulationAxis => m_Config.sASteeringWheelEmulationAxis;
        public static SASteeringWheelEmulationAxisType GetSASteeringWheelEmulationAxis(int index)
        {
            return m_Config.sASteeringWheelEmulationAxis[index];
        }

        public static int[] SASteeringWheelEmulationRange => m_Config.sASteeringWheelEmulationRange;
        public static int GetSASteeringWheelEmulationRange(int index)
        {
            return m_Config.sASteeringWheelEmulationRange[index];
        }

        public static int[][] TouchDisInvertTriggers => m_Config.touchDisInvertTriggers;
        public static int[] getTouchDisInvertTriggers(int index)
        {
            return m_Config.touchDisInvertTriggers[index];
        }

        public static int[] GyroSensitivity => m_Config.gyroSensitivity;
        public static int getGyroSensitivity(int index)
        {
            return m_Config.gyroSensitivity[index];
        }

        public static int[] GyroSensVerticalScale => m_Config.gyroSensVerticalScale;
        public static int getGyroSensVerticalScale(int index)
        {
            return m_Config.gyroSensVerticalScale[index];
        }

        public static int[] GyroInvert => m_Config.gyroInvert;
        public static int getGyroInvert(int index)
        {
            return m_Config.gyroInvert[index];
        }

        public static bool[] GyroTriggerTurns => m_Config.gyroTriggerTurns;
        public static bool getGyroTriggerTurns(int index)
        {
            return m_Config.gyroTriggerTurns[index];
        }

        public static int[] GyroMouseHorizontalAxis => m_Config.gyroMouseHorizontalAxis;
        public static int getGyroMouseHorizontalAxis(int index)
        {
            return m_Config.gyroMouseHorizontalAxis[index];
        }

        public static int[] GyroMouseDeadZone => m_Config.gyroMouseDZ;
        public static int GetGyroMouseDeadZone(int index)
        {
            return m_Config.gyroMouseDZ[index];
        }

        public static void SetGyroMouseDeadZone(int index, int value, ControlService control)
        {
            m_Config.SetGyroMouseDZ(index, value, control);
        }

        public static bool[] GyroMouseToggle => m_Config.gyroMouseToggle;
        public static void SetGyroMouseToggle(int index, bool value, ControlService control) 
            => m_Config.SetGyroMouseToggle(index, value, control);

        public static void SetGyroControlsToggle(int index, bool value, ControlService control)
            => m_Config.SetGyroControlsToggle(index, value, control);

        public static GyroMouseInfo[] GyroMouseInfo => m_Config.gyroMouseInfo;

        public static GyroControlsInfo[] GyroControlsInf => m_Config.gyroControlsInf;
        public static GyroControlsInfo GetGyroControlsInfo(int index)
        {
            return m_Config.gyroControlsInf[index];
        }

        public static SteeringWheelSmoothingInfo[] WheelSmoothInfo => m_Config.wheelSmoothInfo;
        public static int[] SAWheelFuzzValues => m_Config.saWheelFuzzValues;

        //public static DS4Color[] MainColor => m_Config.m_Leds;
        public static ref DS4Color getMainColor(int index)
        {
            return ref m_Config.lightbarSettingInfo[index].ds4winSettings.m_Led;
            //return ref m_Config.m_Leds[index];
        }

        //public static DS4Color[] LowColor => m_Config.m_LowLeds;
        public static ref DS4Color getLowColor(int index)
        {
            return ref m_Config.lightbarSettingInfo[index].ds4winSettings.m_LowLed;
            //return ref m_Config.m_LowLeds[index];
        }

        //public static DS4Color[] ChargingColor => m_Config.m_ChargingLeds;
        public static ref DS4Color getChargingColor(int index)
        {
            return ref m_Config.lightbarSettingInfo[index].ds4winSettings.m_ChargingLed;
            //return ref m_Config.m_ChargingLeds[index];
        }

        //public static DS4Color[] CustomColor => m_Config.m_CustomLeds;
        public static ref DS4Color getCustomColor(int index)
        {
            return ref m_Config.lightbarSettingInfo[index].ds4winSettings.m_CustomLed;
            //return ref m_Config.m_CustomLeds[index];
        }

        //public static bool[] UseCustomLed => m_Config.useCustomLeds;
        public static bool getUseCustomLed(int index)
        {
            return m_Config.lightbarSettingInfo[index].ds4winSettings.useCustomLed;
            //return m_Config.useCustomLeds[index];
        }

        //public static DS4Color[] FlashColor => m_Config.m_FlashLeds;
        public static ref DS4Color getFlashColor(int index)
        {
            return ref m_Config.lightbarSettingInfo[index].ds4winSettings.m_FlashLed;
            //return ref m_Config.m_FlashLeds[index];
        }

        public static byte[] TapSensitivity => m_Config.tapSensitivity;
        public static byte getTapSensitivity(int index)
        {
            return m_Config.tapSensitivity[index];
        }

        public static bool[] DoubleTap => m_Config.doubleTap;
        public static bool getDoubleTap(int index)
        {
            return m_Config.doubleTap[index];
        }

        public static int[] ScrollSensitivity => m_Config.scrollSensitivity;
        public static int[] getScrollSensitivity()
        {
            return m_Config.scrollSensitivity;
        }
        public static int getScrollSensitivity(int index)
        {
            return m_Config.scrollSensitivity[index];
        }

        public static bool[] LowerRCOn => m_Config.lowerRCOn;
        public static bool[] TouchClickPassthru => m_Config.touchClickPassthru;
        public static bool[] TouchpadJitterCompensation => m_Config.touchpadJitterCompensation;
        public static bool getTouchpadJitterCompensation(int index)
        {
            return m_Config.touchpadJitterCompensation[index];
        }

        public static int[] TouchpadInvert => m_Config.touchpadInvert;
        public static int getTouchpadInvert(int index)
        {
            return m_Config.touchpadInvert[index];
        }

        public static TriggerDeadZoneZInfo[] L2ModInfo => m_Config.l2ModInfo;
        public static TriggerDeadZoneZInfo GetL2ModInfo(int index)
        {
            return m_Config.l2ModInfo[index];
        }

        //public static byte[] L2Deadzone => m_Config.l2Deadzone;
        public static byte getL2Deadzone(int index)
        {
            return m_Config.l2ModInfo[index].deadZone;
            //return m_Config.l2Deadzone[index];
        }

        public static TriggerDeadZoneZInfo[] R2ModInfo => m_Config.r2ModInfo;
        public static TriggerDeadZoneZInfo GetR2ModInfo(int index)
        {
            return m_Config.r2ModInfo[index];
        }

        //public static byte[] R2Deadzone => m_Config.r2Deadzone;
        public static byte getR2Deadzone(int index)
        {
            return m_Config.r2ModInfo[index].deadZone;
            //return m_Config.r2Deadzone[index];
        }

        public static double[] SXDeadzone => m_Config.SXDeadzone;
        public static double getSXDeadzone(int index)
        {
            return m_Config.SXDeadzone[index];
        }

        public static double[] SZDeadzone => m_Config.SZDeadzone;
        public static double getSZDeadzone(int index)
        {
            return m_Config.SZDeadzone[index];
        }

        //public static int[] LSDeadzone => m_Config.LSDeadzone;
        public static int getLSDeadzone(int index)
        {
            return m_Config.lsModInfo[index].deadZone;
            //return m_Config.LSDeadzone[index];
        }

        //public static int[] RSDeadzone => m_Config.RSDeadzone;
        public static int getRSDeadzone(int index)
        {
            return m_Config.rsModInfo[index].deadZone;
            //return m_Config.RSDeadzone[index];
        }

        //public static int[] LSAntiDeadzone => m_Config.LSAntiDeadzone;
        public static int getLSAntiDeadzone(int index)
        {
            return m_Config.lsModInfo[index].antiDeadZone;
            //return m_Config.LSAntiDeadzone[index];
        }

        //public static int[] RSAntiDeadzone => m_Config.RSAntiDeadzone;
        public static int getRSAntiDeadzone(int index)
        {
            return m_Config.rsModInfo[index].antiDeadZone;
            //return m_Config.RSAntiDeadzone[index];
        }

        public static StickDeadZoneInfo[] LSModInfo => m_Config.lsModInfo;
        public static StickDeadZoneInfo GetLSDeadInfo(int index)
        {
            return m_Config.lsModInfo[index];
        }

        public static StickDeadZoneInfo[] RSModInfo => m_Config.rsModInfo;
        public static StickDeadZoneInfo GetRSDeadInfo(int index)
        {
            return m_Config.rsModInfo[index];
        }

        public static double[] SXAntiDeadzone => m_Config.SXAntiDeadzone;
        public static double getSXAntiDeadzone(int index)
        {
            return m_Config.SXAntiDeadzone[index];
        }

        public static double[] SZAntiDeadzone => m_Config.SZAntiDeadzone;
        public static double getSZAntiDeadzone(int index)
        {
            return m_Config.SZAntiDeadzone[index];
        }

        //public static int[] LSMaxzone => m_Config.LSMaxzone;
        public static int getLSMaxzone(int index)
        {
            return m_Config.lsModInfo[index].maxZone;
            //return m_Config.LSMaxzone[index];
        }

        //public static int[] RSMaxzone => m_Config.RSMaxzone;
        public static int getRSMaxzone(int index)
        {
            return m_Config.rsModInfo[index].maxZone;
            //return m_Config.RSMaxzone[index];
        }

        public static double[] SXMaxzone => m_Config.SXMaxzone;
        public static double getSXMaxzone(int index)
        {
            return m_Config.SXMaxzone[index];
        }

        public static double[] SZMaxzone => m_Config.SZMaxzone;
        public static double getSZMaxzone(int index)
        {
            return m_Config.SZMaxzone[index];
        }

        //public static int[] L2AntiDeadzone => m_Config.l2AntiDeadzone;
        public static int getL2AntiDeadzone(int index)
        {
            return m_Config.l2ModInfo[index].antiDeadZone;
            //return m_Config.l2AntiDeadzone[index];
        }

        //public static int[] R2AntiDeadzone => m_Config.r2AntiDeadzone;
        public static int getR2AntiDeadzone(int index)
        {
            return m_Config.r2ModInfo[index].antiDeadZone;
            //return m_Config.r2AntiDeadzone[index];
        }

        //public static int[] L2Maxzone => m_Config.l2Maxzone;
        public static int getL2Maxzone(int index)
        {
            return m_Config.l2ModInfo[index].maxZone;
            //return m_Config.l2Maxzone[index];
        }

        //public static int[] R2Maxzone => m_Config.r2Maxzone;
        public static int getR2Maxzone(int index)
        {
            return m_Config.r2ModInfo[index].maxZone;
            //return m_Config.r2Maxzone[index];
        }

        public static double[] LSRotation => m_Config.LSRotation;
        public static double getLSRotation(int index)
        {
            return m_Config.LSRotation[index];
        }

        public static double[] RSRotation => m_Config.RSRotation;
        public static double getRSRotation(int index)
        {
            return m_Config.RSRotation[index];
        }

        public static double[] L2Sens => m_Config.l2Sens;
        public static double getL2Sens(int index)
        {
            return m_Config.l2Sens[index];
        }

        public static double[] R2Sens => m_Config.r2Sens;
        public static double getR2Sens(int index)
        {
            return m_Config.r2Sens[index];
        }

        public static double[] SXSens => m_Config.SXSens;
        public static double getSXSens(int index)
        {
            return m_Config.SXSens[index];
        }

        public static double[] SZSens => m_Config.SZSens;
        public static double getSZSens(int index)
        {
            return m_Config.SZSens[index];
        }

        public static double[] LSSens => m_Config.LSSens;
        public static double getLSSens(int index)
        {
            return m_Config.LSSens[index];
        }

        public static double[] RSSens => m_Config.RSSens;
        public static double getRSSens(int index)
        {
            return m_Config.RSSens[index];
        }

        public static int[] BTPollRate => m_Config.btPollRate;
        public static int getBTPollRate(int index)
        {
            return m_Config.btPollRate[index];
        }

        public static SquareStickInfo[] SquStickInfo => m_Config.squStickInfo;
        public static SquareStickInfo GetSquareStickInfo(int device)
        {
            return m_Config.squStickInfo[device];
        }

        public static StickAntiSnapbackInfo[] LSAntiSnapbackInfo => m_Config.lsAntiSnapbackInfo;
        public static StickAntiSnapbackInfo GetLSAntiSnapbackInfo(int device)
        {
            return m_Config.lsAntiSnapbackInfo[device];
        }

        public static StickAntiSnapbackInfo[] RSAntiSnapbackInfo => m_Config.rsAntiSnapbackInfo;
        public static StickAntiSnapbackInfo GetRSAntiSnapbackInfo(int device)
        {
            return m_Config.rsAntiSnapbackInfo[device];
        }

        public static StickOutputSetting[] LSOutputSettings => m_Config.lsOutputSettings;
        public static StickOutputSetting[] RSOutputSettings => m_Config.rsOutputSettings;

        public static TriggerOutputSettings[] L2OutputSettings => m_Config.l2OutputSettings;
        public static TriggerOutputSettings[] R2OutputSettings => m_Config.r2OutputSettings;

        public static void setLsOutCurveMode(int index, int value)
        {
            m_Config.setLsOutCurveMode(index, value);
        }
        public static int getLsOutCurveMode(int index)
        {
            return m_Config.getLsOutCurveMode(index);
        }
        public static BezierCurve[] lsOutBezierCurveObj => m_Config.lsOutBezierCurveObj;

        public static void setRsOutCurveMode(int index, int value)
        {
            m_Config.setRsOutCurveMode(index, value);
        }
        public static int getRsOutCurveMode(int index)
        {
            return m_Config.getRsOutCurveMode(index);
        }
        public static BezierCurve[] rsOutBezierCurveObj => m_Config.rsOutBezierCurveObj;

        public static void setL2OutCurveMode(int index, int value)
        {
            m_Config.setL2OutCurveMode(index, value);
        }
        public static int getL2OutCurveMode(int index)
        {
            return m_Config.getL2OutCurveMode(index);
        }
        public static BezierCurve[] l2OutBezierCurveObj => m_Config.l2OutBezierCurveObj;

        public static void setR2OutCurveMode(int index, int value)
        {
            m_Config.setR2OutCurveMode(index, value);
        }
        public static int getR2OutCurveMode(int index)
        {
            return m_Config.getR2OutCurveMode(index);
        }
        public static BezierCurve[] r2OutBezierCurveObj => m_Config.r2OutBezierCurveObj;

        public static void setSXOutCurveMode(int index, int value)
        {
            m_Config.setSXOutCurveMode(index, value);
        }
        public static int getSXOutCurveMode(int index)
        {
            return m_Config.getSXOutCurveMode(index);
        }
        public static BezierCurve[] sxOutBezierCurveObj => m_Config.sxOutBezierCurveObj;

        public static void setSZOutCurveMode(int index, int value)
        {
            m_Config.setSZOutCurveMode(index, value);
        }
        public static int getSZOutCurveMode(int index)
        {
            return m_Config.getSZOutCurveMode(index);
        }
        public static BezierCurve[] szOutBezierCurveObj => m_Config.szOutBezierCurveObj;

        public static bool[] TrackballMode => m_Config.trackballMode;
        public static bool getTrackballMode(int index)
        {
            return m_Config.trackballMode[index];
        }

        public static double[] TrackballFriction => m_Config.trackballFriction;
        public static double getTrackballFriction(int index)
        {
            return m_Config.trackballFriction[index];
        }

        public static TouchpadAbsMouseSettings[] TouchAbsMouse => m_Config.touchpadAbsMouse;
        public static TouchpadRelMouseSettings[] TouchRelMouse => m_Config.touchpadRelMouse;

        public static ControlServiceDeviceOptions DeviceOptions => m_Config.deviceOptions;

        public static OutContType[] OutContType => m_Config.outputDevType;
        public static string[] LaunchProgram => m_Config.launchProgram;
        public static string[] ProfilePath => m_Config.profilePath;
        public static string[] OlderProfilePath => m_Config.olderProfilePath;
        public static bool[] DistanceProfiles = m_Config.distanceProfiles;

        public static List<string>[] ProfileActions => m_Config.profileActions;
        public static int getProfileActionCount(int index)
        {
            return m_Config.profileActionCount[index];
        }

        public static void CalculateProfileActionCount(int index)
        {
            m_Config.CalculateProfileActionCount(index);
        }

        public static List<string> getProfileActions(int index)
        {
            return m_Config.profileActions[index];
        }
        
        public static void UpdateDS4CSetting (int deviceNum, string buttonName, bool shift, object action, string exts, DS4KeyType kt, int trigger = 0)
        {
            m_Config.UpdateDs4ControllerSetting(deviceNum, buttonName, shift, action, exts, kt, trigger);
            m_Config.containsCustomAction[deviceNum] = m_Config.HasCustomActions(deviceNum);
            m_Config.containsCustomExtras[deviceNum] = m_Config.HasCustomExtras(deviceNum);
        }

        public static void UpdateDS4Extra(int deviceNum, string buttonName, bool shift, string exts)
        {
            m_Config.UpdateDs4ControllerExtra(deviceNum, buttonName, shift, exts);
            m_Config.containsCustomAction[deviceNum] = m_Config.HasCustomActions(deviceNum);
            m_Config.containsCustomExtras[deviceNum] = m_Config.HasCustomExtras(deviceNum);
        }

        public static ControlActionData GetDS4Action(int deviceNum, string buttonName, bool shift) => m_Config.GetDs4Action(deviceNum, buttonName, shift);
        public static ControlActionData GetDS4Action(int deviceNum, DS4Controls control, bool shift) => m_Config.GetDs4Action(deviceNum, control, shift);
        public static DS4KeyType GetDS4KeyType(int deviceNum, string buttonName, bool shift) => m_Config.GetDs4KeyType(deviceNum, buttonName, shift);
        public static string GetDS4Extra(int deviceNum, string buttonName, bool shift) => m_Config.GetDs4Extra(deviceNum, buttonName, shift);
        public static int GetDS4STrigger(int deviceNum, string buttonName) => m_Config.GetDs4STrigger(deviceNum, buttonName);
        public static int GetDS4STrigger(int deviceNum, DS4Controls control) => m_Config.GetDs4STrigger(deviceNum, control);
        public static List<DS4ControlSettings> getDS4CSettings(int device) => m_Config.ds4settings[device];
        public static DS4ControlSettings GetDS4CSetting(int deviceNum, string control) => m_Config.GetDs4ControllerSetting(deviceNum, control);
        public static DS4ControlSettings GetDS4CSetting(int deviceNum, DS4Controls control) => m_Config.GetDs4ControllerSetting(deviceNum, control);
        public static ControlSettingsGroup GetControlSettingsGroup(int deviceNum) => m_Config.ds4controlSettings[deviceNum];
        public static bool HasCustomActions(int deviceNum) => m_Config.HasCustomActions(deviceNum);
        public static bool HasCustomExtras(int deviceNum) => m_Config.HasCustomExtras(deviceNum);

        public static bool containsCustomAction(int deviceNum)
        {
            return m_Config.containsCustomAction[deviceNum];
        }

        public static bool containsCustomExtras(int deviceNum)
        {
            return m_Config.containsCustomExtras[deviceNum];
        }

        public static void SaveAction(string name, string controls, int mode,
            string details, bool edit, string extras = "")
        {
            m_Config.SaveAction(name, controls, mode, details, edit, extras);
            Mapping.actionDone.Add(new Mapping.ActionState());
        }

        public static void RemoveAction(string name)
        {
            m_Config.RemoveAction(name);
        }

        public static bool LoadActions() => m_Config.LoadActions();

        public static List<SpecialAction> GetActions() => m_Config.actions;

        public static int GetActionIndexOf(string name)
        {
            return m_Config.GetActionIndexOf(name);
        }

        public static int GetProfileActionIndexOf(int device, string name)
        {
            int index = -1;
            m_Config.profileActionIndexDict[device].TryGetValue(name, out index);
            return index;
        }

        public static SpecialAction GetAction(string name)
        {
            return m_Config.GetAction(name);
        }

        public static SpecialAction GetProfileAction(int device, string name)
        {
            SpecialAction sA = null;
            m_Config.profileActionDict[device].TryGetValue(name, out sA);
            return sA;
        }

        public static void CalculateProfileActionDicts(int device)
        {
            m_Config.CalculateProfileActionDicts(device);
        }

        public static void CacheProfileCustomsFlags(int device)
        {
            m_Config.CacheProfileCustomsFlags(device);
        }

        public static void CacheExtraProfileInfo(int device)
        {
            m_Config.CacheExtraProfileInfo(device);
        }

        public static X360Controls getX360ControlsByName(string key)
        {
            return m_Config.GetX360ControlsByName(key);
        }

        public static string GetX360ControlString(X360Controls key)
        {
            return m_Config.GetX360ControlString(key);
        }

        public static DS4Controls getDS4ControlsByName(string key)
        {
            return m_Config.GetDs4ControlsByName(key);
        }

        public static X360Controls getDefaultX360ControlBinding(DS4Controls dc)
        {
            return DefaultButtonMapping[(int)dc];
        }

        public static bool containsLinkedProfile(string serial)
        {
            string tempSerial = serial.Replace(":", string.Empty);
            return m_Config.linkedProfiles.ContainsKey(tempSerial);
        }

        public static string getLinkedProfile(string serial)
        {
            string temp = string.Empty;
            string tempSerial = serial.Replace(":", string.Empty);
            if (m_Config.linkedProfiles.ContainsKey(tempSerial))
            {
                temp = m_Config.linkedProfiles[tempSerial];
            }

            return temp;
        }

        public static void changeLinkedProfile(string serial, string profile)
        {
            string tempSerial = serial.Replace(":", string.Empty);
            m_Config.linkedProfiles[tempSerial] = profile;
        }

        public static void removeLinkedProfile(string serial)
        {
            string tempSerial = serial.Replace(":", string.Empty);
            if (m_Config.linkedProfiles.ContainsKey(tempSerial))
            {
                m_Config.linkedProfiles.Remove(tempSerial);
            }
        }

        public static bool Load() => m_Config.Load();
        
        public static bool LoadProfile(int device, bool launchprogram, ControlService control,
            bool xinputChange = true, bool postLoad = true)
        {
            bool result = m_Config.LoadProfile(device, launchprogram, control, "", xinputChange, postLoad);
            TempProfileNames[device] = string.Empty;
            UseTempProfiles[device] = false;
            TempProfileDistance[device] = false;

            return result;
        }

        public static bool LoadTempProfile(int device, string name, bool launchprogram,
            ControlService control, bool xinputChange = true)
        {
            bool result = m_Config.LoadProfile(device, launchprogram, control, RuntimeAppDataPath + @"\Profiles\" + name + ".xml");
            TempProfileNames[device] = name;
            UseTempProfiles[device] = true;
            TempProfileDistance[device] = name.ToLower().Contains("distance");

            return result;
        }

        public static void LoadBlankDevProfile(int device, bool launchprogram, ControlService control,
            bool xinputChange = true, bool postLoad = true)
        {
            m_Config.LoadBlankProfile(device, launchprogram, control, "", xinputChange, postLoad);
            m_Config.EstablishDefaultSpecialActions(device);
            m_Config.CacheExtraProfileInfo(device);

            TempProfileNames[device] = string.Empty;
            UseTempProfiles[device] = false;
            TempProfileDistance[device] = false;
        }

        public static void LoadBlankDS4Profile(int device, bool launchprogram, ControlService control,
            bool xinputChange = true, bool postLoad = true)
        {
            m_Config.LoadBlankDs4Profile(device, launchprogram, control, "", xinputChange, postLoad);
            m_Config.EstablishDefaultSpecialActions(device);
            m_Config.CacheExtraProfileInfo(device);

            TempProfileNames[device] = string.Empty;
            UseTempProfiles[device] = false;
            TempProfileDistance[device] = false;
        }

        public static void LoadDefaultGamepadGyroProfile(int device, bool launchprogram, ControlService control,
            bool xinputChange = true, bool postLoad = true)
        {
            m_Config.LoadDefaultGamepadGyroProfile(device, launchprogram, control, "", xinputChange, postLoad);
            m_Config.EstablishDefaultSpecialActions(device);
            m_Config.CacheExtraProfileInfo(device);

            TempProfileNames[device] = string.Empty;
            UseTempProfiles[device] = false;
            TempProfileDistance[device] = false;
        }

        public static void LoadDefaultDS4GamepadGyroProfile(int device, bool launchprogram, ControlService control,
            bool xinputChange = true, bool postLoad = true)
        {
            m_Config.LoadDefaultDS4GamepadGyroProfile(device, launchprogram, control, "", xinputChange, postLoad);
            m_Config.EstablishDefaultSpecialActions(device);
            m_Config.CacheExtraProfileInfo(device);

            TempProfileNames[device] = string.Empty;
            UseTempProfiles[device] = false;
            TempProfileDistance[device] = false;
        }

        public static void LoadDefaultMixedControlsProfile(int device, bool launchprogram, ControlService control,
            bool xinputChange = true, bool postLoad = true)
        {
            m_Config.LoadDefaultMixedControlsProfile(device, launchprogram, control, "", xinputChange, postLoad);
            m_Config.EstablishDefaultSpecialActions(device);
            m_Config.CacheExtraProfileInfo(device);

            TempProfileNames[device] = string.Empty;
            UseTempProfiles[device] = false;
            TempProfileDistance[device] = false;
        }

        public static void LoadDefaultDS4MixedControlsProfile(int device, bool launchprogram, ControlService control,
            bool xinputChange = true, bool postLoad = true)
        {
            m_Config.LoadDefaultMixedControlsProfile(device, launchprogram, control, "", xinputChange, postLoad);
            m_Config.EstablishDefaultSpecialActions(device);
            m_Config.CacheExtraProfileInfo(device);

            TempProfileNames[device] = string.Empty;
            UseTempProfiles[device] = false;
            TempProfileDistance[device] = false;
        }

        public static void LoadDefaultMixedGyroMouseProfile(int device, bool launchprogram, ControlService control,
            bool xinputChange = true, bool postLoad = true)
        {
            m_Config.LoadDefaultMixedGyroMouseProfile(device, launchprogram, control, "", xinputChange, postLoad);
            m_Config.EstablishDefaultSpecialActions(device);
            m_Config.CacheExtraProfileInfo(device);

            TempProfileNames[device] = string.Empty;
            UseTempProfiles[device] = false;
            TempProfileDistance[device] = false;
        }

        public static void LoadDefaultDS4MixedGyroMouseProfile(int device, bool launchprogram, ControlService control,
            bool xinputChange = true, bool postLoad = true)
        {
            m_Config.LoadDefaultDs4MixedGyroMouseProfile(device, launchprogram, control, "", xinputChange, postLoad);
            m_Config.EstablishDefaultSpecialActions(device);
            m_Config.CacheExtraProfileInfo(device);

            TempProfileNames[device] = string.Empty;
            UseTempProfiles[device] = false;
            TempProfileDistance[device] = false;
        }

        public static void LoadDefaultKBMProfile(int device, bool launchprogram, ControlService control,
            bool xinputChange = true, bool postLoad = true)
        {
            m_Config.LoadDefaultKBMProfile(device, launchprogram, control, "", xinputChange, postLoad);
            m_Config.EstablishDefaultSpecialActions(device);
            m_Config.CacheExtraProfileInfo(device);

            TempProfileNames[device] = string.Empty;
            UseTempProfiles[device] = false;
            TempProfileDistance[device] = false;
        }

        public static void LoadDefaultKBMGyroMouseProfile(int device, bool launchprogram, ControlService control,
            bool xinputChange = true, bool postLoad = true)
        {
            m_Config.LoadDefaultKBMGyroMouseProfile(device, launchprogram, control, "", xinputChange, postLoad);
            m_Config.EstablishDefaultSpecialActions(device);
            m_Config.CacheExtraProfileInfo(device);

            TempProfileNames[device] = string.Empty;
            UseTempProfiles[device] = false;
            TempProfileDistance[device] = false;
        }

        public static bool Save()
        {
            return m_Config.Save();
        }

        public static void SaveProfile(int device, string proName)
        {
            m_Config.SaveProfile(device, proName);
        }

        public static void SaveAsNewProfile(int device, string propath)
        {
            m_Config.SaveAsNewProfile(device, propath);
        }

        public static bool SaveLinkedProfiles()
        {
            return m_Config.SaveLinkedProfiles();
        }

        public static bool LoadLinkedProfiles()
        {
            return m_Config.LoadLinkedProfiles();
        }

        public static bool SaveControllerConfigs(DS4Device device = null)
        {
            if (device != null)
                return m_Config.SaveControllerConfigsForDevice(device);

            for (int idx = 0; idx < ControlService.MAX_DS4_CONTROLLER_COUNT; idx++)
                if (Program.rootHub.DS4Controllers[idx] != null)
                    m_Config.SaveControllerConfigsForDevice(Program.rootHub.DS4Controllers[idx]);

            return true;
        }

        public static bool LoadControllerConfigs(DS4Device device = null)
        {
            if (device != null)
                return m_Config.LoadControllerConfigsForDevice(device);

            for (int idx = 0; idx < ControlService.MAX_DS4_CONTROLLER_COUNT; idx++)
                if (Program.rootHub.DS4Controllers[idx] != null)
                    m_Config.LoadControllerConfigsForDevice(Program.rootHub.DS4Controllers[idx]);

            return true;
        }

        private static byte applyRatio(byte b1, byte b2, double r)
        {
            if (r > 100.0)
                r = 100.0;
            else if (r < 0.0)
                r = 0.0;

            r *= 0.01;
            return (byte)Math.Round((b1 * (1 - r)) + b2 * r, 0);
        }

        public static DS4Color getTransitionedColor(ref DS4Color c1, ref DS4Color c2, double ratio)
        {
            //Color cs = Color.FromArgb(c1.red, c1.green, c1.blue);
            DS4Color cs = new DS4Color
            {
                red = applyRatio(c1.red, c2.red, ratio),
                green = applyRatio(c1.green, c2.green, ratio),
                blue = applyRatio(c1.blue, c2.blue, ratio)
            };
            return cs;
        }

        private static Color applyRatio(Color c1, Color c2, uint r)
        {
            float ratio = r / 100f;
            float hue1 = c1.GetHue();
            float hue2 = c2.GetHue();
            float bri1 = c1.GetBrightness();
            float bri2 = c2.GetBrightness();
            float sat1 = c1.GetSaturation();
            float sat2 = c2.GetSaturation();
            float hr = hue2 - hue1;
            float br = bri2 - bri1;
            float sr = sat2 - sat1;
            Color csR;
            if (bri1 == 0)
                csR = HuetoRGB(hue2,sat2,bri2 - br*ratio);
            else
                csR = HuetoRGB(hue2 - hr * ratio, sat2 - sr * ratio, bri2 - br * ratio);

            return csR;
        }

        public static Color HuetoRGB(float hue, float sat, float bri)
        {
            float C = (1-Math.Abs(2*bri)-1)* sat;
            float X = C * (1 - Math.Abs((hue / 60) % 2 - 1));
            float m = bri - C / 2;
            float R, G, B;
            if (0 <= hue && hue < 60)
            {
                R = C; G = X; B = 0;
            }
            else if (60 <= hue && hue < 120)
            {
                R = X; G = C; B = 0;
            }
            else if (120 <= hue && hue < 180)
            {
                R = 0; G = C; B = X;
            }
            else if (180 <= hue && hue < 240)
            {
                R = 0; G = X; B = C;
            }
            else if (240 <= hue && hue < 300)
            {
                R = X; G = 0; B = C;
            }
            else if (300 <= hue && hue < 360)
            {
                R = C; G = 0; B = X;
            }
            else
            {
                R = 255; G = 0; B = 0;
            }

            R += m; G += m; B += m;
            R *= 255.0f; G *= 255.0f; B *= 255.0f;
            return Color.FromArgb((int)R, (int)G, (int)B);
        }

        public static double Clamp(double min, double value, double max)
        {
            return (value < min) ? min : (value > max) ? max : value;
        }

        private static int ClampInt(int min, int value, int max)
        {
            return (value < min) ? min : (value > max) ? max : value;
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
        /// Take Windows virtual key value and refresh action alias for currently used output KB+M system
        /// </summary>
        /// <param name="setting">Instance of edited DS4ControlSettings object</param>
        /// <param name="shift">Flag to indicate if shift action is being modified</param>
        public static void RefreshActionAlias(DS4ControlSettings setting, bool shift)
        {
            if (!shift)
            {
                setting.ActionData.actionAlias = 0;
                if (setting.ControlActionType == DS4ControlSettings.ActionType.Key)
                {
                    setting.ActionData.actionAlias = outputKBMMapping.GetRealEventKey(Convert.ToUInt32(setting.ActionData.actionKey));
                }
            }
            else
            {
                setting.ShiftAction.actionAlias = 0;
                if (setting.ShiftActionType == DS4ControlSettings.ActionType.Key)
                {
                    setting.ShiftAction.actionAlias = outputKBMMapping.GetRealEventKey(Convert.ToUInt32(setting.ShiftAction.actionKey));
                }
            }
        }

        public static void RefreshExtrasButtons(int deviceNum, List<DS4Controls> devButtons)
        {
            m_Config.ds4controlSettings[deviceNum].ResetExtraButtons();
            if (devButtons != null)
            {
                m_Config.ds4controlSettings[deviceNum].EstablishExtraButtons(devButtons);
            }
        }
    }
}
