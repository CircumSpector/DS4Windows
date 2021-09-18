using System;
using System.Collections.Generic;
using System.Linq;

using System.IO;
using System.Reflection;
using System.Xml;
using System.Drawing;

using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Globalization;
using System.Diagnostics;
using Sensorit.Base;
using DS4Windows.DS4Control;
using System.Windows.Input;
using System.Runtime.InteropServices;

namespace DS4Windows
{
    [Flags]
    public enum DS4KeyType : byte { None = 0, ScanCode = 1, Toggle = 2, Unbound = 4, Macro = 8, HoldMacro = 16, RepeatMacro = 32 }; // Increment by exponents of 2*, starting at 2^0
    public enum Ds3PadId : byte { None = 0xFF, One = 0x00, Two = 0x01, Three = 0x02, Four = 0x03, All = 0x04 };
    public enum DS4Controls : byte { None, LXNeg, LXPos, LYNeg, LYPos, RXNeg, RXPos, RYNeg, RYPos, L1, L2, L3, R1, R2, R3, Square, Triangle, Circle, Cross, DpadUp, DpadRight, DpadDown, DpadLeft, PS, TouchLeft, TouchUpper, TouchMulti, TouchRight, Share, Options, Mute, GyroXPos, GyroXNeg, GyroZPos, GyroZNeg, SwipeLeft, SwipeRight, SwipeUp, SwipeDown, L2FullPull, R2FullPull, GyroSwipeLeft, GyroSwipeRight, GyroSwipeUp, GyroSwipeDown, Capture, SideL, SideR, LSOuter, RSOuter };
    public enum X360Controls : byte { None, LXNeg, LXPos, LYNeg, LYPos, RXNeg, RXPos, RYNeg, RYPos, LB, LT, LS, RB, RT, RS, X, Y, B, A, DpadUp, DpadRight, DpadDown, DpadLeft, Guide, Back, Start, TouchpadClick, LeftMouse, RightMouse, MiddleMouse, FourthMouse, FifthMouse, WUP, WDOWN, MouseUp, MouseDown, MouseLeft, MouseRight, Unbound };

    public enum SASteeringWheelEmulationAxisType: byte { None = 0, LX, LY, RX, RY, L2R2, VJoy1X, VJoy1Y, VJoy1Z, VJoy2X, VJoy2Y, VJoy2Z };
    public enum OutContType : uint { None = 0, X360, DS4 }

    public enum GyroOutMode : uint
    {
        None,
        Controls,
        Mouse,
        MouseJoystick,
        DirectionalSwipe,
        Passthru,
    }

    public enum TouchpadOutMode : uint
    {
        None,
        Mouse,
        Controls,
        AbsoluteMouse,
        Passthru,
    }

    public enum TrayIconChoice : uint
    {
        Default,
        Colored,
        White,
        Black,
    }

    public enum AppThemeChoice : uint
    {
        Default,
        Dark,
    }

    public class ControlActionData
    {
        // Store base mapping value. Uses Windows virtual key values as the base
        // https://docs.microsoft.com/en-us/windows/win32/inputdev/virtual-key-codes
        public int actionKey;

        // Alias to real value for current output KB+M event system.
        // Allows skipping a translation call every frame
        public uint actionAlias = 0;
        public X360Controls actionBtn;
        public int[] actionMacro = new int[1];
    }

    public class DS4ControlSettings
    {
        public const int MAX_MACRO_VALUE = 286;

        public DS4Controls control;
        public string extras = null;
        public DS4KeyType keyType = DS4KeyType.None;
        public enum ActionType : byte { Default, Key, Button, Macro };
        public ActionType actionType = ActionType.Default;
        public ControlActionData action = new ControlActionData();

        public ActionType shiftActionType = ActionType.Default;
        public ControlActionData shiftAction = new ControlActionData();
        public int shiftTrigger = 0;
        public string shiftExtras = null;
        public DS4KeyType shiftKeyType = DS4KeyType.None;

        public bool IsDefault { get => actionType == ActionType.Default; }
        public bool IsShiftDefault { get => shiftActionType == ActionType.Default; }

        public DS4ControlSettings(DS4Controls ctrl)
        {
            control = ctrl;
        }

