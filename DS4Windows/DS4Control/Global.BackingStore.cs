using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Xml;
using DS4WinWPF.DS4Control.Attributes;
using DS4WinWPF.DS4Control.Logging;
using DS4WinWPF.DS4Control.Profiles.Legacy;
using DS4WinWPF.Properties;

namespace DS4Windows
{
    public partial class Global
    {
        private class BackingStore : IBackingStore
        {
            public const double DEFAULT_UDP_SMOOTH_MINCUTOFF = 0.4;
            public const double DEFAULT_UDP_SMOOTH_BETA = 0.2;

            private readonly int[] _l2OutCurveMode = new int[TEST_PROFILE_ITEM_COUNT] { 0, 0, 0, 0, 0, 0, 0, 0, 0 };

            private readonly int[] _lsOutCurveMode = new int[TEST_PROFILE_ITEM_COUNT] { 0, 0, 0, 0, 0, 0, 0, 0, 0 };

            private readonly int[] _r2OutCurveMode = new int[TEST_PROFILE_ITEM_COUNT] { 0, 0, 0, 0, 0, 0, 0, 0, 0 };

            private readonly int[] _rsOutCurveMode = new int[TEST_PROFILE_ITEM_COUNT] { 0, 0, 0, 0, 0, 0, 0, 0, 0 };

            private readonly int[] _sxOutCurveMode = new int[TEST_PROFILE_ITEM_COUNT] { 0, 0, 0, 0, 0, 0, 0, 0, 0 };

            private readonly int[] _szOutCurveMode = new int[TEST_PROFILE_ITEM_COUNT] { 0, 0, 0, 0, 0, 0, 0, 0, 0 };

            private readonly IList<ControlSettingsGroup> ds4controlSettings;

            private readonly List<DS4ControlSettings>[] Ds4Settings =
                new List<DS4ControlSettings>[TEST_PROFILE_ITEM_COUNT]
                {
                    new(), new(), new(),
                    new(), new(), new(), new(), new(), new()
                };

            private readonly Lazy<ControlServiceDeviceOptions> LazyDeviceOptions =
                new(new ControlServiceDeviceOptions());

            protected readonly XmlDocument m_Xdoc = new();

            private readonly int[] profileActionCount = new int[TEST_PROFILE_ITEM_COUNT] { 0, 0, 0, 0, 0, 0, 0, 0, 0 };

            private readonly Dictionary<string, SpecialAction>[] profileActionDict =
                new Dictionary<string, SpecialAction>[TEST_PROFILE_ITEM_COUNT]
                {
                    new(), new(), new(),
                    new(), new(), new(), new(), new(), new()
                };

            private readonly Dictionary<string, int>[] profileActionIndexDict =
                new Dictionary<string, int>[TEST_PROFILE_ITEM_COUNT]
                {
                    new(), new(), new(),
                    new(), new(), new(), new(), new(), new()
                };

            public string lastVersionChecked = string.Empty;

            public BackingStore()
            {
                ds4controlSettings = new ControlSettingsGroup[TEST_PROFILE_ITEM_COUNT];

                for (var i = 0; i < TEST_PROFILE_ITEM_COUNT; i++)
                {
                    foreach (DS4Controls dc in Enum.GetValues(typeof(DS4Controls)))
                        if (dc != DS4Controls.None)
                            Ds4Settings[i].Add(new DS4ControlSettings(dc));

                    ds4controlSettings[i] = new ControlSettingsGroup(Ds4Settings[i]);

                    EstablishDefaultSpecialActions(i);
                    CacheExtraProfileInfo(i);
                }

                SetupDefaultColors();
            }

            private Dictionary<string, string> LinkedProfiles { get; set; } = new();

            public IList<int> RumbleAutostopTime { get; } = new List<int>
                { 0, 0, 0, 0, 0, 0, 0, 0, 0 }; // Value in milliseconds (0=autustop timer disabled)

            public IList<GyroControlsInfo> GyroControlsInfo { get; set; } = new List<GyroControlsInfo>
            {
                new(), new(), new(),
                new(), new(), new(),
                new(), new(), new()
            };

            public IList<bool> GyroMouseStickToggle { get; set; } = new List<bool>
            {
                false, false, false,
                false, false, false, false, false, false
            };

            public IList<bool> GyroMouseStickTriggerTurns { get; set; } = new List<bool>
                { true, true, true, true, true, true, true, true, true };

            public IList<GyroMouseStickInfo> GyroMouseStickInfo { get; set; } = new List<GyroMouseStickInfo>
            {
                new(),
                new(),
                new(), new(),
                new(), new(),
                new(), new(),
                new()
            };

            public IList<GyroOutMode> GyroOutputMode { get; set; } = new List<GyroOutMode>
            {
                GyroOutMode.Controls, GyroOutMode.Controls,
                GyroOutMode.Controls, GyroOutMode.Controls, GyroOutMode.Controls,
                GyroOutMode.Controls,
                GyroOutMode.Controls, GyroOutMode.Controls, GyroOutMode.Controls
            };

            public IList<GyroDirectionalSwipeInfo> GyroSwipeInfo { get; set; } =
                new List<GyroDirectionalSwipeInfo>
                {
                    new(), new(),
                    new(), new(),
                    new(), new(),
                    new(), new(),
                    new()
                };

            public IList<int> SAWheelFuzzValues { get; set; } = new List<int>
            {
                new(), new(), new(),
                new(), new(), new(), new(), new(), new()
            };

            /// <summary>
            ///     TRUE=AutoProfile reverts to default profile if current foreground process is unknown, FALSE=Leave existing profile
            ///     active when a foreground process is unknown (ie. no matching auto-profile rule)
            /// </summary>
            public bool AutoProfileRevertDefaultProfile { get; set; } = true;

            public int FlashWhenLateAt { get; set; } = 50;

            public ulong LastVersionCheckedNumber { get; set; }

            public bool SwipeProfiles { get; set; } = true;

            public bool UseExclusiveMode { get; set; } // Re-enable Ex Mode

            public bool IsUdpServerEnabled { get; set; }

            public IList<LightbarSettingInfo> LightbarSettingInfo { get; set; } = new List<LightbarSettingInfo>
            {
                new(), new(),
                new(), new(),
                new(), new(),
                new(), new(),
                new()
            };

            public IList<bool> SAMouseStickTriggerCond { get; set; } = new List<bool>
                { true, true, true, true, true, true, true, true, true };

            public IList<string> SAMouseStickTriggers { get; set; } = new List<string>
                { "-1", "-1", "-1", "-1", "-1", "-1", "-1", "-1", "-1" };

            public IList<SASteeringWheelEmulationAxisType> SASteeringWheelEmulationAxis { get; set; } =
                new List<SASteeringWheelEmulationAxisType>
                {
                    SASteeringWheelEmulationAxisType.None, SASteeringWheelEmulationAxisType.None,
                    SASteeringWheelEmulationAxisType.None, SASteeringWheelEmulationAxisType.None,
                    SASteeringWheelEmulationAxisType.None, SASteeringWheelEmulationAxisType.None,
                    SASteeringWheelEmulationAxisType.None, SASteeringWheelEmulationAxisType.None,
                    SASteeringWheelEmulationAxisType.None
                };

            public IList<int> SASteeringWheelEmulationRange { get; set; } =
                new List<int> { 360, 360, 360, 360, 360, 360, 360, 360, 360 };

            public IList<bool> SATriggerCondition { get; set; } = new List<bool>
                { true, true, true, true, true, true, true, true, true };

            public IList<string> SATriggers { get; set; } = new List<string>
                { "-1", "-1", "-1", "-1", "-1", "-1", "-1", "-1", "-1" };

            public IList<bool> StartTouchpadOff { get; set; } = new List<bool>
                { false, false, false, false, false, false, false, false, false };

            public IList<IList<int>> TouchDisInvertTriggers { get; set; } = new List<IList<int>>
            {
                new int[1] { -1 }, new int[1] { -1 }, new int[1] { -1 },
                new int[1] { -1 }, new int[1] { -1 }, new int[1] { -1 }, new int[1] { -1 }, new int[1] { -1 },
                new int[1] { -1 }
            };

            public IList<TouchpadOutMode> TouchOutMode { get; set; } = new List<TouchpadOutMode>
            {
                TouchpadOutMode.Mouse, TouchpadOutMode.Mouse, TouchpadOutMode.Mouse, TouchpadOutMode.Mouse,
                TouchpadOutMode.Mouse, TouchpadOutMode.Mouse, TouchpadOutMode.Mouse, TouchpadOutMode.Mouse,
                TouchpadOutMode.Mouse
            };

            public string ProfilesPath { get; set; } =
                Path.Combine(Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName,
                    Constants.ProfilesFileName);

            public string ActionsPath { get; set; } = Path.Combine(RuntimeAppDataPath, Constants.ActionsFileName);

            public string LinkedProfilesPath { get; set; } =
                Path.Combine(RuntimeAppDataPath, Constants.LinkedProfilesFileName);

            public string ControllerConfigsPath { get; set; } =
                Path.Combine(RuntimeAppDataPath, Constants.ControllerConfigsFileName);

            // ninth (fifth in old builds) value used for options, not last controller
            public IList<ButtonMouseInfo> ButtonMouseInfos { get; set; } = new List<ButtonMouseInfo>
            {
                new(), new(), new(),
                new(), new(), new(),
                new(), new(), new()
            };

            public IList<bool> EnableTouchToggle { get; set; } = new List<bool>
                { true, true, true, true, true, true, true, true, true };

            public IList<int> IdleDisconnectTimeout { get; set; } = new List<int> { 0, 0, 0, 0, 0, 0, 0, 0, 0 };

            public IList<bool> EnableOutputDataToDS4 { get; set; } = new List<bool>
                { true, true, true, true, true, true, true, true, true };

            public IList<bool> TouchpadJitterCompensation { get; set; } = new List<bool>
                { true, true, true, true, true, true, true, true, true };

            public IList<bool> LowerRCOn { get; set; } = new List<bool>
                { false, false, false, false, false, false, false, false, false };

            public IList<bool> TouchClickPassthru { get; set; } = new List<bool>
                { false, false, false, false, false, false, false, false, false };

            public IList<string> ProfilePath { get; set; } = new List<string>
            {
                string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty,
                string.Empty, string.Empty
            };

            public IList<string> OlderProfilePath { get; set; } = new List<string>
            {
                string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty,
                string.Empty, string.Empty
            };

            // Cache properties instead of performing a string comparison every frame
            public IList<bool> DistanceProfiles { get; set; } = new List<bool>
                { false, false, false, false, false, false, false, false, false };

            public IList<byte> RumbleBoost { get; set; } =
                new List<byte> { 100, 100, 100, 100, 100, 100, 100, 100, 100 };

            public IList<byte> TouchSensitivity { get; set; } =
                new List<byte> { 100, 100, 100, 100, 100, 100, 100, 100, 100 };

            public IList<StickDeadZoneInfo> LSModInfo { get; set; } = new List<StickDeadZoneInfo>
            {
                new(), new(),
                new(), new(),
                new(), new(),
                new(), new(),
                new()
            };

            public IList<StickDeadZoneInfo> RSModInfo { get; set; } = new List<StickDeadZoneInfo>
            {
                new(), new(),
                new(), new(),
                new(), new(),
                new(), new(),
                new()
            };

            public IList<TriggerDeadZoneZInfo> L2ModInfo { get; set; } = new List<TriggerDeadZoneZInfo>
            {
                new(), new(),
                new(), new(),
                new(), new(),
                new(), new(),
                new()
            };

            public IList<TriggerDeadZoneZInfo> R2ModInfo { get; set; } = new List<TriggerDeadZoneZInfo>
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

            public IList<byte> TapSensitivity { get; set; } = new List<byte> { 0, 0, 0, 0, 0, 0, 0, 0, 0 };

            public IList<bool> DoubleTap { get; set; } = new List<bool>
                { false, false, false, false, false, false, false, false, false };

            public IList<int> ScrollSensitivity { get; set; } = new List<int> { 0, 0, 0, 0, 0, 0, 0, 0, 0 };

            public IList<int> TouchPadInvert { get; set; } = new List<int> { 0, 0, 0, 0, 0, 0, 0, 0, 0 };

            public IList<int> BluetoothPollRate { get; set; } = new List<int> { 4, 4, 4, 4, 4, 4, 4, 4, 4 };

