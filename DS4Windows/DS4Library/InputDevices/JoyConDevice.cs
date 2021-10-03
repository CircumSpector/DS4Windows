using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using DS4WinWPF.DS4Control.Logging;
using OpenTracing.Util;

namespace DS4Windows.InputDevices
{
    public class JoyConDevice : DS4Device
    {
        public enum JoyConSide : uint
        {
            None,
            Left,
            Right
        }

        private const int AMP_REAL_MIN = 0;

        //private const int AMP_REAL_MAX = 1003;
        private const int AMP_LIMIT_MAX = 404;

        private const int AMP_LIMIT_L_MAX = 404;

        //private const int AMP_LIMIT_R_MAX = 206;
        private const int AMP_LIMIT_R_MAX = 404;

        public const int JOYCON_L_PRODUCT_ID = 0x2006;
        public const int JOYCON_R_PRODUCT_ID = 0x2007;

        private const int SUBCOMMAND_HEADER_LEN = 8;
        private const int SUBCOMMAND_BUFFER_LEN = 64;
        private const int SUBCOMMAND_RESPONSE_TIMEOUT = 500;
        public const int IMU_XAXIS_IDX = 0, IMU_YAW_IDX = 0;
        public const int IMU_YAXIS_IDX = 1, IMU_PITCH_IDX = 1;
        public const int IMU_ZAXIS_IDX = 2, IMU_ROLL_IDX = 2;

        private const double STICK_AXIS_MAX_CUTOFF = 0.96;
        private const double STICK_AXIS_MIN_CUTOFF = 1.04;

        private const double STICK_AXIS_LS_X_MAX_CUTOFF = 0.96;
        private const double STICK_AXIS_LS_X_MIN_CUTOFF = 1.48;
        private const double STICK_AXIS_RS_X_MAX_CUTOFF = 0.96;
        private const double STICK_AXIS_RS_X_MIN_CUTOFF = 1.04;

        private const double STICK_AXIS_LS_Y_MAX_CUTOFF = 0.96;
        private const double STICK_AXIS_LS_Y_MIN_CUTOFF = 1.14;
        private const double STICK_AXIS_RS_Y_MAX_CUTOFF = 0.96;
        private const double STICK_AXIS_RS_Y_MIN_CUTOFF = 1.14;

        public const int INPUT_REPORT_LEN = 362;
        public const int OUTPUT_REPORT_LEN = 49;
        public const int RUMBLE_REPORT_LEN = 64;

        // Converts raw gyro input value to dps. Equal to (4588/65535)
        private const float GYRO_IN_DEG_SEC_FACTOR = 0.070f;
        private new const int WARN_INTERVAL_BT = 40;
        private new const int WARN_INTERVAL_USB = 30;

        private static readonly RumbleTableData[] fixedRumbleTable =
        {
            new(0x00, 0x0040, 0),
            new(0x02, 0x8040, 10), new(0x04, 0x0041, 12), new(0x06, 0x8041, 14),
            new(0x08, 0x0042, 17), new(0x0A, 0x8042, 20), new(0x0C, 0x0043, 24),
            new(0x0E, 0x8043, 28), new(0x10, 0x0044, 33), new(0x12, 0x8044, 40),
            new(0x14, 0x0045, 47), new(0x16, 0x8045, 56), new(0x18, 0x0046, 67),
            new(0x1A, 0x8046, 80), new(0x1C, 0x0047, 95), new(0x1E, 0x8047, 112),
            new(0x20, 0x0048, 117), new(0x22, 0x8048, 123), new(0x24, 0x0049, 128),
            new(0x26, 0x8049, 134), new(0x28, 0x004A, 140), new(0x2A, 0x804A, 146),
            new(0x2C, 0x004B, 152), new(0x2E, 0x804B, 159), new(0x30, 0x004C, 166),
            new(0x32, 0x804C, 173), new(0x34, 0x004D, 181), new(0x36, 0x804D, 189),
            new(0x38, 0x004E, 198), new(0x3A, 0x804E, 206), new(0x3C, 0x004F, 215),
            new(0x3E, 0x804F, 225), new(0x40, 0x0050, 230), new(0x42, 0x8050, 235),
            new(0x44, 0x0051, 240), new(0x46, 0x8051, 245), new(0x48, 0x0052, 251),
            new(0x4A, 0x8052, 256), new(0x4C, 0x0053, 262), new(0x4E, 0x8053, 268),
            new(0x50, 0x0054, 273), new(0x52, 0x8054, 279), new(0x54, 0x0055, 286),
            new(0x56, 0x8055, 292), new(0x58, 0x0056, 298), new(0x5A, 0x8056, 305),
            new(0x5C, 0x0057, 311), new(0x5E, 0x8057, 318), new(0x60, 0x0058, 325),
            new(0x62, 0x8058, 332), new(0x64, 0x0059, 340), new(0x66, 0x8059, 347),
            new(0x68, 0x005A, 355), new(0x6A, 0x805A, 362), new(0x6C, 0x005B, 370),
            new(0x6E, 0x805B, 378), new(0x70, 0x005C, 387), new(0x72, 0x805C, 395),
            new(0x74, 0x005D, 404), new(0x76, 0x805D, 413), new(0x78, 0x005E, 422),
            new(0x7A, 0x805E, 431), new(0x7C, 0x005F, 440), new(0x7E, 0x805F, 450),
            new(0x80, 0x0060, 460), new(0x82, 0x8060, 470), new(0x84, 0x0061, 480),
            new(0x86, 0x8061, 491), new(0x88, 0x0062, 501), new(0x8A, 0x8062, 512),
            new(0x8C, 0x0063, 524), new(0x8E, 0x8063, 535), new(0x90, 0x0064, 547),
            new(0x92, 0x8064, 559), new(0x94, 0x0065, 571), new(0x96, 0x8065, 584),
            new(0x98, 0x0066, 596), new(0x9A, 0x8066, 609), new(0x9C, 0x0067, 623),
            new(0x9E, 0x8067, 636), new(0xA0, 0x0068, 650), new(0xA2, 0x8068, 665),
            new(0xA4, 0x0069, 679), new(0xA6, 0x8069, 694), new(0xA8, 0x006A, 709),
            new(0xAA, 0x806A, 725), new(0xAC, 0x006B, 741), new(0xAE, 0x806B, 757),
            new(0xB0, 0x006C, 773), new(0xB2, 0x806C, 790), new(0xB4, 0x006D, 808),
            new(0xB6, 0x806D, 825), new(0xB8, 0x006E, 843), new(0xBA, 0x806E, 862),
            new(0xBC, 0x006F, 881), new(0xBE, 0x806F, 900), new(0xC0, 0x0070, 920),
            new(0xC2, 0x8070, 940), new(0xC4, 0x0071, 960), new(0xC6, 0x8071, 981),
            new(0xC8, 0x0072, 1003)
        };

        private static readonly RumbleTableData[] compiledRumbleTable = new Func<RumbleTableData[]>(() =>
        {
            var tmpBuffer = new RumbleTableData[fixedRumbleTable.Last().amp + 1];
            var currentOffset = 0;
            var previousEntry = fixedRumbleTable[0];
            tmpBuffer[currentOffset] = previousEntry;
            var currentAmp = previousEntry.amp + 1;
            currentOffset++;

            for (var i = 1; i < fixedRumbleTable.Length; i++)
                //foreach(RumbleTableData entry in fixedRumbleTable)
            {
                var entry = fixedRumbleTable[i];
                if (currentAmp < entry.amp)
                    while (currentAmp < entry.amp)
                    {
                        tmpBuffer[currentOffset] = previousEntry;
                        currentOffset++;
                        currentAmp++;
                    }

                tmpBuffer[currentOffset] = entry;
                currentAmp = entry.amp + 1;
                currentOffset++;
                previousEntry = entry;
            }

            //fixedRumbleTable = null;
            return tmpBuffer;
        })();

        private static readonly byte[] commandBuffHeader =
            { 0x0, 0x1, 0x40, 0x40, 0x0, 0x1, 0x40, 0x40 };

        public double[] accelCoeff = new double[3];

        public short[] accelNeutral = new short[3];
        public short[] accelSens = new short[3];
        public double[] accelSensMulti = new double[3];

        /// <summary>
        ///     Flag to tell methods if device has been successfully initialized and opened
        /// </summary>
        private bool connectionOpened;

        public double currentLeftAmpRatio;
        public double currentRightAmpRatio;