        public void Reset()
        {
            extras = null;
            keyType = DS4KeyType.None;
            actionType = ActionType.Default;
            action = new ControlActionData();
            action.actionAlias = 0;
            //actionAlias = 0;

            shiftActionType = ActionType.Default;
            shiftAction = new ControlActionData();
            shiftAction.actionAlias = 0;
            //shiftActionAlias = 0;
            shiftTrigger = 0;
            shiftExtras = null;
            shiftKeyType = DS4KeyType.None;
        }

        public bool IsExtrasEmpty(string extraStr)
        {
            return string.IsNullOrEmpty(extraStr) || extraStr == "0,0,0,0,0,0,0,0,0";
        }

        internal void UpdateSettings(bool shift, object act, string exts, DS4KeyType kt, int trigger = 0)
        {
            if (!shift)
            {
                if (act is int || act is ushort)
                {
                    actionType = ActionType.Key;
                    action.actionKey = Convert.ToInt32(act);
                }
                else if (act is string || act is X360Controls)
                {
                    actionType = ActionType.Button;
                    if (act is X360Controls)
                    {
                        action.actionBtn = (X360Controls)act;
                    }
                    else
                    {
                        Enum.TryParse(act.ToString(), out action.actionBtn);
                    }
                }
                else if (act is int[])
                {
                    actionType = ActionType.Macro;
                    action.actionMacro = (int[])act;
                }
                else
                {
                    actionType = ActionType.Default;
                    action.actionKey = 0;
                }

                extras = exts;
                keyType = kt;
            }
            else
            {
                if (act is int || act is ushort)
                {
                    shiftActionType = ActionType.Key;
                    shiftAction.actionKey = Convert.ToInt32(act);
                }
                else if (act is string || act is X360Controls)
                {
                    shiftActionType = ActionType.Button;
                    if (act is X360Controls)
                    {
                        shiftAction.actionBtn = (X360Controls)act;
                    }
                    else
                    {
                        Enum.TryParse(act.ToString(), out shiftAction.actionBtn);
                    }
                }
                else if (act is int[])
                {
                    shiftActionType = ActionType.Macro;
                    shiftAction.actionMacro = (int[])act;
                }
                else
                {
                    shiftActionType = ActionType.Default;
                    shiftAction.actionKey = 0;
                }

                shiftExtras = exts;
                shiftKeyType = kt;
                shiftTrigger = trigger;
            }
        }
    }

    public class ControlSettingsGroup
    {
        public List<DS4ControlSettings> LS = new List<DS4ControlSettings>();
        public List<DS4ControlSettings> RS = new List<DS4ControlSettings>();
        public DS4ControlSettings L2;
        public DS4ControlSettings L2FullPull;
        public DS4ControlSettings R2;
        public DS4ControlSettings R2FullPull;

        public DS4ControlSettings GyroSwipeLeft;
        public DS4ControlSettings GyroSwipeRight;
        public DS4ControlSettings GyroSwipeUp;
        public DS4ControlSettings GyroSwipeDown;

        public List<DS4ControlSettings> ControlButtons =
            new List<DS4ControlSettings>();

        public List<DS4ControlSettings> ExtraDeviceButtons =
            new List<DS4ControlSettings>();

        private List<DS4ControlSettings> settingsList;

        public ControlSettingsGroup(List<DS4ControlSettings> settingsList)
        {
            LS.Add(settingsList[(int)DS4Controls.LSOuter - 1]);
            for (int i = (int)DS4Controls.LXNeg; i <= (int)DS4Controls.LYPos; i++)
            {
                LS.Add(settingsList[i-1]);
            }

            LS.Add(settingsList[(int)DS4Controls.RSOuter - 1]);
            for (int i = (int)DS4Controls.RXNeg; i <= (int)DS4Controls.RYPos; i++)
            {
                RS.Add(settingsList[i-1]);
            }

            L2 = settingsList[(int)DS4Controls.L2-1];
            R2 = settingsList[(int)DS4Controls.R2-1];

            L2FullPull = settingsList[(int)DS4Controls.L2FullPull - 1];
            R2FullPull = settingsList[(int)DS4Controls.R2FullPull - 1];

            GyroSwipeLeft = settingsList[(int)DS4Controls.GyroSwipeLeft - 1];
            GyroSwipeRight = settingsList[(int)DS4Controls.GyroSwipeRight - 1];
            GyroSwipeUp = settingsList[(int)DS4Controls.GyroSwipeUp - 1];
            GyroSwipeDown = settingsList[(int)DS4Controls.GyroSwipeDown - 1];

            ControlButtons.Add(settingsList[(int)DS4Controls.L1-1]);
            ControlButtons.Add(settingsList[(int)DS4Controls.L3-1]);
            ControlButtons.Add(settingsList[(int)DS4Controls.R1-1]);
            ControlButtons.Add(settingsList[(int)DS4Controls.R3-1]);

            for (int i = (int)DS4Controls.Square; i <= (int)DS4Controls.SwipeDown; i++)
            {
                ControlButtons.Add(settingsList[i-1]);
            }

            this.settingsList = settingsList;
        }