            public IList<int> GyroMouseDeadZone { get; set; } = new List<int>
            {
                MouseCursor.GYRO_MOUSE_DEADZONE, MouseCursor.GYRO_MOUSE_DEADZONE,
                MouseCursor.GYRO_MOUSE_DEADZONE, MouseCursor.GYRO_MOUSE_DEADZONE,
                MouseCursor.GYRO_MOUSE_DEADZONE, MouseCursor.GYRO_MOUSE_DEADZONE,
                MouseCursor.GYRO_MOUSE_DEADZONE, MouseCursor.GYRO_MOUSE_DEADZONE,
                MouseCursor.GYRO_MOUSE_DEADZONE
            };

            public IList<bool> GyroMouseToggle { get; set; } = new List<bool>
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

            public IList<BezierCurve> LSOutBezierCurveObj { get; set; } = new List<BezierCurve>
                { new(), new(), new(), new(), new(), new(), new(), new(), new() };

            public IList<BezierCurve> RSOutBezierCurveObj { get; set; } = new List<BezierCurve>
                { new(), new(), new(), new(), new(), new(), new(), new(), new() };

            public IList<BezierCurve> L2OutBezierCurveObj { get; set; } = new List<BezierCurve>
                { new(), new(), new(), new(), new(), new(), new(), new(), new() };

            public IList<BezierCurve> R2OutBezierCurveObj { get; set; } = new List<BezierCurve>
                { new(), new(), new(), new(), new(), new(), new(), new(), new() };

            public IList<BezierCurve> SXOutBezierCurveObj { get; set; } = new List<BezierCurve>
                { new(), new(), new(), new(), new(), new(), new(), new(), new() };

            public IList<BezierCurve> SZOutBezierCurveObj { get; set; } = new List<BezierCurve>
                { new(), new(), new(), new(), new(), new(), new(), new(), new() };

            public IList<string> LaunchProgram { get; set; } = new List<string>
            {
                string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty,
                string.Empty, string.Empty
            };

            public IList<bool> DirectInputOnly { get; set; } = new List<bool>
                { false, false, false, false, false, false, false, false, false };

            public int FormWidth { get; set; } = 782;

            public int FormHeight { get; set; } = 550;

            public int FormLocationX { get; set; }

            public int FormLocationY { get; set; }

            public bool StartMinimized { get; set; }

            public bool MinToTaskBar { get; set; }

            public DateTime LastChecked { get; set; }

            public int CheckWhen { get; set; } = 24;

            public int Notifications { get; set; } = 2;

            public bool DisconnectBluetoothAtStop { get; set; }

            public bool Ds4Mapping { get; set; }

            public bool QuickCharge { get; set; }

            public bool CloseMini { get; set; }

            public IList<SpecialAction> Actions { get; set; } = new List<SpecialAction>();

            public IList<string>[] ProfileActions { get; set; } =
                { null, null, null, null, null, null, null, null, null };

            public string UseLang { get; set; } = string.Empty;

            public bool DownloadLang { get; set; } = true;

            public TrayIconChoice UseIconChoice { get; set; }

            public bool FlashWhenLate { get; set; } = true;

            public int UdpServerPort { get; set; } = 26760;

            /// <summary>
            ///     127.0.0.1=IPAddress.Loopback (default), 0.0.0.0=IPAddress.Any as all interfaces, x.x.x.x = Specific ipv4 interface
            ///     address or hostname
            /// </summary>
            public string UdpServerListenAddress { get; set; } = "127.0.0.1";

            public bool UseUdpSmoothing { get; set; }

            public double UdpSmoothingMincutoff { get; set; } = DEFAULT_UDP_SMOOTH_MINCUTOFF;

            public double UdpSmoothingBeta { get; set; } = DEFAULT_UDP_SMOOTH_BETA;

            public bool UseCustomSteamFolder { get; set; }

            public string CustomSteamFolder { get; set; }

            public AppThemeChoice ThemeChoice { get; set; }

            public string FakeExeFileName { get; set; } = string.Empty;

            public ControlServiceDeviceOptions DeviceOptions => LazyDeviceOptions.Value;

            // Cache whether profile has custom action
            public IList<bool> ContainsCustomAction { get; set; } = new bool[TEST_PROFILE_ITEM_COUNT]
                { false, false, false, false, false, false, false, false, false };

            // Cache whether profile has custom extras
            public IList<bool> ContainsCustomExtras { get; set; } = new bool[TEST_PROFILE_ITEM_COUNT]
                { false, false, false, false, false, false, false, false, false };

            public IList<int> GyroSensitivity { get; set; } = new int[TEST_PROFILE_ITEM_COUNT]
                { 100, 100, 100, 100, 100, 100, 100, 100, 100 };

            public IList<int> GyroSensVerticalScale { get; set; } = new int[TEST_PROFILE_ITEM_COUNT]
                { 100, 100, 100, 100, 100, 100, 100, 100, 100 };

            public IList<int> GyroInvert { get; set; } = new List<int> { 0, 0, 0, 0, 0, 0, 0, 0, 0 };

            public IList<bool> GyroTriggerTurns { get; set; } = new List<bool>
                { true, true, true, true, true, true, true, true, true };

            public IList<GyroMouseInfo> GyroMouseInfo { get; set; } = new List<GyroMouseInfo>
            {
                new(), new(),
                new(), new(),
                new(), new(),
                new(), new(),
                new()
            };

            public IList<int> GyroMouseHorizontalAxis { get; set; } = new List<int> { 0, 0, 0, 0, 0, 0, 0, 0, 0 };

            public IList<int> GyroMouseStickHorizontalAxis { get; set; } = new List<int> { 0, 0, 0, 0, 0, 0, 0, 0, 0 };

            public IList<bool> TrackballMode { get; set; } = new List<bool>
                { false, false, false, false, false, false, false, false, false };

            public IList<double> TrackballFriction { get; set; } = new List<double>
                { 10.0, 10.0, 10.0, 10.0, 10.0, 10.0, 10.0, 10.0, 10.0 };

            public IList<TouchPadAbsMouseSettings> TouchPadAbsMouse { get; set; } =
                new List<TouchPadAbsMouseSettings>
                {
                    new(), new(), new(),
                    new(), new(), new(), new(), new(), new()
                };

            public IList<TouchPadRelMouseSettings> TouchPadRelMouse { get; set; } =
                new List<TouchPadRelMouseSettings>
                {
                    new(), new(), new(), new(),
                    new(), new(), new(), new(), new()
                };

            // Used to hold the controller type desired in a profile
            public IList<OutContType> OutputDeviceType { get; set; } = new List<OutContType>
            {
                OutContType.X360,
                OutContType.X360,
                OutContType.X360,
                OutContType.X360,
                OutContType.X360,
                OutContType.X360,
                OutContType.X360,
                OutContType.X360,
                OutContType.X360
            };

            public void RefreshExtrasButtons(int deviceNum, List<DS4Controls> devButtons)
            {
                ds4controlSettings[deviceNum].ResetExtraButtons();
                if (devButtons != null) ds4controlSettings[deviceNum].EstablishExtraButtons(devButtons);
            }

            public int GetLsOutCurveMode(int index)
            {
                return _lsOutCurveMode[index];
            }

            public void SetLsOutCurveMode(int index, int value)
            {
                if (value >= 1)
                    SetOutBezierCurveObjArrayItem(LSOutBezierCurveObj, index, value, BezierCurve.AxisType.LSRS);
                _lsOutCurveMode[index] = value;
            }

            public int GetRsOutCurveMode(int index)
            {
                return _rsOutCurveMode[index];
            }

            public void SetRsOutCurveMode(int index, int value)
            {
                if (value >= 1)
                    SetOutBezierCurveObjArrayItem(RSOutBezierCurveObj, index, value, BezierCurve.AxisType.LSRS);
                _rsOutCurveMode[index] = value;
            }

            public int GetL2OutCurveMode(int index)
            {
                return _l2OutCurveMode[index];
            }

            public void SetL2OutCurveMode(int index, int value)
            {
                if (value >= 1)
                    SetOutBezierCurveObjArrayItem(L2OutBezierCurveObj, index, value, BezierCurve.AxisType.L2R2);
                _l2OutCurveMode[index] = value;
            }

            public int GetR2OutCurveMode(int index)
            {
                return _r2OutCurveMode[index];
            }

            public void SetR2OutCurveMode(int index, int value)
            {
                if (value >= 1)
                    SetOutBezierCurveObjArrayItem(R2OutBezierCurveObj, index, value, BezierCurve.AxisType.L2R2);
                _r2OutCurveMode[index] = value;
            }

            public int GetSXOutCurveMode(int index)
            {
                return _sxOutCurveMode[index];
            }

            public void SetSXOutCurveMode(int index, int value)
            {
                if (value >= 1)
                    SetOutBezierCurveObjArrayItem(SXOutBezierCurveObj, index, value, BezierCurve.AxisType.SA);
                _sxOutCurveMode[index] = value;
            }

            public int GetSZOutCurveMode(int index)
            {
                return _szOutCurveMode[index];
            }

            public void SetSZOutCurveMode(int index, int value)
            {
                if (value >= 1)
                    SetOutBezierCurveObjArrayItem(SZOutBezierCurveObj, index, value, BezierCurve.AxisType.SA);
                _szOutCurveMode[index] = value;
            }

            public DS4Color GetMainColor(int index)
            {
                return LightbarSettingInfo[index].Ds4WinSettings.Led;
            }

            public DS4Color GetLowColor(int index)
            {
                return LightbarSettingInfo[index].Ds4WinSettings.LowLed;
            }

            public DS4Color GetChargingColor(int index)
            {
                return LightbarSettingInfo[index].Ds4WinSettings.ChargingLed;
            }

            public DS4Color GetFlashColor(int index)
            {
                return LightbarSettingInfo[index].Ds4WinSettings.FlashLed;
            }

            public string GetSATriggers(int index)
            {
                return SATriggers[index];
            }

            public bool GetSATriggerCondition(int index)
            {
                return SATriggerCondition[index];
            }

            public GyroOutMode GetGyroOutMode(int device)
            {
                return GyroOutputMode[device];
            }

            public string GetSAMouseStickTriggers(int device)
            {
                return SAMouseStickTriggers[device];
            }

            public bool GetSAMouseStickTriggerCond(int device)
            {
                return SAMouseStickTriggerCond[device];
            }

            public bool GetGyroMouseStickTriggerTurns(int device)
            {
                return GyroMouseStickTriggerTurns[device];
            }

            public int GetGyroMouseStickHorizontalAxis(int index)
            {
                return GyroMouseStickHorizontalAxis[index];
            }

            public GyroMouseStickInfo GetGyroMouseStickInfo(int device)
            {
                return GyroMouseStickInfo[device];
            }

            public GyroDirectionalSwipeInfo GetGyroSwipeInfo(int device)
            {
                return GyroSwipeInfo[device];
            }

            public LightbarSettingInfo GetLightbarSettingsInfo(int index)
            {
                return LightbarSettingInfo[index];
            }

            public bool GetDirectInputOnly(int index)
            {
                return DirectInputOnly[index];
            }

            public bool IsUsingTouchpadForControls(int index)
            {
                return TouchOutMode[index] == TouchpadOutMode.Controls;
            }

            public bool IsUsingSAForControls(int index)
            {
                return GyroOutputMode[index] == GyroOutMode.Controls;
            }

            public SASteeringWheelEmulationAxisType GetSASteeringWheelEmulationAxis(int index)
            {
                return SASteeringWheelEmulationAxis[index];
            }

            public int GetSASteeringWheelEmulationRange(int index)
            {
                return SASteeringWheelEmulationRange[index];
            }

            public int GetGyroSensitivity(int index)
            {
                return GyroSensitivity[index];
            }

            public int GetGyroSensVerticalScale(int index)
            {
                return GyroSensVerticalScale[index];
            }

            public int GetGyroInvert(int index)
            {
                return GyroInvert[index];
            }

            public bool GetGyroTriggerTurns(int index)
            {
                return GyroTriggerTurns[index];
            }

            public int GetGyroMouseHorizontalAxis(int index)
            {
                return GyroMouseHorizontalAxis[index];
            }

            public GyroControlsInfo GetGyroControlsInfo(int index)
            {
                return GyroControlsInfo[index];
            }

            public byte GetTapSensitivity(int index)
            {
                return TapSensitivity[index];
            }

            public bool GetDoubleTap(int index)
            {
                return DoubleTap[index];
            }

            public bool GetTouchPadJitterCompensation(int index)
            {
                return TouchpadJitterCompensation[index];
            }

            public int GetTouchPadInvert(int index)
            {
                return TouchPadInvert[index];
            }

            public TriggerDeadZoneZInfo GetL2ModInfo(int index)
            {
                return L2ModInfo[index];
            }

            public TriggerDeadZoneZInfo GetR2ModInfo(int index)
            {
                return R2ModInfo[index];
            }

            public double GetSXDeadZone(int index)
            {
                return SXDeadzone[index];
            }

