using System;
using System.Collections.Generic;
using DS4Windows.Shared.Common.Types;

namespace DS4Windows
{
    public class SpecialActionV3
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

        //
        // TODO: here to satisfy serializer until I found out how to improve this class
        // 
        public SpecialActionV3()
        {

        }

        public SpecialActionV3(
            string name, 
            string controls, 
            string type, 
            string details, 
            double delayTime = 0,
            string extras = ""
            )
        {
            Name = name;
            Type = type;
            TypeId = ActionTypeId.None;
            Controls = controls;
            DelayTime = delayTime;
            Extras = extras;

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
                    Extras = extras;
            }
            else if (type == "Profile")
            {
                TypeId = ActionTypeId.Profile;
                Details = details;
                if (extras != string.Empty) Extras = extras;
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

        public string Extras { get; set; }

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
            return key switch
            {
                "Share" => DS4Controls.Share,
                "L3" => DS4Controls.L3,
                "R3" => DS4Controls.R3,
                "Options" => DS4Controls.Options,
                "Up" => DS4Controls.DpadUp,
                "Right" => DS4Controls.DpadRight,
                "Down" => DS4Controls.DpadDown,
                "Left" => DS4Controls.DpadLeft,
                "L1" => DS4Controls.L1,
                "R1" => DS4Controls.R1,
                "Triangle" => DS4Controls.Triangle,
                "Circle" => DS4Controls.Circle,
                "Cross" => DS4Controls.Cross,
                "Square" => DS4Controls.Square,
                "PS" => DS4Controls.PS,
                "Mute" => DS4Controls.Mute,
                "Capture" => DS4Controls.Capture,
                "SideL" => DS4Controls.SideL,
                "SideR" => DS4Controls.SideL,
                "Left Stick Left" => DS4Controls.LXNeg,
                "Left Stick Up" => DS4Controls.LYNeg,
                "Right Stick Left" => DS4Controls.RXNeg,
                "Right Stick Up" => DS4Controls.RYNeg,
                "Left Stick Right" => DS4Controls.LXPos,
                "Left Stick Down" => DS4Controls.LYPos,
                "Right Stick Right" => DS4Controls.RXPos,
                "Right Stick Down" => DS4Controls.RYPos,
                "L2" => DS4Controls.L2,
                "L2 Full Pull" => DS4Controls.L2FullPull,
                "R2" => DS4Controls.R2,
                "R2 Full Pull" => DS4Controls.R2FullPull,
                "Left Touch" => DS4Controls.TouchLeft,
                "Multitouch" => DS4Controls.TouchMulti,
                "Upper Touch" => DS4Controls.TouchUpper,
                "Right Touch" => DS4Controls.TouchRight,
                "Swipe Up" => DS4Controls.SwipeUp,
                "Swipe Down" => DS4Controls.SwipeDown,
                "Swipe Left" => DS4Controls.SwipeLeft,
                "Swipe Right" => DS4Controls.SwipeRight,
                "Tilt Up" => DS4Controls.GyroZNeg,
                "Tilt Down" => DS4Controls.GyroZPos,
                "Tilt Left" => DS4Controls.GyroXPos,
                "Tilt Right" => DS4Controls.GyroXNeg,
                _ => 0,
            };
        }
    }
}