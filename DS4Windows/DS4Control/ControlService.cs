using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using AdonisUI.Controls;
using DS4Windows.InputDevices;
using DS4Windows.Shared.Common.Attributes;
using DS4Windows.Shared.Common.Core;
using DS4Windows.Shared.Common.Legacy;
using DS4Windows.Shared.Common.Types;
using DS4Windows.Shared.Common.Util;
using DS4Windows.Shared.Configuration.Application.Schema;
using DS4Windows.Shared.Configuration.Application.Services;
using DS4Windows.Shared.Configuration.Profiles.Services;
using DS4Windows.Shared.Devices.HID;
using DS4Windows.Shared.Emulator.ViGEmGen1.Types.Legacy;
using DS4Windows.VJoyFeeder;
using DS4WinWPF;
using DS4WinWPF.DS4Control;
using DS4WinWPF.DS4Control.Logging;
using DS4WinWPF.Properties;
using DS4WinWPF.Translations;
using Nefarius.ViGEm.Client.Targets;
using static DS4Windows.Global;
using MessageBox = AdonisUI.Controls.MessageBox;
using MessageBoxImage = AdonisUI.Controls.MessageBoxImage;

namespace DS4Windows
{
    public partial class ControlService
    {
        /// <summary>
        ///     TODO: temporary!
        /// </summary>
        [IntermediateSolution]
        public static ControlService CurrentInstance { get; private set; }

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

        private readonly IDS4DeviceEnumerator ds4devices;

        private readonly IAppSettingsService appSettings;

        private readonly IProfilesService profilesService;

        [IntermediateSolution]
        public IAppSettingsService GetAppSettings()
        {
            return appSettings;
        }

        private readonly ActivitySource activitySource = new(Constants.ApplicationName);

        public ControlService(
            ICommandLineOptions cmdParser,
            IOutputSlotManager osl,
            IAppSettingsService appSettings,
            IDS4DeviceEnumerator devices,
            IProfilesService profilesService
        )
        {
            using var activity = activitySource.StartActivity(
                $"{nameof(ControlService)}:Constructor");

            ds4devices = devices;
            this.appSettings = appSettings;
            this.cmdParser = cmdParser;
            this.profilesService = profilesService;

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
                ProfilesService.Instance.ActiveProfiles.ElementAt(i).L2OutputSettings.TwoStageModeChanged +=
                    (sender, e) => { Mapping.l2TwoStageMappingData[tempDev].Reset(); };

                ProfilesService.Instance.ActiveProfiles.ElementAt(i).R2OutputSettings.TwoStageModeChanged +=
                    (sender, e) => { Mapping.r2TwoStageMappingData[tempDev].Reset(); };
            }

            OutputslotMan = osl;

            devices.RequestElevation += DS4Devices_RequestElevation;
            devices.CheckVirtualFunc += CheckForVirtualDevice;
            devices.PrepareDs4Init += PrepareDs4DeviceInit;
            devices.PostDs4Init += PostDs4DeviceInit;
            devices.PreparePendingDevice += CheckForSupportedDevice;
            OutputslotMan.ViGEmFailure += OutputslotMan_ViGEmFailure;

            appSettings.UdpSmoothMinCutoffChanged += ChangeUdpSmoothingAttrs;
            appSettings.UdpSmoothBetaChanged += ChangeUdpSmoothingAttrs;

            CurrentInstance = this;
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
            var stringMac = d.MacAddress.ToFriendlyName();
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
                d.GetConnectionType() == ConnectionType.Usb ? DsConnection.Usb : DsConnection.Bluetooth;
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

            if (appSettings.Settings.DeviceOptions.JoyConSupportSettings.LinkedMode !=
                JoyConDeviceOptions.LinkMode.Joined) return;

            var tempJoyDev = device as JoyConDevice;
            tempJoyDev.PerformStateMerge = true;