            public double GetSZDeadZone(int index)
            {
                return SZDeadzone[index];
            }

            public int GetLSDeadZone(int index)
            {
                return LSModInfo[index].DeadZone;
            }

            public int GetRSDeadZone(int index)
            {
                return RSModInfo[index].DeadZone;
            }

            public StickDeadZoneInfo GetLSDeadInfo(int index)
            {
                return LSModInfo[index];
            }

            public StickDeadZoneInfo GetRSDeadInfo(int index)
            {
                return RSModInfo[index];
            }

            public double GetSXAntiDeadZone(int index)
            {
                return SXAntiDeadzone[index];
            }

            public double GetSZAntiDeadZone(int index)
            {
                return SZAntiDeadzone[index];
            }

            public double GetSXMaxZone(int index)
            {
                return SXMaxzone[index];
            }

            public double GetSZMaxZone(int index)
            {
                return SZMaxzone[index];
            }

            public double GetLSRotation(int index)
            {
                return LSRotation[index];
            }

            public double GetRSRotation(int index)
            {
                return RSRotation[index];
            }

            public double GetL2Sens(int index)
            {
                return L2Sens[index];
            }

            public double GetR2Sens(int index)
            {
                return R2Sens[index];
            }

            public double GetSXSens(int index)
            {
                return SXSens[index];
            }

            public double GetSZSens(int index)
            {
                return SZSens[index];
            }

            public double GetLSSens(int index)
            {
                return LSSens[index];
            }

            public double GetRSSens(int index)
            {
                return RSSens[index];
            }

            public int GetBluetoothPollRate(int index)
            {
                return BluetoothPollRate[index];
            }

            public SquareStickInfo GetSquareStickInfo(int device)
            {
                return SquStickInfo[device];
            }

            public StickAntiSnapbackInfo GetLSAntiSnapbackInfo(int device)
            {
                return LSAntiSnapbackInfo[device];
            }

            public StickAntiSnapbackInfo GetRSAntiSnapbackInfo(int device)
            {
                return RSAntiSnapbackInfo[device];
            }

            public bool GetTrackballMode(int index)
            {
                return TrackballMode[index];
            }

            public double GetTrackballFriction(int index)
            {
                return TrackballFriction[index];
            }

            public int GetProfileActionCount(int index)
            {
                return profileActionCount[index];
            }

            public byte GetRumbleBoost(int index)
            {
                return RumbleBoost[index];
            }

            public int GetRumbleAutostopTime(int index)
            {
                return RumbleAutostopTime[index];
            }

            public bool GetEnableTouchToggle(int index)
            {
                return EnableTouchToggle[index];
            }

            public int GetIdleDisconnectTimeout(int index)
            {
                return IdleDisconnectTimeout[index];
            }

            public bool GetEnableOutputDataToDS4(int index)
            {
                return EnableOutputDataToDS4[index];
            }

            public byte GetTouchSensitivity(int index)
            {
                return TouchSensitivity[index];
            }

            public ControlSettingsGroup GetControlSettingsGroup(int deviceNum)
            {
                return ds4controlSettings[deviceNum];
            }

            public void EstablishDefaultSpecialActions(int idx)
            {
                ProfileActions[idx] = new List<string> { "Disconnect Controller" };
                profileActionCount[idx] = ProfileActions[idx].Count;
            }

            public void CacheProfileCustomsFlags(int device)
            {
                var customAct = false;
                ContainsCustomAction[device] = customAct = HasCustomActions(device);
                ContainsCustomExtras[device] = HasCustomExtras(device);

                if (!customAct)
                {
                    customAct = GyroOutputMode[device] == GyroOutMode.MouseJoystick;
                    customAct = customAct ||
                                SASteeringWheelEmulationAxis[device] >= SASteeringWheelEmulationAxisType.VJoy1X;
                    customAct = customAct || LSOutputSettings[device].Mode != StickMode.Controls;
                    customAct = customAct || RSOutputSettings[device].Mode != StickMode.Controls;
                    ContainsCustomAction[device] = customAct;
                }
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

                foreach (var actionname in ProfileActions[device])
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
                    var sA = Actions[i];
                    if (sA.Name == name)
                        return sA;
                }

                return new SpecialAction("null", "null", "null", "null");
            }

            public int GetActionIndexOf(string name)
            {
                for (int i = 0, actionCount = Actions.Count; i < actionCount; i++)
                    if (Actions[i].Name == name)
                        return i;

                return -1;
            }

            public void SetSaTriggerCond(int index, string text)
            {
                SATriggerCondition[index] = SaTriggerCondValue(text);
            }

            public void SetSaMouseStickTriggerCond(int index, string text)
            {
                SAMouseStickTriggerCond[index] = SaTriggerCondValue(text);
            }

            public void SetGyroMouseDZ(int index, int value, ControlService control)
            {
                GyroMouseDeadZone[index] = value;
                if (index < ControlService.CURRENT_DS4_CONTROLLER_LIMIT && control.touchPad[index] != null)
                    control.touchPad[index].CursorGyroDead = value;
            }

            public void SetGyroControlsToggle(int index, bool value, ControlService control)
            {
                GyroControlsInfo[index].TriggerToggle = value;
                if (index < ControlService.CURRENT_DS4_CONTROLLER_LIMIT && control.touchPad[index] != null)
                    control.touchPad[index].ToggleGyroControls = value;
            }

            public void SetGyroMouseToggle(int index, bool value, ControlService control)
            {
                GyroMouseToggle[index] = value;
                if (index < ControlService.CURRENT_DS4_CONTROLLER_LIMIT && control.touchPad[index] != null)
                    control.touchPad[index].ToggleGyroMouse = value;
            }

