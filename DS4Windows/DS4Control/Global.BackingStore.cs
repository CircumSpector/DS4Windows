using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Xml;

namespace DS4Windows
{
    public partial class Global
    {
        private class BackingStore : IBackingStore
        {
            public const double DEFAULT_UDP_SMOOTH_MINCUTOFF = 0.4;
            public const double DEFAULT_UDP_SMOOTH_BETA = 0.2;

            public string ProfilesPath { get; set; } =
                Path.Combine(Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName, Constants.ProfilesFileName);

            public string ActionsPath { get; set; } = Path.Combine(Global.RuntimeAppDataPath, Constants.ActionsFileName);

            public string LinkedProfilesPath { get; set; } =
                Path.Combine(Global.RuntimeAppDataPath, Constants.LinkedProfilesFileName);

            public string ControllerConfigsPath { get; set; } =
                Path.Combine(Global.RuntimeAppDataPath, Constants.ControllerConfigsFileName);

            protected XmlDocument m_Xdoc = new();

            // ninth (fifth in old builds) value used for options, not last controller
            public IList<ButtonMouseInfo> ButtonMouseInfos { get; set; } =new List<ButtonMouseInfo>()
            {
            new(), new(), new(),
            new(), new(), new(),
            new(), new(), new()
            };

            public bool[] EnableTouchToggle = new bool[Global.TEST_PROFILE_ITEM_COUNT]
                {true, true, true, true, true, true, true, true, true};

            public int[] IdleDisconnectTimeout = new int[Global.TEST_PROFILE_ITEM_COUNT] { 0, 0, 0, 0, 0, 0, 0, 0, 0 };

            public bool[] EnableOutputDataToDS4 = new bool[Global.TEST_PROFILE_ITEM_COUNT]
                {true, true, true, true, true, true, true, true, true};

            public bool[] TouchpadJitterCompensation = new bool[Global.TEST_PROFILE_ITEM_COUNT]
                {true, true, true, true, true, true, true, true, true};

            public IList<bool> LowerRCOn { get; set; } = new List<bool>()
                {false, false, false, false, false, false, false, false, false};

            public bool[] TouchClickPassthru = new bool[Global.TEST_PROFILE_ITEM_COUNT]
                {false, false, false, false, false, false, false, false, false};

            public IList<string> ProfilePath { get; set; } = new List<string>()
            {
                string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty,
                string.Empty, string.Empty
            };

            public IList<string> OlderProfilePath { get; set; } = new List<string>()
            {
            string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty,
            string.Empty, string.Empty
            };

            public Dictionary<string, string> linkedProfiles = new();

            // Cache properties instead of performing a string comparison every frame
            public IList<bool> DistanceProfiles { get; set; }= new List<bool>
                { false, false, false, false, false, false, false, false, false };

            public IList< byte> RumbleBoost { get; set; }= new List<byte>() { 100, 100, 100, 100, 100, 100, 100, 100, 100 };

            public IList<int>
                RumbleAutostopTime = new List<int>()
                    {0, 0, 0, 0, 0, 0, 0, 0, 0}; // Value in milliseconds (0=autustop timer disabled)

            public IList<byte> TouchSensitivity { get; set; } = new List<byte>()
                {100, 100, 100, 100, 100, 100, 100, 100, 100};

            public IList<StickDeadZoneInfo> LSModInfo { get; set; }= new List<StickDeadZoneInfo>()
            {
            new(), new(),
            new(), new(),
            new(), new(),
            new(), new(),
            new()
            };

            public IList<StickDeadZoneInfo> RSModInfo { get; set; }= new List<StickDeadZoneInfo>()
            {
            new(), new(),
            new(), new(),
            new(), new(),
            new(), new(),
            new()
            };

            public IList<TriggerDeadZoneZInfo> L2ModInfo { get; set; }= new List<TriggerDeadZoneZInfo>()
            {
            new(), new(),
            new(), new(),
            new(), new(),
            new(), new(),
            new()
            };

            public IList<TriggerDeadZoneZInfo> R2ModInfo{ get; set; } = new List<TriggerDeadZoneZInfo>()
            {
            new(), new(),
            new(), new(),
            new(), new(),
            new(), new(),
            new()
            };

            public IList<double> LSRotation { get; set; } =
                new List<double> { 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0 };

            public IList<double> RSRotation { get; set; } =
                new List<double> { 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0 };

            public IList<double> SXDeadzone { get; set; } = new List<double>
                { 0.25, 0.25, 0.25, 0.25, 0.25, 0.25, 0.25, 0.25, 0.25 };

            public IList<double> SZDeadzone { get; set; } = new List<double>
                { 0.25, 0.25, 0.25, 0.25, 0.25, 0.25, 0.25, 0.25, 0.25 };

            public IList<double> SXMaxzone { get; set; } =
                new List<double> { 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0 };

            public IList<double> SZMaxzone { get; set; } =
                new List<double> { 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0 };

            public IList<double> SXAntiDeadzone { get; set; } =
                new List<double> { 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0 };

            public IList<double> SZAntiDeadzone { get; set; } =
                new List<double> { 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0 };

            public IList<double> L2Sens { get; set; } =
                new List<double> { 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0 };

            public IList<double> R2Sens { get; set; } =
                new List<double> { 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0 };

            public IList<double> LSSens { get; set; } =
                new List<double> { 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0 };

            public IList<double> RSSens { get; set; } =
                new List<double> { 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0 };

            public IList<double> SXSens { get; set; } =
                new List<double> { 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0 };

            public IList<double> SZSens { get; set; } =
                new List<double> { 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0 };

            public byte[] tapSensitivity = new byte[Global.TEST_PROFILE_ITEM_COUNT] { 0, 0, 0, 0, 0, 0, 0, 0, 0 };

            public bool[] doubleTap = new bool[Global.TEST_PROFILE_ITEM_COUNT]
                {false, false, false, false, false, false, false, false, false};

