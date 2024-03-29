﻿using System;
using System.Collections.Generic;
using DS4Windows.Shared.Common.Legacy;
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

        public List<DS4ControlItem> Trigger { get; } = new();

        public string Type { get; set; }

        public ActionTypeId TypeId { get; set; }

        public string Controls { get; set; }

        public List<int> Macro { get; } = new();

        public string Details { get; set; }

        public List<DS4ControlItem> UTrigger { get; } = new();

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

        private DS4ControlItem GetDs4ControlsByName(string key)
        {
            return key switch
            {
                "Share" => DS4ControlItem.Share,
                "L3" => DS4ControlItem.L3,
                "R3" => DS4ControlItem.R3,
                "Options" => DS4ControlItem.Options,
                "Up" => DS4ControlItem.DpadUp,
                "Right" => DS4ControlItem.DpadRight,
                "Down" => DS4ControlItem.DpadDown,
                "Left" => DS4ControlItem.DpadLeft,
                "L1" => DS4ControlItem.L1,
                "R1" => DS4ControlItem.R1,
                "Triangle" => DS4ControlItem.Triangle,
                "Circle" => DS4ControlItem.Circle,
                "Cross" => DS4ControlItem.Cross,
                "Square" => DS4ControlItem.Square,
                "PS" => DS4ControlItem.PS,
                "Mute" => DS4ControlItem.Mute,
                "Capture" => DS4ControlItem.Capture,
                "SideL" => DS4ControlItem.SideL,
                "SideR" => DS4ControlItem.SideL,
                "Left Stick Left" => DS4ControlItem.LXNeg,
                "Left Stick Up" => DS4ControlItem.LYNeg,
                "Right Stick Left" => DS4ControlItem.RXNeg,
                "Right Stick Up" => DS4ControlItem.RYNeg,
                "Left Stick Right" => DS4ControlItem.LXPos,
                "Left Stick Down" => DS4ControlItem.LYPos,
                "Right Stick Right" => DS4ControlItem.RXPos,
                "Right Stick Down" => DS4ControlItem.RYPos,
                "L2" => DS4ControlItem.L2,
                "L2 Full Pull" => DS4ControlItem.L2FullPull,
                "R2" => DS4ControlItem.R2,
                "R2 Full Pull" => DS4ControlItem.R2FullPull,
                "Left Touch" => DS4ControlItem.TouchLeft,
                "Multitouch" => DS4ControlItem.TouchMulti,
                "Upper Touch" => DS4ControlItem.TouchUpper,
                "Right Touch" => DS4ControlItem.TouchRight,
                "Swipe Up" => DS4ControlItem.SwipeUp,
                "Swipe Down" => DS4ControlItem.SwipeDown,
                "Swipe Left" => DS4ControlItem.SwipeLeft,
                "Swipe Right" => DS4ControlItem.SwipeRight,
                "Tilt Up" => DS4ControlItem.GyroZNeg,
                "Tilt Down" => DS4ControlItem.GyroZPos,
                "Tilt Left" => DS4ControlItem.GyroXPos,
                "Tilt Right" => DS4ControlItem.GyroXNeg,
                _ => 0
            };
        }
    }
}