        public void EstablishExtraButtons(List<DS4Controls> buttonList)
        {
            foreach(DS4Controls control in buttonList)
            {
                ExtraDeviceButtons.Add(settingsList[(int)control - 1]);
            }
        }

        public void ResetExtraButtons()
        {
            ExtraDeviceButtons.Clear();
        }
    }

    public class DebugEventArgs : EventArgs
    {
        protected DateTime m_Time = DateTime.Now;
        protected string m_Data = string.Empty;
        protected bool warning = false;
        protected bool temporary = false;
        public DebugEventArgs(string Data, bool warn, bool temporary = false)
        {
            m_Data = Data;
            warning = warn;
            this.temporary = temporary;
        }

        public DateTime Time => m_Time;
        public string Data => m_Data;
        public bool Warning => warning;
        public bool Temporary => temporary;
    }

    public class MappingDoneEventArgs : EventArgs
    {
        protected int deviceNum = -1;

        public MappingDoneEventArgs(int DeviceID)
        {
            deviceNum = DeviceID;
        }

        public int DeviceID => deviceNum;
    }

    public class ReportEventArgs : EventArgs
    {
        protected Ds3PadId m_Pad = Ds3PadId.None;
        protected byte[] m_Report = new byte[64];

        public ReportEventArgs()
        {
        }

        public ReportEventArgs(Ds3PadId Pad)
        {
            m_Pad = Pad;
        }

        public Ds3PadId Pad
        {
            get { return m_Pad; }
            set { m_Pad = value; }
        }

        public Byte[] Report
        {
            get { return m_Report; }
        }
    }

    public class BatteryReportArgs : EventArgs
    {
        private int index;
        private int level;
        private bool charging;

        public BatteryReportArgs(int index, int level, bool charging)
        {
            this.index = index;
            this.level = level;
            this.charging = charging;
        }

        public int getIndex()
        {
            return index;
        }

        public int getLevel()
        {
            return level;
        }

        public bool isCharging()
        {
            return charging;
        }
    }

    public class ControllerRemovedArgs : EventArgs
    {
        private int index;

        public ControllerRemovedArgs(int index)
        {
            this.index = index;
        }

        public int getIndex()
        {
            return this.index;
        }
    }

    public class DeviceStatusChangeEventArgs : EventArgs
    {
        private int index;

        public DeviceStatusChangeEventArgs(int index)
        {
            this.index = index;
        }

        public int getIndex()
        {
            return index;
        }
    }

    public class SerialChangeArgs : EventArgs
    {
        private int index;
        private string serial;

        public SerialChangeArgs(int index, string serial)
        {
            this.index = index;
            this.serial = serial;
        }

        public int getIndex()
        {
            return index;
        }

        public string getSerial()
        {
            return serial;
        }
    }

    public class OneEuroFilterPair
    {
        public const double DEFAULT_WHEEL_CUTOFF = 0.1;
        public const double DEFAULT_WHEEL_BETA = 0.1;

        public OneEuroFilter axis1Filter = new OneEuroFilter(minCutoff: DEFAULT_WHEEL_CUTOFF, beta: DEFAULT_WHEEL_BETA);
        public OneEuroFilter axis2Filter = new OneEuroFilter(minCutoff: DEFAULT_WHEEL_CUTOFF, beta: DEFAULT_WHEEL_BETA);
    }

    public class OneEuroFilter3D
    {
        public const double DEFAULT_WHEEL_CUTOFF = 0.4;
        public const double DEFAULT_WHEEL_BETA = 0.2;

