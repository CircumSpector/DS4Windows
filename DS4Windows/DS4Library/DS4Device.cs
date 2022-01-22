using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Threading;
using DS4Windows.DS4Library;
using DS4Windows.DS4Library.CoreAudio;
using DS4Windows.InputDevices;
using DS4Windows.Shared.Common.Types;
using DS4Windows.Shared.Common.Util;
using DS4Windows.Shared.Configuration.Application.Schema;
using DS4Windows.Shared.Core.HID;
using DS4Windows.Shared.Core.Util;
using DS4WinWPF.DS4Control.Logging;
using DS4WinWPF.DS4Control.Profiles.Schema;
using PropertyChanged;
using ThreadState = System.Threading.ThreadState;

namespace DS4Windows
{
    /// <summary>
    ///     Represents a Sony DualShock 4 compatible device.
    /// </summary>
    [AddINotifyPropertyChangedInterface]
    public class DS4Device : IDisposable
    {
        public delegate void ReportHandler<TEventArgs>(DS4Device sender, TEventArgs args);

        public enum BTOutputReportMethod : uint
        {
            WriteFile,
            HidD_SetOutputReport
        }

        public enum ExclusiveStatus : byte
        {
            Shared = 0,
            Exclusive = 1,
            HidGuardAffected = 2,
            HidHideAffected = 3
        }

        public enum WheelCalibrationPoint
        {
            None = 0,
            Center = 1,
            Right90 = 2,
            Left90 = 4,
            All = Center | Right90 | Left90
        }

        //internal const int BT_OUTPUT_REPORT_LENGTH = 78;
        protected const int BT_OUTPUT_REPORT_LENGTH = 334;
        private const int BT_OUTPUT_REPORT_0x15_LENGTH = BT_OUTPUT_REPORT_LENGTH;
        private const int BT_OUTPUT_REPORT_0x11_LENGTH = 78;
        internal const int BT_INPUT_REPORT_LENGTH = 547;
        internal const int BT_OUTPUT_CHANGE_LENGTH = 13;

        internal const int USB_OUTPUT_CHANGE_LENGTH = 11;

        // Use large value for worst case scenario
        internal const int READ_STREAM_TIMEOUT = 3000;

        /// <summary>
        ///     Isolated BT report can have latency as high as 15 ms due to hardware.
        /// </summary>
        internal const int WARN_INTERVAL_BT = 40;

        internal const int WARN_INTERVAL_USB = 20;

        /// <summary>
        ///     Maximum values for battery level when no USB cable is connected and when a USB cable is connected
        /// </summary>
        internal const int BATTERY_MAX = 8;

        internal const int BATTERY_MAX_USB = 11;
        public const string BLANK_SERIAL = "00:00:00:00:00:00";
        public const byte SERIAL_FEATURE_ID = 18;
        private const string SONYWA_AUDIO_SEARCHNAME = "DUALSHOCK®4 USB Wireless Adaptor";
        private const string RAIJU_TE_AUDIO_SEARCHNAME = "Razer Raiju Tournament Edition Wired";

        private const byte DEFAULT_BT_REPORT_TYPE = 0x15;

        private const byte DEFAULT_OUTPUT_FEATURES = 0xF7;
        private const byte COPYCAT_OUTPUT_FEATURES = 0xF3; // Remove flash flag

        public const int DEFAULT_JOINT_SLOT_NUMBER = -1;

        protected const int DS4_FEATURE_REPORT_5_LEN = 41;
        protected const int DS4_FEATURE_REPORT_5_CRC32_POS = DS4_FEATURE_REPORT_5_LEN - 4;
        private const int OUTPUT_MIN_COUNT_BT = 3;


        protected const int BT_INPUT_REPORT_CRC32_POS = 74; //last 4 bytes of the 78-sized input report are crc32
        public const uint DefaultPolynomial = 0xedb88320u;
        private const int CRC32_NUM_ATTEMPTS = 10;

        private readonly byte[] outputBTCrc32Head = { 0xA2 };

        /// <summary>
        ///     Autostop timer to stop rumble motors if those are stuck in a rumble state
        /// </summary>
        private readonly Stopwatch rumbleAutostopTimer = new();

        protected readonly DS4SixAxis sixAxis;
        protected readonly Stopwatch standbySw = new();
        protected readonly DS4Touchpad touchpad;

        private bool abortInputThread;
        protected byte[] accel = new byte[6];
        protected DS4Audio audio;
        protected int battery;
        protected byte[] btInputReport;

        /// <summary>
        ///     Specify the poll rate interval used for the DS4 hardware when connected via Bluetooth
        /// </summary>
        protected int btPollRate;

        protected bool charging;

        protected DS4State currentState = new();
        private uint deltaTimeCurrent;
        protected byte deviceSlotMask = 0x00;

        protected int deviceSlotNumber = -1;

        protected string displayName;
        protected bool ds4InactiveFrame = true;
        protected Thread ds4Input, ds4Output;
        public string error;

        protected Queue<Action> eventQueue = new();
        protected object eventQueueLock = new();
        protected ExclusiveStatus exclusiveStatus = ExclusiveStatus.Shared;
        protected bool exitInputThread;
        protected object exitLocker = new();

        protected bool exitOutputThread;

        /// <summary>
        ///     Feature set of gamepad (some non-official DS4 gamepads require a bit different logic than a genuine Sony DS4).
        ///     0=Default DS4 gamepad feature set.
        /// </summary>
        protected VidPidFeatureSet featureSet;

        public DateTime firstActive = DateTime.UtcNow;
        public bool firstReport = true;
        protected byte[] gyro = new byte[6];

        protected GyroMouseSens gyroMouseSensSettings;
        protected uint HamSeed = 2351727372;
        protected bool hasInputEvts;
        protected HidDeviceV3 hDevice;
        protected bool idleInput = true;

        /// <summary>
        ///     behavior only active when > 0
        /// </summary>
        protected int idleTimeout;

        protected byte[] inputReport;
        protected IntPtr InputReportBuffer;

        /// <summary>
        ///     Num of consecutive input report errors (fex if BT device fails 5 times in crc32 and 0x11 data type check then
        ///     switch over to handle incoming BT packets as those were usb PC-friendly packets. Some fake DS4 gamepads needs this)
        /// </summary>
        protected int inputReportErrorCount;

        protected bool isDisconnecting;

        protected bool isRemoved;

        protected bool isRemoving;
        protected int jointDeviceSlotNumber = DEFAULT_JOINT_SLOT_NUMBER;
        protected DS4State jointPreviousState = new();

        protected DS4State jointState = new();
        private byte knownGoodBTOutputReportType = DEFAULT_BT_REPORT_TYPE;
        public DateTime lastActive = DateTime.UtcNow;

        protected long lastTimeElapsed;

        public double lastTimeElapsedDouble;

        public double Latency;

        protected DS4Audio micAudio;

        //public EventHandler<EventArgs> MotionEvent = null;
        public ReportHandler<EventArgs> MotionEvent = null;
        private DS4ControllerOptions nativeOptionsStore;
        public bool oldCharging;
        private byte outputFeaturesByte = DEFAULT_OUTPUT_FEATURES;

        protected bool outputMapGyro = true;

        private byte outputPendCount;
        protected byte[] outReportBuffer, outputReport;

        protected bool performStateMerge;

        protected bool primaryDevice = true;

        private byte priorInputReport30 = 0xff;
        protected DS4State pState = new();

        protected ManualResetEventSlim readWaitEv = new();
        protected bool readyQuickChargeDisconnect;

        public object removeLocker = new();


        private int rumbleAutostopTime;
        protected bool runCalib;

        protected bool synced;

        protected DS4HapticState testRumble = new();

        protected Thread timeoutCheckThread;
        protected bool timeoutEvent;
        protected bool timeoutExecuted;

        private bool timeStampInit;
        private uint timeStampPrevious;

        protected bool useRumble = true;
        protected DateTime utcNow = DateTime.UtcNow;
        protected int warnInterval = WARN_INTERVAL_USB;
        public Point wheel90DegPointLeft;
        public Point wheel90DegPointRight;
        public WheelCalibrationPoint wheelCalibratedAxisBitmask;

        public Point wheelCenterPoint;
        public Point wheelCircleCenterPointLeft;
        public Point wheelCircleCenterPointRight;
        public int wheelFullTurnCount = 0;
        public int wheelPrevFullAngle = 0;

        public int wheelPrevPhysicalAngle = 0;

        public DateTime wheelPrevRecalibrateTime;

        protected int wheelRecalibrateActiveState;

        public DS4Device(HidDeviceV3 hidDevice, string disName,
            VidPidFeatureSet featureSet = VidPidFeatureSet.DefaultDS4)
        {
            hDevice = hidDevice;
            displayName = disName;
            this.featureSet = featureSet;

            ConnectionType = HidConnectionType(hDevice);
            exclusiveStatus = ExclusiveStatus.Shared;
            if (hidDevice.IsExclusive) exclusiveStatus = ExclusiveStatus.Exclusive;

            if (FeatureSet != VidPidFeatureSet.DefaultDS4)
                AppLogger.Instance.LogToGui(
                    $"The gamepad {displayName} ({ConnectionType}) uses custom feature set ({FeatureSet:F})",
                    false);

            MacAddress = hDevice.ReadSerial(SerialReportID);
            runCalib = (this.featureSet & VidPidFeatureSet.NoGyroCalib) == 0;

            touchpad = new DS4Touchpad();
            sixAxis = new DS4SixAxis();
        }

