using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using AdonisUI.Controls;
using DS4Windows.InputDevices;
using DS4Windows.VJoyFeeder;
using DS4WinWPF;
using DS4WinWPF.DS4Control;
using DS4WinWPF.DS4Control.Attributes;
using DS4WinWPF.DS4Control.IoC.Services;
using DS4WinWPF.DS4Control.Logging;
using DS4WinWPF.DS4Control.Util;
using DS4WinWPF.Properties;
using Nefarius.ViGEm.Client.Targets;
using Sensorit.Base;
using static DS4Windows.Global;
using MessageBox = AdonisUI.Controls.MessageBox;
using MessageBoxImage = AdonisUI.Controls.MessageBoxImage;

namespace DS4Windows
{
    public partial class ControlService
    {
        public static ControlService CurrentInstance { get; set; }

        // Might be useful for ScpVBus build
        public const int EXPANDED_CONTROLLER_COUNT = 8;
        public const int MAX_DS4_CONTROLLER_COUNT = Global.MAX_DS4_CONTROLLER_COUNT;
#if FORCE_4_INPUT
        public static int CURRENT_DS4_CONTROLLER_LIMIT = Global.OLD_XINPUT_CONTROLLER_COUNT;
#else
        public static int CURRENT_DS4_CONTROLLER_LIMIT =
            IsWin8OrGreater ? MAX_DS4_CONTROLLER_COUNT : OLD_XINPUT_CONTROLLER_COUNT;
#endif
        public static bool USING_MAX_CONTROLLERS = CURRENT_DS4_CONTROLLER_LIMIT == EXPANDED_CONTROLLER_COUNT;
        public DS4Device[] DS4Controllers = new DS4Device[MAX_DS4_CONTROLLER_COUNT];
        public int activeControllers;
        public Mouse[] touchPad = new Mouse[MAX_DS4_CONTROLLER_COUNT];
        public bool IsRunning;
        public bool loopControllers = true;
        public bool inServiceTask;
        private readonly DS4State[] MappedState = new DS4State[MAX_DS4_CONTROLLER_COUNT];
        private readonly DS4State[] CurrentState = new DS4State[MAX_DS4_CONTROLLER_COUNT];
        private readonly DS4State[] PreviousState = new DS4State[MAX_DS4_CONTROLLER_COUNT];
        private readonly DS4State[] TempState = new DS4State[MAX_DS4_CONTROLLER_COUNT];
        public DS4StateExposed[] ExposedState = new DS4StateExposed[MAX_DS4_CONTROLLER_COUNT];
        public ControllerSlotManager slotManager = new();
        public bool recordingMacro = false;
        public event EventHandler<LogEntryEventArgs> Debug;

        private bool[] buttonsdown = new bool[MAX_DS4_CONTROLLER_COUNT]
            { false, false, false, false, false, false, false, false };

        private bool[] held = new bool[MAX_DS4_CONTROLLER_COUNT];
        private int[] oldmouse = new int[MAX_DS4_CONTROLLER_COUNT] { -1, -1, -1, -1, -1, -1, -1, -1 };

        public OutputDevice[] outputDevices = new OutputDevice[MAX_DS4_CONTROLLER_COUNT]
            { null, null, null, null, null, null, null, null };

        private readonly OneEuroFilter3D[] udpEuroPairAccel = new OneEuroFilter3D[UdpServer.NUMBER_SLOTS]
        {
            new(), new(),
            new(), new()
        };

        private readonly OneEuroFilter3D[] udpEuroPairGyro = new OneEuroFilter3D[UdpServer.NUMBER_SLOTS]
        {
            new(), new(),
            new(), new()
        };

        private readonly Thread tempBusThread;
        private Thread eventDispatchThread;

        public bool suspending;

        //SoundPlayer sp = new SoundPlayer();
        private UdpServer _udpServer;

        private readonly HashSet<string> hidDeviceHidingAffectedDevs = new();
        private readonly HashSet<string> hidDeviceHidingExemptedDevs = new();
        private bool hidDeviceHidingForced;
        private bool hidDeviceHidingEnabled;

        public ControlServiceDeviceOptions DeviceOptions { get; }

        private readonly ICommandLineOptions cmdParser;

        public event EventHandler ServiceStarted;
        public event EventHandler PreServiceStop;
        public event EventHandler ServiceStopped;

        public event EventHandler RunningChanged;

        //public event EventHandler HotplugFinished;
        public delegate void HotplugControllerHandler(ControlService sender, DS4Device device, int index);

        public event HotplugControllerHandler HotplugController;

        private readonly byte[][] udpOutBuffers = new byte[UdpServer.NUMBER_SLOTS][]
        {
            new byte[100], new byte[100],
            new byte[100], new byte[100]
        };

        private readonly object busThrLck = new();
        private bool busThrRunning;
        private readonly Queue<Action> busEvtQueue = new();
        private readonly object busEvtQueueLock = new();

        private readonly IAppSettingsService appSettings;

        public ControlService(
            ICommandLineOptions cmdParser, 
            IOutputSlotManager osl,
            IAppSettingsService appSettings
            )
        {
            this.appSettings = appSettings;
            this.cmdParser = cmdParser;

            Crc32Algorithm.InitializeTable(DS4Device.DefaultPolynomial);
            InitOutputKBMHandler();

            //sp.Stream = DS4WinWPF.Properties.Resources.EE;
            // Cause thread affinity to not be tied to main GUI thread
            tempBusThread = new Thread(() =>
            {
                //_udpServer = new UdpServer(GetPadDetailForIdx);
                busThrRunning = true;

                while (busThrRunning)
                {
                    lock (busEvtQueueLock)
                    {
                        Action tempAct = null;
                        for (int actInd = 0, actLen = busEvtQueue.Count; actInd < actLen; actInd++)
                        {
                            tempAct = busEvtQueue.Dequeue();
                            tempAct.Invoke();
                        }
                    }

                    lock (busThrLck)
                    {
                        Monitor.Wait(busThrLck);
                    }
                }
            });
            tempBusThread.Priority = ThreadPriority.Normal;
            tempBusThread.IsBackground = true;
            tempBusThread.Start();
            //while (_udpServer == null)
            //{
            //    Thread.SpinWait(500);
            //}

            eventDispatchThread = new Thread(() =>
            {
                var currentDis = Dispatcher.CurrentDispatcher;
                EventDispatcher = currentDis;
                Dispatcher.Run();
            });
            eventDispatchThread.IsBackground = true;
            eventDispatchThread.Priority = ThreadPriority.Normal;
            eventDispatchThread.Name = "ControlService Events";
            eventDispatchThread.Start();

            for (int i = 0, arlength = DS4Controllers.Length; i < arlength; i++)
            {
                MappedState[i] = new DS4State();
                CurrentState[i] = new DS4State();
                TempState[i] = new DS4State();
                PreviousState[i] = new DS4State();
                ExposedState[i] = new DS4StateExposed(CurrentState[i]);

                var tempDev = i;
                Instance.Config.L2OutputSettings[i].TwoStageModeChanged += (sender, e) =>
                {
                    Mapping.l2TwoStageMappingData[tempDev].Reset();
                };

                Instance.Config.R2OutputSettings[i].TwoStageModeChanged += (sender, e) =>
                {
                    Mapping.r2TwoStageMappingData[tempDev].Reset();
                };
            }

            OutputslotMan = osl;
            DeviceOptions = Instance.Config.DeviceOptions;

            DS4Devices.RequestElevation += DS4Devices_RequestElevation;
            DS4Devices.checkVirtualFunc = CheckForVirtualDevice;
            DS4Devices.PrepareDS4Init = PrepareDs4DeviceInit;
            DS4Devices.PostDS4Init = PostDs4DeviceInit;
            DS4Devices.PreparePendingDevice = CheckForSupportedDevice;
            OutputslotMan.ViGEmFailure += OutputslotMan_ViGEmFailure;

            UDPServerSmoothingMincutoffChanged += ChangeUdpSmoothingAttrs;
            UDPServerSmoothingBetaChanged += ChangeUdpSmoothingAttrs;
        }
        
        private void GetPadDetailForIdx(int padIdx, ref DualShockPadMeta meta)
        {
            //meta = new DualShockPadMeta();
            meta.PadId = (byte)padIdx;
            meta.Model = DsModel.DS4;

            var d = DS4Controllers[padIdx];
            if (d == null || !d.PrimaryDevice)
            {
                meta.PadMacAddress = null;
                meta.PadState = DsState.Disconnected;
                meta.ConnectionType = DsConnection.None;
                meta.Model = DsModel.None;
                meta.BatteryStatus = 0;
                meta.IsActive = false;
                return;
                //return meta;
            }

            var isValidSerial = false;
            //
            // TODO: can be further simplified
            // 
            var stringMac = d.MacAddress.AsFriendlyName();
            if (!string.IsNullOrEmpty(stringMac))
            {
                stringMac = string.Join("", stringMac.Split(':'));
                //stringMac = stringMac.Replace(":", "").Trim();
                meta.PadMacAddress = PhysicalAddress.Parse(stringMac);
                isValidSerial = d.IsValidSerial();
            }

            if (!isValidSerial)
            {
                //meta.PadMacAddress = null;
                meta.PadState = DsState.Disconnected;
            }
            else
            {
                if (d.IsSynced() || d.IsAlive())
                    meta.PadState = DsState.Connected;
                else
                    meta.PadState = DsState.Reserved;
            }

            meta.ConnectionType =
                d.GetConnectionType() == ConnectionType.USB ? DsConnection.Usb : DsConnection.Bluetooth;
            meta.IsActive = !d.IsDs4Idle();

            var batteryLevel = d.GetBattery();
            if (d.IsCharging() && batteryLevel >= 100)
            {
                meta.BatteryStatus = DsBattery.Charged;
            }
            else
            {
                if (batteryLevel >= 95)
                    meta.BatteryStatus = DsBattery.Full;
                else if (batteryLevel >= 70)
                    meta.BatteryStatus = DsBattery.High;
                else if (batteryLevel >= 50)
                    meta.BatteryStatus = DsBattery.Medium;
                else if (batteryLevel >= 20)
                    meta.BatteryStatus = DsBattery.Low;
                else if (batteryLevel >= 5)
                    meta.BatteryStatus = DsBattery.Dying;
                else
                    meta.BatteryStatus = DsBattery.None;
            }

            //return meta;
        }

        public void PostDs4DeviceInit(DS4Device device)
        {
            if (device.DeviceType is not (InputDeviceType.JoyConL or InputDeviceType.JoyConR)) return;

            if (DeviceOptions.JoyConDeviceOpts.LinkedMode != JoyConDeviceOptions.LinkMode.Joined) return;

            var tempJoyDev = device as JoyConDevice;
            tempJoyDev.PerformStateMerge = true;

            if (device.DeviceType == InputDeviceType.JoyConL)
            {
                tempJoyDev.PrimaryDevice = true;
                tempJoyDev.OutputMapGyro = DeviceOptions.JoyConDeviceOpts.JoinGyroProv ==
                                           JoyConDeviceOptions.JoinedGyroProvider.JoyConL;
            }
            else
            {
                tempJoyDev.PrimaryDevice = false;
                tempJoyDev.OutputMapGyro = DeviceOptions.JoyConDeviceOpts.JoinGyroProv ==
                                           JoyConDeviceOptions.JoinedGyroProvider.JoyConR;
            }
        }