        public OneEuroFilter axis1Filter = new OneEuroFilter(minCutoff: DEFAULT_WHEEL_CUTOFF, beta: DEFAULT_WHEEL_BETA);
        public OneEuroFilter axis2Filter = new OneEuroFilter(minCutoff: DEFAULT_WHEEL_CUTOFF, beta: DEFAULT_WHEEL_BETA);
        public OneEuroFilter axis3Filter = new OneEuroFilter(minCutoff: DEFAULT_WHEEL_CUTOFF, beta: DEFAULT_WHEEL_BETA);

        public void SetFilterAttrs(double minCutoff, double beta)
        {
            axis1Filter.MinCutoff = axis2Filter.MinCutoff = axis3Filter.MinCutoff = minCutoff;
            axis1Filter.Beta = axis2Filter.Beta = axis3Filter.Beta = beta;
        }
    }
    
    

    public class SpecialAction
    {
        public enum ActionTypeId { None, Key, Program, Profile, Macro, DisconnectBT, BatteryCheck, MultiAction, XboxGameDVR, SASteeringWheelEmulationCalibrate }

        public string name;
        public List<DS4Controls> trigger = new List<DS4Controls>();
        public string type;
        public ActionTypeId typeID;
        public string controls;
        public List<int> macro = new List<int>();
        public string details;
        public List<DS4Controls> uTrigger = new List<DS4Controls>();
        public string ucontrols;
        public double delayTime = 0;
        public string extra;
        public bool pressRelease = false;
        public DS4KeyType keyType;
        public bool tappedOnce = false;
        public bool firstTouch = false;
        public bool secondtouchbegin = false;
        public DateTime pastTime;
        public DateTime firstTap;
        public DateTime TimeofEnd;
        public bool automaticUntrigger = false;
        public string prevProfileName;  // Name of the previous profile where automaticUntrigger would jump back to (could be regular or temporary profile. Empty name is the same as regular profile)
        public bool synchronized = false; // If the same trigger has both "key down" and "key released" macros then run those synchronized if this attribute is TRUE (ie. key down macro fully completed before running the key release macro)
        public bool keepKeyState = false; // By default special action type "Macro" resets all keys used in the macro back to default "key up" state after completing the macro even when the macro itself doesn't do it explicitly. If this is TRUE then key states are NOT reset automatically (macro is expected to do it or to leave a key to down state on purpose)

        public SpecialAction(string name, string controls, string type, string details, double delay = 0, string extras = "")
        {
            this.name = name;
            this.type = type;
            this.typeID = ActionTypeId.None;
            this.controls = controls;
            delayTime = delay;
            string[] ctrls = controls.Split('/');
            foreach (string s in ctrls)
                trigger.Add(getDS4ControlsByName(s));

            if (type == "Key")
            {
                typeID = ActionTypeId.Key;
                this.details = details.Split(' ')[0];
                if (!string.IsNullOrEmpty(extras))
                {
                    string[] exts = extras.Split('\n');
                    pressRelease = exts[0] == "Release";
                    this.ucontrols = exts[1];
                    string[] uctrls = exts[1].Split('/');
                    foreach (string s in uctrls)
                        uTrigger.Add(getDS4ControlsByName(s));
                }
                if (details.Contains("Scan Code"))
                    keyType |= DS4KeyType.ScanCode;
            }
            else if (type == "Program")
            {
                typeID = ActionTypeId.Program;
                this.details = details;
                if (extras != string.Empty)
                    extra = extras;
            }
            else if (type == "Profile")
            {
                typeID = ActionTypeId.Profile;
                this.details = details;
                if (extras != string.Empty)
                {
                    extra = extras;
                }
            }
            else if (type == "Macro")
            {
                typeID = ActionTypeId.Macro;
                string[] macs = details.Split('/');
                foreach (string s in macs)
                {
                    int v;
                    if (int.TryParse(s, out v))
                        macro.Add(v);
                }
                if (extras.Contains("Scan Code"))
                    keyType |= DS4KeyType.ScanCode;
                if (extras.Contains("RunOnRelease"))
                    pressRelease = true;
                if (extras.Contains("Sync"))
                    synchronized = true;
                if (extras.Contains("KeepKeyState"))
                    keepKeyState = true;
                if (extras.Contains("Repeat"))
                    keyType |= DS4KeyType.RepeatMacro;
            }
            else if (type == "DisconnectBT")
            {
                typeID = ActionTypeId.DisconnectBT;
            }
            else if (type == "BatteryCheck")
            {
                typeID = ActionTypeId.BatteryCheck;
                string[] dets = details.Split('|');
                this.details = string.Join(",", dets);
            }
            else if (type == "MultiAction")
            {
                typeID = ActionTypeId.MultiAction;
                this.details = details;
            }
            else if (type == "XboxGameDVR")
            {
                this.typeID = ActionTypeId.XboxGameDVR;
                string[] dets = details.Split(',');
                List<string> macros = new List<string>();
                //string dets = "";
                int typeT = 0;
                for (int i = 0; i < 3; i++)
                {
                    if (int.TryParse(dets[i], out typeT))
                    {
                        switch (typeT)
                        {
                            case 0: macros.Add("91/71/71/91"); break;
                            case 1: macros.Add("91/164/82/82/164/91"); break;
                            case 2: macros.Add("91/164/44/44/164/91"); break;
                            case 3: macros.Add(dets[3] + "/" + dets[3]); break;
                            case 4: macros.Add("91/164/71/71/164/91"); break;
                        }
                    }
                }
                this.type = "MultiAction";
                type = "MultiAction";
                this.details = string.Join(",", macros);
            }
            else if (type == "SASteeringWheelEmulationCalibrate")
            {
                typeID = ActionTypeId.SASteeringWheelEmulationCalibrate;
            }
            else
                this.details = details;

            if (type != "Key" && !string.IsNullOrEmpty(extras))
            {
                this.ucontrols = extras;
                string[] uctrls = extras.Split('/');
                foreach (string s in uctrls)
                {
                    if (s == "AutomaticUntrigger") this.automaticUntrigger = true;
                    else uTrigger.Add(getDS4ControlsByName(s));
                }
            }
        }