            if (device.DeviceType == InputDeviceType.JoyConL)
            {
                tempJoyDev.PrimaryDevice = true;
                tempJoyDev.OutputMapGyro = appSettings.Settings.DeviceOptions.JoyConSupportSettings.JoinGyroProv ==
                                           JoyConDeviceOptions.JoinedGyroProvider.JoyConL;
            }
            else
            {
                tempJoyDev.PrimaryDevice = false;
                tempJoyDev.OutputMapGyro = appSettings.Settings.DeviceOptions.JoyConSupportSettings.JoinGyroProv ==
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

        public bool CheckForSupportedDevice(HidDeviceV3 device, VidPidInfo metaInfo)
        {
            switch (metaInfo.InputDevType)
            {
                case InputDeviceType.DualShock4:
                    return appSettings.Settings.DeviceOptions.DS4SupportSettings.Enabled;
                case InputDeviceType.DualSense:
                    return appSettings.Settings.DeviceOptions.DualSenseSupportSettings.Enabled;
                case InputDeviceType.SwitchPro:
                    return appSettings.Settings.DeviceOptions.SwitchProSupportSettings.Enabled;
                case InputDeviceType.JoyConL:
                case InputDeviceType.JoyConR:
                    return appSettings.Settings.DeviceOptions.JoyConSupportSettings.Enabled;
            }

            return false;
        }

        [MissingLocalization]
        private void PrepareDs4DeviceInit(DS4Device device)
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
                            $"Hello, Gamer! \r\n\r\nYour DualSense ({dualSenseDevice.MacAddress.ToFriendlyName()}) is running " +
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
                            new MessageBoxCheckBoxModel(Strings.NotAMoronConfirmationCheckbox)
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
            ds4devices.CheckVirtualFunc -= CheckForVirtualDevice;
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

        private List<DS4ControlItem> GetKnownExtraButtons(DS4Device dev)
        {
            var result = new List<DS4ControlItem>();
            switch (dev.DeviceType)
            {
                case InputDeviceType.JoyConL:
                case InputDeviceType.JoyConR:
                    result.AddRange(new[] { DS4ControlItem.Capture, DS4ControlItem.SideL, DS4ControlItem.SideR });
                    break;
                case InputDeviceType.SwitchPro:
                    result.AddRange(new[] { DS4ControlItem.Capture });
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

        public void ChangeMotionEventStatus(bool state)
        {
            var devices = ds4devices.GetDs4Controllers();
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
            if (ds4devices.IsExclusiveMode && !device.isExclusive())
            {
                var message = Resources.CouldNotOpenDS4.Replace("*Mac address*", device.MacAddress.ToFriendlyName()) +
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

        private OutputDevice EstablishOutDevice(int index, OutputDeviceType contType)
        {
            return OutputslotMan.AllocateController(contType);
        }

        public void EstablishOutFeedback(int index, OutputDeviceType contType,
            OutputDevice outDevice, DS4Device device)
        {
            var devIndex = index;

            if (contType == OutputDeviceType.Xbox360Controller)
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

        public void RemoveOutFeedback(OutputDeviceType contType, OutputDevice outDevice, int inIdx)
        {
            if (contType == OutputDeviceType.Xbox360Controller)
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

        public void AttachNewUnboundOutDev(OutputDeviceType contType)
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

        public void AttachUnboundOutDev(OutSlotDevice slotDevice, OutputDeviceType contType)
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
            var profile = profilesService.ActiveProfiles.ElementAt(index);

            if (profile.DisableVirtualController)
                return;

            var contType = profile.OutputDeviceType;

            var slotDevice = OutputslotMan.FindExistUnboundSlotType(contType);

            var success = false;
            switch (contType)
            {
                case OutputDeviceType.Xbox360Controller:
                {
                    ActiveOutDevType[index] = OutputDeviceType.Xbox360Controller;

                    if (slotDevice == null)
                    {
                        slotDevice = OutputslotMan.FindOpenSlot();
                        if (slotDevice != null)
                        {
                            var tempXbox = EstablishOutDevice(index, OutputDeviceType.Xbox360Controller)
                                as Xbox360OutDevice;
                            //outputDevices[index] = tempXbox;

                            // Enable ViGem feedback callback handler only if lightbar/rumble data output is enabled (if those are disabled then no point enabling ViGem callback handler call)
                            if (profile.EnableOutputDataToDS4)
                            {
                                EstablishOutFeedback(index, OutputDeviceType.Xbox360Controller, tempXbox, device);

                                if (device.JointDeviceSlotNumber != -1)
                                {
                                    var tempDS4Device = DS4Controllers[device.JointDeviceSlotNumber];
                                    if (tempDS4Device != null)
                                        EstablishOutFeedback(device.JointDeviceSlotNumber, OutputDeviceType.Xbox360Controller, tempXbox,
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
                        if (profile.EnableOutputDataToDS4)
                        {
                            EstablishOutFeedback(index, OutputDeviceType.Xbox360Controller, tempXbox, device);

                            if (device.JointDeviceSlotNumber != -1)
                            {
                                var tempDS4Device = DS4Controllers[device.JointDeviceSlotNumber];
                                if (tempDS4Device != null)
                                    EstablishOutFeedback(device.JointDeviceSlotNumber, OutputDeviceType.Xbox360Controller, tempXbox,
                                        tempDS4Device);
                            }
                        }

                        outputDevices[index] = tempXbox;
                        slotDevice.CurrentType = contType;
                        success = true;
                    }

                    if (success)
                        LogDebug(
                            $"Associate X360 Controller in{(slotDevice.PermanentType != OutputDeviceType.None ? " permanent" : "")} slot #{slotDevice.Index + 1} for input {device.DisplayName} controller #{index + 1}");

                    //tempXbox.Connect();
                    //LogDebug("X360 Controller #" + (index + 1) + " connected");
                    break;
                }
                case OutputDeviceType.DualShock4Controller:
                {
                    ActiveOutDevType[index] = OutputDeviceType.DualShock4Controller;
                    if (slotDevice == null)
                    {
                        slotDevice = OutputslotMan.FindOpenSlot();
                        if (slotDevice != null)
                        {
                            var tempDS4 = EstablishOutDevice(index, OutputDeviceType.DualShock4Controller)
                                as DS4OutDevice;

                            // Enable ViGem feedback callback handler only if DS4 lightbar/rumble data output is enabled (if those are disabled then no point enabling ViGem callback handler call)
                            if (profile.EnableOutputDataToDS4)
                            {
                                EstablishOutFeedback(index, OutputDeviceType.DualShock4Controller, tempDS4, device);

                                if (device.JointDeviceSlotNumber != -1)
                                {
                                    var tempDS4Device = DS4Controllers[device.JointDeviceSlotNumber];
                                    if (tempDS4Device != null)
                                        EstablishOutFeedback(device.JointDeviceSlotNumber, OutputDeviceType.DualShock4Controller, tempDS4,
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
                        if (profile.EnableOutputDataToDS4)
                        {
                            EstablishOutFeedback(index, OutputDeviceType.DualShock4Controller, tempDS4, device);

                            if (device.JointDeviceSlotNumber != -1)
                            {
                                var tempDS4Device = DS4Controllers[device.JointDeviceSlotNumber];
                                if (tempDS4Device != null)
                                    EstablishOutFeedback(device.JointDeviceSlotNumber, OutputDeviceType.DualShock4Controller, tempDS4,
                                        tempDS4Device);
                            }
                        }

                        outputDevices[index] = tempDS4;
                        slotDevice.CurrentType = contType;
                        success = true;
                    }

                    if (success)
                        LogDebug(
                            $"Associate DS4 Controller in{(slotDevice.PermanentType != OutputDeviceType.None ? " permanent" : "")} slot #{slotDevice.Index + 1} for input {device.DisplayName} controller #{index + 1}");

                    //DS4OutDevice tempDS4 = new DS4OutDevice(vigemTestClient);
                    //DS4OutDevice tempDS4 = outputslotMan.AllocateController(OutContType.DS4, vigemTestClient)
                    //    as DS4OutDevice;
                    //outputDevices[index] = tempDS4;

                    //tempDS4.Connect();
                    //LogDebug("DS4 Controller #" + (index + 1) + " connected");
                    break;
                }
            }

            if (success)
                profile.IsOutputDeviceEnabled = true;
        }

        public void UnplugOutDev(int index, DS4Device device, bool immediate = false, bool force = false)
        {
            if (profilesService.ActiveProfiles.ElementAt(index).DisableVirtualController)
                return;

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
                ActiveOutDevType[index] = OutputDeviceType.None;
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

            profilesService.ActiveProfiles.ElementAt(index).IsOutputDeviceEnabled = false;
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

                ds4devices.IsExclusiveMode = appSettings.Settings.UseExclusiveMode; //Re-enable Exclusive Mode


                UpdateHidHiddenAttributes();


                //uiContext = tempui as SynchronizationContext;
                if (showInLog)
                {
                    LogDebug(Resources.SearchingController);
                    LogDebug(ds4devices.IsExclusiveMode ? Resources.UsingExclusive : Resources.UsingShared);
                }


                if (appSettings.Settings.UseUDPServer && _udpServer == null)
                {
                    ChangeUDPStatus(true, false);
                    while (udpChangeStatus) Thread.SpinWait(500);
                }


                try
                {
                    loopControllers = true;

                    AssignInitialDevices();


                    EventDispatcher.Invoke(() =>
                    {
                        ds4devices.FindControllers(appSettings.Settings.DeviceOptions.VerboseLogMessages);
                    });


                    IList<DS4Device> devices;


                    devices = ds4devices.GetDs4Controllers().ToList();


                    var numControllers = devices.Count;
                    activeControllers = numControllers;
                    //int ind = 0;
                    DS4LightBarV3.defaultLight = false;
                    //foreach (DS4Device device in devices)
                    //for (int i = 0, devCount = devices.Count(); i < devCount; i++)
                    var i = 0;
                    JoyConDevice tempPrimaryJoyDev = null;


                    //
                    // TODO: GetEnumerator leaks memory
                    // 
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


                        if (appSettings.Settings.DeviceOptions.JoyConSupportSettings.LinkedMode ==
                            JoyConDeviceOptions.LinkMode.Joined)
                            if (device.DeviceType is InputDeviceType.JoyConL or InputDeviceType.JoyConR &&
                                device.PerformStateMerge)
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
                        device.Removal += ds4devices.On_Removal;
                        device.SyncChange += On_SyncChange;
                        device.SyncChange += ds4devices.UpdateSerial;
                        device.SerialChange += On_SerialChange;
                        device.ChargingChanged += CheckQuickCharge;

                        touchPad[i] = new Mouse(i, device);

                        var profileLoaded = true;
                        var useAutoProfile = UseTempProfiles[i];

                        profilesService.ControllerArrived(i, device.MacAddress);

                        var profile = profilesService.ActiveProfiles.ElementAt(i);

                        if (profileLoaded || useAutoProfile)
                        {
                            device.LightBarColor =
                                appSettings.Settings.LightbarSettingInfo[i].Ds4WinSettings.Led;

                            if (!profile.DisableVirtualController && device.IsSynced())
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
                                profile.IsOutputDeviceEnabled = false;
                                ActiveOutDevType[i] = OutputDeviceType.None;
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
                    var udpServerPort = appSettings.Settings.UDPServerPort;
                    var udpServerListenAddress = appSettings.Settings.UDPServerListenAddress;

                    try
                    {
                        _udpServer.Start(udpServerPort, udpServerListenAddress);
                        LogDebug($"UDP server listening on address {udpServerListenAddress} port {udpServerPort}");
                    }
                    catch (SocketException ex)
                    {
                        var errMsg =
                            $"Couldn't start UDP server on address {udpServerListenAddress}:{udpServerPort}, outside applications won't be able to access pad data ({ex.SocketErrorCode})";

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
                if (appSettings.Settings.UDPServerSmoothingOptions.UseSmoothing)
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
            if (device.ConnectionType == ConnectionType.Bluetooth && appSettings.Settings.QuickCharge &&
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
                    var profile = profilesService.ActiveProfiles.ElementAt(i);
                    var tempDevice = DS4Controllers[i];

                    if (tempDevice == null) continue;

                    if (appSettings.Settings.DisconnectBluetoothAtStop && !tempDevice.IsCharging() || suspending)
                    {
                        if (tempDevice.GetConnectionType() == ConnectionType.Bluetooth)
                        {
                            tempDevice.StopUpdate();
                            tempDevice.DisconnectBT(true);
                        }
                        else if (tempDevice.GetConnectionType() == ConnectionType.SonyWirelessAdapter)
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
                        DS4LightBarV3.forcelight[i] = false;
                        DS4LightBarV3.forcedFlash[i] = 0;
                        DS4LightBarV3.defaultLight = true;
                        DS4LightBarV3.UpdateLightBar(DS4Controllers[i], i);
                        tempDevice.IsRemoved = true;
                        tempDevice.StopUpdate();
                        ds4devices.RemoveDevice(tempDevice);
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
                    profile.IsOutputDeviceEnabled = false;
                    DS4Controllers[i] = null;
                    touchPad[i] = null;
                    lag[i] = false;
                    inWarnMonitor[i] = false;
                }

                if (showInLog)
                    LogDebug(Resources.StoppingDS4);

                ds4devices.StopControllers();
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

        /// <summary>
        ///     Gets called when devices got "hot-plugged" (meaning inserted or removed during the lifetime of the application).
        /// </summary>
        public async Task<bool> HotPlug()
        {
            if (!IsRunning) return true;

            inServiceTask = true;
            loopControllers = true;
            EventDispatcher.Invoke(() =>
            {
                ds4devices.FindControllers(appSettings.Settings.DeviceOptions.VerboseLogMessages);
            });

            var devices = ds4devices.GetDs4Controllers().ToList();
            var numControllers = devices.Count;
            activeControllers = numControllers;
            //foreach (DS4Device device in devices)
            //for (int i = 0, devlen = devices.Count(); i < devlen; i++)
            JoyConDevice tempPrimaryJoyDev = null;
            JoyConDevice tempSecondaryJoyDev = null;

            if (appSettings.Settings.DeviceOptions.JoyConSupportSettings.LinkedMode ==
                JoyConDeviceOptions.LinkMode.Joined)
            {
                tempPrimaryJoyDev = devices.FirstOrDefault(d =>
                    d.DeviceType is InputDeviceType.JoyConL or InputDeviceType.JoyConR
                    && d.PrimaryDevice && d.JointDeviceSlotNumber == -1) as JoyConDevice;

                tempSecondaryJoyDev = devices.FirstOrDefault(d =>
                    d.DeviceType is InputDeviceType.JoyConL or InputDeviceType.JoyConR
                    && !d.PrimaryDevice && d.JointDeviceSlotNumber == -1) as JoyConDevice;
            }

            //
            // TODO: GetEnumerator leaks memory
            // 
            for (var devEnum = devices.GetEnumerator(); devEnum.MoveNext() && loopControllers;)
            {
                var device = devEnum.Current;

                if (device.IsDisconnectingStatus())
                    continue;

                if (((Func<bool>)delegate
                    {
                        for (int Index = 0, arlength = DS4Controllers.Length; Index < arlength; Index++)
                            if (DS4Controllers[Index] != null &&
                                Equals(DS4Controllers[Index].MacAddress, device.MacAddress))
                            {
                                device.CheckControllerNumDeviceSettings(numControllers);
                                return true;
                            }

                        return false;
                    })())
                    continue;

                for (int index = 0, controllerCount = DS4Controllers.Length;
                     index < controllerCount && index < CURRENT_DS4_CONTROLLER_LIMIT;
                     index++)
                    if (DS4Controllers[index] == null)
                    {
                        //LogDebug(DS4WinWPF.Properties.Resources.FoundController + device.getMacAddress() + " (" + device.getConnectionType() + ")");
                        LogDebug(Resources.FoundController + " " + device.MacAddress + " (" +
                                 device.GetConnectionType() + ") (" +
                                 device.DisplayName + ")");

                        if (hidDeviceHidingEnabled && CheckAffected(device))
                            //device.CurrentExclusiveStatus = DS4Device.ExclusiveStatus.HidGuardAffected;
                            ChangeExclusiveStatus(device);

                        //
                        // TODO: oh dear...
                        // 
                        var task = new Task(() =>
                        {
                            Thread.Sleep(5);
                            WarnExclusiveModeFailure(device);
                        });
                        task.Start();

                        PrepareDs4DeviceSettingHooks(device);

                        if (appSettings.Settings.DeviceOptions.JoyConSupportSettings.LinkedMode ==
                            JoyConDeviceOptions.LinkMode.Joined)
                            if (device.DeviceType is InputDeviceType.JoyConL or InputDeviceType.JoyConR &&
                                device.PerformStateMerge)
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

                        DS4Controllers[index] = device;
                        device.DeviceSlotNumber = index;

                        Instance.Config.RefreshExtrasButtons(index, GetKnownExtraButtons(device));
                        Instance.Config.LoadControllerConfigs(device);
                        device.LoadStoreSettings();
                        device.CheckControllerNumDeviceSettings(numControllers);

                        slotManager.AddController(device, index);
                        device.Removal += On_DS4Removal;
                        device.Removal += ds4devices.On_Removal;
                        device.SyncChange += On_SyncChange;
                        device.SyncChange += ds4devices.UpdateSerial;
                        device.SerialChange += On_SerialChange;
                        device.ChargingChanged += CheckQuickCharge;

                        touchPad[index] = new Mouse(index, device);

                        var profileLoaded = true;
                        var useAutoProfile = UseTempProfiles[index];

                        profilesService.ControllerArrived(index, device.MacAddress);

                        var profile = profilesService.ActiveProfiles.ElementAt(index);

                        if (profileLoaded || useAutoProfile)
                        {
                            device.LightBarColor = appSettings.Settings.LightbarSettingInfo[index].Ds4WinSettings.Led;

                            if (!profile.DisableVirtualController && device.IsSynced())
                            {
                                if (device.PrimaryDevice)
                                {
                                    PluginOutDev(index, device);
                                }
                                else if (device.JointDeviceSlotNumber != DS4Device.DEFAULT_JOINT_SLOT_NUMBER)
                                {
                                    var otherIdx = device.JointDeviceSlotNumber;
                                    var tempOutDev = outputDevices[otherIdx];
                                    if (tempOutDev != null)
                                    {
                                        var tempConType = ActiveOutDevType[otherIdx];
                                        EstablishOutFeedback(index, tempConType, tempOutDev, device);
                                        outputDevices[index] = tempOutDev;
                                        ActiveOutDevType[index] = tempConType;
                                    }
                                }
                            }
                            else
                            {
                                profile.IsOutputDeviceEnabled = false;
                                ActiveOutDevType[index] = OutputDeviceType.None;
                            }

                            if (device.PrimaryDevice && device.OutputMapGyro)
                            {
                                TouchPadOn(index, device);
                            }
                            else if (device.JointDeviceSlotNumber != DS4Device.DEFAULT_JOINT_SLOT_NUMBER)
                            {
                                var otherIdx = device.JointDeviceSlotNumber;
                                var tempDev = DS4Controllers[otherIdx];
                                if (tempDev != null)
                                {
                                    var mappedIdx = tempDev.PrimaryDevice ? otherIdx : index;
                                    var gyroDev = device.OutputMapGyro ? device :
                                        tempDev.OutputMapGyro ? tempDev : null;
                                    if (gyroDev != null) TouchPadOn(mappedIdx, gyroDev);
                                }
                            }

                            CheckProfileOptions(index, device);
                            SetupInitialHookEvents(index, device);
                        }

                        var tempIdx = index;
                        device.Report += (sender, e) => { On_Report(sender, e, tempIdx); };

                        if (_udpServer != null && index < UdpServer.NUMBER_SLOTS && device.PrimaryDevice)
                            PrepareDevUDPMotion(device, tempIdx);

                        device.StartUpdate();
                        HotplugController?.Invoke(this, device, index);
                        break;
                    }
            }

            inServiceTask = false;

            return true;
        }

        public void CheckProfileOptions(int ind, DS4Device device, bool startUp = false)
        {
            device.ModifyFeatureSetFlag(VidPidFeatureSet.NoOutputData,
                !profilesService.ActiveProfiles.ElementAt(ind).EnableOutputDataToDS4);
            if (!profilesService.ActiveProfiles.ElementAt(ind).EnableOutputDataToDS4)
                LogDebug(
                    "Output data to DS4 disabled. Lightbar and rumble events are not written to DS4 gamepad. If the gamepad is connected over BT then IdleDisconnect option is recommended to let DS4Windows to close the connection after long period of idling.");

            device.SetIdleTimeout(ProfilesService.Instance.ActiveProfiles.ElementAt(ind).IdleDisconnectTimeout);
            device.SetBtPollRate(profilesService.ActiveProfiles.ElementAt(ind).BluetoothPollRate);
            touchPad[ind].ResetTrackAccel(ProfilesService.Instance.ActiveProfiles.ElementAt(ind).TrackballFriction);
            touchPad[ind].ResetToggleGyroModes();

            // Reset current flick stick progress from previous profile
            Mapping.flickMappingData[ind].Reset();

            ProfilesService.Instance.ActiveProfiles.ElementAt(ind).L2OutputSettings.EffectSettings.MaxValue =
                (byte)(Math.Max(ProfilesService.Instance.ActiveProfiles.ElementAt(ind).L2ModInfo.MaxOutput,
                        ProfilesService.Instance.ActiveProfiles.ElementAt(ind).L2ModInfo.MaxZone) /
                    100.0 * 255);
            ProfilesService.Instance.ActiveProfiles.ElementAt(ind).R2OutputSettings.EffectSettings.MaxValue =
                (byte)(Math.Max(ProfilesService.Instance.ActiveProfiles.ElementAt(ind).R2ModInfo.MaxOutput,
                        ProfilesService.Instance.ActiveProfiles.ElementAt(ind).R2ModInfo.MaxZone) /
                    100.0 * 255);

            device.PrepareTriggerEffect(TriggerId.LeftTrigger,
                ProfilesService.Instance.ActiveProfiles.ElementAt(ind).L2OutputSettings.TriggerEffect,
                ProfilesService.Instance.ActiveProfiles.ElementAt(ind).L2OutputSettings.EffectSettings);
            device.PrepareTriggerEffect(TriggerId.RightTrigger,
                ProfilesService.Instance.ActiveProfiles.ElementAt(ind).R2OutputSettings.TriggerEffect,
                ProfilesService.Instance.ActiveProfiles.ElementAt(ind).R2OutputSettings.EffectSettings);

            device.RumbleAutostopTime = profilesService.ActiveProfiles.ElementAt(ind).RumbleAutostopTime;
            device.SetRumble(0, 0);
            device.LightBarColor = appSettings.Settings.LightbarSettingInfo[ind].Ds4WinSettings.Led;

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
            var wheelSmoothInfo = ProfilesService.Instance.ActiveProfiles.ElementAt(ind).WheelSmoothInfo;
            wheelSmoothInfo.SetFilterAttrs(tempFilter);
            wheelSmoothInfo.SetRefreshEvents(tempFilter);

            var flickStickSettings = ProfilesService.Instance.ActiveProfiles.ElementAt(ind).LSOutputSettings
                .OutputSettings.FlickSettings;
            flickStickSettings.RemoveRefreshEvents();
            flickStickSettings.SetRefreshEvents(Mapping.flickMappingData[ind].flickFilter);

            flickStickSettings = ProfilesService.Instance.ActiveProfiles.ElementAt(ind).RSOutputSettings.OutputSettings
                .FlickSettings;
            flickStickSettings.RemoveRefreshEvents();
            flickStickSettings.SetRefreshEvents(Mapping.flickMappingData[ind].flickFilter);

            var tempIdx = ind;
            ProfilesService.Instance.ActiveProfiles.ElementAt(ind).L2OutputSettings.ResetEvents();
            ProfilesService.Instance.ActiveProfiles.ElementAt(ind).L2ModInfo.ResetEvents();
            ProfilesService.Instance.ActiveProfiles.ElementAt(ind).L2OutputSettings.TriggerEffectChanged +=
                (sender, e) =>
                {
                    device.PrepareTriggerEffect(TriggerId.LeftTrigger,
                        ProfilesService.Instance.ActiveProfiles.ElementAt(tempIdx).L2OutputSettings.TriggerEffect,
                        ProfilesService.Instance.ActiveProfiles.ElementAt(tempIdx).L2OutputSettings.EffectSettings);
                };
            ProfilesService.Instance.ActiveProfiles.ElementAt(ind).L2ModInfo.MaxOutputChanged += (sender, e) =>
            {
                var tempInfo = sender as TriggerDeadZoneZInfo;
                ProfilesService.Instance.ActiveProfiles.ElementAt(tempIdx).L2OutputSettings.EffectSettings.MaxValue =
                    (byte)(Math.Max(tempInfo.MaxOutput, tempInfo.MaxZone) / 100.0 * 255.0);

                // Refresh trigger effect
                device.PrepareTriggerEffect(TriggerId.LeftTrigger,
                    ProfilesService.Instance.ActiveProfiles.ElementAt(tempIdx).L2OutputSettings.TriggerEffect,
                    ProfilesService.Instance.ActiveProfiles.ElementAt(tempIdx).L2OutputSettings.EffectSettings);
            };
            ProfilesService.Instance.ActiveProfiles.ElementAt(ind).L2ModInfo.MaxZoneChanged += (sender, e) =>
            {
                var tempInfo = sender as TriggerDeadZoneZInfo;
                ProfilesService.Instance.ActiveProfiles.ElementAt(tempIdx).L2OutputSettings.EffectSettings.MaxValue =
                    (byte)(Math.Max(tempInfo.MaxOutput, tempInfo.MaxZone) / 100.0 * 255.0);

                // Refresh trigger effect
                device.PrepareTriggerEffect(TriggerId.LeftTrigger,
                    ProfilesService.Instance.ActiveProfiles.ElementAt(tempIdx).L2OutputSettings.TriggerEffect,
                    ProfilesService.Instance.ActiveProfiles.ElementAt(tempIdx).L2OutputSettings.EffectSettings);
            };

            ProfilesService.Instance.ActiveProfiles.ElementAt(ind).R2OutputSettings.ResetEvents();
            ProfilesService.Instance.ActiveProfiles.ElementAt(ind).R2OutputSettings.TriggerEffectChanged +=
                (sender, e) =>
                {
                    device.PrepareTriggerEffect(TriggerId.RightTrigger,
                        ProfilesService.Instance.ActiveProfiles.ElementAt(tempIdx).R2OutputSettings.TriggerEffect,
                        ProfilesService.Instance.ActiveProfiles.ElementAt(tempIdx).R2OutputSettings.EffectSettings);
                };
            ProfilesService.Instance.ActiveProfiles.ElementAt(ind).R2ModInfo.MaxOutputChanged += (sender, e) =>
            {
                var tempInfo = sender as TriggerDeadZoneZInfo;
                ProfilesService.Instance.ActiveProfiles.ElementAt(tempIdx).R2OutputSettings.EffectSettings.MaxValue =
                    (byte)(tempInfo.MaxOutput / 100.0 * 255.0);

                // Refresh trigger effect
                device.PrepareTriggerEffect(TriggerId.RightTrigger,
                    ProfilesService.Instance.ActiveProfiles.ElementAt(tempIdx).R2OutputSettings.TriggerEffect,
                    ProfilesService.Instance.ActiveProfiles.ElementAt(tempIdx).R2OutputSettings.EffectSettings);
            };
            ProfilesService.Instance.ActiveProfiles.ElementAt(ind).R2ModInfo.MaxZoneChanged += (sender, e) =>
            {
                var tempInfo = sender as TriggerDeadZoneZInfo;
                ProfilesService.Instance.ActiveProfiles.ElementAt(tempIdx).R2OutputSettings.EffectSettings.MaxValue =
                    (byte)(tempInfo.MaxOutput / 100.0 * 255.0);

                // Refresh trigger effect
                device.PrepareTriggerEffect(TriggerId.RightTrigger,
                    ProfilesService.Instance.ActiveProfiles.ElementAt(tempIdx).R2OutputSettings.TriggerEffect,
                    ProfilesService.Instance.ActiveProfiles.ElementAt(tempIdx).R2OutputSettings.EffectSettings);
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

            var profile = profilesService.ActiveProfiles.ElementAt(ind);

            if (ind < 0) return;

            var synced = device.IsSynced();

            if (!synced)
            {
                if (!profile.IsOutputDeviceEnabled) return;

                ActiveOutDevType[ind] = OutputDeviceType.None;
                UnplugOutDev(ind, device);
            }
            else
            {
                if (profile.DisableVirtualController) return;

                touchPad[ind].ReplaceOneEuroFilterPair();
                touchPad[ind].ReplaceOneEuroFilterPair();

                touchPad[ind].Cursor.ReplaceOneEuroFilterPair();
                touchPad[ind].Cursor.SetupLateOneEuroFilters();
                PluginOutDev(ind, device);
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

            if (ind == -1) return;

            var profile = profilesService.ActiveProfiles.ElementAt(ind);

            var removingStatus = false;
            lock (device.removeLocker)
            {
                if (!device.IsRemoving)
                {
                    removingStatus = true;
                    device.IsRemoving = true;
                }
            }

            if (!removingStatus) return;

            CurrentState[ind].Battery =
                PreviousState[ind].Battery = 0; // Reset for the next connection's initial status change.
            if (profile.IsOutputDeviceEnabled)
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
                device.GetConnectionType() == ConnectionType.Bluetooth && !device.IsCharging())
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
            profile.IsOutputDeviceEnabled = false;
            ActiveOutDevType[ind] = OutputDeviceType.None;
            /* Leave up to Auto Profile system to change the following flags? */
            //Global.UseTempProfiles[ind] = false;
            //Global.TempProfileNames[ind] = string.Empty;
            //Global.TempProfileDistance[ind] = false;

            //Thread.Sleep(XINPUT_UNPLUG_SETTLE_TIME);
        }

        public bool[] lag = new bool[MAX_DS4_CONTROLLER_COUNT]
            { false, false, false, false, false, false, false, false };

        public bool[] inWarnMonitor = new bool[MAX_DS4_CONTROLLER_COUNT]
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
            if (ind == -1) return;

            var profile = profilesService.ActiveProfiles.ElementAt(ind);

            var devError = tempStrings[ind] = device.error;
            if (!string.IsNullOrEmpty(devError)) LogDebug(devError);

            if (inWarnMonitor[ind])
            {
                var flashWhenLateAt = appSettings.Settings.FlashWhenLateAt;
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

            if (ProfilesService.Instance.ActiveProfiles.ElementAt(ind).EnableTouchToggle)
                CheckForTouchToggle(ind, cState, pState);

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

            if (profile.IsOutputDeviceEnabled)
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
                var steeringWheelMappedAxis =
                    ProfilesService.Instance.ActiveProfiles.ElementAt(ind).SASteeringWheelEmulationAxis;
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
            DS4LightBarV3.UpdateLightBar(device, ind);

            if (device.PerformStateMerge) device.PreserveMergedStateData();
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
                    DS4LightBarV3.forcedColor[ind] = color;
                    DS4LightBarV3.forcedFlash[ind] = 2;
                    DS4LightBarV3.forcelight[ind] = true;
                }
            }
            else
            {
                lag[ind] = false;
                LogDebug(Resources.LatencyNotOverTen.Replace("*number*", (ind + 1).ToString()));
                DS4LightBarV3.forcelight[ind] = false;
                DS4LightBarV3.forcedFlash[ind] = 0;
                device.LightBarColor = appSettings.Settings.LightbarSettingInfo[ind].Ds4WinSettings.Led;
            }
        }

        public DS4ControlItem GetActiveInputControl(int ind)
        {
            var cState = CurrentState[ind];
            var eState = ExposedState[ind];
            var tp = touchPad[ind];
            var result = DS4ControlItem.None;

            if (DS4Controllers[ind] != null)
            {
                if (Mapping.getBoolButtonMapping(cState.Cross))
                    result = DS4ControlItem.Cross;
                else if (Mapping.getBoolButtonMapping(cState.Circle))
                    result = DS4ControlItem.Circle;
                else if (Mapping.getBoolButtonMapping(cState.Triangle))
                    result = DS4ControlItem.Triangle;
                else if (Mapping.getBoolButtonMapping(cState.Square))
                    result = DS4ControlItem.Square;
                else if (Mapping.getBoolButtonMapping(cState.L1))
                    result = DS4ControlItem.L1;
                else if (Mapping.getBoolTriggerMapping(cState.L2))
                    result = DS4ControlItem.L2;
                else if (Mapping.getBoolButtonMapping(cState.L3))
                    result = DS4ControlItem.L3;
                else if (Mapping.getBoolButtonMapping(cState.R1))
                    result = DS4ControlItem.R1;
                else if (Mapping.getBoolTriggerMapping(cState.R2))
                    result = DS4ControlItem.R2;
                else if (Mapping.getBoolButtonMapping(cState.R3))
                    result = DS4ControlItem.R3;
                else if (Mapping.getBoolButtonMapping(cState.DpadUp))
                    result = DS4ControlItem.DpadUp;
                else if (Mapping.getBoolButtonMapping(cState.DpadDown))
                    result = DS4ControlItem.DpadDown;
                else if (Mapping.getBoolButtonMapping(cState.DpadLeft))
                    result = DS4ControlItem.DpadLeft;
                else if (Mapping.getBoolButtonMapping(cState.DpadRight))
                    result = DS4ControlItem.DpadRight;
                else if (Mapping.getBoolButtonMapping(cState.Share))
                    result = DS4ControlItem.Share;
                else if (Mapping.getBoolButtonMapping(cState.Options))
                    result = DS4ControlItem.Options;
                else if (Mapping.getBoolButtonMapping(cState.PS))
                    result = DS4ControlItem.PS;
                else if (Mapping.getBoolAxisDirMapping(cState.LX, true))
                    result = DS4ControlItem.LXPos;
                else if (Mapping.getBoolAxisDirMapping(cState.LX, false))
                    result = DS4ControlItem.LXNeg;
                else if (Mapping.getBoolAxisDirMapping(cState.LY, true))
                    result = DS4ControlItem.LYPos;
                else if (Mapping.getBoolAxisDirMapping(cState.LY, false))
                    result = DS4ControlItem.LYNeg;
                else if (Mapping.getBoolAxisDirMapping(cState.RX, true))
                    result = DS4ControlItem.RXPos;
                else if (Mapping.getBoolAxisDirMapping(cState.RX, false))
                    result = DS4ControlItem.RXNeg;
                else if (Mapping.getBoolAxisDirMapping(cState.RY, true))
                    result = DS4ControlItem.RYPos;
                else if (Mapping.getBoolAxisDirMapping(cState.RY, false))
                    result = DS4ControlItem.RYNeg;
                else if (Mapping.getBoolTouchMapping(tp.leftDown))
                    result = DS4ControlItem.TouchLeft;
                else if (Mapping.getBoolTouchMapping(tp.rightDown))
                    result = DS4ControlItem.TouchRight;
                else if (Mapping.getBoolTouchMapping(tp.multiDown))
                    result = DS4ControlItem.TouchMulti;
                else if (Mapping.getBoolTouchMapping(tp.upperDown))
                    result = DS4ControlItem.TouchUpper;
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
            OnDebug(this, new LogEntryEventArgs(data, isWarning));
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
            var boost = profilesService.ActiveProfiles.ElementAt(deviceNum).RumbleAutostopTime;
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