            public void SetGyroMouseStickToggle(int index, bool value, ControlService control)
            {
                GyroMouseStickToggle[index] = value;
                if (index < ControlService.CURRENT_DS4_CONTROLLER_LIMIT && control.touchPad[index] != null)
                    control.touchPad[index].ToggleGyroStick = value;
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

            public async Task<bool> SaveAsNewProfile(int device, string proName)
            {
                ResetProfile(device);
                return await SaveProfile(device, proName);
            }

            /// <summary>
            ///     Persists <see cref="DS4WindowsAppSettings" /> on disk.
            /// </summary>
            /// <returns>True on success, false otherwise.</returns>
            [ConfigurationSystemComponent]
            public bool SaveApplicationSettings()
            {
                var saved = true;

                var settings = new DS4WindowsAppSettings(this, ExecutableProductVersion, APP_CONFIG_VERSION);

                try
                {
                    using var stream = File.Open(ProfilesPath, FileMode.Create);

                    settings.Serialize(stream);
                }
                catch (UnauthorizedAccessException)
                {
                    saved = false;
                }

                //
                // TODO: WTF?!
                // 
                var adminNeeded = IsAdminNeeded;
                if (saved &&
                    (!adminNeeded || adminNeeded && IsAdministrator))
                {
                    var custom_exe_name_path = Path.Combine(ExecutableDirectory, CUSTOM_EXE_CONFIG_FILENAME);
                    var fakeExeFileExists = File.Exists(custom_exe_name_path);
                    if (!string.IsNullOrEmpty(FakeExeFileName) || fakeExeFileExists)
                        File.WriteAllText(custom_exe_name_path, FakeExeFileName);
                }

                return saved;
            }

            /// <summary>
            ///     Restores <see cref="DS4WindowsAppSettings" /> from disk.
            /// </summary>
            /// <returns>True on success, false otherwise.</returns>
            [ConfigurationSystemComponent]
            public async Task<bool> LoadApplicationSettings()
            {
                var loaded = true;

                if (File.Exists(ProfilesPath))
                {
                    await using (var stream = File.OpenRead(ProfilesPath))
                    {
                        (await DS4WindowsAppSettings.DeserializeAsync(stream)).CopyTo(this);
                    }

                    if (loaded)
                    {
                        var custom_exe_name_path = Path.Combine(ExecutableDirectory, CUSTOM_EXE_CONFIG_FILENAME);
                        var fakeExeFileExists = File.Exists(custom_exe_name_path);
                        if (fakeExeFileExists)
                        {
                            var fake_exe_name = (await File.ReadAllTextAsync(custom_exe_name_path)).Trim();
                            var valid = !(fake_exe_name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0);
                            if (valid) FakeExeFileName = fake_exe_name;
                        }
                    }
                }

                return loaded;
            }

            /// <summary>
            ///     Persists a <see cref="DS4WindowsProfile" /> on disk.
            /// </summary>
            /// <param name="device">The index of the device to store the profile for.</param>
            /// <param name="proName">The profile name (without extension or root path).</param>
            /// <returns>True on success, false otherwise.</returns>
            [ConfigurationSystemComponent]
            public async Task<bool> SaveProfile(int device, string proName)
            {
                var saved = true;

                if (proName.EndsWith(XML_EXTENSION))
                    proName = proName.Remove(proName.LastIndexOf(XML_EXTENSION, StringComparison.Ordinal));

                var path = Path.Combine(
                    RuntimeAppDataPath,
                    Constants.ProfilesSubDirectory,
                    $"{proName}{XML_EXTENSION}"
                );

                var profileObject = new DS4WindowsProfile(
                    this,
                    device,
                    ExecutableProductVersion,
                    CONFIG_VERSION
                );

#if WITH_TRACING
                using var scope = GlobalTracer.Instance.BuildSpan(nameof(SaveProfile)).StartActive(true);
#endif

                try
                {
                    foreach (var dcs in Ds4Settings[device])
                    {
                        var property = $"{dcs.Control}.Value";

                        if (dcs.ControlActionType != DS4ControlSettings.ActionType.Default)
                        {
                            var keyType = string.Empty;

                            if (dcs.ControlActionType == DS4ControlSettings.ActionType.Button &&
                                dcs.ActionData.ActionButton == X360Controls.Unbound)
                                keyType += DS4KeyType.Unbound;

                            if (dcs.KeyType.HasFlag(DS4KeyType.HoldMacro))
                                keyType += DS4KeyType.HoldMacro;
                            else if (dcs.KeyType.HasFlag(DS4KeyType.Macro))
                                keyType += DS4KeyType.Macro;

                            if (dcs.KeyType.HasFlag(DS4KeyType.Toggle))
                                keyType += DS4KeyType.Toggle;
                            if (dcs.KeyType.HasFlag(DS4KeyType.ScanCode))
                                keyType += DS4KeyType.ScanCode;

                            if (string.IsNullOrEmpty(keyType))
                                SetNestedProperty(property, profileObject.Controls.KeyTypes, keyType);

                            if (dcs.ControlActionType == DS4ControlSettings.ActionType.Macro)
                            {
                                var ii = dcs.ActionData.ActionMacro;

                                SetNestedProperty(property, profileObject.Controls.Macros, string.Join("/", ii));
                            }
                            else if (dcs.ControlActionType == DS4ControlSettings.ActionType.Key)
                            {
                                SetNestedProperty(property, profileObject.Controls.Keys,
                                    dcs.ActionData.ActionKey.ToString());
                            }
                            else if (dcs.ControlActionType == DS4ControlSettings.ActionType.Button)
                            {
                                SetNestedProperty(property, profileObject.Controls.Buttons,
                                    GetX360ControlString(dcs.ActionData.ActionButton));
                            }
                        }

                        var hasValue = false;
                        if (!string.IsNullOrEmpty(dcs.Extras))
                            if (dcs.Extras.Split(',').Any(s => s != "0"))
                                hasValue = true;

                        if (hasValue) SetNestedProperty(property, profileObject.Controls.Extras, dcs.Extras);

                        if (dcs.ShiftActionType != DS4ControlSettings.ActionType.Default && dcs.ShiftTrigger > 0)
                        {
                            var keyType = string.Empty;

                            if (dcs.ShiftActionType == DS4ControlSettings.ActionType.Button &&
                                dcs.ShiftAction.ActionButton == X360Controls.Unbound)
                                keyType += DS4KeyType.Unbound;

                            if (dcs.ShiftKeyType.HasFlag(DS4KeyType.HoldMacro))
                                keyType += DS4KeyType.HoldMacro;
                            if (dcs.ShiftKeyType.HasFlag(DS4KeyType.Macro))
                                keyType += DS4KeyType.Macro;
                            if (dcs.ShiftKeyType.HasFlag(DS4KeyType.Toggle))
                                keyType += DS4KeyType.Toggle;
                            if (dcs.ShiftKeyType.HasFlag(DS4KeyType.ScanCode))
                                keyType += DS4KeyType.ScanCode;

                            if (keyType != string.Empty)
                                SetNestedProperty(property, profileObject.ShiftControls.KeyTypes, keyType);

                            if (dcs.ShiftActionType == DS4ControlSettings.ActionType.Macro)
                            {
                                var ii = dcs.ShiftAction.ActionMacro;

                                SetNestedProperty(property, profileObject.ShiftControls.Macros, string.Join("/", ii));
                                SetNestedProperty($"{dcs.Control}.ShiftTrigger", profileObject.ShiftControls.Macros,
                                    dcs.ShiftTrigger.ToString());
                            }
                            else if (dcs.ShiftActionType == DS4ControlSettings.ActionType.Key)
                            {
                                SetNestedProperty(property, profileObject.ShiftControls.Keys,
                                    dcs.ShiftAction.ActionKey.ToString());
                                SetNestedProperty($"{dcs.Control}.ShiftTrigger", profileObject.ShiftControls.Keys,
                                    dcs.ShiftTrigger.ToString());
                            }
                            else if (dcs.ShiftActionType == DS4ControlSettings.ActionType.Button)
                            {
                                SetNestedProperty(property, profileObject.ShiftControls.Buttons,
                                    dcs.ShiftAction.ActionKey.ToString());
                                SetNestedProperty($"{dcs.Control}.ShiftTrigger", profileObject.ShiftControls.Buttons,
                                    dcs.ShiftTrigger.ToString());
                            }
                        }

                        hasValue = false;
                        if (!string.IsNullOrEmpty(dcs.ShiftExtras))
                            if (dcs.ShiftExtras.Split(',').Any(s => s != "0"))
                                hasValue = true;

                        if (hasValue)
                        {
                            SetNestedProperty(property, profileObject.ShiftControls.Extras, dcs.ShiftExtras);
                            SetNestedProperty($"{dcs.Control}.ShiftTrigger", profileObject.ShiftControls.Extras,
                                dcs.ShiftTrigger.ToString());
                        }
                    }

                    await using var file = File.Open(path, FileMode.Create);

                    await profileObject.SerializeAsync(file);
                }
                catch
                {
                    saved = false;
                }

                return saved;
            }

            [ConfigurationSystemComponent]
            public async Task<bool> LoadProfile(
                int device,
                bool launchprogram,
                ControlService control,
                string profilePath = null,
                bool xinputChange = true,
                bool postLoad = true
            )
            {
                var loaded = true;
                var customMapKeyTypes = new Dictionary<DS4Controls, DS4KeyType>();
                var customMapKeys = new Dictionary<DS4Controls, ushort>();
                var customMapButtons = new Dictionary<DS4Controls, X360Controls>();
                var customMapMacros = new Dictionary<DS4Controls, string>();
                var customMapExtras = new Dictionary<DS4Controls, string>();
                var shiftCustomMapKeyTypes = new Dictionary<DS4Controls, DS4KeyType>();
                var shiftCustomMapKeys = new Dictionary<DS4Controls, ushort>();
                var shiftCustomMapButtons = new Dictionary<DS4Controls, X360Controls>();
                var shiftCustomMapMacros = new Dictionary<DS4Controls, string>();
                var shiftCustomMapExtras = new Dictionary<DS4Controls, string>();
                var rootname = "DS4Windows";
                var missingSetting = false;
                var migratePerformed = false;
                string profilepath;
                if (string.IsNullOrEmpty(profilePath))
                    profilepath = Path.Combine(RuntimeAppDataPath, Constants.ProfilesSubDirectory,
                        $"{ProfilePath[device]}{XML_EXTENSION}");
                else
                    profilepath = profilePath;

                var xinputPlug = false;
                var xinputStatus = false;

#if WITH_TRACING
                using var scope = GlobalTracer.Instance.BuildSpan(nameof(LoadProfile)).StartActive(true);
#endif

                if (File.Exists(profilepath))
                {
                    XmlNode Item;

                    m_Xdoc.Load(profilepath);

                    if (device < MAX_DS4_CONTROLLER_COUNT)
                    {
                        DS4LightBar.forcelight[device] = false;
                        DS4LightBar.forcedFlash[device] = 0;
                    }

                    var oldContType = ActiveOutDevType[device];

                    // Make sure to reset currently set profile values before parsing
                    ResetProfile(device);
                    ResetMouseProperties(device, control);

                    DS4WindowsProfile profile = null;

                    //
                    // TODO: unfinished
                    // 
                    await using (var stream = File.OpenRead(profilepath))
                    {
                        profile = await DS4WindowsProfile.DeserializeAsync(stream);

                        profile.CopyTo(this, device);
                    }


                    var shiftM = 0;
                    if (m_Xdoc.SelectSingleNode("/" + rootname + "/ShiftModifier") != null)
                        int.TryParse(m_Xdoc.SelectSingleNode("/" + rootname + "/ShiftModifier").InnerText, out shiftM);


                    if (launchprogram && LaunchProgram[device] != string.Empty)
                    {
                        var programPath = LaunchProgram[device];
                        var localAll = Process.GetProcesses();
                        var procFound = false;
                        for (int procInd = 0, procsLen = localAll.Length; !procFound && procInd < procsLen; procInd++)
                            try
                            {
                                var temp = localAll[procInd].MainModule.FileName;
                                if (temp == programPath) procFound = true;
                            }
                            // Ignore any process for which this information
                            // is not exposed
                            catch
                            {
                            }

                        if (!procFound)
                        {
                            var processTask = new Task(() =>
                            {
                                Thread.Sleep(5000);
                                var tempProcess = new Process();
                                tempProcess.StartInfo.FileName = programPath;
                                tempProcess.StartInfo.WorkingDirectory = new FileInfo(programPath).Directory.ToString();
                                //tempProcess.StartInfo.UseShellExecute = false;
                                try
                                {
                                    tempProcess.Start();
                                }
                                catch
                                {
                                }
                            });

                            processTask.Start();
                        }
                    }


                    // Fallback lookup if TouchpadOutMode is not set
                    var tpForControlsPresent = false;
                    var xmlUseTPForControlsElement =
                        m_Xdoc.SelectSingleNode("/" + rootname + "/UseTPforControls");
                    tpForControlsPresent = xmlUseTPForControlsElement != null;
                    if (tpForControlsPresent)
                        try
                        {
                            Item = m_Xdoc.SelectSingleNode("/" + rootname + "/UseTPforControls");
                            if (bool.TryParse(Item?.InnerText ?? "", out var temp))
                                if (temp)
                                    TouchOutMode[device] = TouchpadOutMode.Controls;
                        }
                        catch
                        {
                            TouchOutMode[device] = TouchpadOutMode.Mouse;
                        }

                    // Fallback lookup if GyroOutMode is not set
                    try
                    {
                        Item = m_Xdoc.SelectSingleNode("/" + rootname + "/UseSAforMouse");
                        if (bool.TryParse(Item?.InnerText ?? "", out var temp))
                            if (temp)
                                GyroOutputMode[device] = GyroOutMode.Mouse;
                    }
                    catch
                    {
                        GyroOutputMode[device] = GyroOutMode.Controls;
                    }

                    try
                    {
                        Item = m_Xdoc.SelectSingleNode("/" + rootname + "/GyroMouseStickToggle");
                        bool.TryParse(Item.InnerText, out var temp);
                        SetGyroMouseStickToggle(device, temp, control);
                    }
                    catch
                    {
                        SetGyroMouseStickToggle(device, false, control);
                        missingSetting = true;
                    }


                    // Check for TouchpadOutputMode if UseTPforControls is not present in profile
                    if (!tpForControlsPresent)
                        try
                        {
                            Item = m_Xdoc.SelectSingleNode("/" + rootname + "/TouchpadOutputMode");
                            var tempMode = Item.InnerText;
                            Enum.TryParse(tempMode, out TouchpadOutMode value);
                            TouchOutMode[device] = value;
                        }
                        catch
                        {
                            TouchOutMode[device] = TouchpadOutMode.Mouse;
                            missingSetting = true;
                        }


                    /*try { Item = m_Xdoc.SelectSingleNode("/" + rootname + "/GyroSmoothing"); bool.TryParse(Item.InnerText, out gyroSmoothing[device]); }
                    catch { gyroSmoothing[device] = false; missingSetting = true; }

                    try { Item = m_Xdoc.SelectSingleNode("/" + rootname + "/GyroSmoothingWeight"); int temp = 0; int.TryParse(Item.InnerText, out temp); gyroSmoothWeight[device] = Math.Min(Math.Max(0.0, Convert.ToDouble(temp * 0.01)), 1.0); }
                    catch { gyroSmoothWeight[device] = 0.5; missingSetting = true; }
                    */

                    try
                    {
                        Item = m_Xdoc.SelectSingleNode("/" + rootname + "/GyroMouseDeadZone");
                        int.TryParse(Item.InnerText, out var temp);
                        SetGyroMouseDZ(device, temp, control);
                    }
                    catch
                    {
                        SetGyroMouseDZ(device, MouseCursor.GYRO_MOUSE_DEADZONE, control);
                        missingSetting = true;
                    }

                    try
                    {
                        Item = m_Xdoc.SelectSingleNode("/" + rootname + "/GyroMouseToggle");
                        bool.TryParse(Item.InnerText, out var temp);
                        SetGyroMouseToggle(device, temp, control);
                    }
                    catch
                    {
                        SetGyroMouseToggle(device, false, control);
                        missingSetting = true;
                    }


                    // Note! xxOutputCurveCustom property needs to be read before xxOutputCurveMode property in case the curveMode is value 6


                    try
                    {
                        Item = m_Xdoc.SelectSingleNode("/" + rootname + "/L2HipFireDelay");
                        if (int.TryParse(Item?.InnerText, out var temp))
                            L2OutputSettings[device].hipFireMS = Math.Max(Math.Min(0, temp), 5000);
                    }
                    catch
                    {
                    }

                    try
                    {
                        Item = m_Xdoc.SelectSingleNode("/" + rootname + "/R2HipFireDelay");
                        if (int.TryParse(Item?.InnerText, out var temp))
                            R2OutputSettings[device].hipFireMS = Math.Max(Math.Min(0, temp), 5000);
                    }
                    catch
                    {
                    }


                    // Only change xinput devices under certain conditions. Avoid
                    // performing this upon program startup before loading devices.
                    if (xinputChange && device < ControlService.CURRENT_DS4_CONTROLLER_LIMIT)
                        CheckOldDeviceStatus(device, control, oldContType,
                            out xinputPlug, out xinputStatus);

                    try
                    {
                        Item = m_Xdoc.SelectSingleNode("/" + rootname + "/ProfileActions");
                        ProfileActions[device].Clear();
                        if (!string.IsNullOrEmpty(Item.InnerText))
                        {
                            var actionNames = Item.InnerText.Split('/');
                            for (int actIndex = 0, actLen = actionNames.Length; actIndex < actLen; actIndex++)
                            {
                                var tempActionName = actionNames[actIndex];
                                if (!ProfileActions[device].Contains(tempActionName))
                                    ProfileActions[device].Add(tempActionName);
                            }
                        }
                    }
                    catch
                    {
                        ProfileActions[device].Clear();
                        missingSetting = true;
                    }

                    foreach (var dcs in Ds4Settings[device])
                        dcs.Reset();

                    ContainsCustomAction[device] = false;
                    ContainsCustomExtras[device] = false;
                    profileActionCount[device] = ProfileActions[device].Count;
                    profileActionDict[device].Clear();
                    profileActionIndexDict[device].Clear();
                    foreach (var actionname in ProfileActions[device])
                    {
                        profileActionDict[device][actionname] = Instance.Config.GetAction(actionname);
                        profileActionIndexDict[device][actionname] = Instance.Config.GetActionIndexOf(actionname);
                    }

                    DS4KeyType keyType;
                    ushort wvk;

                    //
                    // Buttons
                    // 
                    {
                        var controls = typeof(ControlsCollection)
                            .GetProperties()
                            .Select(p => new
                            {
                                p.Name,
                                Entity = (ControlsCollectionEntity)typeof(ControlsCollection)
                                    .GetProperty(p.Name)
                                    .GetValue(profile.Controls.Buttons)
                            })
                            .Where(e => !string.IsNullOrEmpty(e.Entity.Value))
                            .ToList();

                        foreach (var item in controls.Where(item => Enum.TryParse(item.Name, out DS4Controls _)))
                        {
                            UpdateDs4ControllerSetting(device, item.Name, false,
                                GetX360ControlsByName(item.Entity.Value), "", DS4KeyType.None);
                            customMapButtons.Add(GetDs4ControlsByName(item.Name),
                                GetX360ControlsByName(item.Entity.Value));
                        }
                    }

                    //
                    // Macros
                    // 
                    {
                        var controls = typeof(ControlsCollection)
                            .GetProperties()
                            .Select(p => new
                            {
                                p.Name,
                                Entity = (ControlsCollectionEntity)typeof(ControlsCollection)
                                    .GetProperty(p.Name)
                                    .GetValue(profile.Controls.Macros)
                            })
                            .Where(e => !string.IsNullOrEmpty(e.Entity.Value))
                            .ToList();

                        foreach (var item in controls)
                        {
                            customMapMacros.Add(GetDs4ControlsByName(item.Name), item.Entity.Value);
                            string[] skeys;
                            int[] keys;
                            if (!string.IsNullOrEmpty(item.Entity.Value))
                            {
                                skeys = item.Entity.Value.Split('/');
                                keys = new int[skeys.Length];
                            }
                            else
                            {
                                skeys = Array.Empty<string>();
                                keys = Array.Empty<int>();
                            }

                            for (int i = 0, keysLength = keys.Length; i < keysLength; i++)
                                keys[i] = int.Parse(skeys[i]);

                            if (Enum.TryParse(item.Name, out DS4Controls _))
                                UpdateDs4ControllerSetting(device, item.Name, false, keys, "", DS4KeyType.None);
                        }
                    }

                    //
                    // Keys
                    //
                    {
                        var controls = typeof(ControlsCollection)
                            .GetProperties()
                            .Select(p => new
                            {
                                p.Name,
                                Entity = (ControlsCollectionEntity)typeof(ControlsCollection)
                                    .GetProperty(p.Name)
                                    .GetValue(profile.Controls.Keys)
                            })
                            .Where(e => !string.IsNullOrEmpty(e.Entity.Value))
                            .ToList();

                        foreach (var item in controls)
                            if (ushort.TryParse(item.Entity.Value, out wvk) &&
                                Enum.TryParse(item.Name, out DS4Controls _))
                            {
                                UpdateDs4ControllerSetting(device, item.Name, false, wvk, "", DS4KeyType.None);
                                customMapKeys.Add(GetDs4ControlsByName(item.Name), wvk);
                            }
                    }

                    //
                    // Extras
                    // 
                    {
                        var controls = typeof(ControlsCollection)
                            .GetProperties()
                            .Select(p => new
                            {
                                p.Name,
                                Entity = (ControlsCollectionEntity)typeof(ControlsCollection)
                                    .GetProperty(p.Name)
                                    .GetValue(profile.Controls.Extras)
                            })
                            .Where(e => !string.IsNullOrEmpty(e.Entity.Value))
                            .ToList();

                        foreach (var item in controls.Where(item => item.Entity.Value != string.Empty &&
                                                                    Enum.TryParse(item.Name, out DS4Controls _)))
                        {
                            UpdateDs4ControllerExtra(device, item.Name, false, item.Entity.Value);
                            customMapExtras.Add(GetDs4ControlsByName(item.Name), item.Entity.Value);
                        }
                    }

                    //
                    // KeyTypes
                    // 
                    {
                        var controls = typeof(ControlsCollection)
                            .GetProperties()
                            .Select(p => new
                            {
                                p.Name,
                                Entity = (ControlsCollectionEntity)typeof(ControlsCollection)
                                    .GetProperty(p.Name)
                                    .GetValue(profile.Controls.KeyTypes)
                            })
                            .Where(e => !string.IsNullOrEmpty(e.Entity.Value))
                            .ToList();

                        foreach (var item in controls)
                        {
                            keyType = DS4KeyType.None;
                            if (item.Entity.Value.Contains(DS4KeyType.ScanCode.ToString()))
                                keyType |= DS4KeyType.ScanCode;
                            if (item.Entity.Value.Contains(DS4KeyType.Toggle.ToString()))
                                keyType |= DS4KeyType.Toggle;
                            if (item.Entity.Value.Contains(DS4KeyType.Macro.ToString()))
                                keyType |= DS4KeyType.Macro;
                            if (item.Entity.Value.Contains(DS4KeyType.HoldMacro.ToString()))
                                keyType |= DS4KeyType.HoldMacro;
                            if (item.Entity.Value.Contains(DS4KeyType.Unbound.ToString()))
                                keyType |= DS4KeyType.Unbound;

                            if (keyType == DS4KeyType.None || !Enum.TryParse(item.Name, out DS4Controls _)) continue;

                            UpdateDs4ControllerKeyType(device, item.Name, false, keyType);
                            customMapKeyTypes.Add(GetDs4ControlsByName(item.Name), keyType);
                        }
                    }

                    //
                    // ShiftControl/Button
                    // 
                    {
                        var controls = typeof(ControlsCollection)
                            .GetProperties()
                            .Select(p => new
                            {
                                p.Name,
                                Entity = (ControlsCollectionEntity)typeof(ControlsCollection)
                                    .GetProperty(p.Name)
                                    .GetValue(profile.ShiftControls.Buttons)
                            })
                            .Where(e => !string.IsNullOrEmpty(e.Entity.Value))
                            .ToList();

                        foreach (var item in controls)
                        {
                            var shiftT = shiftM;
                            if (!string.IsNullOrEmpty(item.Entity.ShiftTrigger))
                                int.TryParse(item.Entity.ShiftTrigger, out shiftT);

                            if (Enum.TryParse(item.Name, out DS4Controls _))
                            {
                                UpdateDs4ControllerSetting(device, item.Name, true,
                                    GetX360ControlsByName(item.Entity.Value), "", DS4KeyType.None, shiftT);
                                shiftCustomMapButtons.Add(GetDs4ControlsByName(item.Name),
                                    GetX360ControlsByName(item.Entity.Value));
                            }
                        }
                    }

                    //
                    // ShiftControl/Macro
                    // 
                    {
                        var controls = typeof(ControlsCollection)
                            .GetProperties()
                            .Select(p => new
                            {
                                p.Name,
                                Entity = (ControlsCollectionEntity)typeof(ControlsCollection)
                                    .GetProperty(p.Name)
                                    .GetValue(profile.ShiftControls.Macros)
                            })
                            .Where(e => !string.IsNullOrEmpty(e.Entity.Value))
                            .ToList();

                        foreach (var item in controls)
                        {
                            shiftCustomMapMacros.Add(GetDs4ControlsByName(item.Name), item.Entity.Value);
                            string[] skeys;
                            int[] keys;
                            if (!string.IsNullOrEmpty(item.Entity.Value))
                            {
                                skeys = item.Entity.Value.Split('/');
                                keys = new int[skeys.Length];
                            }
                            else
                            {
                                skeys = Array.Empty<string>();
                                keys = Array.Empty<int>();
                            }

                            for (int i = 0, keysLength = keys.Length; i < keysLength; i++)
                                keys[i] = int.Parse(skeys[i]);

                            var shiftT = shiftM;
                            if (string.IsNullOrEmpty(item.Entity.ShiftTrigger))
                                int.TryParse(item.Entity.ShiftTrigger, out shiftT);

                            if (Enum.TryParse(item.Name, out DS4Controls _))
                                UpdateDs4ControllerSetting(device, item.Name, true, keys, "", DS4KeyType.None,
                                    shiftT);
                        }
                    }

                    // 
                    // ShiftControl/Key
                    // 
                    {
                        var controls = typeof(ControlsCollection)
                            .GetProperties()
                            .Select(p => new
                            {
                                p.Name,
                                Entity = (ControlsCollectionEntity)typeof(ControlsCollection)
                                    .GetProperty(p.Name)
                                    .GetValue(profile.ShiftControls.Keys)
                            })
                            .Where(e => !string.IsNullOrEmpty(e.Entity.Value))
                            .ToList();

                        foreach (var item in controls)
                            if (ushort.TryParse(item.Entity.Value, out wvk))
                            {
                                var shiftT = shiftM;
                                if (string.IsNullOrEmpty(item.Entity.ShiftTrigger))
                                    int.TryParse(item.Entity.ShiftTrigger, out shiftT);

                                if (Enum.TryParse(item.Name, out DS4Controls _))
                                {
                                    UpdateDs4ControllerSetting(device, item.Name, true, wvk, "", DS4KeyType.None,
                                        shiftT);
                                    shiftCustomMapKeys.Add(GetDs4ControlsByName(item.Name), wvk);
                                }
                            }
                    }

                    //
                    // ShiftControl/Extras
                    // 
                    {
                        var controls = typeof(ControlsCollection)
                            .GetProperties()
                            .Select(p => new
                            {
                                p.Name,
                                Entity = (ControlsCollectionEntity)typeof(ControlsCollection)
                                    .GetProperty(p.Name)
                                    .GetValue(profile.ShiftControls.Extras)
                            })
                            .Where(e => !string.IsNullOrEmpty(e.Entity.Value))
                            .ToList();

                        foreach (var item in controls.Where(item => Enum.TryParse(item.Name, out DS4Controls _)))
                        {
                            UpdateDs4ControllerExtra(device, item.Name, true, item.Entity.Value);
                            shiftCustomMapExtras.Add(GetDs4ControlsByName(item.Name), item.Entity.Value);
                        }
                    }

                    //
                    // ShiftControl/KeyType
                    // 
                    {
                        var controls = typeof(ControlsCollection)
                            .GetProperties()
                            .Select(p => new
                            {
                                p.Name,
                                Entity = (ControlsCollectionEntity)typeof(ControlsCollection)
                                    .GetProperty(p.Name)
                                    .GetValue(profile.ShiftControls.KeyTypes)
                            })
                            .Where(e => !string.IsNullOrEmpty(e.Entity.Value))
                            .ToList();

                        foreach (var item in controls)
                        {
                            keyType = DS4KeyType.None;
                            if (item.Entity.Value.Contains(DS4KeyType.ScanCode.ToString()))
                                keyType |= DS4KeyType.ScanCode;
                            if (item.Entity.Value.Contains(DS4KeyType.Toggle.ToString()))
                                keyType |= DS4KeyType.Toggle;
                            if (item.Entity.Value.Contains(DS4KeyType.Macro.ToString()))
                                keyType |= DS4KeyType.Macro;
                            if (item.Entity.Value.Contains(DS4KeyType.HoldMacro.ToString()))
                                keyType |= DS4KeyType.HoldMacro;
                            if (item.Entity.Value.Contains(DS4KeyType.Unbound.ToString()))
                                keyType |= DS4KeyType.Unbound;

                            if (keyType != DS4KeyType.None &&
                                Enum.TryParse(item.Name, out DS4Controls _))
                            {
                                UpdateDs4ControllerKeyType(device, item.Name, true, keyType);
                                shiftCustomMapKeyTypes.Add(GetDs4ControlsByName(item.Name), keyType);
                            }
                        }
                    }
                }
                else
                {
                    loaded = false;
                    ResetProfile(device);
                    ResetMouseProperties(device, control);

                    // Unplug existing output device if requested profile does not exist
                    var tempOutDev = device < ControlService.CURRENT_DS4_CONTROLLER_LIMIT
                        ? control.outputDevices[device]
                        : null;
                    if (tempOutDev != null)
                    {
                        tempOutDev = null;
                        //Global.ActiveOutDevType[device] = OutContType.None;
                        var tempDev = control.DS4Controllers[device];
                        if (tempDev != null)
                            tempDev.queueEvent(() => { control.UnplugOutDev(device, tempDev); });
                    }
                }

                // Only add missing settings if the actual load was graceful
                if ((missingSetting || migratePerformed) && loaded) // && buttons != null)
                {
                    var proName = Path.GetFileName(profilepath);
                    await SaveProfile(device, proName);
                }

                if (loaded)
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
                    if (postLoad && device < MAX_DS4_CONTROLLER_COUNT)
                        PostLoadSnippet(device, control, xinputStatus, xinputPlug);
                }

                return loaded;
            }