        private DS4Controls getDS4ControlsByName(string key)
        {
            switch (key)
            {
                case "Share": return DS4Controls.Share;
                case "L3": return DS4Controls.L3;
                case "R3": return DS4Controls.R3;
                case "Options": return DS4Controls.Options;
                case "Up": return DS4Controls.DpadUp;
                case "Right": return DS4Controls.DpadRight;
                case "Down": return DS4Controls.DpadDown;
                case "Left": return DS4Controls.DpadLeft;

                case "L1": return DS4Controls.L1;
                case "R1": return DS4Controls.R1;
                case "Triangle": return DS4Controls.Triangle;
                case "Circle": return DS4Controls.Circle;
                case "Cross": return DS4Controls.Cross;
                case "Square": return DS4Controls.Square;

                case "PS": return DS4Controls.PS;
                case "Mute": return DS4Controls.Mute;
                case "Capture": return DS4Controls.Capture;
                case "SideL": return DS4Controls.SideL;
                case "SideR": return DS4Controls.SideL;
                case "Left Stick Left": return DS4Controls.LXNeg;
                case "Left Stick Up": return DS4Controls.LYNeg;
                case "Right Stick Left": return DS4Controls.RXNeg;
                case "Right Stick Up": return DS4Controls.RYNeg;

                case "Left Stick Right": return DS4Controls.LXPos;
                case "Left Stick Down": return DS4Controls.LYPos;
                case "Right Stick Right": return DS4Controls.RXPos;
                case "Right Stick Down": return DS4Controls.RYPos;
                case "L2": return DS4Controls.L2;
                case "L2 Full Pull": return DS4Controls.L2FullPull;
                case "R2": return DS4Controls.R2;
                case "R2 Full Pull": return DS4Controls.R2FullPull;

                case "Left Touch": return DS4Controls.TouchLeft;
                case "Multitouch": return DS4Controls.TouchMulti;
                case "Upper Touch": return DS4Controls.TouchUpper;
                case "Right Touch": return DS4Controls.TouchRight;

                case "Swipe Up": return DS4Controls.SwipeUp;
                case "Swipe Down": return DS4Controls.SwipeDown;
                case "Swipe Left": return DS4Controls.SwipeLeft;
                case "Swipe Right": return DS4Controls.SwipeRight;

                case "Tilt Up": return DS4Controls.GyroZNeg;
                case "Tilt Down": return DS4Controls.GyroZPos;
                case "Tilt Left": return DS4Controls.GyroXPos;
                case "Tilt Right": return DS4Controls.GyroXNeg;
            }

            return 0;
        }
    }
}
