using System;
using System.Collections.Generic;

namespace DS4Windows
{
    public class SpecialAction
    {
        public enum ActionTypeId
        {
            None,
            Key,
            Program,
            Profile,
            Macro,
            DisconnectBT,
            BatteryCheck,
            MultiAction,
            XboxGameDVR,
            SASteeringWheelEmulationCalibrate
        }

        public SpecialAction(string name, string controls, string type, string details, double delay = 0,
            string extras = "")
        {
            Name = name;
            Type = type;
            TypeId = ActionTypeId.None;
            Controls = controls;
            DelayTime = delay;
            var ctrls = controls.Split('/');
            foreach (var s in ctrls)
                Trigger.Add(GetDs4ControlsByName(s));

            if (type == "Key")
            {
                TypeId = ActionTypeId.Key;
                Details = details.Split(' ')[0];
                if (!string.IsNullOrEmpty(extras))
                {
                    var exts = extras.Split('\n');
                    PressRelease = exts[0] == "Release";
                    UControls = exts[1];
                    var uctrls = exts[1].Split('/');
                    foreach (var s in uctrls)
                        UTrigger.Add(GetDs4ControlsByName(s));
                }

                if (details.Contains("Scan Code"))
                    KeyType |= DS4KeyType.ScanCode;
            }
            else if (type == "Program")
            {
                TypeId = ActionTypeId.Program;
                Details = details;
                if (extras != string.Empty)
                    Extra = extras;
            }
            else if (type == "Profile")
            {
                TypeId = ActionTypeId.Profile;
                Details = details;
                if (extras != string.Empty) Extra = extras;
            }
            else if (type == "Macro")
            {
                TypeId = ActionTypeId.Macro;
                var macs = details.Split('/');
                foreach (var s in macs)
                {
                    int v;
                    if (int.TryParse(s, out v))
                        Macro.Add(v);
                }

                if (extras.Contains("Scan Code"))
                    KeyType |= DS4KeyType.ScanCode;
                if (extras.Contains("RunOnRelease"))
                    PressRelease = true;
                if (extras.Contains("Sync"))
                    Synchronized = true;
                if (extras.Contains("KeepKeyState"))
                    KeepKeyState = true;
                if (extras.Contains("Repeat"))
                    KeyType |= DS4KeyType.RepeatMacro;
            }
            else if (type == "DisconnectBT")
            {
                TypeId = ActionTypeId.DisconnectBT;
            }
            else if (type == "BatteryCheck")
            {
                TypeId = ActionTypeId.BatteryCheck;
                var dets = details.Split('|');
                Details = string.Join(",", dets);
            }
            else if (type == "MultiAction")
            {
                TypeId = ActionTypeId.MultiAction;
                Details = details;
            }
            else if (type == "XboxGameDVR")
            {
                TypeId = ActionTypeId.XboxGameDVR;
                var dets = details.Split(',');
                var macros = new List<string>();
                //string dets = "";
                var typeT = 0;
                for (var i = 0; i < 3; i++)
                    if (int.TryParse(dets[i], out typeT))
                        switch (typeT)
                        {
                            case 0:
                                macros.Add("91/71/71/91");
                                break;
                            case 1:
                                macros.Add("91/164/82/82/164/91");
                                break;
                            case 2:
                                macros.Add("91/164/44/44/164/91");
                                break;
                            case 3:
                                macros.Add(dets[3] + "/" + dets[3]);
                                break;
                            case 4:
                                macros.Add("91/164/71/71/164/91");
                                break;
                        }

                Type = "MultiAction";
                type = "MultiAction";
                Details = string.Join(",", macros);
            }
            else if (type == "SASteeringWheelEmulationCalibrate")
            {
                TypeId = ActionTypeId.SASteeringWheelEmulationCalibrate;
            }
            else
            {
                Details = details;
            }

            if (type != "Key" && !string.IsNullOrEmpty(extras))
            {
                UControls = extras;
                var uctrls = extras.Split('/');
                foreach (var s in uctrls)
                    if (s == "AutomaticUntrigger") AutomaticUnTrigger = true;
                    else UTrigger.Add(GetDs4ControlsByName(s));
            }
        }

        public string Name { get; set; }

        public List<DS4Controls> Trigger { get; } = new();

        public string Type { get; set; }

        public ActionTypeId TypeId { get; set; }

        public string Controls { get; set; }

        public List<int> Macro { get; } = new();

        public string Details { get; set; }

        public List<DS4Controls> UTrigger { get; } = new();

        public string UControls { get; set; }

        public double DelayTime { get; set; }

        public string Extra { get; set; }

        public bool PressRelease { get; set; }

        public DS4KeyType KeyType { get; set; }

        public bool TappedOnce { get; set; } = false;

        public bool FirstTouch { get; set; } = false;

        public bool SecondTouchBegin { get; set; } = false;

        public DateTime PastTime { get; set; }

        public DateTime FirstTap { get; set; }

        public DateTime TimeOfEnd { get; set; }

        public bool AutomaticUnTrigger { get; set; }

        /// <summary>
        ///     Name of the previous profile where automaticUntrigger would jump back to (could be regular or temporary profile.
        ///     Empty name is the same as regular profile)
        /// </summary>
        public string PreviousProfileName { get; set; }

        /// <summary>
        ///     If the same trigger has both "key down" and "key released" macros then run those synchronized if this attribute is
        ///     TRUE (ie. key down macro fully completed before running the key release macro)
        /// </summary>
        public bool Synchronized { get; set; }

        /// <summary>
        ///     By default special action type "Macro" resets all keys used in the macro back to default "key up" state after
        ///     completing the macro even when the macro itself doesn't do it explicitly. If this is TRUE then key states are NOT
        ///     reset automatically (macro is expected to do it or to leave a key to down state on purpose)
        /// </summary>
        public bool KeepKeyState { get; set; }

        private DS4Controls GetDs4ControlsByName(string key)
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