        private void PrepareDs4DeviceSettingHooks(DS4Device device)
        {
            switch (device.DeviceType)
            {
                case InputDeviceType.DualSense:
                {
                    var tempDSDev = device as DualSenseDevice;

                    var dSOpts = tempDSDev.NativeOptionsStore;
                    dSOpts.LedModeChanged += () => { tempDSDev.CheckControllerNumDeviceSettings(activeControllers); };
                    break;
                }
                case InputDeviceType.JoyConL:
                case InputDeviceType.JoyConR:
                    break;
            }
        }

        public bool CheckForSupportedDevice(HidDevice device, VidPidInfo metaInfo)
        {
            var result = false;
            switch (metaInfo.InputDevType)
            {
                case InputDeviceType.DS4:
                    result = DeviceOptions.Ds4DeviceOpts.Enabled;
                    break;
                case InputDeviceType.DualSense:
                    result = DeviceOptions.DualSenseOpts.Enabled;
                    break;
                case InputDeviceType.SwitchPro:
                    result = DeviceOptions.SwitchProDeviceOpts.Enabled;
                    break;
                case InputDeviceType.JoyConL:
                case InputDeviceType.JoyConR:
                    result = DeviceOptions.JoyConDeviceOpts.Enabled;
                    break;
            }

            return result;
        }

        public void PrepareDs4DeviceInit(DS4Device device)
        {
            if (!IsWin8OrGreater) device.BTOutputMethod = DS4Device.BTOutputReportMethod.HidD_SetOutputReport;

            if (device is DualSenseDevice dualSenseDevice)
                dualSenseDevice.ProblematicFirmwareVersionDetected += async (ds4Device, version) =>
                {
                    var ds = (DualSenseDevice)ds4Device;
                    var store = (DualSenseControllerOptions)ds.OptionsStore;

                    if (store.HasUserConfirmedProblematicFirmware)
                        return;

                    var messageBox = new MessageBoxModel
                    {
                        Text =
                            $"Hello, Gamer! \r\n\r\nYour DualSense ({dualSenseDevice.MacAddress.AsFriendlyName()}) is running " +
                            $"outdated Firmware (version {version}) known to cause issues (e.g. excessive battery drainage). "
                            + "\r\n\r\nPlease plug the controller into a PlayStation 5 and update the firmware. \r\n\r\nThanks for your attention ❤️",
                        Caption = "Outdated Firmware detected",
                        Icon = MessageBoxImage.Warning,
                        Buttons = new[]
                        {
                            MessageBoxButtons.Yes("Understood")
                        },
                        CheckBoxes = new[]
                        {
                            new MessageBoxCheckBoxModel("I have understood and will not open a bug report about it")
                            {
                                IsChecked = false,
                                Placement = MessageBoxCheckBoxPlacement.BelowText
                            }
                        },
                        IsSoundEnabled = false
                    };

                    await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        MessageBox.Show(Application.Current.MainWindow, messageBox);
                    });