            public int[] scrollSensitivity = new int[Global.TEST_PROFILE_ITEM_COUNT] { 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            public int[] touchpadInvert = new int[Global.TEST_PROFILE_ITEM_COUNT] { 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            public int[] btPollRate = new int[Global.TEST_PROFILE_ITEM_COUNT] { 4, 4, 4, 4, 4, 4, 4, 4, 4 };

            public int[] gyroMouseDZ = new int[Global.TEST_PROFILE_ITEM_COUNT]
            {
            MouseCursor.GYRO_MOUSE_DEADZONE, MouseCursor.GYRO_MOUSE_DEADZONE,
            MouseCursor.GYRO_MOUSE_DEADZONE, MouseCursor.GYRO_MOUSE_DEADZONE,
            MouseCursor.GYRO_MOUSE_DEADZONE, MouseCursor.GYRO_MOUSE_DEADZONE,
            MouseCursor.GYRO_MOUSE_DEADZONE, MouseCursor.GYRO_MOUSE_DEADZONE,
            MouseCursor.GYRO_MOUSE_DEADZONE
            };

            public bool[] gyroMouseToggle = new bool[Global.TEST_PROFILE_ITEM_COUNT]
            {
            false, false, false,
            false, false, false, false, false, false
            };

            public IList<SquareStickInfo> SquStickInfo { get; set; } = new List<SquareStickInfo>
            {
                new(), new(),
                new(), new(),
                new(), new(),
                new(), new(),
                new()
            };

            public IList<StickAntiSnapbackInfo> LSAntiSnapbackInfo { get; set; } = new List<StickAntiSnapbackInfo>
            {
                new(), new(),
                new(), new(),
                new(), new(),
                new(), new(),
                new()
            };

            public IList<StickAntiSnapbackInfo> RSAntiSnapbackInfo { get; set; } = new List<StickAntiSnapbackInfo>
            {
                new(), new(),
                new(), new(),
                new(), new(),
                new(), new(),
                new()
            };

            public IList<StickOutputSetting> LSOutputSettings { get; set; } = new List<StickOutputSetting>
            {
                new(), new(), new(),
                new(), new(), new(),
                new(), new(), new()
            };

            public IList<StickOutputSetting> RSOutputSettings { get; set; } = new List<StickOutputSetting>
            {
                new(), new(), new(),
                new(), new(), new(),
                new(), new(), new()
            };

            public IList<TriggerOutputSettings> L2OutputSettings { get; set; } = new List<TriggerOutputSettings>
            {
                new(), new(), new(),
                new(), new(), new(),
                new(), new(), new()
            };

            public IList<TriggerOutputSettings> R2OutputSettings { get; set; } = new List<TriggerOutputSettings>
            {
                new(), new(), new(),
                new(), new(), new(),
                new(), new(), new()
            };

            public IList<SteeringWheelSmoothingInfo> WheelSmoothInfo { get; set; } =
                new List<SteeringWheelSmoothingInfo>
                {
                    new(), new(),
                    new(), new(),
                    new(), new(),
                    new(), new(),
                    new()
                };

            public int[] saWheelFuzzValues = new int[Global.TEST_PROFILE_ITEM_COUNT];

            private void setOutBezierCurveObjArrayItem(BezierCurve[] bezierCurveArray, int device, int curveOptionValue, BezierCurve.AxisType axisType)
            {
                // Set bezier curve obj of axis. 0=Linear (no curve mapping), 1-5=Pre-defined curves, 6=User supplied custom curve string value of a profile (comma separated list of 4 decimal numbers)
                switch (curveOptionValue)
                {
                    // Commented out case 1..5 because Mapping.cs:SetCurveAndDeadzone function has the original IF-THEN-ELSE code logic for those original 1..5 output curve mappings (ie. no need to initialize the lookup result table).
                    // Only the new bezier custom curve option 6 uses the lookup result table (initialized in BezierCurve class based on an input curve definition).
                    //case 1: bezierCurveArray[device].InitBezierCurve(99.0, 91.0, 0.00, 0.00, axisType); break;  // Enhanced Precision (hard-coded curve) (almost the same curve as bezier 0.70, 0.28, 1.00, 1.00)
                    //case 2: bezierCurveArray[device].InitBezierCurve(99.0, 92.0, 0.00, 0.00, axisType); break;  // Quadric
                    //case 3: bezierCurveArray[device].InitBezierCurve(99.0, 93.0, 0.00, 0.00, axisType); break;  // Cubic
                    //case 4: bezierCurveArray[device].InitBezierCurve(99.0, 94.0, 0.00, 0.00, axisType); break;  // Easeout Quad
                    //case 5: bezierCurveArray[device].InitBezierCurve(99.0, 95.0, 0.00, 0.00, axisType); break;  // Easeout Cubic
                    case 6: bezierCurveArray[device].InitBezierCurve(bezierCurveArray[device].CustomDefinition, axisType); break;  // Custom output curve
                }
            }

            public BezierCurve[] LSOutBezierCurveObj = new BezierCurve[Global.TEST_PROFILE_ITEM_COUNT]
                {new(), new(), new(), new(), new(), new(), new(), new(), new()};

            public BezierCurve[] RSOutBezierCurveObj = new BezierCurve[Global.TEST_PROFILE_ITEM_COUNT]
                {new(), new(), new(), new(), new(), new(), new(), new(), new()};

            public BezierCurve[] L2OutBezierCurveObj = new BezierCurve[Global.TEST_PROFILE_ITEM_COUNT]
                {new(), new(), new(), new(), new(), new(), new(), new(), new()};

            public BezierCurve[] R2OutBezierCurveObj = new BezierCurve[Global.TEST_PROFILE_ITEM_COUNT]
                {new(), new(), new(), new(), new(), new(), new(), new(), new()};

            public BezierCurve[] SXOutBezierCurveObj = new BezierCurve[Global.TEST_PROFILE_ITEM_COUNT]
                {new(), new(), new(), new(), new(), new(), new(), new(), new()};

            public BezierCurve[] SZOutBezierCurveObj = new BezierCurve[Global.TEST_PROFILE_ITEM_COUNT]
                {new(), new(), new(), new(), new(), new(), new(), new(), new()};

            private int[] _lsOutCurveMode = new int[Global.TEST_PROFILE_ITEM_COUNT] { 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            public int getLsOutCurveMode(int index) { return _lsOutCurveMode[index]; }
            public void setLsOutCurveMode(int index, int value)
            {
                if (value >= 1) setOutBezierCurveObjArrayItem(LSOutBezierCurveObj, index, value, BezierCurve.AxisType.LSRS);
                _lsOutCurveMode[index] = value;
            }

            private int[] _rsOutCurveMode = new int[Global.TEST_PROFILE_ITEM_COUNT] { 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            public int getRsOutCurveMode(int index) { return _rsOutCurveMode[index]; }
            public void setRsOutCurveMode(int index, int value)
            {
                if (value >= 1) setOutBezierCurveObjArrayItem(RSOutBezierCurveObj, index, value, BezierCurve.AxisType.LSRS);
                _rsOutCurveMode[index] = value;
            }

            private int[] _l2OutCurveMode = new int[Global.TEST_PROFILE_ITEM_COUNT] { 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            public int getL2OutCurveMode(int index) { return _l2OutCurveMode[index]; }
            public void setL2OutCurveMode(int index, int value)
            {
                if (value >= 1) setOutBezierCurveObjArrayItem(L2OutBezierCurveObj, index, value, BezierCurve.AxisType.L2R2);
                _l2OutCurveMode[index] = value;
            }

            private int[] _r2OutCurveMode = new int[Global.TEST_PROFILE_ITEM_COUNT] { 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            public int getR2OutCurveMode(int index) { return _r2OutCurveMode[index]; }
            public void setR2OutCurveMode(int index, int value)
            {
                if (value >= 1) setOutBezierCurveObjArrayItem(R2OutBezierCurveObj, index, value, BezierCurve.AxisType.L2R2);
                _r2OutCurveMode[index] = value;
            }

            private int[] _sxOutCurveMode = new int[Global.TEST_PROFILE_ITEM_COUNT] { 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            public int getSXOutCurveMode(int index) { return _sxOutCurveMode[index]; }
            public void setSXOutCurveMode(int index, int value)
            {
                if (value >= 1) setOutBezierCurveObjArrayItem(SXOutBezierCurveObj, index, value, BezierCurve.AxisType.SA);
                _sxOutCurveMode[index] = value;
            }

            private int[] _szOutCurveMode = new int[Global.TEST_PROFILE_ITEM_COUNT] { 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            public int getSZOutCurveMode(int index) { return _szOutCurveMode[index]; }
            public void setSZOutCurveMode(int index, int value)
            {
                if (value >= 1) setOutBezierCurveObjArrayItem(SZOutBezierCurveObj, index, value, BezierCurve.AxisType.SA);
                _szOutCurveMode[index] = value;
            }

            public LightbarSettingInfo[] lightbarSettingInfo = new LightbarSettingInfo[Global.TEST_PROFILE_ITEM_COUNT]
            {
            new(), new(),
            new(), new(),
            new(), new(),
            new(), new(),
            new()
            };

            public IList<string> LaunchProgram { get; set; } = new List<string>()
            {
                string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty,
                string.Empty, string.Empty
            };

            public bool[] dinputOnly = new bool[Global.TEST_PROFILE_ITEM_COUNT]
                {false, false, false, false, false, false, false, false, false};

            public bool[] startTouchpadOff = new bool[Global.TEST_PROFILE_ITEM_COUNT]
                {false, false, false, false, false, false, false, false, false};

            public TouchpadOutMode[] touchOutMode = new TouchpadOutMode[Global.TEST_PROFILE_ITEM_COUNT]
            {
            TouchpadOutMode.Mouse, TouchpadOutMode.Mouse, TouchpadOutMode.Mouse, TouchpadOutMode.Mouse,
            TouchpadOutMode.Mouse, TouchpadOutMode.Mouse, TouchpadOutMode.Mouse, TouchpadOutMode.Mouse,
            TouchpadOutMode.Mouse
            };

            public GyroOutMode[] gyroOutMode = new GyroOutMode[Global.TEST_PROFILE_ITEM_COUNT]
            {
            GyroOutMode.Controls, GyroOutMode.Controls,
            GyroOutMode.Controls, GyroOutMode.Controls, GyroOutMode.Controls, GyroOutMode.Controls,
            GyroOutMode.Controls, GyroOutMode.Controls, GyroOutMode.Controls
            };

            public GyroControlsInfo[] gyroControlsInf = new GyroControlsInfo[Global.TEST_PROFILE_ITEM_COUNT]
            {
            new(), new(), new(),
            new(), new(), new(),
            new(), new(), new()
            };

            public string[] sATriggers = new string[Global.TEST_PROFILE_ITEM_COUNT]
                {"-1", "-1", "-1", "-1", "-1", "-1", "-1", "-1", "-1"};

            public string[] sAMouseStickTriggers = new string[Global.TEST_PROFILE_ITEM_COUNT]
                {"-1", "-1", "-1", "-1", "-1", "-1", "-1", "-1", "-1"};

            public bool[] sATriggerCond = new bool[Global.TEST_PROFILE_ITEM_COUNT]
                {true, true, true, true, true, true, true, true, true};

            public bool[] sAMouseStickTriggerCond = new bool[Global.TEST_PROFILE_ITEM_COUNT]
                {true, true, true, true, true, true, true, true, true};

            public bool[] gyroMouseStickTriggerTurns = new bool[Global.TEST_PROFILE_ITEM_COUNT]
                {true, true, true, true, true, true, true, true, true};

            public GyroMouseStickInfo[] gyroMStickInfo = new GyroMouseStickInfo[Global.TEST_PROFILE_ITEM_COUNT]
            {
            new(),
            new(),
            new(), new(),
            new(), new(),
            new(), new(),
            new()
            };

            public GyroDirectionalSwipeInfo[] gyroSwipeInfo = new GyroDirectionalSwipeInfo[Global.TEST_PROFILE_ITEM_COUNT]
            {
            new(), new(),
            new(), new(),
            new(), new(),
            new(), new(),
            new()
            };

            public bool[] gyroMouseStickToggle = new bool[Global.TEST_PROFILE_ITEM_COUNT]
            {
            false, false, false,
            false, false, false, false, false, false
            };

            public SASteeringWheelEmulationAxisType[] sASteeringWheelEmulationAxis =
                new SASteeringWheelEmulationAxisType[Global.TEST_PROFILE_ITEM_COUNT]
                {
                SASteeringWheelEmulationAxisType.None, SASteeringWheelEmulationAxisType.None,
                SASteeringWheelEmulationAxisType.None, SASteeringWheelEmulationAxisType.None,
                SASteeringWheelEmulationAxisType.None, SASteeringWheelEmulationAxisType.None,
                SASteeringWheelEmulationAxisType.None, SASteeringWheelEmulationAxisType.None,
                SASteeringWheelEmulationAxisType.None
                };

            public int[] sASteeringWheelEmulationRange = new int[Global.TEST_PROFILE_ITEM_COUNT]
                {360, 360, 360, 360, 360, 360, 360, 360, 360};

            public int[][] touchDisInvertTriggers = new int[Global.TEST_PROFILE_ITEM_COUNT][]
            {
            new int[1] {-1}, new int[1] {-1}, new int[1] {-1},
            new int[1] {-1}, new int[1] {-1}, new int[1] {-1}, new int[1] {-1}, new int[1] {-1}, new int[1] {-1}
            };

            public bool useExclusiveMode; // Re-enable Ex Mode
            public int FormWidth { get; set; } = 782;

            public int FormHeight { get; set; } = 550;

            public int FormLocationX { get; set; }

            public int FormLocationY { get; set; }

            public bool StartMinimized { get; set; }

            public bool MinToTaskBar { get; set; }
            public DateTime LastChecked { get; set; }
            public string lastVersionChecked = string.Empty;
            public ulong LastVersionCheckedNumber;
            public int CheckWhen { get; set; } = 24;
            public int Notifications { get; set; } = 2;
            public bool DisconnectBluetoothAtStop { get; set; }
            public bool swipeProfiles = true;
            public bool Ds4Mapping { get; set; }
            public bool QuickCharge { get; set; }

            public bool CloseMini { get; set; }

            public IList<SpecialAction> Actions { get; set; } = new List<SpecialAction>();

            public IList<DS4ControlSettings>[] Ds4Settings { get; set; } = new List<DS4ControlSettings>[Global.TEST_PROFILE_ITEM_COUNT]
            {
            new(), new(), new(),
            new(), new(), new(), new(), new(), new()
            };

            public ControlSettingsGroup[] ds4controlSettings;

            public IList<string>[] ProfileActions { get; set; } = { null, null, null, null, null, null, null, null, null };

            public int[] profileActionCount = new int[Global.TEST_PROFILE_ITEM_COUNT] { 0, 0, 0, 0, 0, 0, 0, 0, 0 };

            public Dictionary<string, SpecialAction>[] profileActionDict =
                new Dictionary<string, SpecialAction>[Global.TEST_PROFILE_ITEM_COUNT]
                {
                new(), new(), new(),
                new(), new(), new(), new(), new(), new()
                };

            public Dictionary<string, int>[] profileActionIndexDict =
                new Dictionary<string, int>[Global.TEST_PROFILE_ITEM_COUNT]
                {
                new(), new(), new(),
                new(), new(), new(), new(), new(), new()
                };

            public string UseLang { get; set; } = string.Empty;

            public bool DownloadLang { get; set; } = true;
            public TrayIconChoice UseIconChoice { get; set; }
            public bool FlashWhenLate { get; set; } = true;
            public int flashWhenLateAt = 50;
            public bool UseUdpServer;
            public int UdpServerPort { get; set; } = 26760;

            /// <summary>
            ///     127.0.0.1=IPAddress.Loopback (default), 0.0.0.0=IPAddress.Any as all interfaces, x.x.x.x = Specific ipv4 interface
            ///     address or hostname
            /// </summary>
            public string UdpServerListenAddress { get; set; } = "127.0.0.1";

            public bool UseUdpSmoothing { get; set; }
            public double udpSmoothingMincutoff = DEFAULT_UDP_SMOOTH_MINCUTOFF;
            public double udpSmoothingBeta = DEFAULT_UDP_SMOOTH_BETA;
            
            public bool UseCustomSteamFolder { get; set; }
            
            public string CustomSteamFolder { get; set; }

            public AppThemeChoice ThemeChoice { get; set; }
            public string fakeExeFileName = string.Empty;

            public ControlServiceDeviceOptions DeviceOptions => new();

            // Cache whether profile has custom action
            public bool[] containsCustomAction = new bool[Global.TEST_PROFILE_ITEM_COUNT]
                {false, false, false, false, false, false, false, false, false};

            // Cache whether profile has custom extras
            public bool[] containsCustomExtras = new bool[Global.TEST_PROFILE_ITEM_COUNT]
                {false, false, false, false, false, false, false, false, false};

            public int[] gyroSensitivity = new int[Global.TEST_PROFILE_ITEM_COUNT]
                {100, 100, 100, 100, 100, 100, 100, 100, 100};

            public int[] gyroSensVerticalScale = new int[Global.TEST_PROFILE_ITEM_COUNT]
                {100, 100, 100, 100, 100, 100, 100, 100, 100};

            public int[] gyroInvert = new int[Global.TEST_PROFILE_ITEM_COUNT] { 0, 0, 0, 0, 0, 0, 0, 0, 0 };

            public bool[] gyroTriggerTurns = new bool[Global.TEST_PROFILE_ITEM_COUNT]
                {true, true, true, true, true, true, true, true, true};

            public GyroMouseInfo[] gyroMouseInfo = new GyroMouseInfo[Global.TEST_PROFILE_ITEM_COUNT]
            {
            new(), new(),
            new(), new(),
            new(), new(),
            new(), new(),
            new()
            };

            public int[] gyroMouseHorizontalAxis = new int[Global.TEST_PROFILE_ITEM_COUNT] { 0, 0, 0, 0, 0, 0, 0, 0, 0 };

            public int[] gyroMouseStickHorizontalAxis = new int[Global.TEST_PROFILE_ITEM_COUNT] { 0, 0, 0, 0, 0, 0, 0, 0, 0 };

            public bool[] trackballMode = new bool[Global.TEST_PROFILE_ITEM_COUNT]
                {false, false, false, false, false, false, false, false, false};

            public double[] TrackballFriction = new double[Global.TEST_PROFILE_ITEM_COUNT]
                {10.0, 10.0, 10.0, 10.0, 10.0, 10.0, 10.0, 10.0, 10.0};


            public TouchpadAbsMouseSettings[] TouchpadAbsMouse =
                new TouchpadAbsMouseSettings[Global.TEST_PROFILE_ITEM_COUNT]
                {
                new(), new(), new(),
                new(), new(), new(), new(), new(), new()
                };

            public TouchpadRelMouseSettings[] TouchpadRelMouse =
                new TouchpadRelMouseSettings[Global.TEST_PROFILE_ITEM_COUNT]
                {
                new(), new(), new(), new(),
                new(), new(), new(), new(), new()
                };

            // Used to hold the controller type desired in a profile
            public List<OutContType> OutputDeviceType { get; set; } = new()
            {
                DS4Windows.OutContType.X360,
                DS4Windows.OutContType.X360,
                DS4Windows.OutContType.X360,
                DS4Windows.OutContType.X360,
                DS4Windows.OutContType.X360,
                DS4Windows.OutContType.X360,
                DS4Windows.OutContType.X360,
                DS4Windows.OutContType.X360,
                DS4Windows.OutContType.X360
            };

            /// <summary>
            ///     TRUE=AutoProfile reverts to default profile if current foreground process is unknown, FALSE=Leave existing profile
            ///     active when a foreground process is unknown (ie. no matching auto-profile rule)
            /// </summary>
            public bool AutoProfileRevertDefaultProfile = true;

            private bool tempBool;

            public BackingStore()
            {
                ds4controlSettings = new ControlSettingsGroup[Global.TEST_PROFILE_ITEM_COUNT];

                for (int i = 0; i < Global.TEST_PROFILE_ITEM_COUNT; i++)
                {
                    foreach (DS4Controls dc in Enum.GetValues(typeof(DS4Controls)))
                    {
                        if (dc != DS4Controls.None)
                            Ds4Settings[i].Add(new DS4ControlSettings(dc));
                    }

                    ds4controlSettings[i] = new ControlSettingsGroup(Ds4Settings[i]);

                    EstablishDefaultSpecialActions(i);
                    CacheExtraProfileInfo(i);
                }

                SetupDefaultColors();
            }

            public void EstablishDefaultSpecialActions(int idx)
            {
                ProfileActions[idx] = new List<string> { "Disconnect Controller" };
                profileActionCount[idx] = ProfileActions[idx].Count;
            }

            public void CacheProfileCustomsFlags(int device)
            {
                bool customAct = false;
                containsCustomAction[device] = customAct = HasCustomActions(device);
                containsCustomExtras[device] = HasCustomExtras(device);

                if (!customAct)
                {
                    customAct = gyroOutMode[device] == GyroOutMode.MouseJoystick;
                    customAct = customAct || sASteeringWheelEmulationAxis[device] >= SASteeringWheelEmulationAxisType.VJoy1X;
                    customAct = customAct || LSOutputSettings[device].mode != StickMode.Controls;
                    customAct = customAct || RSOutputSettings[device].mode != StickMode.Controls;
                    containsCustomAction[device] = customAct;
                }
            }

            private void SetupDefaultColors()
            {
                lightbarSettingInfo[0].ds4winSettings.m_Led = new DS4Color(Color.Blue);
                lightbarSettingInfo[1].ds4winSettings.m_Led = new DS4Color(Color.Red);
                lightbarSettingInfo[2].ds4winSettings.m_Led = new DS4Color(Color.Green);
                lightbarSettingInfo[3].ds4winSettings.m_Led = new DS4Color(Color.Pink);
                lightbarSettingInfo[4].ds4winSettings.m_Led = new DS4Color(Color.Blue);
                lightbarSettingInfo[5].ds4winSettings.m_Led = new DS4Color(Color.Red);
                lightbarSettingInfo[6].ds4winSettings.m_Led = new DS4Color(Color.Green);
                lightbarSettingInfo[7].ds4winSettings.m_Led = new DS4Color(Color.Pink);
                lightbarSettingInfo[8].ds4winSettings.m_Led = new DS4Color(Color.White);
            }

            public void CacheExtraProfileInfo(int device)
            {
                CalculateProfileActionCount(device);
                CalculateProfileActionDicts(device);
                CacheProfileCustomsFlags(device);
            }

            public void CalculateProfileActionCount(int index)
            {
                profileActionCount[index] = ProfileActions[index].Count;
            }

            public void CalculateProfileActionDicts(int device)
            {
                profileActionDict[device].Clear();
                profileActionIndexDict[device].Clear();

                foreach (string actionname in ProfileActions[device])
                {
                    profileActionDict[device][actionname] = GetAction(actionname);
                    profileActionIndexDict[device][actionname] = GetActionIndexOf(actionname);
                }
            }

            public SpecialAction GetAction(string name)
            {
                //foreach (SpecialAction sA in actions)
                for (int i = 0, actionCount = Actions.Count; i < actionCount; i++)
                {
                    SpecialAction sA = Actions[i];
                    if (sA.Name == name)
                        return sA;
                }

                return new SpecialAction("null", "null", "null", "null");
            }

            public int GetActionIndexOf(string name)
            {
                for (int i = 0, actionCount = Actions.Count; i < actionCount; i++)
                {
                    if (Actions[i].Name == name)
                        return i;
                }

                return -1;
            }

            private string stickOutputCurveString(int id)
            {
                string result = "linear";
                switch (id)
                {
                    case 0: break;
                    case 1: result = "enhanced-precision"; break;
                    case 2: result = "quadratic"; break;
                    case 3: result = "cubic"; break;
                    case 4: result = "easeout-quad"; break;
                    case 5: result = "easeout-cubic"; break;
                    case 6: result = "custom"; break;
                    default: break;
                }

                return result;
            }

            private int stickOutputCurveId(string name)
            {
                int id = 0;
                switch (name)
                {
                    case "linear": id = 0; break;
                    case "enhanced-precision": id = 1; break;
                    case "quadratic": id = 2; break;
                    case "cubic": id = 3; break;
                    case "easeout-quad": id = 4; break;
                    case "easeout-cubic": id = 5; break;
                    case "custom": id = 6; break;
                    default: break;
                }

                return id;
            }

            private string axisOutputCurveString(int id)
            {
                return stickOutputCurveString(id);
            }

            private int axisOutputCurveId(string name)
            {
                return stickOutputCurveId(name);
            }

            private bool SaTriggerCondValue(string text)
            {
                bool result = true;
                switch (text)
                {
                    case "and": result = true; break;
                    case "or": result = false; break;
                    default: result = true; break;
                }

                return result;
            }

            private string SaTriggerCondString(bool value)
            {
                string result = value ? "and" : "or";
                return result;
            }

            public void SetSaTriggerCond(int index, string text)
            {
                sATriggerCond[index] = SaTriggerCondValue(text);
            }

            public void SetSaMouseStickTriggerCond(int index, string text)
            {
                sAMouseStickTriggerCond[index] = SaTriggerCondValue(text);
            }

            public void SetGyroMouseDZ(int index, int value, ControlService control)
            {
                gyroMouseDZ[index] = value;
                if (index < ControlService.CURRENT_DS4_CONTROLLER_LIMIT && control.touchPad[index] != null)
                    control.touchPad[index].CursorGyroDead = value;
            }

            public void SetGyroControlsToggle(int index, bool value, ControlService control)
            {
                gyroControlsInf[index].triggerToggle = value;
                if (index < ControlService.CURRENT_DS4_CONTROLLER_LIMIT && control.touchPad[index] != null)
                    control.touchPad[index].ToggleGyroControls = value;
            }

            public void SetGyroMouseToggle(int index, bool value, ControlService control)
            {
                gyroMouseToggle[index] = value;
                if (index < ControlService.CURRENT_DS4_CONTROLLER_LIMIT && control.touchPad[index] != null)
                    control.touchPad[index].ToggleGyroMouse = value;
            }

            public void SetGyroMouseStickToggle(int index, bool value, ControlService control)
            {
                gyroMouseStickToggle[index] = value;
                if (index < ControlService.CURRENT_DS4_CONTROLLER_LIMIT && control.touchPad[index] != null)
                    control.touchPad[index].ToggleGyroStick = value;
            }

            private string OutContDeviceString(OutContType id)
            {
                string result = "X360";
                switch (id)
                {
                    case DS4Windows.OutContType.None:
                    case DS4Windows.OutContType.X360: result = "X360"; break;
                    case DS4Windows.OutContType.DS4: result = "DS4"; break;
                    default: break;
                }

                return result;
            }

            private OutContType OutContDeviceId(string name)
            {
                OutContType id = DS4Windows.OutContType.X360;
                switch (name)
                {
                    case "None":
                    case "X360": id = DS4Windows.OutContType.X360; break;
                    case "DS4": id = DS4Windows.OutContType.DS4; break;
                    default: break;
                }

                return id;
            }

            private void PortOldGyroSettings(int device)
            {
                if (gyroOutMode[device] == GyroOutMode.None)
                {
                    gyroOutMode[device] = GyroOutMode.Controls;
                }
            }

            private string GetGyroOutModeString(GyroOutMode mode)
            {
                string result = "None";
                switch (mode)
                {
                    case GyroOutMode.Controls:
                        result = "Controls";
                        break;
                    case GyroOutMode.Mouse:
                        result = "Mouse";
                        break;
                    case GyroOutMode.MouseJoystick:
                        result = "MouseJoystick";
                        break;
                    case GyroOutMode.Passthru:
                        result = "Passthru";
                        break;
                    default:
                        break;
                }

                return result;
            }

            private GyroOutMode GetGyroOutModeType(string modeString)
            {
                GyroOutMode result = GyroOutMode.None;
                switch (modeString)
                {
                    case "Controls":
                        result = GyroOutMode.Controls;
                        break;
                    case "Mouse":
                        result = GyroOutMode.Mouse;
                        break;
                    case "MouseJoystick":
                        result = GyroOutMode.MouseJoystick;
                        break;
                    case "Passthru":
                        result = GyroOutMode.Passthru;
                        break;
                    default:
                        break;
                }

                return result;
            }

            private string GetLightbarModeString(LightbarMode mode)
            {
                string result = "DS4Win";
                switch (mode)
                {
                    case LightbarMode.DS4Win:
                        result = "DS4Win";
                        break;
                    case LightbarMode.Passthru:
                        result = "Passthru";
                        break;
                    default:
                        break;
                }
                return result;
            }

            private LightbarMode GetLightbarModeType(string modeString)
            {
                LightbarMode result = LightbarMode.DS4Win;
                switch (modeString)
                {
                    case "DS4Win":
                        result = LightbarMode.DS4Win;
                        break;
                    case "Passthru":
                        result = LightbarMode.Passthru;
                        break;
                    default:
                        break;
                }

                return result;
            }

            public bool SaveAsNewProfile(int device, string proName)
            {
                bool Saved = true;
                ResetProfile(device);
                Saved = SaveProfile(device, proName);
                return Saved;
            }

            public bool SaveProfile(int device, string proName)
            {
                var Saved = true;
                //string path = Global.RuntimeAppDataPath + @"\Profiles\" + Path.GetFileNameWithoutExtension(proName) + ".xml";
                if (proName.EndsWith(XML_EXTENSION)) proName = proName.Remove(proName.LastIndexOf(XML_EXTENSION));

                var path = Path.Combine(
                    RuntimeAppDataPath,
                    Constants.ProfilesSubDirectory,
                    $"{proName}{XML_EXTENSION}"
                );

                try
                {
                    XmlNode tmpNode;
                    XmlNode xmlControls = m_Xdoc.SelectSingleNode("/DS4Windows/Control");
                    XmlNode xmlShiftControls = m_Xdoc.SelectSingleNode("/DS4Windows/ShiftControl");
                    m_Xdoc.RemoveAll();

                    tmpNode = m_Xdoc.CreateXmlDeclaration("1.0", "utf-8", string.Empty);
                    m_Xdoc.AppendChild(tmpNode);

                    tmpNode = m_Xdoc.CreateComment(string.Format(" DS4Windows Configuration Data. {0} ", DateTime.Now));
                    m_Xdoc.AppendChild(tmpNode);

                    tmpNode = m_Xdoc.CreateComment(string.Format(" Made with DS4Windows version {0} ", Global.ExecutableProductVersion));
                    m_Xdoc.AppendChild(tmpNode);

                    tmpNode = m_Xdoc.CreateWhitespace("\r\n");
                    m_Xdoc.AppendChild(tmpNode);

                    XmlElement rootElement = m_Xdoc.CreateElement("DS4Windows", null);
                    rootElement.SetAttribute("app_version", Global.ExecutableProductVersion);
                    rootElement.SetAttribute("config_version", Global.CONFIG_VERSION.ToString());

                    LightbarSettingInfo lightbarSettings = lightbarSettingInfo[device];
                    LightbarDS4WinInfo lightInfo = lightbarSettings.ds4winSettings;

                    XmlNode xmlTouchToggle = m_Xdoc.CreateNode(XmlNodeType.Element, "touchToggle", null); xmlTouchToggle.InnerText = EnableTouchToggle[device].ToString(); rootElement.AppendChild(xmlTouchToggle);
                    XmlNode xmlIdleDisconnectTimeout = m_Xdoc.CreateNode(XmlNodeType.Element, "idleDisconnectTimeout", null); xmlIdleDisconnectTimeout.InnerText = IdleDisconnectTimeout[device].ToString(); rootElement.AppendChild(xmlIdleDisconnectTimeout);
                    XmlNode xmlOutputDataToDS4 = m_Xdoc.CreateNode(XmlNodeType.Element, "outputDataToDS4", null); xmlOutputDataToDS4.InnerText = EnableOutputDataToDS4[device].ToString(); rootElement.AppendChild(xmlOutputDataToDS4);
                    XmlNode xmlColor = m_Xdoc.CreateNode(XmlNodeType.Element, "Color", null);
                    xmlColor.InnerText = lightInfo.m_Led.red.ToString() + "," + lightInfo.m_Led.green.ToString() + "," + lightInfo.m_Led.blue.ToString();
                    rootElement.AppendChild(xmlColor);
                    XmlNode xmlRumbleBoost = m_Xdoc.CreateNode(XmlNodeType.Element, "RumbleBoost", null); xmlRumbleBoost.InnerText = RumbleBoost[device].ToString(); rootElement.AppendChild(xmlRumbleBoost);
                    XmlNode xmlRumbleAutostopTime = m_Xdoc.CreateNode(XmlNodeType.Element, "RumbleAutostopTime", null); xmlRumbleAutostopTime.InnerText = RumbleAutostopTime[device].ToString(); rootElement.AppendChild(xmlRumbleAutostopTime);
                    XmlNode xmlLightbarMode = m_Xdoc.CreateNode(XmlNodeType.Element, "LightbarMode", null); xmlLightbarMode.InnerText = GetLightbarModeString(lightbarSettings.mode); rootElement.AppendChild(xmlLightbarMode);
                    XmlNode xmlLedAsBatteryIndicator = m_Xdoc.CreateNode(XmlNodeType.Element, "ledAsBatteryIndicator", null); xmlLedAsBatteryIndicator.InnerText = lightInfo.ledAsBattery.ToString(); rootElement.AppendChild(xmlLedAsBatteryIndicator);
                    XmlNode xmlLowBatteryFlash = m_Xdoc.CreateNode(XmlNodeType.Element, "FlashType", null); xmlLowBatteryFlash.InnerText = lightInfo.flashType.ToString(); rootElement.AppendChild(xmlLowBatteryFlash);
                    XmlNode xmlFlashBatterAt = m_Xdoc.CreateNode(XmlNodeType.Element, "flashBatteryAt", null); xmlFlashBatterAt.InnerText = lightInfo.flashAt.ToString(); rootElement.AppendChild(xmlFlashBatterAt);
                    XmlNode xmlTouchSensitivity = m_Xdoc.CreateNode(XmlNodeType.Element, "touchSensitivity", null); xmlTouchSensitivity.InnerText = TouchSensitivity[device].ToString(); rootElement.AppendChild(xmlTouchSensitivity);
                    XmlNode xmlLowColor = m_Xdoc.CreateNode(XmlNodeType.Element, "LowColor", null);
                    xmlLowColor.InnerText = lightInfo.m_LowLed.red.ToString() + "," + lightInfo.m_LowLed.green.ToString() + "," + lightInfo.m_LowLed.blue.ToString();
                    rootElement.AppendChild(xmlLowColor);
                    XmlNode xmlChargingColor = m_Xdoc.CreateNode(XmlNodeType.Element, "ChargingColor", null);
                    xmlChargingColor.InnerText = lightInfo.m_ChargingLed.red.ToString() + "," + lightInfo.m_ChargingLed.green.ToString() + "," + lightInfo.m_ChargingLed.blue.ToString();
                    rootElement.AppendChild(xmlChargingColor);
                    XmlNode xmlFlashColor = m_Xdoc.CreateNode(XmlNodeType.Element, "FlashColor", null);
                    xmlFlashColor.InnerText = lightInfo.m_FlashLed.red.ToString() + "," + lightInfo.m_FlashLed.green.ToString() + "," + lightInfo.m_FlashLed.blue.ToString();
                    rootElement.AppendChild(xmlFlashColor);
                    XmlNode xmlTouchpadJitterCompensation = m_Xdoc.CreateNode(XmlNodeType.Element, "touchpadJitterCompensation", null); xmlTouchpadJitterCompensation.InnerText = TouchpadJitterCompensation[device].ToString(); rootElement.AppendChild(xmlTouchpadJitterCompensation);
                    XmlNode xmlLowerRCOn = m_Xdoc.CreateNode(XmlNodeType.Element, "lowerRCOn", null); xmlLowerRCOn.InnerText = LowerRCOn[device].ToString(); rootElement.AppendChild(xmlLowerRCOn);
                    XmlNode xmlTapSensitivity = m_Xdoc.CreateNode(XmlNodeType.Element, "tapSensitivity", null); xmlTapSensitivity.InnerText = tapSensitivity[device].ToString(); rootElement.AppendChild(xmlTapSensitivity);
                    XmlNode xmlDouble = m_Xdoc.CreateNode(XmlNodeType.Element, "doubleTap", null); xmlDouble.InnerText = doubleTap[device].ToString(); rootElement.AppendChild(xmlDouble);
                    XmlNode xmlScrollSensitivity = m_Xdoc.CreateNode(XmlNodeType.Element, "scrollSensitivity", null); xmlScrollSensitivity.InnerText = scrollSensitivity[device].ToString(); rootElement.AppendChild(xmlScrollSensitivity);
                    XmlNode xmlLeftTriggerMiddle = m_Xdoc.CreateNode(XmlNodeType.Element, "LeftTriggerMiddle", null); xmlLeftTriggerMiddle.InnerText = L2ModInfo[device].deadZone.ToString(); rootElement.AppendChild(xmlLeftTriggerMiddle);
                    XmlNode xmlRightTriggerMiddle = m_Xdoc.CreateNode(XmlNodeType.Element, "RightTriggerMiddle", null); xmlRightTriggerMiddle.InnerText = R2ModInfo[device].deadZone.ToString(); rootElement.AppendChild(xmlRightTriggerMiddle);
                    XmlNode xmlTouchpadInvert = m_Xdoc.CreateNode(XmlNodeType.Element, "TouchpadInvert", null); xmlTouchpadInvert.InnerText = touchpadInvert[device].ToString(); rootElement.AppendChild(xmlTouchpadInvert);
                    XmlNode xmlTouchClickPasthru = m_Xdoc.CreateNode(XmlNodeType.Element, "TouchpadClickPassthru", null); xmlTouchClickPasthru.InnerText = TouchClickPassthru[device].ToString(); rootElement.AppendChild(xmlTouchClickPasthru);

                    XmlNode xmlL2AD = m_Xdoc.CreateNode(XmlNodeType.Element, "L2AntiDeadZone", null); xmlL2AD.InnerText = L2ModInfo[device].antiDeadZone.ToString(); rootElement.AppendChild(xmlL2AD);
                    XmlNode xmlR2AD = m_Xdoc.CreateNode(XmlNodeType.Element, "R2AntiDeadZone", null); xmlR2AD.InnerText = R2ModInfo[device].antiDeadZone.ToString(); rootElement.AppendChild(xmlR2AD);
                    XmlNode xmlL2Maxzone = m_Xdoc.CreateNode(XmlNodeType.Element, "L2MaxZone", null); xmlL2Maxzone.InnerText = L2ModInfo[device].maxZone.ToString(); rootElement.AppendChild(xmlL2Maxzone);
                    XmlNode xmlR2Maxzone = m_Xdoc.CreateNode(XmlNodeType.Element, "R2MaxZone", null); xmlR2Maxzone.InnerText = R2ModInfo[device].maxZone.ToString(); rootElement.AppendChild(xmlR2Maxzone);
                    XmlNode xmlL2MaxOutput = m_Xdoc.CreateNode(XmlNodeType.Element, "L2MaxOutput", null); xmlL2MaxOutput.InnerText = L2ModInfo[device].maxOutput.ToString(); rootElement.AppendChild(xmlL2MaxOutput);
                    XmlNode xmlR2MaxOutput = m_Xdoc.CreateNode(XmlNodeType.Element, "R2MaxOutput", null); xmlR2MaxOutput.InnerText = R2ModInfo[device].maxOutput.ToString(); rootElement.AppendChild(xmlR2MaxOutput);
                    XmlNode xmlButtonMouseSensitivity = m_Xdoc.CreateNode(XmlNodeType.Element, "ButtonMouseSensitivity", null); xmlButtonMouseSensitivity.InnerText = ButtonMouseInfos[device].buttonSensitivity.ToString(); rootElement.AppendChild(xmlButtonMouseSensitivity);
                    XmlNode xmlButtonMouseOffset = m_Xdoc.CreateNode(XmlNodeType.Element, "ButtonMouseOffset", null); xmlButtonMouseOffset.InnerText = ButtonMouseInfos[device].mouseVelocityOffset.ToString(); rootElement.AppendChild(xmlButtonMouseOffset);
                    XmlNode xmlRainbow = m_Xdoc.CreateNode(XmlNodeType.Element, "Rainbow", null); xmlRainbow.InnerText = lightInfo.rainbow.ToString(); rootElement.AppendChild(xmlRainbow);
                    XmlNode xmlMaxSatRainbow = m_Xdoc.CreateNode(XmlNodeType.Element, "MaxSatRainbow", null); xmlMaxSatRainbow.InnerText = Convert.ToInt32(lightInfo.maxRainbowSat * 100.0).ToString(); rootElement.AppendChild(xmlMaxSatRainbow);
                    XmlNode xmlLSD = m_Xdoc.CreateNode(XmlNodeType.Element, "LSDeadZone", null); xmlLSD.InnerText = LSModInfo[device].deadZone.ToString(); rootElement.AppendChild(xmlLSD);
                    XmlNode xmlRSD = m_Xdoc.CreateNode(XmlNodeType.Element, "RSDeadZone", null); xmlRSD.InnerText = RSModInfo[device].deadZone.ToString(); rootElement.AppendChild(xmlRSD);
                    XmlNode xmlLSAD = m_Xdoc.CreateNode(XmlNodeType.Element, "LSAntiDeadZone", null); xmlLSAD.InnerText = LSModInfo[device].antiDeadZone.ToString(); rootElement.AppendChild(xmlLSAD);
                    XmlNode xmlRSAD = m_Xdoc.CreateNode(XmlNodeType.Element, "RSAntiDeadZone", null); xmlRSAD.InnerText = RSModInfo[device].antiDeadZone.ToString(); rootElement.AppendChild(xmlRSAD);
                    XmlNode xmlLSMaxZone = m_Xdoc.CreateNode(XmlNodeType.Element, "LSMaxZone", null); xmlLSMaxZone.InnerText = LSModInfo[device].maxZone.ToString(); rootElement.AppendChild(xmlLSMaxZone);
                    XmlNode xmlRSMaxZone = m_Xdoc.CreateNode(XmlNodeType.Element, "RSMaxZone", null); xmlRSMaxZone.InnerText = RSModInfo[device].maxZone.ToString(); rootElement.AppendChild(xmlRSMaxZone);
                    XmlNode xmlLSVerticalScale = m_Xdoc.CreateNode(XmlNodeType.Element, "LSVerticalScale", null); xmlLSVerticalScale.InnerText = LSModInfo[device].verticalScale.ToString(); rootElement.AppendChild(xmlLSVerticalScale);
                    XmlNode xmlRSVerticalScale = m_Xdoc.CreateNode(XmlNodeType.Element, "RSVerticalScale", null); xmlRSVerticalScale.InnerText = RSModInfo[device].verticalScale.ToString(); rootElement.AppendChild(xmlRSVerticalScale);
                    XmlNode xmlLSMaxOutput = m_Xdoc.CreateNode(XmlNodeType.Element, "LSMaxOutput", null); xmlLSMaxOutput.InnerText = LSModInfo[device].maxOutput.ToString(); rootElement.AppendChild(xmlLSMaxOutput);
                    XmlNode xmlRSMaxOutput = m_Xdoc.CreateNode(XmlNodeType.Element, "RSMaxOutput", null); xmlRSMaxOutput.InnerText = RSModInfo[device].maxOutput.ToString(); rootElement.AppendChild(xmlRSMaxOutput);
                    XmlNode xmlLSMaxOutputForce = m_Xdoc.CreateNode(XmlNodeType.Element, "LSMaxOutputForce", null); xmlLSMaxOutputForce.InnerText = LSModInfo[device].maxOutputForce.ToString(); rootElement.AppendChild(xmlLSMaxOutputForce);
                    XmlNode xmlRSMaxOutputForce = m_Xdoc.CreateNode(XmlNodeType.Element, "RSMaxOutputForce", null); xmlRSMaxOutputForce.InnerText = RSModInfo[device].maxOutputForce.ToString(); rootElement.AppendChild(xmlRSMaxOutputForce);
                    XmlNode xmlLSDeadZoneType = m_Xdoc.CreateNode(XmlNodeType.Element, "LSDeadZoneType", null); xmlLSDeadZoneType.InnerText = LSModInfo[device].deadzoneType.ToString(); rootElement.AppendChild(xmlLSDeadZoneType);
                    XmlNode xmlRSDeadZoneType = m_Xdoc.CreateNode(XmlNodeType.Element, "RSDeadZoneType", null); xmlRSDeadZoneType.InnerText = RSModInfo[device].deadzoneType.ToString(); rootElement.AppendChild(xmlRSDeadZoneType);

                    XmlElement xmlLSAxialDeadGroupEl = m_Xdoc.CreateElement("LSAxialDeadOptions");
                    XmlElement xmlLSAxialDeadX = m_Xdoc.CreateElement("DeadZoneX"); xmlLSAxialDeadX.InnerText = LSModInfo[device].xAxisDeadInfo.deadZone.ToString(); xmlLSAxialDeadGroupEl.AppendChild(xmlLSAxialDeadX);
                    XmlElement xmlLSAxialDeadY = m_Xdoc.CreateElement("DeadZoneY"); xmlLSAxialDeadY.InnerText = LSModInfo[device].yAxisDeadInfo.deadZone.ToString(); xmlLSAxialDeadGroupEl.AppendChild(xmlLSAxialDeadY);
                    XmlElement xmlLSAxialMaxX = m_Xdoc.CreateElement("MaxZoneX"); xmlLSAxialMaxX.InnerText = LSModInfo[device].xAxisDeadInfo.maxZone.ToString(); xmlLSAxialDeadGroupEl.AppendChild(xmlLSAxialMaxX);
                    XmlElement xmlLSAxialMaxY = m_Xdoc.CreateElement("MaxZoneY"); xmlLSAxialMaxY.InnerText = LSModInfo[device].yAxisDeadInfo.maxZone.ToString(); xmlLSAxialDeadGroupEl.AppendChild(xmlLSAxialMaxY);
                    XmlElement xmlLSAxialAntiDeadX = m_Xdoc.CreateElement("AntiDeadZoneX"); xmlLSAxialAntiDeadX.InnerText = LSModInfo[device].xAxisDeadInfo.antiDeadZone.ToString(); xmlLSAxialDeadGroupEl.AppendChild(xmlLSAxialAntiDeadX);
                    XmlElement xmlLSAxialAntiDeadY = m_Xdoc.CreateElement("AntiDeadZoneY"); xmlLSAxialAntiDeadY.InnerText = LSModInfo[device].yAxisDeadInfo.antiDeadZone.ToString(); xmlLSAxialDeadGroupEl.AppendChild(xmlLSAxialAntiDeadY);
                    XmlElement xmlLSAxialMaxOutputX = m_Xdoc.CreateElement("MaxOutputX"); xmlLSAxialMaxOutputX.InnerText = LSModInfo[device].xAxisDeadInfo.maxOutput.ToString(); xmlLSAxialDeadGroupEl.AppendChild(xmlLSAxialMaxOutputX);
                    XmlElement xmlLSAxialMaxOutputY = m_Xdoc.CreateElement("MaxOutputY"); xmlLSAxialMaxOutputY.InnerText = LSModInfo[device].yAxisDeadInfo.maxOutput.ToString(); xmlLSAxialDeadGroupEl.AppendChild(xmlLSAxialMaxOutputY);
                    rootElement.AppendChild(xmlLSAxialDeadGroupEl);

                    XmlElement xmlRSAxialDeadGroupEl = m_Xdoc.CreateElement("RSAxialDeadOptions");
                    XmlElement xmlRSAxialDeadX = m_Xdoc.CreateElement("DeadZoneX"); xmlRSAxialDeadX.InnerText = RSModInfo[device].xAxisDeadInfo.deadZone.ToString(); xmlRSAxialDeadGroupEl.AppendChild(xmlRSAxialDeadX);
                    XmlElement xmlRSAxialDeadY = m_Xdoc.CreateElement("DeadZoneY"); xmlRSAxialDeadY.InnerText = RSModInfo[device].yAxisDeadInfo.deadZone.ToString(); xmlRSAxialDeadGroupEl.AppendChild(xmlRSAxialDeadY);
                    XmlElement xmlRSAxialMaxX = m_Xdoc.CreateElement("MaxZoneX"); xmlRSAxialMaxX.InnerText = RSModInfo[device].xAxisDeadInfo.maxZone.ToString(); xmlRSAxialDeadGroupEl.AppendChild(xmlRSAxialMaxX);
                    XmlElement xmlRSAxialMaxY = m_Xdoc.CreateElement("MaxZoneY"); xmlRSAxialMaxY.InnerText = RSModInfo[device].yAxisDeadInfo.maxZone.ToString(); xmlRSAxialDeadGroupEl.AppendChild(xmlRSAxialMaxY);
                    XmlElement xmlRSAxialAntiDeadX = m_Xdoc.CreateElement("AntiDeadZoneX"); xmlRSAxialAntiDeadX.InnerText = RSModInfo[device].xAxisDeadInfo.antiDeadZone.ToString(); xmlRSAxialDeadGroupEl.AppendChild(xmlRSAxialAntiDeadX);
                    XmlElement xmlRSAxialAntiDeadY = m_Xdoc.CreateElement("AntiDeadZoneY"); xmlRSAxialAntiDeadY.InnerText = RSModInfo[device].yAxisDeadInfo.antiDeadZone.ToString(); xmlRSAxialDeadGroupEl.AppendChild(xmlRSAxialAntiDeadY);
                    XmlElement xmlRSAxialMaxOutputX = m_Xdoc.CreateElement("MaxOutputX"); xmlRSAxialMaxOutputX.InnerText = RSModInfo[device].xAxisDeadInfo.maxOutput.ToString(); xmlRSAxialDeadGroupEl.AppendChild(xmlRSAxialMaxOutputX);
                    XmlElement xmlRSAxialMaxOutputY = m_Xdoc.CreateElement("MaxOutputY"); xmlRSAxialMaxOutputY.InnerText = RSModInfo[device].yAxisDeadInfo.maxOutput.ToString(); xmlRSAxialDeadGroupEl.AppendChild(xmlRSAxialMaxOutputY);
                    rootElement.AppendChild(xmlRSAxialDeadGroupEl);

                    XmlNode xmlLSRotation = m_Xdoc.CreateNode(XmlNodeType.Element, "LSRotation", null); xmlLSRotation.InnerText = Convert.ToInt32(LSRotation[device] * 180.0 / Math.PI).ToString(); rootElement.AppendChild(xmlLSRotation);
                    XmlNode xmlRSRotation = m_Xdoc.CreateNode(XmlNodeType.Element, "RSRotation", null); xmlRSRotation.InnerText = Convert.ToInt32(RSRotation[device] * 180.0 / Math.PI).ToString(); rootElement.AppendChild(xmlRSRotation);
                    XmlNode xmlLSFuzz = m_Xdoc.CreateNode(XmlNodeType.Element, "LSFuzz", null); xmlLSFuzz.InnerText = LSModInfo[device].fuzz.ToString(); rootElement.AppendChild(xmlLSFuzz);
                    XmlNode xmlRSFuzz = m_Xdoc.CreateNode(XmlNodeType.Element, "RSFuzz", null); xmlRSFuzz.InnerText = RSModInfo[device].fuzz.ToString(); rootElement.AppendChild(xmlRSFuzz);
                    XmlNode xmlLSOuterBindDead = m_Xdoc.CreateNode(XmlNodeType.Element, "LSOuterBindDead", null); xmlLSOuterBindDead.InnerText = Convert.ToInt32(LSModInfo[device].outerBindDeadZone).ToString(); rootElement.AppendChild(xmlLSOuterBindDead);
                    XmlNode xmlRSOuterBindDead = m_Xdoc.CreateNode(XmlNodeType.Element, "RSOuterBindDead", null); xmlRSOuterBindDead.InnerText = Convert.ToInt32(RSModInfo[device].outerBindDeadZone).ToString(); rootElement.AppendChild(xmlRSOuterBindDead);
                    XmlNode xmlLSOuterBindInvert = m_Xdoc.CreateNode(XmlNodeType.Element, "LSOuterBindInvert", null); xmlLSOuterBindInvert.InnerText = LSModInfo[device].outerBindInvert.ToString(); rootElement.AppendChild(xmlLSOuterBindInvert);
                    XmlNode xmlRSOuterBindInvert = m_Xdoc.CreateNode(XmlNodeType.Element, "RSOuterBindInvert", null); xmlRSOuterBindInvert.InnerText = RSModInfo[device].outerBindInvert.ToString(); rootElement.AppendChild(xmlRSOuterBindInvert);

                    XmlNode xmlSXD = m_Xdoc.CreateNode(XmlNodeType.Element, "SXDeadZone", null); xmlSXD.InnerText = SXDeadzone[device].ToString(); rootElement.AppendChild(xmlSXD);
                    XmlNode xmlSZD = m_Xdoc.CreateNode(XmlNodeType.Element, "SZDeadZone", null); xmlSZD.InnerText = SZDeadzone[device].ToString(); rootElement.AppendChild(xmlSZD);

                    XmlNode xmlSXMaxzone = m_Xdoc.CreateNode(XmlNodeType.Element, "SXMaxZone", null); xmlSXMaxzone.InnerText = Convert.ToInt32(SXMaxzone[device] * 100.0).ToString(); rootElement.AppendChild(xmlSXMaxzone);
                    XmlNode xmlSZMaxzone = m_Xdoc.CreateNode(XmlNodeType.Element, "SZMaxZone", null); xmlSZMaxzone.InnerText = Convert.ToInt32(SZMaxzone[device] * 100.0).ToString(); rootElement.AppendChild(xmlSZMaxzone);

                    XmlNode xmlSXAntiDeadzone = m_Xdoc.CreateNode(XmlNodeType.Element, "SXAntiDeadZone", null); xmlSXAntiDeadzone.InnerText = Convert.ToInt32(SXAntiDeadzone[device] * 100.0).ToString(); rootElement.AppendChild(xmlSXAntiDeadzone);
                    XmlNode xmlSZAntiDeadzone = m_Xdoc.CreateNode(XmlNodeType.Element, "SZAntiDeadZone", null); xmlSZAntiDeadzone.InnerText = Convert.ToInt32(SZAntiDeadzone[device] * 100.0).ToString(); rootElement.AppendChild(xmlSZAntiDeadzone);

                    XmlNode xmlSens = m_Xdoc.CreateNode(XmlNodeType.Element, "Sensitivity", null);
                    xmlSens.InnerText = $"{LSSens[device]}|{RSSens[device]}|{L2Sens[device]}|{R2Sens[device]}|{SXSens[device]}|{SZSens[device]}";
                    rootElement.AppendChild(xmlSens);

                    XmlNode xmlChargingType = m_Xdoc.CreateNode(XmlNodeType.Element, "ChargingType", null); xmlChargingType.InnerText = lightInfo.chargingType.ToString(); rootElement.AppendChild(xmlChargingType);
                    XmlNode xmlMouseAccel = m_Xdoc.CreateNode(XmlNodeType.Element, "MouseAcceleration", null); xmlMouseAccel.InnerText = ButtonMouseInfos[device].mouseAccel.ToString(); rootElement.AppendChild(xmlMouseAccel);
                    XmlNode xmlMouseVerticalScale = m_Xdoc.CreateNode(XmlNodeType.Element, "ButtonMouseVerticalScale", null); xmlMouseVerticalScale.InnerText = Convert.ToInt32(ButtonMouseInfos[device].buttonVerticalScale * 100).ToString(); rootElement.AppendChild(xmlMouseVerticalScale);
                    //XmlNode xmlShiftMod = m_Xdoc.CreateNode(XmlNodeType.Element, "ShiftModifier", null); xmlShiftMod.InnerText = shiftModifier[device].ToString(); rootElement.AppendChild(xmlShiftMod);
                    XmlNode xmlLaunchProgram = m_Xdoc.CreateNode(XmlNodeType.Element, "LaunchProgram", null); xmlLaunchProgram.InnerText = LaunchProgram[device].ToString(); rootElement.AppendChild(xmlLaunchProgram);
                    XmlNode xmlDinput = m_Xdoc.CreateNode(XmlNodeType.Element, "DinputOnly", null); xmlDinput.InnerText = dinputOnly[device].ToString(); rootElement.AppendChild(xmlDinput);
                    XmlNode xmlStartTouchpadOff = m_Xdoc.CreateNode(XmlNodeType.Element, "StartTouchpadOff", null); xmlStartTouchpadOff.InnerText = startTouchpadOff[device].ToString(); rootElement.AppendChild(xmlStartTouchpadOff);
                    XmlNode xmlTouchOutMode = m_Xdoc.CreateNode(XmlNodeType.Element, "TouchpadOutputMode", null); xmlTouchOutMode.InnerText = touchOutMode[device].ToString(); rootElement.AppendChild(xmlTouchOutMode);
                    XmlNode xmlSATriggers = m_Xdoc.CreateNode(XmlNodeType.Element, "SATriggers", null); xmlSATriggers.InnerText = sATriggers[device].ToString(); rootElement.AppendChild(xmlSATriggers);
                    XmlNode xmlSATriggerCond = m_Xdoc.CreateNode(XmlNodeType.Element, "SATriggerCond", null); xmlSATriggerCond.InnerText = SaTriggerCondString(sATriggerCond[device]); rootElement.AppendChild(xmlSATriggerCond);
                    XmlNode xmlSASteeringWheelEmulationAxis = m_Xdoc.CreateNode(XmlNodeType.Element, "SASteeringWheelEmulationAxis", null); xmlSASteeringWheelEmulationAxis.InnerText = sASteeringWheelEmulationAxis[device].ToString("G"); rootElement.AppendChild(xmlSASteeringWheelEmulationAxis);
                    XmlNode xmlSASteeringWheelEmulationRange = m_Xdoc.CreateNode(XmlNodeType.Element, "SASteeringWheelEmulationRange", null); xmlSASteeringWheelEmulationRange.InnerText = sASteeringWheelEmulationRange[device].ToString(); rootElement.AppendChild(xmlSASteeringWheelEmulationRange);
                    XmlNode xmlSASteeringWheelFuzz = m_Xdoc.CreateNode(XmlNodeType.Element, "SASteeringWheelFuzz", null); xmlSASteeringWheelFuzz.InnerText = saWheelFuzzValues[device].ToString(); rootElement.AppendChild(xmlSASteeringWheelFuzz);

                    XmlElement xmlSASteeringWheelSmoothingGroupEl = m_Xdoc.CreateElement("SASteeringWheelSmoothingOptions");
                    XmlElement xmlSASteeringWheelUseSmoothing = m_Xdoc.CreateElement("SASteeringWheelUseSmoothing"); xmlSASteeringWheelUseSmoothing.InnerText = WheelSmoothInfo[device].Enabled.ToString(); xmlSASteeringWheelSmoothingGroupEl.AppendChild(xmlSASteeringWheelUseSmoothing);
                    XmlElement xmlSASteeringWheelSmoothMinCutoff = m_Xdoc.CreateElement("SASteeringWheelSmoothMinCutoff"); xmlSASteeringWheelSmoothMinCutoff.InnerText = WheelSmoothInfo[device].MinCutoff.ToString(); xmlSASteeringWheelSmoothingGroupEl.AppendChild(xmlSASteeringWheelSmoothMinCutoff);
                    XmlElement xmlSASteeringWheelSmoothBeta = m_Xdoc.CreateElement("SASteeringWheelSmoothBeta"); xmlSASteeringWheelSmoothBeta.InnerText = WheelSmoothInfo[device].Beta.ToString(); xmlSASteeringWheelSmoothingGroupEl.AppendChild(xmlSASteeringWheelSmoothBeta);
                    rootElement.AppendChild(xmlSASteeringWheelSmoothingGroupEl);

                    //XmlNode xmlSASteeringWheelUseSmoothing = m_Xdoc.CreateNode(XmlNodeType.Element, "SASteeringWheelUseSmoothing", null); xmlSASteeringWheelUseSmoothing.InnerText = wheelSmoothInfo[device].Enabled.ToString(); rootElement.AppendChild(xmlSASteeringWheelUseSmoothing);
                    //XmlNode xmlSASteeringWheelSmoothMinCutoff = m_Xdoc.CreateNode(XmlNodeType.Element, "SASteeringWheelSmoothMinCutoff", null); xmlSASteeringWheelSmoothMinCutoff.InnerText = wheelSmoothInfo[device].MinCutoff.ToString(); rootElement.AppendChild(xmlSASteeringWheelSmoothMinCutoff);
                    //XmlNode xmlSASteeringWheelSmoothBeta = m_Xdoc.CreateNode(XmlNodeType.Element, "SASteeringWheelSmoothBeta", null); xmlSASteeringWheelSmoothBeta.InnerText = wheelSmoothInfo[device].Beta.ToString(); rootElement.AppendChild(xmlSASteeringWheelSmoothBeta);

                    XmlNode xmlTouchDisInvTriggers = m_Xdoc.CreateNode(XmlNodeType.Element, "TouchDisInvTriggers", null);
                    string tempTouchDisInv = string.Join(",", touchDisInvertTriggers[device]);
                    xmlTouchDisInvTriggers.InnerText = tempTouchDisInv;
                    rootElement.AppendChild(xmlTouchDisInvTriggers);

                    XmlNode xmlGyroSensitivity = m_Xdoc.CreateNode(XmlNodeType.Element, "GyroSensitivity", null); xmlGyroSensitivity.InnerText = gyroSensitivity[device].ToString(); rootElement.AppendChild(xmlGyroSensitivity);
                    XmlNode xmlGyroSensVerticalScale = m_Xdoc.CreateNode(XmlNodeType.Element, "GyroSensVerticalScale", null); xmlGyroSensVerticalScale.InnerText = gyroSensVerticalScale[device].ToString(); rootElement.AppendChild(xmlGyroSensVerticalScale);
                    XmlNode xmlGyroInvert = m_Xdoc.CreateNode(XmlNodeType.Element, "GyroInvert", null); xmlGyroInvert.InnerText = gyroInvert[device].ToString(); rootElement.AppendChild(xmlGyroInvert);
                    XmlNode xmlGyroTriggerTurns = m_Xdoc.CreateNode(XmlNodeType.Element, "GyroTriggerTurns", null); xmlGyroTriggerTurns.InnerText = gyroTriggerTurns[device].ToString(); rootElement.AppendChild(xmlGyroTriggerTurns);
                    /*XmlNode xmlGyroSmoothWeight = m_Xdoc.CreateNode(XmlNodeType.Element, "GyroSmoothingWeight", null); xmlGyroSmoothWeight.InnerText = Convert.ToInt32(gyroSmoothWeight[device] * 100).ToString(); rootElement.AppendChild(xmlGyroSmoothWeight);
                    XmlNode xmlGyroSmoothing = m_Xdoc.CreateNode(XmlNodeType.Element, "GyroSmoothing", null); xmlGyroSmoothing.InnerText = gyroSmoothing[device].ToString(); rootElement.AppendChild(xmlGyroSmoothing);
                    */

                    XmlElement xmlGyroControlsSettingsElement = m_Xdoc.CreateElement("GyroControlsSettings");
                    XmlNode xmlGyroControlsTriggers = m_Xdoc.CreateNode(XmlNodeType.Element, "Triggers", null); xmlGyroControlsTriggers.InnerText = gyroControlsInf[device].triggers; xmlGyroControlsSettingsElement.AppendChild(xmlGyroControlsTriggers);
                    XmlNode xmlGyroControlsTriggerCond = m_Xdoc.CreateNode(XmlNodeType.Element, "TriggerCond", null); xmlGyroControlsTriggerCond.InnerText = SaTriggerCondString(gyroControlsInf[device].triggerCond); xmlGyroControlsSettingsElement.AppendChild(xmlGyroControlsTriggerCond);
                    XmlNode xmlGyroControlsTriggerTurns = m_Xdoc.CreateNode(XmlNodeType.Element, "TriggerTurns", null); xmlGyroControlsTriggerTurns.InnerText = gyroControlsInf[device].triggerTurns.ToString(); xmlGyroControlsSettingsElement.AppendChild(xmlGyroControlsTriggerTurns);
                    XmlNode xmlGyroControlsToggle = m_Xdoc.CreateNode(XmlNodeType.Element, "Toggle", null); xmlGyroControlsToggle.InnerText = gyroControlsInf[device].triggerToggle.ToString(); xmlGyroControlsSettingsElement.AppendChild(xmlGyroControlsToggle);
                    rootElement.AppendChild(xmlGyroControlsSettingsElement);

                    XmlElement xmlGyroSmoothingElement = m_Xdoc.CreateElement("GyroMouseSmoothingSettings");
                    XmlNode xmlGyroSmoothing = m_Xdoc.CreateNode(XmlNodeType.Element, "UseSmoothing", null); xmlGyroSmoothing.InnerText = gyroMouseInfo[device].enableSmoothing.ToString(); xmlGyroSmoothingElement.AppendChild(xmlGyroSmoothing);
                    XmlNode xmlGyroSmoothingMethod = m_Xdoc.CreateNode(XmlNodeType.Element, "SmoothingMethod", null); xmlGyroSmoothingMethod.InnerText = gyroMouseInfo[device].SmoothMethodIdentifier(); xmlGyroSmoothingElement.AppendChild(xmlGyroSmoothingMethod);
                    XmlNode xmlGyroSmoothWeight = m_Xdoc.CreateNode(XmlNodeType.Element, "SmoothingWeight", null); xmlGyroSmoothWeight.InnerText = Convert.ToInt32(gyroMouseInfo[device].smoothingWeight * 100).ToString(); xmlGyroSmoothingElement.AppendChild(xmlGyroSmoothWeight);
                    XmlNode xmlGyroSmoothMincutoff = m_Xdoc.CreateNode(XmlNodeType.Element, "SmoothingMinCutoff", null); xmlGyroSmoothMincutoff.InnerText = gyroMouseInfo[device].minCutoff.ToString(); xmlGyroSmoothingElement.AppendChild(xmlGyroSmoothMincutoff);
                    XmlNode xmlGyroSmoothBeta = m_Xdoc.CreateNode(XmlNodeType.Element, "SmoothingBeta", null); xmlGyroSmoothBeta.InnerText = gyroMouseInfo[device].beta.ToString(); xmlGyroSmoothingElement.AppendChild(xmlGyroSmoothBeta);
                    rootElement.AppendChild(xmlGyroSmoothingElement);

                    XmlNode xmlGyroMouseHAxis = m_Xdoc.CreateNode(XmlNodeType.Element, "GyroMouseHAxis", null); xmlGyroMouseHAxis.InnerText = gyroMouseHorizontalAxis[device].ToString(); rootElement.AppendChild(xmlGyroMouseHAxis);
                    XmlNode xmlGyroMouseDZ = m_Xdoc.CreateNode(XmlNodeType.Element, "GyroMouseDeadZone", null); xmlGyroMouseDZ.InnerText = gyroMouseDZ[device].ToString(); rootElement.AppendChild(xmlGyroMouseDZ);
                    XmlNode xmlGyroMinThreshold = m_Xdoc.CreateNode(XmlNodeType.Element, "GyroMouseMinThreshold", null); xmlGyroMinThreshold.InnerText = gyroMouseInfo[device].minThreshold.ToString(); rootElement.AppendChild(xmlGyroMinThreshold);
                    XmlNode xmlGyroMouseToggle = m_Xdoc.CreateNode(XmlNodeType.Element, "GyroMouseToggle", null); xmlGyroMouseToggle.InnerText = gyroMouseToggle[device].ToString(); rootElement.AppendChild(xmlGyroMouseToggle);

                    XmlNode xmlGyroOutMode = m_Xdoc.CreateNode(XmlNodeType.Element, "GyroOutputMode", null); xmlGyroOutMode.InnerText = gyroOutMode[device].ToString(); rootElement.AppendChild(xmlGyroOutMode);
                    XmlNode xmlGyroMStickTriggers = m_Xdoc.CreateNode(XmlNodeType.Element, "GyroMouseStickTriggers", null); xmlGyroMStickTriggers.InnerText = sAMouseStickTriggers[device].ToString(); rootElement.AppendChild(xmlGyroMStickTriggers);
                    XmlNode xmlGyroMStickTriggerCond = m_Xdoc.CreateNode(XmlNodeType.Element, "GyroMouseStickTriggerCond", null); xmlGyroMStickTriggerCond.InnerText = SaTriggerCondString(sAMouseStickTriggerCond[device]); rootElement.AppendChild(xmlGyroMStickTriggerCond);
                    XmlNode xmlGyroMStickTriggerTurns = m_Xdoc.CreateNode(XmlNodeType.Element, "GyroMouseStickTriggerTurns", null); xmlGyroMStickTriggerTurns.InnerText = gyroMouseStickTriggerTurns[device].ToString(); rootElement.AppendChild(xmlGyroMStickTriggerTurns);
                    XmlNode xmlGyroMStickHAxis = m_Xdoc.CreateNode(XmlNodeType.Element, "GyroMouseStickHAxis", null); xmlGyroMStickHAxis.InnerText = gyroMouseStickHorizontalAxis[device].ToString(); rootElement.AppendChild(xmlGyroMStickHAxis);
                    XmlNode xmlGyroMStickDZ = m_Xdoc.CreateNode(XmlNodeType.Element, "GyroMouseStickDeadZone", null); xmlGyroMStickDZ.InnerText = gyroMStickInfo[device].deadZone.ToString(); rootElement.AppendChild(xmlGyroMStickDZ);
                    XmlNode xmlGyroMStickMaxZ = m_Xdoc.CreateNode(XmlNodeType.Element, "GyroMouseStickMaxZone", null); xmlGyroMStickMaxZ.InnerText = gyroMStickInfo[device].maxZone.ToString(); rootElement.AppendChild(xmlGyroMStickMaxZ);
                    XmlNode xmlGyroMStickOutputStick = m_Xdoc.CreateNode(XmlNodeType.Element, "GyroMouseStickOutputStick", null); xmlGyroMStickOutputStick.InnerText = gyroMStickInfo[device].outputStick.ToString(); rootElement.AppendChild(xmlGyroMStickOutputStick);
                    XmlNode xmlGyroMStickOutputStickAxes = m_Xdoc.CreateNode(XmlNodeType.Element, "GyroMouseStickOutputStickAxes", null); xmlGyroMStickOutputStickAxes.InnerText = gyroMStickInfo[device].outputStickDir.ToString(); rootElement.AppendChild(xmlGyroMStickOutputStickAxes);
                    XmlNode xmlGyroMStickAntiDX = m_Xdoc.CreateNode(XmlNodeType.Element, "GyroMouseStickAntiDeadX", null); xmlGyroMStickAntiDX.InnerText = gyroMStickInfo[device].antiDeadX.ToString(); rootElement.AppendChild(xmlGyroMStickAntiDX);
                    XmlNode xmlGyroMStickAntiDY = m_Xdoc.CreateNode(XmlNodeType.Element, "GyroMouseStickAntiDeadY", null); xmlGyroMStickAntiDY.InnerText = gyroMStickInfo[device].antiDeadY.ToString(); rootElement.AppendChild(xmlGyroMStickAntiDY);
                    XmlNode xmlGyroMStickInvert = m_Xdoc.CreateNode(XmlNodeType.Element, "GyroMouseStickInvert", null); xmlGyroMStickInvert.InnerText = gyroMStickInfo[device].inverted.ToString(); rootElement.AppendChild(xmlGyroMStickInvert);
                    XmlNode xmlGyroMStickToggle = m_Xdoc.CreateNode(XmlNodeType.Element, "GyroMouseStickToggle", null); xmlGyroMStickToggle.InnerText = gyroMouseStickToggle[device].ToString(); rootElement.AppendChild(xmlGyroMStickToggle);
                    XmlNode xmlGyroMStickMaxOutput = m_Xdoc.CreateNode(XmlNodeType.Element, "GyroMouseStickMaxOutput", null); xmlGyroMStickMaxOutput.InnerText = gyroMStickInfo[device].maxOutput.ToString(); rootElement.AppendChild(xmlGyroMStickMaxOutput);
                    XmlNode xmlGyroMStickMaxOutputEnabled = m_Xdoc.CreateNode(XmlNodeType.Element, "GyroMouseStickMaxOutputEnabled", null); xmlGyroMStickMaxOutputEnabled.InnerText = gyroMStickInfo[device].maxOutputEnabled.ToString(); rootElement.AppendChild(xmlGyroMStickMaxOutputEnabled);
                    XmlNode xmlGyroMStickVerticalScale = m_Xdoc.CreateNode(XmlNodeType.Element, "GyroMouseStickVerticalScale", null); xmlGyroMStickVerticalScale.InnerText = gyroMStickInfo[device].vertScale.ToString(); rootElement.AppendChild(xmlGyroMStickVerticalScale);


                    /*XmlNode xmlGyroMStickSmoothing = m_Xdoc.CreateNode(XmlNodeType.Element, "GyroMouseStickSmoothing", null); xmlGyroMStickSmoothing.InnerText = gyroMStickInfo[device].useSmoothing.ToString(); rootElement.AppendChild(xmlGyroMStickSmoothing);
                    XmlNode xmlGyroMStickSmoothWeight = m_Xdoc.CreateNode(XmlNodeType.Element, "GyroMouseStickSmoothingWeight", null); xmlGyroMStickSmoothWeight.InnerText = Convert.ToInt32(gyroMStickInfo[device].smoothWeight * 100).ToString(); rootElement.AppendChild(xmlGyroMStickSmoothWeight);
                    */
                    XmlElement xmlGyroMStickSmoothingElement = m_Xdoc.CreateElement("GyroMouseStickSmoothingSettings");
                    XmlNode xmlGyroMStickSmoothing = m_Xdoc.CreateNode(XmlNodeType.Element, "UseSmoothing", null); xmlGyroMStickSmoothing.InnerText = gyroMStickInfo[device].useSmoothing.ToString(); xmlGyroMStickSmoothingElement.AppendChild(xmlGyroMStickSmoothing);
                    XmlNode xmlGyroMStickSmoothingMethod = m_Xdoc.CreateNode(XmlNodeType.Element, "SmoothingMethod", null); xmlGyroMStickSmoothingMethod.InnerText = gyroMStickInfo[device].SmoothMethodIdentifier(); xmlGyroMStickSmoothingElement.AppendChild(xmlGyroMStickSmoothingMethod);
                    XmlNode xmlGyroMStickSmoothWeight = m_Xdoc.CreateNode(XmlNodeType.Element, "SmoothingWeight", null); xmlGyroMStickSmoothWeight.InnerText = Convert.ToInt32(gyroMStickInfo[device].smoothWeight * 100).ToString(); xmlGyroMStickSmoothingElement.AppendChild(xmlGyroMStickSmoothWeight);
                    XmlNode xmlGyroMStickSmoothMincutoff = m_Xdoc.CreateNode(XmlNodeType.Element, "SmoothingMinCutoff", null); xmlGyroMStickSmoothMincutoff.InnerText = gyroMStickInfo[device].minCutoff.ToString(); xmlGyroMStickSmoothingElement.AppendChild(xmlGyroMStickSmoothMincutoff);
                    XmlNode xmlGyroMStickSmoothBeta = m_Xdoc.CreateNode(XmlNodeType.Element, "SmoothingBeta", null); xmlGyroMStickSmoothBeta.InnerText = gyroMStickInfo[device].beta.ToString(); xmlGyroMStickSmoothingElement.AppendChild(xmlGyroMStickSmoothBeta);
                    rootElement.AppendChild(xmlGyroMStickSmoothingElement);

                    XmlElement xmlGyroSwipeSettingsElement = m_Xdoc.CreateElement("GyroSwipeSettings");
                    XmlNode xmlGyroSwipeDeadzoneX = m_Xdoc.CreateNode(XmlNodeType.Element, "DeadZoneX", null); xmlGyroSwipeDeadzoneX.InnerText = gyroSwipeInfo[device].deadzoneX.ToString(); xmlGyroSwipeSettingsElement.AppendChild(xmlGyroSwipeDeadzoneX);
                    XmlNode xmlGyroSwipeDeadzoneY = m_Xdoc.CreateNode(XmlNodeType.Element, "DeadZoneY", null); xmlGyroSwipeDeadzoneY.InnerText = gyroSwipeInfo[device].deadzoneY.ToString(); xmlGyroSwipeSettingsElement.AppendChild(xmlGyroSwipeDeadzoneY);
                    XmlNode xmlGyroSwipeTriggers = m_Xdoc.CreateNode(XmlNodeType.Element, "Triggers", null); xmlGyroSwipeTriggers.InnerText = gyroSwipeInfo[device].triggers; xmlGyroSwipeSettingsElement.AppendChild(xmlGyroSwipeTriggers);
                    XmlNode xmlGyroSwipeTriggerCond = m_Xdoc.CreateNode(XmlNodeType.Element, "TriggerCond", null); xmlGyroSwipeTriggerCond.InnerText = SaTriggerCondString(gyroSwipeInfo[device].triggerCond); xmlGyroSwipeSettingsElement.AppendChild(xmlGyroSwipeTriggerCond);
                    XmlNode xmlGyroSwipeTriggerTurns = m_Xdoc.CreateNode(XmlNodeType.Element, "TriggerTurns", null); xmlGyroSwipeTriggerTurns.InnerText = gyroSwipeInfo[device].triggerTurns.ToString(); xmlGyroSwipeSettingsElement.AppendChild(xmlGyroSwipeTriggerTurns);
                    XmlNode xmlGyroSwipeXAxis = m_Xdoc.CreateNode(XmlNodeType.Element, "XAxis", null); xmlGyroSwipeXAxis.InnerText = gyroSwipeInfo[device].xAxis.ToString(); xmlGyroSwipeSettingsElement.AppendChild(xmlGyroSwipeXAxis);
                    XmlNode xmlGyroSwipeDelayTime = m_Xdoc.CreateNode(XmlNodeType.Element, "DelayTime", null); xmlGyroSwipeDelayTime.InnerText = gyroSwipeInfo[device].delayTime.ToString(); xmlGyroSwipeSettingsElement.AppendChild(xmlGyroSwipeDelayTime);
                    rootElement.AppendChild(xmlGyroSwipeSettingsElement);

                    XmlNode xmlProfileActions = m_Xdoc.CreateNode(XmlNodeType.Element, "ProfileActions", null); xmlProfileActions.InnerText = string.Join("/", ProfileActions[device]); rootElement.AppendChild(xmlProfileActions);
                    XmlNode xmlBTPollRate = m_Xdoc.CreateNode(XmlNodeType.Element, "BTPollRate", null); xmlBTPollRate.InnerText = btPollRate[device].ToString(); rootElement.AppendChild(xmlBTPollRate);

                    XmlNode xmlLsOutputCurveMode = m_Xdoc.CreateNode(XmlNodeType.Element, "LSOutputCurveMode", null); xmlLsOutputCurveMode.InnerText = stickOutputCurveString(getLsOutCurveMode(device)); rootElement.AppendChild(xmlLsOutputCurveMode);
                    XmlNode xmlLsOutputCurveCustom = m_Xdoc.CreateNode(XmlNodeType.Element, "LSOutputCurveCustom", null); xmlLsOutputCurveCustom.InnerText = LSOutBezierCurveObj[device].ToString(); rootElement.AppendChild(xmlLsOutputCurveCustom);

                    XmlNode xmlRsOutputCurveMode = m_Xdoc.CreateNode(XmlNodeType.Element, "RSOutputCurveMode", null); xmlRsOutputCurveMode.InnerText = stickOutputCurveString(getRsOutCurveMode(device)); rootElement.AppendChild(xmlRsOutputCurveMode);
                    XmlNode xmlRsOutputCurveCustom = m_Xdoc.CreateNode(XmlNodeType.Element, "RSOutputCurveCustom", null); xmlRsOutputCurveCustom.InnerText = RSOutBezierCurveObj[device].ToString(); rootElement.AppendChild(xmlRsOutputCurveCustom);

                    XmlNode xmlLsSquareStickMode = m_Xdoc.CreateNode(XmlNodeType.Element, "LSSquareStick", null); xmlLsSquareStickMode.InnerText = SquStickInfo[device].lsMode.ToString(); rootElement.AppendChild(xmlLsSquareStickMode);
                    XmlNode xmlRsSquareStickMode = m_Xdoc.CreateNode(XmlNodeType.Element, "RSSquareStick", null); xmlRsSquareStickMode.InnerText = SquStickInfo[device].rsMode.ToString(); rootElement.AppendChild(xmlRsSquareStickMode);

                    XmlNode xmlSquareStickRoundness = m_Xdoc.CreateNode(XmlNodeType.Element, "SquareStickRoundness", null); xmlSquareStickRoundness.InnerText = SquStickInfo[device].lsRoundness.ToString(); rootElement.AppendChild(xmlSquareStickRoundness);
                    XmlNode xmlSquareRStickRoundness = m_Xdoc.CreateNode(XmlNodeType.Element, "SquareRStickRoundness", null); xmlSquareRStickRoundness.InnerText = SquStickInfo[device].rsRoundness.ToString(); rootElement.AppendChild(xmlSquareRStickRoundness);

                    XmlNode xmlLsAntiSnapbackEnabled = m_Xdoc.CreateNode(XmlNodeType.Element, "LSAntiSnapback", null); xmlLsAntiSnapbackEnabled.InnerText = LSAntiSnapbackInfo[device].enabled.ToString(); rootElement.AppendChild(xmlLsAntiSnapbackEnabled);
                    XmlNode xmlRsAntiSnapbackEnabled = m_Xdoc.CreateNode(XmlNodeType.Element, "RSAntiSnapback", null); xmlRsAntiSnapbackEnabled.InnerText = RSAntiSnapbackInfo[device].enabled.ToString(); rootElement.AppendChild(xmlRsAntiSnapbackEnabled);

                    XmlNode xmlLsAntiSnapbackDelta = m_Xdoc.CreateNode(XmlNodeType.Element, "LSAntiSnapbackDelta", null); xmlLsAntiSnapbackDelta.InnerText = LSAntiSnapbackInfo[device].delta.ToString(); rootElement.AppendChild(xmlLsAntiSnapbackDelta);
                    XmlNode xmlRsAntiSnapbackDelta = m_Xdoc.CreateNode(XmlNodeType.Element, "RSAntiSnapbackDelta", null); xmlRsAntiSnapbackDelta.InnerText = RSAntiSnapbackInfo[device].delta.ToString(); rootElement.AppendChild(xmlRsAntiSnapbackDelta);

                    XmlNode xmlLsAntiSnapbackTimeout = m_Xdoc.CreateNode(XmlNodeType.Element, "LSAntiSnapbackTimeout", null); xmlLsAntiSnapbackTimeout.InnerText = LSAntiSnapbackInfo[device].timeout.ToString(); rootElement.AppendChild(xmlLsAntiSnapbackTimeout);
                    XmlNode xmlRsAntiSnapbackTimeout = m_Xdoc.CreateNode(XmlNodeType.Element, "RSAntiSnapbackTimeout", null); xmlRsAntiSnapbackTimeout.InnerText = RSAntiSnapbackInfo[device].timeout.ToString(); rootElement.AppendChild(xmlRsAntiSnapbackTimeout);

                    XmlNode xmlLsOutputMode = m_Xdoc.CreateNode(XmlNodeType.Element, "LSOutputMode", null); xmlLsOutputMode.InnerText = LSOutputSettings[device].mode.ToString(); rootElement.AppendChild(xmlLsOutputMode);
                    XmlNode xmlRsOutputMode = m_Xdoc.CreateNode(XmlNodeType.Element, "RSOutputMode", null); xmlRsOutputMode.InnerText = RSOutputSettings[device].mode.ToString(); rootElement.AppendChild(xmlRsOutputMode);

                    XmlElement xmlLsOutputSettingsElement = m_Xdoc.CreateElement("LSOutputSettings");
                    XmlElement xmlLsFlickStickGroupElement = m_Xdoc.CreateElement("FlickStickSettings"); xmlLsOutputSettingsElement.AppendChild(xmlLsFlickStickGroupElement);
                    XmlNode xmlLsFlickStickRWC = m_Xdoc.CreateNode(XmlNodeType.Element, "RealWorldCalibration", null); xmlLsFlickStickRWC.InnerText = LSOutputSettings[device].outputSettings.flickSettings.realWorldCalibration.ToString(); xmlLsFlickStickGroupElement.AppendChild(xmlLsFlickStickRWC);
                    XmlNode xmlLsFlickStickThreshold = m_Xdoc.CreateNode(XmlNodeType.Element, "FlickThreshold", null); xmlLsFlickStickThreshold.InnerText = LSOutputSettings[device].outputSettings.flickSettings.flickThreshold.ToString(); xmlLsFlickStickGroupElement.AppendChild(xmlLsFlickStickThreshold);
                    XmlNode xmlLsFlickStickTime = m_Xdoc.CreateNode(XmlNodeType.Element, "FlickTime", null); xmlLsFlickStickTime.InnerText = LSOutputSettings[device].outputSettings.flickSettings.flickTime.ToString(); xmlLsFlickStickGroupElement.AppendChild(xmlLsFlickStickTime);
                    rootElement.AppendChild(xmlLsOutputSettingsElement);

                    XmlElement xmlRsOutputSettingsElement = m_Xdoc.CreateElement("RSOutputSettings");
                    XmlElement xmlRsFlickStickGroupElement = m_Xdoc.CreateElement("FlickStickSettings"); xmlRsOutputSettingsElement.AppendChild(xmlRsFlickStickGroupElement);
                    XmlNode xmlRsFlickStickRWC = m_Xdoc.CreateNode(XmlNodeType.Element, "RealWorldCalibration", null); xmlRsFlickStickRWC.InnerText = RSOutputSettings[device].outputSettings.flickSettings.realWorldCalibration.ToString(); xmlRsFlickStickGroupElement.AppendChild(xmlRsFlickStickRWC);
                    XmlNode xmlRsFlickStickThreshold = m_Xdoc.CreateNode(XmlNodeType.Element, "FlickThreshold", null); xmlRsFlickStickThreshold.InnerText = RSOutputSettings[device].outputSettings.flickSettings.flickThreshold.ToString(); xmlRsFlickStickGroupElement.AppendChild(xmlRsFlickStickThreshold);
                    XmlNode xmlRsFlickStickTime = m_Xdoc.CreateNode(XmlNodeType.Element, "FlickTime", null); xmlRsFlickStickTime.InnerText = RSOutputSettings[device].outputSettings.flickSettings.flickTime.ToString(); xmlRsFlickStickGroupElement.AppendChild(xmlRsFlickStickTime);
                    rootElement.AppendChild(xmlRsOutputSettingsElement);

                    XmlNode xmlL2OutputCurveMode = m_Xdoc.CreateNode(XmlNodeType.Element, "L2OutputCurveMode", null); xmlL2OutputCurveMode.InnerText = axisOutputCurveString(getL2OutCurveMode(device)); rootElement.AppendChild(xmlL2OutputCurveMode);
                    XmlNode xmlL2OutputCurveCustom = m_Xdoc.CreateNode(XmlNodeType.Element, "L2OutputCurveCustom", null); xmlL2OutputCurveCustom.InnerText = L2OutBezierCurveObj[device].ToString(); rootElement.AppendChild(xmlL2OutputCurveCustom);

                    XmlNode xmlL2TwoStageMode = m_Xdoc.CreateNode(XmlNodeType.Element, "L2TwoStageMode", null); xmlL2TwoStageMode.InnerText = L2OutputSettings[device].twoStageMode.ToString(); rootElement.AppendChild(xmlL2TwoStageMode);
                    XmlNode xmlR2TwoStageMode = m_Xdoc.CreateNode(XmlNodeType.Element, "R2TwoStageMode", null); xmlR2TwoStageMode.InnerText = R2OutputSettings[device].twoStageMode.ToString(); rootElement.AppendChild(xmlR2TwoStageMode);

                    XmlNode xmlL2TriggerEffect = m_Xdoc.CreateNode(XmlNodeType.Element, "L2TriggerEffect", null); xmlL2TriggerEffect.InnerText = L2OutputSettings[device].triggerEffect.ToString(); rootElement.AppendChild(xmlL2TriggerEffect);
                    XmlNode xmlR2TriggerEffect = m_Xdoc.CreateNode(XmlNodeType.Element, "R2TriggerEffect", null); xmlR2TriggerEffect.InnerText = R2OutputSettings[device].triggerEffect.ToString(); rootElement.AppendChild(xmlR2TriggerEffect);

                    XmlNode xmlR2OutputCurveMode = m_Xdoc.CreateNode(XmlNodeType.Element, "R2OutputCurveMode", null); xmlR2OutputCurveMode.InnerText = axisOutputCurveString(getR2OutCurveMode(device)); rootElement.AppendChild(xmlR2OutputCurveMode);
                    XmlNode xmlR2OutputCurveCustom = m_Xdoc.CreateNode(XmlNodeType.Element, "R2OutputCurveCustom", null); xmlR2OutputCurveCustom.InnerText = R2OutBezierCurveObj[device].ToString(); rootElement.AppendChild(xmlR2OutputCurveCustom);

                    XmlNode xmlSXOutputCurveMode = m_Xdoc.CreateNode(XmlNodeType.Element, "SXOutputCurveMode", null); xmlSXOutputCurveMode.InnerText = axisOutputCurveString(getSXOutCurveMode(device)); rootElement.AppendChild(xmlSXOutputCurveMode);
                    XmlNode xmlSXOutputCurveCustom = m_Xdoc.CreateNode(XmlNodeType.Element, "SXOutputCurveCustom", null); xmlSXOutputCurveCustom.InnerText = SXOutBezierCurveObj[device].ToString(); rootElement.AppendChild(xmlSXOutputCurveCustom);

                    XmlNode xmlSZOutputCurveMode = m_Xdoc.CreateNode(XmlNodeType.Element, "SZOutputCurveMode", null); xmlSZOutputCurveMode.InnerText = axisOutputCurveString(getSZOutCurveMode(device)); rootElement.AppendChild(xmlSZOutputCurveMode);
                    XmlNode xmlSZOutputCurveCustom = m_Xdoc.CreateNode(XmlNodeType.Element, "SZOutputCurveCustom", null); xmlSZOutputCurveCustom.InnerText = SZOutBezierCurveObj[device].ToString(); rootElement.AppendChild(xmlSZOutputCurveCustom);

                    XmlNode xmlTrackBallMode = m_Xdoc.CreateNode(XmlNodeType.Element, "TrackballMode", null); xmlTrackBallMode.InnerText = trackballMode[device].ToString(); rootElement.AppendChild(xmlTrackBallMode);
                    XmlNode xmlTrackBallFriction = m_Xdoc.CreateNode(XmlNodeType.Element, "TrackballFriction", null); xmlTrackBallFriction.InnerText = TrackballFriction[device].ToString(); rootElement.AppendChild(xmlTrackBallFriction);

                    XmlNode xmlTouchRelMouseRotation = m_Xdoc.CreateNode(XmlNodeType.Element, "TouchRelMouseRotation", null); xmlTouchRelMouseRotation.InnerText = Convert.ToInt32(TouchpadRelMouse[device].rotation * 180.0 / Math.PI).ToString(); rootElement.AppendChild(xmlTouchRelMouseRotation);
                    XmlNode xmlTouchRelMouseMinThreshold = m_Xdoc.CreateNode(XmlNodeType.Element, "TouchRelMouseMinThreshold", null); xmlTouchRelMouseMinThreshold.InnerText = TouchpadRelMouse[device].minThreshold.ToString(); rootElement.AppendChild(xmlTouchRelMouseMinThreshold);

                    XmlElement xmlTouchAbsMouseGroupEl = m_Xdoc.CreateElement("TouchpadAbsMouseSettings");
                    XmlElement xmlTouchAbsMouseMaxZoneX = m_Xdoc.CreateElement("MaxZoneX"); xmlTouchAbsMouseMaxZoneX.InnerText = TouchpadAbsMouse[device].maxZoneX.ToString(); xmlTouchAbsMouseGroupEl.AppendChild(xmlTouchAbsMouseMaxZoneX);
                    XmlElement xmlTouchAbsMouseMaxZoneY = m_Xdoc.CreateElement("MaxZoneY"); xmlTouchAbsMouseMaxZoneY.InnerText = TouchpadAbsMouse[device].maxZoneY.ToString(); xmlTouchAbsMouseGroupEl.AppendChild(xmlTouchAbsMouseMaxZoneY);
                    XmlElement xmlTouchAbsMouseSnapCenter = m_Xdoc.CreateElement("SnapToCenter"); xmlTouchAbsMouseSnapCenter.InnerText = TouchpadAbsMouse[device].snapToCenter.ToString(); xmlTouchAbsMouseGroupEl.AppendChild(xmlTouchAbsMouseSnapCenter);
                    rootElement.AppendChild(xmlTouchAbsMouseGroupEl);

                    XmlNode xmlOutContDevice = m_Xdoc.CreateNode(XmlNodeType.Element, "OutputContDevice", null); xmlOutContDevice.InnerText = OutContDeviceString(OutputDeviceType[device]); rootElement.AppendChild(xmlOutContDevice);

                    XmlNode NodeControl = m_Xdoc.CreateNode(XmlNodeType.Element, "Control", null);
                    XmlNode Key = m_Xdoc.CreateNode(XmlNodeType.Element, "Key", null);
                    XmlNode Macro = m_Xdoc.CreateNode(XmlNodeType.Element, "Macro", null);
                    XmlNode KeyType = m_Xdoc.CreateNode(XmlNodeType.Element, "KeyType", null);
                    XmlNode Button = m_Xdoc.CreateNode(XmlNodeType.Element, "Button", null);
                    XmlNode Extras = m_Xdoc.CreateNode(XmlNodeType.Element, "Extras", null);

                    XmlNode NodeShiftControl = m_Xdoc.CreateNode(XmlNodeType.Element, "ShiftControl", null);

                    XmlNode ShiftKey = m_Xdoc.CreateNode(XmlNodeType.Element, "Key", null);
                    XmlNode ShiftMacro = m_Xdoc.CreateNode(XmlNodeType.Element, "Macro", null);
                    XmlNode ShiftKeyType = m_Xdoc.CreateNode(XmlNodeType.Element, "KeyType", null);
                    XmlNode ShiftButton = m_Xdoc.CreateNode(XmlNodeType.Element, "Button", null);
                    XmlNode ShiftExtras = m_Xdoc.CreateNode(XmlNodeType.Element, "Extras", null);

                    foreach (DS4ControlSettings dcs in Ds4Settings[device])
                    {
                        if (dcs.ControlActionType != DS4ControlSettings.ActionType.Default)
                        {
                            XmlNode buttonNode;
                            string keyType = string.Empty;

                            if (dcs.ControlActionType == DS4ControlSettings.ActionType.Button &&
                                dcs.ActionData.ActionButton == X360Controls.Unbound)
                            {
                                keyType += DS4KeyType.Unbound;
                            }

                            if (dcs.KeyType.HasFlag(DS4KeyType.HoldMacro))
                                keyType += DS4KeyType.HoldMacro;
                            else if (dcs.KeyType.HasFlag(DS4KeyType.Macro))
                                keyType += DS4KeyType.Macro;

                            if (dcs.KeyType.HasFlag(DS4KeyType.Toggle))
                                keyType += DS4KeyType.Toggle;
                            if (dcs.KeyType.HasFlag(DS4KeyType.ScanCode))
                                keyType += DS4KeyType.ScanCode;

                            if (keyType != string.Empty)
                            {
                                buttonNode = m_Xdoc.CreateNode(XmlNodeType.Element, dcs.Control.ToString(), null);
                                buttonNode.InnerText = keyType;
                                KeyType.AppendChild(buttonNode);
                            }

                            buttonNode = m_Xdoc.CreateNode(XmlNodeType.Element, dcs.Control.ToString(), null);
                            if (dcs.ControlActionType == DS4ControlSettings.ActionType.Macro)
                            {
                                int[] ii = dcs.ActionData.ActionMacro;
                                buttonNode.InnerText = string.Join("/", ii);
                                Macro.AppendChild(buttonNode);
                            }
                            else if (dcs.ControlActionType == DS4ControlSettings.ActionType.Key)
                            {
                                buttonNode.InnerText = dcs.ActionData.ActionKey.ToString();
                                Key.AppendChild(buttonNode);
                            }
                            else if (dcs.ControlActionType == DS4ControlSettings.ActionType.Button)
                            {
                                buttonNode.InnerText = GetX360ControlString((X360Controls)dcs.ActionData.ActionButton);
                                Button.AppendChild(buttonNode);
                            }
                        }

                        bool hasvalue = false;
                        if (!string.IsNullOrEmpty(dcs.Extras))
                        {
                            foreach (string s in dcs.Extras.Split(','))
                            {
                                if (s != "0")
                                {
                                    hasvalue = true;
                                    break;
                                }
                            }
                        }

                        if (hasvalue)
                        {
                            XmlNode extraNode = m_Xdoc.CreateNode(XmlNodeType.Element, dcs.Control.ToString(), null);
                            extraNode.InnerText = dcs.Extras;
                            Extras.AppendChild(extraNode);
                        }

                        if (dcs.ShiftActionType != DS4ControlSettings.ActionType.Default && dcs.ShiftTrigger > 0)
                        {
                            XmlElement buttonNode;
                            string keyType = string.Empty;

                            if (dcs.ShiftActionType == DS4ControlSettings.ActionType.Button &&
                                dcs.ShiftAction.ActionButton == X360Controls.Unbound)
                            {
                                keyType += DS4KeyType.Unbound;
                            }

                            if (dcs.ShiftKeyType.HasFlag(DS4KeyType.HoldMacro))
                                keyType += DS4KeyType.HoldMacro;
                            if (dcs.ShiftKeyType.HasFlag(DS4KeyType.Macro))
                                keyType += DS4KeyType.Macro;
                            if (dcs.ShiftKeyType.HasFlag(DS4KeyType.Toggle))
                                keyType += DS4KeyType.Toggle;
                            if (dcs.ShiftKeyType.HasFlag(DS4KeyType.ScanCode))
                                keyType += DS4KeyType.ScanCode;

                            if (keyType != string.Empty)
                            {
                                buttonNode = m_Xdoc.CreateElement(dcs.Control.ToString());
                                buttonNode.InnerText = keyType;
                                ShiftKeyType.AppendChild(buttonNode);
                            }

                            buttonNode = m_Xdoc.CreateElement(dcs.Control.ToString());
                            buttonNode.SetAttribute("Trigger", dcs.ShiftTrigger.ToString());
                            if (dcs.ShiftActionType == DS4ControlSettings.ActionType.Macro)
                            {
                                int[] ii = dcs.ShiftAction.ActionMacro;
                                buttonNode.InnerText = string.Join("/", ii);
                                ShiftMacro.AppendChild(buttonNode);
                            }
                            else if (dcs.ShiftActionType == DS4ControlSettings.ActionType.Key)
                            {
                                buttonNode.InnerText = dcs.ShiftAction.ActionKey.ToString();
                                ShiftKey.AppendChild(buttonNode);
                            }
                            else if (dcs.ShiftActionType == DS4ControlSettings.ActionType.Button)
                            {
                                buttonNode.InnerText = dcs.ShiftAction.ActionButton.ToString();
                                ShiftButton.AppendChild(buttonNode);
                            }
                        }

                        hasvalue = false;
                        if (!string.IsNullOrEmpty(dcs.ShiftExtras))
                        {
                            foreach (string s in dcs.ShiftExtras.Split(','))
                            {
                                if (s != "0")
                                {
                                    hasvalue = true;
                                    break;
                                }
                            }
                        }

                        if (hasvalue)
                        {
                            XmlNode extraNode = m_Xdoc.CreateNode(XmlNodeType.Element, dcs.Control.ToString(), null);
                            extraNode.InnerText = dcs.ShiftExtras;
                            ShiftExtras.AppendChild(extraNode);
                        }
                    }

                    rootElement.AppendChild(NodeControl);
                    if (Button.HasChildNodes)
                        NodeControl.AppendChild(Button);
                    if (Macro.HasChildNodes)
                        NodeControl.AppendChild(Macro);
                    if (Key.HasChildNodes)
                        NodeControl.AppendChild(Key);
                    if (Extras.HasChildNodes)
                        NodeControl.AppendChild(Extras);
                    if (KeyType.HasChildNodes)
                        NodeControl.AppendChild(KeyType);

                    if (NodeControl.HasChildNodes)
                        rootElement.AppendChild(NodeControl);

                    rootElement.AppendChild(NodeShiftControl);
                    if (ShiftButton.HasChildNodes)
                        NodeShiftControl.AppendChild(ShiftButton);
                    if (ShiftMacro.HasChildNodes)
                        NodeShiftControl.AppendChild(ShiftMacro);
                    if (ShiftKey.HasChildNodes)
                        NodeShiftControl.AppendChild(ShiftKey);
                    if (ShiftKeyType.HasChildNodes)
                        NodeShiftControl.AppendChild(ShiftKeyType);
                    if (ShiftExtras.HasChildNodes)
                        NodeShiftControl.AppendChild(ShiftExtras);

                    m_Xdoc.AppendChild(rootElement);
                    m_Xdoc.Save(path);
                }
                catch { Saved = false; }
                return Saved;
            }

            public DS4Controls GetDs4ControlsByName(string key)
            {
                if (!key.StartsWith("bn"))
                    return (DS4Controls)Enum.Parse(typeof(DS4Controls), key, true);

                switch (key)
                {
                    case "bnShare": return DS4Controls.Share;
                    case "bnL3": return DS4Controls.L3;
                    case "bnR3": return DS4Controls.R3;
                    case "bnOptions": return DS4Controls.Options;
                    case "bnUp": return DS4Controls.DpadUp;
                    case "bnRight": return DS4Controls.DpadRight;
                    case "bnDown": return DS4Controls.DpadDown;
                    case "bnLeft": return DS4Controls.DpadLeft;

                    case "bnL1": return DS4Controls.L1;
                    case "bnR1": return DS4Controls.R1;
                    case "bnTriangle": return DS4Controls.Triangle;
                    case "bnCircle": return DS4Controls.Circle;
                    case "bnCross": return DS4Controls.Cross;
                    case "bnSquare": return DS4Controls.Square;

                    case "bnPS": return DS4Controls.PS;
                    case "bnLSLeft": return DS4Controls.LXNeg;
                    case "bnLSUp": return DS4Controls.LYNeg;
                    case "bnRSLeft": return DS4Controls.RXNeg;
                    case "bnRSUp": return DS4Controls.RYNeg;

                    case "bnLSRight": return DS4Controls.LXPos;
                    case "bnLSDown": return DS4Controls.LYPos;
                    case "bnRSRight": return DS4Controls.RXPos;
                    case "bnRSDown": return DS4Controls.RYPos;
                    case "bnL2": return DS4Controls.L2;
                    case "bnR2": return DS4Controls.R2;

                    case "bnTouchLeft": return DS4Controls.TouchLeft;
                    case "bnTouchMulti": return DS4Controls.TouchMulti;
                    case "bnTouchUpper": return DS4Controls.TouchUpper;
                    case "bnTouchRight": return DS4Controls.TouchRight;
                    case "bnGyroXP": return DS4Controls.GyroXPos;
                    case "bnGyroXN": return DS4Controls.GyroXNeg;
                    case "bnGyroZP": return DS4Controls.GyroZPos;
                    case "bnGyroZN": return DS4Controls.GyroZNeg;

                    case "bnSwipeUp": return DS4Controls.SwipeUp;
                    case "bnSwipeDown": return DS4Controls.SwipeDown;
                    case "bnSwipeLeft": return DS4Controls.SwipeLeft;
                    case "bnSwipeRight": return DS4Controls.SwipeRight;

                    #region OldShiftname
                    case "sbnShare": return DS4Controls.Share;
                    case "sbnL3": return DS4Controls.L3;
                    case "sbnR3": return DS4Controls.R3;
                    case "sbnOptions": return DS4Controls.Options;
                    case "sbnUp": return DS4Controls.DpadUp;
                    case "sbnRight": return DS4Controls.DpadRight;
                    case "sbnDown": return DS4Controls.DpadDown;
                    case "sbnLeft": return DS4Controls.DpadLeft;

                    case "sbnL1": return DS4Controls.L1;
                    case "sbnR1": return DS4Controls.R1;
                    case "sbnTriangle": return DS4Controls.Triangle;
                    case "sbnCircle": return DS4Controls.Circle;
                    case "sbnCross": return DS4Controls.Cross;
                    case "sbnSquare": return DS4Controls.Square;

                    case "sbnPS": return DS4Controls.PS;
                    case "sbnLSLeft": return DS4Controls.LXNeg;
                    case "sbnLSUp": return DS4Controls.LYNeg;
                    case "sbnRSLeft": return DS4Controls.RXNeg;
                    case "sbnRSUp": return DS4Controls.RYNeg;

                    case "sbnLSRight": return DS4Controls.LXPos;
                    case "sbnLSDown": return DS4Controls.LYPos;
                    case "sbnRSRight": return DS4Controls.RXPos;
                    case "sbnRSDown": return DS4Controls.RYPos;
                    case "sbnL2": return DS4Controls.L2;
                    case "sbnR2": return DS4Controls.R2;

                    case "sbnTouchLeft": return DS4Controls.TouchLeft;
                    case "sbnTouchMulti": return DS4Controls.TouchMulti;
                    case "sbnTouchUpper": return DS4Controls.TouchUpper;
                    case "sbnTouchRight": return DS4Controls.TouchRight;
                    case "sbnGsyroXP": return DS4Controls.GyroXPos;
                    case "sbnGyroXN": return DS4Controls.GyroXNeg;
                    case "sbnGyroZP": return DS4Controls.GyroZPos;
                    case "sbnGyroZN": return DS4Controls.GyroZNeg;
                    #endregion

                    case "bnShiftShare": return DS4Controls.Share;
                    case "bnShiftL3": return DS4Controls.L3;
                    case "bnShiftR3": return DS4Controls.R3;
                    case "bnShiftOptions": return DS4Controls.Options;
                    case "bnShiftUp": return DS4Controls.DpadUp;
                    case "bnShiftRight": return DS4Controls.DpadRight;
                    case "bnShiftDown": return DS4Controls.DpadDown;
                    case "bnShiftLeft": return DS4Controls.DpadLeft;

                    case "bnShiftL1": return DS4Controls.L1;
                    case "bnShiftR1": return DS4Controls.R1;
                    case "bnShiftTriangle": return DS4Controls.Triangle;
                    case "bnShiftCircle": return DS4Controls.Circle;
                    case "bnShiftCross": return DS4Controls.Cross;
                    case "bnShiftSquare": return DS4Controls.Square;

                    case "bnShiftPS": return DS4Controls.PS;
                    case "bnShiftLSLeft": return DS4Controls.LXNeg;
                    case "bnShiftLSUp": return DS4Controls.LYNeg;
                    case "bnShiftRSLeft": return DS4Controls.RXNeg;
                    case "bnShiftRSUp": return DS4Controls.RYNeg;

                    case "bnShiftLSRight": return DS4Controls.LXPos;
                    case "bnShiftLSDown": return DS4Controls.LYPos;
                    case "bnShiftRSRight": return DS4Controls.RXPos;
                    case "bnShiftRSDown": return DS4Controls.RYPos;
                    case "bnShiftL2": return DS4Controls.L2;
                    case "bnShiftR2": return DS4Controls.R2;

                    case "bnShiftTouchLeft": return DS4Controls.TouchLeft;
                    case "bnShiftTouchMulti": return DS4Controls.TouchMulti;
                    case "bnShiftTouchUpper": return DS4Controls.TouchUpper;
                    case "bnShiftTouchRight": return DS4Controls.TouchRight;
                    case "bnShiftGyroXP": return DS4Controls.GyroXPos;
                    case "bnShiftGyroXN": return DS4Controls.GyroXNeg;
                    case "bnShiftGyroZP": return DS4Controls.GyroZPos;
                    case "bnShiftGyroZN": return DS4Controls.GyroZNeg;

                    case "bnShiftSwipeUp": return DS4Controls.SwipeUp;
                    case "bnShiftSwipeDown": return DS4Controls.SwipeDown;
                    case "bnShiftSwipeLeft": return DS4Controls.SwipeLeft;
                    case "bnShiftSwipeRight": return DS4Controls.SwipeRight;
                }

                return 0;
            }

            public X360Controls GetX360ControlsByName(string key)
            {
                X360Controls x3c;
                if (Enum.TryParse(key, true, out x3c))
                    return x3c;

                switch (key)
                {
                    case "Back": return X360Controls.Back;
                    case "Left Stick": return X360Controls.LS;
                    case "Right Stick": return X360Controls.RS;
                    case "Start": return X360Controls.Start;
                    case "Up Button": return X360Controls.DpadUp;
                    case "Right Button": return X360Controls.DpadRight;
                    case "Down Button": return X360Controls.DpadDown;
                    case "Left Button": return X360Controls.DpadLeft;

                    case "Left Bumper": return X360Controls.LB;
                    case "Right Bumper": return X360Controls.RB;
                    case "Y Button": return X360Controls.Y;
                    case "B Button": return X360Controls.B;
                    case "A Button": return X360Controls.A;
                    case "X Button": return X360Controls.X;

                    case "Guide": return X360Controls.Guide;
                    case "Left X-Axis-": return X360Controls.LXNeg;
                    case "Left Y-Axis-": return X360Controls.LYNeg;
                    case "Right X-Axis-": return X360Controls.RXNeg;
                    case "Right Y-Axis-": return X360Controls.RYNeg;

                    case "Left X-Axis+": return X360Controls.LXPos;
                    case "Left Y-Axis+": return X360Controls.LYPos;
                    case "Right X-Axis+": return X360Controls.RXPos;
                    case "Right Y-Axis+": return X360Controls.RYPos;
                    case "Left Trigger": return X360Controls.LT;
                    case "Right Trigger": return X360Controls.RT;
                    case "Touchpad Click": return X360Controls.TouchpadClick;

                    case "Left Mouse Button": return X360Controls.LeftMouse;
                    case "Right Mouse Button": return X360Controls.RightMouse;
                    case "Middle Mouse Button": return X360Controls.MiddleMouse;
                    case "4th Mouse Button": return X360Controls.FourthMouse;
                    case "5th Mouse Button": return X360Controls.FifthMouse;
                    case "Mouse Wheel Up": return X360Controls.WUP;
                    case "Mouse Wheel Down": return X360Controls.WDOWN;
                    case "Mouse Up": return X360Controls.MouseUp;
                    case "Mouse Down": return X360Controls.MouseDown;
                    case "Mouse Left": return X360Controls.MouseLeft;
                    case "Mouse Right": return X360Controls.MouseRight;
                    case "Unbound": return X360Controls.Unbound;
                }

                return X360Controls.Unbound;
            }

            public string GetX360ControlString(X360Controls key)
            {
                switch (key)
                {
                    case X360Controls.Back: return "Back";
                    case X360Controls.LS: return "Left Stick";
                    case X360Controls.RS: return "Right Stick";
                    case X360Controls.Start: return "Start";
                    case X360Controls.DpadUp: return "Up Button";
                    case X360Controls.DpadRight: return "Right Button";
                    case X360Controls.DpadDown: return "Down Button";
                    case X360Controls.DpadLeft: return "Left Button";

                    case X360Controls.LB: return "Left Bumper";
                    case X360Controls.RB: return "Right Bumper";
                    case X360Controls.Y: return "Y Button";
                    case X360Controls.B: return "B Button";
                    case X360Controls.A: return "A Button";
                    case X360Controls.X: return "X Button";

                    case X360Controls.Guide: return "Guide";
                    case X360Controls.LXNeg: return "Left X-Axis-";
                    case X360Controls.LYNeg: return "Left Y-Axis-";
                    case X360Controls.RXNeg: return "Right X-Axis-";
                    case X360Controls.RYNeg: return "Right Y-Axis-";

                    case X360Controls.LXPos: return "Left X-Axis+";
                    case X360Controls.LYPos: return "Left Y-Axis+";
                    case X360Controls.RXPos: return "Right X-Axis+";
                    case X360Controls.RYPos: return "Right Y-Axis+";
                    case X360Controls.LT: return "Left Trigger";
                    case X360Controls.RT: return "Right Trigger";
                    case X360Controls.TouchpadClick: return "Touchpad Click";

                    case X360Controls.LeftMouse: return "Left Mouse Button";
                    case X360Controls.RightMouse: return "Right Mouse Button";
                    case X360Controls.MiddleMouse: return "Middle Mouse Button";
                    case X360Controls.FourthMouse: return "4th Mouse Button";
                    case X360Controls.FifthMouse: return "5th Mouse Button";
                    case X360Controls.WUP: return "Mouse Wheel Up";
                    case X360Controls.WDOWN: return "Mouse Wheel Down";
                    case X360Controls.MouseUp: return "Mouse Up";
                    case X360Controls.MouseDown: return "Mouse Down";
                    case X360Controls.MouseLeft: return "Mouse Left";
                    case X360Controls.MouseRight: return "Mouse Right";
                    case X360Controls.Unbound: return "Unbound";
                }

                return "Unbound";
            }

            public bool LoadProfile(int device, bool launchprogram, ControlService control,
                string propath = "", bool xinputChange = true, bool postLoad = true)
            {
                bool Loaded = true;
                Dictionary<DS4Controls, DS4KeyType> customMapKeyTypes = new Dictionary<DS4Controls, DS4KeyType>();
                Dictionary<DS4Controls, UInt16> customMapKeys = new Dictionary<DS4Controls, UInt16>();
                Dictionary<DS4Controls, X360Controls> customMapButtons = new Dictionary<DS4Controls, X360Controls>();
                Dictionary<DS4Controls, String> customMapMacros = new Dictionary<DS4Controls, String>();
                Dictionary<DS4Controls, String> customMapExtras = new Dictionary<DS4Controls, String>();
                Dictionary<DS4Controls, DS4KeyType> shiftCustomMapKeyTypes = new Dictionary<DS4Controls, DS4KeyType>();
                Dictionary<DS4Controls, UInt16> shiftCustomMapKeys = new Dictionary<DS4Controls, UInt16>();
                Dictionary<DS4Controls, X360Controls> shiftCustomMapButtons = new Dictionary<DS4Controls, X360Controls>();
                Dictionary<DS4Controls, String> shiftCustomMapMacros = new Dictionary<DS4Controls, String>();
                Dictionary<DS4Controls, String> shiftCustomMapExtras = new Dictionary<DS4Controls, String>();
                string rootname = "DS4Windows";
                bool missingSetting = false;
                bool migratePerformed = false;
                string profilepath;
                if (string.IsNullOrEmpty(propath))
                    profilepath = Path.Combine(RuntimeAppDataPath, Constants.ProfilesSubDirectory,
                        $"{ProfilePath[device]}{XML_EXTENSION}");
                else
                    profilepath = propath;

                bool xinputPlug = false;
                bool xinputStatus = false;

                if (File.Exists(profilepath))
                {
                    XmlNode Item;

                    ProfileMigration tmpMigration = new ProfileMigration(profilepath);
                    if (tmpMigration.RequiresMigration())
                    {
                        tmpMigration.Migrate();
                        m_Xdoc.Load(tmpMigration.ProfileReader);
                        migratePerformed = true;
                    }
                    else if (tmpMigration.ProfileReader != null)
                    {
                        m_Xdoc.Load(tmpMigration.ProfileReader);
                        //m_Xdoc.Load(profilepath);
                    }
                    else
                    {
                        Loaded = false;
                    }

                    if (m_Xdoc.SelectSingleNode(rootname) == null)
                    {
                        rootname = "DS4Windows";
                        missingSetting = true;
                    }

                    if (device < Global.MAX_DS4_CONTROLLER_COUNT)
                    {
                        DS4LightBar.forcelight[device] = false;
                        DS4LightBar.forcedFlash[device] = 0;
                    }

                    OutContType oldContType = Global.ActiveOutDevType[device];
                    LightbarSettingInfo lightbarSettings = lightbarSettingInfo[device];
                    LightbarDS4WinInfo lightInfo = lightbarSettings.ds4winSettings;

                    // Make sure to reset currently set profile values before parsing
                    ResetProfile(device);
                    ResetMouseProperties(device, control);

                    try { Item = m_Xdoc.SelectSingleNode("/" + rootname + "/touchToggle"); Boolean.TryParse(Item.InnerText, out EnableTouchToggle[device]); }
                    catch { missingSetting = true; }

                    try
                    {
                        Item = m_Xdoc.SelectSingleNode("/" + rootname + "/idleDisconnectTimeout");
                        if (int.TryParse(Item?.InnerText ?? "", out int temp))
                        {
                            IdleDisconnectTimeout[device] = temp;
                        }
                    }
                    catch { missingSetting = true; }

                    try { Item = m_Xdoc.SelectSingleNode("/" + rootname + "/outputDataToDS4"); Boolean.TryParse(Item.InnerText, out EnableOutputDataToDS4[device]); }
                    catch { missingSetting = true; }

                    try
                    {
                        Item = m_Xdoc.SelectSingleNode("/" + rootname + "/LightbarMode");
                        string tempMode = Item.InnerText;
                        lightbarSettings.mode = GetLightbarModeType(tempMode);
                    }
                    catch { lightbarSettings.mode = LightbarMode.DS4Win; missingSetting = true; }

                    //New method for saving color
                    try
                    {
                        Item = m_Xdoc.SelectSingleNode("/" + rootname + "/Color");
                        string[] colors;
                        if (!string.IsNullOrEmpty(Item.InnerText))
                            colors = Item.InnerText.Split(',');
                        else
                            colors = new string[0];

                        lightInfo.m_Led.red = byte.Parse(colors[0]);
                        lightInfo.m_Led.green = byte.Parse(colors[1]);
                        lightInfo.m_Led.blue = byte.Parse(colors[2]);
                    }
                    catch { missingSetting = true; }

                    if (m_Xdoc.SelectSingleNode("/" + rootname + "/Color") == null)
                    {
                        //Old method of color saving
                        try { Item = m_Xdoc.SelectSingleNode("/" + rootname + "/Red"); Byte.TryParse(Item.InnerText, out lightInfo.m_Led.red); }
                        catch { missingSetting = true; }
                        try { Item = m_Xdoc.SelectSingleNode("/" + rootname + "/Green"); Byte.TryParse(Item.InnerText, out lightInfo.m_Led.green); }
                        catch { missingSetting = true; }

                        try { Item = m_Xdoc.SelectSingleNode("/" + rootname + "/Blue"); Byte.TryParse(Item.InnerText, out lightInfo.m_Led.blue); }
                        catch { missingSetting = true; }
                    }

                    try
                    {
                        Item = m_Xdoc.SelectSingleNode("/" + rootname + "/RumbleBoost"); Byte.TryParse(Item.InnerText, out var value);
                        RumbleBoost[device] = value;
                    }
                    catch { missingSetting = true; }

                    try
                    {
                        Item = m_Xdoc.SelectSingleNode("/" + rootname + "/RumbleAutostopTime"); Int32.TryParse(Item.InnerText, out var value);
                        RumbleAutostopTime[device] = value;
                    }
                    catch { missingSetting = true; }

                    try { Item = m_Xdoc.SelectSingleNode("/" + rootname + "/ledAsBatteryIndicator"); Boolean.TryParse(Item.InnerText, out lightInfo.ledAsBattery); }
                    catch { missingSetting = true; }

                    try { Item = m_Xdoc.SelectSingleNode("/" + rootname + "/FlashType"); Byte.TryParse(Item.InnerText, out lightInfo.flashType); }
                    catch { missingSetting = true; }

                    try { Item = m_Xdoc.SelectSingleNode("/" + rootname + "/flashBatteryAt"); Int32.TryParse(Item.InnerText, out lightInfo.flashAt); }
                    catch { missingSetting = true; }

                    try
                    {
                        Item = m_Xdoc.SelectSingleNode("/" + rootname + "/touchSensitivity"); Byte.TryParse(Item.InnerText, out var value);
                        TouchSensitivity[device] = value;
                    }
                    catch { missingSetting = true; }

                    //New method for saving color
                    try
                    {
                        Item = m_Xdoc.SelectSingleNode("/" + rootname + "/LowColor");
                        string[] colors;
                        if (!string.IsNullOrEmpty(Item.InnerText))
                            colors = Item.InnerText.Split(',');
                        else
                            colors = new string[0];

                        lightInfo.m_LowLed.red = byte.Parse(colors[0]);
                        lightInfo.m_LowLed.green = byte.Parse(colors[1]);
                        lightInfo.m_LowLed.blue = byte.Parse(colors[2]);
                    }
                    catch { missingSetting = true; }

                    if (m_Xdoc.SelectSingleNode("/" + rootname + "/LowColor") == null)
                    {
                        //Old method of color saving
                        try { Item = m_Xdoc.SelectSingleNode("/" + rootname + "/LowRed"); byte.TryParse(Item.InnerText, out lightInfo.m_LowLed.red); }
                        catch { missingSetting = true; }
                        try { Item = m_Xdoc.SelectSingleNode("/" + rootname + "/LowGreen"); byte.TryParse(Item.InnerText, out lightInfo.m_LowLed.green); }
                        catch { missingSetting = true; }
                        try { Item = m_Xdoc.SelectSingleNode("/" + rootname + "/LowBlue"); byte.TryParse(Item.InnerText, out lightInfo.m_LowLed.blue); }
                        catch { missingSetting = true; }
                    }

                    //New method for saving color
                    try
                    {
                        Item = m_Xdoc.SelectSingleNode("/" + rootname + "/ChargingColor");
                        string[] colors;
                        if (!string.IsNullOrEmpty(Item.InnerText))
                            colors = Item.InnerText.Split(',');
                        else
                            colors = new string[0];

                        lightInfo.m_ChargingLed.red = byte.Parse(colors[0]);
                        lightInfo.m_ChargingLed.green = byte.Parse(colors[1]);
                        lightInfo.m_ChargingLed.blue = byte.Parse(colors[2]);
                    }
                    catch { missingSetting = true; }

                    if (m_Xdoc.SelectSingleNode("/" + rootname + "/ChargingColor") == null)
                    {
                        try { Item = m_Xdoc.SelectSingleNode("/" + rootname + "/ChargingRed"); Byte.TryParse(Item.InnerText, out lightInfo.m_ChargingLed.red); }
                        catch { missingSetting = true; }
                        try { Item = m_Xdoc.SelectSingleNode("/" + rootname + "/ChargingGreen"); Byte.TryParse(Item.InnerText, out lightInfo.m_ChargingLed.green); }
                        catch { missingSetting = true; }
                        try { Item = m_Xdoc.SelectSingleNode("/" + rootname + "/ChargingBlue"); Byte.TryParse(Item.InnerText, out lightInfo.m_ChargingLed.blue); }
                        catch { missingSetting = true; }
                    }

                    try
                    {
                        Item = m_Xdoc.SelectSingleNode("/" + rootname + "/FlashColor");
                        string[] colors;
                        if (!string.IsNullOrEmpty(Item.InnerText))
                            colors = Item.InnerText.Split(',');
                        else
                            colors = new string[0];
                        lightInfo.m_FlashLed.red = byte.Parse(colors[0]);
                        lightInfo.m_FlashLed.green = byte.Parse(colors[1]);
                        lightInfo.m_FlashLed.blue = byte.Parse(colors[2]);
                    }
                    catch { missingSetting = true; }

                    try { Item = m_Xdoc.SelectSingleNode("/" + rootname + "/touchpadJitterCompensation"); bool.TryParse(Item.InnerText, out TouchpadJitterCompensation[device]); }
                    catch { missingSetting = true; }

                    try
                    {
                        Item = m_Xdoc.SelectSingleNode("/" + rootname + "/lowerRCOn"); bool.TryParse(Item.InnerText, out var value);
                        LowerRCOn[device] = value;
                    }
                    catch { missingSetting = true; }
                    try { Item = m_Xdoc.SelectSingleNode("/" + rootname + "/tapSensitivity"); byte.TryParse(Item.InnerText, out tapSensitivity[device]); }
                    catch { missingSetting = true; }
                    try { Item = m_Xdoc.SelectSingleNode("/" + rootname + "/doubleTap"); bool.TryParse(Item.InnerText, out doubleTap[device]); }
                    catch { missingSetting = true; }
                    try { Item = m_Xdoc.SelectSingleNode("/" + rootname + "/scrollSensitivity"); int.TryParse(Item.InnerText, out scrollSensitivity[device]); }
                    catch { missingSetting = true; }

                    try { Item = m_Xdoc.SelectSingleNode("/" + rootname + "/TouchpadInvert"); int temp = 0; int.TryParse(Item.InnerText, out temp); touchpadInvert[device] = Math.Min(Math.Max(temp, 0), 3); }
                    catch { touchpadInvert[device] = 0; missingSetting = true; }

                    try { Item = m_Xdoc.SelectSingleNode("/" + rootname + "/TouchpadClickPassthru"); bool.TryParse(Item.InnerText, out TouchClickPassthru[device]); }
                    catch { missingSetting = true; }

                    try { Item = m_Xdoc.SelectSingleNode("/" + rootname + "/LeftTriggerMiddle"); byte.TryParse(Item.InnerText, out L2ModInfo[device].deadZone); }
                    catch { missingSetting = true; }
                    try { Item = m_Xdoc.SelectSingleNode("/" + rootname + "/RightTriggerMiddle"); byte.TryParse(Item.InnerText, out R2ModInfo[device].deadZone); }
                    catch { missingSetting = true; }

                    try { Item = m_Xdoc.SelectSingleNode("/" + rootname + "/L2AntiDeadZone"); int.TryParse(Item.InnerText, out L2ModInfo[device].antiDeadZone); }
                    catch { L2ModInfo[device].antiDeadZone = 0; missingSetting = true; }

                    try { Item = m_Xdoc.SelectSingleNode("/" + rootname + "/R2AntiDeadZone"); int.TryParse(Item.InnerText, out R2ModInfo[device].antiDeadZone); }
                    catch { R2ModInfo[device].antiDeadZone = 0; missingSetting = true; }

                    try
                    {
                        Item = m_Xdoc.SelectSingleNode("/" + rootname + "/L2MaxZone"); int temp = 100;
                        int.TryParse(Item.InnerText, out temp);
                        L2ModInfo[device].maxZone = Math.Min(Math.Max(temp, 0), 100);
                    }
                    catch { L2ModInfo[device].maxZone = 100; missingSetting = true; }

                    try
                    {
                        Item = m_Xdoc.SelectSingleNode("/" + rootname + "/R2MaxZone"); int temp = 100;
                        int.TryParse(Item.InnerText, out temp);
                        R2ModInfo[device].maxZone = Math.Min(Math.Max(temp, 0), 100);
                    }
                    catch { R2ModInfo[device].maxZone = 100; missingSetting = true; }

                    try
                    {
                        Item = m_Xdoc.SelectSingleNode("/" + rootname + "/L2MaxOutput"); double temp = 100.0;
                        temp = double.Parse(Item.InnerText);
                        L2ModInfo[device].maxOutput = Math.Min(Math.Max(temp, 0.0), 100.0);
                    }
                    catch { L2ModInfo[device].maxOutput = 100.0; missingSetting = true; }

                    try
                    {
                        Item = m_Xdoc.SelectSingleNode("/" + rootname + "/R2MaxOutput"); double temp = 100.0;
                        temp = double.Parse(Item.InnerText);
                        R2ModInfo[device].maxOutput = Math.Min(Math.Max(temp, 0.0), 100.0);
                    }
                    catch { R2ModInfo[device].maxOutput = 100.0; missingSetting = true; }

                    try
                    {
                        Item = m_Xdoc.SelectSingleNode("/" + rootname + "/LSRotation"); int temp = 0;
                        int.TryParse(Item.InnerText, out temp);
                        temp = Math.Min(Math.Max(temp, -180), 180);
                        LSRotation[device] = temp * Math.PI / 180.0;
                    }
                    catch { LSRotation[device] = 0.0; missingSetting = true; }

                    try
                    {
                        Item = m_Xdoc.SelectSingleNode("/" + rootname + "/RSRotation"); int temp = 0;
                        int.TryParse(Item.InnerText, out temp);
                        temp = Math.Min(Math.Max(temp, -180), 180);
                        RSRotation[device] = temp * Math.PI / 180.0;
                    }
                    catch { RSRotation[device] = 0.0; missingSetting = true; }

                    try
                    {
                        Item = m_Xdoc.SelectSingleNode("/" + rootname + "/LSFuzz"); int temp = 0;
                        int.TryParse(Item.InnerText, out temp);
                        temp = Math.Min(Math.Max(temp, 0), 100);
                        LSModInfo[device].fuzz = temp;
                    }
                    catch { LSModInfo[device].fuzz = StickDeadZoneInfo.DEFAULT_FUZZ; missingSetting = true; }

                    try
                    {
                        Item = m_Xdoc.SelectSingleNode("/" + rootname + "/RSFuzz"); int temp = 0;
                        int.TryParse(Item.InnerText, out temp);
                        temp = Math.Min(Math.Max(temp, 0), 100);
                        RSModInfo[device].fuzz = temp;
                    }
                    catch { RSModInfo[device].fuzz = StickDeadZoneInfo.DEFAULT_FUZZ; missingSetting = true; }

                    try { Item = m_Xdoc.SelectSingleNode("/" + rootname + "/ButtonMouseSensitivity"); int.TryParse(Item.InnerText, out ButtonMouseInfos[device].buttonSensitivity); }
                    catch { missingSetting = true; }

                    try { Item = m_Xdoc.SelectSingleNode("/" + rootname + "/ButtonMouseOffset"); double.TryParse(Item.InnerText, out ButtonMouseInfos[device].mouseVelocityOffset); }
                    catch { missingSetting = true; }

                    try
                    {
                        Item = m_Xdoc.SelectSingleNode("/" + rootname + "/ButtonMouseVerticalScale");
                        int.TryParse(Item.InnerText, out int temp);
                        ButtonMouseInfos[device].buttonVerticalScale = Math.Min(Math.Max(temp, 0), 500) * 0.01;
                    }
                    catch { missingSetting = true; }

                    try { Item = m_Xdoc.SelectSingleNode("/" + rootname + "/Rainbow"); double.TryParse(Item.InnerText, out lightInfo.rainbow); }
                    catch { lightInfo.rainbow = 0; missingSetting = true; }
                    try
                    {
                        Item = m_Xdoc.SelectSingleNode("/" + rootname + "/MaxSatRainbow");
                        int.TryParse(Item.InnerText, out int temp);
                        lightInfo.maxRainbowSat = Math.Max(0, Math.Min(100, temp)) / 100.0;
                    }
                    catch { lightInfo.maxRainbowSat = 1.0; missingSetting = true; }

                    try
                    {
                        Item = m_Xdoc.SelectSingleNode("/" + rootname + "/LSDeadZone");
                        int.TryParse(Item.InnerText, out int temp);
                        temp = Math.Min(Math.Max(temp, 0), 127);
                        LSModInfo[device].deadZone = temp;
                    }
                    catch
                    {
                        LSModInfo[device].deadZone = 10; missingSetting = true;
                    }

                    try
                    {
                        Item = m_Xdoc.SelectSingleNode("/" + rootname + "/RSDeadZone");
                        int.TryParse(Item.InnerText, out int temp);
                        temp = Math.Min(Math.Max(temp, 0), 127);
                        RSModInfo[device].deadZone = temp;
                    }
                    catch
                    {
                        RSModInfo[device].deadZone = 10; missingSetting = true;
                    }

                    try { Item = m_Xdoc.SelectSingleNode("/" + rootname + "/LSAntiDeadZone"); int.TryParse(Item.InnerText, out LSModInfo[device].antiDeadZone); }
                    catch { LSModInfo[device].antiDeadZone = 20; missingSetting = true; }
                    try { Item = m_Xdoc.SelectSingleNode("/" + rootname + "/RSAntiDeadZone"); int.TryParse(Item.InnerText, out RSModInfo[device].antiDeadZone); }
                    catch { RSModInfo[device].antiDeadZone = 20; missingSetting = true; }

                    try
                    {
                        Item = m_Xdoc.SelectSingleNode("/" + rootname + "/LSMaxZone"); int temp = 100;
                        int.TryParse(Item.InnerText, out temp);
                        LSModInfo[device].maxZone = Math.Min(Math.Max(temp, 0), 100);
                    }
                    catch { LSModInfo[device].maxZone = 100; missingSetting = true; }

                    try
                    {
                        Item = m_Xdoc.SelectSingleNode("/" + rootname + "/RSMaxZone"); int temp = 100;
                        int.TryParse(Item.InnerText, out temp);
                        RSModInfo[device].maxZone = Math.Min(Math.Max(temp, 0), 100);
                    }
                    catch { RSModInfo[device].maxZone = 100; missingSetting = true; }

                    try
                    {
                        Item = m_Xdoc.SelectSingleNode("/" + rootname + "/LSVerticalScale");
                        if (double.TryParse(Item?.InnerText ?? "", out double temp))
                        {
                            LSModInfo[device].verticalScale = Math.Min(Math.Max(temp, 0.0), 200.0);
                        }
                    }
                    catch { LSModInfo[device].verticalScale = StickDeadZoneInfo.DEFAULT_VERTICAL_SCALE; }

                    try
                    {
                        Item = m_Xdoc.SelectSingleNode("/" + rootname + "/LSMaxOutput"); double temp = 100.0;
                        temp = double.Parse(Item.InnerText);
                        LSModInfo[device].maxOutput = Math.Min(Math.Max(temp, 0.0), 100.0);
                    }
                    catch { LSModInfo[device].maxOutput = 100.0; missingSetting = true; }

                    try
                    {
                        Item = m_Xdoc.SelectSingleNode("/" + rootname + "/LSMaxOutputForce");
                        if (bool.TryParse(Item?.InnerText ?? "", out bool temp))
                        {
                            LSModInfo[device].maxOutputForce = temp;
                        }
                    }
                    catch { LSModInfo[device].maxOutputForce = StickDeadZoneInfo.DEFAULT_MAXOUTPUT_FORCE; missingSetting = true; }

                    try
                    {
                        Item = m_Xdoc.SelectSingleNode("/" + rootname + "/RSMaxOutput"); double temp = 100.0;
                        temp = double.Parse(Item.InnerText);
                        RSModInfo[device].maxOutput = Math.Min(Math.Max(temp, 0.0), 100.0);
                    }
                    catch { RSModInfo[device].maxOutput = 100; missingSetting = true; }

                    try
                    {
                        Item = m_Xdoc.SelectSingleNode("/" + rootname + "/RSMaxOutputForce");
                        if (bool.TryParse(Item?.InnerText ?? "", out bool temp))
                        {
                            RSModInfo[device].maxOutputForce = temp;
                        }
                    }
                    catch { RSModInfo[device].maxOutputForce = StickDeadZoneInfo.DEFAULT_MAXOUTPUT_FORCE; missingSetting = true; }

                    try
                    {
                        Item = m_Xdoc.SelectSingleNode("/" + rootname + "/LSOuterBindDead");
                        if (int.TryParse(Item?.InnerText ?? "", out int temp))
                        {
                            temp = Math.Min(Math.Max(temp, 0), 100);
                            LSModInfo[device].outerBindDeadZone = temp;
                        }
                    }
                    catch { LSModInfo[device].outerBindDeadZone = StickDeadZoneInfo.DEFAULT_OUTER_BIND_DEAD; }

                    try
                    {
                        Item = m_Xdoc.SelectSingleNode("/" + rootname + "/RSOuterBindDead");
                        if (int.TryParse(Item?.InnerText ?? "", out int temp))
                        {
                            temp = Math.Min(Math.Max(temp, 0), 100);
                            RSModInfo[device].outerBindDeadZone = temp;
                        }
                    }
                    catch { RSModInfo[device].outerBindDeadZone = StickDeadZoneInfo.DEFAULT_OUTER_BIND_DEAD; }

                    try
                    {
                        Item = m_Xdoc.SelectSingleNode("/" + rootname + "/LSOuterBindInvert");
                        if (bool.TryParse(Item?.InnerText ?? "", out bool temp))
                        {
                            LSModInfo[device].outerBindInvert = temp;
                        }
                    }
                    catch { LSModInfo[device].outerBindInvert = StickDeadZoneInfo.DEFAULT_OUTER_BIND_INVERT; }

                    try
                    {
                        Item = m_Xdoc.SelectSingleNode("/" + rootname + "/RSOuterBindInvert");
                        if (bool.TryParse(Item?.InnerText ?? "", out bool temp))
                        {
                            RSModInfo[device].outerBindInvert = temp;
                        }
                    }
                    catch { RSModInfo[device].outerBindInvert = StickDeadZoneInfo.DEFAULT_OUTER_BIND_INVERT; }

                    try
                    {
                        Item = m_Xdoc.SelectSingleNode("/" + rootname + "/RSVerticalScale");
                        if (double.TryParse(Item?.InnerText ?? "", out double temp))
                        {
                            RSModInfo[device].verticalScale = Math.Min(Math.Max(temp, 0.0), 200.0);
                        }
                    }
                    catch { RSModInfo[device].verticalScale = StickDeadZoneInfo.DEFAULT_VERTICAL_SCALE; }

                    try
                    {
                        Item = m_Xdoc.SelectSingleNode("/" + rootname + "/LSDeadZoneType");
                        if (Enum.TryParse(Item?.InnerText ?? "", out StickDeadZoneInfo.DeadZoneType temp))
                        {
                            LSModInfo[device].deadzoneType = temp;
                        }
                    }
                    catch { }

                    bool lsAxialDeadGroup = false;
                    XmlNode lsAxialDeadElement =
                        m_Xdoc.SelectSingleNode("/" + rootname + "/LSAxialDeadOptions");
                    lsAxialDeadGroup = lsAxialDeadElement != null;

                    if (lsAxialDeadGroup)
                    {
                        try
                        {
                            Item = lsAxialDeadElement.SelectSingleNode("DeadZoneX");
                            int.TryParse(Item.InnerText, out int temp);
                            temp = Math.Min(Math.Max(temp, 0), 127);
                            LSModInfo[device].xAxisDeadInfo.deadZone = temp;
                        }
                        catch { }

                        try
                        {
                            Item = lsAxialDeadElement.SelectSingleNode("DeadZoneY");
                            int.TryParse(Item.InnerText, out int temp);
                            temp = Math.Min(Math.Max(temp, 0), 127);
                            LSModInfo[device].yAxisDeadInfo.deadZone = temp;
                        }
                        catch { }

                        try
                        {
                            Item = lsAxialDeadElement.SelectSingleNode("MaxZoneX");
                            int.TryParse(Item.InnerText, out int temp);
                            LSModInfo[device].xAxisDeadInfo.maxZone = Math.Min(Math.Max(temp, 0), 100);
                        }
                        catch { }

                        try
                        {
                            Item = lsAxialDeadElement.SelectSingleNode("MaxZoneY");
                            int.TryParse(Item.InnerText, out int temp);
                            LSModInfo[device].yAxisDeadInfo.maxZone = Math.Min(Math.Max(temp, 0), 100);
                        }
                        catch { }

                        try
                        {
                            Item = lsAxialDeadElement.SelectSingleNode("AntiDeadZoneX");
                            int.TryParse(Item.InnerText, out int temp);
                            LSModInfo[device].xAxisDeadInfo.antiDeadZone = Math.Min(Math.Max(temp, 0), 100);
                        }
                        catch { }

                        try
                        {
                            Item = lsAxialDeadElement.SelectSingleNode("AntiDeadZoneY");
                            int.TryParse(Item.InnerText, out int temp);
                            LSModInfo[device].yAxisDeadInfo.antiDeadZone = Math.Min(Math.Max(temp, 0), 100);
                        }
                        catch { }

                        try
                        {
                            Item = lsAxialDeadElement.SelectSingleNode("MaxOutputX");
                            double.TryParse(Item.InnerText, out double temp);
                            LSModInfo[device].xAxisDeadInfo.maxOutput = Math.Min(Math.Max(temp, 0.0), 100.0);
                        }
                        catch { }

                        try
                        {
                            Item = lsAxialDeadElement.SelectSingleNode("MaxOutputY");
                            double.TryParse(Item.InnerText, out double temp);
                            LSModInfo[device].yAxisDeadInfo.maxOutput = Math.Min(Math.Max(temp, 0.0), 100.0);
                        }
                        catch { }
                    }

                    try
                    {
                        Item = m_Xdoc.SelectSingleNode("/" + rootname + "/RSDeadZoneType");
                        if (Enum.TryParse(Item?.InnerText ?? "", out StickDeadZoneInfo.DeadZoneType temp))
                        {
                            RSModInfo[device].deadzoneType = temp;
                        }
                    }
                    catch { }

                    bool rsAxialDeadGroup = false;
                    XmlNode rsAxialDeadElement =
                        m_Xdoc.SelectSingleNode("/" + rootname + "/RSAxialDeadOptions");
                    rsAxialDeadGroup = rsAxialDeadElement != null;

                    if (rsAxialDeadGroup)
                    {
                        try
                        {
                            Item = rsAxialDeadElement.SelectSingleNode("DeadZoneX");
                            int.TryParse(Item.InnerText, out int temp);
                            temp = Math.Min(Math.Max(temp, 0), 127);
                            RSModInfo[device].xAxisDeadInfo.deadZone = temp;
                        }
                        catch { }

                        try
                        {
                            Item = rsAxialDeadElement.SelectSingleNode("DeadZoneY");
                            int.TryParse(Item.InnerText, out int temp);
                            temp = Math.Min(Math.Max(temp, 0), 127);
                            RSModInfo[device].yAxisDeadInfo.deadZone = temp;
                        }
                        catch { }

                        try
                        {
                            Item = rsAxialDeadElement.SelectSingleNode("MaxZoneX");
                            int.TryParse(Item.InnerText, out int temp);
                            RSModInfo[device].xAxisDeadInfo.maxZone = Math.Min(Math.Max(temp, 0), 100);
                        }
                        catch { }

                        try
                        {
                            Item = rsAxialDeadElement.SelectSingleNode("MaxZoneY");
                            int.TryParse(Item.InnerText, out int temp);
                            RSModInfo[device].yAxisDeadInfo.maxZone = Math.Min(Math.Max(temp, 0), 100);
                        }
                        catch { }

                        try
                        {
                            Item = rsAxialDeadElement.SelectSingleNode("AntiDeadZoneX");
                            int.TryParse(Item.InnerText, out int temp);
                            RSModInfo[device].xAxisDeadInfo.antiDeadZone = Math.Min(Math.Max(temp, 0), 100);
                        }
                        catch { }

                        try
                        {
                            Item = rsAxialDeadElement.SelectSingleNode("AntiDeadZoneY");
                            int.TryParse(Item.InnerText, out int temp);
                            RSModInfo[device].yAxisDeadInfo.antiDeadZone = Math.Min(Math.Max(temp, 0), 100);
                        }
                        catch { }

                        try
                        {
                            Item = rsAxialDeadElement.SelectSingleNode("MaxOutputX");
                            double.TryParse(Item.InnerText, out double temp);
                            RSModInfo[device].xAxisDeadInfo.maxOutput = Math.Min(Math.Max(temp, 0.0), 100.0);
                        }
                        catch { }

                        try
                        {
                            Item = rsAxialDeadElement.SelectSingleNode("MaxOutputY");
                            double.TryParse(Item.InnerText, out double temp);
                            RSModInfo[device].yAxisDeadInfo.maxOutput = Math.Min(Math.Max(temp, 0.0), 100.0);
                        }
                        catch { }
                    }

                    try
                    {
                        Item = m_Xdoc.SelectSingleNode("/" + rootname + "/SXDeadZone"); double.TryParse(Item.InnerText, out var value);
                        SXDeadzone[device] = value;
                    }
                    catch { SXDeadzone[device] = 0.02; missingSetting = true; }

                    try
                    {
                        Item = m_Xdoc.SelectSingleNode("/" + rootname + "/SZDeadZone"); double.TryParse(Item.InnerText, out var value);
                        SZDeadzone[device] = value;
                    }
                    catch { SZDeadzone[device] = 0.02; missingSetting = true; }

                    try
                    {
                        Item = m_Xdoc.SelectSingleNode("/" + rootname + "/SXMaxZone");
                        int temp = 0;
                        int.TryParse(Item.InnerText, out temp);
                        SXMaxzone[device] = Math.Min(Math.Max(temp * 0.01, 0.0), 1.0);
                    }
                    catch { SXMaxzone[device] = 1.0; missingSetting = true; }

                    try
                    {
                        Item = m_Xdoc.SelectSingleNode("/" + rootname + "/SZMaxZone");
                        int temp = 0;
                        int.TryParse(Item.InnerText, out temp);
                        SZMaxzone[device] = Math.Min(Math.Max(temp * 0.01, 0.0), 1.0);
                    }
                    catch { SZMaxzone[device] = 1.0; missingSetting = true; }

                    try
                    {
                        Item = m_Xdoc.SelectSingleNode("/" + rootname + "/SXAntiDeadZone");
                        int temp = 0;
                        int.TryParse(Item.InnerText, out temp);
                        SXAntiDeadzone[device] = Math.Min(Math.Max(temp * 0.01, 0.0), 1.0);
                    }
                    catch { SXAntiDeadzone[device] = 0.0; missingSetting = true; }

                    try
                    {
                        Item = m_Xdoc.SelectSingleNode("/" + rootname + "/SZAntiDeadZone");
                        int temp = 0;
                        int.TryParse(Item.InnerText, out temp);
                        SZAntiDeadzone[device] = Math.Min(Math.Max(temp * 0.01, 0.0), 1.0);
                    }
                    catch { SZAntiDeadzone[device] = 0.0; missingSetting = true; }

                    try
                    {
                        Item = m_Xdoc.SelectSingleNode("/" + rootname + "/Sensitivity");
                        string[] s = Item.InnerText.Split('|');
                        if (s.Length == 1)
                            s = Item.InnerText.Split(',');
                        if (!double.TryParse(s[0], out var v1) || v1 < .5f)
                            LSSens[device] = 1;
                        LSSens[device] = v1;
                        if (!double.TryParse(s[1], out var v2) || v2 < .5f)
                            RSSens[device] = 1;
                        RSSens[device] = v2;
                        if (!double.TryParse(s[2], out var v3) || v3 < .1f)
                            L2Sens[device] = 1;
                        L2Sens[device] = v3;
                        if (!double.TryParse(s[3], out var v4) || v4 < .1f)
                            R2Sens[device] = 1;
                        R2Sens[device] = v4;
                        if (!double.TryParse(s[4], out var v5) || v5 < .5f)
                            SXSens[device] = 1;
                        SXSens[device] = v5;
                        if (!double.TryParse(s[5], out var v6) || v6 < .5f)
                            SZSens[device] = 1;
                        SZSens[device] = v6;
                    }
                    catch { missingSetting = true; }

                    try { Item = m_Xdoc.SelectSingleNode("/" + rootname + "/ChargingType"); int.TryParse(Item.InnerText, out lightInfo.chargingType); }
                    catch { missingSetting = true; }
                    try { Item = m_Xdoc.SelectSingleNode("/" + rootname + "/MouseAcceleration"); bool.TryParse(Item.InnerText, out ButtonMouseInfos[device].mouseAccel); }
                    catch { missingSetting = true; }

                    int shiftM = 0;
                    if (m_Xdoc.SelectSingleNode("/" + rootname + "/ShiftModifier") != null)
                        int.TryParse(m_Xdoc.SelectSingleNode("/" + rootname + "/ShiftModifier").InnerText, out shiftM);

                    try
                    {
                        Item = m_Xdoc.SelectSingleNode("/" + rootname + "/LaunchProgram");
                        LaunchProgram[device] = Item.InnerText;
                    }
                    catch { LaunchProgram[device] = string.Empty; missingSetting = true; }

                    if (launchprogram == true && LaunchProgram[device] != string.Empty)
                    {
                        string programPath = LaunchProgram[device];
                        System.Diagnostics.Process[] localAll = System.Diagnostics.Process.GetProcesses();
                        bool procFound = false;
                        for (int procInd = 0, procsLen = localAll.Length; !procFound && procInd < procsLen; procInd++)
                        {
                            try
                            {
                                string temp = localAll[procInd].MainModule.FileName;
                                if (temp == programPath)
                                {
                                    procFound = true;
                                }
                            }
                            // Ignore any process for which this information
                            // is not exposed
                            catch { }
                        }

                        if (!procFound)
                        {
                            Task processTask = new Task(() =>
                            {
                                Thread.Sleep(5000);
                                System.Diagnostics.Process tempProcess = new System.Diagnostics.Process();
                                tempProcess.StartInfo.FileName = programPath;
                                tempProcess.StartInfo.WorkingDirectory = new FileInfo(programPath).Directory.ToString();
                                //tempProcess.StartInfo.UseShellExecute = false;
                                try { tempProcess.Start(); }
                                catch { }
                            });

                            processTask.Start();
                        }
                    }

                    try
                    {
                        Item = m_Xdoc.SelectSingleNode("/" + rootname + "/DinputOnly");
                        bool.TryParse(Item.InnerText, out dinputOnly[device]);
                    }
                    catch { dinputOnly[device] = false; missingSetting = true; }

                    bool oldUseDInputOnly = Global.UseDirectInputOnly[device];

                    try
                    {
                        Item = m_Xdoc.SelectSingleNode("/" + rootname + "/StartTouchpadOff");
                        bool.TryParse(Item.InnerText, out startTouchpadOff[device]);
                        if (startTouchpadOff[device] == true) control.StartTPOff(device);
                    }
                    catch { startTouchpadOff[device] = false; missingSetting = true; }

                    // Fallback lookup if TouchpadOutMode is not set
                    bool tpForControlsPresent = false;
                    XmlNode xmlUseTPForControlsElement =
                        m_Xdoc.SelectSingleNode("/" + rootname + "/UseTPforControls");
                    tpForControlsPresent = xmlUseTPForControlsElement != null;
                    if (tpForControlsPresent)
                    {
                        try
                        {
                            Item = m_Xdoc.SelectSingleNode("/" + rootname + "/UseTPforControls");
                            if (bool.TryParse(Item?.InnerText ?? "", out bool temp))
                            {
                                if (temp) touchOutMode[device] = TouchpadOutMode.Controls;
                            }
                        }
                        catch { touchOutMode[device] = TouchpadOutMode.Mouse; }
                    }

                    // Fallback lookup if GyroOutMode is not set
                    try
                    {
                        Item = m_Xdoc.SelectSingleNode("/" + rootname + "/UseSAforMouse");
                        if (bool.TryParse(Item?.InnerText ?? "", out bool temp))
                        {
                            if (temp) gyroOutMode[device] = GyroOutMode.Mouse;
                        }
                    }
                    catch { gyroOutMode[device] = GyroOutMode.Controls; }

                    try { Item = m_Xdoc.SelectSingleNode("/" + rootname + "/SATriggers"); sATriggers[device] = Item.InnerText; }
                    catch { sATriggers[device] = "-1"; missingSetting = true; }

                    try { Item = m_Xdoc.SelectSingleNode("/" + rootname + "/SATriggerCond"); sATriggerCond[device] = SaTriggerCondValue(Item.InnerText); }
                    catch { sATriggerCond[device] = true; missingSetting = true; }

                    try { Item = m_Xdoc.SelectSingleNode("/" + rootname + "/SASteeringWheelEmulationAxis"); SASteeringWheelEmulationAxisType.TryParse(Item.InnerText, out sASteeringWheelEmulationAxis[device]); }
                    catch { sASteeringWheelEmulationAxis[device] = SASteeringWheelEmulationAxisType.None; missingSetting = true; }

                    try { Item = m_Xdoc.SelectSingleNode("/" + rootname + "/SASteeringWheelEmulationRange"); int.TryParse(Item.InnerText, out sASteeringWheelEmulationRange[device]); }
                    catch { sASteeringWheelEmulationRange[device] = 360; missingSetting = true; }

                    bool sASteeringWheelSmoothingGroup = false;
                    XmlNode xmlSASteeringWheelSmoothElement =
                        m_Xdoc.SelectSingleNode("/" + rootname + "/SASteeringWheelSmoothingOptions");
                    sASteeringWheelSmoothingGroup = xmlSASteeringWheelSmoothElement != null;

                    if (sASteeringWheelSmoothingGroup)
                    {
                        try
                        {
                            Item = xmlSASteeringWheelSmoothElement.SelectSingleNode("SASteeringWheelUseSmoothing");
                            //Item = m_Xdoc.SelectSingleNode("/" + rootname + "/SASteeringWheelUseSmoothing");
                            bool.TryParse(Item.InnerText, out bool temp);
                            WheelSmoothInfo[device].Enabled = temp;
                        }
                        catch { WheelSmoothInfo[device].Reset(); missingSetting = true; }

                        try
                        {
                            Item = xmlSASteeringWheelSmoothElement.SelectSingleNode("SASteeringWheelSmoothMinCutoff");
                            //Item = m_Xdoc.SelectSingleNode("/" + rootname + "/SASteeringWheelSmoothMinCutoff");
                            double.TryParse(Item.InnerText, out double temp);
                            WheelSmoothInfo[device].MinCutoff = temp;
                        }
                        catch { WheelSmoothInfo[device].MinCutoff = OneEuroFilterPair.DEFAULT_WHEEL_CUTOFF; missingSetting = true; }

                        try
                        {
                            Item = xmlSASteeringWheelSmoothElement.SelectSingleNode("SASteeringWheelSmoothBeta");
                            //Item = m_Xdoc.SelectSingleNode("/" + rootname + "/SASteeringWheelSmoothBeta");
                            double.TryParse(Item.InnerText, out double temp);
                            WheelSmoothInfo[device].Beta = temp;
                        }
                        catch { WheelSmoothInfo[device].Beta = OneEuroFilterPair.DEFAULT_WHEEL_BETA; missingSetting = true; }
                    }

                    try
                    {
                        Item = m_Xdoc.SelectSingleNode("/" + rootname + "/SASteeringWheelFuzz");
                        int.TryParse(Item.InnerText, out int temp);
                        saWheelFuzzValues[device] = temp >= 0 && temp <= 100 ? temp : 0;
                    }
                    catch { saWheelFuzzValues[device] = 0; missingSetting = true; }

                    try
                    {
                        Item = m_Xdoc.SelectSingleNode("/" + rootname + "/GyroOutputMode");
                        string tempMode = Item.InnerText;
                        //gyroOutMode[device] = GetGyroOutModeType(tempMode);
                        Enum.TryParse(tempMode, out gyroOutMode[device]);
                    }
                    catch { PortOldGyroSettings(device); missingSetting = true; }

                    XmlNode xmlGyroControlsElement =
                        m_Xdoc.SelectSingleNode("/" + rootname + "/GyroControlsSettings");
                    if (xmlGyroControlsElement != null)
                    {
                        try
                        {
                            Item = xmlGyroControlsElement.SelectSingleNode("Triggers");
                            if (Item != null)
                            {
                                gyroControlsInf[device].triggers = Item.InnerText;
                            }
                        }
                        catch { }

                        try
                        {
                            Item = xmlGyroControlsElement.SelectSingleNode("TriggerCond");
                            if (Item != null)
                            {
                                gyroControlsInf[device].triggerCond = SaTriggerCondValue(Item.InnerText);
                            }
                        }
                        catch { }

                        try
                        {
                            Item = xmlGyroControlsElement.SelectSingleNode("TriggerTurns");
                            if (bool.TryParse(Item?.InnerText ?? "", out bool tempTurns))
                            {
                                gyroControlsInf[device].triggerTurns = tempTurns;
                            }
                        }
                        catch { }

                        try
                        {
                            Item = xmlGyroControlsElement.SelectSingleNode("Toggle");
                            if (bool.TryParse(Item?.InnerText ?? "", out bool tempToggle))
                            {
                                SetGyroControlsToggle(device, tempToggle, control);
                            }
                        }
                        catch { }
                    }

                    try { Item = m_Xdoc.SelectSingleNode("/" + rootname + "/GyroMouseStickTriggers"); sAMouseStickTriggers[device] = Item.InnerText; }
                    catch { sAMouseStickTriggers[device] = "-1"; missingSetting = true; }

                    try { Item = m_Xdoc.SelectSingleNode("/" + rootname + "/GyroMouseStickTriggerCond"); sAMouseStickTriggerCond[device] = SaTriggerCondValue(Item.InnerText); }
                    catch { sAMouseStickTriggerCond[device] = true; missingSetting = true; }

                    try { Item = m_Xdoc.SelectSingleNode("/" + rootname + "/GyroMouseStickTriggerTurns"); bool.TryParse(Item.InnerText, out gyroMouseStickTriggerTurns[device]); }
                    catch { gyroMouseStickTriggerTurns[device] = true; missingSetting = true; }

                    try { Item = m_Xdoc.SelectSingleNode("/" + rootname + "/GyroMouseStickHAxis"); int temp = 0; int.TryParse(Item.InnerText, out temp); gyroMouseStickHorizontalAxis[device] = Math.Min(Math.Max(0, temp), 1); }
                    catch { gyroMouseStickHorizontalAxis[device] = 0; missingSetting = true; }

                    try
                    {
                        Item = m_Xdoc.SelectSingleNode("/" + rootname + "/GyroMouseStickDeadZone"); int.TryParse(Item.InnerText, out int temp);
                        gyroMStickInfo[device].deadZone = temp;
                    }
                    catch { gyroMStickInfo[device].deadZone = 30; missingSetting = true; }

                    try
                    {
                        Item = m_Xdoc.SelectSingleNode("/" + rootname + "/GyroMouseStickMaxZone"); int.TryParse(Item.InnerText, out int temp);
                        gyroMStickInfo[device].maxZone = Math.Max(temp, 1);
                    }
                    catch { gyroMStickInfo[device].maxZone = 830; missingSetting = true; }

                    try
                    {
                        Item = m_Xdoc.SelectSingleNode("/" + rootname + "/GyroMouseStickOutputStick");
                        if (Enum.TryParse(Item?.InnerText ?? "", out GyroMouseStickInfo.OutputStick temp))
                        {
                            gyroMStickInfo[device].outputStick = temp;
                        }
                    }
                    catch { }

                    try
                    {
                        Item = m_Xdoc.SelectSingleNode("/" + rootname + "/GyroMouseStickOutputStickAxes");
                        if (Enum.TryParse(Item?.InnerText ?? "", out GyroMouseStickInfo.OutputStickAxes temp))
                        {
                            gyroMStickInfo[device].outputStickDir = temp;
                        }
                    }
                    catch { }

                    try
                    {
                        Item = m_Xdoc.SelectSingleNode("/" + rootname + "/GyroMouseStickAntiDeadX"); double.TryParse(Item.InnerText, out double temp);
                        gyroMStickInfo[device].antiDeadX = temp;
                    }
                    catch { gyroMStickInfo[device].antiDeadX = 0.4; missingSetting = true; }

                    try
                    {
                        Item = m_Xdoc.SelectSingleNode("/" + rootname + "/GyroMouseStickAntiDeadY"); double.TryParse(Item.InnerText, out double temp);
                        gyroMStickInfo[device].antiDeadY = temp;
                    }
                    catch { gyroMStickInfo[device].antiDeadY = 0.4; missingSetting = true; }

                    try { Item = m_Xdoc.SelectSingleNode("/" + rootname + "/GyroMouseStickInvert"); uint.TryParse(Item.InnerText, out gyroMStickInfo[device].inverted); }
                    catch { gyroMStickInfo[device].inverted = 0; missingSetting = true; }

                    try
                    {
                        Item = m_Xdoc.SelectSingleNode("/" + rootname + "/GyroMouseStickToggle");
                        bool.TryParse(Item.InnerText, out bool temp);
                        SetGyroMouseStickToggle(device, temp, control);
                    }
                    catch
                    {
                        SetGyroMouseStickToggle(device, false, control);
                        missingSetting = true;
                    }

                    try
                    {
                        Item = m_Xdoc.SelectSingleNode("/" + rootname + "/GyroMouseStickMaxOutput"); double temp = 100.0;
                        temp = double.Parse(Item.InnerText);
                        gyroMStickInfo[device].maxOutput = Math.Min(Math.Max(temp, 0.0), 100.0);
                    }
                    catch { gyroMStickInfo[device].maxOutput = 100.0; missingSetting = true; }

                    try
                    {
                        Item = m_Xdoc.SelectSingleNode("/" + rootname + "/GyroMouseStickMaxOutputEnabled");
                        bool.TryParse(Item.InnerText, out bool temp);
                        gyroMStickInfo[device].maxOutputEnabled = temp;
                    }
                    catch { gyroMStickInfo[device].maxOutputEnabled = false; missingSetting = true; }

                    try { Item = m_Xdoc.SelectSingleNode("/" + rootname + "/GyroMouseStickVerticalScale"); int.TryParse(Item.InnerText, out gyroMStickInfo[device].vertScale); }
                    catch { gyroMStickInfo[device].vertScale = 100; missingSetting = true; }

                    bool gyroMStickSmoothingGroup = false;
                    XmlNode xmlGyroMStickSmoothingElement =
                        m_Xdoc.SelectSingleNode("/" + rootname + "/GyroMouseStickSmoothingSettings");
                    gyroMStickSmoothingGroup = xmlGyroMStickSmoothingElement != null;

                    if (gyroMStickSmoothingGroup)
                    {
                        try
                        {
                            Item = xmlGyroMStickSmoothingElement.SelectSingleNode("UseSmoothing");
                            bool.TryParse(Item.InnerText, out bool tempSmoothing);
                            gyroMStickInfo[device].useSmoothing = tempSmoothing;
                        }
                        catch { gyroMStickInfo[device].useSmoothing = false; missingSetting = true; }

                        try
                        {
                            Item = xmlGyroMStickSmoothingElement.SelectSingleNode("SmoothingMethod");
                            string temp = Item?.InnerText ?? GyroMouseStickInfo.DEFAULT_SMOOTH_TECHNIQUE;
                            gyroMStickInfo[device].DetermineSmoothMethod(temp);
                        }
                        catch { gyroMStickInfo[device].ResetSmoothing(); missingSetting = true; }

                        try
                        {
                            Item = xmlGyroMStickSmoothingElement.SelectSingleNode("SmoothingWeight");
                            int temp = 0; int.TryParse(Item.InnerText, out temp);
                            gyroMStickInfo[device].smoothWeight = Math.Min(Math.Max(0.0, Convert.ToDouble(temp * 0.01)), 1.0);
                        }
                        catch { gyroMStickInfo[device].smoothWeight = 0.5; missingSetting = true; }

                        try
                        {
                            Item = xmlGyroMStickSmoothingElement.SelectSingleNode("SmoothingMinCutoff");
                            double.TryParse(Item.InnerText, out double temp);
                            gyroMStickInfo[device].minCutoff = Math.Min(Math.Max(0.0, temp), 100.0);
                        }
                        catch { gyroMStickInfo[device].minCutoff = GyroMouseStickInfo.DEFAULT_MINCUTOFF; missingSetting = true; }

                        try
                        {
                            Item = xmlGyroMStickSmoothingElement.SelectSingleNode("SmoothingBeta");
                            double.TryParse(Item.InnerText, out double temp);
                            gyroMStickInfo[device].beta = Math.Min(Math.Max(0.0, temp), 1.0);
                        }
                        catch { gyroMStickInfo[device].beta = GyroMouseStickInfo.DEFAULT_BETA; missingSetting = true; }
                    }
                    else
                    {
                        missingSetting = true;
                    }

                    XmlNode xmlGyroSwipeElement =
                        m_Xdoc.SelectSingleNode("/" + rootname + "/GyroSwipeSettings");
                    if (xmlGyroSwipeElement != null)
                    {
                        try
                        {
                            Item = xmlGyroSwipeElement.SelectSingleNode("DeadZoneX");
                            if (int.TryParse(Item?.InnerText ?? "", out int tempDead))
                            {
                                gyroSwipeInfo[device].deadzoneX = tempDead;
                            }
                        }
                        catch { }

                        try
                        {
                            Item = xmlGyroSwipeElement.SelectSingleNode("DeadZoneY");
                            if (int.TryParse(Item?.InnerText ?? "", out int tempDead))
                            {
                                gyroSwipeInfo[device].deadzoneY = tempDead;
                            }
                        }
                        catch { }

                        try
                        {
                            Item = xmlGyroSwipeElement.SelectSingleNode("Triggers");
                            if (Item != null)
                            {
                                gyroSwipeInfo[device].triggers = Item.InnerText;
                            }
                        }
                        catch { }

                        try
                        {
                            Item = xmlGyroSwipeElement.SelectSingleNode("TriggerCond");
                            if (Item != null)
                            {
                                gyroSwipeInfo[device].triggerCond = SaTriggerCondValue(Item.InnerText);
                            }
                        }
                        catch { }

                        try
                        {
                            Item = xmlGyroSwipeElement.SelectSingleNode("TriggerTurns");
                            if (bool.TryParse(Item?.InnerText ?? "", out bool tempTurns))
                            {
                                gyroSwipeInfo[device].triggerTurns = tempTurns;
                            }
                        }
                        catch { }

                        try
                        {
                            Item = xmlGyroSwipeElement.SelectSingleNode("XAxis");
                            if (Enum.TryParse(Item?.InnerText ?? "", out GyroDirectionalSwipeInfo.XAxisSwipe tempX))
                            {
                                gyroSwipeInfo[device].xAxis = tempX;
                            }
                        }
                        catch { }

                        try
                        {
                            Item = xmlGyroSwipeElement.SelectSingleNode("DelayTime");
                            if (int.TryParse(Item?.InnerText ?? "", out int tempDelay))
                            {
                                gyroSwipeInfo[device].delayTime = tempDelay;
                            }
                        }
                        catch { }
                    }

                    // Check for TouchpadOutputMode if UseTPforControls is not present in profile
                    if (!tpForControlsPresent)
                    {
                        try
                        {
                            Item = m_Xdoc.SelectSingleNode("/" + rootname + "/TouchpadOutputMode");
                            string tempMode = Item.InnerText;
                            Enum.TryParse(tempMode, out touchOutMode[device]);
                        }
                        catch { touchOutMode[device] = TouchpadOutMode.Mouse; missingSetting = true; }
                    }

                    try
                    {
                        Item = m_Xdoc.SelectSingleNode("/" + rootname + "/TouchDisInvTriggers");
                        string[] triggers = Item.InnerText.Split(',');
                        int temp = -1;
                        List<int> tempIntList = new List<int>();
                        for (int i = 0, arlen = triggers.Length; i < arlen; i++)
                        {
                            if (int.TryParse(triggers[i], out temp))
                                tempIntList.Add(temp);
                        }

                        touchDisInvertTriggers[device] = tempIntList.ToArray();
                    }
                    catch { touchDisInvertTriggers[device] = new int[1] { -1 }; missingSetting = true; }

                    try { Item = m_Xdoc.SelectSingleNode("/" + rootname + "/GyroSensitivity"); int.TryParse(Item.InnerText, out gyroSensitivity[device]); }
                    catch { gyroSensitivity[device] = 100; missingSetting = true; }

                    try { Item = m_Xdoc.SelectSingleNode("/" + rootname + "/GyroSensVerticalScale"); int.TryParse(Item.InnerText, out gyroSensVerticalScale[device]); }
                    catch { gyroSensVerticalScale[device] = 100; missingSetting = true; }

                    try { Item = m_Xdoc.SelectSingleNode("/" + rootname + "/GyroInvert"); int.TryParse(Item.InnerText, out gyroInvert[device]); }
                    catch { gyroInvert[device] = 0; missingSetting = true; }

                    try { Item = m_Xdoc.SelectSingleNode("/" + rootname + "/GyroTriggerTurns"); bool.TryParse(Item.InnerText, out gyroTriggerTurns[device]); }
                    catch { gyroTriggerTurns[device] = true; missingSetting = true; }

                    /*try { Item = m_Xdoc.SelectSingleNode("/" + rootname + "/GyroSmoothing"); bool.TryParse(Item.InnerText, out gyroSmoothing[device]); }
                    catch { gyroSmoothing[device] = false; missingSetting = true; }

                    try { Item = m_Xdoc.SelectSingleNode("/" + rootname + "/GyroSmoothingWeight"); int temp = 0; int.TryParse(Item.InnerText, out temp); gyroSmoothWeight[device] = Math.Min(Math.Max(0.0, Convert.ToDouble(temp * 0.01)), 1.0); }
                    catch { gyroSmoothWeight[device] = 0.5; missingSetting = true; }
                    */

                    bool gyroSmoothingGroup = false;
                    XmlNode xmlGyroSmoothingElement =
                        m_Xdoc.SelectSingleNode("/" + rootname + "/GyroMouseSmoothingSettings");
                    gyroSmoothingGroup = xmlGyroSmoothingElement != null;

                    if (gyroSmoothingGroup)
                    {
                        try
                        {
                            Item = xmlGyroSmoothingElement.SelectSingleNode("UseSmoothing");
                            bool.TryParse(Item.InnerText, out bool temp);
                            gyroMouseInfo[device].enableSmoothing = temp;
                        }
                        catch { gyroMouseInfo[device].enableSmoothing = false; missingSetting = true; }

                        try
                        {
                            Item = xmlGyroSmoothingElement.SelectSingleNode("SmoothingMethod");
                            string temp = Item?.InnerText ?? DS4Windows.GyroMouseInfo.DEFAULT_SMOOTH_TECHNIQUE;
                            gyroMouseInfo[device].DetermineSmoothMethod(temp);
                        }
                        catch { gyroMouseInfo[device].ResetSmoothing(); missingSetting = true; }

                        try
                        {
                            Item = xmlGyroSmoothingElement.SelectSingleNode("SmoothingWeight");
                            int.TryParse(Item.InnerText, out int temp);
                            gyroMouseInfo[device].smoothingWeight = Math.Min(Math.Max(0.0, Convert.ToDouble(temp * 0.01)), 1.0);
                        }
                        catch { gyroMouseInfo[device].smoothingWeight = 0.5; missingSetting = true; }

                        try
                        {
                            Item = xmlGyroSmoothingElement.SelectSingleNode("SmoothingMinCutoff");
                            double.TryParse(Item.InnerText, out double temp);
                            gyroMouseInfo[device].minCutoff = Math.Min(Math.Max(0.0, temp), 100.0);
                        }
                        catch { gyroMouseInfo[device].minCutoff = DS4Windows.GyroMouseInfo.DEFAULT_MINCUTOFF; missingSetting = true; }

                        try
                        {
                            Item = xmlGyroSmoothingElement.SelectSingleNode("SmoothingBeta");
                            double.TryParse(Item.InnerText, out double temp);
                            gyroMouseInfo[device].beta = Math.Min(Math.Max(0.0, temp), 1.0);
                        }
                        catch { gyroMouseInfo[device].beta = DS4Windows.GyroMouseInfo.DEFAULT_BETA; missingSetting = true; }
                    }
                    else
                    {
                        missingSetting = true;
                    }

                    try { Item = m_Xdoc.SelectSingleNode("/" + rootname + "/GyroMouseHAxis"); int temp = 0; int.TryParse(Item.InnerText, out temp); gyroMouseHorizontalAxis[device] = Math.Min(Math.Max(0, temp), 1); }
                    catch { gyroMouseHorizontalAxis[device] = 0; missingSetting = true; }

                    try
                    {
                        Item = m_Xdoc.SelectSingleNode("/" + rootname + "/GyroMouseDeadZone"); int.TryParse(Item.InnerText, out int temp);
                        SetGyroMouseDZ(device, temp, control);
                    }
                    catch { SetGyroMouseDZ(device, MouseCursor.GYRO_MOUSE_DEADZONE, control); missingSetting = true; }

                    try
                    {
                        Item = m_Xdoc.SelectSingleNode("/" + rootname + "/GyroMouseMinThreshold");
                        double.TryParse(Item.InnerText, out double temp);
                        temp = Math.Min(Math.Max(temp, 1.0), 40.0);
                        gyroMouseInfo[device].minThreshold = temp;
                    }
                    catch { gyroMouseInfo[device].minThreshold = DS4Windows.GyroMouseInfo.DEFAULT_MIN_THRESHOLD; missingSetting = true; }

                    try
                    {
                        Item = m_Xdoc.SelectSingleNode("/" + rootname + "/GyroMouseToggle");
                        bool.TryParse(Item.InnerText, out bool temp);
                        SetGyroMouseToggle(device, temp, control);
                    }
                    catch
                    {
                        SetGyroMouseToggle(device, false, control);
                        missingSetting = true;
                    }

                    try
                    {
                        Item = m_Xdoc.SelectSingleNode("/" + rootname + "/BTPollRate");
                        int temp = 0;
                        int.TryParse(Item.InnerText, out temp);
                        btPollRate[device] = (temp >= 0 && temp <= 16) ? temp : 4;
                    }
                    catch { btPollRate[device] = 4; missingSetting = true; }

                    // Note! xxOutputCurveCustom property needs to be read before xxOutputCurveMode property in case the curveMode is value 6
                    try { Item = m_Xdoc.SelectSingleNode("/" + rootname + "/LSOutputCurveCustom"); LSOutBezierCurveObj[device].CustomDefinition = Item.InnerText; }
                    catch { missingSetting = true; }
                    try { Item = m_Xdoc.SelectSingleNode("/" + rootname + "/LSOutputCurveMode"); setLsOutCurveMode(device, stickOutputCurveId(Item.InnerText)); }
                    catch { setLsOutCurveMode(device, 0); missingSetting = true; }

                    try { Item = m_Xdoc.SelectSingleNode("/" + rootname + "/RSOutputCurveCustom"); RSOutBezierCurveObj[device].CustomDefinition = Item.InnerText; }
                    catch { missingSetting = true; }
                    try { Item = m_Xdoc.SelectSingleNode("/" + rootname + "/RSOutputCurveMode"); setRsOutCurveMode(device, stickOutputCurveId(Item.InnerText)); }
                    catch { setRsOutCurveMode(device, 0); missingSetting = true; }

                    try { Item = m_Xdoc.SelectSingleNode("/" + rootname + "/LSSquareStick"); bool.TryParse(Item.InnerText, out SquStickInfo[device].lsMode); }
                    catch { SquStickInfo[device].lsMode = false; missingSetting = true; }

                    try { Item = m_Xdoc.SelectSingleNode("/" + rootname + "/SquareStickRoundness"); double.TryParse(Item.InnerText, out SquStickInfo[device].lsRoundness); }
                    catch { SquStickInfo[device].lsRoundness = 5.0; missingSetting = true; }
                    try { Item = m_Xdoc.SelectSingleNode("/" + rootname + "/SquareRStickRoundness"); double.TryParse(Item.InnerText, out SquStickInfo[device].rsRoundness); }
                    catch { SquStickInfo[device].rsRoundness = 5.0; missingSetting = true; }

                    try { Item = m_Xdoc.SelectSingleNode("/" + rootname + "/RSSquareStick"); bool.TryParse(Item.InnerText, out SquStickInfo[device].rsMode); }
                    catch { SquStickInfo[device].rsMode = false; missingSetting = true; }


                    try { Item = m_Xdoc.SelectSingleNode("/" + rootname + "/LSAntiSnapback"); bool.TryParse(Item.InnerText, out LSAntiSnapbackInfo[device].enabled); }
                    catch { LSAntiSnapbackInfo[device].enabled = StickAntiSnapbackInfo.DEFAULT_ENABLED; missingSetting = true; }
                    try { Item = m_Xdoc.SelectSingleNode("/" + rootname + "/RSAntiSnapback"); bool.TryParse(Item.InnerText, out RSAntiSnapbackInfo[device].enabled); }
                    catch { RSAntiSnapbackInfo[device].enabled = StickAntiSnapbackInfo.DEFAULT_ENABLED; missingSetting = true; }

                    try { Item = m_Xdoc.SelectSingleNode("/" + rootname + "/LSAntiSnapbackDelta"); double.TryParse(Item.InnerText, out LSAntiSnapbackInfo[device].delta); }
                    catch { LSAntiSnapbackInfo[device].delta = StickAntiSnapbackInfo.DEFAULT_DELTA; missingSetting = true; }
                    try { Item = m_Xdoc.SelectSingleNode("/" + rootname + "/RSAntiSnapbackDelta"); double.TryParse(Item.InnerText, out RSAntiSnapbackInfo[device].delta); }
                    catch { RSAntiSnapbackInfo[device].delta = StickAntiSnapbackInfo.DEFAULT_DELTA; missingSetting = true; }

                    try { Item = m_Xdoc.SelectSingleNode("/" + rootname + "/LSAntiSnapbackTimeout"); int.TryParse(Item.InnerText, out LSAntiSnapbackInfo[device].timeout); }
                    catch { LSAntiSnapbackInfo[device].timeout = StickAntiSnapbackInfo.DEFAULT_TIMEOUT; missingSetting = true; }
                    try { Item = m_Xdoc.SelectSingleNode("/" + rootname + "/RSAntiSnapbackTimeout"); int.TryParse(Item.InnerText, out RSAntiSnapbackInfo[device].timeout); }
                    catch { RSAntiSnapbackInfo[device].timeout = StickAntiSnapbackInfo.DEFAULT_TIMEOUT; missingSetting = true; }

                    try { Item = m_Xdoc.SelectSingleNode("/" + rootname + "/LSOutputMode"); Enum.TryParse(Item.InnerText, out LSOutputSettings[device].mode); }
                    catch { missingSetting = true; }

                    try { Item = m_Xdoc.SelectSingleNode("/" + rootname + "/RSOutputMode"); Enum.TryParse(Item.InnerText, out RSOutputSettings[device].mode); }
                    catch { missingSetting = true; }

                    XmlNode xmlLSOutputSettingsElement =
                        m_Xdoc.SelectSingleNode("/" + rootname + "/LSOutputSettings");
                    bool lsOutputGroup = xmlLSOutputSettingsElement != null;
                    if (lsOutputGroup)
                    {
                        bool flickStickLSGroup = false;
                        XmlNode xmlFlickStickLSElement =
                            xmlLSOutputSettingsElement.SelectSingleNode("FlickStickSettings");
                        flickStickLSGroup = xmlFlickStickLSElement != null;

                        if (flickStickLSGroup)
                        {
                            try
                            {
                                Item = xmlFlickStickLSElement.SelectSingleNode("RealWorldCalibration");
                                double.TryParse(Item.InnerText, out double temp);
                                LSOutputSettings[device].outputSettings.flickSettings.realWorldCalibration = temp;
                            }
                            catch { missingSetting = true; }

                            try
                            {
                                Item = xmlFlickStickLSElement.SelectSingleNode("FlickThreshold");
                                double.TryParse(Item.InnerText, out double temp);
                                LSOutputSettings[device].outputSettings.flickSettings.flickThreshold = temp;
                            }
                            catch { missingSetting = true; }

                            try
                            {
                                Item = xmlFlickStickLSElement.SelectSingleNode("FlickTime");
                                double.TryParse(Item.InnerText, out double temp);
                                LSOutputSettings[device].outputSettings.flickSettings.flickTime = temp;
                            }
                            catch { missingSetting = true; }
                        }
                        else
                        {
                            missingSetting = true;
                        }
                    }
                    else
                    {
                        missingSetting = true;
                    }

                    XmlNode xmlRSOutputSettingsElement =
                        m_Xdoc.SelectSingleNode("/" + rootname + "/RSOutputSettings");
                    bool rsOutputGroup = xmlRSOutputSettingsElement != null;
                    if (rsOutputGroup)
                    {
                        bool flickStickRSGroup = false;
                        XmlNode xmlFlickStickRSElement =
                            xmlRSOutputSettingsElement.SelectSingleNode("FlickStickSettings");
                        flickStickRSGroup = xmlFlickStickRSElement != null;

                        if (flickStickRSGroup)
                        {
                            try
                            {
                                Item = xmlFlickStickRSElement.SelectSingleNode("RealWorldCalibration");
                                double.TryParse(Item.InnerText, out double temp);
                                RSOutputSettings[device].outputSettings.flickSettings.realWorldCalibration = temp;
                            }
                            catch { missingSetting = true; }

                            try
                            {
                                Item = xmlFlickStickRSElement.SelectSingleNode("FlickThreshold");
                                double.TryParse(Item.InnerText, out double temp);
                                RSOutputSettings[device].outputSettings.flickSettings.flickThreshold = temp;
                            }
                            catch { missingSetting = true; }

                            try
                            {
                                Item = xmlFlickStickRSElement.SelectSingleNode("FlickTime");
                                double.TryParse(Item.InnerText, out double temp);
                                RSOutputSettings[device].outputSettings.flickSettings.flickTime = temp;
                            }
                            catch { missingSetting = true; }
                        }
                        else
                        {
                            missingSetting = true;
                        }
                    }
                    else
                    {
                        missingSetting = true;
                    }

                    try { Item = m_Xdoc.SelectSingleNode("/" + rootname + "/L2OutputCurveCustom"); L2OutBezierCurveObj[device].CustomDefinition = Item.InnerText; }
                    catch { missingSetting = true; }
                    try { Item = m_Xdoc.SelectSingleNode("/" + rootname + "/L2OutputCurveMode"); setL2OutCurveMode(device, axisOutputCurveId(Item.InnerText)); }
                    catch { setL2OutCurveMode(device, 0); missingSetting = true; }

                    try
                    {
                        Item = m_Xdoc.SelectSingleNode("/" + rootname + "/L2TwoStageMode");
                        if (Enum.TryParse(Item?.InnerText, out TwoStageTriggerMode tempMode))
                        {
                            L2OutputSettings[device].TwoStageMode = tempMode;
                        }
                    }
                    catch { }

                    try
                    {
                        Item = m_Xdoc.SelectSingleNode("/" + rootname + "/L2HipFireDelay");
                        if (int.TryParse(Item?.InnerText, out int temp))
                        {
                            L2OutputSettings[device].hipFireMS = Math.Max(Math.Min(0, temp), 5000);
                        }
                    }
                    catch { }

                    try
                    {
                        Item = m_Xdoc.SelectSingleNode("/" + rootname + "/L2TriggerEffect");
                        if (Enum.TryParse(Item?.InnerText, out InputDevices.TriggerEffects tempMode))
                        {
                            L2OutputSettings[device].TriggerEffect = tempMode;
                        }
                    }
                    catch { }

                    try { Item = m_Xdoc.SelectSingleNode("/" + rootname + "/R2OutputCurveCustom"); R2OutBezierCurveObj[device].CustomDefinition = Item.InnerText; }
                    catch { missingSetting = true; }
                    try { Item = m_Xdoc.SelectSingleNode("/" + rootname + "/R2OutputCurveMode"); setR2OutCurveMode(device, axisOutputCurveId(Item.InnerText)); }
                    catch { setR2OutCurveMode(device, 0); missingSetting = true; }

                    try
                    {
                        Item = m_Xdoc.SelectSingleNode("/" + rootname + "/R2TwoStageMode");
                        if (Enum.TryParse(Item?.InnerText, out TwoStageTriggerMode tempMode))
                        {
                            R2OutputSettings[device].TwoStageMode = tempMode;
                        }
                    }
                    catch { }

                    try
                    {
                        Item = m_Xdoc.SelectSingleNode("/" + rootname + "/R2HipFireDelay");
                        if (int.TryParse(Item?.InnerText, out int temp))
                        {
                            R2OutputSettings[device].hipFireMS = Math.Max(Math.Min(0, temp), 5000);
                        }
                    }
                    catch { }

                    try
                    {
                        Item = m_Xdoc.SelectSingleNode("/" + rootname + "/R2TriggerEffect");
                        if (Enum.TryParse(Item?.InnerText, out InputDevices.TriggerEffects tempMode))
                        {
                            R2OutputSettings[device].TriggerEffect = tempMode;
                        }
                    }
                    catch { }

                    try { Item = m_Xdoc.SelectSingleNode("/" + rootname + "/SXOutputCurveCustom"); SXOutBezierCurveObj[device].CustomDefinition = Item.InnerText; }
                    catch { missingSetting = true; }
                    try { Item = m_Xdoc.SelectSingleNode("/" + rootname + "/SXOutputCurveMode"); setSXOutCurveMode(device, axisOutputCurveId(Item.InnerText)); }
                    catch { setSXOutCurveMode(device, 0); missingSetting = true; }

                    try { Item = m_Xdoc.SelectSingleNode("/" + rootname + "/SZOutputCurveCustom"); SZOutBezierCurveObj[device].CustomDefinition = Item.InnerText; }
                    catch { missingSetting = true; }
                    try { Item = m_Xdoc.SelectSingleNode("/" + rootname + "/SZOutputCurveMode"); setSZOutCurveMode(device, axisOutputCurveId(Item.InnerText)); }
                    catch { setSZOutCurveMode(device, 0); missingSetting = true; }

                    try { Item = m_Xdoc.SelectSingleNode("/" + rootname + "/TrackballMode"); bool.TryParse(Item.InnerText, out trackballMode[device]); }
                    catch { trackballMode[device] = false; missingSetting = true; }

                    try { Item = m_Xdoc.SelectSingleNode("/" + rootname + "/TrackballFriction"); double.TryParse(Item.InnerText, out TrackballFriction[device]); }
                    catch { TrackballFriction[device] = 10.0; missingSetting = true; }

                    try
                    {
                        Item = m_Xdoc.SelectSingleNode("/" + rootname + "/TouchRelMouseRotation");
                        int.TryParse(Item.InnerText, out int temp);
                        temp = Math.Min(Math.Max(temp, -180), 180);
                        TouchpadRelMouse[device].rotation = temp * Math.PI / 180.0;
                    }
                    catch { TouchpadRelMouse[device].rotation = 0.0; missingSetting = true; }

                    try
                    {
                        Item = m_Xdoc.SelectSingleNode("/" + rootname + "/TouchRelMouseMinThreshold");
                        double.TryParse(Item.InnerText, out double temp);
                        temp = Math.Min(Math.Max(temp, 1.0), 40.0);
                        TouchpadRelMouse[device].minThreshold = temp;
                    }
                    catch { TouchpadRelMouse[device].minThreshold = TouchpadRelMouseSettings.DEFAULT_MIN_THRESHOLD; missingSetting = true; }


                    bool touchpadAbsMouseGroup = false;
                    XmlNode touchpadAbsMouseElement =
                        m_Xdoc.SelectSingleNode("/" + rootname + "/TouchpadAbsMouseSettings");
                    touchpadAbsMouseGroup = touchpadAbsMouseElement != null;

                    if (touchpadAbsMouseGroup)
                    {
                        try
                        {
                            Item = touchpadAbsMouseElement.SelectSingleNode("MaxZoneX");
                            int.TryParse(Item.InnerText, out int temp);
                            TouchpadAbsMouse[device].maxZoneX = temp;
                        }
                        catch { TouchpadAbsMouse[device].maxZoneX = TouchpadAbsMouseSettings.DEFAULT_MAXZONE_X; missingSetting = true; }

                        try
                        {
                            Item = touchpadAbsMouseElement.SelectSingleNode("MaxZoneY");
                            int.TryParse(Item.InnerText, out int temp);
                            TouchpadAbsMouse[device].maxZoneY = temp;
                        }
                        catch { TouchpadAbsMouse[device].maxZoneY = TouchpadAbsMouseSettings.DEFAULT_MAXZONE_Y; missingSetting = true; }

                        try
                        {
                            Item = touchpadAbsMouseElement.SelectSingleNode("SnapToCenter");
                            bool.TryParse(Item.InnerText, out bool temp);
                            TouchpadAbsMouse[device].snapToCenter = temp;
                        }
                        catch { TouchpadAbsMouse[device].snapToCenter = TouchpadAbsMouseSettings.DEFAULT_SNAP_CENTER; missingSetting = true; }
                    }
                    else
                    {
                        missingSetting = true;
                    }

                    try { Item = m_Xdoc.SelectSingleNode("/" + rootname + "/OutputContDevice"); OutputDeviceType[device] = OutContDeviceId(Item.InnerText); }
                    catch { OutputDeviceType[device] = DS4Windows.OutContType.X360; missingSetting = true; }

                    // Only change xinput devices under certain conditions. Avoid
                    // performing this upon program startup before loading devices.
                    if (xinputChange && device < ControlService.CURRENT_DS4_CONTROLLER_LIMIT)
                    {
                        CheckOldDeviceStatus(device, control, oldContType,
                            out xinputPlug, out xinputStatus);
                    }

                    try
                    {
                        Item = m_Xdoc.SelectSingleNode("/" + rootname + "/ProfileActions");
                        ProfileActions[device].Clear();
                        if (!string.IsNullOrEmpty(Item.InnerText))
                        {
                            string[] actionNames = Item.InnerText.Split('/');
                            for (int actIndex = 0, actLen = actionNames.Length; actIndex < actLen; actIndex++)
                            {
                                string tempActionName = actionNames[actIndex];
                                if (!ProfileActions[device].Contains(tempActionName))
                                {
                                    ProfileActions[device].Add(tempActionName);
                                }
                            }
                        }
                    }
                    catch { ProfileActions[device].Clear(); missingSetting = true; }

                    foreach (DS4ControlSettings dcs in Ds4Settings[device])
                        dcs.Reset();

                    containsCustomAction[device] = false;
                    containsCustomExtras[device] = false;
                    profileActionCount[device] = ProfileActions[device].Count;
                    profileActionDict[device].Clear();
                    profileActionIndexDict[device].Clear();
                    foreach (string actionname in ProfileActions[device])
                    {
                        profileActionDict[device][actionname] = Global.Instance.GetAction(actionname);
                        profileActionIndexDict[device][actionname] = Global.Instance.GetActionIndexOf(actionname);
                    }

                    DS4KeyType keyType;
                    ushort wvk;

                    {
                        XmlNode ParentItem = m_Xdoc.SelectSingleNode("/" + rootname + "/Control/Button");
                        if (ParentItem != null)
                        {
                            foreach (XmlNode item in ParentItem.ChildNodes)
                            {
                                if (Enum.TryParse(item.Name, out DS4Controls currentControl))
                                {
                                    UpdateDs4ControllerSetting(device, item.Name, false, GetX360ControlsByName(item.InnerText), "", DS4KeyType.None, 0);
                                    customMapButtons.Add(GetDs4ControlsByName(item.Name), GetX360ControlsByName(item.InnerText));
                                }
                            }
                        }

                        ParentItem = m_Xdoc.SelectSingleNode("/" + rootname + "/Control/Macro");
                        if (ParentItem != null)
                        {
                            foreach (XmlNode item in ParentItem.ChildNodes)
                            {
                                customMapMacros.Add(GetDs4ControlsByName(item.Name), item.InnerText);
                                string[] skeys;
                                int[] keys;
                                if (!string.IsNullOrEmpty(item.InnerText))
                                {
                                    skeys = item.InnerText.Split('/');
                                    keys = new int[skeys.Length];
                                }
                                else
                                {
                                    skeys = new string[0];
                                    keys = new int[0];
                                }

                                for (int i = 0, keylen = keys.Length; i < keylen; i++)
                                    keys[i] = int.Parse(skeys[i]);

                                if (Enum.TryParse(item.Name, out DS4Controls currentControl))
                                {
                                    UpdateDs4ControllerSetting(device, item.Name, false, keys, "", DS4KeyType.None, 0);
                                }
                            }
                        }

                        ParentItem = m_Xdoc.SelectSingleNode("/" + rootname + "/Control/Key");
                        if (ParentItem != null)
                        {
                            foreach (XmlNode item in ParentItem.ChildNodes)
                            {
                                if (ushort.TryParse(item.InnerText, out wvk) && Enum.TryParse(item.Name, out DS4Controls currentControl))
                                {
                                    UpdateDs4ControllerSetting(device, item.Name, false, wvk, "", DS4KeyType.None, 0);
                                    customMapKeys.Add(GetDs4ControlsByName(item.Name), wvk);
                                }
                            }
                        }

                        ParentItem = m_Xdoc.SelectSingleNode("/" + rootname + "/Control/Extras");
                        if (ParentItem != null)
                        {
                            foreach (XmlNode item in ParentItem.ChildNodes)
                            {
                                if (item.InnerText != string.Empty && Enum.TryParse(item.Name, out DS4Controls currentControl))
                                {
                                    UpdateDs4ControllerExtra(device, item.Name, false, item.InnerText);
                                    customMapExtras.Add(GetDs4ControlsByName(item.Name), item.InnerText);
                                }
                                else
                                    ParentItem.RemoveChild(item);
                            }
                        }

                        ParentItem = m_Xdoc.SelectSingleNode("/" + rootname + "/Control/KeyType");
                        if (ParentItem != null)
                        {
                            foreach (XmlNode item in ParentItem.ChildNodes)
                            {
                                if (item != null)
                                {
                                    keyType = DS4KeyType.None;
                                    if (item.InnerText.Contains(DS4KeyType.ScanCode.ToString()))
                                        keyType |= DS4KeyType.ScanCode;
                                    if (item.InnerText.Contains(DS4KeyType.Toggle.ToString()))
                                        keyType |= DS4KeyType.Toggle;
                                    if (item.InnerText.Contains(DS4KeyType.Macro.ToString()))
                                        keyType |= DS4KeyType.Macro;
                                    if (item.InnerText.Contains(DS4KeyType.HoldMacro.ToString()))
                                        keyType |= DS4KeyType.HoldMacro;
                                    if (item.InnerText.Contains(DS4KeyType.Unbound.ToString()))
                                        keyType |= DS4KeyType.Unbound;

                                    if (keyType != DS4KeyType.None && Enum.TryParse(item.Name, out DS4Controls currentControl))
                                    {
                                        UpdateDs4ControllerKeyType(device, item.Name, false, keyType);
                                        customMapKeyTypes.Add(GetDs4ControlsByName(item.Name), keyType);
                                    }
                                }
                            }
                        }

                        ParentItem = m_Xdoc.SelectSingleNode("/" + rootname + "/ShiftControl/Button");
                        if (ParentItem != null)
                        {
                            foreach (XmlElement item in ParentItem.ChildNodes)
                            {
                                int shiftT = shiftM;
                                if (item.HasAttribute("Trigger"))
                                    int.TryParse(item.Attributes["Trigger"].Value, out shiftT);

                                if (Enum.TryParse(item.Name, out DS4Controls currentControl))
                                {
                                    UpdateDs4ControllerSetting(device, item.Name, true, GetX360ControlsByName(item.InnerText), "", DS4KeyType.None, shiftT);
                                    shiftCustomMapButtons.Add(GetDs4ControlsByName(item.Name), GetX360ControlsByName(item.InnerText));
                                }
                            }
                        }

                        ParentItem = m_Xdoc.SelectSingleNode("/" + rootname + "/ShiftControl/Macro");
                        if (ParentItem != null)
                        {
                            foreach (XmlElement item in ParentItem.ChildNodes)
                            {
                                shiftCustomMapMacros.Add(GetDs4ControlsByName(item.Name), item.InnerText);
                                string[] skeys;
                                int[] keys;
                                if (!string.IsNullOrEmpty(item.InnerText))
                                {
                                    skeys = item.InnerText.Split('/');
                                    keys = new int[skeys.Length];
                                }
                                else
                                {
                                    skeys = new string[0];
                                    keys = new int[0];
                                }

                                for (int i = 0, keylen = keys.Length; i < keylen; i++)
                                    keys[i] = int.Parse(skeys[i]);

                                int shiftT = shiftM;
                                if (item.HasAttribute("Trigger"))
                                    int.TryParse(item.Attributes["Trigger"].Value, out shiftT);

                                if (Enum.TryParse(item.Name, out DS4Controls currentControl))
                                {
                                    UpdateDs4ControllerSetting(device, item.Name, true, keys, "", DS4KeyType.None, shiftT);
                                }
                            }
                        }

                        ParentItem = m_Xdoc.SelectSingleNode("/" + rootname + "/ShiftControl/Key");
                        if (ParentItem != null)
                        {
                            foreach (XmlElement item in ParentItem.ChildNodes)
                            {
                                if (ushort.TryParse(item.InnerText, out wvk))
                                {
                                    int shiftT = shiftM;
                                    if (item.HasAttribute("Trigger"))
                                        int.TryParse(item.Attributes["Trigger"].Value, out shiftT);

                                    if (Enum.TryParse(item.Name, out DS4Controls currentControl))
                                    {
                                        UpdateDs4ControllerSetting(device, item.Name, true, wvk, "", DS4KeyType.None, shiftT);
                                        shiftCustomMapKeys.Add(GetDs4ControlsByName(item.Name), wvk);
                                    }
                                }
                            }
                        }

                        ParentItem = m_Xdoc.SelectSingleNode("/" + rootname + "/ShiftControl/Extras");
                        if (ParentItem != null)
                        {
                            foreach (XmlElement item in ParentItem.ChildNodes)
                            {
                                if (item.InnerText != string.Empty)
                                {
                                    if (Enum.TryParse(item.Name, out DS4Controls currentControl))
                                    {
                                        UpdateDs4ControllerExtra(device, item.Name, true, item.InnerText);
                                        shiftCustomMapExtras.Add(GetDs4ControlsByName(item.Name), item.InnerText);
                                    }
                                }
                                else
                                    ParentItem.RemoveChild(item);
                            }
                        }

                        ParentItem = m_Xdoc.SelectSingleNode("/" + rootname + "/ShiftControl/KeyType");
                        if (ParentItem != null)
                        {
                            foreach (XmlElement item in ParentItem.ChildNodes)
                            {
                                if (item != null)
                                {
                                    keyType = DS4KeyType.None;
                                    if (item.InnerText.Contains(DS4KeyType.ScanCode.ToString()))
                                        keyType |= DS4KeyType.ScanCode;
                                    if (item.InnerText.Contains(DS4KeyType.Toggle.ToString()))
                                        keyType |= DS4KeyType.Toggle;
                                    if (item.InnerText.Contains(DS4KeyType.Macro.ToString()))
                                        keyType |= DS4KeyType.Macro;
                                    if (item.InnerText.Contains(DS4KeyType.HoldMacro.ToString()))
                                        keyType |= DS4KeyType.HoldMacro;
                                    if (item.InnerText.Contains(DS4KeyType.Unbound.ToString()))
                                        keyType |= DS4KeyType.Unbound;

                                    if (keyType != DS4KeyType.None && Enum.TryParse(item.Name, out DS4Controls currentControl))
                                    {
                                        UpdateDs4ControllerKeyType(device, item.Name, true, keyType);
                                        shiftCustomMapKeyTypes.Add(GetDs4ControlsByName(item.Name), keyType);
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    Loaded = false;
                    ResetProfile(device);
                    ResetMouseProperties(device, control);

                    // Unplug existing output device if requested profile does not exist
                    OutputDevice tempOutDev = device < ControlService.CURRENT_DS4_CONTROLLER_LIMIT ?
                        control.outputDevices[device] : null;
                    if (tempOutDev != null)
                    {
                        tempOutDev = null;
                        //Global.ActiveOutDevType[device] = OutContType.None;
                        DS4Device tempDev = control.DS4Controllers[device];
                        if (tempDev != null)
                        {
                            tempDev.queueEvent(() =>
                            {
                                control.UnplugOutDev(device, tempDev);
                            });
                        }
                    }
                }

                // Only add missing settings if the actual load was graceful
                if ((missingSetting || migratePerformed) && Loaded)// && buttons != null)
                {
                    string proName = Path.GetFileName(profilepath);
                    SaveProfile(device, proName);
                }

                if (Loaded)
                {
                    CacheProfileCustomsFlags(device);
                    ButtonMouseInfos[device].activeButtonSensitivity =
                        ButtonMouseInfos[device].buttonSensitivity;

                    //if (device < Global.MAX_DS4_CONTROLLER_COUNT && control.touchPad[device] != null)
                    //{
                    //    control.touchPad[device]?.ResetToggleGyroModes();
                    //    GyroOutMode currentGyro = gyroOutMode[device];
                    //    if (currentGyro == GyroOutMode.Mouse)
                    //    {
                    //        control.touchPad[device].ToggleGyroMouse =
                    //            gyroMouseToggle[device];
                    //    }
                    //    else if (currentGyro == GyroOutMode.MouseJoystick)
                    //    {
                    //        control.touchPad[device].ToggleGyroMouse =
                    //            gyroMouseStickToggle[device];
                    //    }
                    //}

                    // If a device exists, make sure to transfer relevant profile device
                    // options to device instance
                    if (postLoad && device < Global.MAX_DS4_CONTROLLER_COUNT)
                    {
                        PostLoadSnippet(device, control, xinputStatus, xinputPlug);
                    }
                }

                return Loaded;
            }

            public bool Load()
            {
                bool Loaded = true;
                bool missingSetting = false;

                try
                {
                    if (File.Exists(ProfilesPath))
                    {
                        XmlNode Item;

                        m_Xdoc.Load(ProfilesPath);

                        try { Item = m_Xdoc.SelectSingleNode("/Profile/useExclusiveMode"); Boolean.TryParse(Item.InnerText, out useExclusiveMode); } // Ex Mode
                        catch { missingSetting = true; } // Ex Mode

                        try
                        {
                            Item = m_Xdoc.SelectSingleNode("/Profile/startMinimized"); Boolean.TryParse(Item.InnerText, out var value);
                            StartMinimized = value;
                        }
                        catch { missingSetting = true; }

                        try
                        {
                            Item = m_Xdoc.SelectSingleNode("/Profile/minimizeToTaskbar"); Boolean.TryParse(Item.InnerText, out var value);
                            MinToTaskBar = value;
                        }
                        catch { missingSetting = true; }

                        try
                        {
                            Item = m_Xdoc.SelectSingleNode("/Profile/formWidth"); Int32.TryParse(Item.InnerText, out var value);
                            FormWidth = value;
                        }
                        catch { missingSetting = true; }

                        try
                        {
                            Item = m_Xdoc.SelectSingleNode("/Profile/formHeight"); Int32.TryParse(Item.InnerText, out var value);
                            FormHeight = value;
                        }
                        catch { missingSetting = true; }
                        try
                        {
                            int temp = 0;
                            Item = m_Xdoc.SelectSingleNode("/Profile/formLocationX"); Int32.TryParse(Item.InnerText, out temp);
                            FormLocationX = Math.Max(temp, 0);
                        }
                        catch { missingSetting = true; }

                        try
                        {
                            int temp = 0;
                            Item = m_Xdoc.SelectSingleNode("/Profile/formLocationY"); Int32.TryParse(Item.InnerText, out temp);
                            FormLocationY = Math.Max(temp, 0);
                        }
                        catch { missingSetting = true; }

                        for (int i = 0; i < Global.MAX_DS4_CONTROLLER_COUNT; i++)
                        {
                            string contTag = $"/Profile/Controller{i + 1}";
                            try
                            {
                                Item = m_Xdoc.SelectSingleNode(contTag); ProfilePath[i] = Item?.InnerText ?? string.Empty;
                                if (ProfilePath[i].ToLower().Contains("distance"))
                                {
                                    DistanceProfiles[i] = true;
                                }

                                OlderProfilePath[i] = ProfilePath[i];
                            }
                            catch { ProfilePath[i] = OlderProfilePath[i] = string.Empty; DistanceProfiles[i] = false; }
                        }

                        try
                        {
                            Item = m_Xdoc.SelectSingleNode("/Profile/LastChecked"); DateTime.TryParse(Item.InnerText, out var lc);
                            LastChecked = lc;
                        }
                        catch { missingSetting = true; }

                        try
                        {
                            Item = m_Xdoc.SelectSingleNode("/Profile/CheckWhen"); Int32.TryParse(Item.InnerText, out var cw);
                            CheckWhen = cw;
                        }
                        catch { missingSetting = true; }
                        try
                        {
                            Item = m_Xdoc.SelectSingleNode("/Profile/LastVersionChecked");
                            string tempVer = Item?.InnerText ?? string.Empty;
                            if (!string.IsNullOrEmpty(tempVer))
                            {
                                LastVersionCheckedNumber = Global.CompileVersionNumberFromString(tempVer);
                                if (LastVersionCheckedNumber > 0) lastVersionChecked = tempVer;
                            }
                        }
                        catch { missingSetting = true; }

                        try
                        {
                            Item = m_Xdoc.SelectSingleNode("/Profile/Notifications");
                            if (!int.TryParse(Item.InnerText, out var not))
                            { Notifications = (Boolean.Parse(Item.InnerText) ? 2 : 0); }

                            Notifications = not;
                        }
                        catch { missingSetting = true; }

                        try
                        {
                            Item = m_Xdoc.SelectSingleNode("/Profile/DisconnectBTAtStop"); Boolean.TryParse(Item.InnerText, out var dc);
                            DisconnectBluetoothAtStop = dc;
                        }
                        catch { missingSetting = true; }
                        try { Item = m_Xdoc.SelectSingleNode("/Profile/SwipeProfiles"); Boolean.TryParse(Item.InnerText, out swipeProfiles); }
                        catch { missingSetting = true; }
                        //try { Item = m_Xdoc.SelectSingleNode("/Profile/UseDS4ForMapping"); Boolean.TryParse(Item.InnerText, out ds4Mapping); }
                        //catch { missingSetting = true; }
                        try
                        {
                            Item = m_Xdoc.SelectSingleNode("/Profile/QuickCharge"); Boolean.TryParse(Item.InnerText, out var value);
                            QuickCharge = value;
                        }
                        catch { missingSetting = true; }

                        try
                        {
                            Item = m_Xdoc.SelectSingleNode("/Profile/CloseMinimizes"); Boolean.TryParse(Item.InnerText, out var value);
                            CloseMini = value;
                        }
                        catch { missingSetting = true; }
                        try { Item = m_Xdoc.SelectSingleNode("/Profile/UseLang"); UseLang = Item.InnerText; }
                        catch { missingSetting = true; }

                        try
                        {
                            Item = m_Xdoc.SelectSingleNode("/Profile/DownloadLang"); Boolean.TryParse(Item.InnerText, out var value);
                            DownloadLang = value;
                        }
                        catch { missingSetting = true; }

                        try
                        {
                            Item = m_Xdoc.SelectSingleNode("/Profile/FlashWhenLate"); Boolean.TryParse(Item.InnerText, out var value);
                            FlashWhenLate = value;
                        }
                        catch { missingSetting = true; }
                        try { Item = m_Xdoc.SelectSingleNode("/Profile/FlashWhenLateAt"); int.TryParse(Item.InnerText, out flashWhenLateAt); }
                        catch { missingSetting = true; }

                        Item = m_Xdoc.SelectSingleNode("/Profile/AppIcon");
                        bool hasIconChoice = Item != null;
                        if (hasIconChoice)
                        {
                            hasIconChoice = Enum.TryParse(Item.InnerText ?? "", out TrayIconChoice ch);
                            UseIconChoice = ch;
                        }

                        if (!hasIconChoice)
                        {
                            missingSetting = true;

                            try
                            {
                                Item = m_Xdoc.SelectSingleNode("/Profile/WhiteIcon");
                                if (bool.TryParse(Item?.InnerText ?? "", out bool temp))
                                {
                                    UseIconChoice = temp ? TrayIconChoice.White : TrayIconChoice.Default;
                                }
                            }
                            catch { missingSetting = true; }
                        }

                        try
                        {
                            Item = m_Xdoc.SelectSingleNode("/Profile/AppTheme");
                            string temp = Item.InnerText;
                            Enum.TryParse(temp, out AppThemeChoice choice);
                            ThemeChoice = choice;
                        }
                        catch { missingSetting = true; ThemeChoice = AppThemeChoice.Default; }

                        try { Item = m_Xdoc.SelectSingleNode("/Profile/UseUDPServer"); Boolean.TryParse(Item.InnerText, out UseUdpServer); }
                        catch { missingSetting = true; }
                        try { Item = m_Xdoc.SelectSingleNode("/Profile/UDPServerPort"); int temp; int.TryParse(Item.InnerText, out temp); UdpServerPort = Math.Min(Math.Max(temp, 1024), 65535); }
                        catch { missingSetting = true; }
                        try { Item = m_Xdoc.SelectSingleNode("/Profile/UDPServerListenAddress"); UdpServerListenAddress = Item.InnerText; }
                        catch { missingSetting = true; }

                        bool udpServerSmoothingGroup = false;
                        XmlNode xmlUdpServerSmoothElement =
                            m_Xdoc.SelectSingleNode("/Profile/UDPServerSmoothingOptions");
                        udpServerSmoothingGroup = xmlUdpServerSmoothElement != null;
                        if (udpServerSmoothingGroup)
                        {
                            try
                            {
                                Item = xmlUdpServerSmoothElement.SelectSingleNode("UseSmoothing");
                                bool.TryParse(Item.InnerText, out bool temp);
                                UseUdpSmoothing = temp;
                            }
                            catch { missingSetting = true; }

                            try
                            {
                                Item = xmlUdpServerSmoothElement.SelectSingleNode("UdpSmoothMinCutoff");
                                double.TryParse(Item.InnerText, out double temp);
                                temp = Math.Min(Math.Max(temp, 0.00001), 100.0);
                                udpSmoothingMincutoff = temp;
                            }
                            catch { missingSetting = true; }

                            try
                            {
                                Item = xmlUdpServerSmoothElement.SelectSingleNode("UdpSmoothBeta");
                                double.TryParse(Item.InnerText, out double temp);
                                temp = Math.Min(Math.Max(temp, 0.0), 1.0);
                                udpSmoothingBeta = temp;
                            }
                            catch { missingSetting = true; }
                        }

                        try
                        {
                            Item = m_Xdoc.SelectSingleNode("/Profile/UseCustomSteamFolder"); Boolean.TryParse(Item.InnerText, out var value);
                            UseCustomSteamFolder = value;
                        }
                        catch { missingSetting = true; }
                        try { Item = m_Xdoc.SelectSingleNode("/Profile/CustomSteamFolder"); CustomSteamFolder = Item.InnerText; }
                        catch { missingSetting = true; }
                        try { Item = m_Xdoc.SelectSingleNode("/Profile/AutoProfileRevertDefaultProfile"); Boolean.TryParse(Item.InnerText, out AutoProfileRevertDefaultProfile); }
                        catch { missingSetting = true; }


                        XmlNode xmlDeviceOptions = m_Xdoc.SelectSingleNode("/Profile/DeviceOptions");
                        if (xmlDeviceOptions != null)
                        {
                            XmlNode xmlDS4Support = xmlDeviceOptions.SelectSingleNode("DS4SupportSettings");
                            if (xmlDS4Support != null)
                            {
                                try
                                {
                                    XmlNode item = xmlDS4Support.SelectSingleNode("Enabled");
                                    if (bool.TryParse(item?.InnerText ?? "", out bool temp))
                                    {
                                        DeviceOptions.DS4DeviceOpts.Enabled = temp;
                                    }
                                }
                                catch { }
                            }

                            XmlNode xmlDualSenseSupport = xmlDeviceOptions.SelectSingleNode("DualSenseSupportSettings");
                            if (xmlDualSenseSupport != null)
                            {
                                try
                                {
                                    XmlNode item = xmlDualSenseSupport.SelectSingleNode("Enabled");
                                    if (bool.TryParse(item?.InnerText ?? "", out bool temp))
                                    {
                                        DeviceOptions.DualSenseOpts.Enabled = temp;
                                    }
                                }
                                catch { }
                            }

                            XmlNode xmlSwitchProSupport = xmlDeviceOptions.SelectSingleNode("SwitchProSupportSettings");
                            if (xmlSwitchProSupport != null)
                            {
                                try
                                {
                                    XmlNode item = xmlSwitchProSupport.SelectSingleNode("Enabled");
                                    if (bool.TryParse(item?.InnerText ?? "", out bool temp))
                                    {
                                        DeviceOptions.SwitchProDeviceOpts.Enabled = temp;
                                    }
                                }
                                catch { }
                            }

                            XmlNode xmlJoyConSupport = xmlDeviceOptions.SelectSingleNode("JoyConSupportSettings");
                            if (xmlJoyConSupport != null)
                            {
                                try
                                {
                                    XmlNode item = xmlJoyConSupport.SelectSingleNode("Enabled");
                                    if (bool.TryParse(item?.InnerText ?? "", out bool temp))
                                    {
                                        DeviceOptions.JoyConDeviceOpts.Enabled = temp;
                                    }
                                }
                                catch { }

                                try
                                {
                                    XmlNode item = xmlJoyConSupport.SelectSingleNode("LinkMode");
                                    if (Enum.TryParse(item?.InnerText ?? "", out JoyConDeviceOptions.LinkMode temp))
                                    {
                                        DeviceOptions.JoyConDeviceOpts.LinkedMode = temp;
                                    }
                                }
                                catch { }

                                try
                                {
                                    XmlNode item = xmlJoyConSupport.SelectSingleNode("JoinedGyroProvider");
                                    if (Enum.TryParse(item?.InnerText ?? "", out JoyConDeviceOptions.JoinedGyroProvider temp))
                                    {
                                        DeviceOptions.JoyConDeviceOpts.JoinGyroProv = temp;
                                    }
                                }
                                catch { }
                            }
                        }

                        for (int i = 0; i < Global.MAX_DS4_CONTROLLER_COUNT; i++)
                        {
                            try
                            {
                                Item = m_Xdoc.SelectSingleNode("/Profile/CustomLed" + (i + 1));
                                string[] ss = Item.InnerText.Split(':');
                                bool.TryParse(ss[0], out lightbarSettingInfo[i].ds4winSettings.useCustomLed);
                                DS4Color.TryParse(ss[1], ref lightbarSettingInfo[i].ds4winSettings.m_CustomLed);
                            }
                            catch { lightbarSettingInfo[i].ds4winSettings.useCustomLed = false; lightbarSettingInfo[i].ds4winSettings.m_CustomLed = new DS4Color(Color.Blue); missingSetting = true; }
                        }
                    }
                }
                catch { }

                if (missingSetting)
                    Save();

                if (Loaded)
                {
                    string custom_exe_name_path = Path.Combine(Global.ExecutableDirectory, Global.CUSTOM_EXE_CONFIG_FILENAME);
                    bool fakeExeFileExists = File.Exists(custom_exe_name_path);
                    if (fakeExeFileExists)
                    {
                        string fake_exe_name = File.ReadAllText(custom_exe_name_path).Trim();
                        bool valid = !(fake_exe_name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0);
                        if (valid)
                        {
                            fakeExeFileName = fake_exe_name;
                        }
                    }
                }

                return Loaded;
            }

            public bool Save()
            {
                bool Saved = true;

                XmlNode Node;

                m_Xdoc.RemoveAll();

                Node = m_Xdoc.CreateXmlDeclaration("1.0", "utf-8", String.Empty);
                m_Xdoc.AppendChild(Node);

                Node = m_Xdoc.CreateComment(String.Format(" Profile Configuration Data. {0} ", DateTime.Now));
                m_Xdoc.AppendChild(Node);

                Node = m_Xdoc.CreateComment(string.Format(" Made with DS4Windows version {0} ", Global.ExecutableProductVersion));
                m_Xdoc.AppendChild(Node);

                Node = m_Xdoc.CreateWhitespace("\r\n");
                m_Xdoc.AppendChild(Node);

                XmlElement rootElement = m_Xdoc.CreateElement("Profile", null);
                rootElement.SetAttribute("app_version", Global.ExecutableProductVersion);
                rootElement.SetAttribute("config_version", Global.APP_CONFIG_VERSION.ToString());

                // Ex Mode (+1 line)
                XmlNode xmlUseExclNode = m_Xdoc.CreateNode(XmlNodeType.Element, "useExclusiveMode", null); xmlUseExclNode.InnerText = useExclusiveMode.ToString(); rootElement.AppendChild(xmlUseExclNode);
                XmlNode xmlStartMinimized = m_Xdoc.CreateNode(XmlNodeType.Element, "startMinimized", null); xmlStartMinimized.InnerText = StartMinimized.ToString(); rootElement.AppendChild(xmlStartMinimized);
                XmlNode xmlminToTaskbar = m_Xdoc.CreateNode(XmlNodeType.Element, "minimizeToTaskbar", null); xmlminToTaskbar.InnerText = MinToTaskBar.ToString(); rootElement.AppendChild(xmlminToTaskbar);
                XmlNode xmlFormWidth = m_Xdoc.CreateNode(XmlNodeType.Element, "formWidth", null); xmlFormWidth.InnerText = FormWidth.ToString(); rootElement.AppendChild(xmlFormWidth);
                XmlNode xmlFormHeight = m_Xdoc.CreateNode(XmlNodeType.Element, "formHeight", null); xmlFormHeight.InnerText = FormHeight.ToString(); rootElement.AppendChild(xmlFormHeight);
                XmlNode xmlFormLocationX = m_Xdoc.CreateNode(XmlNodeType.Element, "formLocationX", null); xmlFormLocationX.InnerText = FormLocationX.ToString(); rootElement.AppendChild(xmlFormLocationX);
                XmlNode xmlFormLocationY = m_Xdoc.CreateNode(XmlNodeType.Element, "formLocationY", null); xmlFormLocationY.InnerText = FormLocationY.ToString(); rootElement.AppendChild(xmlFormLocationY);

                for (int i = 0; i < Global.MAX_DS4_CONTROLLER_COUNT; i++)
                {
                    string contTagName = $"Controller{i + 1}";
                    XmlNode xmlControllerNode = m_Xdoc.CreateNode(XmlNodeType.Element, contTagName, null); xmlControllerNode.InnerText = !Global.LinkedProfileCheck[i] ? ProfilePath[i] : OlderProfilePath[i];
                    if (!string.IsNullOrEmpty(xmlControllerNode.InnerText))
                    {
                        rootElement.AppendChild(xmlControllerNode);
                    }
                }

                XmlNode xmlLastChecked = m_Xdoc.CreateNode(XmlNodeType.Element, "LastChecked", null); xmlLastChecked.InnerText = LastChecked.ToString(); rootElement.AppendChild(xmlLastChecked);
                XmlNode xmlCheckWhen = m_Xdoc.CreateNode(XmlNodeType.Element, "CheckWhen", null); xmlCheckWhen.InnerText = CheckWhen.ToString(); rootElement.AppendChild(xmlCheckWhen);
                if (!string.IsNullOrEmpty(lastVersionChecked))
                {
                    XmlNode xmlLastVersionChecked = m_Xdoc.CreateNode(XmlNodeType.Element, "LastVersionChecked", null); xmlLastVersionChecked.InnerText = lastVersionChecked.ToString(); rootElement.AppendChild(xmlLastVersionChecked);
                }

                XmlNode xmlNotifications = m_Xdoc.CreateNode(XmlNodeType.Element, "Notifications", null); xmlNotifications.InnerText = Notifications.ToString(); rootElement.AppendChild(xmlNotifications);
                XmlNode xmlDisconnectBT = m_Xdoc.CreateNode(XmlNodeType.Element, "DisconnectBTAtStop", null); xmlDisconnectBT.InnerText = DisconnectBluetoothAtStop.ToString(); rootElement.AppendChild(xmlDisconnectBT);
                XmlNode xmlSwipeProfiles = m_Xdoc.CreateNode(XmlNodeType.Element, "SwipeProfiles", null); xmlSwipeProfiles.InnerText = swipeProfiles.ToString(); rootElement.AppendChild(xmlSwipeProfiles);
                //XmlNode xmlDS4Mapping = m_Xdoc.CreateNode(XmlNodeType.Element, "UseDS4ForMapping", null); xmlDS4Mapping.InnerText = ds4Mapping.ToString(); rootElement.AppendChild(xmlDS4Mapping);
                XmlNode xmlQuickCharge = m_Xdoc.CreateNode(XmlNodeType.Element, "QuickCharge", null); xmlQuickCharge.InnerText = QuickCharge.ToString(); rootElement.AppendChild(xmlQuickCharge);
                XmlNode xmlCloseMini = m_Xdoc.CreateNode(XmlNodeType.Element, "CloseMinimizes", null); xmlCloseMini.InnerText = CloseMini.ToString(); rootElement.AppendChild(xmlCloseMini);
                XmlNode xmlUseLang = m_Xdoc.CreateNode(XmlNodeType.Element, "UseLang", null); xmlUseLang.InnerText = UseLang.ToString(); rootElement.AppendChild(xmlUseLang);
                XmlNode xmlDownloadLang = m_Xdoc.CreateNode(XmlNodeType.Element, "DownloadLang", null); xmlDownloadLang.InnerText = DownloadLang.ToString(); rootElement.AppendChild(xmlDownloadLang);
                XmlNode xmlFlashWhenLate = m_Xdoc.CreateNode(XmlNodeType.Element, "FlashWhenLate", null); xmlFlashWhenLate.InnerText = FlashWhenLate.ToString(); rootElement.AppendChild(xmlFlashWhenLate);
                XmlNode xmlFlashWhenLateAt = m_Xdoc.CreateNode(XmlNodeType.Element, "FlashWhenLateAt", null); xmlFlashWhenLateAt.InnerText = flashWhenLateAt.ToString(); rootElement.AppendChild(xmlFlashWhenLateAt);
                XmlNode xmlAppIconChoice = m_Xdoc.CreateNode(XmlNodeType.Element, "AppIcon", null); xmlAppIconChoice.InnerText = UseIconChoice.ToString(); rootElement.AppendChild(xmlAppIconChoice);
                XmlNode xmlAppThemeChoice = m_Xdoc.CreateNode(XmlNodeType.Element, "AppTheme", null); xmlAppThemeChoice.InnerText = ThemeChoice.ToString(); rootElement.AppendChild(xmlAppThemeChoice);
                XmlNode xmlUseUDPServ = m_Xdoc.CreateNode(XmlNodeType.Element, "UseUDPServer", null); xmlUseUDPServ.InnerText = UseUdpServer.ToString(); rootElement.AppendChild(xmlUseUDPServ);
                XmlNode xmlUDPServPort = m_Xdoc.CreateNode(XmlNodeType.Element, "UDPServerPort", null); xmlUDPServPort.InnerText = UdpServerPort.ToString(); rootElement.AppendChild(xmlUDPServPort);
                XmlNode xmlUDPServListenAddress = m_Xdoc.CreateNode(XmlNodeType.Element, "UDPServerListenAddress", null); xmlUDPServListenAddress.InnerText = UdpServerListenAddress; rootElement.AppendChild(xmlUDPServListenAddress);

                XmlElement xmlUdpServerSmoothElement = m_Xdoc.CreateElement("UDPServerSmoothingOptions");
                XmlElement xmlUDPUseSmoothing = m_Xdoc.CreateElement("UseSmoothing"); xmlUDPUseSmoothing.InnerText = UseUdpSmoothing.ToString(); xmlUdpServerSmoothElement.AppendChild(xmlUDPUseSmoothing);
                XmlElement xmlUDPSmoothMinCutoff = m_Xdoc.CreateElement("UdpSmoothMinCutoff"); xmlUDPSmoothMinCutoff.InnerText = udpSmoothingMincutoff.ToString(); xmlUdpServerSmoothElement.AppendChild(xmlUDPSmoothMinCutoff);
                XmlElement xmlUDPSmoothBeta = m_Xdoc.CreateElement("UdpSmoothBeta"); xmlUDPSmoothBeta.InnerText = udpSmoothingBeta.ToString(); xmlUdpServerSmoothElement.AppendChild(xmlUDPSmoothBeta);
                rootElement.AppendChild(xmlUdpServerSmoothElement);

                XmlNode xmlUseCustomSteamFolder = m_Xdoc.CreateNode(XmlNodeType.Element, "UseCustomSteamFolder", null); xmlUseCustomSteamFolder.InnerText = UseCustomSteamFolder.ToString(); rootElement.AppendChild(xmlUseCustomSteamFolder);
                XmlNode xmlCustomSteamFolder = m_Xdoc.CreateNode(XmlNodeType.Element, "CustomSteamFolder", null); xmlCustomSteamFolder.InnerText = CustomSteamFolder; rootElement.AppendChild(xmlCustomSteamFolder);
                XmlNode xmlAutoProfileRevertDefaultProfile = m_Xdoc.CreateNode(XmlNodeType.Element, "AutoProfileRevertDefaultProfile", null); xmlAutoProfileRevertDefaultProfile.InnerText = AutoProfileRevertDefaultProfile.ToString(); rootElement.AppendChild(xmlAutoProfileRevertDefaultProfile);

                XmlElement xmlDeviceOptions = m_Xdoc.CreateElement("DeviceOptions", null);
                XmlElement xmlDS4Support = m_Xdoc.CreateElement("DS4SupportSettings", null);
                XmlElement xmlDS4Enabled = m_Xdoc.CreateElement("Enabled", null);
                xmlDS4Enabled.InnerText = DeviceOptions.DS4DeviceOpts.Enabled.ToString();
                xmlDS4Support.AppendChild(xmlDS4Enabled);
                xmlDeviceOptions.AppendChild(xmlDS4Support);

                XmlElement xmlDualSenseSupport = m_Xdoc.CreateElement("DualSenseSupportSettings", null);
                XmlElement xmlDualSenseEnabled = m_Xdoc.CreateElement("Enabled", null);
                xmlDualSenseEnabled.InnerText = DeviceOptions.DualSenseOpts.Enabled.ToString();
                xmlDualSenseSupport.AppendChild(xmlDualSenseEnabled);

                xmlDeviceOptions.AppendChild(xmlDualSenseSupport);

                XmlElement xmlSwitchProSupport = m_Xdoc.CreateElement("SwitchProSupportSettings", null);
                XmlElement xmlSwitchProEnabled = m_Xdoc.CreateElement("Enabled", null);
                xmlSwitchProEnabled.InnerText = DeviceOptions.SwitchProDeviceOpts.Enabled.ToString();
                xmlSwitchProSupport.AppendChild(xmlSwitchProEnabled);

                xmlDeviceOptions.AppendChild(xmlSwitchProSupport);

                XmlElement xmlJoyConSupport = m_Xdoc.CreateElement("JoyConSupportSettings", null);
                XmlElement xmlJoyconEnabled = m_Xdoc.CreateElement("Enabled", null);
                xmlJoyconEnabled.InnerText = DeviceOptions.JoyConDeviceOpts.Enabled.ToString();
                xmlJoyConSupport.AppendChild(xmlJoyconEnabled);
                XmlElement xmlJoyconLinkMode = m_Xdoc.CreateElement("LinkMode", null);
                xmlJoyconLinkMode.InnerText = DeviceOptions.JoyConDeviceOpts.LinkedMode.ToString();
                xmlJoyConSupport.AppendChild(xmlJoyconLinkMode);
                XmlElement xmlJoyconUnionGyro = m_Xdoc.CreateElement("JoinedGyroProvider", null);
                xmlJoyconUnionGyro.InnerText = DeviceOptions.JoyConDeviceOpts.JoinGyroProv.ToString();
                xmlJoyConSupport.AppendChild(xmlJoyconUnionGyro);

                xmlDeviceOptions.AppendChild(xmlJoyConSupport);

                rootElement.AppendChild(xmlDeviceOptions);

                for (int i = 0; i < Global.MAX_DS4_CONTROLLER_COUNT; i++)
                {
                    XmlNode xmlCustomLed = m_Xdoc.CreateNode(XmlNodeType.Element, "CustomLed" + (1 + i), null);
                    xmlCustomLed.InnerText = lightbarSettingInfo[i].ds4winSettings.useCustomLed + ":" + lightbarSettingInfo[i].ds4winSettings.m_CustomLed.red + "," + lightbarSettingInfo[i].ds4winSettings.m_CustomLed.green + "," + lightbarSettingInfo[i].ds4winSettings.m_CustomLed.blue;
                    rootElement.AppendChild(xmlCustomLed);
                }

                m_Xdoc.AppendChild(rootElement);

                try
                {
                    m_Xdoc.Save(ProfilesPath);
                }
                catch (UnauthorizedAccessException) { Saved = false; }

                bool adminNeeded = Global.IsAdminNeeded;
                if (Saved &&
                    (!adminNeeded || (adminNeeded && Global.IsAdministrator)))
                {
                    string custom_exe_name_path = Path.Combine(Global.ExecutableDirectory, Global.CUSTOM_EXE_CONFIG_FILENAME);
                    bool fakeExeFileExists = File.Exists(custom_exe_name_path);
                    if (!string.IsNullOrEmpty(fakeExeFileName) || fakeExeFileExists)
                    {
                        File.WriteAllText(custom_exe_name_path, fakeExeFileName);
                    }
                }

                return Saved;
            }

            private void CreateAction()
            {
                XmlDocument m_Xdoc = new XmlDocument();
                PrepareActionsXml(m_Xdoc);
                m_Xdoc.Save(ActionsPath);
            }

            private void PrepareActionsXml(XmlDocument xmlDoc)
            {
                XmlNode Node;

                Node = xmlDoc.CreateXmlDeclaration("1.0", "utf-8", String.Empty);
                xmlDoc.AppendChild(Node);

                Node = xmlDoc.CreateComment(String.Format(" Special Actions Configuration Data. {0} ", DateTime.Now));
                xmlDoc.AppendChild(Node);

                Node = xmlDoc.CreateWhitespace("\r\n");
                xmlDoc.AppendChild(Node);

                Node = xmlDoc.CreateNode(XmlNodeType.Element, "Actions", "");
                xmlDoc.AppendChild(Node);
            }

            public bool SaveAction(string name, string controls, int mode, string details, bool edit, string extras = "")
            {
                bool saved = true;
                if (!File.Exists(ActionsPath))
                    CreateAction();

                try
                {
                    m_Xdoc.Load(ActionsPath);
                }
                catch (XmlException)
                {
                    // XML file has become corrupt. Start from scratch
                    AppLogger.LogToGui(DS4WinWPF.Properties.Resources.XMLActionsCorrupt, true);
                    m_Xdoc.RemoveAll();
                    PrepareActionsXml(m_Xdoc);
                }

                XmlNode Node;

                Node = m_Xdoc.CreateComment(String.Format(" Special Actions Configuration Data. {0} ", DateTime.Now));
                foreach (XmlNode node in m_Xdoc.SelectNodes("//comment()"))
                    node.ParentNode.ReplaceChild(Node, node);

                Node = m_Xdoc.SelectSingleNode("Actions");
                XmlElement el = m_Xdoc.CreateElement("Action");
                el.SetAttribute("Name", name);
                el.AppendChild(m_Xdoc.CreateElement("Trigger")).InnerText = controls;
                switch (mode)
                {
                    case 1:
                        el.AppendChild(m_Xdoc.CreateElement("Type")).InnerText = "Macro";
                        el.AppendChild(m_Xdoc.CreateElement("Details")).InnerText = details;
                        if (extras != string.Empty)
                            el.AppendChild(m_Xdoc.CreateElement("Extras")).InnerText = extras;
                        break;
                    case 2:
                        el.AppendChild(m_Xdoc.CreateElement("Type")).InnerText = "Program";
                        el.AppendChild(m_Xdoc.CreateElement("Details")).InnerText = details.Split('?')[0];
                        el.AppendChild(m_Xdoc.CreateElement("Arguements")).InnerText = extras;
                        el.AppendChild(m_Xdoc.CreateElement("Delay")).InnerText = details.Split('?')[1];
                        break;
                    case 3:
                        el.AppendChild(m_Xdoc.CreateElement("Type")).InnerText = "Profile";
                        el.AppendChild(m_Xdoc.CreateElement("Details")).InnerText = details;
                        el.AppendChild(m_Xdoc.CreateElement("UnloadTrigger")).InnerText = extras;
                        break;
                    case 4:
                        el.AppendChild(m_Xdoc.CreateElement("Type")).InnerText = "Key";
                        el.AppendChild(m_Xdoc.CreateElement("Details")).InnerText = details;
                        if (!string.IsNullOrEmpty(extras))
                        {
                            string[] exts = extras.Split('\n');
                            el.AppendChild(m_Xdoc.CreateElement("UnloadTrigger")).InnerText = exts[1];
                            el.AppendChild(m_Xdoc.CreateElement("UnloadStyle")).InnerText = exts[0];
                        }
                        break;
                    case 5:
                        el.AppendChild(m_Xdoc.CreateElement("Type")).InnerText = "DisconnectBT";
                        el.AppendChild(m_Xdoc.CreateElement("Details")).InnerText = details;
                        break;
                    case 6:
                        el.AppendChild(m_Xdoc.CreateElement("Type")).InnerText = "BatteryCheck";
                        el.AppendChild(m_Xdoc.CreateElement("Details")).InnerText = details;
                        break;
                    case 7:
                        el.AppendChild(m_Xdoc.CreateElement("Type")).InnerText = "MultiAction";
                        el.AppendChild(m_Xdoc.CreateElement("Details")).InnerText = details;
                        break;
                    case 8:
                        el.AppendChild(m_Xdoc.CreateElement("Type")).InnerText = "SASteeringWheelEmulationCalibrate";
                        el.AppendChild(m_Xdoc.CreateElement("Details")).InnerText = details;
                        break;
                }

                if (edit)
                {
                    XmlNode oldxmlprocess = m_Xdoc.SelectSingleNode("/Actions/Action[@Name=\"" + name + "\"]");
                    Node.ReplaceChild(el, oldxmlprocess);
                }
                else { Node.AppendChild(el); }

                m_Xdoc.AppendChild(Node);
                try { m_Xdoc.Save(ActionsPath); }
                catch { saved = false; }
                LoadActions();
                return saved;
            }

            public void RemoveAction(string name)
            {
                m_Xdoc.Load(ActionsPath);
                XmlNode Node = m_Xdoc.SelectSingleNode("Actions");
                XmlNode Item = m_Xdoc.SelectSingleNode("/Actions/Action[@Name=\"" + name + "\"]");
                if (Item != null)
                    Node.RemoveChild(Item);

                m_Xdoc.AppendChild(Node);
                m_Xdoc.Save(ActionsPath);
                LoadActions();
            }

            public bool LoadActions()
            {
                bool saved = true;
                if (!File.Exists(Path.Combine(Global.RuntimeAppDataPath, Constants.ActionsFileName)))
                {
                    SaveAction("Disconnect Controller", "PS/Options", 5, "0", false);
                    saved = false;
                }

                try
                {
                    Actions.Clear();
                    XmlDocument doc = new XmlDocument();
                    doc.Load(Path.Combine(Global.RuntimeAppDataPath, Constants.ActionsFileName));
                    XmlNodeList actionslist = doc.SelectNodes("Actions/Action");
                    string name, controls, type, details, extras, extras2;
                    Mapping.actionDone.Clear();
                    foreach (XmlNode x in actionslist)
                    {
                        name = x.Attributes["Name"].Value;
                        controls = x.ChildNodes[0].InnerText;
                        type = x.ChildNodes[1].InnerText;
                        details = x.ChildNodes[2].InnerText;
                        Mapping.actionDone.Add(new Mapping.ActionState());
                        if (type == "Profile")
                        {
                            extras = x.ChildNodes[3].InnerText;
                            Actions.Add(new SpecialAction(name, controls, type, details, 0, extras));
                        }
                        else if (type == "Macro")
                        {
                            if (x.ChildNodes[3] != null) extras = x.ChildNodes[3].InnerText;
                            else extras = string.Empty;
                            Actions.Add(new SpecialAction(name, controls, type, details, 0, extras));
                        }
                        else if (type == "Key")
                        {
                            if (x.ChildNodes[3] != null)
                            {
                                extras = x.ChildNodes[3].InnerText;
                                extras2 = x.ChildNodes[4].InnerText;
                            }
                            else
                            {
                                extras = string.Empty;
                                extras2 = string.Empty;
                            }
                            if (!string.IsNullOrEmpty(extras))
                                Actions.Add(new SpecialAction(name, controls, type, details, 0, extras2 + '\n' + extras));
                            else
                                Actions.Add(new SpecialAction(name, controls, type, details));
                        }
                        else if (type == "DisconnectBT")
                        {
                            double doub;
                            if (double.TryParse(details, System.Globalization.NumberStyles.Float, Global.ConfigFileDecimalCulture, out doub))
                                Actions.Add(new SpecialAction(name, controls, type, "", doub));
                            else
                                Actions.Add(new SpecialAction(name, controls, type, ""));
                        }
                        else if (type == "BatteryCheck")
                        {
                            double doub;
                            if (double.TryParse(details.Split('|')[0], System.Globalization.NumberStyles.Float, Global.ConfigFileDecimalCulture, out doub))
                                Actions.Add(new SpecialAction(name, controls, type, details, doub));
                            else if (double.TryParse(details.Split(',')[0], System.Globalization.NumberStyles.Float, Global.ConfigFileDecimalCulture, out doub))
                                Actions.Add(new SpecialAction(name, controls, type, details, doub));
                            else
                                Actions.Add(new SpecialAction(name, controls, type, details));
                        }
                        else if (type == "Program")
                        {
                            double doub;
                            if (x.ChildNodes[3] != null)
                            {
                                extras = x.ChildNodes[3].InnerText;
                                if (double.TryParse(x.ChildNodes[4].InnerText, System.Globalization.NumberStyles.Float, Global.ConfigFileDecimalCulture, out doub))
                                    Actions.Add(new SpecialAction(name, controls, type, details, doub, extras));
                                else
                                    Actions.Add(new SpecialAction(name, controls, type, details, 0, extras));
                            }
                            else
                            {
                                Actions.Add(new SpecialAction(name, controls, type, details));
                            }
                        }
                        else if (type == "XboxGameDVR" || type == "MultiAction")
                        {
                            Actions.Add(new SpecialAction(name, controls, type, details));
                        }
                        else if (type == "SASteeringWheelEmulationCalibrate")
                        {
                            double doub;
                            if (double.TryParse(details, System.Globalization.NumberStyles.Float, Global.ConfigFileDecimalCulture, out doub))
                                Actions.Add(new SpecialAction(name, controls, type, "", doub));
                            else
                                Actions.Add(new SpecialAction(name, controls, type, ""));
                        }
                    }
                }
                catch { saved = false; }
                return saved;
            }

            public bool CreateLinkedProfiles()
            {
                bool saved = true;
                XmlDocument m_Xdoc = new XmlDocument();
                XmlNode Node;

                Node = m_Xdoc.CreateXmlDeclaration("1.0", "utf-8", string.Empty);
                m_Xdoc.AppendChild(Node);

                Node = m_Xdoc.CreateComment(string.Format(" Mac Address and Profile Linking Data. {0} ", DateTime.Now));
                m_Xdoc.AppendChild(Node);

                Node = m_Xdoc.CreateWhitespace("\r\n");
                m_Xdoc.AppendChild(Node);

                Node = m_Xdoc.CreateNode(XmlNodeType.Element, "LinkedControllers", "");
                m_Xdoc.AppendChild(Node);

                try { m_Xdoc.Save(LinkedProfilesPath); }
                catch (UnauthorizedAccessException) { AppLogger.LogToGui("Unauthorized Access - Save failed to path: " + LinkedProfilesPath, false); saved = false; }

                return saved;
            }

            public bool LoadLinkedProfiles()
            {
                bool loaded = true;
                if (File.Exists(LinkedProfilesPath))
                {
                    XmlDocument linkedXdoc = new XmlDocument();
                    XmlNode Node;
                    linkedXdoc.Load(LinkedProfilesPath);
                    linkedProfiles.Clear();

                    try
                    {
                        Node = linkedXdoc.SelectSingleNode("/LinkedControllers");
                        XmlNodeList links = Node.ChildNodes;
                        for (int i = 0, listLen = links.Count; i < listLen; i++)
                        {
                            XmlNode current = links[i];
                            string serial = current.Name.Replace("MAC", string.Empty);
                            string profile = current.InnerText;
                            linkedProfiles[serial] = profile;
                        }
                    }
                    catch { loaded = false; }
                }
                else
                {
                    AppLogger.LogToGui("LinkedProfiles.xml can't be found.", false);
                    loaded = false;
                }

                return loaded;
            }

            public bool SaveLinkedProfiles()
            {
                bool saved = true;
                if (File.Exists(LinkedProfilesPath))
                {
                    XmlDocument linkedXdoc = new XmlDocument();
                    XmlNode Node;

                    Node = linkedXdoc.CreateXmlDeclaration("1.0", "utf-8", string.Empty);
                    linkedXdoc.AppendChild(Node);

                    Node = linkedXdoc.CreateComment(string.Format(" Mac Address and Profile Linking Data. {0} ", DateTime.Now));
                    linkedXdoc.AppendChild(Node);

                    Node = linkedXdoc.CreateWhitespace("\r\n");
                    linkedXdoc.AppendChild(Node);

                    Node = linkedXdoc.CreateNode(XmlNodeType.Element, "LinkedControllers", "");
                    linkedXdoc.AppendChild(Node);

                    Dictionary<string, string>.KeyCollection serials = linkedProfiles.Keys;
                    //for (int i = 0, itemCount = linkedProfiles.Count; i < itemCount; i++)
                    for (var serialEnum = serials.GetEnumerator(); serialEnum.MoveNext();)
                    {
                        //string serial = serials.ElementAt(i);
                        string serial = serialEnum.Current;
                        string profile = linkedProfiles[serial];
                        XmlElement link = linkedXdoc.CreateElement("MAC" + serial);
                        link.InnerText = profile;
                        Node.AppendChild(link);
                    }

                    try { linkedXdoc.Save(LinkedProfilesPath); }
                    catch (UnauthorizedAccessException) { AppLogger.LogToGui("Unauthorized Access - Save failed to path: " + LinkedProfilesPath, false); saved = false; }
                }
                else
                {
                    saved = CreateLinkedProfiles();
                    saved = saved && SaveLinkedProfiles();
                }

                return saved;
            }

            public bool CreateControllerConfigs()
            {
                bool saved = true;
                XmlDocument configXdoc = new XmlDocument();
                XmlNode Node;

                Node = configXdoc.CreateXmlDeclaration("1.0", "utf-8", string.Empty);
                configXdoc.AppendChild(Node);

                Node = configXdoc.CreateComment(string.Format(" Controller config data. {0} ", DateTime.Now));
                configXdoc.AppendChild(Node);

                Node = configXdoc.CreateWhitespace("\r\n");
                configXdoc.AppendChild(Node);

                Node = configXdoc.CreateNode(XmlNodeType.Element, "Controllers", "");
                configXdoc.AppendChild(Node);

                try { configXdoc.Save(ControllerConfigsPath); }
                catch (UnauthorizedAccessException) { AppLogger.LogToGui("Unauthorized Access - Save failed to path: " + ControllerConfigsPath, false); saved = false; }

                return saved;
            }

            public bool LoadControllerConfigsForDevice(DS4Device device)
            {
                bool loaded = false;

                if (device == null) return false;
                if (!File.Exists(ControllerConfigsPath)) CreateControllerConfigs();

                try
                {
                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.Load(ControllerConfigsPath);

                    XmlNode node = xmlDoc.SelectSingleNode("/Controllers/Controller[@Mac=\"" + device.getMacAddress() + "\"]");
                    if (node != null)
                    {
                        Int32 intValue;
                        if (Int32.TryParse(node["wheelCenterPoint"]?.InnerText.Split(',')[0] ?? "", out intValue)) device.wheelCenterPoint.X = intValue;
                        if (Int32.TryParse(node["wheelCenterPoint"]?.InnerText.Split(',')[1] ?? "", out intValue)) device.wheelCenterPoint.Y = intValue;
                        if (Int32.TryParse(node["wheel90DegPointLeft"]?.InnerText.Split(',')[0] ?? "", out intValue)) device.wheel90DegPointLeft.X = intValue;
                        if (Int32.TryParse(node["wheel90DegPointLeft"]?.InnerText.Split(',')[1] ?? "", out intValue)) device.wheel90DegPointLeft.Y = intValue;
                        if (Int32.TryParse(node["wheel90DegPointRight"]?.InnerText.Split(',')[0] ?? "", out intValue)) device.wheel90DegPointRight.X = intValue;
                        if (Int32.TryParse(node["wheel90DegPointRight"]?.InnerText.Split(',')[1] ?? "", out intValue)) device.wheel90DegPointRight.Y = intValue;

                        device.optionsStore.LoadSettings(xmlDoc, node);

                        loaded = true;
                    }
                }
                catch
                {
                    AppLogger.LogToGui("ControllerConfigs.xml can't be found.", false);
                    loaded = false;
                }

                return loaded;
            }

            public bool SaveControllerConfigsForDevice(DS4Device device)
            {
                bool saved = true;

                if (device == null) return false;
                if (!File.Exists(ControllerConfigsPath)) CreateControllerConfigs();

                try
                {
                    //XmlNode node = null;
                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.Load(ControllerConfigsPath);

                    XmlNode node = xmlDoc.SelectSingleNode("/Controllers/Controller[@Mac=\"" + device.getMacAddress() + "\"]");
                    XmlNode xmlControllersNode = xmlDoc.SelectSingleNode("/Controllers");
                    if (node == null)
                    {
                        XmlElement el = xmlDoc.CreateElement("Controller");
                        node = xmlControllersNode.AppendChild(el);
                    }
                    else
                    {
                        node.RemoveAll();
                    }

                    XmlAttribute macAttr = xmlDoc.CreateAttribute("Mac");
                    macAttr.Value = device.getMacAddress();
                    node.Attributes.Append(macAttr);

                    XmlAttribute contTypeAttr = xmlDoc.CreateAttribute("ControllerType");
                    contTypeAttr.Value = device.DeviceType.ToString();
                    node.Attributes.Append(contTypeAttr);

                    if (!device.wheelCenterPoint.IsEmpty)
                    {
                        XmlElement wheelCenterEl = xmlDoc.CreateElement("wheelCenterPoint");
                        wheelCenterEl.InnerText = $"{device.wheelCenterPoint.X},{device.wheelCenterPoint.Y}";
                        node.AppendChild(wheelCenterEl);

                        XmlElement wheel90DegPointLeftEl = xmlDoc.CreateElement("wheel90DegPointLeft");
                        wheel90DegPointLeftEl.InnerText = $"{device.wheel90DegPointLeft.X},{device.wheel90DegPointLeft.Y}";
                        node.AppendChild(wheel90DegPointLeftEl);

                        XmlElement wheel90DegPointRightEl = xmlDoc.CreateElement("wheel90DegPointRight");
                        wheel90DegPointRightEl.InnerText = $"{device.wheel90DegPointRight.X},{device.wheel90DegPointRight.Y}";
                        node.AppendChild(wheel90DegPointRightEl);
                    }

                    device.optionsStore.PersistSettings(xmlDoc, node);

                    // Remove old elements
                    xmlDoc.RemoveAll();

                    XmlNode Node;
                    Node = xmlDoc.CreateXmlDeclaration("1.0", "utf-8", string.Empty);
                    xmlDoc.AppendChild(Node);

                    Node = xmlDoc.CreateComment(string.Format(" Controller config data. {0} ", DateTime.Now));
                    xmlDoc.AppendChild(Node);

                    Node = xmlDoc.CreateWhitespace("\r\n");
                    xmlDoc.AppendChild(Node);

                    // Write old Controllers node back in
                    xmlDoc.AppendChild(xmlControllersNode);

                    // Save XML to file
                    xmlDoc.Save(ControllerConfigsPath);
                }
                catch (UnauthorizedAccessException)
                {
                    AppLogger.LogToGui("Unauthorized Access - Save failed to path: " + ControllerConfigsPath, false);
                    saved = false;
                }

                return saved;
            }

            public void UpdateDs4ControllerSetting(int deviceNum, string buttonName, bool shift, object action, string exts,
                DS4KeyType kt, int trigger = 0)
            {
                DS4Controls dc;
                if (buttonName.StartsWith("bn"))
                    dc = GetDs4ControlsByName(buttonName);
                else
                    dc = (DS4Controls)Enum.Parse(typeof(DS4Controls), buttonName, true);

                var temp = (int)dc;
                if (temp > 0)
                {
                    var index = temp - 1;
                    var dcs = Ds4Settings[deviceNum][index];
                    dcs.UpdateSettings(shift, action, exts, kt, trigger);
                    Global.RefreshActionAlias(dcs, shift);
                }
            }

            public void UpdateDs4ControllerExtra(int deviceNum, string buttonName, bool shift, string exts)
            {
                DS4Controls dc;
                if (buttonName.StartsWith("bn"))
                    dc = GetDs4ControlsByName(buttonName);
                else
                    dc = (DS4Controls)Enum.Parse(typeof(DS4Controls), buttonName, true);

                int temp = (int)dc;
                if (temp > 0)
                {
                    int index = temp - 1;
                    DS4ControlSettings dcs = Ds4Settings[deviceNum][index];
                    if (shift)
                        dcs.ShiftExtras = exts;
                    else
                        dcs.Extras = exts;
                }
            }

            private void UpdateDs4ControllerKeyType(int deviceNum, string buttonName, bool shift, DS4KeyType keyType)
            {
                DS4Controls dc;
                if (buttonName.StartsWith("bn"))
                    dc = GetDs4ControlsByName(buttonName);
                else
                    dc = (DS4Controls)Enum.Parse(typeof(DS4Controls), buttonName, true);

                var temp = (int)dc;
                if (temp > 0)
                {
                    var index = temp - 1;
                    var dcs = Ds4Settings[deviceNum][index];
                    if (shift)
                        dcs.ShiftKeyType = keyType;
                    else
                        dcs.KeyType = keyType;
                }
            }

            public ControlActionData GetDs4Action(int deviceNum, string buttonName, bool shift)
            {
                DS4Controls dc;
                if (buttonName.StartsWith("bn"))
                    dc = GetDs4ControlsByName(buttonName);
                else
                    dc = (DS4Controls)Enum.Parse(typeof(DS4Controls), buttonName, true);

                var temp = (int)dc;
                if (temp > 0)
                {
                    var index = temp - 1;
                    var dcs = Ds4Settings[deviceNum][index];
                    if (shift)
                        return dcs.ShiftAction;
                    return dcs.ActionData;
                }

                return null;
            }

            public ControlActionData GetDs4Action(int deviceNum, DS4Controls dc, bool shift)
            {
                int temp = (int)dc;
                if (temp > 0)
                {
                    int index = temp - 1;
                    DS4ControlSettings dcs = Ds4Settings[deviceNum][index];
                    if (shift)
                    {
                        return dcs.ShiftAction;
                    }
                    else
                    {
                        return dcs.ActionData;
                    }
                }

                return null;
            }

            public string GetDs4Extra(int deviceNum, string buttonName, bool shift)
            {
                DS4Controls dc;
                if (buttonName.StartsWith("bn"))
                    dc = GetDs4ControlsByName(buttonName);
                else
                    dc = (DS4Controls)Enum.Parse(typeof(DS4Controls), buttonName, true);

                var temp = (int)dc;
                if (temp > 0)
                {
                    var index = temp - 1;
                    var dcs = Ds4Settings[deviceNum][index];
                    if (shift)
                        return dcs.ShiftExtras;
                    return dcs.Extras;
                }

                return null;
            }

            public DS4KeyType GetDs4KeyType(int deviceNum, string buttonName, bool shift)
            {
                DS4Controls dc;
                if (buttonName.StartsWith("bn"))
                    dc = GetDs4ControlsByName(buttonName);
                else
                    dc = (DS4Controls)Enum.Parse(typeof(DS4Controls), buttonName, true);

                var temp = (int)dc;
                if (temp > 0)
                {
                    var index = temp - 1;
                    var dcs = Ds4Settings[deviceNum][index];
                    if (shift)
                        return dcs.ShiftKeyType;
                    return dcs.KeyType;
                }

                return DS4KeyType.None;
            }

            public int GetDs4STrigger(int deviceNum, string buttonName)
            {
                DS4Controls dc;
                if (buttonName.StartsWith("bn"))
                    dc = GetDs4ControlsByName(buttonName);
                else
                    dc = (DS4Controls)Enum.Parse(typeof(DS4Controls), buttonName, true);

                var temp = (int)dc;
                if (temp > 0)
                {
                    var index = temp - 1;
                    var dcs = Ds4Settings[deviceNum][index];
                    return dcs.ShiftTrigger;
                }

                return 0;
            }

            public int GetDs4STrigger(int deviceNum, DS4Controls dc)
            {
                int temp = (int)dc;
                if (temp > 0)
                {
                    int index = temp - 1;
                    DS4ControlSettings dcs = Ds4Settings[deviceNum][index];
                    return dcs.ShiftTrigger;
                }

                return 0;
            }

            public DS4ControlSettings GetDs4ControllerSetting(int deviceNum, string buttonName)
            {
                DS4Controls dc;
                if (buttonName.StartsWith("bn"))
                    dc = GetDs4ControlsByName(buttonName);
                else
                    dc = (DS4Controls)Enum.Parse(typeof(DS4Controls), buttonName, true);

                int temp = (int)dc;
                if (temp > 0)
                {
                    int index = temp - 1;
                    DS4ControlSettings dcs = Ds4Settings[deviceNum][index];
                    return dcs;
                }

                return null;
            }

            public DS4ControlSettings GetDs4ControllerSetting(int deviceNum, DS4Controls dc)
            {
                int temp = (int)dc;
                if (temp > 0)
                {
                    int index = temp - 1;
                    DS4ControlSettings dcs = Ds4Settings[deviceNum][index];
                    return dcs;
                }

                return null;
            }

            public bool HasCustomActions(int deviceNum)
            {
                var ds4settingsList = Ds4Settings[deviceNum];
                for (int i = 0, settingsLen = ds4settingsList.Count; i < settingsLen; i++)
                {
                    DS4ControlSettings dcs = ds4settingsList[i];
                    if (dcs.ControlActionType != DS4ControlSettings.ActionType.Default || dcs.ShiftActionType != DS4ControlSettings.ActionType.Default)
                        return true;
                }

                return false;
            }

            public bool HasCustomExtras(int deviceNum)
            {
                var ds4settingsList = Ds4Settings[deviceNum];
                for (int i = 0, settingsLen = ds4settingsList.Count; i < settingsLen; i++)
                {
                    DS4ControlSettings dcs = ds4settingsList[i];
                    if (dcs.Extras != null || dcs.ShiftExtras != null)
                        return true;
                }

                return false;
            }

            private void ResetMouseProperties(int device, ControlService control)
            {
                if (device < Global.MAX_DS4_CONTROLLER_COUNT &&
                        control.touchPad[device] != null)
                {
                    control.touchPad[device]?.ResetToggleGyroModes();
                }
            }

            private void ResetProfile(int device)
            {
                ButtonMouseInfos[device].Reset();
                gyroControlsInf[device].Reset();

                EnableTouchToggle[device] = true;
                IdleDisconnectTimeout[device] = 0;
                EnableOutputDataToDS4[device] = true;
                TouchpadJitterCompensation[device] = true;
                LowerRCOn[device] = false;
                TouchClickPassthru[device] = false;

                RumbleBoost[device] = 100;
                RumbleAutostopTime[device] = 0;
                TouchSensitivity[device] = 100;

                LSModInfo[device].Reset();
                RSModInfo[device].Reset();
                LSModInfo[device].deadZone = RSModInfo[device].deadZone = 10;
                LSModInfo[device].antiDeadZone = RSModInfo[device].antiDeadZone = 20;
                LSModInfo[device].maxZone = RSModInfo[device].maxZone = 100;
                LSModInfo[device].maxOutput = RSModInfo[device].maxOutput = 100.0;
                LSModInfo[device].fuzz = RSModInfo[device].fuzz = StickDeadZoneInfo.DEFAULT_FUZZ;

                //l2ModInfo[device].deadZone = r2ModInfo[device].deadZone = 0;
                //l2ModInfo[device].antiDeadZone = r2ModInfo[device].antiDeadZone = 0;
                //l2ModInfo[device].maxZone = r2ModInfo[device].maxZone = 100;
                //l2ModInfo[device].maxOutput = r2ModInfo[device].maxOutput = 100.0;
                L2ModInfo[device].Reset();
                R2ModInfo[device].Reset();

                LSRotation[device] = 0.0;
                RSRotation[device] = 0.0;
                SXDeadzone[device] = SZDeadzone[device] = 0.25;
                SXMaxzone[device] = SZMaxzone[device] = 1.0;
                SXAntiDeadzone[device] = SZAntiDeadzone[device] = 0.0;
                L2Sens[device] = R2Sens[device] = 1;
                LSSens[device] = RSSens[device] = 1;
                SXSens[device] = SZSens[device] = 1;
                tapSensitivity[device] = 0;
                doubleTap[device] = false;
                scrollSensitivity[device] = 0;
                touchpadInvert[device] = 0;
                btPollRate[device] = 4;

                LSOutputSettings[device].ResetSettings();
                RSOutputSettings[device].ResetSettings();
                L2OutputSettings[device].ResetSettings();
                R2OutputSettings[device].ResetSettings();

                LightbarSettingInfo lightbarSettings = lightbarSettingInfo[device];
                LightbarDS4WinInfo lightInfo = lightbarSettings.ds4winSettings;
                lightbarSettings.Mode = LightbarMode.DS4Win;
                lightInfo.m_LowLed = new DS4Color(Color.Black);
                //m_LowLeds[device] = new DS4Color(Color.Black);

                Color tempColor = Color.Blue;
                switch (device)
                {
                    case 0: tempColor = Color.Blue; break;
                    case 1: tempColor = Color.Red; break;
                    case 2: tempColor = Color.Green; break;
                    case 3: tempColor = Color.Pink; break;
                    case 4: tempColor = Color.Blue; break;
                    case 5: tempColor = Color.Red; break;
                    case 6: tempColor = Color.Green; break;
                    case 7: tempColor = Color.Pink; break;
                    case 8: tempColor = Color.White; break;
                    default: tempColor = Color.Blue; break;
                }

                lightInfo.m_Led = new DS4Color(tempColor);
                lightInfo.m_ChargingLed = new DS4Color(Color.Black);
                lightInfo.m_FlashLed = new DS4Color(Color.Black);
                lightInfo.flashAt = 0;
                lightInfo.flashType = 0;
                lightInfo.chargingType = 0;
                lightInfo.rainbow = 0;
                lightInfo.maxRainbowSat = 1.0;
                lightInfo.ledAsBattery = false;

                LaunchProgram[device] = string.Empty;
                dinputOnly[device] = false;
                startTouchpadOff[device] = false;
                touchOutMode[device] = TouchpadOutMode.Mouse;
                sATriggers[device] = "-1";
                sATriggerCond[device] = true;
                gyroOutMode[device] = GyroOutMode.Controls;
                sAMouseStickTriggers[device] = "-1";
                sAMouseStickTriggerCond[device] = true;

                gyroMStickInfo[device].Reset();
                gyroSwipeInfo[device].Reset();

                gyroMouseStickToggle[device] = false;
                gyroMouseStickTriggerTurns[device] = true;
                sASteeringWheelEmulationAxis[device] = SASteeringWheelEmulationAxisType.None;
                sASteeringWheelEmulationRange[device] = 360;
                saWheelFuzzValues[device] = 0;
                WheelSmoothInfo[device].Reset();
                touchDisInvertTriggers[device] = new int[1] { -1 };
                gyroSensitivity[device] = 100;
                gyroSensVerticalScale[device] = 100;
                gyroInvert[device] = 0;
                gyroTriggerTurns[device] = true;
                gyroMouseInfo[device].Reset();

                gyroMouseHorizontalAxis[device] = 0;
                gyroMouseToggle[device] = false;
                SquStickInfo[device].lsMode = false;
                SquStickInfo[device].rsMode = false;
                SquStickInfo[device].lsRoundness = 5.0;
                SquStickInfo[device].rsRoundness = 5.0;
                LSAntiSnapbackInfo[device].timeout = StickAntiSnapbackInfo.DEFAULT_TIMEOUT;
                LSAntiSnapbackInfo[device].delta = StickAntiSnapbackInfo.DEFAULT_DELTA;
                LSAntiSnapbackInfo[device].enabled = StickAntiSnapbackInfo.DEFAULT_ENABLED;
                setLsOutCurveMode(device, 0);
                setRsOutCurveMode(device, 0);
                setL2OutCurveMode(device, 0);
                setR2OutCurveMode(device, 0);
                setSXOutCurveMode(device, 0);
                setSZOutCurveMode(device, 0);
                trackballMode[device] = false;
                TrackballFriction[device] = 10.0;
                TouchpadAbsMouse[device].Reset();
                TouchpadRelMouse[device].Reset();
                OutputDeviceType[device] = DS4Windows.OutContType.X360;
                Ds4Mapping = false;
            }

            private void PrepareBlankingProfile(int device, ControlService control, out bool xinputPlug, out bool xinputStatus, bool xinputChange = true)
            {
                xinputPlug = false;
                xinputStatus = false;

                OutContType oldContType = Global.ActiveOutDevType[device];

                // Make sure to reset currently set profile values before parsing
                ResetProfile(device);
                ResetMouseProperties(device, control);

                // Only change xinput devices under certain conditions. Avoid
                // performing this upon program startup before loading devices.
                if (xinputChange)
                {
                    CheckOldDeviceStatus(device, control, oldContType,
                        out xinputPlug, out xinputStatus);
                }

                foreach (DS4ControlSettings dcs in Ds4Settings[device])
                    dcs.Reset();

                ProfileActions[device].Clear();
                containsCustomAction[device] = false;
                containsCustomExtras[device] = false;
            }

            public void LoadBlankDs4Profile(int device, bool launchprogram, ControlService control,
                string propath = "", bool xinputChange = true, bool postLoad = true)
            {
                PrepareBlankingProfile(device, control, out bool xinputPlug, out bool xinputStatus, xinputChange);

                StickDeadZoneInfo lsInfo = LSModInfo[device];
                lsInfo.deadZone = (int)(0.00 * 127);
                lsInfo.antiDeadZone = 0;
                lsInfo.maxZone = 100;

                StickDeadZoneInfo rsInfo = RSModInfo[device];
                rsInfo.deadZone = (int)(0.00 * 127);
                rsInfo.antiDeadZone = 0;
                rsInfo.maxZone = 100;

                TriggerDeadZoneZInfo l2Info = L2ModInfo[device];
                l2Info.deadZone = (byte)(0.00 * 255);

                TriggerDeadZoneZInfo r2Info = R2ModInfo[device];
                r2Info.deadZone = (byte)(0.00 * 255);

                OutputDeviceType[device] = DS4Windows.OutContType.DS4;

                // If a device exists, make sure to transfer relevant profile device
                // options to device instance
                if (postLoad && device < Global.MAX_DS4_CONTROLLER_COUNT)
                {
                    PostLoadSnippet(device, control, xinputStatus, xinputPlug);
                }
            }

            public void LoadBlankProfile(int device, bool launchprogram, ControlService control,
                string propath = "", bool xinputChange = true, bool postLoad = true)
            {
                bool xinputPlug = false;
                bool xinputStatus = false;

                OutContType oldContType = Global.ActiveOutDevType[device];

                // Make sure to reset currently set profile values before parsing
                ResetProfile(device);
                ResetMouseProperties(device, control);

                // Only change xinput devices under certain conditions. Avoid
                // performing this upon program startup before loading devices.
                if (xinputChange)
                {
                    CheckOldDeviceStatus(device, control, oldContType,
                        out xinputPlug, out xinputStatus);
                }

                foreach (DS4ControlSettings dcs in Ds4Settings[device])
                    dcs.Reset();

                ProfileActions[device].Clear();
                containsCustomAction[device] = false;
                containsCustomExtras[device] = false;

                // If a device exists, make sure to transfer relevant profile device
                // options to device instance
                if (postLoad && device < Global.MAX_DS4_CONTROLLER_COUNT)
                {
                    PostLoadSnippet(device, control, xinputStatus, xinputPlug);
                }
            }

            public void LoadDefaultGamepadGyroProfile(int device, bool launchprogram, ControlService control,
                string propath = "", bool xinputChange = true, bool postLoad = true)
            {
                bool xinputPlug = false;
                bool xinputStatus = false;

                OutContType oldContType = Global.ActiveOutDevType[device];

                // Make sure to reset currently set profile values before parsing
                ResetProfile(device);
                ResetMouseProperties(device, control);

                // Only change xinput devices under certain conditions. Avoid
                // performing this upon program startup before loading devices.
                if (xinputChange)
                {
                    CheckOldDeviceStatus(device, control, oldContType,
                        out xinputPlug, out xinputStatus);
                }

                foreach (DS4ControlSettings dcs in Ds4Settings[device])
                    dcs.Reset();

                ProfileActions[device].Clear();
                containsCustomAction[device] = false;
                containsCustomExtras[device] = false;

                gyroOutMode[device] = GyroOutMode.MouseJoystick;
                sAMouseStickTriggers[device] = "4";
                sAMouseStickTriggerCond[device] = true;
                gyroMouseStickTriggerTurns[device] = false;
                gyroMStickInfo[device].useSmoothing = true;
                gyroMStickInfo[device].smoothingMethod = GyroMouseStickInfo.SmoothingMethod.OneEuro;

                // If a device exists, make sure to transfer relevant profile device
                // options to device instance
                if (postLoad && device < Global.MAX_DS4_CONTROLLER_COUNT)
                {
                    PostLoadSnippet(device, control, xinputStatus, xinputPlug);
                }
            }

            public void LoadDefaultDS4GamepadGyroProfile(int device, bool launchprogram, ControlService control,
                string propath = "", bool xinputChange = true, bool postLoad = true)
            {
                PrepareBlankingProfile(device, control, out bool xinputPlug, out bool xinputStatus, xinputChange);

                StickDeadZoneInfo lsInfo = LSModInfo[device];
                lsInfo.deadZone = (int)(0.00 * 127);
                lsInfo.antiDeadZone = 0;
                lsInfo.maxZone = 100;

                StickDeadZoneInfo rsInfo = RSModInfo[device];
                rsInfo.deadZone = (int)(0.00 * 127);
                rsInfo.antiDeadZone = 0;
                rsInfo.maxZone = 100;

                TriggerDeadZoneZInfo l2Info = L2ModInfo[device];
                l2Info.deadZone = (byte)(0.00 * 255);

                TriggerDeadZoneZInfo r2Info = R2ModInfo[device];
                r2Info.deadZone = (byte)(0.00 * 255);

                gyroOutMode[device] = GyroOutMode.MouseJoystick;
                sAMouseStickTriggers[device] = "4";
                sAMouseStickTriggerCond[device] = true;
                gyroMouseStickTriggerTurns[device] = false;
                gyroMStickInfo[device].useSmoothing = true;
                gyroMStickInfo[device].smoothingMethod = GyroMouseStickInfo.SmoothingMethod.OneEuro;

                OutputDeviceType[device] = DS4Windows.OutContType.DS4;

                // If a device exists, make sure to transfer relevant profile device
                // options to device instance
                if (postLoad && device < Global.MAX_DS4_CONTROLLER_COUNT)
                {
                    PostLoadSnippet(device, control, xinputStatus, xinputPlug);
                }
            }

            public void LoadDefaultMixedGyroMouseProfile(int device, bool launchprogram, ControlService control,
                string propath = "", bool xinputChange = true, bool postLoad = true)
            {
                bool xinputPlug = false;
                bool xinputStatus = false;

                OutContType oldContType = Global.ActiveOutDevType[device];

                // Make sure to reset currently set profile values before parsing
                ResetProfile(device);
                ResetMouseProperties(device, control);

                // Only change xinput devices under certain conditions. Avoid
                // performing this upon program startup before loading devices.
                if (xinputChange)
                {
                    CheckOldDeviceStatus(device, control, oldContType,
                        out xinputPlug, out xinputStatus);
                }

                foreach (DS4ControlSettings dcs in Ds4Settings[device])
                    dcs.Reset();

                ProfileActions[device].Clear();
                containsCustomAction[device] = false;
                containsCustomExtras[device] = false;

                gyroOutMode[device] = GyroOutMode.Mouse;
                sATriggers[device] = "4";
                sATriggerCond[device] = true;
                gyroTriggerTurns[device] = false;
                gyroMouseInfo[device].enableSmoothing = true;
                gyroMouseInfo[device].smoothingMethod = DS4Windows.GyroMouseInfo.SmoothingMethod.OneEuro;

                StickDeadZoneInfo rsInfo = RSModInfo[device];
                rsInfo.deadZone = (int)(0.10 * 127);
                rsInfo.antiDeadZone = 0;
                rsInfo.maxZone = 90;

                // If a device exists, make sure to transfer relevant profile device
                // options to device instance
                if (postLoad && device < Global.MAX_DS4_CONTROLLER_COUNT)
                {
                    PostLoadSnippet(device, control, xinputStatus, xinputPlug);
                }
            }

            public void LoadDefaultDs4MixedGyroMouseProfile(int device, bool launchprogram, ControlService control,
                string propath = "", bool xinputChange = true, bool postLoad = true)
            {
                PrepareBlankingProfile(device, control, out bool xinputPlug, out bool xinputStatus, xinputChange);

                StickDeadZoneInfo lsInfo = LSModInfo[device];
                lsInfo.deadZone = (int)(0.00 * 127);
                lsInfo.antiDeadZone = 0;
                lsInfo.maxZone = 100;

                StickDeadZoneInfo rsInfo = RSModInfo[device];
                rsInfo.deadZone = (int)(0.10 * 127);
                rsInfo.antiDeadZone = 0;
                rsInfo.maxZone = 100;

                TriggerDeadZoneZInfo l2Info = L2ModInfo[device];
                l2Info.deadZone = (byte)(0.00 * 255);

                TriggerDeadZoneZInfo r2Info = R2ModInfo[device];
                r2Info.deadZone = (byte)(0.00 * 255);

                gyroOutMode[device] = GyroOutMode.Mouse;
                sATriggers[device] = "4";
                sATriggerCond[device] = true;
                gyroTriggerTurns[device] = false;
                gyroMouseInfo[device].enableSmoothing = true;
                gyroMouseInfo[device].smoothingMethod = DS4Windows.GyroMouseInfo.SmoothingMethod.OneEuro;

                OutputDeviceType[device] = DS4Windows.OutContType.DS4;

                // If a device exists, make sure to transfer relevant profile device
                // options to device instance
                if (postLoad && device < Global.MAX_DS4_CONTROLLER_COUNT)
                {
                    PostLoadSnippet(device, control, xinputStatus, xinputPlug);
                }
            }

            public void LoadDefaultDS4MixedControlsProfile(int device, bool launchprogram, ControlService control,
                string propath = "", bool xinputChange = true, bool postLoad = true)
            {
                PrepareBlankingProfile(device, control, out bool xinputPlug, out bool xinputStatus, xinputChange);

                DS4ControlSettings setting = GetDs4ControllerSetting(device, DS4Controls.RYNeg);
                setting.UpdateSettings(false, X360Controls.MouseUp, "", DS4KeyType.None);
                setting = GetDs4ControllerSetting(device, DS4Controls.RYPos);
                setting.UpdateSettings(false, X360Controls.MouseDown, "", DS4KeyType.None);
                setting = GetDs4ControllerSetting(device, DS4Controls.RXNeg);
                setting.UpdateSettings(false, X360Controls.MouseLeft, "", DS4KeyType.None);
                setting = GetDs4ControllerSetting(device, DS4Controls.RXPos);
                setting.UpdateSettings(false, X360Controls.MouseRight, "", DS4KeyType.None);

                StickDeadZoneInfo rsInfo = RSModInfo[device];
                rsInfo.deadZone = (int)(0.035 * 127);
                rsInfo.antiDeadZone = 0;
                rsInfo.maxZone = 90;

                OutputDeviceType[device] = DS4Windows.OutContType.DS4;

                // If a device exists, make sure to transfer relevant profile device
                // options to device instance
                if (postLoad && device < Global.MAX_DS4_CONTROLLER_COUNT)
                {
                    PostLoadSnippet(device, control, xinputStatus, xinputPlug);
                }
            }

            public void LoadDefaultMixedControlsProfile(int device, bool launchprogram, ControlService control,
                string propath = "", bool xinputChange = true, bool postLoad = true)
            {
                bool xinputPlug = false;
                bool xinputStatus = false;

                OutContType oldContType = Global.ActiveOutDevType[device];

                // Make sure to reset currently set profile values before parsing
                ResetProfile(device);
                ResetMouseProperties(device, control);

                // Only change xinput devices under certain conditions. Avoid
                // performing this upon program startup before loading devices.
                if (xinputChange)
                {
                    CheckOldDeviceStatus(device, control, oldContType,
                        out xinputPlug, out xinputStatus);
                }

                foreach (DS4ControlSettings dcs in Ds4Settings[device])
                    dcs.Reset();

                ProfileActions[device].Clear();
                containsCustomAction[device] = false;
                containsCustomExtras[device] = false;

                DS4ControlSettings setting = GetDs4ControllerSetting(device, DS4Controls.RYNeg);
                setting.UpdateSettings(false, X360Controls.MouseUp, "", DS4KeyType.None);
                setting = GetDs4ControllerSetting(device, DS4Controls.RYPos);
                setting.UpdateSettings(false, X360Controls.MouseDown, "", DS4KeyType.None);
                setting = GetDs4ControllerSetting(device, DS4Controls.RXNeg);
                setting.UpdateSettings(false, X360Controls.MouseLeft, "", DS4KeyType.None);
                setting = GetDs4ControllerSetting(device, DS4Controls.RXPos);
                setting.UpdateSettings(false, X360Controls.MouseRight, "", DS4KeyType.None);

                StickDeadZoneInfo rsInfo = RSModInfo[device];
                rsInfo.deadZone = (int)(0.035 * 127);
                rsInfo.antiDeadZone = 0;
                rsInfo.maxZone = 90;

                // If a device exists, make sure to transfer relevant profile device
                // options to device instance
                if (postLoad && device < Global.MAX_DS4_CONTROLLER_COUNT)
                {
                    PostLoadSnippet(device, control, xinputStatus, xinputPlug);
                }
            }

            public void LoadDefaultKBMProfile(int device, bool launchprogram, ControlService control,
                string propath = "", bool xinputChange = true, bool postLoad = true)
            {
                bool xinputPlug = false;
                bool xinputStatus = false;

                OutContType oldContType = Global.ActiveOutDevType[device];

                // Make sure to reset currently set profile values before parsing
                ResetProfile(device);
                ResetMouseProperties(device, control);

                // Only change xinput devices under certain conditions. Avoid
                // performing this upon program startup before loading devices.
                if (xinputChange)
                {
                    CheckOldDeviceStatus(device, control, oldContType,
                        out xinputPlug, out xinputStatus);
                }

                foreach (DS4ControlSettings dcs in Ds4Settings[device])
                    dcs.Reset();

                ProfileActions[device].Clear();
                containsCustomAction[device] = false;
                containsCustomExtras[device] = false;

                StickDeadZoneInfo lsInfo = LSModInfo[device];
                lsInfo.antiDeadZone = 0;

                StickDeadZoneInfo rsInfo = RSModInfo[device];
                rsInfo.deadZone = (int)(0.035 * 127);
                rsInfo.antiDeadZone = 0;
                rsInfo.maxZone = 90;

                TriggerDeadZoneZInfo l2Info = L2ModInfo[device];
                l2Info.deadZone = (byte)(0.20 * 255);

                TriggerDeadZoneZInfo r2Info = R2ModInfo[device];
                r2Info.deadZone = (byte)(0.20 * 255);

                // Flag to unplug virtual controller
                dinputOnly[device] = true;

                DS4ControlSettings setting = GetDs4ControllerSetting(device, DS4Controls.LYNeg);
                setting.UpdateSettings(false, KeyInterop.VirtualKeyFromKey(Key.W), "", DS4KeyType.None);
                setting = GetDs4ControllerSetting(device, DS4Controls.LXNeg);
                setting.UpdateSettings(false, KeyInterop.VirtualKeyFromKey(Key.A), "", DS4KeyType.None);
                setting = GetDs4ControllerSetting(device, DS4Controls.LYPos);
                setting.UpdateSettings(false, KeyInterop.VirtualKeyFromKey(Key.S), "", DS4KeyType.None);
                setting = GetDs4ControllerSetting(device, DS4Controls.LXPos);
                setting.UpdateSettings(false, KeyInterop.VirtualKeyFromKey(Key.D), "", DS4KeyType.None);
                setting = GetDs4ControllerSetting(device, DS4Controls.L3);
                setting.UpdateSettings(false, KeyInterop.VirtualKeyFromKey(Key.LeftShift), "", DS4KeyType.None);

                setting = GetDs4ControllerSetting(device, DS4Controls.RYNeg);
                setting.UpdateSettings(false, X360Controls.MouseUp, "", DS4KeyType.None);
                setting = GetDs4ControllerSetting(device, DS4Controls.RYPos);
                setting.UpdateSettings(false, X360Controls.MouseDown, "", DS4KeyType.None);
                setting = GetDs4ControllerSetting(device, DS4Controls.RXNeg);
                setting.UpdateSettings(false, X360Controls.MouseLeft, "", DS4KeyType.None);
                setting = GetDs4ControllerSetting(device, DS4Controls.RXPos);
                setting.UpdateSettings(false, X360Controls.MouseRight, "", DS4KeyType.None);
                setting = GetDs4ControllerSetting(device, DS4Controls.R3);
                setting.UpdateSettings(false, KeyInterop.VirtualKeyFromKey(Key.LeftCtrl), "", DS4KeyType.None);

                setting = GetDs4ControllerSetting(device, DS4Controls.DpadUp);
                setting.UpdateSettings(false, X360Controls.Unbound, "", DS4KeyType.None);
                setting = GetDs4ControllerSetting(device, DS4Controls.DpadRight);
                setting.UpdateSettings(false, X360Controls.WDOWN, "", DS4KeyType.None);
                setting = GetDs4ControllerSetting(device, DS4Controls.DpadDown);
                setting.UpdateSettings(false, X360Controls.Unbound, "", DS4KeyType.None);
                setting = GetDs4ControllerSetting(device, DS4Controls.DpadLeft);
                setting.UpdateSettings(false, X360Controls.WUP, "", DS4KeyType.None);

                setting = GetDs4ControllerSetting(device, DS4Controls.Cross);
                setting.UpdateSettings(false, KeyInterop.VirtualKeyFromKey(Key.Space), "", DS4KeyType.None);
                setting = GetDs4ControllerSetting(device, DS4Controls.Square);
                setting.UpdateSettings(false, KeyInterop.VirtualKeyFromKey(Key.F), "", DS4KeyType.None);
                setting = GetDs4ControllerSetting(device, DS4Controls.Triangle);
                setting.UpdateSettings(false, KeyInterop.VirtualKeyFromKey(Key.E), "", DS4KeyType.None);
                setting = GetDs4ControllerSetting(device, DS4Controls.Circle);
                setting.UpdateSettings(false, KeyInterop.VirtualKeyFromKey(Key.C), "", DS4KeyType.None);

                setting = GetDs4ControllerSetting(device, DS4Controls.L1);
                setting.UpdateSettings(false, KeyInterop.VirtualKeyFromKey(Key.Q), "", DS4KeyType.None);
                setting = GetDs4ControllerSetting(device, DS4Controls.L2);
                setting.UpdateSettings(false, X360Controls.RightMouse, "", DS4KeyType.None);
                setting = GetDs4ControllerSetting(device, DS4Controls.R1);
                setting.UpdateSettings(false, KeyInterop.VirtualKeyFromKey(Key.R), "", DS4KeyType.None);
                setting = GetDs4ControllerSetting(device, DS4Controls.R2);
                setting.UpdateSettings(false, X360Controls.LeftMouse, "", DS4KeyType.None);

                setting = GetDs4ControllerSetting(device, DS4Controls.Share);
                setting.UpdateSettings(false, KeyInterop.VirtualKeyFromKey(Key.Tab), "", DS4KeyType.None);
                setting = GetDs4ControllerSetting(device, DS4Controls.Options);
                setting.UpdateSettings(false, KeyInterop.VirtualKeyFromKey(Key.Escape), "", DS4KeyType.None);

                // If a device exists, make sure to transfer relevant profile device
                // options to device instance
                if (postLoad && device < Global.MAX_DS4_CONTROLLER_COUNT)
                {
                    PostLoadSnippet(device, control, xinputStatus, xinputPlug);
                }
            }

            public void LoadDefaultKBMGyroMouseProfile(int device, bool launchprogram, ControlService control,
                string propath = "", bool xinputChange = true, bool postLoad = true)
            {
                bool xinputPlug = false;
                bool xinputStatus = false;

                OutContType oldContType = Global.ActiveOutDevType[device];

                // Make sure to reset currently set profile values before parsing
                ResetProfile(device);
                ResetMouseProperties(device, control);

                // Only change xinput devices under certain conditions. Avoid
                // performing this upon program startup before loading devices.
                if (xinputChange)
                {
                    CheckOldDeviceStatus(device, control, oldContType,
                        out xinputPlug, out xinputStatus);
                }

                foreach (DS4ControlSettings dcs in Ds4Settings[device])
                    dcs.Reset();

                ProfileActions[device].Clear();
                containsCustomAction[device] = false;
                containsCustomExtras[device] = false;

                StickDeadZoneInfo lsInfo = LSModInfo[device];
                lsInfo.antiDeadZone = 0;

                StickDeadZoneInfo rsInfo = RSModInfo[device];
                rsInfo.deadZone = (int)(0.105 * 127);
                rsInfo.antiDeadZone = 0;
                rsInfo.maxZone = 90;

                TriggerDeadZoneZInfo l2Info = L2ModInfo[device];
                l2Info.deadZone = (byte)(0.20 * 255);

                TriggerDeadZoneZInfo r2Info = R2ModInfo[device];
                r2Info.deadZone = (byte)(0.20 * 255);

                gyroOutMode[device] = GyroOutMode.Mouse;
                sATriggers[device] = "4";
                sATriggerCond[device] = true;
                gyroTriggerTurns[device] = false;
                gyroMouseInfo[device].enableSmoothing = true;
                gyroMouseInfo[device].smoothingMethod = DS4Windows.GyroMouseInfo.SmoothingMethod.OneEuro;

                // Flag to unplug virtual controller
                dinputOnly[device] = true;

                DS4ControlSettings setting = GetDs4ControllerSetting(device, DS4Controls.LYNeg);
                setting.UpdateSettings(false, KeyInterop.VirtualKeyFromKey(Key.W), "", DS4KeyType.None);
                setting = GetDs4ControllerSetting(device, DS4Controls.LXNeg);
                setting.UpdateSettings(false, KeyInterop.VirtualKeyFromKey(Key.A), "", DS4KeyType.None);
                setting = GetDs4ControllerSetting(device, DS4Controls.LYPos);
                setting.UpdateSettings(false, KeyInterop.VirtualKeyFromKey(Key.S), "", DS4KeyType.None);
                setting = GetDs4ControllerSetting(device, DS4Controls.LXPos);
                setting.UpdateSettings(false, KeyInterop.VirtualKeyFromKey(Key.D), "", DS4KeyType.None);
                setting = GetDs4ControllerSetting(device, DS4Controls.L3);
                setting.UpdateSettings(false, KeyInterop.VirtualKeyFromKey(Key.LeftShift), "", DS4KeyType.None);

                setting = GetDs4ControllerSetting(device, DS4Controls.RYNeg);
                setting.UpdateSettings(false, X360Controls.MouseUp, "", DS4KeyType.None);
                setting = GetDs4ControllerSetting(device, DS4Controls.RYPos);
                setting.UpdateSettings(false, X360Controls.MouseDown, "", DS4KeyType.None);
                setting = GetDs4ControllerSetting(device, DS4Controls.RXNeg);
                setting.UpdateSettings(false, X360Controls.MouseLeft, "", DS4KeyType.None);
                setting = GetDs4ControllerSetting(device, DS4Controls.RXPos);
                setting.UpdateSettings(false, X360Controls.MouseRight, "", DS4KeyType.None);
                setting = GetDs4ControllerSetting(device, DS4Controls.R3);
                setting.UpdateSettings(false, KeyInterop.VirtualKeyFromKey(Key.LeftCtrl), "", DS4KeyType.None);

                setting = GetDs4ControllerSetting(device, DS4Controls.DpadUp);
                setting.UpdateSettings(false, X360Controls.Unbound, "", DS4KeyType.None);
                setting = GetDs4ControllerSetting(device, DS4Controls.DpadRight);
                setting.UpdateSettings(false, X360Controls.WDOWN, "", DS4KeyType.None);
                setting = GetDs4ControllerSetting(device, DS4Controls.DpadDown);
                setting.UpdateSettings(false, X360Controls.Unbound, "", DS4KeyType.None);
                setting = GetDs4ControllerSetting(device, DS4Controls.DpadLeft);
                setting.UpdateSettings(false, X360Controls.WUP, "", DS4KeyType.None);

                setting = GetDs4ControllerSetting(device, DS4Controls.Cross);
                setting.UpdateSettings(false, KeyInterop.VirtualKeyFromKey(Key.Space), "", DS4KeyType.None);
                setting = GetDs4ControllerSetting(device, DS4Controls.Square);
                setting.UpdateSettings(false, KeyInterop.VirtualKeyFromKey(Key.F), "", DS4KeyType.None);
                setting = GetDs4ControllerSetting(device, DS4Controls.Triangle);
                setting.UpdateSettings(false, KeyInterop.VirtualKeyFromKey(Key.E), "", DS4KeyType.None);
                setting = GetDs4ControllerSetting(device, DS4Controls.Circle);
                setting.UpdateSettings(false, KeyInterop.VirtualKeyFromKey(Key.C), "", DS4KeyType.None);

                setting = GetDs4ControllerSetting(device, DS4Controls.L1);
                //setting.UpdateSettings(false, KeyInterop.VirtualKeyFromKey(Key.Q), "", DS4KeyType.None);
                setting.UpdateSettings(false, X360Controls.Unbound, "", DS4KeyType.None);
                setting = GetDs4ControllerSetting(device, DS4Controls.L2);
                setting.UpdateSettings(false, X360Controls.RightMouse, "", DS4KeyType.None);
                setting = GetDs4ControllerSetting(device, DS4Controls.R1);
                setting.UpdateSettings(false, KeyInterop.VirtualKeyFromKey(Key.R), "", DS4KeyType.None);
                setting = GetDs4ControllerSetting(device, DS4Controls.R2);
                setting.UpdateSettings(false, X360Controls.LeftMouse, "", DS4KeyType.None);

                setting = GetDs4ControllerSetting(device, DS4Controls.Share);
                setting.UpdateSettings(false, KeyInterop.VirtualKeyFromKey(Key.Tab), "", DS4KeyType.None);
                setting = GetDs4ControllerSetting(device, DS4Controls.Options);
                setting.UpdateSettings(false, KeyInterop.VirtualKeyFromKey(Key.Escape), "", DS4KeyType.None);

                // If a device exists, make sure to transfer relevant profile device
                // options to device instance
                if (postLoad && device < Global.MAX_DS4_CONTROLLER_COUNT)
                {
                    PostLoadSnippet(device, control, xinputStatus, xinputPlug);
                }
            }

            private void CheckOldDeviceStatus(int device, ControlService control,
                OutContType oldContType, out bool xinputPlug, out bool xinputStatus)
            {
                xinputPlug = false;
                xinputStatus = false;

                if (device < ControlService.CURRENT_DS4_CONTROLLER_LIMIT)
                {
                    bool oldUseDInputOnly = Global.UseDirectInputOnly[device];
                    DS4Device tempDevice = control.DS4Controllers[device];
                    bool exists = tempBool = (tempDevice != null);
                    bool synced = tempBool = exists ? tempDevice.isSynced() : false;
                    bool isAlive = tempBool = exists ? tempDevice.IsAlive() : false;
                    if (dinputOnly[device] != oldUseDInputOnly)
                    {
                        if (dinputOnly[device] == true)
                        {
                            xinputPlug = false;
                            xinputStatus = true;
                        }
                        else if (synced && isAlive)
                        {
                            xinputPlug = true;
                            xinputStatus = true;
                        }
                    }
                    else if (!dinputOnly[device] &&
                        oldContType != OutputDeviceType[device])
                    {
                        xinputPlug = true;
                        xinputStatus = true;
                    }
                }
            }

            private void PostLoadSnippet(int device, ControlService control, bool xinputStatus, bool xinputPlug)
            {
                DS4Device tempDev = control.DS4Controllers[device];
                if (tempDev != null && tempDev.isSynced())
                {
                    tempDev.queueEvent(() =>
                    {
                        //tempDev.setIdleTimeout(idleDisconnectTimeout[device]);
                        //tempDev.setBTPollRate(btPollRate[device]);
                        if (xinputStatus && tempDev.PrimaryDevice)
                        {
                            if (xinputPlug)
                            {
                                OutputDevice tempOutDev = control.outputDevices[device];
                                if (tempOutDev != null)
                                {
                                    tempOutDev = null;
                                    //Global.ActiveOutDevType[device] = OutContType.None;
                                    control.UnplugOutDev(device, tempDev);
                                }

                                OutContType tempContType = OutputDeviceType[device];
                                control.PluginOutDev(device, tempDev);
                                //Global.UseDirectInputOnly[device] = false;
                            }
                            else
                            {
                                //Global.ActiveOutDevType[device] = OutContType.None;
                                control.UnplugOutDev(device, tempDev);
                            }
                        }

                        //tempDev.RumbleAutostopTime = rumbleAutostopTime[device];
                        //tempDev.setRumble(0, 0);
                        //tempDev.LightBarColor = Global.getMainColor(device);
                        control.CheckProfileOptions(device, tempDev, true);
                    });

                    //Program.rootHub.touchPad[device]?.ResetTrackAccel(trackballFriction[device]);
                }
            }
        }
    }
}