        public ConnectionType ConnectionType { get; protected set; }

        public PhysicalAddress MacAddress { get; protected set; }

        protected DS4HapticState CurrentHaptics { get; set; } = new();

        public ControllerOptionsStore OptionsStore { get; protected set; }

        public bool ReadyQuickChargeDisconnect
        {
            get => readyQuickChargeDisconnect;
            set => readyQuickChargeDisconnect = value;
        }

        public int WheelRecalibrateActiveState
        {
            get => wheelRecalibrateActiveState;
            set => wheelRecalibrateActiveState = value;
        }

        public bool ExitOutputThread => exitOutputThread;

        public HidDeviceV3 HidDevice => hDevice;
        public bool IsHidExclusive => HidDevice.IsExclusive;

        public bool IsExclusive => exclusiveStatus > ExclusiveStatus.Shared;

        public ExclusiveStatus CurrentExclusiveStatus
        {
            get => exclusiveStatus;
            set => exclusiveStatus = value;
        }

        public bool IsDisconnecting
        {
            get => isDisconnecting;
            protected set => isDisconnecting = value;
        }

        public bool IsRemoving
        {
            get => isRemoving;
            set => isRemoving = value;
        }

        public bool IsRemoved
        {
            get => isRemoved;
            set => isRemoved = value;
        }

        public int IdleTimeout
        {
            get => idleTimeout;
            set => idleTimeout = value;
        }

        public VidPidFeatureSet FeatureSet
        {
            get => featureSet;
            set => featureSet = value;
        }

        public bool UseRumble
        {
            get => useRumble;
            set => useRumble = value;
        }

        public int Battery => battery;

        public bool Charging => charging;

        public byte RightLightFastRumble
        {
            get => CurrentHaptics.RumbleState.RumbleMotorStrengthRightLightFast;
            set
            {
                if (CurrentHaptics.RumbleState.RumbleMotorStrengthRightLightFast != value)
                    CurrentHaptics.RumbleState.RumbleMotorStrengthRightLightFast = value;
            }
        }

        public byte LeftHeavySlowRumble
        {
            get => CurrentHaptics.RumbleState.RumbleMotorStrengthLeftHeavySlow;
            set
            {
                if (CurrentHaptics.RumbleState.RumbleMotorStrengthLeftHeavySlow != value)
                    CurrentHaptics.RumbleState.RumbleMotorStrengthLeftHeavySlow = value;
            }
        }

        public int RumbleAutostopTime
        {
            get => rumbleAutostopTime;
            set
            {
                // Value in milliseconds
                rumbleAutostopTime = value;

                // If autostop timer is disabled (value 0) then stop existing autostop timer otherwise restart it
                if (value <= 0)
                    rumbleAutostopTimer.Reset();
                else
                    rumbleAutostopTimer.Restart();
            }
        }

        public DS4Color LightBarColor
        {
            get => CurrentHaptics.LightbarState.LightBarColor;
            set
            {
                if (CurrentHaptics.LightbarState.LightBarColor.Red != value.Red ||
                    CurrentHaptics.LightbarState.LightBarColor.Green != value.Green ||
                    CurrentHaptics.LightbarState.LightBarColor.Blue != value.Blue)
                    CurrentHaptics.LightbarState.LightBarColor = value;
            }
        }

        public int BTPollRate
        {
            get => btPollRate;
            set
            {
                if (btPollRate != value && value >= 0 && value <= 16) btPollRate = value;
            }
        }

        public DS4Touchpad Touchpad => touchpad;
        public DS4SixAxis SixAxis => sixAxis;
        public string DisplayName => displayName;
        public ManualResetEventSlim ReadWaitEv => readWaitEv;

        public virtual byte SerialReportID => SERIAL_FEATURE_ID;
        public BTOutputReportMethod BTOutputMethod { get; set; }

        public InputDeviceType DeviceType { get; protected set; }

        public virtual GyroMouseSens GyroMouseSensSettings => gyroMouseSensSettings;