                    store.HasUserConfirmedProblematicFirmware = messageBox.CheckBoxes.First().IsChecked;
                    ds.PersistOptionsStore(Instance.Config.ControllerConfigsPath);
                };
        }

        public CheckVirtualInfo CheckForVirtualDevice(string deviceInstanceId)
        {
            var temp = GetDeviceProperty(deviceInstanceId,
                NativeMethods.DEVPKEY_Device_UINumber);

            var info = new CheckVirtualInfo
            {
                PropertyValue = temp,
                DeviceInstanceId = deviceInstanceId
            };
            return info;
        }

        public void ShutDown()
        {
            DS4Devices.checkVirtualFunc = null;
            OutputslotMan.ShutDown();
            OutputSlotPersist.Instance.WriteConfig(OutputslotMan);

            outputKBMHandler.Disconnect();

            EventDispatcher.InvokeShutdown();
            EventDispatcher = null;

            eventDispatchThread.Join();
            eventDispatchThread = null;
        }

        private void DS4Devices_RequestElevation(RequestElevationArgs args)
        {
            // Launches an elevated child process to re-enable device
            var startInfo =
                new ProcessStartInfo(ExecutableLocation);
            startInfo.Verb = "runas";
            startInfo.Arguments = "re-enabledevice " + args.InstanceId;
            startInfo.UseShellExecute = true;

            try
            {
                var child = Process.Start(startInfo);
                if (!child.WaitForExit(30000))
                    child.Kill();
                else
                    args.StatusCode = child.ExitCode;
                child.Dispose();
            }
            catch
            {
            }
        }

        public async Task LoadPermanentSlotsConfig()
        {
            await OutputSlotPersist.Instance.ReadConfig(OutputslotMan);
        }
        
        private List<DS4Controls> GetKnownExtraButtons(DS4Device dev)
        {
            var result = new List<DS4Controls>();
            switch (dev.DeviceType)
            {
                case InputDeviceType.JoyConL:
                case InputDeviceType.JoyConR:
                    result.AddRange(new[] { DS4Controls.Capture, DS4Controls.SideL, DS4Controls.SideR });
                    break;
                case InputDeviceType.SwitchPro:
                    result.AddRange(new[] { DS4Controls.Capture });
                    break;
            }

            return result;
        }

        private void ChangeExclusiveStatus(DS4Device dev)
        {
            if (hidHideInstalled) dev.CurrentExclusiveStatus = DS4Device.ExclusiveStatus.HidHideAffected;
        }

        private void TestQueueBus(Action temp)
        {
            lock (busEvtQueueLock)
            {
                busEvtQueue.Enqueue(temp);
            }

            lock (busThrLck)
            {
                Monitor.Pulse(busThrLck);
            }
        }

        public void ChangeUDPStatus(bool state, bool openPort = true)
        {
            if (state && _udpServer == null)
            {
                udpChangeStatus = true;
                TestQueueBus(() =>
                {
                    _udpServer = new UdpServer(GetPadDetailForIdx);
                    if (openPort)
                        // Change thread affinity of object to have normal priority
                        Task.Run(() =>
                        {
                            var UDP_SERVER_PORT = Instance.Config.UdpServerPort;
                            var UDP_SERVER_LISTEN_ADDRESS = Instance.Config.UdpServerListenAddress;

                            try
                            {
                                _udpServer.Start(UDP_SERVER_PORT, UDP_SERVER_LISTEN_ADDRESS);
                                LogDebug(
                                    $"UDP server listening on address {UDP_SERVER_LISTEN_ADDRESS} port {UDP_SERVER_PORT}");
                            }
                            catch (SocketException ex)
                            {
                                var errMsg =
                                    string.Format(
                                        "Couldn't start UDP server on address {0}:{1}, outside applications won't be able to access pad data ({2})",
                                        UDP_SERVER_LISTEN_ADDRESS, UDP_SERVER_PORT, ex.SocketErrorCode);

                                LogDebug(errMsg, true);
                                AppLogger.Instance.LogToTray(errMsg, true, true);
                            }
                        }).Wait();

                    udpChangeStatus = false;
                });
            }
            else if (!state && _udpServer != null)
            {
                TestQueueBus(() =>
                {
                    udpChangeStatus = true;
                    _udpServer.Stop();
                    _udpServer = null;
                    AppLogger.Instance.LogToGui("Closed UDP server", false);
                    udpChangeStatus = false;

                    for (var i = 0; i < UdpServer.NUMBER_SLOTS; i++) ResetUdpSmoothingFilters(i);
                });
            }
        }

        public void ChangeMotionEventStatus(bool state)
        {
            var devices = DS4Devices.GetDS4Controllers();
            if (state)
            {
                var i = 0;
                foreach (var dev in devices)
                {
                    var tempIdx = i;
                    dev.QueueEvent(() =>
                    {
                        if (i < UdpServer.NUMBER_SLOTS && dev.PrimaryDevice) PrepareDevUDPMotion(dev, tempIdx);
                    });

                    i++;
                }
            }
            else
            {
                foreach (var dev in devices)
                    dev.QueueEvent(() =>
                    {
                        if (dev.MotionEvent != null)
                        {
                            dev.Report -= dev.MotionEvent;
                            dev.MotionEvent = null;
                        }
                    });
            }
        }
        
        private void WarnExclusiveModeFailure(DS4Device device)
        {
            if (DS4Devices.isExclusiveMode && !device.isExclusive())
            {
                var message = Resources.CouldNotOpenDS4.Replace("*Mac address*", device.MacAddress.AsFriendlyName()) +
                              " " +
                              Resources.QuitOtherPrograms;
                LogDebug(message, true);
                AppLogger.Instance.LogToTray(message, true);
            }
        }
        
        public void AssignInitialDevices()
        {
            foreach (var slotDevice in OutputslotMan.OutputSlots)
                if (slotDevice.CurrentReserveStatus ==
                    OutSlotDevice.ReserveStatus.Permanent)
                {
                    var outDevice = EstablishOutDevice(0, slotDevice.PermanentType);
                    OutputslotMan.DeferredPlugin(outDevice, -1, outputDevices, slotDevice.PermanentType);
                }
            /*OutSlotDevice slotDevice =
                outputslotMan.FindExistUnboundSlotType(OutContType.X360);

            if (slotDevice == null)
            {
                slotDevice = outputslotMan.FindOpenSlot();
                slotDevice.CurrentReserveStatus = OutSlotDevice.ReserveStatus.Permanent;
                slotDevice.PermanentType = OutContType.X360;
                OutputDevice outDevice = EstablishOutDevice(0, OutContType.X360);
                Xbox360OutDevice tempXbox = outDevice as Xbox360OutDevice;
                outputslotMan.DeferredPlugin(tempXbox, -1, outputDevices, OutContType.X360);
            }
            */

            /*slotDevice = outputslotMan.FindExistUnboundSlotType(OutContType.X360);
            if (slotDevice == null)
            {
                slotDevice = outputslotMan.FindOpenSlot();
                slotDevice.CurrentReserveStatus = OutSlotDevice.ReserveStatus.Permanent;
                slotDevice.DesiredType = OutContType.X360;
                OutputDevice outDevice = EstablishOutDevice(1, OutContType.X360);
                Xbox360OutDevice tempXbox = outDevice as Xbox360OutDevice;
                outputslotMan.DeferredPlugin(tempXbox, 1, outputDevices);
            }*/
        }

        private OutputDevice EstablishOutDevice(int index, OutContType contType)
        {
            return OutputslotMan.AllocateController(contType);
        }

        public void EstablishOutFeedback(int index, OutContType contType,
            OutputDevice outDevice, DS4Device device)
        {
            var devIndex = index;

            if (contType == OutContType.X360)
            {
                var tempXbox = outDevice as Xbox360OutDevice;
                Xbox360FeedbackReceivedEventHandler p = (sender, args) =>
                {
                    //Console.WriteLine("Rumble ({0}, {1}) {2}",
                    //    args.LargeMotor, args.SmallMotor, DateTime.Now.ToString("hh:mm:ss.FFFF"));
                    SetDevRumble(device, args.LargeMotor, args.SmallMotor, devIndex);
                };
                tempXbox.cont.FeedbackReceived += p;
                tempXbox.forceFeedbacksDict.Add(index, p);
            }
            //else if (contType == OutContType.DS4)
            //{
            //    DS4OutDevice tempDS4 = outDevice as DS4OutDevice;
            //    LightbarSettingInfo deviceLightbarSettingsInfo = Global.LightbarSettingsInfo[devIndex];

            //    Nefarius.ViGEm.Client.Targets.DualShock4FeedbackReceivedEventHandler p = (sender, args) =>
            //    {
            //        bool useRumble = false; bool useLight = false;
            //        byte largeMotor = args.LargeMotor;
            //        byte smallMotor = args.SmallMotor;
            //        //SetDevRumble(device, largeMotor, smallMotor, devIndex);
            //        DS4Color color = new DS4Color(args.LightbarColor.Red,
            //                args.LightbarColor.Green,
            //                args.LightbarColor.Blue);

            //        //Console.WriteLine("IN EVENT");
            //        //Console.WriteLine("Rumble ({0}, {1}) | Light ({2}, {3}, {4}) {5}",
            //        //    largeMotor, smallMotor, color.red, color.green, color.blue, DateTime.Now.ToString("hh:mm:ss.FFFF"));

            //        if (largeMotor != 0 || smallMotor != 0)
            //        {
            //            useRumble = true;
            //        }

            //        // Let games to control lightbar only when the mode is Passthru (otherwise DS4Windows controls the light)
            //        if (deviceLightbarSettingsInfo.Mode == LightbarMode.Passthru && (color.red != 0 || color.green != 0 || color.blue != 0))
            //        {
            //            useLight = true;
            //        }

            //        if (!useRumble && !useLight)
            //        {
            //            //Console.WriteLine("Fallback");
            //            if (device.LeftHeavySlowRumble != 0 || device.RightLightFastRumble != 0)
            //            {
            //                useRumble = true;
            //            }
            //            else if (deviceLightbarSettingsInfo.Mode == LightbarMode.Passthru &&
            //                (device.LightBarColor.red != 0 ||
            //                device.LightBarColor.green != 0 ||
            //                device.LightBarColor.blue != 0))
            //            {
            //                useLight = true;
            //            }
            //        }

            //        if (useRumble)
            //        {
            //            //Console.WriteLine("Perform rumble");
            //            SetDevRumble(device, largeMotor, smallMotor, devIndex);
            //        }

            //        if (useLight)
            //        {
            //            //Console.WriteLine("Change lightbar color");
            //            /*DS4HapticState haptics = new DS4HapticState
            //            {
            //                LightBarColor = color,
            //            };
            //            device.SetHapticState(ref haptics);
            //            */

            //            DS4LightbarState lightState = new DS4LightbarState
            //            {
            //                LightBarColor = color,
            //            };
            //            device.SetLightbarState(ref lightState);
            //        }

            //        //Console.WriteLine();
            //    };

            //    tempDS4.cont.FeedbackReceived += p;
            //    tempDS4.forceFeedbacksDict.Add(index, p);
            //}
        }

        public void RemoveOutFeedback(OutContType contType, OutputDevice outDevice, int inIdx)
        {
            if (contType == OutContType.X360)
            {
                var tempXbox = outDevice as Xbox360OutDevice;
                tempXbox.RemoveFeedback(inIdx);
                //tempXbox.cont.FeedbackReceived -= tempXbox.forceFeedbackCall;
                //tempXbox.forceFeedbackCall = null;
            }
            //else if (contType == OutContType.DS4)
            //{
            //    DS4OutDevice tempDS4 = outDevice as DS4OutDevice;
            //    tempDS4.RemoveFeedback(inIdx);
            //    //tempDS4.cont.FeedbackReceived -= tempDS4.forceFeedbackCall;
            //    //tempDS4.forceFeedbackCall = null;
            //}
        }

        public void AttachNewUnboundOutDev(OutContType contType)
        {
            var slotDevice = OutputslotMan.FindOpenSlot();
            if (slotDevice != null &&
                slotDevice.CurrentAttachedStatus == OutSlotDevice.AttachedStatus.UnAttached)
            {
                var outDevice = EstablishOutDevice(-1, contType);
                OutputslotMan.DeferredPlugin(outDevice, -1, outputDevices, contType);
                LogDebug($"Plugging virtual {contType} Controller");
            }
        }

        public void AttachUnboundOutDev(OutSlotDevice slotDevice, OutContType contType)
        {
            if (slotDevice.CurrentAttachedStatus == OutSlotDevice.AttachedStatus.UnAttached &&
                slotDevice.CurrentInputBound == OutSlotDevice.InputBound.Unbound)
            {
                var outDevice = EstablishOutDevice(-1, contType);
                OutputslotMan.DeferredPlugin(outDevice, -1, outputDevices, contType);
                LogDebug($"Plugging virtual {contType} Controller");
            }
        }

        public void DetachUnboundOutDev(OutSlotDevice slotDevice)
        {
            if (slotDevice.CurrentInputBound == OutSlotDevice.InputBound.Unbound)
            {
                var dev = slotDevice.OutputDevice;
                var tempType = dev.GetDeviceType();
                slotDevice.CurrentInputBound = OutSlotDevice.InputBound.Unbound;
                OutputslotMan.DeferredRemoval(dev, -1, outputDevices);
                LogDebug($"Unplugging virtual {tempType} Controller");
            }
        }

        public void PluginOutDev(int index, DS4Device device)
        {
            var contType = Instance.Config.OutputDeviceType[index];

            OutSlotDevice slotDevice = null;
            if (!Instance.Config.GetDirectInputOnly(index))
                slotDevice = OutputslotMan.FindExistUnboundSlotType(contType);

            if (UseDirectInputOnly[index])
            {
                var success = false;
                if (contType == OutContType.X360)
                {
                    ActiveOutDevType[index] = OutContType.X360;

                    if (slotDevice == null)
                    {
                        slotDevice = OutputslotMan.FindOpenSlot();
                        if (slotDevice != null)
                        {
                            var tempXbox = EstablishOutDevice(index, OutContType.X360)
                                as Xbox360OutDevice;
                            //outputDevices[index] = tempXbox;

                            // Enable ViGem feedback callback handler only if lightbar/rumble data output is enabled (if those are disabled then no point enabling ViGem callback handler call)
                            if (Instance.Config.EnableOutputDataToDS4[index])
                            {
                                EstablishOutFeedback(index, OutContType.X360, tempXbox, device);

                                if (device.JointDeviceSlotNumber != -1)
                                {
                                    var tempDS4Device = DS4Controllers[device.JointDeviceSlotNumber];
                                    if (tempDS4Device != null)
                                        EstablishOutFeedback(device.JointDeviceSlotNumber, OutContType.X360, tempXbox,
                                            tempDS4Device);
                                }
                            }

                            OutputslotMan.DeferredPlugin(tempXbox, index, outputDevices, contType);
                            //slotDevice.CurrentInputBound = OutSlotDevice.InputBound.Bound;

                            LogDebug("Plugging in virtual X360 Controller");
                            success = true;
                        }
                        else
                        {
                            LogDebug("Failed. No open output slot found");
                        }
                    }
                    else
                    {
                        slotDevice.CurrentInputBound = OutSlotDevice.InputBound.Bound;
                        var tempXbox = slotDevice.OutputDevice as Xbox360OutDevice;

                        // Enable ViGem feedback callback handler only if lightbar/rumble data output is enabled (if those are disabled then no point enabling ViGem callback handler call)
                        if (Instance.Config.EnableOutputDataToDS4[index])
                        {
                            EstablishOutFeedback(index, OutContType.X360, tempXbox, device);

                            if (device.JointDeviceSlotNumber != -1)
                            {
                                var tempDS4Device = DS4Controllers[device.JointDeviceSlotNumber];
                                if (tempDS4Device != null)
                                    EstablishOutFeedback(device.JointDeviceSlotNumber, OutContType.X360, tempXbox,
                                        tempDS4Device);
                            }
                        }

                        outputDevices[index] = tempXbox;
                        slotDevice.CurrentType = contType;
                        success = true;
                    }

                    if (success)
                        LogDebug(
                            $"Associate X360 Controller in{(slotDevice.PermanentType != OutContType.None ? " permanent" : "")} slot #{slotDevice.Index + 1} for input {device.DisplayName} controller #{index + 1}");

                    //tempXbox.Connect();
                    //LogDebug("X360 Controller #" + (index + 1) + " connected");
                }
                else if (contType == OutContType.DS4)
                {
                    ActiveOutDevType[index] = OutContType.DS4;
                    if (slotDevice == null)
                    {
                        slotDevice = OutputslotMan.FindOpenSlot();
                        if (slotDevice != null)
                        {
                            var tempDS4 = EstablishOutDevice(index, OutContType.DS4)
                                as DS4OutDevice;

                            // Enable ViGem feedback callback handler only if DS4 lightbar/rumble data output is enabled (if those are disabled then no point enabling ViGem callback handler call)
                            if (Instance.Config.EnableOutputDataToDS4[index])
                            {
                                EstablishOutFeedback(index, OutContType.DS4, tempDS4, device);

                                if (device.JointDeviceSlotNumber != -1)
                                {
                                    var tempDS4Device = DS4Controllers[device.JointDeviceSlotNumber];
                                    if (tempDS4Device != null)
                                        EstablishOutFeedback(device.JointDeviceSlotNumber, OutContType.DS4, tempDS4,
                                            tempDS4Device);
                                }
                            }

                            OutputslotMan.DeferredPlugin(tempDS4, index, outputDevices, contType);
                            //slotDevice.CurrentInputBound = OutSlotDevice.InputBound.Bound;

                            LogDebug("Plugging in virtual DS4 Controller");
                            success = true;
                        }
                        else
                        {
                            LogDebug("Failed. No open output slot found");
                        }
                    }
                    else
                    {
                        slotDevice.CurrentInputBound = OutSlotDevice.InputBound.Bound;
                        var tempDS4 = slotDevice.OutputDevice as DS4OutDevice;

                        // Enable ViGem feedback callback handler only if lightbar/rumble data output is enabled (if those are disabled then no point enabling ViGem callback handler call)
                        if (Instance.Config.EnableOutputDataToDS4[index])
                        {
                            EstablishOutFeedback(index, OutContType.DS4, tempDS4, device);

                            if (device.JointDeviceSlotNumber != -1)
                            {
                                var tempDS4Device = DS4Controllers[device.JointDeviceSlotNumber];
                                if (tempDS4Device != null)
                                    EstablishOutFeedback(device.JointDeviceSlotNumber, OutContType.DS4, tempDS4,
                                        tempDS4Device);
                            }
                        }

                        outputDevices[index] = tempDS4;
                        slotDevice.CurrentType = contType;
                        success = true;
                    }

                    if (success)
                        LogDebug(
                            $"Associate DS4 Controller in{(slotDevice.PermanentType != OutContType.None ? " permanent" : "")} slot #{slotDevice.Index + 1} for input {device.DisplayName} controller #{index + 1}");

                    //DS4OutDevice tempDS4 = new DS4OutDevice(vigemTestClient);
                    //DS4OutDevice tempDS4 = outputslotMan.AllocateController(OutContType.DS4, vigemTestClient)
                    //    as DS4OutDevice;
                    //outputDevices[index] = tempDS4;

                    //tempDS4.Connect();
                    //LogDebug("DS4 Controller #" + (index + 1) + " connected");
                }

                if (success) UseDirectInputOnly[index] = false;
            }
        }

        public void UnplugOutDev(int index, DS4Device device, bool immediate = false, bool force = false)
        {
            if (UseDirectInputOnly[index]) return;

            //OutContType contType = Global.OutContType[index];
            var dev = outputDevices[index];
            var slotDevice = OutputslotMan.GetOutSlotDevice(dev);
            if (dev != null && slotDevice != null)
            {
                var tempType = dev.GetDeviceType();
                LogDebug(
                    $"Disassociate {tempType} Controller from{(slotDevice.CurrentReserveStatus == OutSlotDevice.ReserveStatus.Permanent ? " permanent" : "")} slot #{slotDevice.Index + 1} for input {device.DisplayName} controller #{index + 1}");

                var currentType = ActiveOutDevType[index];
                outputDevices[index] = null;
                ActiveOutDevType[index] = OutContType.None;
                if (slotDevice.CurrentAttachedStatus == OutSlotDevice.AttachedStatus.Attached &&
                    slotDevice.CurrentReserveStatus == OutSlotDevice.ReserveStatus.Dynamic || force)
                {
                    //slotDevice.CurrentInputBound = OutSlotDevice.InputBound.Unbound;
                    OutputslotMan.DeferredRemoval(dev, index, outputDevices, immediate);
                    LogDebug($"Unplugging virtual {tempType} Controller");
                }
                else if (slotDevice.CurrentAttachedStatus == OutSlotDevice.AttachedStatus.Attached)
                {
                    slotDevice.CurrentInputBound = OutSlotDevice.InputBound.Unbound;
                    dev.ResetState();
                    dev.RemoveFeedbacks();
                    //RemoveOutFeedback(currentType, dev);
                }
                //dev.Disconnect();
                //LogDebug(tempType + " Controller # " + (index + 1) + " unplugged");
            }

            UseDirectInputOnly[index] = true;
        }

        public async Task<bool> Start(bool showInLog = true)
        {
            inServiceTask = true;
            
            if (OutputslotMan.Client != null)
            {
                if (showInLog)
                    LogDebug(Resources.Starting);

                LogDebug($"Using output KB+M handler: {outputKBMHandler.GetFullDisplayName()}");
                LogDebug($"Connection to ViGEmBus {ViGEmBusVersion} established");

                DS4Devices.isExclusiveMode = Instance.Config.UseExclusiveMode; //Re-enable Exclusive Mode

                UpdateHidHiddenAttributes();

                //uiContext = tempui as SynchronizationContext;
                if (showInLog)
                {
                    LogDebug(Resources.SearchingController);
                    LogDebug(DS4Devices.isExclusiveMode ? Resources.UsingExclusive : Resources.UsingShared);
                }

                if (Instance.Config.IsUdpServerEnabled && _udpServer == null)
                {
                    ChangeUDPStatus(true, false);
                    while (udpChangeStatus) Thread.SpinWait(500);
                }

                try
                {
                    loopControllers = true;
                    AssignInitialDevices();

                    EventDispatcher.Invoke(() => { DS4Devices.FindControllers(); });

                    var devices = DS4Devices.GetDS4Controllers();
                    var numControllers = new List<DS4Device>(devices).Count;
                    activeControllers = numControllers;
                    //int ind = 0;
                    DS4LightBar.defaultLight = false;
                    //foreach (DS4Device device in devices)
                    //for (int i = 0, devCount = devices.Count(); i < devCount; i++)
                    var i = 0;
                    JoyConDevice tempPrimaryJoyDev = null;
                    for (var devEnum = devices.GetEnumerator(); devEnum.MoveNext() && loopControllers; i++)
                    {
                        var device = devEnum.Current;
                        if (showInLog)
                            LogDebug(Resources.FoundController + " " + device.MacAddress + " (" +
                                     device.GetConnectionType() + ") (" +
                                     device.DisplayName + ")");

                        if (hidDeviceHidingEnabled && CheckAffected(device))
                            //device.CurrentExclusiveStatus = DS4Device.ExclusiveStatus.HidGuardAffected;
                            ChangeExclusiveStatus(device);

                        var task = new Task(() =>
                        {
                            Thread.Sleep(5);
                            WarnExclusiveModeFailure(device);
                        });
                        task.Start();

                        PrepareDs4DeviceSettingHooks(device);

                        if (DeviceOptions.JoyConDeviceOpts.LinkedMode == JoyConDeviceOptions.LinkMode.Joined)
                            if (device.DeviceType is InputDeviceType.JoyConL or InputDeviceType.JoyConR && device.PerformStateMerge)
                            {
                                if (tempPrimaryJoyDev == null)
                                {
                                    tempPrimaryJoyDev = device as JoyConDevice;
                                }
                                else
                                {
                                    var currentJoyDev = device as JoyConDevice;
                                    tempPrimaryJoyDev.JointDevice = currentJoyDev;
                                    currentJoyDev.JointDevice = tempPrimaryJoyDev;

                                    tempPrimaryJoyDev.JointState = currentJoyDev.JointState;

                                    var parentJoy = tempPrimaryJoyDev;
                                    tempPrimaryJoyDev.Removal += (sender, args) =>
                                    {
                                        currentJoyDev.JointDevice = null;
                                    };
                                    currentJoyDev.Removal += (sender, args) => { parentJoy.JointDevice = null; };

                                    tempPrimaryJoyDev = null;
                                }
                            }

                        DS4Controllers[i] = device;
                        device.DeviceSlotNumber = i;

                        Instance.Config.RefreshExtrasButtons(i, GetKnownExtraButtons(device));
                        Instance.Config.LoadControllerConfigs(device);

                        device.LoadStoreSettings();
                        device.CheckControllerNumDeviceSettings(numControllers);

                        slotManager.AddController(device, i);

                        device.Removal += On_DS4Removal;
                        device.Removal += DS4Devices.On_Removal;
                        device.SyncChange += On_SyncChange;
                        device.SyncChange += DS4Devices.UpdateSerial;
                        device.SerialChange += On_SerialChange;
                        device.ChargingChanged += CheckQuickCharge;

                        touchPad[i] = new Mouse(i, device);

                        var profileLoaded = false;
                        var useAutoProfile = UseTempProfiles[i];

                        if (!useAutoProfile)
                        {
                            if (device.IsValidSerial() && Instance.Config.ContainsLinkedProfile(device.MacAddress))
                            {
                                Instance.Config.ProfilePath[i] = Instance.Config.GetLinkedProfile(device.MacAddress);
                                LinkedProfileCheck[i] = true;
                            }
                            else
                            {
                                Instance.Config.ProfilePath[i] = Instance.Config.OlderProfilePath[i];
                                LinkedProfileCheck[i] = false;
                            }

                            profileLoaded = await Instance.LoadProfile(i, false, this, false, false);
                        }

                        if (profileLoaded || useAutoProfile)
                        {
                            device.LightBarColor = Instance.Config.GetMainColor(i);

                            if (!Instance.Config.GetDirectInputOnly(i) && device.IsSynced())
                            {
                                if (device.PrimaryDevice)
                                {
                                    PluginOutDev(i, device);
                                }
                                else if (device.JointDeviceSlotNumber != DS4Device.DEFAULT_JOINT_SLOT_NUMBER)
                                {
                                    var otherIdx = device.JointDeviceSlotNumber;
                                    var tempOutDev = outputDevices[otherIdx];
                                    if (tempOutDev != null)
                                    {
                                        var tempConType = ActiveOutDevType[otherIdx];
                                        EstablishOutFeedback(i, tempConType, tempOutDev, device);
                                        outputDevices[i] = tempOutDev;
                                        ActiveOutDevType[i] = tempConType;
                                    }
                                }
                            }
                            else
                            {
                                UseDirectInputOnly[i] = true;
                                ActiveOutDevType[i] = OutContType.None;
                            }

                            if (device.PrimaryDevice && device.OutputMapGyro)
                            {
                                TouchPadOn(i, device);
                            }
                            else if (device.JointDeviceSlotNumber != DS4Device.DEFAULT_JOINT_SLOT_NUMBER)
                            {
                                var otherIdx = device.JointDeviceSlotNumber;
                                var tempDev = DS4Controllers[otherIdx];
                                if (tempDev != null)
                                {
                                    var mappedIdx = tempDev.PrimaryDevice ? otherIdx : i;
                                    var gyroDev = device.OutputMapGyro ? device :
                                        tempDev.OutputMapGyro ? tempDev : null;
                                    if (gyroDev != null) TouchPadOn(mappedIdx, gyroDev);
                                }
                            }

                            CheckProfileOptions(i, device, true);
                            SetupInitialHookEvents(i, device);
                        }

                        var tempIdx = i;
                        device.Report += (sender, e) => { On_Report(sender, e, tempIdx); };

                        if (_udpServer != null && i < UdpServer.NUMBER_SLOTS && device.PrimaryDevice)
                            PrepareDevUDPMotion(device, tempIdx);

                        device.StartUpdate();
                        //string filename = ProfilePath[ind];
                        //ind++;

                        if (i >= CURRENT_DS4_CONTROLLER_LIMIT) // out of Xinput devices!
                            break;
                    }
                }
                catch (Exception e)
                {
                    LogDebug(e.Message, true);
                    AppLogger.Instance.LogToTray(e.Message, true);
                }

                IsRunning = true;

                if (_udpServer != null)
                {
                    //var UDP_SERVER_PORT = 26760;
                    var UDP_SERVER_PORT = Instance.Config.UdpServerPort;
                    var UDP_SERVER_LISTEN_ADDRESS = Instance.Config.UdpServerListenAddress;

                    try
                    {
                        _udpServer.Start(UDP_SERVER_PORT, UDP_SERVER_LISTEN_ADDRESS);
                        LogDebug($"UDP server listening on address {UDP_SERVER_LISTEN_ADDRESS} port {UDP_SERVER_PORT}");
                    }
                    catch (SocketException ex)
                    {
                        var errMsg =
                            $"Couldn't start UDP server on address {UDP_SERVER_LISTEN_ADDRESS}:{UDP_SERVER_PORT}, outside applications won't be able to access pad data ({ex.SocketErrorCode})";

                        LogDebug(errMsg, true);
                        AppLogger.Instance.LogToTray(errMsg, true, true);
                    }
                }
            }
            else
            {
                var logMessage = string.Empty;
                if (!IsViGEmInstalled)
                    logMessage = "ViGEmBus is not installed";
                else if (!IsRunningSupportedViGEmBus)
                    logMessage =
                        $"Unsupported ViGEmBus found ({ViGEmBusVersion}). Please install at least ViGEmBus 1.17.333.0";
                else
                    logMessage =
                        "Could not connect to ViGEmBus. Please check the status of the System device in Device Manager and if Visual C++ 2017 Redistributable is installed.";

                LogDebug(logMessage);
                AppLogger.Instance.LogToTray(logMessage);
            }

            inServiceTask = false;
            Instance.RunHotPlug = true;
            ServiceStarted?.Invoke(this, EventArgs.Empty);
            RunningChanged?.Invoke(this, EventArgs.Empty);
            return true;
        }

        private void PrepareDevUDPMotion(DS4Device device, int index)
        {
            var tempIdx = index;
            DS4Device.ReportHandler<EventArgs> tempEvnt = (sender, args) =>
            {
                var padDetail = new DualShockPadMeta();
                GetPadDetailForIdx(tempIdx, ref padDetail);
                var stateForUdp = TempState[tempIdx];

                CurrentState[tempIdx].CopyTo(stateForUdp);
                if (Instance.Config.UseUdpSmoothing)
                {
                    if (stateForUdp.elapsedTime == 0)
                        // No timestamp was found. Exit out of routine
                        return;

                    var rate = 1.0 / stateForUdp.elapsedTime;
                    var accelFilter = udpEuroPairAccel[tempIdx];
                    stateForUdp.Motion.accelXG = accelFilter.Axis1Filter.Filter(stateForUdp.Motion.accelXG, rate);
                    stateForUdp.Motion.accelYG = accelFilter.Axis2Filter.Filter(stateForUdp.Motion.accelYG, rate);
                    stateForUdp.Motion.accelZG = accelFilter.Axis3Filter.Filter(stateForUdp.Motion.accelZG, rate);

                    var gyroFilter = udpEuroPairGyro[tempIdx];
                    stateForUdp.Motion.angVelYaw = gyroFilter.Axis1Filter.Filter(stateForUdp.Motion.angVelYaw, rate);
                    stateForUdp.Motion.angVelPitch =
                        gyroFilter.Axis2Filter.Filter(stateForUdp.Motion.angVelPitch, rate);
                    stateForUdp.Motion.angVelRoll = gyroFilter.Axis3Filter.Filter(stateForUdp.Motion.angVelRoll, rate);
                }

                _udpServer.NewReportIncoming(ref padDetail, stateForUdp, udpOutBuffers[tempIdx]);
            };

            device.MotionEvent = tempEvnt;
            device.Report += tempEvnt;
        }

        private void CheckQuickCharge(DS4Device device)
        {
            if (device.ConnectionType == ConnectionType.BT && Instance.Config.QuickCharge &&
                device.Charging)
                // Set disconnect flag here. Later Hotplug event will check
                // for presence of flag and remove the device then
                device.ReadyQuickChargeDisconnect = true;
        }

        public void PrepareAbort()
        {
            for (int i = 0, arlength = DS4Controllers.Length; i < arlength; i++)
            {
                var tempDevice = DS4Controllers[i];
                if (tempDevice != null) tempDevice.PrepareAbort();
            }
        }

        public bool Stop(bool showInLog = true, bool immediateUnplug = false)
        {
            if (IsRunning)
            {
                IsRunning = false;
                Instance.RunHotPlug = false;
                inServiceTask = true;
                PreServiceStop?.Invoke(this, EventArgs.Empty);

                if (showInLog)
                    LogDebug(Resources.StoppingX360);

                LogDebug("Closing connection to ViGEmBus");

                var anyUnplugged = false;
                for (int i = 0, controllerCount = DS4Controllers.Length; i < controllerCount; i++)
                {
                    var tempDevice = DS4Controllers[i];
                    
                    if (tempDevice == null) continue;

                    if (appSettings.Settings.DisconnectBluetoothAtStop && !tempDevice.IsCharging() || suspending)
                    {
                        if (tempDevice.GetConnectionType() == ConnectionType.BT)
                        {
                            tempDevice.StopUpdate();
                            tempDevice.DisconnectBT(true);
                        }
                        else if (tempDevice.GetConnectionType() == ConnectionType.SONYWA)
                        {
                            tempDevice.StopUpdate();
                            tempDevice.DisconnectDongle(true);
                        }
                        else
                        {
                            tempDevice.StopUpdate();
                        }
                    }
                    else
                    {
                        DS4LightBar.forcelight[i] = false;
                        DS4LightBar.forcedFlash[i] = 0;
                        DS4LightBar.defaultLight = true;
                        DS4LightBar.UpdateLightBar(DS4Controllers[i], i);
                        tempDevice.IsRemoved = true;
                        tempDevice.StopUpdate();
                        DS4Devices.RemoveDevice(tempDevice);
                        Thread.Sleep(50);
                    }

                    CurrentState[i].Battery =
                        PreviousState[i].Battery = 0; // Reset for the next connection's initial status change.
                    var tempout = outputDevices[i];
                    if (tempout != null)
                    {
                        UnplugOutDev(i, tempDevice, immediateUnplug, true);
                        anyUnplugged = true;
                    }

                    //outputDevices[i] = null;
                    //UseDirectInputOnly[i] = true;
                    //Global.ActiveOutDevType[i] = OutContType.None;
                    UseDirectInputOnly[i] = true;
                    DS4Controllers[i] = null;
                    touchPad[i] = null;
                    lag[i] = false;
                    inWarnMonitor[i] = false;
                }

                if (showInLog)
                    LogDebug(Resources.StoppingDS4);

                DS4Devices.StopControllers();
                slotManager.ClearControllerList();

                if (_udpServer != null) ChangeUDPStatus(false);

                if (showInLog)
                    LogDebug(Resources.StoppedDS4Windows);

                while (OutputslotMan.RunningQueue) Thread.SpinWait(500);
                OutputslotMan.Stop(true);

                if (anyUnplugged) Thread.Sleep(OutputSlotManager.DELAY_TIME);

                inServiceTask = false;
                activeControllers = 0;
            }

            Instance.RunHotPlug = false;
            ServiceStopped?.Invoke(this, EventArgs.Empty);
            RunningChanged?.Invoke(this, EventArgs.Empty);
            return true;
        }

        public async Task<bool> HotPlug()
        {
            if (!IsRunning) return true;

            inServiceTask = true;
            loopControllers = true;
            EventDispatcher.Invoke(DS4Devices.FindControllers);

            var devices = DS4Devices.GetDS4Controllers();
            var numControllers = new List<DS4Device>(devices).Count;
            activeControllers = numControllers;
            //foreach (DS4Device device in devices)
            //for (int i = 0, devlen = devices.Count(); i < devlen; i++)
            JoyConDevice tempPrimaryJoyDev = null;
            JoyConDevice tempSecondaryJoyDev = null;

            if (DeviceOptions.JoyConDeviceOpts.LinkedMode == JoyConDeviceOptions.LinkMode.Joined)
            {
                tempPrimaryJoyDev = devices.FirstOrDefault(d =>
                    d.DeviceType is InputDeviceType.JoyConL or InputDeviceType.JoyConR
                    && d.PrimaryDevice && d.JointDeviceSlotNumber == -1) as JoyConDevice;

                tempSecondaryJoyDev = devices.FirstOrDefault(d =>
                    d.DeviceType is InputDeviceType.JoyConL or InputDeviceType.JoyConR
                    && !d.PrimaryDevice && d.JointDeviceSlotNumber == -1) as JoyConDevice;
            }

            for (var devEnum = devices.GetEnumerator(); devEnum.MoveNext() && loopControllers;)
            {
                var device = devEnum.Current;

                if (device.IsDisconnectingStatus())
                    continue;

                if (((Func<bool>)delegate
                {
                    for (int Index = 0, arlength = DS4Controllers.Length; Index < arlength; Index++)
                        if (DS4Controllers[Index] != null &&
                            DS4Controllers[Index].MacAddress == device.MacAddress)
                        {
                            device.CheckControllerNumDeviceSettings(numControllers);
                            return true;
                        }

                    return false;
                })())
                    continue;

                for (int Index = 0, controllerCount = DS4Controllers.Length;
                    Index < controllerCount && Index < CURRENT_DS4_CONTROLLER_LIMIT;
                    Index++)
                    if (DS4Controllers[Index] == null)
                    {
                        //LogDebug(DS4WinWPF.Properties.Resources.FoundController + device.getMacAddress() + " (" + device.getConnectionType() + ")");
                        LogDebug(Resources.FoundController + " " + device.MacAddress + " (" +
                                 device.GetConnectionType() + ") (" +
                                 device.DisplayName + ")");

                        if (hidDeviceHidingEnabled && CheckAffected(device))
                            //device.CurrentExclusiveStatus = DS4Device.ExclusiveStatus.HidGuardAffected;
                            ChangeExclusiveStatus(device);

                        var task = new Task(() =>
                        {
                            Thread.Sleep(5);
                            WarnExclusiveModeFailure(device);
                        });
                        task.Start();

                        PrepareDs4DeviceSettingHooks(device);

                        if (DeviceOptions.JoyConDeviceOpts.LinkedMode == JoyConDeviceOptions.LinkMode.Joined)
                            if (device.DeviceType is InputDeviceType.JoyConL or InputDeviceType.JoyConR && device.PerformStateMerge)
                            {
                                switch (device.PrimaryDevice)
                                {
                                    case true when tempSecondaryJoyDev != null:
                                    {
                                        var currentJoyDev = device as JoyConDevice;
                                        tempSecondaryJoyDev.JointDevice = currentJoyDev;
                                        currentJoyDev.JointDevice = tempSecondaryJoyDev;

                                        tempSecondaryJoyDev.JointState = currentJoyDev.JointState;

                                        var secondaryJoy = tempSecondaryJoyDev;
                                        secondaryJoy.Removal += (sender, args) => { currentJoyDev.JointDevice = null; };
                                        currentJoyDev.Removal += (sender, args) => { secondaryJoy.JointDevice = null; };

                                        tempSecondaryJoyDev = null;
                                        tempPrimaryJoyDev = null;
                                        break;
                                    }
                                    case false when tempPrimaryJoyDev != null:
                                    {
                                        var currentJoyDev = device as JoyConDevice;
                                        tempPrimaryJoyDev.JointDevice = currentJoyDev;
                                        currentJoyDev.JointDevice = tempPrimaryJoyDev;

                                        tempPrimaryJoyDev.JointState = currentJoyDev.JointState;

                                        var parentJoy = tempPrimaryJoyDev;
                                        tempPrimaryJoyDev.Removal += (sender, args) =>
                                        {
                                            currentJoyDev.JointDevice = null;
                                        };
                                        currentJoyDev.Removal += (sender, args) => { parentJoy.JointDevice = null; };

                                        tempPrimaryJoyDev = null;
                                        break;
                                    }
                                }
                            }

                        DS4Controllers[Index] = device;
                        device.DeviceSlotNumber = Index;

                        Instance.Config.RefreshExtrasButtons(Index, GetKnownExtraButtons(device));
                        Instance.Config.LoadControllerConfigs(device);
                        device.LoadStoreSettings();
                        device.CheckControllerNumDeviceSettings(numControllers);

                        slotManager.AddController(device, Index);
                        device.Removal += On_DS4Removal;
                        device.Removal += DS4Devices.On_Removal;
                        device.SyncChange += On_SyncChange;
                        device.SyncChange += DS4Devices.UpdateSerial;
                        device.SerialChange += On_SerialChange;
                        device.ChargingChanged += CheckQuickCharge;

                        touchPad[Index] = new Mouse(Index, device);
                        var profileLoaded = false;
                        var useAutoProfile = UseTempProfiles[Index];
                        if (!useAutoProfile)
                        {
                            if (device.IsValidSerial() && Instance.Config.ContainsLinkedProfile(device.MacAddress))
                            {
                                Instance.Config.ProfilePath[Index] =
                                    Instance.Config.GetLinkedProfile(device.MacAddress);
                                LinkedProfileCheck[Index] = true;
                            }
                            else
                            {
                                Instance.Config.ProfilePath[Index] = Instance.Config.OlderProfilePath[Index];
                                LinkedProfileCheck[Index] = false;
                            }

                            profileLoaded = await Instance.LoadProfile(Index, false, this, false, false);
                        }

                        if (profileLoaded || useAutoProfile)
                        {
                            device.LightBarColor = Instance.Config.GetMainColor(Index);

                            if (!Instance.Config.GetDirectInputOnly(Index) && device.IsSynced())
                            {
                                if (device.PrimaryDevice)
                                {
                                    PluginOutDev(Index, device);
                                }
                                else if (device.JointDeviceSlotNumber != DS4Device.DEFAULT_JOINT_SLOT_NUMBER)
                                {
                                    var otherIdx = device.JointDeviceSlotNumber;
                                    var tempOutDev = outputDevices[otherIdx];
                                    if (tempOutDev != null)
                                    {
                                        var tempConType = ActiveOutDevType[otherIdx];
                                        EstablishOutFeedback(Index, tempConType, tempOutDev, device);
                                        outputDevices[Index] = tempOutDev;
                                        ActiveOutDevType[Index] = tempConType;
                                    }
                                }
                            }
                            else
                            {
                                UseDirectInputOnly[Index] = true;
                                ActiveOutDevType[Index] = OutContType.None;
                            }

                            if (device.PrimaryDevice && device.OutputMapGyro)
                            {
                                TouchPadOn(Index, device);
                            }
                            else if (device.JointDeviceSlotNumber != DS4Device.DEFAULT_JOINT_SLOT_NUMBER)
                            {
                                var otherIdx = device.JointDeviceSlotNumber;
                                var tempDev = DS4Controllers[otherIdx];
                                if (tempDev != null)
                                {
                                    var mappedIdx = tempDev.PrimaryDevice ? otherIdx : Index;
                                    var gyroDev = device.OutputMapGyro ? device :
                                        tempDev.OutputMapGyro ? tempDev : null;
                                    if (gyroDev != null) TouchPadOn(mappedIdx, gyroDev);
                                }
                            }

                            CheckProfileOptions(Index, device);
                            SetupInitialHookEvents(Index, device);
                        }

                        var tempIdx = Index;
                        device.Report += (sender, e) => { On_Report(sender, e, tempIdx); };

                        if (_udpServer != null && Index < UdpServer.NUMBER_SLOTS && device.PrimaryDevice)
                            PrepareDevUDPMotion(device, tempIdx);

                        device.StartUpdate();
                        HotplugController?.Invoke(this, device, Index);
                        break;
                    }
            }

            inServiceTask = false;

            return true;
        }
        
        public void CheckProfileOptions(int ind, DS4Device device, bool startUp = false)
        {
            device.ModifyFeatureSetFlag(VidPidFeatureSet.NoOutputData, !Instance.Config.GetEnableOutputDataToDS4(ind));
            if (!Instance.Config.GetEnableOutputDataToDS4(ind))
                LogDebug(
                    "Output data to DS4 disabled. Lightbar and rumble events are not written to DS4 gamepad. If the gamepad is connected over BT then IdleDisconnect option is recommended to let DS4Windows to close the connection after long period of idling.");

            device.SetIdleTimeout(Instance.Config.GetIdleDisconnectTimeout(ind));
            device.SetBtPollRate(Instance.Config.GetBluetoothPollRate(ind));
            touchPad[ind].ResetTrackAccel(Instance.Config.GetTrackballFriction(ind));
            touchPad[ind].ResetToggleGyroModes();

            // Reset current flick stick progress from previous profile
            Mapping.flickMappingData[ind].Reset();

            Instance.Config.L2OutputSettings[ind].TrigEffectSettings.maxValue =
                (byte)(Math.Max(Instance.Config.L2ModInfo[ind].maxOutput, Instance.Config.L2ModInfo[ind].maxZone) /
                    100.0 * 255);
            Instance.Config.R2OutputSettings[ind].TrigEffectSettings.maxValue =
                (byte)(Math.Max(Instance.Config.R2ModInfo[ind].maxOutput, Instance.Config.R2ModInfo[ind].maxZone) /
                    100.0 * 255);

            device.PrepareTriggerEffect(TriggerId.LeftTrigger, Instance.Config.L2OutputSettings[ind].TriggerEffect,
                Instance.Config.L2OutputSettings[ind].TrigEffectSettings);
            device.PrepareTriggerEffect(TriggerId.RightTrigger, Instance.Config.R2OutputSettings[ind].TriggerEffect,
                Instance.Config.R2OutputSettings[ind].TrigEffectSettings);

            device.RumbleAutostopTime = Instance.Config.GetRumbleAutostopTime(ind);
            device.SetRumble(0, 0);
            device.LightBarColor = Instance.Config.GetMainColor(ind);

            if (!startUp) CheckLaunchProfileOption(ind, device);
        }

        private void CheckLaunchProfileOption(int ind, DS4Device device)
        {
            var programPath = Instance.Config.LaunchProgram[ind];
            if (programPath != string.Empty)
            {
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
        }

        private void SetupInitialHookEvents(int ind, DS4Device device)
        {
            ResetUdpSmoothingFilters(ind);

            // Set up filter for new input device
            var tempFilter = new OneEuroFilter(OneEuroFilterPair.DEFAULT_WHEEL_CUTOFF,
                OneEuroFilterPair.DEFAULT_WHEEL_BETA);
            Mapping.wheelFilters[ind] = tempFilter;

            // Carry over initial profile wheel smoothing values to filter instances.
            // Set up event hooks to keep values in sync
            var wheelSmoothInfo = Instance.Config.WheelSmoothInfo[ind];
            wheelSmoothInfo.SetFilterAttrs(tempFilter);
            wheelSmoothInfo.SetRefreshEvents(tempFilter);

            var flickStickSettings = Instance.Config.LSOutputSettings[ind].OutputSettings.flickSettings;
            flickStickSettings.RemoveRefreshEvents();
            flickStickSettings.SetRefreshEvents(Mapping.flickMappingData[ind].flickFilter);

            flickStickSettings = Instance.Config.RSOutputSettings[ind].OutputSettings.flickSettings;
            flickStickSettings.RemoveRefreshEvents();
            flickStickSettings.SetRefreshEvents(Mapping.flickMappingData[ind].flickFilter);

            var tempIdx = ind;
            Instance.Config.L2OutputSettings[ind].ResetEvents();
            Instance.Config.L2ModInfo[ind].ResetEvents();
            Instance.Config.L2OutputSettings[ind].TriggerEffectChanged += (sender, e) =>
            {
                device.PrepareTriggerEffect(TriggerId.LeftTrigger,
                    Instance.Config.L2OutputSettings[tempIdx].TriggerEffect,
                    Instance.Config.L2OutputSettings[tempIdx].TrigEffectSettings);
            };
            Instance.Config.L2ModInfo[ind].MaxOutputChanged += (sender, e) =>
            {
                var tempInfo = sender as TriggerDeadZoneZInfo;
                Instance.Config.L2OutputSettings[tempIdx].TrigEffectSettings.maxValue =
                    (byte)(Math.Max(tempInfo.maxOutput, tempInfo.maxZone) / 100.0 * 255.0);

                // Refresh trigger effect
                device.PrepareTriggerEffect(TriggerId.LeftTrigger,
                    Instance.Config.L2OutputSettings[tempIdx].TriggerEffect,
                    Instance.Config.L2OutputSettings[tempIdx].TrigEffectSettings);
            };
            Instance.Config.L2ModInfo[ind].MaxZoneChanged += (sender, e) =>
            {
                var tempInfo = sender as TriggerDeadZoneZInfo;
                Instance.Config.L2OutputSettings[tempIdx].TrigEffectSettings.maxValue =
                    (byte)(Math.Max(tempInfo.maxOutput, tempInfo.maxZone) / 100.0 * 255.0);

                // Refresh trigger effect
                device.PrepareTriggerEffect(TriggerId.LeftTrigger,
                    Instance.Config.L2OutputSettings[tempIdx].TriggerEffect,
                    Instance.Config.L2OutputSettings[tempIdx].TrigEffectSettings);
            };

            Instance.Config.R2OutputSettings[ind].ResetEvents();
            Instance.Config.R2OutputSettings[ind].TriggerEffectChanged += (sender, e) =>
            {
                device.PrepareTriggerEffect(TriggerId.RightTrigger,
                    Instance.Config.R2OutputSettings[tempIdx].TriggerEffect,
                    Instance.Config.R2OutputSettings[tempIdx].TrigEffectSettings);
            };
            Instance.Config.R2ModInfo[ind].MaxOutputChanged += (sender, e) =>
            {
                var tempInfo = sender as TriggerDeadZoneZInfo;
                Instance.Config.R2OutputSettings[tempIdx].TrigEffectSettings.maxValue =
                    (byte)(tempInfo.maxOutput / 100.0 * 255.0);

                // Refresh trigger effect
                device.PrepareTriggerEffect(TriggerId.RightTrigger,
                    Instance.Config.R2OutputSettings[tempIdx].TriggerEffect,
                    Instance.Config.R2OutputSettings[tempIdx].TrigEffectSettings);
            };
            Instance.Config.R2ModInfo[ind].MaxZoneChanged += (sender, e) =>
            {
                var tempInfo = sender as TriggerDeadZoneZInfo;
                Instance.Config.R2OutputSettings[tempIdx].TrigEffectSettings.maxValue =
                    (byte)(tempInfo.maxOutput / 100.0 * 255.0);

                // Refresh trigger effect
                device.PrepareTriggerEffect(TriggerId.RightTrigger,
                    Instance.Config.R2OutputSettings[tempIdx].TriggerEffect,
                    Instance.Config.R2OutputSettings[tempIdx].TrigEffectSettings);
            };
        }

        public void TouchPadOn(int ind, DS4Device device)
        {
            var tPad = touchPad[ind];
            //ITouchpadBehaviour tPad = touchPad[ind];
            device.Touchpad.TouchButtonDown += tPad.touchButtonDown;
            device.Touchpad.TouchButtonUp += tPad.touchButtonUp;
            device.Touchpad.TouchesBegan += tPad.TouchesBegan;
            device.Touchpad.TouchesMoved += tPad.TouchesMoved;
            device.Touchpad.TouchesEnded += tPad.TouchesEnded;
            device.Touchpad.TouchUnchanged += tPad.TouchUnchanged;
            //device.Touchpad.PreTouchProcess += delegate { touchPad[ind].populatePriorButtonStates(); };
            device.Touchpad.PreTouchProcess += (sender, args) => { touchPad[ind].populatePriorButtonStates(); };
            device.SixAxis.SixAccelMoved += tPad.SixAxisMoved;
            //LogDebug("Touchpad mode for " + device.MacAddress + " is now " + tmode.ToString());
            //Log.LogToTray("Touchpad mode for " + device.MacAddress + " is now " + tmode.ToString());
        }

        public string GetDs4Battery(int index)
        {
            var d = DS4Controllers[index];
            if (d != null)
            {
                string battery;
                if (!d.IsAlive())
                    battery = "...";

                if (d.IsCharging())
                {
                    if (d.GetBattery() >= 100)
                        battery = Resources.Full;
                    else
                        battery = d.GetBattery() + "%+";
                }
                else
                {
                    battery = d.GetBattery() + "%";
                }

                return battery;
            }

            return Resources.NA;
        }

        public string getDS4Status(int index)
        {
            var d = DS4Controllers[index];
            if (d != null)
                return d.GetConnectionType() + "";
            return Resources.NoneText;
        }

        protected void On_SerialChange(object sender, EventArgs e)
        {
            var device = (DS4Device)sender;
            var ind = -1;
            for (int i = 0, arlength = MAX_DS4_CONTROLLER_COUNT; ind == -1 && i < arlength; i++)
            {
                var tempDev = DS4Controllers[i];
                if (tempDev != null && device == tempDev)
                    ind = i;
            }

            if (ind >= 0) OnDeviceSerialChange(this, ind, device.MacAddress);
        }

        protected void On_SyncChange(object sender, EventArgs e)
        {
            var device = (DS4Device)sender;
            var ind = -1;
            for (int i = 0, arlength = CURRENT_DS4_CONTROLLER_LIMIT; ind == -1 && i < arlength; i++)
            {
                var tempDev = DS4Controllers[i];
                if (tempDev != null && device == tempDev)
                    ind = i;
            }

            if (ind >= 0)
            {
                var synced = device.IsSynced();

                if (!synced)
                {
                    if (!UseDirectInputOnly[ind])
                    {
                        ActiveOutDevType[ind] = OutContType.None;
                        UnplugOutDev(ind, device);
                    }
                }
                else
                {
                    if (!Instance.Config.GetDirectInputOnly(ind))
                    {
                        touchPad[ind].ReplaceOneEuroFilterPair();
                        touchPad[ind].ReplaceOneEuroFilterPair();

                        touchPad[ind].Cursor.ReplaceOneEuroFilterPair();
                        touchPad[ind].Cursor.SetupLateOneEuroFilters();
                        PluginOutDev(ind, device);
                    }
                }
            }
        }

        //Called when DS4 is disconnected or timed out
        protected virtual void On_DS4Removal(object sender, EventArgs e)
        {
            var device = (DS4Device)sender;
            var ind = -1;
            for (int i = 0, arlength = DS4Controllers.Length; ind == -1 && i < arlength; i++)
                if (DS4Controllers[i] != null && device.MacAddress == DS4Controllers[i].MacAddress)
                    ind = i;

            if (ind != -1)
            {
                var removingStatus = false;
                lock (device.removeLocker)
                {
                    if (!device.IsRemoving)
                    {
                        removingStatus = true;
                        device.IsRemoving = true;
                    }
                }

                if (removingStatus)
                {
                    CurrentState[ind].Battery =
                        PreviousState[ind].Battery = 0; // Reset for the next connection's initial status change.
                    if (!UseDirectInputOnly[ind])
                    {
                        UnplugOutDev(ind, device);
                    }
                    else if (!device.PrimaryDevice)
                    {
                        var outDev = outputDevices[ind];
                        if (outDev != null)
                        {
                            outDev.RemoveFeedback(ind);
                            outputDevices[ind] = null;
                        }
                    }

                    // Use Task to reset device synth state and commit it
                    Task.Run(() => { Mapping.Commit(ind); }).Wait();

                    var removed = Resources.ControllerWasRemoved.Replace("*Mac address*", (ind + 1).ToString());
                    if (device.GetBattery() <= 20 &&
                        device.GetConnectionType() == ConnectionType.BT && !device.IsCharging())
                        removed += ". " + Resources.ChargeController;

                    LogDebug(removed);
                    AppLogger.Instance.LogToTray(removed);
                    /*Stopwatch sw = new Stopwatch();
                    sw.Start();
                    while (sw.ElapsedMilliseconds < XINPUT_UNPLUG_SETTLE_TIME)
                    {
                        // Use SpinWait to keep control of current thread. Using Sleep could potentially
                        // cause other events to get run out of order
                        System.Threading.Thread.SpinWait(500);
                    }
                    sw.Stop();
                    */

                    device.IsRemoved = true;
                    device.Synced = false;
                    DS4Controllers[ind] = null;
                    //eventDispatcher.Invoke(() =>
                    //{
                    slotManager.RemoveController(device, ind);
                    //});

                    touchPad[ind] = null;
                    lag[ind] = false;
                    inWarnMonitor[ind] = false;
                    UseDirectInputOnly[ind] = true;
                    ActiveOutDevType[ind] = OutContType.None;
                    /* Leave up to Auto Profile system to change the following flags? */
                    //Global.UseTempProfiles[ind] = false;
                    //Global.TempProfileNames[ind] = string.Empty;
                    //Global.TempProfileDistance[ind] = false;

                    //Thread.Sleep(XINPUT_UNPLUG_SETTLE_TIME);
                }
            }
        }

        public bool[] lag = new bool[MAX_DS4_CONTROLLER_COUNT]
            { false, false, false, false, false, false, false, false };

        public bool[] inWarnMonitor = new bool[MAX_DS4_CONTROLLER_COUNT]
            { false, false, false, false, false, false, false, false };

        private byte[] currentBattery = new byte[MAX_DS4_CONTROLLER_COUNT] { 0, 0, 0, 0, 0, 0, 0, 0 };

        private bool[] charging = new bool[MAX_DS4_CONTROLLER_COUNT]
            { false, false, false, false, false, false, false, false };

        private readonly string[] tempStrings = new string[MAX_DS4_CONTROLLER_COUNT]
        {
            string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty,
            string.Empty
        };

        // Called every time a new input report has arrived
        [ConfigurationSystemComponent]
        [HighMemoryPressure]
        protected virtual void On_Report(DS4Device device, EventArgs e, int ind)
        {
            if (ind != -1)
            {
                var devError = tempStrings[ind] = device.error;
                if (!string.IsNullOrEmpty(devError)) LogDebug(devError);

                if (inWarnMonitor[ind])
                {
                    var flashWhenLateAt = Instance.Config.FlashWhenLateAt;
                    if (!lag[ind] && device.Latency >= flashWhenLateAt)
                    {
                        lag[ind] = true;
                        LagFlashWarning(device, ind, true);
                    }
                    else if (lag[ind] && device.Latency < flashWhenLateAt)
                    {
                        lag[ind] = false;
                        LagFlashWarning(device, ind, false);
                    }
                }
                else
                {
                    if (DateTime.UtcNow - device.firstActive > TimeSpan.FromSeconds(5)) inWarnMonitor[ind] = true;
                }

                DS4State cState;
                if (!device.PerformStateMerge)
                {
                    cState = CurrentState[ind];
                    device.GetRawCurrentState(cState);
                }
                else
                {
                    cState = device.JointState;
                    device.MergeStateData(cState);
                    // Need to copy state object info for use in UDP server
                    cState.CopyTo(CurrentState[ind]);
                }

                var pState = device.GetPreviousStateReference();
                //device.getPreviousState(PreviousState[ind]);
                //DS4State pState = PreviousState[ind];

                if (device.firstReport && device.IsSynced())
                {
                    // Only send Log message when device is considered a primary device
                    if (device.PrimaryDevice)
                    {
                        if (File.Exists(Path.Combine(RuntimeAppDataPath, Constants.ProfilesSubDirectory,
                            Instance.Config.ProfilePath[ind] + ".xml")))
                        {
                            var prolog = string.Format(Resources.UsingProfile, (ind + 1).ToString(),
                                Instance.Config.ProfilePath[ind], $"{device.Battery}");
                            LogDebug(prolog);
                            AppLogger.Instance.LogToTray(prolog);
                        }
                        else
                        {
                            var prolog = string.Format(Resources.NotUsingProfile, (ind + 1).ToString(),
                                $"{device.Battery}");
                            LogDebug(prolog);
                            AppLogger.Instance.LogToTray(prolog);
                        }
                    }

                    device.firstReport = false;
                }

                if (!device.PrimaryDevice)
                    // Skip mapping routine if part of a joined device
                    return;

                if (Instance.Config.GetEnableTouchToggle(ind)) CheckForTouchToggle(ind, cState, pState);

                cState = Mapping.SetCurveAndDeadzone(ind, cState, TempState[ind]);

                if (!recordingMacro && (UseTempProfiles[ind] ||
                                        Instance.Config.ContainsCustomAction[ind] ||
                                        Instance.Config.ContainsCustomExtras[ind] ||
                                        Instance.Config.GetProfileActionCount(ind) > 0))
                {
                    var tempMapState = MappedState[ind];
                    Mapping.MapCustom(ind, cState, tempMapState, ExposedState[ind], touchPad[ind], this);

                    // Copy current Touchpad and Gyro data
                    // Might change to use new DS4State.CopyExtrasTo method
                    tempMapState.Motion = cState.Motion;
                    tempMapState.ds4Timestamp = cState.ds4Timestamp;
                    tempMapState.FrameCounter = cState.FrameCounter;
                    tempMapState.TouchPacketCounter = cState.TouchPacketCounter;
                    tempMapState.TrackPadTouch0 = cState.TrackPadTouch0;
                    tempMapState.TrackPadTouch1 = cState.TrackPadTouch1;
                    cState = tempMapState;
                }

                if (!UseDirectInputOnly[ind])
                {
                    outputDevices[ind]?.ConvertAndSendReport(cState, ind);
                    //testNewReport(ref x360reports[ind], cState, ind);
                    //x360controls[ind]?.SendReport(x360reports[ind]);

                    //x360Bus.Parse(cState, processingData[ind].Report, ind);
                    // We push the translated Xinput state, and simultaneously we
                    // pull back any possible rumble data coming from Xinput consumers.
                    /*if (x360Bus.Report(processingData[ind].Report, processingData[ind].Rumble))
                    {
                        byte Big = processingData[ind].Rumble[3];
                        byte Small = processingData[ind].Rumble[4];

                        if (processingData[ind].Rumble[1] == 0x08)
                        {
                            SetDevRumble(device, Big, Small, ind);
                        }
                    }
                    */
                }
                else
                {
                    // UseDInputOnly profile may re-map sixaxis gyro sensor values as a VJoy joystick axis (steering wheel emulation mode using VJoy output device). Handle this option because VJoy output works even in USeDInputOnly mode.
                    // If steering wheel emulation uses LS/RS/R2/L2 output axies then the profile should NOT use UseDInputOnly option at all because those require a virtual output device.
                    var steeringWheelMappedAxis = Instance.Config.GetSASteeringWheelEmulationAxis(ind);
                    switch (steeringWheelMappedAxis)
                    {
                        case SASteeringWheelEmulationAxisType.None: break;

                        case SASteeringWheelEmulationAxisType.VJoy1X:
                        case SASteeringWheelEmulationAxisType.VJoy2X:
                            vJoyFeeder.FeedAxisValue(cState.SASteeringWheelEmulationUnit,
                                ((uint)steeringWheelMappedAxis - (uint)SASteeringWheelEmulationAxisType.VJoy1X) / 3 + 1,
                                HID_USAGES.HID_USAGE_X);
                            break;

                        case SASteeringWheelEmulationAxisType.VJoy1Y:
                        case SASteeringWheelEmulationAxisType.VJoy2Y:
                            vJoyFeeder.FeedAxisValue(cState.SASteeringWheelEmulationUnit,
                                ((uint)steeringWheelMappedAxis - (uint)SASteeringWheelEmulationAxisType.VJoy1X) / 3 + 1,
                                HID_USAGES.HID_USAGE_Y);
                            break;

                        case SASteeringWheelEmulationAxisType.VJoy1Z:
                        case SASteeringWheelEmulationAxisType.VJoy2Z:
                            vJoyFeeder.FeedAxisValue(cState.SASteeringWheelEmulationUnit,
                                ((uint)steeringWheelMappedAxis - (uint)SASteeringWheelEmulationAxisType.VJoy1X) / 3 + 1,
                                HID_USAGES.HID_USAGE_Z);
                            break;
                    }
                }

                // Output any synthetic events.
                Mapping.Commit(ind);

                // Update the Lightbar color
                DS4LightBar.UpdateLightBar(device, ind);

                if (device.PerformStateMerge) device.PreserveMergedStateData();
            }
        }

        public void LagFlashWarning(DS4Device device, int ind, bool on)
        {
            if (on)
            {
                lag[ind] = true;
                LogDebug(string.Format(Resources.LatencyOverTen, ind + 1, device.Latency), true);
                if (appSettings.Settings.FlashWhenLate)
                {
                    var color = new DS4Color(50, 0, 0);
                    DS4LightBar.forcedColor[ind] = color;
                    DS4LightBar.forcedFlash[ind] = 2;
                    DS4LightBar.forcelight[ind] = true;
                }
            }
            else
            {
                lag[ind] = false;
                LogDebug(Resources.LatencyNotOverTen.Replace("*number*", (ind + 1).ToString()));
                DS4LightBar.forcelight[ind] = false;
                DS4LightBar.forcedFlash[ind] = 0;
                device.LightBarColor = Instance.Config.GetMainColor(ind);
            }
        }

        public DS4Controls GetActiveInputControl(int ind)
        {
            var cState = CurrentState[ind];
            var eState = ExposedState[ind];
            var tp = touchPad[ind];
            var result = DS4Controls.None;

            if (DS4Controllers[ind] != null)
            {
                if (Mapping.getBoolButtonMapping(cState.Cross))
                    result = DS4Controls.Cross;
                else if (Mapping.getBoolButtonMapping(cState.Circle))
                    result = DS4Controls.Circle;
                else if (Mapping.getBoolButtonMapping(cState.Triangle))
                    result = DS4Controls.Triangle;
                else if (Mapping.getBoolButtonMapping(cState.Square))
                    result = DS4Controls.Square;
                else if (Mapping.getBoolButtonMapping(cState.L1))
                    result = DS4Controls.L1;
                else if (Mapping.getBoolTriggerMapping(cState.L2))
                    result = DS4Controls.L2;
                else if (Mapping.getBoolButtonMapping(cState.L3))
                    result = DS4Controls.L3;
                else if (Mapping.getBoolButtonMapping(cState.R1))
                    result = DS4Controls.R1;
                else if (Mapping.getBoolTriggerMapping(cState.R2))
                    result = DS4Controls.R2;
                else if (Mapping.getBoolButtonMapping(cState.R3))
                    result = DS4Controls.R3;
                else if (Mapping.getBoolButtonMapping(cState.DpadUp))
                    result = DS4Controls.DpadUp;
                else if (Mapping.getBoolButtonMapping(cState.DpadDown))
                    result = DS4Controls.DpadDown;
                else if (Mapping.getBoolButtonMapping(cState.DpadLeft))
                    result = DS4Controls.DpadLeft;
                else if (Mapping.getBoolButtonMapping(cState.DpadRight))
                    result = DS4Controls.DpadRight;
                else if (Mapping.getBoolButtonMapping(cState.Share))
                    result = DS4Controls.Share;
                else if (Mapping.getBoolButtonMapping(cState.Options))
                    result = DS4Controls.Options;
                else if (Mapping.getBoolButtonMapping(cState.PS))
                    result = DS4Controls.PS;
                else if (Mapping.getBoolAxisDirMapping(cState.LX, true))
                    result = DS4Controls.LXPos;
                else if (Mapping.getBoolAxisDirMapping(cState.LX, false))
                    result = DS4Controls.LXNeg;
                else if (Mapping.getBoolAxisDirMapping(cState.LY, true))
                    result = DS4Controls.LYPos;
                else if (Mapping.getBoolAxisDirMapping(cState.LY, false))
                    result = DS4Controls.LYNeg;
                else if (Mapping.getBoolAxisDirMapping(cState.RX, true))
                    result = DS4Controls.RXPos;
                else if (Mapping.getBoolAxisDirMapping(cState.RX, false))
                    result = DS4Controls.RXNeg;
                else if (Mapping.getBoolAxisDirMapping(cState.RY, true))
                    result = DS4Controls.RYPos;
                else if (Mapping.getBoolAxisDirMapping(cState.RY, false))
                    result = DS4Controls.RYNeg;
                else if (Mapping.getBoolTouchMapping(tp.leftDown))
                    result = DS4Controls.TouchLeft;
                else if (Mapping.getBoolTouchMapping(tp.rightDown))
                    result = DS4Controls.TouchRight;
                else if (Mapping.getBoolTouchMapping(tp.multiDown))
                    result = DS4Controls.TouchMulti;
                else if (Mapping.getBoolTouchMapping(tp.upperDown))
                    result = DS4Controls.TouchUpper;
            }

            return result;
        }

        public bool[] touchreleased = new bool[MAX_DS4_CONTROLLER_COUNT]
                { true, true, true, true, true, true, true, true },
            touchslid = new bool[MAX_DS4_CONTROLLER_COUNT] { false, false, false, false, false, false, false, false };

        public Dispatcher EventDispatcher { get; private set; }

        public IOutputSlotManager OutputslotMan { get; }

        protected virtual void CheckForTouchToggle(int deviceID, DS4State cState, DS4State pState)
        {
            if (!Instance.Config.IsUsingTouchpadForControls(deviceID) && cState.Touch1 && pState.PS)
            {
                if (Instance.GetTouchActive(deviceID) && touchreleased[deviceID])
                {
                    Instance.TouchActive[deviceID] = false;
                    LogDebug(Resources.TouchpadMovementOff);
                    AppLogger.Instance.LogToTray(Resources.TouchpadMovementOff);
                    touchreleased[deviceID] = false;
                }
                else if (touchreleased[deviceID])
                {
                    Instance.TouchActive[deviceID] = true;
                    LogDebug(Resources.TouchpadMovementOn);
                    AppLogger.Instance.LogToTray(Resources.TouchpadMovementOn);
                    touchreleased[deviceID] = false;
                }
            }
            else
            {
                touchreleased[deviceID] = true;
            }
        }

        public virtual void StartTPOff(int deviceID)
        {
            if (deviceID < CURRENT_DS4_CONTROLLER_LIMIT) Instance.TouchActive[deviceID] = false;
        }

        public virtual string TouchpadSlide(int ind)
        {
            var cState = CurrentState[ind];
            var slidedir = "none";
            if (DS4Controllers[ind] != null && cState.Touch2 &&
                !(touchPad[ind].dragging || touchPad[ind].dragging2))
            {
                if (touchPad[ind].slideright && !touchslid[ind])
                {
                    slidedir = "right";
                    touchslid[ind] = true;
                }
                else if (touchPad[ind].slideleft && !touchslid[ind])
                {
                    slidedir = "left";
                    touchslid[ind] = true;
                }
                else if (!touchPad[ind].slideleft && !touchPad[ind].slideright)
                {
                    slidedir = "";
                    touchslid[ind] = false;
                }
            }

            return slidedir;
        }

        [LoggingComponent]
        public virtual void LogDebug(string data, bool isWarning = false)
        {
            //Console.WriteLine(System.DateTime.Now.ToString("G") + "> " + Data);
            if (Debug == null) return;
            var args = new LogEntryEventArgs(data, isWarning);
            OnDebug(this, args);
        }

        [LoggingComponent]
        public virtual void OnDebug(object sender, LogEntryEventArgs args)
        {
            Debug?.Invoke(this, args);
        }

        // sets the rumble adjusted with rumble boost. General use method
        public virtual void SetRumble(byte heavyMotor, byte lightMotor, int deviceNum)
        {
            if (deviceNum < CURRENT_DS4_CONTROLLER_LIMIT)
            {
                var device = DS4Controllers[deviceNum];
                if (device != null)
                    SetDevRumble(device, heavyMotor, lightMotor, deviceNum);
                //device.setRumble((byte)lightBoosted, (byte)heavyBoosted);
            }
        }

        // sets the rumble adjusted with rumble boost. Method more used for
        // report handling. Avoid constant checking for a device.
        public void SetDevRumble(DS4Device device,
            byte heavyMotor, byte lightMotor, int deviceNum)
        {
            var boost = Instance.Config.GetRumbleBoost(deviceNum);
            var lightBoosted = lightMotor * (uint)boost / 100;
            if (lightBoosted > 255)
                lightBoosted = 255;
            var heavyBoosted = heavyMotor * (uint)boost / 100;
            if (heavyBoosted > 255)
                heavyBoosted = 255;

            device.SetRumble((byte)lightBoosted, (byte)heavyBoosted);
        }

        public DS4State GetDs4State(int ind)
        {
            return CurrentState[ind];
        }

        public DS4State GetDs4StateMapped(int ind)
        {
            return MappedState[ind];
        }

        public DS4State GetDs4StateTemp(int ind)
        {
            return TempState[ind];
        }
    }
}