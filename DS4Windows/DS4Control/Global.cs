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
    public partial class Global
    {
        private static readonly Lazy<Global> LazyInstance = new(() => new Global());

        public static Global Instance => LazyInstance.Value;

        private Global()
        {
        }

        // Use 15 minutes for default Idle Disconnect when initially enabling the option
        public const int DEFAULT_ENABLE_IDLE_DISCONN_MINS = 15;
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

        private readonly BackingStore _config = new();

        protected static Int32 m_IdleTimeout = 600000;

        /// <summary>
        ///     Full path to main executable.
        /// </summary>
        public static string ExecutableLocation => Process.GetCurrentProcess().MainModule.FileName;

        /// <summary>
        ///     Directory containing the <see cref="ExecutableFileName"/>.
        /// </summary>
        public static string ExecutableDirectory => Directory.GetParent(ExecutableLocation).FullName;

        /// <summary>
        ///     File name of main executable.
        /// </summary>
        public static string ExecutableFileName => Path.GetFileName(ExecutableLocation);

        /// <summary>
        ///     <see cref="FileVersionInfo"/> of <see cref="ExecutableLocation"/>.
        /// </summary>
        public static FileVersionInfo ExecutableFileVersion => FileVersionInfo.GetVersionInfo(ExecutableLocation);

        /// <summary>
        ///     Product version of <see cref="ExecutableFileVersion"/>.
        /// </summary>
        public static string ExecutableProductVersion => ExecutableFileVersion.ProductVersion;

        /// <summary>
        ///     Numeric representation of <see cref="ExecutableFileVersion"/>.
        /// </summary>
        public static ulong ExecutableVersionLong => (ulong)ExecutableFileVersion.ProductMajorPart << 48 |
            (ulong)ExecutableFileVersion.ProductMinorPart << 32 | (ulong)ExecutableFileVersion.ProductBuildPart << 16;

        /// <summary>
        ///     Is the underlying OS Windows 8 (or newer).
        /// </summary>
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

        /// <summary>
        ///     Is the underlying OS Windows 10 (or newer).
        /// </summary>
        public static bool IsWin10OrGreater => Environment.OSVersion.Version.Major >= 10;

        public static string RuntimeAppDataPath { get; set; } = RoamingAppDataPath;

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

        public void FindConfigLocation()
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

        public  void CreateStdActions()
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
        public bool UseExclusiveMode
        {
            set { _config.useExclusiveMode = value; }
            get { return _config.useExclusiveMode; }
        } // -- Re-Enable Ex Mode Ends here

        public bool getUseExclusiveMode()
        {
            return _config.useExclusiveMode;
        }
        public DateTime LastChecked
        {
            set { _config.lastChecked = value; }
            get { return _config.lastChecked; }
        }

        public int CheckWhen
        {
            set { _config.CheckWhen = value; }
            get { return _config.CheckWhen; }
        }

        public string LastVersionChecked
        {
            get { return _config.lastVersionChecked; }
            set
            {
                _config.lastVersionChecked = value;
                _config.lastVersionCheckedNum = CompileVersionNumberFromString(value);
            }
        }

        public ulong LastVersionCheckedNum
        {
            get { return _config.lastVersionCheckedNum; }
        }

        public int Notifications
        {
            set { _config.notifications = value; }
            get { return _config.notifications; }
        }

        public bool DCBTatStop
        {
            set { _config.disconnectBTAtStop = value; }
            get { return _config.disconnectBTAtStop; }
        }

        public  bool SwipeProfiles
        {
            set { _config.swipeProfiles = value; }
            get { return _config.swipeProfiles; }
        }

        public  bool DS4Mapping
        {
            set { _config.ds4Mapping = value; }
            get { return _config.ds4Mapping; }
        }

        public  bool QuickCharge
        {
            set { _config.quickCharge = value; }
            get { return _config.quickCharge; }
        }

        public  bool getQuickCharge()
        {
            return _config.quickCharge;
        }

        public  bool CloseMini
        {
            set { _config.closeMini = value; }
            get { return _config.closeMini; }
        }

        public  bool StartMinimized
        {
            set { _config.startMinimized = value; }
            get { return _config.startMinimized; }
        }

        public  bool MinToTaskbar
        {
            set { _config.minToTaskbar = value; }
            get { return _config.minToTaskbar; }
        }

        public  bool GetMinToTaskbar()
        {
            return _config.minToTaskbar;
        }

        public  int FormWidth
        {
            set { _config.formWidth = value; }
            get { return _config.formWidth; }
        }

        public  int FormHeight
        {
            set { _config.formHeight = value; }
            get { return _config.formHeight; }
        }

        public  int FormLocationX
        {
            set { _config.formLocationX = value; }
            get { return _config.formLocationX; }
        }

        public  int FormLocationY
        {
            set { _config.formLocationY = value; }
            get { return _config.formLocationY; }
        }

        public  string UseLang
        {
            set { _config.useLang = value; }
            get { return _config.useLang; }
        }

        public  bool DownloadLang
        {
            set { _config.downloadLang = value; }
            get { return _config.downloadLang; }
        }

        public  bool FlashWhenLate
        {
            set { _config.FlashWhenLate = value; }
            get { return _config.FlashWhenLate; }
        }

        public  bool getFlashWhenLate()
        {
            return _config.FlashWhenLate;
        }

        public  int FlashWhenLateAt
        {
            set { _config.flashWhenLateAt = value; }
            get { return _config.flashWhenLateAt; }
        }

        public  int getFlashWhenLateAt()
        {
            return _config.flashWhenLateAt;
        }

        public  bool isUsingUDPServer()
        {
            return _config.UseUdpServer;
        }
        public  void setUsingUDPServer(bool state)
        {
            _config.UseUdpServer = state;
        }

        public  int getUDPServerPortNum()
        {
            return _config.UdpServerPort;
        }
        public  void setUDPServerPort(int value)
        {
            _config.UdpServerPort = value;
        }

        public  string getUDPServerListenAddress()
        {
            return _config.UdpServerListenAddress;
        }
        public  void setUDPServerListenAddress(string value)
        {
            _config.UdpServerListenAddress = value.Trim();
        }

        public  bool UseUDPSeverSmoothing
        {
            get => _config.UseUdpSmoothing;
            set => _config.UseUdpSmoothing = value;
        }

        public  bool IsUsingUDPServerSmoothing()
        {
            return _config.UseUdpSmoothing;
        }

        public  double UDPServerSmoothingMincutoff
        {
            get => _config.udpSmoothingMincutoff;
            set
            {
                double temp = _config.udpSmoothingMincutoff;
                if (temp == value) return;
                _config.udpSmoothingMincutoff = value;
                UDPServerSmoothingMincutoffChanged?.Invoke(null, EventArgs.Empty);
            }
        }
        public static event EventHandler UDPServerSmoothingMincutoffChanged;

        public  double UDPServerSmoothingBeta
        {
            get => _config.udpSmoothingBeta;
            set
            {
                double temp = _config.udpSmoothingBeta;
                if (temp == value) return;
                _config.udpSmoothingBeta = value;
                UDPServerSmoothingBetaChanged?.Invoke(null, EventArgs.Empty);
            }
        }
        public static event EventHandler UDPServerSmoothingBetaChanged;

        public  TrayIconChoice UseIconChoice
        {
            get => _config.UseIconChoice;
            set => _config.UseIconChoice = value;
        }

        public  AppThemeChoice UseCurrentTheme
        {
            get => _config.ThemeChoice;
            set => _config.ThemeChoice = value;
        }

        public  bool UseCustomSteamFolder
        {
            set { _config.UseCustomSteamFolder = value; }
            get { return _config.UseCustomSteamFolder; }
        }

        public  string CustomSteamFolder
        {
            set { _config.CustomSteamFolder = value; }
            get { return _config.CustomSteamFolder; }
        }

        public  bool AutoProfileRevertDefaultProfile
        {
            set { _config.AutoProfileRevertDefaultProfile = value; }
            get { return _config.AutoProfileRevertDefaultProfile; }
        }

        /// <summary>
        /// Fake name used for user copy of DS4Windows.exe
        /// </summary>
        public  string FakeExeName
        {
            get => _config.fakeExeFileName;
            set
            {
                bool valid = !(value.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0);
                if (valid)
                {
                    _config.fakeExeFileName = value;
                }
            }
        }

        // controller/profile specfic values
        public  ButtonMouseInfo[] ButtonMouseInfos => _config.buttonMouseInfos;

        public  byte[] RumbleBoost => _config.rumble;
        public  byte getRumbleBoost(int index)
        {
            return _config.rumble[index];
        }

        public  void setRumbleAutostopTime(int index, int value)
        {
            _config.rumbleAutostopTime[index] = value;
            
            DS4Device tempDev = Program.rootHub.DS4Controllers[index];
            if (tempDev != null && tempDev.isSynced())
                tempDev.RumbleAutostopTime = value;
        }

        public  int getRumbleAutostopTime(int index)
        {
            return _config.rumbleAutostopTime[index];
        }

        public  bool[] EnableTouchToggle => _config.enableTouchToggle;
        public  bool getEnableTouchToggle(int index)
        {
            return _config.enableTouchToggle[index];
        }

        public  int[] IdleDisconnectTimeout => _config.idleDisconnectTimeout;
        public  int getIdleDisconnectTimeout(int index)
        {
            return _config.idleDisconnectTimeout[index];
        }

        public  bool[] EnableOutputDataToDS4 => _config.enableOutputDataToDS4;
        public  bool getEnableOutputDataToDS4(int index)
        {
            return _config.enableOutputDataToDS4[index];
        }

        public  byte[] TouchSensitivity => _config.touchSensitivity;
        public  byte[] getTouchSensitivity()
        {
            return _config.touchSensitivity;
        }

        public  byte getTouchSensitivity(int index)
        {
            return _config.touchSensitivity[index];
        }

        public  bool[] TouchActive => TouchpadActive;
        public  bool GetTouchActive(int index)
        {
            return TouchpadActive[index];
        }

        public  LightbarSettingInfo[] LightbarSettingsInfo => _config.lightbarSettingInfo;
        public  LightbarSettingInfo getLightbarSettingsInfo(int index)
        {
            return _config.lightbarSettingInfo[index];
        }

        public  bool[] DinputOnly => _config.dinputOnly;
        public  bool getDInputOnly(int index)
        {
            return _config.dinputOnly[index];
        }

        public  bool[] StartTouchpadOff => _config.startTouchpadOff;

        public  bool IsUsingTouchpadForControls(int index)
        {
            return _config.touchOutMode[index] == TouchpadOutMode.Controls;
        }

        public TouchpadOutMode[] TouchOutMode => _config.touchOutMode;

        public  bool IsUsingSAForControls(int index)
        {
            return _config.gyroOutMode[index] == GyroOutMode.Controls;
        }

        public  string[] SATriggers => _config.sATriggers;
        public  string getSATriggers(int index)
        {
            return _config.sATriggers[index];
        }

        public  bool[] SATriggerCond => _config.sATriggerCond;
        public  bool getSATriggerCond(int index)
        {
            return _config.sATriggerCond[index];
        }
        public  void SetSaTriggerCond(int index, string text)
        {
            _config.SetSaTriggerCond(index, text);
        }


        public  GyroOutMode[] GyroOutputMode => _config.gyroOutMode;
        public  GyroOutMode GetGyroOutMode(int device)
        {
            return _config.gyroOutMode[device];
        }

        public  string[] SAMousestickTriggers => _config.sAMouseStickTriggers;
        public  string GetSAMouseStickTriggers(int device)
        {
            return _config.sAMouseStickTriggers[device];
        }

        public  bool[] SAMouseStickTriggerCond => _config.sAMouseStickTriggerCond;
        public  bool GetSAMouseStickTriggerCond(int device)
        {
            return _config.sAMouseStickTriggerCond[device];
        }
        public  void SetSaMouseStickTriggerCond(int index, string text)
        {
            _config.SetSaMouseStickTriggerCond(index, text);
        }

        public  bool[] GyroMouseStickTriggerTurns => _config.gyroMouseStickTriggerTurns;
        public  bool GetGyroMouseStickTriggerTurns(int device)
        {
            return _config.gyroMouseStickTriggerTurns[device];
        }

        public  int[] GyroMouseStickHorizontalAxis =>
            _config.gyroMouseStickHorizontalAxis;
        public  int getGyroMouseStickHorizontalAxis(int index)
        {
            return _config.gyroMouseStickHorizontalAxis[index];
        }

        public  GyroMouseStickInfo[] GyroMouseStickInf => _config.gyroMStickInfo;
        public  GyroMouseStickInfo GetGyroMouseStickInfo(int device)
        {
            return _config.gyroMStickInfo[device];
        }

        public  GyroDirectionalSwipeInfo[] GyroSwipeInf => _config.gyroSwipeInfo;
        public  GyroDirectionalSwipeInfo GetGyroSwipeInfo(int device)
        {
            return _config.gyroSwipeInfo[device];
        }

        public  bool[] GyroMouseStickToggle => _config.gyroMouseStickToggle;
        public  void SetGyroMouseStickToggle(int index, bool value, ControlService control)
            => _config.SetGyroMouseStickToggle(index, value, control);

        public  SASteeringWheelEmulationAxisType[] SASteeringWheelEmulationAxis => _config.sASteeringWheelEmulationAxis;
        public  SASteeringWheelEmulationAxisType GetSASteeringWheelEmulationAxis(int index)
        {
            return _config.sASteeringWheelEmulationAxis[index];
        }

        public  int[] SASteeringWheelEmulationRange => _config.sASteeringWheelEmulationRange;
        public  int GetSASteeringWheelEmulationRange(int index)
        {
            return _config.sASteeringWheelEmulationRange[index];
        }

        public  int[][] TouchDisInvertTriggers => _config.touchDisInvertTriggers;
        public  int[] getTouchDisInvertTriggers(int index)
        {
            return _config.touchDisInvertTriggers[index];
        }

        public  int[] GyroSensitivity => _config.gyroSensitivity;
        public  int getGyroSensitivity(int index)
        {
            return _config.gyroSensitivity[index];
        }

        public  int[] GyroSensVerticalScale => _config.gyroSensVerticalScale;
        public  int getGyroSensVerticalScale(int index)
        {
            return _config.gyroSensVerticalScale[index];
        }

        public  int[] GyroInvert => _config.gyroInvert;
        public  int getGyroInvert(int index)
        {
            return _config.gyroInvert[index];
        }

        public  bool[] GyroTriggerTurns => _config.gyroTriggerTurns;
        public  bool getGyroTriggerTurns(int index)
        {
            return _config.gyroTriggerTurns[index];
        }

        public  int[] GyroMouseHorizontalAxis => _config.gyroMouseHorizontalAxis;
        public  int getGyroMouseHorizontalAxis(int index)
        {
            return _config.gyroMouseHorizontalAxis[index];
        }

        public  int[] GyroMouseDeadZone => _config.gyroMouseDZ;
        public  int GetGyroMouseDeadZone(int index)
        {
            return _config.gyroMouseDZ[index];
        }

        public  void SetGyroMouseDeadZone(int index, int value, ControlService control)
        {
            _config.SetGyroMouseDZ(index, value, control);
        }

        public  bool[] GyroMouseToggle => _config.gyroMouseToggle;
        public  void SetGyroMouseToggle(int index, bool value, ControlService control) 
            => _config.SetGyroMouseToggle(index, value, control);

        public  void SetGyroControlsToggle(int index, bool value, ControlService control)
            => _config.SetGyroControlsToggle(index, value, control);

        public  GyroMouseInfo[] GyroMouseInfo => _config.gyroMouseInfo;

        public  GyroControlsInfo[] GyroControlsInf => _config.gyroControlsInf;
        public  GyroControlsInfo GetGyroControlsInfo(int index)
        {
            return _config.gyroControlsInf[index];
        }

        public  SteeringWheelSmoothingInfo[] WheelSmoothInfo => _config.wheelSmoothInfo;
        public  int[] SAWheelFuzzValues => _config.saWheelFuzzValues;

        //public static DS4Color[] MainColor => m_Config.m_Leds;
        public  ref DS4Color getMainColor(int index)
        {
            return ref _config.lightbarSettingInfo[index].ds4winSettings.m_Led;
            //return ref m_Config.m_Leds[index];
        }

        //public static DS4Color[] LowColor => m_Config.m_LowLeds;
        public  ref DS4Color getLowColor(int index)
        {
            return ref _config.lightbarSettingInfo[index].ds4winSettings.m_LowLed;
            //return ref m_Config.m_LowLeds[index];
        }

        //public static DS4Color[] ChargingColor => m_Config.m_ChargingLeds;
        public  ref DS4Color getChargingColor(int index)
        {
            return ref _config.lightbarSettingInfo[index].ds4winSettings.m_ChargingLed;
            //return ref m_Config.m_ChargingLeds[index];
        }

        //public static DS4Color[] CustomColor => m_Config.m_CustomLeds;
        public  ref DS4Color getCustomColor(int index)
        {
            return ref _config.lightbarSettingInfo[index].ds4winSettings.m_CustomLed;
            //return ref m_Config.m_CustomLeds[index];
        }

        //public static bool[] UseCustomLed => m_Config.useCustomLeds;
        public  bool getUseCustomLed(int index)
        {
            return _config.lightbarSettingInfo[index].ds4winSettings.useCustomLed;
            //return m_Config.useCustomLeds[index];
        }

        //public static DS4Color[] FlashColor => m_Config.m_FlashLeds;
        public  ref DS4Color getFlashColor(int index)
        {
            return ref _config.lightbarSettingInfo[index].ds4winSettings.m_FlashLed;
            //return ref m_Config.m_FlashLeds[index];
        }

        public  byte[] TapSensitivity => _config.tapSensitivity;
        public  byte getTapSensitivity(int index)
        {
            return _config.tapSensitivity[index];
        }

        public  bool[] DoubleTap => _config.doubleTap;
        public  bool getDoubleTap(int index)
        {
            return _config.doubleTap[index];
        }

        public  int[] ScrollSensitivity => _config.scrollSensitivity;
        public  int[] getScrollSensitivity()
        {
            return _config.scrollSensitivity;
        }
        public  int getScrollSensitivity(int index)
        {
            return _config.scrollSensitivity[index];
        }

        public  bool[] LowerRCOn => _config.lowerRCOn;
        public  bool[] TouchClickPassthru => _config.touchClickPassthru;
        public  bool[] TouchpadJitterCompensation => _config.touchpadJitterCompensation;
        public  bool getTouchpadJitterCompensation(int index)
        {
            return _config.touchpadJitterCompensation[index];
        }

        public  int[] TouchpadInvert => _config.touchpadInvert;
        public  int getTouchpadInvert(int index)
        {
            return _config.touchpadInvert[index];
        }

        public  TriggerDeadZoneZInfo[] L2ModInfo => _config.l2ModInfo;
        public  TriggerDeadZoneZInfo GetL2ModInfo(int index)
        {
            return _config.l2ModInfo[index];
        }

        //public static byte[] L2Deadzone => m_Config.l2Deadzone;
        public  byte getL2Deadzone(int index)
        {
            return _config.l2ModInfo[index].deadZone;
            //return m_Config.l2Deadzone[index];
        }

        public  TriggerDeadZoneZInfo[] R2ModInfo => _config.r2ModInfo;
        public  TriggerDeadZoneZInfo GetR2ModInfo(int index)
        {
            return _config.r2ModInfo[index];
        }

        //public static byte[] R2Deadzone => m_Config.r2Deadzone;
        public  byte getR2Deadzone(int index)
        {
            return _config.r2ModInfo[index].deadZone;
            //return m_Config.r2Deadzone[index];
        }

        public  double[] SXDeadzone => _config.SXDeadzone;
        public  double getSXDeadzone(int index)
        {
            return _config.SXDeadzone[index];
        }

        public  double[] SZDeadzone => _config.SZDeadzone;
        public  double getSZDeadzone(int index)
        {
            return _config.SZDeadzone[index];
        }

        //public static int[] LSDeadzone => m_Config.LSDeadzone;
        public  int getLSDeadzone(int index)
        {
            return _config.lsModInfo[index].deadZone;
            //return m_Config.LSDeadzone[index];
        }

        //public static int[] RSDeadzone => m_Config.RSDeadzone;
        public  int getRSDeadzone(int index)
        {
            return _config.rsModInfo[index].deadZone;
            //return m_Config.RSDeadzone[index];
        }

        //public static int[] LSAntiDeadzone => m_Config.LSAntiDeadzone;
        public  int getLSAntiDeadzone(int index)
        {
            return _config.lsModInfo[index].antiDeadZone;
            //return m_Config.LSAntiDeadzone[index];
        }

        //public static int[] RSAntiDeadzone => m_Config.RSAntiDeadzone;
        public  int getRSAntiDeadzone(int index)
        {
            return _config.rsModInfo[index].antiDeadZone;
            //return m_Config.RSAntiDeadzone[index];
        }

        public  StickDeadZoneInfo[] LSModInfo => _config.lsModInfo;
        public  StickDeadZoneInfo GetLSDeadInfo(int index)
        {
            return _config.lsModInfo[index];
        }

        public  StickDeadZoneInfo[] RSModInfo => _config.rsModInfo;
        public  StickDeadZoneInfo GetRSDeadInfo(int index)
        {
            return _config.rsModInfo[index];
        }

        public  double[] SXAntiDeadzone => _config.SXAntiDeadzone;
        public  double getSXAntiDeadzone(int index)
        {
            return _config.SXAntiDeadzone[index];
        }

        public  double[] SZAntiDeadzone => _config.SZAntiDeadzone;
        public  double getSZAntiDeadzone(int index)
        {
            return _config.SZAntiDeadzone[index];
        }

        //public static int[] LSMaxzone => m_Config.LSMaxzone;
        public  int getLSMaxzone(int index)
        {
            return _config.lsModInfo[index].maxZone;
            //return m_Config.LSMaxzone[index];
        }

        //public static int[] RSMaxzone => m_Config.RSMaxzone;
        public  int getRSMaxzone(int index)
        {
            return _config.rsModInfo[index].maxZone;
            //return m_Config.RSMaxzone[index];
        }

        public  double[] SXMaxzone => _config.SXMaxzone;
        public  double getSXMaxzone(int index)
        {
            return _config.SXMaxzone[index];
        }

        public  double[] SZMaxzone => _config.SZMaxzone;
        public  double getSZMaxzone(int index)
        {
            return _config.SZMaxzone[index];
        }

        //public static int[] L2AntiDeadzone => m_Config.l2AntiDeadzone;
        public  int getL2AntiDeadzone(int index)
        {
            return _config.l2ModInfo[index].antiDeadZone;
            //return m_Config.l2AntiDeadzone[index];
        }

        //public static int[] R2AntiDeadzone => m_Config.r2AntiDeadzone;
        public  int getR2AntiDeadzone(int index)
        {
            return _config.r2ModInfo[index].antiDeadZone;
            //return m_Config.r2AntiDeadzone[index];
        }

        //public static int[] L2Maxzone => m_Config.l2Maxzone;
        public  int getL2Maxzone(int index)
        {
            return _config.l2ModInfo[index].maxZone;
            //return m_Config.l2Maxzone[index];
        }

        //public static int[] R2Maxzone => m_Config.r2Maxzone;
        public  int getR2Maxzone(int index)
        {
            return _config.r2ModInfo[index].maxZone;
            //return m_Config.r2Maxzone[index];
        }

        public  double[] LSRotation => _config.LSRotation;
        public  double getLSRotation(int index)
        {
            return _config.LSRotation[index];
        }

        public  double[] RSRotation => _config.RSRotation;
        public  double getRSRotation(int index)
        {
            return _config.RSRotation[index];
        }

        public  double[] L2Sens => _config.l2Sens;
        public  double getL2Sens(int index)
        {
            return _config.l2Sens[index];
        }

        public  double[] R2Sens => _config.r2Sens;
        public  double getR2Sens(int index)
        {
            return _config.r2Sens[index];
        }

        public  double[] SXSens => _config.SXSens;
        public  double getSXSens(int index)
        {
            return _config.SXSens[index];
        }

        public  double[] SZSens => _config.SZSens;
        public  double getSZSens(int index)
        {
            return _config.SZSens[index];
        }

        public  double[] LSSens => _config.LSSens;
        public  double getLSSens(int index)
        {
            return _config.LSSens[index];
        }

        public  double[] RSSens => _config.RSSens;
        public  double getRSSens(int index)
        {
            return _config.RSSens[index];
        }

        public  int[] BTPollRate => _config.btPollRate;
        public  int getBTPollRate(int index)
        {
            return _config.btPollRate[index];
        }

        public  SquareStickInfo[] SquStickInfo => _config.squStickInfo;
        public  SquareStickInfo GetSquareStickInfo(int device)
        {
            return _config.squStickInfo[device];
        }

        public  StickAntiSnapbackInfo[] LSAntiSnapbackInfo => _config.lsAntiSnapbackInfo;
        public  StickAntiSnapbackInfo GetLSAntiSnapbackInfo(int device)
        {
            return _config.lsAntiSnapbackInfo[device];
        }

        public  StickAntiSnapbackInfo[] RSAntiSnapbackInfo => _config.rsAntiSnapbackInfo;
        public  StickAntiSnapbackInfo GetRSAntiSnapbackInfo(int device)
        {
            return _config.rsAntiSnapbackInfo[device];
        }

        public  StickOutputSetting[] LSOutputSettings => _config.lsOutputSettings;
        public  StickOutputSetting[] RSOutputSettings => _config.rsOutputSettings;

        public  TriggerOutputSettings[] L2OutputSettings => _config.l2OutputSettings;
        public  TriggerOutputSettings[] R2OutputSettings => _config.r2OutputSettings;

        public  void setLsOutCurveMode(int index, int value)
        {
            _config.setLsOutCurveMode(index, value);
        }
        public  int getLsOutCurveMode(int index)
        {
            return _config.getLsOutCurveMode(index);
        }
        public  BezierCurve[] lsOutBezierCurveObj => _config.lsOutBezierCurveObj;

        public  void setRsOutCurveMode(int index, int value)
        {
            _config.setRsOutCurveMode(index, value);
        }
        public  int getRsOutCurveMode(int index)
        {
            return _config.getRsOutCurveMode(index);
        }
        public  BezierCurve[] rsOutBezierCurveObj => _config.rsOutBezierCurveObj;

        public  void setL2OutCurveMode(int index, int value)
        {
            _config.setL2OutCurveMode(index, value);
        }
        public  int getL2OutCurveMode(int index)
        {
            return _config.getL2OutCurveMode(index);
        }
        public  BezierCurve[] l2OutBezierCurveObj => _config.l2OutBezierCurveObj;

        public  void setR2OutCurveMode(int index, int value)
        {
            _config.setR2OutCurveMode(index, value);
        }
        public  int getR2OutCurveMode(int index)
        {
            return _config.getR2OutCurveMode(index);
        }
        public  BezierCurve[] r2OutBezierCurveObj => _config.r2OutBezierCurveObj;

        public  void setSXOutCurveMode(int index, int value)
        {
            _config.setSXOutCurveMode(index, value);
        }
        public  int getSXOutCurveMode(int index)
        {
            return _config.getSXOutCurveMode(index);
        }
        public  BezierCurve[] sxOutBezierCurveObj => _config.sxOutBezierCurveObj;

        public  void setSZOutCurveMode(int index, int value)
        {
            _config.setSZOutCurveMode(index, value);
        }
        public  int getSZOutCurveMode(int index)
        {
            return _config.getSZOutCurveMode(index);
        }
        public  BezierCurve[] szOutBezierCurveObj => _config.szOutBezierCurveObj;

        public  bool[] TrackballMode => _config.trackballMode;
        public  bool getTrackballMode(int index)
        {
            return _config.trackballMode[index];
        }

        public  double[] TrackballFriction => _config.trackballFriction;
        public  double getTrackballFriction(int index)
        {
            return _config.trackballFriction[index];
        }

        public  TouchpadAbsMouseSettings[] TouchAbsMouse => _config.touchpadAbsMouse;
        public  TouchpadRelMouseSettings[] TouchRelMouse => _config.touchpadRelMouse;

        public  ControlServiceDeviceOptions DeviceOptions => _config.DeviceOptions;

        public  OutContType[] OutContType => _config.OutputDeviceType;
        public  string[] LaunchProgram => _config.launchProgram;
        public  string[] ProfilePath => _config.profilePath;
        public  string[] OlderProfilePath => _config.olderProfilePath;
        public  bool[] DistanceProfiles => _config.distanceProfiles;

        public  List<string>[] ProfileActions => _config.profileActions;
        public  int getProfileActionCount(int index)
        {
            return _config.profileActionCount[index];
        }

        public  void CalculateProfileActionCount(int index)
        {
            _config.CalculateProfileActionCount(index);
        }

        public  List<string> getProfileActions(int index)
        {
            return _config.profileActions[index];
        }
        
        public  void UpdateDS4CSetting (int deviceNum, string buttonName, bool shift, object action, string exts, DS4KeyType kt, int trigger = 0)
        {
            _config.UpdateDs4ControllerSetting(deviceNum, buttonName, shift, action, exts, kt, trigger);
            _config.containsCustomAction[deviceNum] = _config.HasCustomActions(deviceNum);
            _config.containsCustomExtras[deviceNum] = _config.HasCustomExtras(deviceNum);
        }

        public  void UpdateDS4Extra(int deviceNum, string buttonName, bool shift, string exts)
        {
            _config.UpdateDs4ControllerExtra(deviceNum, buttonName, shift, exts);
            _config.containsCustomAction[deviceNum] = _config.HasCustomActions(deviceNum);
            _config.containsCustomExtras[deviceNum] = _config.HasCustomExtras(deviceNum);
        }

        public  ControlActionData GetDS4Action(int deviceNum, string buttonName, bool shift) => _config.GetDs4Action(deviceNum, buttonName, shift);
        public  ControlActionData GetDS4Action(int deviceNum, DS4Controls control, bool shift) => _config.GetDs4Action(deviceNum, control, shift);
        public  DS4KeyType GetDS4KeyType(int deviceNum, string buttonName, bool shift) => _config.GetDs4KeyType(deviceNum, buttonName, shift);
        public  string GetDS4Extra(int deviceNum, string buttonName, bool shift) => _config.GetDs4Extra(deviceNum, buttonName, shift);
        public  int GetDS4STrigger(int deviceNum, string buttonName) => _config.GetDs4STrigger(deviceNum, buttonName);
        public  int GetDS4STrigger(int deviceNum, DS4Controls control) => _config.GetDs4STrigger(deviceNum, control);
        public  List<DS4ControlSettings> getDS4CSettings(int device) => _config.ds4settings[device];
        public  DS4ControlSettings GetDS4CSetting(int deviceNum, string control) => _config.GetDs4ControllerSetting(deviceNum, control);
        public  DS4ControlSettings GetDS4CSetting(int deviceNum, DS4Controls control) => _config.GetDs4ControllerSetting(deviceNum, control);
        public  ControlSettingsGroup GetControlSettingsGroup(int deviceNum) => _config.ds4controlSettings[deviceNum];
        public  bool HasCustomActions(int deviceNum) => _config.HasCustomActions(deviceNum);
        public  bool HasCustomExtras(int deviceNum) => _config.HasCustomExtras(deviceNum);

        public  bool containsCustomAction(int deviceNum)
        {
            return _config.containsCustomAction[deviceNum];
        }

        public  bool containsCustomExtras(int deviceNum)
        {
            return _config.containsCustomExtras[deviceNum];
        }

        public  void SaveAction(string name, string controls, int mode,
            string details, bool edit, string extras = "")
        {
            _config.SaveAction(name, controls, mode, details, edit, extras);
            Mapping.actionDone.Add(new Mapping.ActionState());
        }

        public  void RemoveAction(string name)
        {
            _config.RemoveAction(name);
        }

        public  bool LoadActions() => _config.LoadActions();

        public  List<SpecialAction> GetActions() => _config.actions;

        public  int GetActionIndexOf(string name)
        {
            return _config.GetActionIndexOf(name);
        }

        public  int GetProfileActionIndexOf(int device, string name)
        {
            int index = -1;
            _config.profileActionIndexDict[device].TryGetValue(name, out index);
            return index;
        }

        public  SpecialAction GetAction(string name)
        {
            return _config.GetAction(name);
        }

        public  SpecialAction GetProfileAction(int device, string name)
        {
            SpecialAction sA = null;
            _config.profileActionDict[device].TryGetValue(name, out sA);
            return sA;
        }

        public  void CalculateProfileActionDicts(int device)
        {
            _config.CalculateProfileActionDicts(device);
        }

        public  void CacheProfileCustomsFlags(int device)
        {
            _config.CacheProfileCustomsFlags(device);
        }

        public  void CacheExtraProfileInfo(int device)
        {
            _config.CacheExtraProfileInfo(device);
        }

        public  X360Controls getX360ControlsByName(string key)
        {
            return _config.GetX360ControlsByName(key);
        }

        public  string GetX360ControlString(X360Controls key)
        {
            return _config.GetX360ControlString(key);
        }

        public  DS4Controls getDS4ControlsByName(string key)
        {
            return _config.GetDs4ControlsByName(key);
        }

        public  X360Controls getDefaultX360ControlBinding(DS4Controls dc)
        {
            return DefaultButtonMapping[(int)dc];
        }

        public  bool containsLinkedProfile(string serial)
        {
            string tempSerial = serial.Replace(":", string.Empty);
            return _config.linkedProfiles.ContainsKey(tempSerial);
        }

        public  string getLinkedProfile(string serial)
        {
            string temp = string.Empty;
            string tempSerial = serial.Replace(":", string.Empty);
            if (_config.linkedProfiles.ContainsKey(tempSerial))
            {
                temp = _config.linkedProfiles[tempSerial];
            }

            return temp;
        }

        public  void changeLinkedProfile(string serial, string profile)
        {
            string tempSerial = serial.Replace(":", string.Empty);
            _config.linkedProfiles[tempSerial] = profile;
        }

        public  void removeLinkedProfile(string serial)
        {
            string tempSerial = serial.Replace(":", string.Empty);
            if (_config.linkedProfiles.ContainsKey(tempSerial))
            {
                _config.linkedProfiles.Remove(tempSerial);
            }
        }

        public  bool Load() => _config.Load();
        
        public  bool LoadProfile(int device, bool launchprogram, ControlService control,
            bool xinputChange = true, bool postLoad = true)
        {
            bool result = _config.LoadProfile(device, launchprogram, control, "", xinputChange, postLoad);
            TempProfileNames[device] = string.Empty;
            UseTempProfiles[device] = false;
            TempProfileDistance[device] = false;

            return result;
        }

        public  bool LoadTempProfile(int device, string name, bool launchprogram,
            ControlService control, bool xinputChange = true)
        {
            bool result = _config.LoadProfile(device, launchprogram, control, RuntimeAppDataPath + @"\Profiles\" + name + ".xml");
            TempProfileNames[device] = name;
            UseTempProfiles[device] = true;
            TempProfileDistance[device] = name.ToLower().Contains("distance");

            return result;
        }

        public  void LoadBlankDevProfile(int device, bool launchprogram, ControlService control,
            bool xinputChange = true, bool postLoad = true)
        {
            _config.LoadBlankProfile(device, launchprogram, control, "", xinputChange, postLoad);
            _config.EstablishDefaultSpecialActions(device);
            _config.CacheExtraProfileInfo(device);

            TempProfileNames[device] = string.Empty;
            UseTempProfiles[device] = false;
            TempProfileDistance[device] = false;
        }

        public  void LoadBlankDS4Profile(int device, bool launchprogram, ControlService control,
            bool xinputChange = true, bool postLoad = true)
        {
            _config.LoadBlankDs4Profile(device, launchprogram, control, "", xinputChange, postLoad);
            _config.EstablishDefaultSpecialActions(device);
            _config.CacheExtraProfileInfo(device);

            TempProfileNames[device] = string.Empty;
            UseTempProfiles[device] = false;
            TempProfileDistance[device] = false;
        }

        public  void LoadDefaultGamepadGyroProfile(int device, bool launchprogram, ControlService control,
            bool xinputChange = true, bool postLoad = true)
        {
            _config.LoadDefaultGamepadGyroProfile(device, launchprogram, control, "", xinputChange, postLoad);
            _config.EstablishDefaultSpecialActions(device);
            _config.CacheExtraProfileInfo(device);

            TempProfileNames[device] = string.Empty;
            UseTempProfiles[device] = false;
            TempProfileDistance[device] = false;
        }

        public  void LoadDefaultDS4GamepadGyroProfile(int device, bool launchprogram, ControlService control,
            bool xinputChange = true, bool postLoad = true)
        {
            _config.LoadDefaultDS4GamepadGyroProfile(device, launchprogram, control, "", xinputChange, postLoad);
            _config.EstablishDefaultSpecialActions(device);
            _config.CacheExtraProfileInfo(device);

            TempProfileNames[device] = string.Empty;
            UseTempProfiles[device] = false;
            TempProfileDistance[device] = false;
        }

        public  void LoadDefaultMixedControlsProfile(int device, bool launchprogram, ControlService control,
            bool xinputChange = true, bool postLoad = true)
        {
            _config.LoadDefaultMixedControlsProfile(device, launchprogram, control, "", xinputChange, postLoad);
            _config.EstablishDefaultSpecialActions(device);
            _config.CacheExtraProfileInfo(device);

            TempProfileNames[device] = string.Empty;
            UseTempProfiles[device] = false;
            TempProfileDistance[device] = false;
        }

        public  void LoadDefaultDS4MixedControlsProfile(int device, bool launchprogram, ControlService control,
            bool xinputChange = true, bool postLoad = true)
        {
            _config.LoadDefaultMixedControlsProfile(device, launchprogram, control, "", xinputChange, postLoad);
            _config.EstablishDefaultSpecialActions(device);
            _config.CacheExtraProfileInfo(device);

            TempProfileNames[device] = string.Empty;
            UseTempProfiles[device] = false;
            TempProfileDistance[device] = false;
        }

        public  void LoadDefaultMixedGyroMouseProfile(int device, bool launchprogram, ControlService control,
            bool xinputChange = true, bool postLoad = true)
        {
            _config.LoadDefaultMixedGyroMouseProfile(device, launchprogram, control, "", xinputChange, postLoad);
            _config.EstablishDefaultSpecialActions(device);
            _config.CacheExtraProfileInfo(device);

            TempProfileNames[device] = string.Empty;
            UseTempProfiles[device] = false;
            TempProfileDistance[device] = false;
        }

        public  void LoadDefaultDS4MixedGyroMouseProfile(int device, bool launchprogram, ControlService control,
            bool xinputChange = true, bool postLoad = true)
        {
            _config.LoadDefaultDs4MixedGyroMouseProfile(device, launchprogram, control, "", xinputChange, postLoad);
            _config.EstablishDefaultSpecialActions(device);
            _config.CacheExtraProfileInfo(device);

            TempProfileNames[device] = string.Empty;
            UseTempProfiles[device] = false;
            TempProfileDistance[device] = false;
        }

        public  void LoadDefaultKBMProfile(int device, bool launchprogram, ControlService control,
            bool xinputChange = true, bool postLoad = true)
        {
            _config.LoadDefaultKBMProfile(device, launchprogram, control, "", xinputChange, postLoad);
            _config.EstablishDefaultSpecialActions(device);
            _config.CacheExtraProfileInfo(device);

            TempProfileNames[device] = string.Empty;
            UseTempProfiles[device] = false;
            TempProfileDistance[device] = false;
        }

        public  void LoadDefaultKBMGyroMouseProfile(int device, bool launchprogram, ControlService control,
            bool xinputChange = true, bool postLoad = true)
        {
            _config.LoadDefaultKBMGyroMouseProfile(device, launchprogram, control, "", xinputChange, postLoad);
            _config.EstablishDefaultSpecialActions(device);
            _config.CacheExtraProfileInfo(device);

            TempProfileNames[device] = string.Empty;
            UseTempProfiles[device] = false;
            TempProfileDistance[device] = false;
        }

        public  bool Save()
        {
            return _config.Save();
        }

        public  void SaveProfile(int device, string proName)
        {
            _config.SaveProfile(device, proName);
        }

        public  void SaveAsNewProfile(int device, string propath)
        {
            _config.SaveAsNewProfile(device, propath);
        }

        public  bool SaveLinkedProfiles()
        {
            return _config.SaveLinkedProfiles();
        }

        public  bool LoadLinkedProfiles()
        {
            return _config.LoadLinkedProfiles();
        }

        public  bool SaveControllerConfigs(DS4Device device = null)
        {
            if (device != null)
                return _config.SaveControllerConfigsForDevice(device);

            for (int idx = 0; idx < ControlService.MAX_DS4_CONTROLLER_COUNT; idx++)
                if (Program.rootHub.DS4Controllers[idx] != null)
                    _config.SaveControllerConfigsForDevice(Program.rootHub.DS4Controllers[idx]);

            return true;
        }

        public  bool LoadControllerConfigs(DS4Device device = null)
        {
            if (device != null)
                return _config.LoadControllerConfigsForDevice(device);

            for (int idx = 0; idx < ControlService.MAX_DS4_CONTROLLER_COUNT; idx++)
                if (Program.rootHub.DS4Controllers[idx] != null)
                    _config.LoadControllerConfigsForDevice(Program.rootHub.DS4Controllers[idx]);

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
                setting.ActionData.ActionAlias = 0;
                if (setting.ControlActionType == DS4ControlSettings.ActionType.Key)
                {
                    setting.ActionData.ActionAlias = outputKBMMapping.GetRealEventKey(Convert.ToUInt32(setting.ActionData.ActionKey));
                }
            }
            else
            {
                setting.ShiftAction.ActionAlias = 0;
                if (setting.ShiftActionType == DS4ControlSettings.ActionType.Key)
                {
                    setting.ShiftAction.ActionAlias = outputKBMMapping.GetRealEventKey(Convert.ToUInt32(setting.ShiftAction.ActionKey));
                }
            }
        }

        public  void RefreshExtrasButtons(int deviceNum, List<DS4Controls> devButtons)
        {
            _config.ds4controlSettings[deviceNum].ResetExtraButtons();
            if (devButtons != null)
            {
                _config.ds4controlSettings[deviceNum].EstablishExtraButtons(devButtons);
            }
        }
    }
}