        public int DeviceSlotNumber
        {
            get => deviceSlotNumber;
            set
            {
                if (deviceSlotNumber == value) return;
                deviceSlotNumber = value;
                DeviceSlotNumberChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public DS4State JointState
        {
            get => jointState;
            set => jointState = value;
        }

        public DS4State JointPreviousState
        {
            get => jointPreviousState;
            set => jointPreviousState = value;
        }

        public bool PerformStateMerge
        {
            get => performStateMerge;
            set => performStateMerge = value;
        }

        public bool PrimaryDevice
        {
            get => primaryDevice;
            set => primaryDevice = value;
        }

        public virtual int JointDeviceSlotNumber
        {
            get => jointDeviceSlotNumber;
            set => jointDeviceSlotNumber = value;
        }

        public bool OutputMapGyro
        {
            get => outputMapGyro;
            set => outputMapGyro = value;
        }

        public bool Synced
        {
            get => synced;
            set
            {
                if (synced != value) synced = value;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     Persist <see cref="OptionsStore" /> to XML.
        /// </summary>
        /// <param name="path">Full path to XML file.</param>
        /// <returns>True on success, false otherwise.</returns>
        public bool PersistOptionsStore(string path)
        {
            ControllerConfigs config = null;

            try
            {
                using var read = File.OpenRead(path);

                config = ControllerConfigs.Deserialize(read);
            }
            catch (FileNotFoundException)
            {
                //
                // Nonexistent, create fresh
                // 
                config = new ControllerConfigs();
            }
            catch (InvalidOperationException)
            {
                //
                // Old format loaded, ignore and overwrite
                // 
                config = new ControllerConfigs();
            }

            config.Controllers[MacAddress] = OptionsStore;

            using var write = File.Open(path, FileMode.Create);

            config.Serialize(write);

            return true;
        }

        /// <summary>
        ///     Load <see cref="OptionsStore" /> from XML.
        /// </summary>
        /// <param name="path">Full path to XML file.</param>
        /// <returns>True on success, false otherwise.</returns>
        public bool LoadOptionsStoreFrom(string path)
        {
            try
            {
                using var stream = File.OpenRead(path);

                var config = ControllerConfigs.Deserialize(stream);

                var store = config.Controllers[MacAddress];

                switch (store)
                {
                    case DS4ControllerOptions options:
                        options.DeepCloneTo((DS4ControllerOptions)OptionsStore);
                        break;
                    case DualSenseControllerOptions options:
                        options.DeepCloneTo((DualSenseControllerOptions)OptionsStore);
                        break;
                    case SwitchProControllerOptions options:
                        options.DeepCloneTo((SwitchProControllerOptions)OptionsStore);
                        break;
                    case JoyConControllerOptions options:
                        options.DeepCloneTo((JoyConControllerOptions)OptionsStore);
                        break;
                }
            }
            catch (FileNotFoundException)
            {
                return false;
            }
            catch (InvalidOperationException)
            {
                //
                // XML format malformed, ignore
                // 
                return false;
            }
            catch (KeyNotFoundException)
            {
                //
                // No config for this device found, ignore
                // 
                return false;
            }

            return true;
        }

        public int getWarnInterval()
        {
            return warnInterval;
        }

        //public event EventHandler<EventArgs> Report = null;
        public virtual event ReportHandler<EventArgs> Report;
        public virtual event EventHandler<EventArgs> Removal;
        public event EventHandler<EventArgs> SyncChange;
        public event EventHandler<EventArgs> SerialChange;

        public bool isHidExclusive()
        {
            return HidDevice.IsExclusive;
        }

        public bool isExclusive()
        {
            return exclusiveStatus > ExclusiveStatus.Shared;
        }

        public bool IsDisconnectingStatus()
        {
            return isDisconnecting;
        }

        public event EventHandler MacAddressChanged;

        public ConnectionType GetConnectionType()
        {
            return ConnectionType;
        }

        public int getIdleTimeout()
        {
            return idleTimeout;
        }

        public void SetIdleTimeout(int value)
        {
            if (idleTimeout != value) idleTimeout = value;
        }

        public VidPidFeatureSet ModifyFeatureSetFlag(VidPidFeatureSet featureBitFlag, bool flagSet)
        {
            if (flagSet) featureSet |= featureBitFlag;
            else featureSet &= ~featureBitFlag;
            return featureSet;
        }

        public virtual event Action<DS4Device> BatteryChanged;

        public int GetBattery()
        {
            return battery;
        }

        public virtual event Action<DS4Device> ChargingChanged;

        public bool IsCharging()
        {
            return charging;
        }

        public long getLastTimeElapsed()
        {
            return lastTimeElapsed;
        }

        public double getLastTimeElapsedDouble()
        {
            return lastTimeElapsedDouble;
        }

        public byte getLeftHeavySlowRumble()
        {
            return CurrentHaptics.RumbleState.RumbleMotorStrengthLeftHeavySlow;
        }

        public byte getLightBarOnDuration()
        {
            return CurrentHaptics.LightbarState.LightBarFlashDurationOn;
        }

        public int getBTPollRate()
        {
            return btPollRate;
        }

        public void SetBtPollRate(int value)
        {
            if (btPollRate != value && value >= 0 && value <= 16) btPollRate = value;
        }

        public static ConnectionType HidConnectionType(HidDeviceV3 hidDevice)
        {
            var result = ConnectionType.Usb;
            if (hidDevice.Capabilities.InputReportByteLength == 64)
            {
                if (hidDevice.Capabilities.NumberFeatureDataIndices == 22) result = ConnectionType.SonyWirelessAdapter;
            }
            else
            {
                result = ConnectionType.Bluetooth;
            }

            return result;
        }

        public bool ShouldRunCalib()
        {
            return runCalib;
        }

        protected event EventHandler DeviceSlotNumberChanged;

        /// <summary>
        ///     Initialization actions after constructor call.
        /// </summary>
        public virtual void PostInit()
        {
            DeviceType = InputDeviceType.DualShock4;

            gyroMouseSensSettings = new GyroMouseSens();
            OptionsStore = nativeOptionsStore = new DS4ControllerOptions();
            SetupOptionsEvents();

            //
            // Connected via USB or proprietary wireless adapter which behaves equal to USB
            // 
            if (ConnectionType is ConnectionType.Usb or ConnectionType.SonyWirelessAdapter)
            {
                inputReport = new byte[64];
                InputReportBuffer = Marshal.AllocHGlobal(inputReport.Length);

                outputReport = new byte[hDevice.Capabilities.OutputReportByteLength];
                outReportBuffer = new byte[hDevice.Capabilities.OutputReportByteLength];

                if (ConnectionType == ConnectionType.Usb)
                {
                    warnInterval = WARN_INTERVAL_USB;
                    var tempAttr = hDevice.Attributes;
                    if (tempAttr.VendorId == 0x054C && tempAttr.ProductId == 0x09CC)
                    {
                        audio = new DS4Audio(searchDeviceInstance: hDevice.ParentPath);
                        micAudio = new DS4Audio(DataFlow.Capture,
                            hDevice.ParentPath);
                    }
                    else if (tempAttr.VendorId == DS4DeviceEnumerator.RAZER_VID &&
                             tempAttr.ProductId == 0x1007)
                    {
                        audio = new DS4Audio(searchDeviceInstance: hDevice.ParentPath);
                        micAudio = new DS4Audio(DataFlow.Capture,
                            hDevice.ParentPath);
                    }
                    else if (featureSet.HasFlag(VidPidFeatureSet.MonitorAudio))
                    {
                        audio = new DS4Audio(searchDeviceInstance: hDevice.ParentPath);
                        micAudio = new DS4Audio(DataFlow.Capture,
                            hDevice.ParentPath);
                    }

                    synced = true;
                }
                else
                {
                    warnInterval = WARN_INTERVAL_BT;
                    audio = new DS4Audio(searchDeviceInstance: hDevice.ParentPath);
                    micAudio = new DS4Audio(DataFlow.Capture,
                        hDevice.ParentPath);
                    runCalib = synced = IsValidSerial();
                }
            }
            else
            {
                btInputReport = new byte[BT_INPUT_REPORT_LENGTH];
                inputReport = new byte[BT_INPUT_REPORT_LENGTH - 2];
                InputReportBuffer = Marshal.AllocHGlobal(btInputReport.Length);

                // If OnlyOutputData0x05 feature is not set then use the default DS4 output buffer size. However, some Razer gamepads use 32 bytes output buffer and output data type 0x05 in BT mode (writeData fails if the code tries to write too many unnecessary bytes)
                if ((featureSet & VidPidFeatureSet.OnlyOutputData0x05) == 0)
                {
                    // Default DS4 logic while writing data to gamepad
                    outputReport = new byte[BT_OUTPUT_REPORT_LENGTH];
                    outReportBuffer = new byte[BT_OUTPUT_REPORT_LENGTH];
                }
                else
                {
                    // Use the gamepad specific output buffer size (but minimum of 15 bytes to avoid out-of-index errors in this app)
                    outputReport = new byte[hDevice.Capabilities.OutputReportByteLength <= 15
                        ? 15
                        : hDevice.Capabilities.OutputReportByteLength];
                    outReportBuffer = new byte[hDevice.Capabilities.OutputReportByteLength <= 15
                        ? 15
                        : hDevice.Capabilities.OutputReportByteLength];
                }

                warnInterval = WARN_INTERVAL_BT;
                synced = IsValidSerial();
            }

            if (runCalib)
                RefreshCalibration();

            if (ConnectionType == ConnectionType.Bluetooth &&
                !featureSet.HasFlag(VidPidFeatureSet.NoOutputData) &&
                !featureSet.HasFlag(VidPidFeatureSet.OnlyOutputData0x05))
                CheckOutputReportTypes();

            SendOutputReport(true, true,
                false); // initialize the output report (don't force disconnect the gamepad on initialization even if writeData fails because some fake DS4 gamepads don't support writeData over BT)
        }

        private void CheckOutputReportTypes()
        {
            // Use Tuple here for convenience
            var reportIds = new (byte Id, int Length)[]
            {
                (0x15, BT_OUTPUT_REPORT_0x15_LENGTH),
                (0x11, BT_OUTPUT_REPORT_0x11_LENGTH)
            };

            byte finalReport = 0x00;
            foreach (var element in reportIds)
            {
                var len = element.Length;
                var outputBuffer = new byte[element.Length];
                outputBuffer[0] = element.Id;
                //outputBuffer[1] = (byte)(0xC0 | 0x04);
                outputBuffer[2] = 0xA0;

                // Need to calculate and populate CRC-32 data so controller will accept the report
                var calcCrc32 = ~Crc32Algorithm.Compute(outputBTCrc32Head);
                calcCrc32 = ~Crc32Algorithm.CalculateBasicHash(ref calcCrc32, ref outputBuffer, 0, len - 4);
                outputBuffer[len - 4] = (byte)calcCrc32;
                outputBuffer[len - 3] = (byte)(calcCrc32 >> 8);
                outputBuffer[len - 2] = (byte)(calcCrc32 >> 16);
                outputBuffer[len - 1] = (byte)(calcCrc32 >> 24);

                if (WriteOutput(outputBuffer))
                {
                    finalReport = element.Id;
                    knownGoodBTOutputReportType = element.Id;
                    outputReport = new byte[len];
                    outReportBuffer = new byte[len];
                    break;
                }
            }

            if (finalReport == 0x00) ModifyFeatureSetFlag(VidPidFeatureSet.NoOutputData, true);
        }

        private void TimeoutTestThread()
        {
            while (!timeoutExecuted)
                if (timeoutEvent)
                {
                    timeoutExecuted = true;
                    SendOutputReport(true, true); // Kick Windows into noticing the disconnection.
                }
                else
                {
                    timeoutEvent = true;
                    Thread.Sleep(READ_STREAM_TIMEOUT);
                }
        }

        public virtual void RefreshCalibration()
        {
            var calibration = new byte[41];
            calibration[0] = ConnectionType == ConnectionType.Bluetooth ? (byte)0x05 : (byte)0x02;

            if (ConnectionType == ConnectionType.Bluetooth)
            {
                var found = false;
                for (var tries = 0; !found && tries < 5; tries++)
                {
                    hDevice.ReadFeatureData(calibration);
                    var recvCrc32 = calibration[DS4_FEATURE_REPORT_5_CRC32_POS] |
                                    (uint)(calibration[DS4_FEATURE_REPORT_5_CRC32_POS + 1] << 8) |
                                    (uint)(calibration[DS4_FEATURE_REPORT_5_CRC32_POS + 2] << 16) |
                                    (uint)(calibration[DS4_FEATURE_REPORT_5_CRC32_POS + 3] << 24);

                    var calcCrc32 = ~Crc32Algorithm.Compute(new byte[] { 0xA3 });
                    calcCrc32 = ~Crc32Algorithm.CalculateBasicHash(ref calcCrc32, ref calibration, 0,
                        DS4_FEATURE_REPORT_5_LEN - 4);
                    var validCrc = recvCrc32 == calcCrc32;
                    if (!validCrc && tries >= 5)
                    {
                        AppLogger.Instance.LogToGui("Gyro Calibration Failed", true);
                        continue;
                    }

                    if (validCrc) found = true;
                }

                sixAxis.SetCalibrationData(ref calibration, ConnectionType == ConnectionType.Usb);

                if (hDevice.Attributes.ProductId == 0x5C4 && hDevice.Attributes.VendorId == 0x054C &&
                    sixAxis.fixupInvertedGyroAxis())
                    AppLogger.Instance.LogToGui(
                        $"Automatically fixed inverted YAW gyro axis in DS4 v.1 BT gamepad ({MacAddress})",
                        false);
            }
            else
            {
                hDevice.ReadFeatureData(calibration);
                sixAxis.SetCalibrationData(ref calibration, ConnectionType == ConnectionType.Usb);
            }
        }

        public virtual void StartUpdate()
        {
            inputReportErrorCount = 0;

            if (ds4Input == null)
            {
                if (ConnectionType == ConnectionType.Bluetooth)
                {
                    if (BTOutputMethod == BTOutputReportMethod.HidD_SetOutputReport)
                    {
                        ds4Output = new Thread(PerformDs4Output);
                        ds4Output.Priority = ThreadPriority.Normal;
                        ds4Output.Name = "DS4 Output thread: " + MacAddress;
                        ds4Output.IsBackground = true;
                        ds4Output.Start();
                    }

                    timeoutCheckThread = new Thread(TimeoutTestThread);
                    timeoutCheckThread.Priority = ThreadPriority.BelowNormal;
                    timeoutCheckThread.Name = "DS4 Timeout thread: " + MacAddress;
                    timeoutCheckThread.IsBackground = true;
                    timeoutCheckThread.Start();
                }
                else
                {
                    ds4Output = new Thread(OutReportCopy);
                    ds4Output.Priority = ThreadPriority.Normal;
                    ds4Output.Name = "DS4 Arr Copy thread: " + MacAddress;
                    ds4Output.IsBackground = true;
                    ds4Output.Start();
                }

                ds4Input = new Thread(PerformDs4Input);
                ds4Input.Priority = ThreadPriority.AboveNormal;
                ds4Input.Name = "DS4 Input thread: " + MacAddress;
                ds4Input.IsBackground = true;
                //ds4Input.Start();
            }
            else
            {
                Debug.WriteLine("Thread already running for DS4: " + MacAddress);
            }
        }

        public virtual void StopUpdate()
        {
            if (ds4Input is { IsAlive: true } && !ds4Input.ThreadState.HasFlag(ThreadState.Stopped) &&
                !ds4Input.ThreadState.HasFlag(ThreadState.AbortRequested))
                try
                {
                    exitInputThread = true;
                    //ds4Input.Interrupt();
                    if (!abortInputThread)
                        ds4Input.Join();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }

            StopOutputUpdate();
        }

        protected virtual void StopOutputUpdate()
        {
            lock (exitLocker)
            {
                if (ds4Output != null &&
                    ds4Output.IsAlive && !ds4Output.ThreadState.HasFlag(ThreadState.Stopped) &&
                    !ds4Output.ThreadState.HasFlag(ThreadState.AbortRequested))
                    try
                    {
                        exitOutputThread = true;
                        ds4Output.Interrupt();
                        ds4Output.Join();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
            }
        }

        protected bool WriteOutput(byte[] outputBuffer)
        {
            if (ConnectionType == ConnectionType.Bluetooth)
            {
                //if ((this.featureSet & VidPidFeatureSet.OnlyOutputData0x05) == 0)
                //    return hDevice.WriteOutputReportViaControl(outputReport);

                if (BTOutputMethod == BTOutputReportMethod.WriteFile)
                    // Use Interrupt endpoint for almost BT DS4 connected devices now
                    return hDevice.WriteOutputReportViaInterrupt(outputBuffer, READ_STREAM_TIMEOUT);
                return hDevice.WriteOutputReportViaControl(outputBuffer);
            }

            return hDevice.WriteOutputReportViaInterrupt(outputBuffer, READ_STREAM_TIMEOUT);
        }

        protected bool WriteOutput()
        {
            if (ConnectionType == ConnectionType.Bluetooth)
                //if ((this.featureSet & VidPidFeatureSet.OnlyOutputData0x05) == 0)
                //    return hDevice.WriteOutputReportViaControl(outputReport);

                return BTOutputMethod == BTOutputReportMethod.WriteFile
                    ? hDevice.WriteOutputReportViaInterrupt(outputReport, READ_STREAM_TIMEOUT)
                    : hDevice.WriteOutputReportViaControl(outputReport);

            return hDevice.WriteOutputReportViaInterrupt(outReportBuffer, READ_STREAM_TIMEOUT);
        }

        private unsafe void PerformDs4Output()
        {
            try
            {
                var lastError = 0;
                bool result = false, currentRumble = false;
                while (!exitOutputThread)
                {
                    if (currentRumble)
                    {
                        lock (outputReport)
                        {
                            result = WriteOutput();
                        }

                        currentRumble = false;
                        if (!result)
                        {
                            currentRumble = true;
                            exitOutputThread = true;
                            var thisError = Marshal.GetLastWin32Error();
                            if (lastError != thisError)
                            {
                                Console.WriteLine(MacAddress + " " + DateTime.UtcNow.ToString("o") +
                                                  "> encountered write failure: " + thisError);
                                //Log.LogToGui(Mac.ToString() + " encountered write failure: " + thisError, true);
                                lastError = thisError;
                            }
                        }
                    }

                    if (!currentRumble)
                    {
                        lastError = 0;
                        lock (outReportBuffer)
                        {
                            Monitor.Wait(outReportBuffer);
                            fixed (byte* byteR = outputReport, byteB = outReportBuffer)
                            {
                                for (int i = 0, arlen = BT_OUTPUT_CHANGE_LENGTH; i < arlen; i++)
                                    byteR[i] = byteB[i];
                            }

                            //outReportBuffer.CopyTo(outputReport, 0);
                            if (outputPendCount > 1)
                            {
                                outputPendCount--;
                            }
                            else if (outputPendCount == 1)
                            {
                                outputPendCount--;
                                standbySw.Restart();
                            }
                            else
                            {
                                standbySw.Restart();
                            }
                        }

                        currentRumble = true;
                    }
                }
            }
            catch (ThreadInterruptedException)
            {
            }
        }

        /**
         * Is the device alive and receiving valid sensor input reports?
         */
        public virtual bool IsAlive()
        {
            return priorInputReport30 != 0xff;
        }

        public bool IsSynced()
        {
            return synced;
        }

        protected unsafe void PerformDs4Input()
        {
            unchecked
            {
                firstActive = DateTime.UtcNow;
                //NativeMethods.HidD_SetNumInputBuffers(hDevice.safeReadHandle.DangerousGetHandle(), 3);
                var latencyQueue = new Queue<long>(21); // Set capacity at max + 1 to avoid any resizing
                var tempLatencyCount = 0;
                long oldtime = 0;
                var currerror = string.Empty;
                long curtime = 0;
                long testelapsed = 0;
                timeoutEvent = false;
                ds4InactiveFrame = true;
                idleInput = true;
                var syncWriteReport = ConnectionType != ConnectionType.Bluetooth ||
                                      BTOutputMethod == BTOutputReportMethod.WriteFile;
                //bool syncWriteReport = true;
                var forceWrite = false;

                var maxBatteryValue = 0;
                var tempBattery = 0;
                var tempCharging = charging;
                uint tempStamp = 0;
                var elapsedDeltaTime = 0.0;
                uint tempDelta = 0;
                byte tempByte = 0;
                int CRC32_POS_1 = BT_INPUT_REPORT_CRC32_POS + 1,
                    CRC32_POS_2 = BT_INPUT_REPORT_CRC32_POS + 2,
                    CRC32_POS_3 = BT_INPUT_REPORT_CRC32_POS + 3;
                var crcpos = BT_INPUT_REPORT_CRC32_POS;
                var crcoffset = 0;
                long latencySum = 0;

                // Run continuous calibration on Gyro when starting input loop
                sixAxis.ResetContinuousCalibration();
                standbySw.Start();

                while (!exitInputThread)
                {
                    oldCharging = charging;
                    currerror = string.Empty;

                    if (tempLatencyCount >= 20)
                    {
                        latencySum -= latencyQueue.Dequeue();
                        tempLatencyCount--;
                    }

                    latencySum += lastTimeElapsed;
                    latencyQueue.Enqueue(lastTimeElapsed);
                    tempLatencyCount++;

                    //Latency = latencyQueue.Average();
                    Latency = latencySum / (double)tempLatencyCount;

                    readWaitEv.Set();


                    // Sony DS4 and compatible gamepads send data packets with 0x11 type code in BT mode. 
                    // Will no longer support any third party fake DS4 that does not behave according to official DS4 specs
                    //if (conType == ConnectionType.BT)
                    if (ConnectionType == ConnectionType.Bluetooth &&
                        (featureSet & VidPidFeatureSet.OnlyInputData0x01) == 0)
                    {
                        //HidDevice.ReadStatus res = hDevice.ReadFile(btInputReport);
                        //HidDevice.ReadStatus res = hDevice.ReadAsyncWithFileStream(btInputReport, READ_STREAM_TIMEOUT);
                        HidDeviceV3.ReadStatus res;


                        res = hDevice.ReadInputReport(InputReportBuffer, btInputReport.Length, out _);
                        Marshal.Copy(InputReportBuffer, btInputReport, 0, btInputReport.Length);


                        timeoutEvent = false;
                        if (res == HidDeviceV3.ReadStatus.Success)
                        {
                            //Array.Copy(btInputReport, 2, inputReport, 0, inputReport.Length);


                            fixed (byte* byteP = &btInputReport[2], imp = inputReport)
                            {
                                for (var j = 0; j < BT_INPUT_REPORT_LENGTH - 2; j++) imp[j] = byteP[j];
                            }


                            //uint recvCrc32 = BitConverter.ToUInt32(btInputReport, BT_INPUT_REPORT_CRC32_POS);
                            var recvCrc32 = btInputReport[BT_INPUT_REPORT_CRC32_POS] |
                                            (uint)(btInputReport[CRC32_POS_1] << 8) |
                                            (uint)(btInputReport[CRC32_POS_2] << 16) |
                                            (uint)(btInputReport[CRC32_POS_3] << 24);

                            var calcCrc32 = ~Crc32Algorithm.CalculateFasterBT78Hash(ref HamSeed,
                                ref btInputReport,
                                ref crcoffset, ref crcpos);
                            if (recvCrc32 != calcCrc32)
                            {
                                //Log.LogToGui("Crc check failed", true);
                                //Console.WriteLine(MacAddress.ToString() + " " + System.DateTime.UtcNow.ToString("o") + "" +
                                //                    "> invalid CRC32 in BT input report: 0x" + recvCrc32.ToString("X8") + " expected: 0x" + calcCrc32.ToString("X8"));

                                currentState.PacketCounter =
                                    pState.PacketCounter +
                                    1; //still increase so we know there were lost packets

                                // If the incoming data packet does not have the native DS4 type or CRC-32 checks keep failing. Fail out and disconnect controller.
                                if (inputReportErrorCount >= CRC32_NUM_ATTEMPTS)
                                {
                                    AppLogger.Instance.LogToGui(
                                        $"{MacAddress} failed CRC-32 checks {CRC32_NUM_ATTEMPTS} times. Disconnecting",
                                        false);

                                    readWaitEv.Reset();
                                    SendOutputReport(true,
                                        true); // Kick Windows into noticing the disconnection.
                                    StopOutputUpdate();
                                    isDisconnecting = true;
                                    Removal?.Invoke(this, EventArgs.Empty);

                                    timeoutExecuted = true;
                                    return;
                                }

                                inputReportErrorCount++;

                                readWaitEv.Reset();
                                continue;
                            }


                            inputReportErrorCount = 0;
                        }
                        else
                        {
                            if (res == HidDeviceV3.ReadStatus.WaitTimedOut)
                            {
                                AppLogger.Instance.LogToGui(MacAddress + " disconnected due to timeout", true);
                            }
                            else
                            {
                                var winError = Marshal.GetLastWin32Error();
                                Console.WriteLine(MacAddress + " " + DateTime.UtcNow.ToString("o") +
                                                  "> disconnect due to read failure: " + winError);
                                //Log.LogToGui(Mac.ToString() + " disconnected due to read failure: " + winError, true);
                                AppLogger.Instance.LogToGui(
                                    MacAddress + " disconnected due to read failure: " + winError,
                                    true);
                            }

                            readWaitEv.Reset();
                            SendOutputReport(true, true); // Kick Windows into noticing the disconnection.
                            StopOutputUpdate();
                            isDisconnecting = true;
                            Removal?.Invoke(this, EventArgs.Empty);

                            timeoutExecuted = true;
                            return;
                        }
                    }
                    else
                    {
                        //HidDevice.ReadStatus res = hDevice.ReadFile(inputReport);
                        //Array.Clear(inputReport, 0, inputReport.Length);
                        //HidDevice.ReadStatus res = hDevice.ReadAsyncWithFileStream(inputReport, READ_STREAM_TIMEOUT);


                        var res = hDevice.ReadInputReport(InputReportBuffer, inputReport.Length, out _);
                        Marshal.Copy(InputReportBuffer, inputReport, 0, inputReport.Length);

                        if (res != HidDeviceV3.ReadStatus.Success)
                        {
                            if (res == HidDeviceV3.ReadStatus.WaitTimedOut)
                            {
                                AppLogger.Instance.LogToGui(MacAddress + " disconnected due to timeout", true);
                            }
                            else
                            {
                                var winError = Marshal.GetLastWin32Error();
                                Console.WriteLine(MacAddress + " " + DateTime.UtcNow.ToString("o") +
                                                  "> disconnect due to read failure: " + winError);
                                //Log.LogToGui(Mac.ToString() + " disconnected due to read failure: " + winError, true);
                            }

                            readWaitEv.Reset();
                            StopOutputUpdate();
                            isDisconnecting = true;
                            Removal?.Invoke(this, EventArgs.Empty);

                            timeoutExecuted = true;
                            return;
                        }
                    }


                    readWaitEv.Wait();
                    readWaitEv.Reset();


                    curtime = Stopwatch.GetTimestamp();
                    testelapsed = curtime - oldtime;
                    lastTimeElapsedDouble = testelapsed * (1.0 / Stopwatch.Frequency) * 1000.0;
                    lastTimeElapsed = (long)lastTimeElapsedDouble;
                    oldtime = curtime;

                    // Not going to do featureSet check anymore
                    if (ConnectionType == ConnectionType.Bluetooth && btInputReport[0] != 0x11 &&
                        (featureSet & VidPidFeatureSet.OnlyInputData0x01) == 0)
                        //Received incorrect report, skip it
                        continue;

                    utcNow = DateTime.UtcNow; // timestamp with UTC in case system time zone changes

                    currentState.PacketCounter = pState.PacketCounter + 1;
                    currentState.ReportTimeStamp = utcNow;
                    currentState.LX = inputReport[1];
                    currentState.LY = inputReport[2];
                    currentState.RX = inputReport[3];
                    currentState.RY = inputReport[4];
                    currentState.L2 = inputReport[8];
                    currentState.R2 = inputReport[9];

                    tempByte = inputReport[5];
                    currentState.Triangle = (tempByte & (1 << 7)) != 0;
                    currentState.Circle = (tempByte & (1 << 6)) != 0;
                    currentState.Cross = (tempByte & (1 << 5)) != 0;
                    currentState.Square = (tempByte & (1 << 4)) != 0;

                    // First 4 bits denote dpad state. Clock representation
                    // with 8 meaning centered and 0 meaning DpadUp.
                    var dpad_state = (byte)(tempByte & 0x0F);

                    switch (dpad_state)
                    {
                        case 0:
                            currentState.DpadUp = true;
                            currentState.DpadDown = false;
                            currentState.DpadLeft = false;
                            currentState.DpadRight = false;
                            break;
                        case 1:
                            currentState.DpadUp = true;
                            currentState.DpadDown = false;
                            currentState.DpadLeft = false;
                            currentState.DpadRight = true;
                            break;
                        case 2:
                            currentState.DpadUp = false;
                            currentState.DpadDown = false;
                            currentState.DpadLeft = false;
                            currentState.DpadRight = true;
                            break;
                        case 3:
                            currentState.DpadUp = false;
                            currentState.DpadDown = true;
                            currentState.DpadLeft = false;
                            currentState.DpadRight = true;
                            break;
                        case 4:
                            currentState.DpadUp = false;
                            currentState.DpadDown = true;
                            currentState.DpadLeft = false;
                            currentState.DpadRight = false;
                            break;
                        case 5:
                            currentState.DpadUp = false;
                            currentState.DpadDown = true;
                            currentState.DpadLeft = true;
                            currentState.DpadRight = false;
                            break;
                        case 6:
                            currentState.DpadUp = false;
                            currentState.DpadDown = false;
                            currentState.DpadLeft = true;
                            currentState.DpadRight = false;
                            break;
                        case 7:
                            currentState.DpadUp = true;
                            currentState.DpadDown = false;
                            currentState.DpadLeft = true;
                            currentState.DpadRight = false;
                            break;
                        case 8:
                        default:
                            currentState.DpadUp = false;
                            currentState.DpadDown = false;
                            currentState.DpadLeft = false;
                            currentState.DpadRight = false;
                            break;
                    }

                    tempByte = inputReport[6];
                    currentState.R3 = (tempByte & (1 << 7)) != 0;
                    currentState.L3 = (tempByte & (1 << 6)) != 0;
                    currentState.Options = (tempByte & (1 << 5)) != 0;
                    currentState.Share = (tempByte & (1 << 4)) != 0;
                    currentState.R2Btn = (inputReport[6] & (1 << 3)) != 0;
                    currentState.L2Btn = (inputReport[6] & (1 << 2)) != 0;
                    currentState.R1 = (tempByte & (1 << 1)) != 0;
                    currentState.L1 = (tempByte & (1 << 0)) != 0;

                    tempByte = inputReport[7];
                    currentState.PS = (tempByte & (1 << 0)) != 0;
                    currentState.TouchButton = (tempByte & 0x02) != 0;
                    currentState.OutputTouchButton = currentState.TouchButton;
                    currentState.FrameCounter = (byte)(tempByte >> 2);

                    if ((featureSet & VidPidFeatureSet.NoBatteryReading) == 0)
                    {
                        tempByte = inputReport[30];
                        tempCharging = (tempByte & 0x10) != 0;
                        if (tempCharging != charging)
                        {
                            charging = tempCharging;
                            ChargingChanged?.Invoke(this);
                        }

                        maxBatteryValue = charging ? BATTERY_MAX_USB : BATTERY_MAX;
                        tempBattery = (tempByte & 0x0f) * 100 / maxBatteryValue;
                        tempBattery = Math.Min(tempBattery, 100);
                        if (tempBattery != battery)
                        {
                            battery = tempBattery;
                            BatteryChanged?.Invoke(this);
                        }

                        currentState.Battery = (byte)battery;
                        //Debug.WriteLine("CURRENT BATTERY: " + (inputReport[30] & 0x0f) + " | " + tempBattery + " | " + battery);
                        if (tempByte != priorInputReport30)
                            priorInputReport30 = tempByte;
                        //Debug.WriteLine(MacAddress.ToString() + " " + System.DateTime.UtcNow.ToString("o") + "> power subsystem octet: 0x" + inputReport[30].ToString("x02"));
                    }
                    else
                    {
                        // Some gamepads don't send battery values in DS4 compatible data fields, so use dummy 99% value to avoid constant low battery warnings
                        priorInputReport30 = 0x0F;
                        battery = 99;
                        currentState.Battery = 99;
                    }

                    tempStamp = (uint)((ushort)(inputReport[11] << 8) | inputReport[10]);
                    if (timeStampInit == false)
                    {
                        timeStampInit = true;
                        deltaTimeCurrent = tempStamp * 16u / 3u;
                    }
                    else if (timeStampPrevious > tempStamp)
                    {
                        tempDelta = ushort.MaxValue - timeStampPrevious + tempStamp + 1u;
                        deltaTimeCurrent = tempDelta * 16u / 3u;
                    }
                    else
                    {
                        tempDelta = tempStamp - timeStampPrevious;
                        deltaTimeCurrent = tempDelta * 16u / 3u;
                    }

                    // Make sure timestamps don't match
                    if (deltaTimeCurrent != 0)
                    {
                        elapsedDeltaTime = 0.000001 * deltaTimeCurrent; // Convert from microseconds to seconds
                        currentState.totalMicroSec = pState.totalMicroSec + deltaTimeCurrent;
                    }
                    else
                    {
                        // Duplicate timestamp. Use system clock for elapsed time instead
                        elapsedDeltaTime = lastTimeElapsedDouble * .001;
                        currentState.totalMicroSec = pState.totalMicroSec + (uint)(elapsedDeltaTime * 1000000);
                    }

                    currentState.elapsedTime = elapsedDeltaTime;
                    currentState.ds4Timestamp = (ushort)tempStamp;
                    timeStampPrevious = tempStamp;

                    //Simpler touch storing
                    currentState.TrackPadTouch0.RawTrackingNum = inputReport[35];
                    currentState.TrackPadTouch0.Id = (byte)(inputReport[35] & 0x7f);
                    currentState.TrackPadTouch0.IsActive = (inputReport[35] & 0x80) == 0;
                    currentState.TrackPadTouch0.X =
                        (short)(((ushort)(inputReport[37] & 0x0f) << 8) | inputReport[36]);
                    currentState.TrackPadTouch0.Y =
                        (short)((inputReport[38] << 4) | ((ushort)(inputReport[37] & 0xf0) >> 4));

                    currentState.TrackPadTouch1.RawTrackingNum = inputReport[39];
                    currentState.TrackPadTouch1.Id = (byte)(inputReport[39] & 0x7f);
                    currentState.TrackPadTouch1.IsActive = (inputReport[39] & 0x80) == 0;
                    currentState.TrackPadTouch1.X =
                        (short)(((ushort)(inputReport[41] & 0x0f) << 8) | inputReport[40]);
                    currentState.TrackPadTouch1.Y =
                        (short)((inputReport[42] << 4) | ((ushort)(inputReport[41] & 0xf0) >> 4));

                    if (ConnectionType == ConnectionType.SonyWirelessAdapter)
                    {
                        var controllerSynced = inputReport[31] == 0;
                        if (controllerSynced != synced)
                        {
                            runCalib = synced = controllerSynced;
                            SyncChange?.Invoke(this, EventArgs.Empty);
                            if (synced)
                            {
                                forceWrite = true;
                                sixAxis.ResetContinuousCalibration();
                            }
                            else
                            {
                                standbySw.Reset();
                                sixAxis.StopContinuousCalibration();
                            }
                        }
                    }


                    // XXX DS4State mapping needs fixup, turn touches into an array[4] of structs.  And include the touchpad details there instead.
                    try
                    {
                        // Only care if one touch packet is detected. Other touch packets
                        // don't seem to contain relevant data. ds4drv does not use them either.
                        for (int touches = Math.Max((int)inputReport[-1 + DS4Touchpad.DS4_TOUCHPAD_DATA_OFFSET - 1],
                                 1),
                             touchOffset = 0;
                             touches > 0;
                             touches--, touchOffset += 9)
                            //for (int touches = inputReport[-1 + DS4Touchpad.TOUCHPAD_DATA_OFFSET - 1], touchOffset = 0; touches > 0; touches--, touchOffset += 9)
                        {
                            currentState.TouchPacketCounter =
                                inputReport[-1 + DS4Touchpad.DS4_TOUCHPAD_DATA_OFFSET + touchOffset];
                            currentState.Touch1 =
                                inputReport[0 + DS4Touchpad.DS4_TOUCHPAD_DATA_OFFSET + touchOffset] >> 7 != 0
                                    ? false
                                    : true; // finger 1 detected
                            currentState.Touch1Identifier =
                                (byte)(inputReport[0 + DS4Touchpad.DS4_TOUCHPAD_DATA_OFFSET + touchOffset] & 0x7f);
                            currentState.Touch2 =
                                inputReport[4 + DS4Touchpad.DS4_TOUCHPAD_DATA_OFFSET + touchOffset] >> 7 != 0
                                    ? false
                                    : true; // finger 2 detected
                            currentState.Touch2Identifier =
                                (byte)(inputReport[4 + DS4Touchpad.DS4_TOUCHPAD_DATA_OFFSET + touchOffset] & 0x7f);
                            currentState.Touch1Finger =
                                currentState.Touch1 || currentState.Touch2; // >= 1 touch detected
                            currentState.Touch2Fingers =
                                currentState.Touch1 && currentState.Touch2; // 2 touches detected
                            var touchX =
                                ((inputReport[2 + DS4Touchpad.DS4_TOUCHPAD_DATA_OFFSET + touchOffset] & 0xF) << 8) |
                                inputReport[1 + DS4Touchpad.DS4_TOUCHPAD_DATA_OFFSET + touchOffset];
                            currentState.TouchLeft = touchX >= DS4Touchpad.RESOLUTION_X_MAX * 2 / 5 ? false : true;
                            currentState.TouchRight = touchX < DS4Touchpad.RESOLUTION_X_MAX * 2 / 5 ? false : true;
                            // Even when idling there is still a touch packet indicating no touch 1 or 2
                            if (synced)
                                touchpad.HandleTouchPad(inputReport, currentState,
                                    DS4Touchpad.DS4_TOUCHPAD_DATA_OFFSET,
                                    touchOffset);
                        }
                    }
                    catch (Exception ex)
                    {
                        currerror = $"Touchpad: {ex.Message}";
                    }


                    // Store Gyro and Accel values
                    //Array.Copy(inputReport, 13, gyro, 0, 6);
                    //Array.Copy(inputReport, 19, accel, 0, 6);

                    // Store Gyro and Accel values. Use pointers here as it seems faster than using Array.Copy
                    fixed (byte* pbInput = &inputReport[13], pbGyro = gyro, pbAccel = accel)
                    {
                        for (var i = 0; i < 6; i++) pbGyro[i] = pbInput[i];

                        for (var i = 6; i < 12; i++) pbAccel[i - 6] = pbInput[i];

                        if (synced) sixAxis.HandleSixAxis(pbGyro, pbAccel, currentState, elapsedDeltaTime);
                    }

                    /* Debug output of incoming HID data:
                    if (cState.L2 == 0xff && cState.R2 == 0xff)
                    {
                        Debug.Write(MacAddress.ToString() + " " + System.DateTime.UtcNow.ToString("o") + ">");
                        for (int i = 0; i < inputReport.Length; i++)
                        {
                            Debug.Write(" " + inputReport[i].ToString("x2"));
                        }

                        Console.WriteLine();
                    }
                    */


                    ds4InactiveFrame = currentState.FrameCounter == pState.FrameCounter;
                    if (!ds4InactiveFrame) isRemoved = false;

                    if (ConnectionType == ConnectionType.Usb)
                    {
                        if (idleTimeout == 0)
                        {
                            lastActive = utcNow;
                        }
                        else
                        {
                            idleInput = IsDs4Idle();
                            if (!idleInput) lastActive = utcNow;
                        }
                    }
                    else
                    {
                        var shouldDisconnect = false;
                        if (!isRemoved && idleTimeout > 0)
                        {
                            idleInput = IsDs4Idle();
                            if (idleInput)
                            {
                                var timeout = lastActive + TimeSpan.FromSeconds(idleTimeout);
                                if (!charging)
                                    shouldDisconnect = utcNow >= timeout;
                            }
                            else
                            {
                                lastActive = utcNow;
                            }
                        }
                        else
                        {
                            lastActive = utcNow;
                        }

                        if (shouldDisconnect)
                        {
                            AppLogger.Instance.LogToGui(MacAddress + " disconnecting due to idle disconnect", false);

                            if (ConnectionType == ConnectionType.Bluetooth)
                            {
                                if (DisconnectBT(true))
                                {
                                    timeoutExecuted = true;
                                    return; // all done
                                }
                            }
                            else if (ConnectionType == ConnectionType.SonyWirelessAdapter)
                            {
                                DisconnectDongle();
                            }
                        }
                    }


                    Report?.Invoke(this, EventArgs.Empty);


                    SendOutputReport(syncWriteReport, forceWrite);
                    forceWrite = false;


                    if (!string.IsNullOrEmpty(currerror))
                        error = currerror;
                    else if (!string.IsNullOrEmpty(error))
                        error = string.Empty;


                    currentState.CopyTo(pState);


                    if (!hasInputEvts) continue;

                    lock (eventQueueLock)
                    {
                        Action tempAct = null;
                        for (int actInd = 0, actLen = eventQueue.Count; actInd < actLen; actInd++)
                        {
                            tempAct = eventQueue.Dequeue();
                            tempAct.Invoke();
                        }

                        hasInputEvts = false;
                    }
                }
            }

            timeoutExecuted = true;
        }

        private unsafe void PrepareOutputReportInner(ref bool change, ref bool haptime)
        {
            var usingBT = ConnectionType == ConnectionType.Bluetooth;

            if (usingBT && (featureSet & VidPidFeatureSet.OnlyOutputData0x05) == 0)
            {
                outReportBuffer[0] = knownGoodBTOutputReportType;
                //outReportBuffer[0] = 0x15;
                //outReportBuffer[1] = (byte)(0x80 | btPollRate); // input report rate
                outReportBuffer[1] = (byte)(0xC0 | btPollRate); // input report rate
                outReportBuffer[2] = 0xA0;

                // enable rumble (0x01), lightbar (0x02), flash (0x04). Default: 0xF7
                outReportBuffer[3] = outputFeaturesByte;
                outReportBuffer[4] = 0x04;

                outReportBuffer[6] = CurrentHaptics.RumbleState.RumbleMotorStrengthRightLightFast; // fast motor
                outReportBuffer[7] = CurrentHaptics.RumbleState.RumbleMotorStrengthLeftHeavySlow; // slow motor
                outReportBuffer[8] = CurrentHaptics.LightbarState.LightBarColor.Red; // red
                outReportBuffer[9] = CurrentHaptics.LightbarState.LightBarColor.Green; // green
                outReportBuffer[10] = CurrentHaptics.LightbarState.LightBarColor.Blue; // blue
                outReportBuffer[11] = CurrentHaptics.LightbarState.LightBarFlashDurationOn; // flash on duration
                outReportBuffer[12] = CurrentHaptics.LightbarState.LightBarFlashDurationOff; // flash off duration

                fixed (byte* byteR = outputReport, byteB = outReportBuffer)
                {
                    for (int i = 0, arlen = BT_OUTPUT_CHANGE_LENGTH; !change && i < arlen; i++)
                        change = byteR[i] != byteB[i];
                }

                /*if (change)
                {
                    Console.WriteLine("CHANGE: {0} {1} {2} {3} {4} {5}", currentHap.LightBarColor.red, currentHap.LightBarColor.green, currentHap.LightBarColor.blue, currentHap.RumbleMotorStrengthRightLightFast, currentHap.RumbleMotorStrengthLeftHeavySlow, DateTime.Now.ToString());
                }
                */

                haptime = haptime || change;
            }
            else
            {
                outReportBuffer[0] = 0x05;
                // enable rumble (0x01), lightbar (0x02), flash (0x04). Default: 0xF7
                outReportBuffer[1] = outputFeaturesByte;
                outReportBuffer[2] = 0x04;
                outReportBuffer[4] = CurrentHaptics.RumbleState.RumbleMotorStrengthRightLightFast; // fast motor
                outReportBuffer[5] = CurrentHaptics.RumbleState.RumbleMotorStrengthLeftHeavySlow; // slow  motor
                outReportBuffer[6] = CurrentHaptics.LightbarState.LightBarColor.Red; // red
                outReportBuffer[7] = CurrentHaptics.LightbarState.LightBarColor.Green; // green
                outReportBuffer[8] = CurrentHaptics.LightbarState.LightBarColor.Blue; // blue
                outReportBuffer[9] = CurrentHaptics.LightbarState.LightBarFlashDurationOn; // flash on duration
                outReportBuffer[10] = CurrentHaptics.LightbarState.LightBarFlashDurationOff; // flash off duration

                fixed (byte* byteR = outputReport, byteB = outReportBuffer)
                {
                    for (int i = 0, arlen = USB_OUTPUT_CHANGE_LENGTH; !change && i < arlen; i++)
                        change = byteR[i] != byteB[i];
                }

                haptime = haptime || change;
                if (haptime && audio != null)
                {
                    // Headphone volume levels
                    outReportBuffer[19] = outReportBuffer[20] =
                        Convert.ToByte(audio.getVolume());
                    // Microphone volume level
                    outReportBuffer[21] = Convert.ToByte(micAudio.getVolume());
                }
            }
        }

        private void SendOutputReport(bool synchronous, bool force = false, bool quitOutputThreadOnError = true)
        {
            MergeStates();
            //setTestRumble();
            //setHapticState();

            var quitOutputThread = false;
            var usingBT = ConnectionType == ConnectionType.Bluetooth;

            // Some gamepads don't support lightbar and rumble, so no need to write out anything (writeOut always fails, so DS4Windows would accidentally force quit the gamepad connection).
            // If noOutputData featureSet flag is set then don't try to write out anything to the gamepad device.
            if ((featureSet & VidPidFeatureSet.NoOutputData) != 0)
            {
                if (exitOutputThread == false && (IsRemoving || IsRemoved))
                {
                    // Gamepad disconnecting or disconnected. Signal closing of OutputUpdate thread
                    StopOutputUpdate();
                    exitOutputThread = true;
                }

                return;
            }

            //bool output = outputPendCount > 0, change = force;
            bool output = outputPendCount > 0, change = force;
            //bool output = false, change = force;
            var haptime = output || standbySw.ElapsedMilliseconds >= 4000L;

            if (usingBT &&
                BTOutputMethod == BTOutputReportMethod.HidD_SetOutputReport)
                Monitor.Enter(outReportBuffer);

            PrepareOutputReportInner(ref change, ref haptime);

            if (rumbleAutostopTimer.IsRunning)
                // Workaround to a bug in ViGem driver. Force stop potentially stuck rumble motor on the next output report if there haven't been new rumble events within X seconds
                if (rumbleAutostopTimer.ElapsedMilliseconds >= rumbleAutostopTime)
                    SetRumble(0, 0);

            if (synchronous)
            {
                if (output || haptime)
                {
                    if (change)
                    {
                        outputPendCount = OUTPUT_MIN_COUNT_BT;
                        standbySw.Reset();
                    }
                    else if (outputPendCount > 1)
                    {
                        outputPendCount--;
                    }
                    else if (outputPendCount == 1)
                    {
                        outputPendCount--;
                        standbySw.Restart();
                    }
                    else
                    {
                        standbySw.Restart();
                    }
                    //standbySw.Restart();

                    if (usingBT)
                    {
                        if (BTOutputMethod == BTOutputReportMethod.HidD_SetOutputReport)
                            Monitor.Enter(outputReport);

                        outReportBuffer.CopyTo(outputReport, 0);

                        if ((featureSet & VidPidFeatureSet.OnlyOutputData0x05) == 0)
                        {
                            // Need to calculate and populate CRC-32 data so controller will accept the report
                            var len = outputReport.Length;
                            var calcCrc32 = ~Crc32Algorithm.Compute(outputBTCrc32Head);
                            calcCrc32 = ~Crc32Algorithm.CalculateBasicHash(ref calcCrc32, ref outputReport, 0,
                                len - 4);
                            outputReport[len - 4] = (byte)calcCrc32;
                            outputReport[len - 3] = (byte)(calcCrc32 >> 8);
                            outputReport[len - 2] = (byte)(calcCrc32 >> 16);
                            outputReport[len - 1] = (byte)(calcCrc32 >> 24);

                            //Console.WriteLine("Write CRC-32 to output report");
                        }
                    }

                    try
                    {
                        if (!WriteOutput())
                            if (quitOutputThreadOnError)
                            {
                                var winError = Marshal.GetLastWin32Error();

                                // Logfile notification that the gamepad is force disconnected because of writeOutput failed
                                if (quitOutputThread == false && !isDisconnecting)
                                    AppLogger.Instance.LogToGui(
                                        $"Gamepad data write connection is lost. Disconnecting the gamepad. LastErrorCode={winError}",
                                        false);

                                quitOutputThread = true;
                            }
                    }
                    catch
                    {
                    } // If it's dead already, don't worry about it.

                    if (usingBT)
                    {
                        if (BTOutputMethod == BTOutputReportMethod.HidD_SetOutputReport) Monitor.Exit(outputReport);
                    }
                    else
                    {
                        lock (outReportBuffer)
                        {
                            Monitor.Pulse(outReportBuffer);
                        }
                    }
                }
            }
            else
            {
                //for (int i = 0, arlen = outputReport.Length; !change && i < arlen; i++)
                //    change = outputReport[i] != outReportBuffer[i];

                if (output || haptime)
                {
                    if (change)
                    {
                        outputPendCount = OUTPUT_MIN_COUNT_BT;
                        standbySw.Reset();
                    }

                    Monitor.Pulse(outReportBuffer);
                }
            }

            if (usingBT &&
                BTOutputMethod == BTOutputReportMethod.HidD_SetOutputReport)
                Monitor.Exit(outReportBuffer);

            if (quitOutputThread)
            {
                StopOutputUpdate();
                exitOutputThread = true;
            }
        }

        // Perform outReportBuffer copy on a separate thread to save
        // time on main input thread
        private void OutReportCopy()
        {
            try
            {
                while (!exitOutputThread)
                    lock (outReportBuffer)
                    {
                        outReportBuffer.CopyTo(outputReport, 0);
                        Monitor.Wait(outReportBuffer);
                    }
            }
            catch (ThreadInterruptedException)
            {
            }
        }

        public virtual bool DisconnectWireless(bool callRemoval = false)
        {
            var result = false;
            if (ConnectionType == ConnectionType.Bluetooth)
                result = DisconnectBT(callRemoval);
            else if (ConnectionType == ConnectionType.SonyWirelessAdapter) result = DisconnectDongle(callRemoval);

            return result;
        }

        public virtual bool DisconnectBT(bool callRemoval = false)
        {
            if (MacAddress != null)
            {
                // Wait for output report to be written
                StopOutputUpdate();
                Console.WriteLine("Trying to disconnect BT device " + MacAddress);
                var btHandle = IntPtr.Zero;
                uint IOCTL_BTH_DISCONNECT_DEVICE = 0x41000c;

                var btAddr = new byte[8];
                //
                // TODO: can be further simplified
                // 
                var sbytes = MacAddress.ToFriendlyName().Split(':');
                for (var i = 0; i < 6; i++)
                    // parse hex byte in reverse order
                    btAddr[5 - i] = Convert.ToByte(sbytes[i], 16);

                var lbtAddr = BitConverter.ToInt64(btAddr, 0);

                var success = false;

                lock (outputReport)
                {
                    var p = new NativeMethods.BLUETOOTH_FIND_RADIO_PARAMS();
                    p.dwSize = Marshal.SizeOf(typeof(NativeMethods.BLUETOOTH_FIND_RADIO_PARAMS));
                    var searchHandle = NativeMethods.BluetoothFindFirstRadio(ref p, ref btHandle);
                    var bytesReturned = 0;

                    while (!success && btHandle != IntPtr.Zero)
                    {
                        success = NativeMethods.DeviceIoControl(btHandle, IOCTL_BTH_DISCONNECT_DEVICE, ref lbtAddr, 8,
                            IntPtr.Zero, 0, ref bytesReturned, IntPtr.Zero);
                        NativeMethods.CloseHandle(btHandle);
                        if (!success)
                            if (!NativeMethods.BluetoothFindNextRadio(searchHandle, ref btHandle))
                                btHandle = IntPtr.Zero;
                    }

                    NativeMethods.BluetoothFindRadioClose(searchHandle);
                    Console.WriteLine("Disconnect successful: " + success);
                }

                success = true; // XXX return value indicates failure, but it still works?
                if (success)
                {
                    IsDisconnecting = true;

                    if (callRemoval)
                        Removal?.Invoke(this, EventArgs.Empty);

                    //System.Threading.Tasks.Task.Factory.StartNew(() => { Removal?.Invoke(this, EventArgs.Empty); });
                }

                return success;
            }

            return false;
        }

        public virtual bool DisconnectDongle(bool remove = false)
        {
            var result = false;
            var disconnectReport = new byte[65];
            disconnectReport[0] = 0xe2;
            disconnectReport[1] = 0x02;
            Array.Clear(disconnectReport, 2, 63);

            if (remove)
                StopOutputUpdate();

            lock (outputReport)
            {
                result = hDevice.WriteFeatureReport(disconnectReport);
            }

            if (result && remove)
            {
                isDisconnecting = true;

                Removal?.Invoke(this, EventArgs.Empty);

                //System.Threading.Tasks.Task.Factory.StartNew(() => { Removal?.Invoke(this, EventArgs.Empty); });
                //Removal?.Invoke(this, EventArgs.Empty);
            }
            else if (result && !remove)
            {
                isRemoved = true;
            }

            return result;
        }

        public void SetRumble(byte rightLightFastMotor, byte leftHeavySlowMotor)
        {
            testRumble.RumbleState.RumbleMotorStrengthRightLightFast = rightLightFastMotor;
            testRumble.RumbleState.RumbleMotorStrengthLeftHeavySlow = leftHeavySlowMotor;
            testRumble.RumbleState.RumbleMotorsExplicitlyOff = rightLightFastMotor == 0 && leftHeavySlowMotor == 0;

            // If rumble autostop timer (msecs) is enabled for this device then restart autostop timer everytime rumble is modified (or stop the timer if rumble is set to zero)
            if (rumbleAutostopTime > 0)
            {
                if (testRumble.RumbleState.RumbleMotorsExplicitlyOff)
                    rumbleAutostopTimer
                        .Reset(); // Stop an autostop timer because ViGem driver sent properly a zero rumble notification
                else if (CurrentHaptics.RumbleState.RumbleMotorStrengthLeftHeavySlow != leftHeavySlowMotor ||
                         CurrentHaptics.RumbleState.RumbleMotorStrengthRightLightFast != rightLightFastMotor)
                    rumbleAutostopTimer
                        .Restart(); // Start an autostop timer to stop potentially stuck rumble motor because of lost rumble notification events from ViGem driver
            }
        }

        protected void MergeStates()
        {
            if (testRumble.IsRumbleSet())
            {
                if (testRumble.RumbleState.RumbleMotorsExplicitlyOff)
                    testRumble.RumbleState.RumbleMotorsExplicitlyOff = false;

                //currentHap.rumbleState.RumbleMotorStrengthLeftHeavySlow = testRumble.rumbleState.RumbleMotorStrengthLeftHeavySlow;
                //currentHap.rumbleState.RumbleMotorStrengthRightLightFast = testRumble.rumbleState.RumbleMotorStrengthRightLightFast;
                CurrentHaptics.RumbleState = testRumble.RumbleState;
            }
        }

        public DS4State GetRawCurrentState()
        {
            return currentState.Clone();
        }

        public DS4State GetRawPreviousState()
        {
            return pState.Clone();
        }

        public void GetRawCurrentState(DS4State state)
        {
            currentState.CopyTo(state);
        }

        public void GetRawPreviousState(DS4State state)
        {
            pState.CopyTo(state);
        }

        public virtual DS4State GetCurrentStateReference()
        {
            return currentState;
        }

        public virtual DS4State GetPreviousStateReference()
        {
            return pState;
        }

        public DS4State GetRawCurrentStateRef()
        {
            return currentState;
        }

        public DS4State GetRawPreviousStateRef()
        {
            return pState;
        }

        public virtual void PreserveMergedStateData()
        {
        }

        public bool IsDs4Idle()
        {
            if (currentState.Square || currentState.Cross || currentState.Circle || currentState.Triangle)
                return false;
            if (currentState.DpadUp || currentState.DpadLeft || currentState.DpadDown || currentState.DpadRight)
                return false;
            if (currentState.L3 || currentState.R3 || currentState.L1 || currentState.R1 || currentState.Share ||
                currentState.Options || currentState.PS)
                return false;
            if (currentState.L2 != 0 || currentState.R2 != 0)
                return false;
            // TODO calibrate to get an accurate jitter and center-play range and centered position
            const int slop = 64;
            if (currentState.LX <= 127 - slop || currentState.LX >= 128 + slop || currentState.LY <= 127 - slop ||
                currentState.LY >= 128 + slop)
                return false;
            if (currentState.RX <= 127 - slop || currentState.RX >= 128 + slop || currentState.RY <= 127 - slop ||
                currentState.RY >= 128 + slop)
                return false;
            if (currentState.Touch1 || currentState.Touch2 || currentState.TouchButton)
                return false;
            return true;
        }

        public void SetHapticState(ref DS4HapticState hs)
        {
            CurrentHaptics = hs;
        }

        public void SetLightbarState(ref DS4LightbarState lightState)
        {
            CurrentHaptics.LightbarState = lightState;
        }

        public void SetRumbleState(ref DS4ForceFeedbackState rumbleState)
        {
            CurrentHaptics.RumbleState = rumbleState;
        }

        public override string ToString()
        {
            return MacAddress.ToFriendlyName();
        }

        protected void RunRemoval()
        {
            Removal?.Invoke(this, EventArgs.Empty);
        }

        public void removeReportHandlers()
        {
            Report = null;
        }

        public void QueueEvent(Action act)
        {
            lock (eventQueueLock)
            {
                eventQueue.Enqueue(act);
                hasInputEvts = true;
            }
        }

        public void UpdateSerial()
        {
            hDevice.ResetSerial();

            var tempMac = hDevice.ReadSerial(SerialReportID);

            if (tempMac.Equals(MacAddress)) return;

            MacAddress = tempMac;
            SerialChange?.Invoke(this, EventArgs.Empty);
            MacAddressChanged?.Invoke(this, EventArgs.Empty);
        }

        public bool IsValidSerial()
        {
            return !MacAddress.Equals(PhysicalAddress.Parse(BLANK_SERIAL));
        }

        public static bool IsValidSerial(PhysicalAddress test)
        {
            return !test.Equals(PhysicalAddress.Parse(BLANK_SERIAL));
        }

        public void PrepareAbort()
        {
            abortInputThread = true;
        }

        public virtual void MergeStateData(DS4State dState)
        {
        }

        private void PrepareOutputFeaturesByte()
        {
            if (nativeOptionsStore != null)
            {
                if (nativeOptionsStore.IsCopyCat)
                    outputFeaturesByte = COPYCAT_OUTPUT_FEATURES;
                else
                    outputFeaturesByte = DEFAULT_OUTPUT_FEATURES;
            }
        }

        private void SetupOptionsEvents()
        {
            if (nativeOptionsStore != null)
                nativeOptionsStore.IsCopyCatChanged += PrepareOutputFeaturesByte;
        }

        public virtual void PrepareTriggerEffect(TriggerId trigger,
            TriggerEffects effect, TriggerEffectSettings effectSettings)
        {
        }

        public virtual void CheckControllerNumDeviceSettings(int numControllers)
        {
        }

        public virtual void LoadStoreSettings()
        {
            if (nativeOptionsStore != null) PrepareOutputFeaturesByte();
        }

        private void ReleaseUnmanagedResources()
        {
            if (InputReportBuffer != IntPtr.Zero)
                Marshal.FreeHGlobal(InputReportBuffer);
        }

        protected virtual void Dispose(bool disposing)
        {
            ReleaseUnmanagedResources();
            if (disposing)
            {
                hDevice?.Dispose();
                readWaitEv?.Dispose();
            }
        }

        ~DS4Device()
        {
            Dispose(false);
        }

        public class GyroMouseSens
        {
            public double mouseCoefficient = 0.012;
            public double mouseOffset = 0.2;
            public double mouseSmoothOffset = 0.2;
        }
    }
}