            [ConfigurationSystemComponent]
            public bool SaveAction(string name, string controls, int mode, string details, bool edit,
                string extras = "")
            {
                var saved = true;
                if (!File.Exists(ActionsPath))
                    CreateAction();

                try
                {
                    m_Xdoc.Load(ActionsPath);
                }
                catch (XmlException)
                {
                    // XML file has become corrupt. Start from scratch
                    AppLogger.Instance.LogToGui(Resources.XMLActionsCorrupt, true);
                    m_Xdoc.RemoveAll();
                    PrepareActionsXml(m_Xdoc);
                }

                XmlNode Node;

                Node = m_Xdoc.CreateComment(string.Format(" Special Actions Configuration Data. {0} ", DateTime.Now));
                foreach (XmlNode node in m_Xdoc.SelectNodes("//comment()"))
                    node.ParentNode.ReplaceChild(Node, node);

                Node = m_Xdoc.SelectSingleNode("Actions");
                var el = m_Xdoc.CreateElement("Action");
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
                            var exts = extras.Split('\n');
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
                    var oldxmlprocess = m_Xdoc.SelectSingleNode("/Actions/Action[@Name=\"" + name + "\"]");
                    Node.ReplaceChild(el, oldxmlprocess);
                }
                else
                {
                    Node.AppendChild(el);
                }