        public short[] gyroBias = new short[3];
        public short[] gyroCalibOffsets = new short[3];
        public double[] gyroCoeff = new double[3];
        public short[] gyroSens = new short[3];
        public double[] gyroSensMulti = new double[3];
        private byte[] inputReportBuffer;
        private JoyConDevice jointDevice;

        private readonly ushort[] leftStickCalib = new ushort[6];
        private readonly ushort leftStickOffsetX = 0;
        private readonly ushort leftStickOffsetY = 0;

        private StickAxisData leftStickXData;
        private StickAxisData leftStickYData;

        private readonly ReaderWriterLockSlim lockSlim = new();

        private JoyConControllerOptions nativeOptionsStore;
        private byte[] outputReportBuffer;

        private readonly ushort[] rightStickCalib = new ushort[6];
        private readonly ushort rightStickOffsetX = 0;
        private readonly ushort rightStickOffsetY = 0;
        private StickAxisData rightStickXData;
        private StickAxisData rightStickYData;
        private byte[] rumbleReportBuffer;

        public JoyConDevice(HidDevice hidDevice,
            string disName, VidPidFeatureSet featureSet = VidPidFeatureSet.DefaultDS4) :
            base(hidDevice, disName, featureSet)
        {
            runCalib = false;
            synced = true;
            DeviceSlotNumberChanged += (sender, e) => { CalculateDeviceSlotMask(); };

            Removal += JoyConDevice_Removal;
        }

        public byte FrameCount { get; set; }
        public JoyConSide SideType { get; private set; }

        public int InputReportLen => INPUT_REPORT_LEN;
        public int OutputReportLen => OUTPUT_REPORT_LEN;
        public int RumbleReportLen => RUMBLE_REPORT_LEN;
        public double CombLatency { get; set; }

        public bool EnableHomeLED { get; set; } = true;

        public JoyConDevice JointDevice
        {
            get => jointDevice;
            set
            {
                jointDevice = value;
                if (jointDevice == null)
                {
                }
            }
        }

        public override int JointDeviceSlotNumber
        {
            get
            {
                var result = -1;
                if (jointDevice != null) result = jointDevice.deviceSlotNumber;

                return result;
            }
        }

        public override event ReportHandler<EventArgs> Report;
        public override event EventHandler<EventArgs> Removal;
        public override event EventHandler BatteryChanged;
        public override event EventHandler ChargingChanged;

        private void JoyConDevice_Removal(object sender, EventArgs e)
        {
            connectionOpened = false;
        }

        private JoyConSide DetermineSideType()
        {
            var result = JoyConSide.None;
            var productId = hDevice.Attributes.ProductId;
            if (productId == JOYCON_L_PRODUCT_ID)
                result = JoyConSide.Left;
            else if (productId == JOYCON_R_PRODUCT_ID) result = JoyConSide.Right;

            return result;
        }

        public override void PostInit()
        {
            SideType = DetermineSideType();
            if (SideType == JoyConSide.Left)
                deviceType = InputDeviceType.JoyConL;
            else if (SideType == JoyConSide.Right) deviceType = InputDeviceType.JoyConR;

            conType = ConnectionType.BT;
            warnInterval = WARN_INTERVAL_BT;

            gyroMouseSensSettings = new GyroMouseSens();
            OptionsStore = nativeOptionsStore = new JoyConControllerOptions();
            SetupOptionsEvents();

            inputReportBuffer = new byte[INPUT_REPORT_LEN];
            outputReportBuffer = new byte[OUTPUT_REPORT_LEN];
            rumbleReportBuffer = new byte[RUMBLE_REPORT_LEN];

            if (!hDevice.IsFileStreamOpen()) hDevice.OpenFileStream(inputReportBuffer.Length);
        }

        public static ConnectionType DetermineConnectionType(HidDevice hDevice)
        {
            var result = ConnectionType.BT;
            return result;
        }

        public override void StartUpdate()
        {
            inputReportErrorCount = 0;

            try
            {
                SetOperational();
            }
            catch (IOException)
            {
                AppLogger.Instance.LogToGui($"Controller {MacAddress} failed to initialize. Closing device", true);
            }

            if (!connectionOpened)
            {
                // Failed to open device. Tell app to consider device detached
                isDisconnecting = true;
                Removal?.Invoke(this, EventArgs.Empty);
            }
            else if (ds4Input == null)
            {
                ds4Input = new Thread(ReadInput);
                ds4Input.IsBackground = true;
                ds4Input.Priority = ThreadPriority.AboveNormal;
                ds4Input.Name = "JoyCon Reader Thread";
                ds4Input.Start();
            }
        }

        protected override void StopOutputUpdate()
        {
        }