                m_Xdoc.AppendChild(Node);
                try
                {
                    m_Xdoc.Save(ActionsPath);
                }
                catch
                {
                    saved = false;
                }

                LoadActions();
                return saved;
            }

            [ConfigurationSystemComponent]
            public void RemoveAction(string name)
            {
                m_Xdoc.Load(ActionsPath);
                var Node = m_Xdoc.SelectSingleNode("Actions");
                var Item = m_Xdoc.SelectSingleNode("/Actions/Action[@Name=\"" + name + "\"]");
                if (Item != null)
                    Node.RemoveChild(Item);

                m_Xdoc.AppendChild(Node);
                m_Xdoc.Save(ActionsPath);
                LoadActions();
            }

            [ConfigurationSystemComponent]
            public bool LoadActions()
            {
                var saved = true;
                if (!File.Exists(Path.Combine(RuntimeAppDataPath, Constants.ActionsFileName)))
                {
                    SaveAction("Disconnect Controller", "PS/Options", 5, "0", false);
                    saved = false;
                }

                try
                {
                    Actions.Clear();
                    var doc = new XmlDocument();
                    doc.Load(Path.Combine(RuntimeAppDataPath, Constants.ActionsFileName));
                    var actionslist = doc.SelectNodes("Actions/Action");
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
                                Actions.Add(
                                    new SpecialAction(name, controls, type, details, 0, extras2 + '\n' + extras));
                            else
                                Actions.Add(new SpecialAction(name, controls, type, details));
                        }
                        else if (type == "DisconnectBT")
                        {
                            double doub;
                            if (double.TryParse(details, NumberStyles.Float, ConfigFileDecimalCulture, out doub))
                                Actions.Add(new SpecialAction(name, controls, type, "", doub));
                            else
                                Actions.Add(new SpecialAction(name, controls, type, ""));
                        }
                        else if (type == "BatteryCheck")
                        {
                            double doub;
                            if (double.TryParse(details.Split('|')[0], NumberStyles.Float, ConfigFileDecimalCulture,
                                out doub))
                                Actions.Add(new SpecialAction(name, controls, type, details, doub));
                            else if (double.TryParse(details.Split(',')[0], NumberStyles.Float,
                                ConfigFileDecimalCulture, out doub))
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
                                if (double.TryParse(x.ChildNodes[4].InnerText, NumberStyles.Float,
                                    ConfigFileDecimalCulture, out doub))
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
                            if (double.TryParse(details, NumberStyles.Float, ConfigFileDecimalCulture, out doub))
                                Actions.Add(new SpecialAction(name, controls, type, "", doub));
                            else
                                Actions.Add(new SpecialAction(name, controls, type, ""));
                        }
                    }
                }
                catch
                {
                    saved = false;
                }

                return saved;
            }

            [ConfigurationSystemComponent]
            public bool LoadLinkedProfiles()
            {
                var loaded = true;
                if (File.Exists(LinkedProfilesPath))
                {
                    try
                    {
                        using var stream = File.OpenRead(LinkedProfilesPath);

                        var profiles = DS4WinWPF.DS4Control.Profiles.Legacy.LinkedProfiles.Deserialize(stream);

                        LinkedProfiles = profiles.Assignments.ToDictionary(
                            x => x.Key.ToString(),
                            x => x.Value.ToString()
                        );
                    }
                    catch
                    {
                        loaded = false;
                    }
                }
                else
                {
                    AppLogger.Instance.LogToGui("LinkedProfiles.xml can't be found.", false);
                    loaded = false;
                }

                return loaded;
            }

            [ConfigurationSystemComponent]
            public bool SaveLinkedProfiles()
            {
                var saved = true;

                try
                {
                    using var stream = File.Open(LinkedProfilesPath, FileMode.Create);

                    var profiles = new LinkedProfiles
                    {
                        //Assignments = LinkedProfiles.ToDictionary(x => PhysicalAddress.Parse(x.Key), x => Guid.Parse(x.Value))
                        Assignments = LinkedProfiles.ToDictionary(x => PhysicalAddress.Parse(x.Key), x => x.Value)
                    };

                    profiles.Serialize(stream);
                }
                catch (UnauthorizedAccessException)
                {
                    AppLogger.Instance.LogToGui("Unauthorized Access - Save failed to path: " + LinkedProfilesPath,
                        false);
                    saved = false;
                }

                return saved;
            }

            [ConfigurationSystemComponent]
            public bool LoadControllerConfigs(DS4Device device = null)
            {
                if (device != null)
                    return device.LoadOptionsStoreFrom(ControllerConfigsPath);

                for (var idx = 0; idx < ControlService.MAX_DS4_CONTROLLER_COUNT; idx++)
                    if (ControlService.CurrentInstance.DS4Controllers[idx] != null)
                        ControlService.CurrentInstance.DS4Controllers[idx].LoadOptionsStoreFrom(ControllerConfigsPath);

                return true;
            }

            [ConfigurationSystemComponent]
            public bool SaveControllerConfigs(DS4Device device = null)
            {
                if (device != null)
                    return device.PersistOptionsStore(ControllerConfigsPath);

                for (var idx = 0; idx < ControlService.MAX_DS4_CONTROLLER_COUNT; idx++)
                    if (ControlService.CurrentInstance.DS4Controllers[idx] != null)
                        ControlService.CurrentInstance.DS4Controllers[idx].PersistOptionsStore(ControllerConfigsPath);

                return true;
            }

            public void UpdateDs4ControllerSetting(int deviceNum, string buttonName, bool shift, object action,
                string exts,
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
                    RefreshActionAlias(dcs, shift);
                }
            }

            public SpecialAction GetProfileAction(int device, string name)
            {
                SpecialAction sA = null;
                profileActionDict[device].TryGetValue(name, out sA);
                return sA;
            }

            public bool ContainsLinkedProfile(string serial)
            {
                var tempSerial = serial.Replace(":", string.Empty);
                return LinkedProfiles.ContainsKey(tempSerial);
            }

            public string GetLinkedProfile(string serial)
            {
                var temp = string.Empty;
                var tempSerial = serial.Replace(":", string.Empty);
                if (LinkedProfiles.ContainsKey(tempSerial)) temp = LinkedProfiles[tempSerial];

                return temp;
            }

            public void ChangeLinkedProfile(string serial, string profile)
            {
                var tempSerial = serial.Replace(":", string.Empty);
                LinkedProfiles[tempSerial] = profile;
            }

            public void RemoveLinkedProfile(string serial)
            {
                var tempSerial = serial.Replace(":", string.Empty);
                if (LinkedProfiles.ContainsKey(tempSerial)) LinkedProfiles.Remove(tempSerial);
            }

            public int GetProfileActionIndexOf(int device, string name)
            {
                var index = -1;
                profileActionIndexDict[device].TryGetValue(name, out index);
                return index;
            }

            public void UpdateDs4ControllerExtra(int deviceNum, string buttonName, bool shift, string exts)
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
                        dcs.ShiftExtras = exts;
                    else
                        dcs.Extras = exts;
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
                var temp = (int)dc;
                if (temp > 0)
                {
                    var index = temp - 1;
                    var dcs = Ds4Settings[deviceNum][index];
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

                var temp = (int)dc;
                if (temp > 0)
                {
                    var index = temp - 1;
                    var dcs = Ds4Settings[deviceNum][index];
                    return dcs;
                }

                return null;
            }

            public DS4ControlSettings GetDs4ControllerSetting(int deviceNum, DS4Controls dc)
            {
                var temp = (int)dc;
                if (temp > 0)
                {
                    var index = temp - 1;
                    var dcs = Ds4Settings[deviceNum][index];
                    return dcs;
                }

                return null;
            }

            public bool HasCustomActions(int deviceNum)
            {
                var ds4settingsList = Ds4Settings[deviceNum];
                for (int i = 0, settingsLen = ds4settingsList.Count; i < settingsLen; i++)
                {
                    var dcs = ds4settingsList[i];
                    if (dcs.ControlActionType != DS4ControlSettings.ActionType.Default ||
                        dcs.ShiftActionType != DS4ControlSettings.ActionType.Default)
                        return true;
                }

                return false;
            }

            public bool HasCustomExtras(int deviceNum)
            {
                var ds4settingsList = Ds4Settings[deviceNum];
                for (int i = 0, settingsLen = ds4settingsList.Count; i < settingsLen; i++)
                {
                    var dcs = ds4settingsList[i];
                    if (dcs.Extras != null || dcs.ShiftExtras != null)
                        return true;
                }

                return false;
            }

            public void LoadBlankDs4Profile(int device, bool launchprogram, ControlService control,
                string propath = "", bool xinputChange = true, bool postLoad = true)
            {
                PrepareBlankingProfile(device, control, out var xinputPlug, out var xinputStatus, xinputChange);

                var lsInfo = LSModInfo[device];
                lsInfo.DeadZone = (int)(0.00 * 127);
                lsInfo.AntiDeadZone = 0;
                lsInfo.MaxZone = 100;

                var rsInfo = RSModInfo[device];
                rsInfo.DeadZone = (int)(0.00 * 127);
                rsInfo.AntiDeadZone = 0;
                rsInfo.MaxZone = 100;

                var l2Info = L2ModInfo[device];
                l2Info.deadZone = (byte)(0.00 * 255);

                var r2Info = R2ModInfo[device];
                r2Info.deadZone = (byte)(0.00 * 255);

                OutputDeviceType[device] = OutContType.DS4;

                // If a device exists, make sure to transfer relevant profile device
                // options to device instance
                if (postLoad && device < MAX_DS4_CONTROLLER_COUNT)
                    PostLoadSnippet(device, control, xinputStatus, xinputPlug);
            }

            public void LoadBlankProfile(int device, bool launchprogram, ControlService control,
                string propath = "", bool xinputChange = true, bool postLoad = true)
            {
                var xinputPlug = false;
                var xinputStatus = false;

                var oldContType = ActiveOutDevType[device];

                // Make sure to reset currently set profile values before parsing
                ResetProfile(device);
                ResetMouseProperties(device, control);

                // Only change xinput devices under certain conditions. Avoid
                // performing this upon program startup before loading devices.
                if (xinputChange)
                    CheckOldDeviceStatus(device, control, oldContType,
                        out xinputPlug, out xinputStatus);

                foreach (var dcs in Ds4Settings[device])
                    dcs.Reset();

                ProfileActions[device].Clear();
                ContainsCustomAction[device] = false;
                ContainsCustomExtras[device] = false;

                // If a device exists, make sure to transfer relevant profile device
                // options to device instance
                if (postLoad && device < MAX_DS4_CONTROLLER_COUNT)
                    PostLoadSnippet(device, control, xinputStatus, xinputPlug);
            }

            public void LoadDefaultGamepadGyroProfile(int device, bool launchprogram, ControlService control,
                string propath = "", bool xinputChange = true, bool postLoad = true)
            {
                var xinputPlug = false;
                var xinputStatus = false;

                var oldContType = ActiveOutDevType[device];

                // Make sure to reset currently set profile values before parsing
                ResetProfile(device);
                ResetMouseProperties(device, control);

                // Only change xinput devices under certain conditions. Avoid
                // performing this upon program startup before loading devices.
                if (xinputChange)
                    CheckOldDeviceStatus(device, control, oldContType,
                        out xinputPlug, out xinputStatus);

                foreach (var dcs in Ds4Settings[device])
                    dcs.Reset();

                ProfileActions[device].Clear();
                ContainsCustomAction[device] = false;
                ContainsCustomExtras[device] = false;

                GyroOutputMode[device] = GyroOutMode.MouseJoystick;
                SAMouseStickTriggers[device] = "4";
                SAMouseStickTriggerCond[device] = true;
                GyroMouseStickTriggerTurns[device] = false;
                GyroMouseStickInfo[device].UseSmoothing = true;
                GyroMouseStickInfo[device].Smoothing = DS4Windows.GyroMouseStickInfo.SmoothingMethod.OneEuro;

                // If a device exists, make sure to transfer relevant profile device
                // options to device instance
                if (postLoad && device < MAX_DS4_CONTROLLER_COUNT)
                    PostLoadSnippet(device, control, xinputStatus, xinputPlug);
            }

            public void LoadDefaultDS4GamepadGyroProfile(int device, bool launchprogram, ControlService control,
                string propath = "", bool xinputChange = true, bool postLoad = true)
            {
                PrepareBlankingProfile(device, control, out var xinputPlug, out var xinputStatus, xinputChange);

                var lsInfo = LSModInfo[device];
                lsInfo.DeadZone = (int)(0.00 * 127);
                lsInfo.AntiDeadZone = 0;
                lsInfo.MaxZone = 100;

                var rsInfo = RSModInfo[device];
                rsInfo.DeadZone = (int)(0.00 * 127);
                rsInfo.AntiDeadZone = 0;
                rsInfo.MaxZone = 100;

                var l2Info = L2ModInfo[device];
                l2Info.deadZone = (byte)(0.00 * 255);

                var r2Info = R2ModInfo[device];
                r2Info.deadZone = (byte)(0.00 * 255);

                GyroOutputMode[device] = GyroOutMode.MouseJoystick;
                SAMouseStickTriggers[device] = "4";
                SAMouseStickTriggerCond[device] = true;
                GyroMouseStickTriggerTurns[device] = false;
                GyroMouseStickInfo[device].UseSmoothing = true;
                GyroMouseStickInfo[device].Smoothing = DS4Windows.GyroMouseStickInfo.SmoothingMethod.OneEuro;

                OutputDeviceType[device] = OutContType.DS4;

                // If a device exists, make sure to transfer relevant profile device
                // options to device instance
                if (postLoad && device < MAX_DS4_CONTROLLER_COUNT)
                    PostLoadSnippet(device, control, xinputStatus, xinputPlug);
            }

            public void LoadDefaultMixedGyroMouseProfile(int device, bool launchprogram, ControlService control,
                string propath = "", bool xinputChange = true, bool postLoad = true)
            {
                var xinputPlug = false;
                var xinputStatus = false;

                var oldContType = ActiveOutDevType[device];

                // Make sure to reset currently set profile values before parsing
                ResetProfile(device);
                ResetMouseProperties(device, control);

                // Only change xinput devices under certain conditions. Avoid
                // performing this upon program startup before loading devices.
                if (xinputChange)
                    CheckOldDeviceStatus(device, control, oldContType,
                        out xinputPlug, out xinputStatus);

                foreach (var dcs in Ds4Settings[device])
                    dcs.Reset();

                ProfileActions[device].Clear();
                ContainsCustomAction[device] = false;
                ContainsCustomExtras[device] = false;

                GyroOutputMode[device] = GyroOutMode.Mouse;
                SATriggers[device] = "4";
                SATriggerCondition[device] = true;
                GyroTriggerTurns[device] = false;
                GyroMouseInfo[device].enableSmoothing = true;
                GyroMouseInfo[device].smoothingMethod = DS4Windows.GyroMouseInfo.SmoothingMethod.OneEuro;

                var rsInfo = RSModInfo[device];
                rsInfo.DeadZone = (int)(0.10 * 127);
                rsInfo.AntiDeadZone = 0;
                rsInfo.MaxZone = 90;

                // If a device exists, make sure to transfer relevant profile device
                // options to device instance
                if (postLoad && device < MAX_DS4_CONTROLLER_COUNT)
                    PostLoadSnippet(device, control, xinputStatus, xinputPlug);
            }

            public void LoadDefaultDs4MixedGyroMouseProfile(int device, bool launchprogram, ControlService control,
                string propath = "", bool xinputChange = true, bool postLoad = true)
            {
                PrepareBlankingProfile(device, control, out var xinputPlug, out var xinputStatus, xinputChange);

                var lsInfo = LSModInfo[device];
                lsInfo.DeadZone = (int)(0.00 * 127);
                lsInfo.AntiDeadZone = 0;
                lsInfo.MaxZone = 100;

                var rsInfo = RSModInfo[device];
                rsInfo.DeadZone = (int)(0.10 * 127);
                rsInfo.AntiDeadZone = 0;
                rsInfo.MaxZone = 100;

                var l2Info = L2ModInfo[device];
                l2Info.deadZone = (byte)(0.00 * 255);

                var r2Info = R2ModInfo[device];
                r2Info.deadZone = (byte)(0.00 * 255);

                GyroOutputMode[device] = GyroOutMode.Mouse;
                SATriggers[device] = "4";
                SATriggerCondition[device] = true;
                GyroTriggerTurns[device] = false;
                GyroMouseInfo[device].enableSmoothing = true;
                GyroMouseInfo[device].smoothingMethod = DS4Windows.GyroMouseInfo.SmoothingMethod.OneEuro;

                OutputDeviceType[device] = OutContType.DS4;

                // If a device exists, make sure to transfer relevant profile device
                // options to device instance
                if (postLoad && device < MAX_DS4_CONTROLLER_COUNT)
                    PostLoadSnippet(device, control, xinputStatus, xinputPlug);
            }

            public void LoadDefaultDS4MixedControlsProfile(int device, bool launchprogram, ControlService control,
                string propath = "", bool xinputChange = true, bool postLoad = true)
            {
                PrepareBlankingProfile(device, control, out var xinputPlug, out var xinputStatus, xinputChange);

                var setting = GetDs4ControllerSetting(device, DS4Controls.RYNeg);
                setting.UpdateSettings(false, X360Controls.MouseUp, "", DS4KeyType.None);
                setting = GetDs4ControllerSetting(device, DS4Controls.RYPos);
                setting.UpdateSettings(false, X360Controls.MouseDown, "", DS4KeyType.None);
                setting = GetDs4ControllerSetting(device, DS4Controls.RXNeg);
                setting.UpdateSettings(false, X360Controls.MouseLeft, "", DS4KeyType.None);
                setting = GetDs4ControllerSetting(device, DS4Controls.RXPos);
                setting.UpdateSettings(false, X360Controls.MouseRight, "", DS4KeyType.None);

                var rsInfo = RSModInfo[device];
                rsInfo.DeadZone = (int)(0.035 * 127);
                rsInfo.AntiDeadZone = 0;
                rsInfo.MaxZone = 90;

                OutputDeviceType[device] = OutContType.DS4;

                // If a device exists, make sure to transfer relevant profile device
                // options to device instance
                if (postLoad && device < MAX_DS4_CONTROLLER_COUNT)
                    PostLoadSnippet(device, control, xinputStatus, xinputPlug);
            }

            public void LoadDefaultMixedControlsProfile(int device, bool launchprogram, ControlService control,
                string propath = "", bool xinputChange = true, bool postLoad = true)
            {
                var xinputPlug = false;
                var xinputStatus = false;

                var oldContType = ActiveOutDevType[device];

                // Make sure to reset currently set profile values before parsing
                ResetProfile(device);
                ResetMouseProperties(device, control);

                // Only change xinput devices under certain conditions. Avoid
                // performing this upon program startup before loading devices.
                if (xinputChange)
                    CheckOldDeviceStatus(device, control, oldContType,
                        out xinputPlug, out xinputStatus);

                foreach (var dcs in Ds4Settings[device])
                    dcs.Reset();

                ProfileActions[device].Clear();
                ContainsCustomAction[device] = false;
                ContainsCustomExtras[device] = false;

                var setting = GetDs4ControllerSetting(device, DS4Controls.RYNeg);
                setting.UpdateSettings(false, X360Controls.MouseUp, "", DS4KeyType.None);
                setting = GetDs4ControllerSetting(device, DS4Controls.RYPos);
                setting.UpdateSettings(false, X360Controls.MouseDown, "", DS4KeyType.None);
                setting = GetDs4ControllerSetting(device, DS4Controls.RXNeg);
                setting.UpdateSettings(false, X360Controls.MouseLeft, "", DS4KeyType.None);
                setting = GetDs4ControllerSetting(device, DS4Controls.RXPos);
                setting.UpdateSettings(false, X360Controls.MouseRight, "", DS4KeyType.None);

                var rsInfo = RSModInfo[device];
                rsInfo.DeadZone = (int)(0.035 * 127);
                rsInfo.AntiDeadZone = 0;
                rsInfo.MaxZone = 90;

                // If a device exists, make sure to transfer relevant profile device
                // options to device instance
                if (postLoad && device < MAX_DS4_CONTROLLER_COUNT)
                    PostLoadSnippet(device, control, xinputStatus, xinputPlug);
            }

            public void LoadDefaultKBMProfile(int device, bool launchprogram, ControlService control,
                string propath = "", bool xinputChange = true, bool postLoad = true)
            {
                var xinputPlug = false;
                var xinputStatus = false;

                var oldContType = ActiveOutDevType[device];

                // Make sure to reset currently set profile values before parsing
                ResetProfile(device);
                ResetMouseProperties(device, control);

                // Only change xinput devices under certain conditions. Avoid
                // performing this upon program startup before loading devices.
                if (xinputChange)
                    CheckOldDeviceStatus(device, control, oldContType,
                        out xinputPlug, out xinputStatus);

                foreach (var dcs in Ds4Settings[device])
                    dcs.Reset();

                ProfileActions[device].Clear();
                ContainsCustomAction[device] = false;
                ContainsCustomExtras[device] = false;

                var lsInfo = LSModInfo[device];
                lsInfo.AntiDeadZone = 0;

                var rsInfo = RSModInfo[device];
                rsInfo.DeadZone = (int)(0.035 * 127);
                rsInfo.AntiDeadZone = 0;
                rsInfo.MaxZone = 90;

                var l2Info = L2ModInfo[device];
                l2Info.deadZone = (byte)(0.20 * 255);

                var r2Info = R2ModInfo[device];
                r2Info.deadZone = (byte)(0.20 * 255);

                // Flag to unplug virtual controller
                DirectInputOnly[device] = true;

                var setting = GetDs4ControllerSetting(device, DS4Controls.LYNeg);
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
                if (postLoad && device < MAX_DS4_CONTROLLER_COUNT)
                    PostLoadSnippet(device, control, xinputStatus, xinputPlug);
            }

            public void LoadDefaultKBMGyroMouseProfile(int device, bool launchprogram, ControlService control,
                string propath = "", bool xinputChange = true, bool postLoad = true)
            {
                var xinputPlug = false;
                var xinputStatus = false;

                var oldContType = ActiveOutDevType[device];

                // Make sure to reset currently set profile values before parsing
                ResetProfile(device);
                ResetMouseProperties(device, control);

                // Only change xinput devices under certain conditions. Avoid
                // performing this upon program startup before loading devices.
                if (xinputChange)
                    CheckOldDeviceStatus(device, control, oldContType,
                        out xinputPlug, out xinputStatus);

                foreach (var dcs in Ds4Settings[device])
                    dcs.Reset();

                ProfileActions[device].Clear();
                ContainsCustomAction[device] = false;
                ContainsCustomExtras[device] = false;

                var lsInfo = LSModInfo[device];
                lsInfo.AntiDeadZone = 0;

                var rsInfo = RSModInfo[device];
                rsInfo.DeadZone = (int)(0.105 * 127);
                rsInfo.AntiDeadZone = 0;
                rsInfo.MaxZone = 90;

                var l2Info = L2ModInfo[device];
                l2Info.deadZone = (byte)(0.20 * 255);

                var r2Info = R2ModInfo[device];
                r2Info.deadZone = (byte)(0.20 * 255);

                GyroOutputMode[device] = GyroOutMode.Mouse;
                SATriggers[device] = "4";
                SATriggerCondition[device] = true;
                GyroTriggerTurns[device] = false;
                GyroMouseInfo[device].enableSmoothing = true;
                GyroMouseInfo[device].smoothingMethod = DS4Windows.GyroMouseInfo.SmoothingMethod.OneEuro;

                // Flag to unplug virtual controller
                DirectInputOnly[device] = true;

                var setting = GetDs4ControllerSetting(device, DS4Controls.LYNeg);
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
                if (postLoad && device < MAX_DS4_CONTROLLER_COUNT)
                    PostLoadSnippet(device, control, xinputStatus, xinputPlug);
            }

            public string StickOutputCurveString(int id)
            {
                var result = "linear";
                switch (id)
                {
                    case 0: break;
                    case 1:
                        result = "enhanced-precision";
                        break;
                    case 2:
                        result = "quadratic";
                        break;
                    case 3:
                        result = "cubic";
                        break;
                    case 4:
                        result = "easeout-quad";
                        break;
                    case 5:
                        result = "easeout-cubic";
                        break;
                    case 6:
                        result = "custom";
                        break;
                }

                return result;
            }

            public string AxisOutputCurveString(int id)
            {
                return StickOutputCurveString(id);
            }

            public string SaTriggerCondString(bool value)
            {
                var result = value ? "and" : "or";
                return result;
            }

            public int StickOutputCurveId(string name)
            {
                var id = 0;
                switch (name)
                {
                    case "linear":
                        id = 0;
                        break;
                    case "enhanced-precision":
                        id = 1;
                        break;
                    case "quadratic":
                        id = 2;
                        break;
                    case "cubic":
                        id = 3;
                        break;
                    case "easeout-quad":
                        id = 4;
                        break;
                    case "easeout-cubic":
                        id = 5;
                        break;
                    case "custom":
                        id = 6;
                        break;
                }

                return id;
            }

            public bool SaTriggerCondValue(string text)
            {
                var result = true;
                switch (text)
                {
                    case "and":
                        result = true;
                        break;
                    case "or":
                        result = false;
                        break;
                    default:
                        result = true;
                        break;
                }

                return result;
            }

            private void SetNestedProperty(string compoundProperty, object target, object value)
            {
                var bits = compoundProperty.Split('.');
                for (var i = 0; i < bits.Length - 1; i++)
                {
                    var propertyToGet = target.GetType().GetProperty(bits[i]);
                    target = propertyToGet.GetValue(target, null);
                }

                var propertyToSet = target.GetType().GetProperty(bits.Last());
                propertyToSet.SetValue(target, value, null);
            }

            private void SetOutBezierCurveObjArrayItem(IList<BezierCurve> bezierCurveArray, int device,
                int curveOptionValue, BezierCurve.AxisType axisType)
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
                    case 6:
                        bezierCurveArray[device].InitBezierCurve(bezierCurveArray[device].CustomDefinition, axisType);
                        break; // Custom output curve
                }
            }

            private void SetupDefaultColors()
            {
                LightbarSettingInfo[0].Ds4WinSettings.Led = new DS4Color(Color.Blue);
                LightbarSettingInfo[1].Ds4WinSettings.Led = new DS4Color(Color.Red);
                LightbarSettingInfo[2].Ds4WinSettings.Led = new DS4Color(Color.Green);
                LightbarSettingInfo[3].Ds4WinSettings.Led = new DS4Color(Color.Pink);
                LightbarSettingInfo[4].Ds4WinSettings.Led = new DS4Color(Color.Blue);
                LightbarSettingInfo[5].Ds4WinSettings.Led = new DS4Color(Color.Red);
                LightbarSettingInfo[6].Ds4WinSettings.Led = new DS4Color(Color.Green);
                LightbarSettingInfo[7].Ds4WinSettings.Led = new DS4Color(Color.Pink);
                LightbarSettingInfo[8].Ds4WinSettings.Led = new DS4Color(Color.White);
            }

            private int AxisOutputCurveId(string name)
            {
                return StickOutputCurveId(name);
            }

            private string OutContDeviceString(OutContType id)
            {
                var result = "X360";
                switch (id)
                {
                    case OutContType.None:
                    case OutContType.X360:
                        result = "X360";
                        break;
                    case OutContType.DS4:
                        result = "DS4";
                        break;
                }

                return result;
            }

            private OutContType OutContDeviceId(string name)
            {
                var id = OutContType.X360;
                switch (name)
                {
                    case "None":
                    case "X360":
                        id = OutContType.X360;
                        break;
                    case "DS4":
                        id = OutContType.DS4;
                        break;
                }

                return id;
            }

            private void PortOldGyroSettings(int device)
            {
                if (GyroOutputMode[device] == GyroOutMode.None)
                    GyroOutputMode[device] = GyroOutMode.Controls;
            }

            private string GetGyroOutModeString(GyroOutMode mode)
            {
                var result = "None";
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
                }

                return result;
            }

            private GyroOutMode GetGyroOutModeType(string modeString)
            {
                var result = GyroOutMode.None;
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
                }

                return result;
            }

            private string GetLightbarModeString(LightbarMode mode)
            {
                var result = "DS4Win";
                switch (mode)
                {
                    case LightbarMode.DS4Win:
                        result = "DS4Win";
                        break;
                    case LightbarMode.Passthru:
                        result = "Passthru";
                        break;
                }

                return result;
            }

            private LightbarMode GetLightbarModeType(string modeString)
            {
                var result = LightbarMode.DS4Win;
                switch (modeString)
                {
                    case "DS4Win":
                        result = LightbarMode.DS4Win;
                        break;
                    case "Passthru":
                        result = LightbarMode.Passthru;
                        break;
                }

                return result;
            }

            private void CreateAction()
            {
                var m_Xdoc = new XmlDocument();
                PrepareActionsXml(m_Xdoc);
                m_Xdoc.Save(ActionsPath);
            }

            private void PrepareActionsXml(XmlDocument xmlDoc)
            {
                XmlNode Node;

                Node = xmlDoc.CreateXmlDeclaration("1.0", "utf-8", string.Empty);
                xmlDoc.AppendChild(Node);

                Node = xmlDoc.CreateComment(string.Format(" Special Actions Configuration Data. {0} ", DateTime.Now));
                xmlDoc.AppendChild(Node);

                Node = xmlDoc.CreateWhitespace("\r\n");
                xmlDoc.AppendChild(Node);

                Node = xmlDoc.CreateNode(XmlNodeType.Element, "Actions", "");
                xmlDoc.AppendChild(Node);
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

            private void ResetMouseProperties(int device, ControlService control)
            {
                if (device < MAX_DS4_CONTROLLER_COUNT &&
                    control.touchPad[device] != null)
                    control.touchPad[device]?.ResetToggleGyroModes();
            }

            private void ResetProfile(int device)
            {
                ButtonMouseInfos[device].Reset();
                GyroControlsInfo[device].Reset();

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
                LSModInfo[device].DeadZone = RSModInfo[device].DeadZone = 10;
                LSModInfo[device].AntiDeadZone = RSModInfo[device].AntiDeadZone = 20;
                LSModInfo[device].MaxZone = RSModInfo[device].MaxZone = 100;
                LSModInfo[device].MaxOutput = RSModInfo[device].MaxOutput = 100.0;
                LSModInfo[device].Fuzz = RSModInfo[device].Fuzz = StickDeadZoneInfo.DEFAULT_FUZZ;

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
                TapSensitivity[device] = 0;
                DoubleTap[device] = false;
                ScrollSensitivity[device] = 0;
                TouchPadInvert[device] = 0;
                BluetoothPollRate[device] = 4;

                LSOutputSettings[device].ResetSettings();
                RSOutputSettings[device].ResetSettings();
                L2OutputSettings[device].ResetSettings();
                R2OutputSettings[device].ResetSettings();

                var lightbarSettings = LightbarSettingInfo[device];
                var lightInfo = lightbarSettings.Ds4WinSettings;
                lightbarSettings.Mode = LightbarMode.DS4Win;
                lightInfo.LowLed = new DS4Color(Color.Black);
                //m_LowLeds[device] = new DS4Color(Color.Black);

                var tempColor = Color.Blue;
                switch (device)
                {
                    case 0:
                        tempColor = Color.Blue;
                        break;
                    case 1:
                        tempColor = Color.Red;
                        break;
                    case 2:
                        tempColor = Color.Green;
                        break;
                    case 3:
                        tempColor = Color.Pink;
                        break;
                    case 4:
                        tempColor = Color.Blue;
                        break;
                    case 5:
                        tempColor = Color.Red;
                        break;
                    case 6:
                        tempColor = Color.Green;
                        break;
                    case 7:
                        tempColor = Color.Pink;
                        break;
                    case 8:
                        tempColor = Color.White;
                        break;
                    default:
                        tempColor = Color.Blue;
                        break;
                }

                lightInfo.Led = new DS4Color(tempColor);
                lightInfo.ChargingLed = new DS4Color(Color.Black);
                lightInfo.FlashLed = new DS4Color(Color.Black);
                lightInfo.FlashAt = 0;
                lightInfo.FlashType = 0;
                lightInfo.ChargingType = 0;
                lightInfo.Rainbow = 0;
                lightInfo.MaxRainbowSaturation = 1.0;
                lightInfo.LedAsBattery = false;

                LaunchProgram[device] = string.Empty;
                DirectInputOnly[device] = false;
                StartTouchpadOff[device] = false;
                TouchOutMode[device] = TouchpadOutMode.Mouse;
                SATriggers[device] = "-1";
                SATriggerCondition[device] = true;
                GyroOutputMode[device] = GyroOutMode.Controls;
                SAMouseStickTriggers[device] = "-1";
                SAMouseStickTriggerCond[device] = true;

                GyroMouseStickInfo[device].Reset();
                GyroSwipeInfo[device].Reset();

                GyroMouseStickToggle[device] = false;
                GyroMouseStickTriggerTurns[device] = true;
                SASteeringWheelEmulationAxis[device] = SASteeringWheelEmulationAxisType.None;
                SASteeringWheelEmulationRange[device] = 360;
                SAWheelFuzzValues[device] = 0;
                WheelSmoothInfo[device].Reset();
                TouchDisInvertTriggers[device] = new int[1] { -1 };
                GyroSensitivity[device] = 100;
                GyroSensVerticalScale[device] = 100;
                GyroInvert[device] = 0;
                GyroTriggerTurns[device] = true;
                GyroMouseInfo[device].Reset();

                GyroMouseHorizontalAxis[device] = 0;
                GyroMouseToggle[device] = false;
                SquStickInfo[device].LSMode = false;
                SquStickInfo[device].RSMode = false;
                SquStickInfo[device].LSRoundness = 5.0;
                SquStickInfo[device].RSRoundness = 5.0;
                LSAntiSnapbackInfo[device].Timeout = StickAntiSnapbackInfo.DEFAULT_TIMEOUT;
                LSAntiSnapbackInfo[device].Delta = StickAntiSnapbackInfo.DEFAULT_DELTA;
                LSAntiSnapbackInfo[device].Enabled = StickAntiSnapbackInfo.DEFAULT_ENABLED;
                SetLsOutCurveMode(device, 0);
                SetRsOutCurveMode(device, 0);
                SetL2OutCurveMode(device, 0);
                SetR2OutCurveMode(device, 0);
                SetSXOutCurveMode(device, 0);
                SetSZOutCurveMode(device, 0);
                TrackballMode[device] = false;
                TrackballFriction[device] = 10.0;
                TouchPadAbsMouse[device].Reset();
                TouchPadRelMouse[device].Reset();
                OutputDeviceType[device] = OutContType.X360;
                Ds4Mapping = false;
            }

            private void PrepareBlankingProfile(int device, ControlService control, out bool xinputPlug,
                out bool xinputStatus, bool xinputChange = true)
            {
                xinputPlug = false;
                xinputStatus = false;

                var oldContType = ActiveOutDevType[device];

                // Make sure to reset currently set profile values before parsing
                ResetProfile(device);
                ResetMouseProperties(device, control);

                // Only change xinput devices under certain conditions. Avoid
                // performing this upon program startup before loading devices.
                if (xinputChange)
                    CheckOldDeviceStatus(device, control, oldContType,
                        out xinputPlug, out xinputStatus);

                foreach (var dcs in Ds4Settings[device])
                    dcs.Reset();

                ProfileActions[device].Clear();
                ContainsCustomAction[device] = false;
                ContainsCustomExtras[device] = false;
            }

            private void CheckOldDeviceStatus(int device, ControlService control,
                OutContType oldContType, out bool xinputPlug, out bool xinputStatus)
            {
                xinputPlug = false;
                xinputStatus = false;

                if (device < ControlService.CURRENT_DS4_CONTROLLER_LIMIT)
                {
                    var oldUseDInputOnly = UseDirectInputOnly[device];
                    var tempDevice = control.DS4Controllers[device];
                    var exists = tempDevice != null;
                    var synced = exists ? tempDevice.isSynced() : false;
                    var isAlive = exists ? tempDevice.IsAlive() : false;
                    if (DirectInputOnly[device] != oldUseDInputOnly)
                    {
                        if (DirectInputOnly[device])
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
                    else if (!DirectInputOnly[device] &&
                             oldContType != OutputDeviceType[device])
                    {
                        xinputPlug = true;
                        xinputStatus = true;
                    }
                }
            }

            private void PostLoadSnippet(int device, ControlService control, bool xinputStatus, bool xinputPlug)
            {
                var tempDev = control.DS4Controllers[device];
                if (tempDev != null && tempDev.isSynced())
                    tempDev.queueEvent(() =>
                    {
                        //tempDev.setIdleTimeout(idleDisconnectTimeout[device]);
                        //tempDev.setBTPollRate(btPollRate[device]);
                        if (xinputStatus && tempDev.PrimaryDevice)
                        {
                            if (xinputPlug)
                            {
                                var tempOutDev = control.outputDevices[device];
                                if (tempOutDev != null)
                                {
                                    tempOutDev = null;
                                    //Global.ActiveOutDevType[device] = OutContType.None;
                                    control.UnplugOutDev(device, tempDev);
                                }

                                var tempContType = OutputDeviceType[device];
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

                //ControlService.CurrentInstance.touchPad[device]?.ResetTrackAccel(trackballFriction[device]);
            }
        }
    }
}