        protected void ReadInput()
        {
            byte[] stick_raw = { 0, 0, 0 };
            byte[] stick_raw2 = { 0, 0, 0 };
            short[] accel_raw = { 0, 0, 0 };
            var gyro_raw = new short[9];
            var gyro_out = new short[9];
            //short gyroYaw = 0, gyroYaw2 = 0, gyroYaw3 = 0;
            //short gyroPitch = 0, gyroPitch2 = 0, gyroPitch3 = 0;
            //short gyroRoll = 0, gyroRoll2 = 0, gyroRoll3 = 0;
            short tempShort = 0;
            //int tempAxis = 0;
            var tempAxisX = 0;
            var tempAxisY = 0;

            /*long currentTime = 0;
            long previousTime = 0;
            long deltaElapsed = 0;
            double lastElapsed;
            double tempTimeElapsed;
            bool firstReport = true;
            */

            unchecked
            {
                firstActive = DateTime.UtcNow;
                NativeMethods.HidD_SetNumInputBuffers(hDevice.safeReadHandle.DangerousGetHandle(), 3);
                var latencyQueue = new Queue<long>(21); // Set capacity at max + 1 to avoid any resizing
                var tempLatencyCount = 0;
                long oldtime = 0;
                var currerror = string.Empty;
                long curtime = 0;
                long testelapsed = 0;
                timeoutEvent = false;
                ds4InactiveFrame = true;
                idleInput = true;
                var syncWriteReport = conType != ConnectionType.BT;
                //bool forceWrite = false;

                //int maxBatteryValue = 0;
                var tempBattery = 0;
                var tempCharging = charging;
                //uint tempStamp = 0;
                var elapsedDeltaTime = 0.0;
                //uint tempDelta = 0;
                byte tempByte = 0;
                long latencySum = 0;

                // Run continuous calibration on Gyro when starting input loop
                sixAxis.ResetContinuousCalibration();
                standbySw.Start();

                while (!exitInputThread)
                {
#if WITH_TRACING
                    using var scope = GlobalTracer.Instance.BuildSpan($"{nameof(JoyConDevice)}::{nameof(ReadInput)}")
                        .StartActive(true);
#endif

                    oldCharging = charging;
                    currerror = string.Empty;

                    readWaitEv.Set();

                    var res = hDevice.ReadWithFileStream(inputReportBuffer);
                    if (res == HidDevice.ReadStatus.Success)
                    {
                        if (inputReportBuffer[0] != 0x30)
                        {
                            //Console.WriteLine("Got unexpected input report id 0x{0:X2}. Try again",
                            //    inputReportBuffer[0]);

                            readWaitEv.Reset();
                            inputReportErrorCount++;
                            if (inputReportErrorCount > 10)
                            {
                                exitInputThread = true;
                                isDisconnecting = true;
                                Removal?.Invoke(this, EventArgs.Empty);
                            }

                            continue;
                        }
                    }
                    else
                    {
                        readWaitEv.Reset();
                        exitInputThread = true;
                        isDisconnecting = true;
                        Removal?.Invoke(this, EventArgs.Empty);
                        continue;
                    }

                    readWaitEv.Wait();
                    readWaitEv.Reset();

                    inputReportErrorCount = 0;
                    curtime = Stopwatch.GetTimestamp();
                    testelapsed = curtime - oldtime;
                    lastTimeElapsedDouble = testelapsed * (1.0 / Stopwatch.Frequency) * 1000.0;
                    lastTimeElapsed = (long)lastTimeElapsedDouble;
                    oldtime = curtime;
                    elapsedDeltaTime = lastTimeElapsedDouble * .001;
                    CombLatency += elapsedDeltaTime;

                    if (elapsedDeltaTime <= 0.005) continue;

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

                    utcNow = DateTime.UtcNow; // timestamp with UTC in case system time zone changes
                    currentState.PacketCounter = pState.PacketCounter + 1;
                    // DS4 Frame Counter range is [0-127]
                    currentState.FrameCounter = (byte)(currentState.PacketCounter % 128);
                    currentState.ReportTimeStamp = utcNow;

                    currentState.elapsedTime = CombLatency;
                    currentState.totalMicroSec = pState.totalMicroSec + (uint)(CombLatency * 1000000);
                    CombLatency = 0.0;

                    if ((featureSet & VidPidFeatureSet.NoBatteryReading) == 0)
                    {
                        tempByte = inputReportBuffer[2];
                        // Strip out LSB from high nibble. Used as Charging flag and will be checked later
                        tempBattery = ((tempByte & 0xE0) >> 4) * 100 / 8;
                        tempBattery = Math.Min(tempBattery, 100);
                        if (tempBattery != battery)
                        {
                            battery = tempBattery;
                            BatteryChanged?.Invoke(this, EventArgs.Empty);
                        }

                        currentState.Battery = (byte)tempBattery;

                        tempCharging = (tempByte & 0x10) != 0;
                        if (tempCharging != charging)
                        {
                            charging = tempCharging;
                            ChargingChanged?.Invoke(this, EventArgs.Empty);
                        }
                    }
                    else
                    {
                        battery = 99;
                        currentState.Battery = 99;
                    }

                    if (SideType == JoyConSide.Left)
                    {
                        tempByte = inputReportBuffer[4];
                        currentState.Share = (tempByte & 0x01) != 0;
                        currentState.L3 = (tempByte & 0x08) != 0;
                        // Capture Button
                        //cState.PS = (tempByte & 0x20) != 0;
                        currentState.Capture = (tempByte & 0x20) != 0;

                        tempByte = inputReportBuffer[5];
                        currentState.DpadUp = (tempByte & 0x02) != 0;
                        currentState.DpadDown = (tempByte & 0x01) != 0;
                        currentState.DpadLeft = (tempByte & 0x08) != 0;
                        currentState.DpadRight = (tempByte & 0x04) != 0;
                        currentState.L1 = (tempByte & 0x40) != 0;
                        currentState.L2Btn = (tempByte & 0x80) != 0;
                        currentState.L2 = (byte)(currentState.L2Btn ? 255 : 0);
                        currentState.SideL = (tempByte & 0x20) != 0;
                        currentState.SideR = (tempByte & 0x10) != 0;

                        stick_raw[0] = inputReportBuffer[6];
                        stick_raw[1] = inputReportBuffer[7];
                        stick_raw[2] = inputReportBuffer[8];

                        tempAxisX = (stick_raw[0] | ((stick_raw[1] & 0x0F) << 8)) - leftStickOffsetX;
                        tempAxisX = tempAxisX > leftStickXData.max ? leftStickXData.max :
                            tempAxisX < leftStickXData.min ? leftStickXData.min : tempAxisX;
                        currentState.LX = (byte)((tempAxisX - leftStickXData.min) /
                            (double)(leftStickXData.max - leftStickXData.min) * 255);

                        tempAxisY = ((stick_raw[1] >> 4) | (stick_raw[2] << 4)) - leftStickOffsetY;
                        tempAxisY = tempAxisY > leftStickYData.max ? leftStickYData.max :
                            tempAxisY < leftStickYData.min ? leftStickYData.min : tempAxisY;
                        currentState.LY =
                            (byte)((((tempAxisY - leftStickYData.min) /
                                (double)(leftStickYData.max - leftStickYData.min) - 0.5) * -1.0 + 0.5) * 255);

                        // JoyCon on its side flips axes and directions
                        //cState.LY = JoyConStickAdjust(tempAxisX, leftStickXData.mid, leftStickXData.max - leftStickXData.min, -1);
                        //cState.LX = JoyConStickAdjust(tempAxisY, leftStickYData.mid, leftStickYData.max - leftStickYData.min, -1);
                    }
                    else if (SideType == JoyConSide.Right)
                    {
                        tempByte = inputReportBuffer[3];
                        currentState.Circle = (tempByte & 0x08) != 0;
                        currentState.Cross = (tempByte & 0x04) != 0;
                        currentState.Triangle = (tempByte & 0x02) != 0;
                        currentState.Square = (tempByte & 0x01) != 0;
                        currentState.R1 = (tempByte & 0x40) != 0;
                        currentState.R2Btn = (tempByte & 0x80) != 0;
                        currentState.R2 = (byte)(currentState.R2Btn ? 255 : 0);
                        currentState.SideL = (tempByte & 0x20) != 0;
                        currentState.SideR = (tempByte & 0x10) != 0;

                        tempByte = inputReportBuffer[4];
                        currentState.Options = (tempByte & 0x02) != 0;
                        currentState.R3 = (tempByte & 0x04) != 0;
                        currentState.PS = (tempByte & 0x10) != 0;

                        stick_raw2[0] = inputReportBuffer[9];
                        stick_raw2[1] = inputReportBuffer[10];
                        stick_raw2[2] = inputReportBuffer[11];

                        tempAxisX = (stick_raw2[0] | ((stick_raw2[1] & 0x0F) << 8)) - rightStickOffsetX;
                        tempAxisX = tempAxisX > rightStickXData.max ? rightStickXData.max :
                            tempAxisX < rightStickXData.min ? rightStickXData.min : tempAxisX;
                        currentState.RX = (byte)((tempAxisX - rightStickXData.min) /
                            (double)(rightStickXData.max - rightStickXData.min) * 255);

                        tempAxisY = ((stick_raw2[1] >> 4) | (stick_raw2[2] << 4)) - rightStickOffsetY;
                        tempAxisY = tempAxisY > rightStickYData.max ? rightStickYData.max :
                            tempAxisY < rightStickYData.min ? rightStickYData.min : tempAxisY;
                        currentState.RY =
                            (byte)((((tempAxisY - rightStickYData.min) /
                                (double)(rightStickYData.max - rightStickYData.min) - 0.5) * -1.0 + 0.5) * 255);

                        // JoyCon on its side flips axes
                        //cState.LY = JoyConStickAdjust(tempAxisX, rightStickXData.mid, rightStickXData.max - rightStickXData.min, -1);
                        //cState.LX = JoyConStickAdjust(tempAxisY, rightStickYData.mid, rightStickYData.max - rightStickYData.min, -1);
                    }

                    for (var i = 0; i < 3; i++)
                    {
                        var data_offset = i * 12;
                        var gyro_offset = i * 3;
                        accel_raw[IMU_XAXIS_IDX] = (short)((ushort)(inputReportBuffer[16 + data_offset] << 8) |
                                                           inputReportBuffer[15 + data_offset]);
                        accel_raw[IMU_YAXIS_IDX] = (short)((ushort)(inputReportBuffer[14 + data_offset] << 8) |
                                                           inputReportBuffer[13 + data_offset]);
                        accel_raw[IMU_ZAXIS_IDX] = (short)((ushort)(inputReportBuffer[18 + data_offset] << 8) |
                                                           inputReportBuffer[17 + data_offset]);

                        tempShort = gyro_raw[IMU_YAW_IDX + gyro_offset] =
                            (short)((ushort)(inputReportBuffer[24 + data_offset] << 8) |
                                    inputReportBuffer[23 + data_offset]);
                        //gyro_out[IMU_YAW_IDX + gyro_offset] = (short)(tempShort - device.gyroBias[IMU_YAW_IDX]);
                        gyro_out[IMU_YAW_IDX + gyro_offset] = tempShort;

                        tempShort = gyro_raw[IMU_PITCH_IDX + gyro_offset] =
                            (short)((ushort)(inputReportBuffer[22 + data_offset] << 8) |
                                    inputReportBuffer[21 + data_offset]);
                        //gyro_out[IMU_PITCH_IDX + gyro_offset] = (short)(tempShort - device.gyroBias[IMU_PITCH_IDX]);
                        gyro_out[IMU_PITCH_IDX + gyro_offset] = tempShort;

                        tempShort = gyro_raw[IMU_ROLL_IDX + gyro_offset] =
                            (short)((ushort)(inputReportBuffer[20 + data_offset] << 8) |
                                    inputReportBuffer[19 + data_offset]);
                        //gyro_out[IMU_ROLL_IDX + gyro_offset] = (short)(tempShort - device.gyroBias[IMU_ROLL_IDX]);
                        gyro_out[IMU_ROLL_IDX + gyro_offset] = tempShort;

                        //Console.WriteLine($"IDX: ({i}) Accel: X({accel_raw[IMU_XAXIS_IDX]}) Y({accel_raw[IMU_YAXIS_IDX]}) Z({accel_raw[IMU_ZAXIS_IDX]})");
                        //Console.WriteLine($"IDX: ({i}) Gyro: Yaw({gyro_raw[IMU_YAW_IDX + gyro_offset]}) Pitch({gyro_raw[IMU_PITCH_IDX + gyro_offset]}) Roll({gyro_raw[IMU_ROLL_IDX + gyro_offset]})");
                        //Console.WriteLine($"IDX: ({i}) Gyro OUT: Yaw({gyro_out[IMU_YAW_IDX + gyro_offset]}) Pitch({gyro_out[IMU_PITCH_IDX + gyro_offset]}) Roll({gyro_out[IMU_ROLL_IDX + gyro_offset]})");
                        //Console.WriteLine();

                        //if (sideType == JoyConSide.Right)
                        //{
                        //    Console.WriteLine($"IDX: ({i}) Accel: X({accel_raw[IMU_XAXIS_IDX]}) Y({accel_raw[IMU_YAXIS_IDX]}) Z({accel_raw[IMU_ZAXIS_IDX]})");
                        //    Console.WriteLine($"IDX: ({i}) Gyro: Yaw({gyro_raw[IMU_YAW_IDX + gyro_offset]}) Pitch({gyro_raw[IMU_PITCH_IDX + gyro_offset]}) Roll({gyro_raw[IMU_ROLL_IDX + gyro_offset]})");
                        //    Console.WriteLine($"IDX: ({i}) Gyro OUT: Yaw({gyro_out[IMU_YAW_IDX + gyro_offset]}) Pitch({gyro_out[IMU_PITCH_IDX + gyro_offset]}) Roll({gyro_out[IMU_ROLL_IDX + gyro_offset]})");
                        //    Console.WriteLine();
                        //}
                    }

                    // For Accel, just use most recent sampled values
                    int accelX = accel_raw[IMU_XAXIS_IDX];
                    int accelY = accel_raw[IMU_YAXIS_IDX];
                    int accelZ = accel_raw[IMU_ZAXIS_IDX];

                    // Just use most recent sample for now
                    int gyroYaw = (short)(-1 * (gyro_out[6 + IMU_YAW_IDX] - gyroBias[IMU_YAW_IDX] +
                                                gyroCalibOffsets[IMU_YAW_IDX]));
                    int gyroPitch = (short)(gyro_out[6 + IMU_PITCH_IDX] - gyroBias[IMU_PITCH_IDX] -
                                            gyroCalibOffsets[IMU_PITCH_IDX]);
                    int gyroRoll = (short)(gyro_out[6 + IMU_ROLL_IDX] - gyroBias[IMU_ROLL_IDX] -
                                           gyroCalibOffsets[IMU_ROLL_IDX]);
                    //cState.Motion.populate(gyroYaw, gyroPitch, gyroRoll, accelX, accelY, accelZ, cState.elapsedTime, pState.Motion);

                    // Need to populate the SixAxis object manually to work around conversions
                    //Console.WriteLine("GyroYaw: {0}", gyroYaw);
                    var tempMotion = currentState.Motion;
                    // Perform continous calibration routine with raw values
                    sixAxis.PrepareNonDS4SixAxis(ref gyroYaw, ref gyroPitch, ref gyroRoll,
                        ref accelX, ref accelY, ref accelZ);

                    // JoyCon Right axes are inverted. Adjust axes directions
                    if (SideType == JoyConSide.Right)
                    {
                        accelX *= -1;
                        accelZ *= -1; // accelY *= -1;
                        gyroYaw *= -1;
                        gyroPitch *= -1; //gyroRoll *= -1;
                    }

                    tempMotion.gyroYawFull = gyroYaw;
                    tempMotion.gyroPitchFull = -gyroPitch;
                    tempMotion.gyroRollFull = gyroRoll;
                    tempMotion.accelXFull = accelX * 2;
                    tempMotion.accelYFull = -accelZ * 2;
                    tempMotion.accelZFull = -accelY * 2;

                    tempMotion.elapsed = elapsedDeltaTime;
                    tempMotion.previousAxis = pState.Motion;
                    tempMotion.gyroYaw = gyroYaw / 256;
                    tempMotion.gyroPitch = -gyroPitch / 256;
                    tempMotion.gyroRoll = gyroRoll / 256;
                    tempMotion.accelX = accelX / 31;
                    tempMotion.accelY = -accelZ / 31;
                    tempMotion.accelZ = -accelY / 31;
                    //tempMotion.outputAccelX = tempMotion.accelX; tempMotion.outputAccelY = tempMotion.accelY; tempMotion.outputAccelZ = tempMotion.accelZ;
                    tempMotion.outputAccelX = 0;
                    tempMotion.outputAccelY = 0;
                    tempMotion.outputAccelZ = 0;
                    tempMotion.outputGyroControls = false;
                    tempMotion.accelXG = accelX * 2 / DS4Windows.SixAxis.F_ACC_RES_PER_G;
                    tempMotion.accelYG = -accelZ * 2 / DS4Windows.SixAxis.F_ACC_RES_PER_G;
                    tempMotion.accelZG = -accelY * 2 / DS4Windows.SixAxis.F_ACC_RES_PER_G;

                    tempMotion.angVelYaw = gyroYaw * GYRO_IN_DEG_SEC_FACTOR;
                    tempMotion.angVelPitch = -gyroPitch * GYRO_IN_DEG_SEC_FACTOR;
                    tempMotion.angVelRoll = gyroRoll * GYRO_IN_DEG_SEC_FACTOR;

                    var args = new SixAxisEventArgs(currentState.ReportTimeStamp, currentState.Motion);
                    sixAxis.FireSixAxisEvent(args);

                    if (conType == ConnectionType.USB)
                    {
                        if (idleTimeout == 0)
                        {
                            lastActive = utcNow;
                        }
                        else
                        {
                            idleInput = isDS4Idle();
                            if (!idleInput) lastActive = utcNow;
                        }
                    }
                    else
                    {
                        var shouldDisconnect = false;
                        if (!isRemoved && idleTimeout > 0)
                        {
                            idleInput = isDS4Idle();
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

                            if (conType == ConnectionType.BT)
                                if (DisconnectBT(true))
                                {
                                    timeoutExecuted = true;
                                    return; // all done
                                }
                        }
                    }

                    Report?.Invoke(this, EventArgs.Empty);
                    WriteReport();

                    //forceWrite = false;

                    if (!string.IsNullOrEmpty(currerror))
                        error = currerror;
                    else if (!string.IsNullOrEmpty(error))
                        error = string.Empty;

                    pState.Motion.CopyFrom(currentState.Motion);
                    currentState.CopyTo(pState);

                    if (hasInputEvts)
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
        }

        private void SetOperational()
        {
            // Set device to normal power state
            byte[] powerChoiceArray = { 0x00 };
            Subcommand(SwitchProSubCmd.SET_LOW_POWER_STATE, powerChoiceArray, 1, true);

            if (SideType == JoyConSide.Right && EnableHomeLED)
            {
                // Turn on Home light (Solid)
                var light = Enumerable.Repeat((byte)0xFF, 25).ToArray();
                light[0] = 0x1F;
                light[1] = 0xF0;
                //Thread.Sleep(1000);
                Subcommand(0x38, light, 25, true);
            }

            // Turn on bottom LEDs
            byte[] leds = { deviceSlotMask };
            //Thread.Sleep(1000);
            Subcommand(0x30, leds, 1, true);

            // Enable Gyro
            byte[] imuEnable = { 0x01 };
            //Thread.Sleep(1000);
            Subcommand(0x40, imuEnable, 1, true);

            // Enable High Performance Gyro mode
            byte[] gyroModeBuffer = { 0x03, 0x00, 0x00, 0x01 };
            //Thread.Sleep(1000);
            Subcommand(0x41, gyroModeBuffer, 4, true);

            // Enable Rumble
            byte[] rumbleEnable = { 0x01 };
            // Disable Rumble
            //byte[] rumbleEnable = new byte[] { 0x00 };
            //Thread.Sleep(1000);
            Subcommand(0x48, rumbleEnable, 1, true);

            if (SideType == JoyConSide.Right)
            {
                // Suspend NFC/IR MCU state. Don't know if it will matter
                Console.WriteLine("RESET NFC/IR MCU");
                byte[] shitBuffer = { 0x01 };
                Subcommand(0x20, shitBuffer, 0, true);
                Thread.Sleep(1000);
            }

            //Thread.Sleep(1000);
            EnableFastPollRate();
            SetInitRumble();
            Thread.Sleep(500);
            CalibrationData();

            Console.WriteLine("FINISHED");

            //if (connectionType == ConnectionType.USB)
            //{
            //    Thread.Sleep(300);
            //    //SetInitRumble();
            //}

            connectionOpened = true;
        }

        private void EnableFastPollRate()
        {
            // Enable fatest poll rate
            byte[] tempArray = { 0x30 };
            Subcommand(SwitchProSubCmd.SET_INPUT_MODE, tempArray, 1, true);
            //Thread.Sleep(1000);
        }

        public void SetInitRumble()
        {
            bool result;
            //HidDevice.ReadStatus res;
            //byte[] tmpReport = new byte[64];
            var rumble_data = new byte[8];
            rumble_data[0] = 0x0;
            rumble_data[1] = 0x1;
            rumble_data[2] = 0x40;
            rumble_data[3] = 0x40;

            for (var i = 0; i < 4; i++) rumble_data[4 + i] = rumble_data[i];

            var tmpRumble = new byte[RUMBLE_REPORT_LEN];
            Array.Copy(rumble_data, 0, tmpRumble, 2, rumble_data.Length);
            tmpRumble[0] = 0x10;
            tmpRumble[1] = FrameCount;
            FrameCount = (byte)(++FrameCount & 0x0F);

            result = hDevice.WriteOutputReportViaInterrupt(tmpRumble, 0);
            hDevice.fileStream.Flush();
            //res = hidDevice.ReadWithFileStream(tmpReport, 500);
            //res = hidDevice.ReadFile(tmpReport);
        }

        private byte[] Subcommand(byte subcommand, byte[] tmpBuffer, uint bufLen,
            bool checkResponse = false)
        {
            bool result;
            var commandBuffer = new byte[SUBCOMMAND_BUFFER_LEN];
            Array.Copy(commandBuffHeader, 0, commandBuffer, 2, SUBCOMMAND_HEADER_LEN);
            Array.Copy(tmpBuffer, 0, commandBuffer, 11, bufLen);

            commandBuffer[0] = 0x01;
            commandBuffer[1] = FrameCount;
            FrameCount = (byte)(++FrameCount & 0x0F);
            commandBuffer[10] = subcommand;

            result = hDevice.WriteOutputReportViaInterrupt(commandBuffer, 0);
            hDevice.fileStream.Flush();

            byte[] tmpReport = null;
            if (result && checkResponse)
            {
                tmpReport = new byte[INPUT_REPORT_LEN];
                HidDevice.ReadStatus res;
                res = hDevice.ReadWithFileStream(tmpReport, SUBCOMMAND_RESPONSE_TIMEOUT);
                var tries = 1;
                while (res == HidDevice.ReadStatus.Success &&
                       tmpReport[0] != 0x21 && tmpReport[14] != subcommand && tries < 100)
                {
                    //Console.WriteLine("TRY AGAIN: {0}", tmpReport[0]);
                    res = hDevice.ReadWithFileStream(tmpReport, SUBCOMMAND_RESPONSE_TIMEOUT);
                    tries++;
                }

                //Console.WriteLine("END GAME: {0} {1} {2}", subcommand, tmpReport[0], tries);
            }

            return tmpReport;
        }

        public void WriteReport()
        {
            MergeStates();

            var dirty = false;
            double tempRatio;
            if (SideType == JoyConSide.Left)
            {
                tempRatio = CurrentHaptics.RumbleState.RumbleMotorStrengthLeftHeavySlow / 255.0;
                dirty = tempRatio != 0 || tempRatio != currentLeftAmpRatio;
                currentLeftAmpRatio = tempRatio;
            }
            else if (SideType == JoyConSide.Right)
            {
                tempRatio = CurrentHaptics.RumbleState.RumbleMotorStrengthRightLightFast / 255.0;
                dirty = tempRatio != 0 || tempRatio != currentRightAmpRatio;
                currentRightAmpRatio = tempRatio;
            }

            if (dirty)
            {
                PrepareRumbleData(rumbleReportBuffer);
                var result = hDevice.WriteOutputReportViaInterrupt(rumbleReportBuffer, 100);
            }
        }

        public override bool IsAlive()
        {
            return !isDisconnecting && connectionOpened;
        }

        public void PrepareRumbleData(byte[] buffer)
        {
            //Array.Copy(commandBuffHeader, 0, buffer, 2, SUBCOMMAND_HEADER_LEN);
            buffer[0] = 0x10;
            buffer[1] = FrameCount;
            FrameCount = (byte)(++FrameCount & 0x0F);

            ushort freq_data_high = 0x0001; // 320
            byte freq_data_low = 0x40; // 160

            int idx;
            byte amp_high;
            ushort amp_low;

            if (SideType == JoyConSide.Left)
            {
                idx = (int)(currentLeftAmpRatio * AMP_LIMIT_MAX);
                var entry = compiledRumbleTable[idx];
                amp_high = entry.high;
                amp_low = entry.low;

                buffer[2] = (byte)((freq_data_high >> 8) & 0xFF); // 0
                buffer[3] = (byte)((freq_data_high & 0xFF) + amp_high); // 1
                buffer[4] = (byte)((freq_data_low + (amp_low >> 8)) & 0xFF); // 2
                buffer[5] = (byte)(amp_low & 0xFF); // 3
            }
            else if (SideType == JoyConSide.Right)
            {
                idx = (int)(currentRightAmpRatio * AMP_LIMIT_MAX);
                var entry = compiledRumbleTable[idx];
                amp_high = entry.high;
                amp_low = entry.low;
                buffer[6] = (byte)((freq_data_high >> 8) & 0xFF); // 4
                buffer[7] = (byte)((freq_data_high & 0xFF) + amp_high); // 5
                buffer[8] = (byte)((freq_data_low + (amp_low >> 8)) & 0xFF); // 6
                buffer[9] = (byte)(amp_low & 0xFF); // 7
            }

            //byte amp_high = 0x9a; // 609
            //ushort amp_low = 0x8066; // 609
            //buffer[2] = 0x28; // 0
            //buffer[3] = 0xc8; // 1
            //buffer[4] = 0x81; // 2
            //buffer[5] = 0x71; // 3

            //buffer[6] = 0x28; // 4
            //buffer[7] = 0xc8; // 5
            //buffer[8] = 0x81; // 6
            //buffer[9] = 0x71; // 7

            //Console.WriteLine("RUMBLE BUFF: {0}", string.Join(", ", buffer));
            //Console.WriteLine("RUMBLE BUFF: {0}",
            //    string.Concat(buffer.Select(i => string.Format("{0:x2} ", i))));
        }

        public void CalibrationData()
        {
            const int SPI_RESP_OFFSET = 20;
            byte[] command;
            byte[] tmpBuffer;

            //command = new byte[] { 0x00, 0x50, 0x00, 0x00, 0x01 };
            //tmpBuffer = Subcommand(SwitchProSubCmd.SPI_FLASH_READ, command, 5, checkResponse: true);
            //Console.WriteLine("THE POWER");
            //Console.WriteLine(string.Join(",", tmpBuffer));
            //Console.WriteLine(tmpBuffer[SPI_RESP_OFFSET]);
            //Console.WriteLine();

            var foundUserCalib = false;

            if (SideType == JoyConSide.Left)
            {
                command = new byte[] { 0x10, 0x80, 0x00, 0x00, 0x02 };
                tmpBuffer = Subcommand(SwitchProSubCmd.SPI_FLASH_READ, command, 5, true);
                if (tmpBuffer[SPI_RESP_OFFSET] == 0xB2 && tmpBuffer[SPI_RESP_OFFSET + 1] == 0xA1) foundUserCalib = true;

                if (foundUserCalib)
                {
                    command = new byte[] { 0x12, 0x80, 0x00, 0x00, 0x09 };
                    tmpBuffer = Subcommand(SwitchProSubCmd.SPI_FLASH_READ, command, 5, true);
                    //Console.WriteLine("FOUND USER CALIB");
                }
                else
                {
                    command = new byte[] { 0x3D, 0x60, 0x00, 0x00, 0x09 };
                    tmpBuffer = Subcommand(SwitchProSubCmd.SPI_FLASH_READ, command, 5, true);
                    //Console.WriteLine("CHECK FACTORY CALIB");
                }

                leftStickCalib[0] =
                    (ushort)(((tmpBuffer[1 + SPI_RESP_OFFSET] << 8) & 0xF00) |
                             tmpBuffer[0 + SPI_RESP_OFFSET]); // X Axis Max above center
                leftStickCalib[1] =
                    (ushort)((tmpBuffer[2 + SPI_RESP_OFFSET] << 4) |
                             (tmpBuffer[1 + SPI_RESP_OFFSET] >> 4)); // Y Axis Max above center
                leftStickCalib[2] =
                    (ushort)(((tmpBuffer[4 + SPI_RESP_OFFSET] << 8) & 0xF00) |
                             tmpBuffer[3 + SPI_RESP_OFFSET]); // X Axis Center
                leftStickCalib[3] =
                    (ushort)((tmpBuffer[5 + SPI_RESP_OFFSET] << 4) |
                             (tmpBuffer[4 + SPI_RESP_OFFSET] >> 4)); // Y Axis Center
                leftStickCalib[4] =
                    (ushort)(((tmpBuffer[7 + SPI_RESP_OFFSET] << 8) & 0xF00) |
                             tmpBuffer[6 + SPI_RESP_OFFSET]); // X Axis Min below center
                leftStickCalib[5] =
                    (ushort)((tmpBuffer[8 + SPI_RESP_OFFSET] << 4) |
                             (tmpBuffer[7 + SPI_RESP_OFFSET] >> 4)); // Y Axis Min below center

                if (foundUserCalib)
                {
                    leftStickXData.max = (ushort)(leftStickCalib[0] + leftStickCalib[2]);
                    leftStickXData.mid = leftStickCalib[2];
                    leftStickXData.min = (ushort)(leftStickCalib[2] - leftStickCalib[4]);

                    leftStickYData.max = (ushort)(leftStickCalib[1] + leftStickCalib[3]);
                    leftStickYData.mid = leftStickCalib[3];
                    leftStickYData.min = (ushort)(leftStickCalib[3] - leftStickCalib[5]);
                }
                else
                {
                    leftStickXData.max = (ushort)((leftStickCalib[0] + leftStickCalib[2]) * STICK_AXIS_LS_X_MAX_CUTOFF);
                    leftStickXData.min = (ushort)((leftStickCalib[2] - leftStickCalib[4]) * STICK_AXIS_LS_X_MIN_CUTOFF);
                    //leftStickXData.mid = leftStickCalib[2];
                    leftStickXData.mid = (ushort)((leftStickXData.max - leftStickXData.min) / 2.0 + leftStickXData.min);

                    leftStickYData.max = (ushort)((leftStickCalib[1] + leftStickCalib[3]) * STICK_AXIS_LS_Y_MAX_CUTOFF);
                    leftStickYData.min = (ushort)((leftStickCalib[3] - leftStickCalib[5]) * STICK_AXIS_LS_Y_MIN_CUTOFF);
                    //leftStickYData.mid = leftStickCalib[3];
                    leftStickYData.mid = (ushort)((leftStickYData.max - leftStickYData.min) / 2.0 + leftStickYData.min);
                    //leftStickOffsetX = leftStickOffsetY = 140;
                }

                //leftStickOffsetX = leftStickCalib[2];
                //leftStickOffsetY = leftStickCalib[3];

                //Console.WriteLine(string.Join(",", tmpBuffer));
                //Console.WriteLine();
                //Console.WriteLine(string.Join(",", leftStickCalib));

                /*
                // Grab Factory LS Dead Zone
                command = new byte[] { 0x86, 0x60, 0x00, 0x00, 0x10 };
                byte[] deadZoneBuffer = Subcommand(SwitchProSubCmd.SPI_FLASH_READ, command, 5, checkResponse: true);
                deadzoneLS = (ushort)((deadZoneBuffer[4 + SPI_RESP_OFFSET] << 8) & 0xF00 | deadZoneBuffer[3 + SPI_RESP_OFFSET]);
                //Console.WriteLine("DZ Left: {0}", deadzoneLS);
                //Console.WriteLine(string.Join(",", deadZoneBuffer));
                */
            }
            else if (SideType == JoyConSide.Right)
            {
                foundUserCalib = false;
                command = new byte[] { 0x1B, 0x80, 0x00, 0x00, 0x02 };
                tmpBuffer = Subcommand(SwitchProSubCmd.SPI_FLASH_READ, command, 5, true);
                if (tmpBuffer[SPI_RESP_OFFSET] == 0xB2 && tmpBuffer[SPI_RESP_OFFSET + 1] == 0xA1) foundUserCalib = true;

                if (foundUserCalib)
                {
                    command = new byte[] { 0x1D, 0x80, 0x00, 0x00, 0x09 };
                    tmpBuffer = Subcommand(SwitchProSubCmd.SPI_FLASH_READ, command, 5, true);
                    //Console.WriteLine("FOUND RIGHT USER CALIB");
                }
                else
                {
                    command = new byte[] { 0x46, 0x60, 0x00, 0x00, 0x09 };
                    tmpBuffer = Subcommand(SwitchProSubCmd.SPI_FLASH_READ, command, 5, true);
                    //Console.WriteLine("CHECK RIGHT FACTORY CALIB");
                }

                rightStickCalib[2] =
                    (ushort)(((tmpBuffer[1 + SPI_RESP_OFFSET] << 8) & 0xF00) |
                             tmpBuffer[0 + SPI_RESP_OFFSET]); // X Axis Center
                rightStickCalib[3] =
                    (ushort)((tmpBuffer[2 + SPI_RESP_OFFSET] << 4) |
                             (tmpBuffer[1 + SPI_RESP_OFFSET] >> 4)); // Y Axis Center
                rightStickCalib[4] =
                    (ushort)(((tmpBuffer[4 + SPI_RESP_OFFSET] << 8) & 0xF00) |
                             tmpBuffer[3 + SPI_RESP_OFFSET]); // X Axis Min below center
                rightStickCalib[5] =
                    (ushort)((tmpBuffer[5 + SPI_RESP_OFFSET] << 4) |
                             (tmpBuffer[4 + SPI_RESP_OFFSET] >> 4)); // Y Axis Min below center
                rightStickCalib[0] =
                    (ushort)(((tmpBuffer[7 + SPI_RESP_OFFSET] << 8) & 0xF00) |
                             tmpBuffer[6 + SPI_RESP_OFFSET]); // X Axis Max above center
                rightStickCalib[1] =
                    (ushort)((tmpBuffer[8 + SPI_RESP_OFFSET] << 4) |
                             (tmpBuffer[7 + SPI_RESP_OFFSET] >> 4)); // Y Axis Max above center

                if (foundUserCalib)
                {
                    rightStickXData.max = (ushort)(rightStickCalib[2] + rightStickCalib[4]);
                    rightStickXData.mid = rightStickCalib[2];
                    rightStickXData.min = (ushort)(rightStickCalib[2] - rightStickCalib[0]);

                    rightStickYData.max = (ushort)(rightStickCalib[3] + rightStickCalib[5]);
                    rightStickYData.mid = rightStickCalib[3];
                    rightStickYData.min = (ushort)(rightStickCalib[3] - rightStickCalib[1]);
                }
                else
                {
                    rightStickXData.max =
                        (ushort)((rightStickCalib[2] + rightStickCalib[0]) * STICK_AXIS_RS_X_MAX_CUTOFF);
                    rightStickXData.min =
                        (ushort)((rightStickCalib[2] - rightStickCalib[4]) * STICK_AXIS_RS_X_MIN_CUTOFF);
                    //rightStickXData.mid = rightStickCalib[2];
                    rightStickXData.mid =
                        (ushort)((rightStickXData.max - rightStickXData.min) / 2.0 + rightStickXData.min);

                    rightStickYData.max =
                        (ushort)((rightStickCalib[3] + rightStickCalib[1]) * STICK_AXIS_RS_Y_MAX_CUTOFF);
                    rightStickYData.min =
                        (ushort)((rightStickCalib[3] - rightStickCalib[5]) * STICK_AXIS_RS_Y_MIN_CUTOFF);
                    //rightStickYData.mid = rightStickCalib[3];
                    rightStickYData.mid =
                        (ushort)((rightStickYData.max - rightStickYData.min) / 2.0 + rightStickYData.min);
                    //rightStickOffsetX = rightStickOffsetY = 140;
                }

                //rightStickOffsetX = rightStickCalib[2];
                //rightStickOffsetY = rightStickCalib[5];

                //Console.WriteLine(string.Join(",", tmpBuffer));
                //Console.WriteLine();
                //Console.WriteLine(string.Join(",", rightStickCalib));

                /*
                // Grab Factory RS Dead Zone
                command = new byte[] { 0x98, 0x60, 0x00, 0x00, 0x10 };
                deadZoneBuffer = Subcommand(SwitchProSubCmd.SPI_FLASH_READ, command, 5, checkResponse: true);
                deadzoneRS = (ushort)((deadZoneBuffer[4 + SPI_RESP_OFFSET] << 8) & 0xF00 | deadZoneBuffer[3 + SPI_RESP_OFFSET]);
                //Console.WriteLine("DZ Right: {0}", deadzoneRS);
                //Console.WriteLine(string.Join(",", deadZoneBuffer));*/
            }

            foundUserCalib = false;
            command = new byte[] { 0x26, 0x80, 0x00, 0x00, 0x02 };
            tmpBuffer = Subcommand(SwitchProSubCmd.SPI_FLASH_READ, command, 5, true);
            if (tmpBuffer[SPI_RESP_OFFSET] == 0xB2 && tmpBuffer[SPI_RESP_OFFSET + 1] == 0xA1) foundUserCalib = true;

            //Console.WriteLine("{0}", string.Join(",", tmpBuffer.Skip(offset).ToArray()));
            if (foundUserCalib)
            {
                command = new byte[] { 0x28, 0x80, 0x00, 0x00, 0x18 };
                tmpBuffer = Subcommand(SwitchProSubCmd.SPI_FLASH_READ, command, 5, true);
                //Console.WriteLine("FOUND USER CALIB");
            }
            else
            {
                command = new byte[] { 0x20, 0x60, 0x00, 0x00, 0x18 };
                tmpBuffer = Subcommand(SwitchProSubCmd.SPI_FLASH_READ, command, 5, true);
                //Console.WriteLine("CHECK FACTORY CALIB");
            }

            accelNeutral[IMU_XAXIS_IDX] =
                (short)(((tmpBuffer[3 + SPI_RESP_OFFSET] << 8) & 0xFF00) |
                        tmpBuffer[2 + SPI_RESP_OFFSET]); // Accel X Offset
            accelNeutral[IMU_YAXIS_IDX] =
                (short)(((tmpBuffer[1 + SPI_RESP_OFFSET] << 8) & 0xFF00) |
                        tmpBuffer[0 + SPI_RESP_OFFSET]); // Accel Y Offset
            accelNeutral[IMU_ZAXIS_IDX] =
                (short)(((tmpBuffer[5 + SPI_RESP_OFFSET] << 8) & 0xFF00) |
                        tmpBuffer[4 + SPI_RESP_OFFSET]); // Accel Z Offset
            //Console.WriteLine("ACCEL NEUTRAL: {0}", string.Join(",", accelNeutral));
            //Console.WriteLine("{0}", string.Join(",", tmpBuffer.Skip(offset).ToArray()));

            accelSens[IMU_XAXIS_IDX] =
                (short)(((tmpBuffer[9 + SPI_RESP_OFFSET] << 8) & 0xFF00) |
                        tmpBuffer[8 + SPI_RESP_OFFSET]); // Accel X Sens
            accelSens[IMU_YAXIS_IDX] =
                (short)(((tmpBuffer[7 + SPI_RESP_OFFSET] << 8) & 0xFF00) |
                        tmpBuffer[6 + SPI_RESP_OFFSET]); // Accel Y Sens
            accelSens[IMU_ZAXIS_IDX] =
                (short)(((tmpBuffer[11 + SPI_RESP_OFFSET] << 8) & 0xFF00) |
                        tmpBuffer[10 + SPI_RESP_OFFSET]); // Accel Z Sens
            //Console.WriteLine("ACCEL SENS: {0}", string.Join(",", accelSens));
            //Console.WriteLine("{0}", string.Join(",", tmpBuffer.Skip(SPI_RESP_OFFSET).ToArray()));

            accelCoeff[IMU_XAXIS_IDX] = 1.0 / (accelSens[IMU_XAXIS_IDX] - accelNeutral[IMU_XAXIS_IDX]) * 4.0;
            accelCoeff[IMU_YAXIS_IDX] = 1.0 / (accelSens[IMU_YAXIS_IDX] - accelNeutral[IMU_YAXIS_IDX]) * 4.0;
            accelCoeff[IMU_ZAXIS_IDX] = 1.0 / (accelSens[IMU_ZAXIS_IDX] - accelNeutral[IMU_ZAXIS_IDX]) * 4.0;
            //accelCoeff[IMU_XAXIS_IDX] = (accelSens[IMU_XAXIS_IDX] - accelNeutral[IMU_XAXIS_IDX]) / 65535.0 / 1000.0;
            //accelCoeff[IMU_YAXIS_IDX] = (accelSens[IMU_YAXIS_IDX] - accelNeutral[IMU_YAXIS_IDX]) / 65535.0 / 1000.0;
            //accelCoeff[IMU_ZAXIS_IDX] = (accelSens[IMU_ZAXIS_IDX] - accelNeutral[IMU_ZAXIS_IDX]) / 65535.0 / 1000.0;
            //Console.WriteLine("ACCEL COEFF: {0}", string.Join(",", accelCoeff));

            //accelSensMulti[IMU_XAXIS_IDX] = accelSens[IMU_XAXIS_IDX] / (2 * 8192.0);
            //accelSensMulti[IMU_YAXIS_IDX] = accelSens[IMU_YAXIS_IDX] / (2 * 8192.0);
            //accelSensMulti[IMU_ZAXIS_IDX] = accelSens[IMU_ZAXIS_IDX] / (2 * 8192.0);

            gyroBias[0] =
                (short)(((tmpBuffer[17 + SPI_RESP_OFFSET] << 8) & 0xFF00) |
                        tmpBuffer[16 + SPI_RESP_OFFSET]); // Gyro Yaw Offset
            gyroBias[1] =
                (short)(((tmpBuffer[15 + SPI_RESP_OFFSET] << 8) & 0xFF00) |
                        tmpBuffer[14 + SPI_RESP_OFFSET]); // Gyro Pitch Offset
            gyroBias[2] =
                (short)(((tmpBuffer[13 + SPI_RESP_OFFSET] << 8) & 0xFF00) |
                        tmpBuffer[12 + SPI_RESP_OFFSET]); // Gyro Roll Offset

            gyroSens[IMU_YAW_IDX] =
                (short)(((tmpBuffer[23 + SPI_RESP_OFFSET] << 8) & 0xFF00) |
                        tmpBuffer[22 + SPI_RESP_OFFSET]); // Gyro Yaw Sens
            gyroSens[IMU_PITCH_IDX] =
                (short)(((tmpBuffer[21 + SPI_RESP_OFFSET] << 8) & 0xFF00) |
                        tmpBuffer[20 + SPI_RESP_OFFSET]); // Gyro Pitch Sens
            gyroSens[IMU_ROLL_IDX] =
                (short)(((tmpBuffer[19 + SPI_RESP_OFFSET] << 8) & 0xFF00) |
                        tmpBuffer[18 + SPI_RESP_OFFSET]); // Gyro Roll Sens

            //Console.WriteLine("GYRO BIAS: {0}", string.Join(",", gyroBias));
            //Console.WriteLine("GYRO SENS: {0}", string.Join(",", gyroSens));
            //Console.WriteLine("{0}", string.Join(",", tmpBuffer.Skip(SPI_RESP_OFFSET).ToArray()));

            gyroCoeff[IMU_YAW_IDX] = 936.0 / (gyroSens[IMU_YAW_IDX] - gyroBias[IMU_YAW_IDX]);
            gyroCoeff[IMU_PITCH_IDX] = 936.0 / (gyroSens[IMU_PITCH_IDX] - gyroBias[IMU_PITCH_IDX]);
            gyroCoeff[IMU_ROLL_IDX] = 936.0 / (gyroSens[IMU_ROLL_IDX] - gyroBias[IMU_ROLL_IDX]);
            //gyroCoeff[IMU_YAW_IDX] = (gyroSens[IMU_YAW_IDX] - gyroBias[IMU_YAW_IDX]) / 65535.0;
            //gyroCoeff[IMU_PITCH_IDX] = (gyroSens[IMU_PITCH_IDX] - gyroBias[IMU_PITCH_IDX]) / 65535.0;
            //gyroCoeff[IMU_ROLL_IDX] = (gyroSens[IMU_ROLL_IDX] - gyroBias[IMU_ROLL_IDX]) / 65535.0;
            //Console.WriteLine("GYRO COEFF: {0}", string.Join(",", gyroCoeff));
        }

        public byte JoyConStickAdjust(int raw, int offset, int range, int sense)
        {
            var scaled = sense * (raw - offset) * 256 / range + 128;
            //if (scaled > 119 && scaled < 138) scaled = 128; // dead zone
            if (scaled > 255) scaled = 255;
            else if (scaled < 0) scaled = 0;
            return (byte)scaled;
        }

        public override bool DisconnectWireless(bool callRemoval = false)
        {
            var result = false;
            result = DisconnectBT(callRemoval);
            //StopOutputUpdate();
            //Detach();
            return result;
        }

        public override bool DisconnectBT(bool callRemoval = false)
        {
            StopOutputUpdate();
            Detach();

            uint IOCTL_BTH_DISCONNECT_DEVICE = 0x41000c;

            var btAddr = new byte[8];
            var sbytes = MacAddress.Split(':');
            for (var i = 0; i < 6; i++)
                // parse hex byte in reverse order
                btAddr[5 - i] = Convert.ToByte(sbytes[i], 16);

            var lbtAddr = BitConverter.ToInt64(btAddr, 0);

            var btHandle = IntPtr.Zero;
            var success = false;
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
            success = true;

            // Need to grab reference here as Removal call would
            // remove device reference
            var tempJointDevice = jointDevice;
            if (callRemoval)
            {
                isDisconnecting = true;
                Removal?.Invoke(this, EventArgs.Empty);
            }

            // Place check here for now due to direct calls in other portions of
            // code. Would be better placed in DisconnectWireless method
            if (primaryDevice &&
                tempJointDevice != null)
                tempJointDevice.queueEvent(() => { tempJointDevice.DisconnectBT(callRemoval); });

            return success;
        }

        public override bool DisconnectDongle(bool remove = false)
        {
            StopOutputUpdate();
            Detach();

            if (remove)
            {
                isDisconnecting = true;
                Removal?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                isRemoved = true;
            }

            return true;
        }

        private void Detach()
        {
            //bool result;

            if (connectionOpened)
            {
                // Disable Gyro
                byte[] tmpOffBuffer = { 0x0 };
                Subcommand(0x40, tmpOffBuffer, 1, true);

                // Possibly disable rumble? Leave commented
                tmpOffBuffer = new byte[] { 0x0 };
                Subcommand(0x48, tmpOffBuffer, 1, true);

                // Revert back to low power state
                byte[] powerChoiceArray = { 0x01 };
                Subcommand(SwitchProSubCmd.SET_LOW_POWER_STATE, powerChoiceArray, 1, true);
            }

            connectionOpened = false;
        }

        private void CalculateDeviceSlotMask()
        {
            // Map 1-15 as a set of 4 LED lights
            switch (deviceSlotNumber)
            {
                case 0:
                case 1:
                case 2:
                case 3:
                    deviceSlotMask = (byte)(1 << deviceSlotNumber);
                    break;
                case 4:
                    deviceSlotMask = 0x01 | 0x02;
                    break;
                case 5:
                    deviceSlotMask = 0x01 | 0x04;
                    break;
                case 6:
                    deviceSlotMask = 0x01 | 0x08;
                    break;

                case 7:
                    deviceSlotMask = 0x02 | 0x04;
                    break;
                case 8:
                    deviceSlotMask = 0x02 | 0x08;
                    break;

                case 9:
                    deviceSlotMask = 0x04 | 0x08;
                    break;

                case 10:
                    deviceSlotMask = 0x01 | 0x02 | 0x04;
                    break;
                case 11:
                    deviceSlotMask = 0x01 | 0x02 | 0x08;
                    break;
                case 12:
                    deviceSlotMask = 0x01 | 0x04 | 0x08;
                    break;

                case 13:
                    deviceSlotMask = 0x02 | 0x04 | 0x08;
                    break;

                case 14:
                    deviceSlotMask = 0x01 | 0x02 | 0x04 | 0x08;
                    break;
                default:
                    deviceSlotMask = 0x00;
                    break;
            }
        }

        private void SetupOptionsEvents()
        {
            if (nativeOptionsStore != null)
            {
            }
        }

        public override void LoadStoreSettings()
        {
            if (nativeOptionsStore != null) EnableHomeLED = nativeOptionsStore.EnableHomeLED;
        }

        public override DS4State GetCurrentStateReference()
        {
            DS4State tempState = null;
            if (!performStateMerge)
                tempState = currentState;
            else
                tempState = jointState;

            return tempState;
        }

        public override DS4State GetPreviousStateReference()
        {
            DS4State tempState = null;
            if (!performStateMerge)
                tempState = pState;
            else
                tempState = jointPreviousState;

            return tempState;
        }

        public override void PreserveMergedStateData()
        {
            jointState.CopyTo(jointPreviousState);
        }

        public override void MergeStateData(DS4State dState)
        {
            using (var locker = new WriteLocker(lockSlim))
            {
                if (DeviceType == InputDeviceType.JoyConL)
                {
                    dState.LX = currentState.LX;
                    dState.LY = currentState.LY;
                    dState.L1 = currentState.L1;
                    dState.L2 = currentState.L2;
                    dState.L3 = currentState.L3;
                    dState.L2Btn = currentState.L2Btn;
                    dState.DpadUp = currentState.DpadUp;
                    dState.DpadDown = currentState.DpadDown;
                    dState.DpadLeft = currentState.DpadLeft;
                    dState.DpadRight = currentState.DpadRight;
                    dState.Share = currentState.Share;
                    dState.Capture = currentState.Capture;
                    if (primaryDevice)
                    {
                        dState.elapsedTime = currentState.elapsedTime;
                        dState.totalMicroSec = currentState.totalMicroSec;
                        dState.ReportTimeStamp = currentState.ReportTimeStamp;
                        dState.SideL = currentState.SideL;
                        dState.SideR = currentState.SideR;
                    }

                    if (outputMapGyro) dState.Motion = currentState.Motion;
                    //dState.Motion = cState.Motion;
                }
                else if (DeviceType == InputDeviceType.JoyConR)
                {
                    dState.RX = currentState.RX;
                    dState.RY = currentState.RY;
                    dState.R1 = currentState.R1;
                    dState.R2 = currentState.R2;
                    dState.R3 = currentState.R3;
                    dState.R2Btn = currentState.R2Btn;
                    dState.Cross = currentState.Cross;
                    dState.Circle = currentState.Circle;
                    dState.Triangle = currentState.Triangle;
                    dState.Square = currentState.Square;
                    dState.PS = currentState.PS;
                    dState.Options = currentState.Options;
                    if (primaryDevice)
                    {
                        dState.elapsedTime = currentState.elapsedTime;
                        dState.totalMicroSec = currentState.totalMicroSec;
                        dState.ReportTimeStamp = currentState.ReportTimeStamp;
                        dState.SideL = currentState.SideL;
                        dState.SideR = currentState.SideR;
                    }

                    if (outputMapGyro) dState.Motion = currentState.Motion;
                    //dState.Motion = cState.Motion;
                }
            }
        }

        public class RumbleTableData
        {
            public int amp;
            public byte high;
            public ushort low;

            public RumbleTableData(byte high, ushort low, int amp)
            {
                this.high = high;
                this.low = low;
                this.amp = amp;
            }
        }

        public static class SwitchProSubCmd
        {
            public const byte SET_INPUT_MODE = 0x03;
            public const byte SET_LOW_POWER_STATE = 0x08;
            public const byte SPI_FLASH_READ = 0x10;
            public const byte SET_LIGHTS = 0x30; // LEDs on controller
            public const byte SET_HOME_LIGHT = 0x38;
            public const byte ENABLE_IMU = 0x40;
            public const byte SET_IMU_SENS = 0x41;
            public const byte ENABLE_VIBRATION = 0x48;
        }

        public struct StickAxisData
        {
            public ushort max;
            public ushort mid;
            public ushort min;
        }
    }
}