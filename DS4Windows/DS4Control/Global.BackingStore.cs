﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Xml;
using DS4Windows.Shared.Common.Attributes;
using DS4Windows.Shared.Common.Core;
using DS4Windows.Shared.Common.Legacy;
using DS4Windows.Shared.Common.Types;
using DS4Windows.Shared.Configuration.Profiles.Services;
using DS4WinWPF.DS4Control.IoC.Services;
using DS4WinWPF.DS4Control.Logging;
using DS4WinWPF.DS4Control.Profiles.Schema;
using DS4WinWPF.Properties;

namespace DS4Windows
{
    public partial class Global
    {
        private class BackingStore : IBackingStore
        {
            private readonly int[] _l2OutCurveMode = new int[TEST_PROFILE_ITEM_COUNT] { 0, 0, 0, 0, 0, 0, 0, 0, 0 };

            private readonly int[] _lsOutCurveMode = new int[TEST_PROFILE_ITEM_COUNT] { 0, 0, 0, 0, 0, 0, 0, 0, 0 };

            private readonly int[] _r2OutCurveMode = new int[TEST_PROFILE_ITEM_COUNT] { 0, 0, 0, 0, 0, 0, 0, 0, 0 };

            private readonly int[] _rsOutCurveMode = new int[TEST_PROFILE_ITEM_COUNT] { 0, 0, 0, 0, 0, 0, 0, 0, 0 };

            private readonly int[] _sxOutCurveMode = new int[TEST_PROFILE_ITEM_COUNT] { 0, 0, 0, 0, 0, 0, 0, 0, 0 };

            private readonly int[] _szOutCurveMode = new int[TEST_PROFILE_ITEM_COUNT] { 0, 0, 0, 0, 0, 0, 0, 0, 0 };

            private readonly IList<ControlSettingsGroup> ds4controlSettings;

            private readonly List<DS4ControlSettingsV3>[] Ds4Settings =
                new List<DS4ControlSettingsV3>[TEST_PROFILE_ITEM_COUNT]
                {
                    new(), new(), new(),
                    new(), new(), new(), new(), new(), new()
                };

            protected readonly XmlDocument m_Xdoc = new();

            private readonly int[] profileActionCount = new int[TEST_PROFILE_ITEM_COUNT] { 0, 0, 0, 0, 0, 0, 0, 0, 0 };

            private readonly Dictionary<string, SpecialActionV3>[] profileActionDict =
                new Dictionary<string, SpecialActionV3>[TEST_PROFILE_ITEM_COUNT]
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
                ds4controlSettings = new ControlSettingsGroup[TEST_PROFILE_ITEM_COUNT - 1];

                for (var i = 0; i < TEST_PROFILE_ITEM_COUNT - 1; i++)
                {
                    foreach (DS4ControlItem dc in Enum.GetValues(typeof(DS4ControlItem)))
                        if (dc != DS4ControlItem.None)
                            Ds4Settings[i].Add(new DS4ControlSettingsV3(dc));

                    ds4controlSettings[i] = new ControlSettingsGroup(Ds4Settings[i]);

                    EstablishDefaultSpecialActions(i);
                    CacheExtraProfileInfo(i);
                }
            }

            private Dictionary<PhysicalAddress, string> LinkedProfiles { get; set; } = new();

            /// <summary>
            ///     TRUE=AutoProfile reverts to default profile if current foreground process is unknown, FALSE=Leave existing profile
            ///     active when a foreground process is unknown (ie. no matching auto-profile rule)
            /// </summary>
            public bool AutoProfileRevertDefaultProfile { get; set; } = true;

            public string UseLang { get; set; } = string.Empty;

            public ulong LastVersionCheckedNumber { get; set; }

            public IList<string> SAMouseStickTriggers { get; set; } = new List<string>
                { "-1", "-1", "-1", "-1", "-1", "-1", "-1", "-1", "-1" };

            public IList<bool> SATriggerCondition { get; set; } = new List<bool>
                { true, true, true, true, true, true, true, true, true };

            public IList<IList<int>> TouchDisInvertTriggers { get; set; } = new List<IList<int>>
            {
                new int[1] { -1 }, new int[1] { -1 }, new int[1] { -1 },
                new int[1] { -1 }, new int[1] { -1 }, new int[1] { -1 }, new int[1] { -1 }, new int[1] { -1 },
                new int[1] { -1 }
            };

            [Obsolete]
            public string ProfilesPath { get; set; } =
                Path.Combine(Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName,
                    Constants.LegacyProfilesFileName);

            public string ActionsPath { get; set; } = Path.Combine(RuntimeAppDataPath, Constants.LegacyActionsFileName);

            [Obsolete]
            public string LinkedProfilesPath { get; set; } =
                Path.Combine(RuntimeAppDataPath, Constants.LegacyLinkedProfilesFileName);

            public string ControllerConfigsPath { get; set; } =
                Path.Combine(RuntimeAppDataPath, Constants.LegacyControllerConfigsFileName);

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

            public IList<int> GyroMouseDeadZone { get; set; } = new List<int>
            {
                MouseCursor.GYRO_MOUSE_DEADZONE, MouseCursor.GYRO_MOUSE_DEADZONE,
                MouseCursor.GYRO_MOUSE_DEADZONE, MouseCursor.GYRO_MOUSE_DEADZONE,
                MouseCursor.GYRO_MOUSE_DEADZONE, MouseCursor.GYRO_MOUSE_DEADZONE,
                MouseCursor.GYRO_MOUSE_DEADZONE, MouseCursor.GYRO_MOUSE_DEADZONE,
                MouseCursor.GYRO_MOUSE_DEADZONE
            };

            public IList<BezierCurve> LSOutCurve { get; set; } = new List<BezierCurve>
                { new(), new(), new(), new(), new(), new(), new(), new(), new() };

            public IList<BezierCurve> RSOutCurve { get; set; } = new List<BezierCurve>
                { new(), new(), new(), new(), new(), new(), new(), new(), new() };

            public IList<BezierCurve> L2OutCurve { get; set; } = new List<BezierCurve>
                { new(), new(), new(), new(), new(), new(), new(), new(), new() };

            public IList<BezierCurve> R2OutCurve { get; set; } = new List<BezierCurve>
                { new(), new(), new(), new(), new(), new(), new(), new(), new() };

            public IList<BezierCurve> SXOutCurve { get; set; } = new List<BezierCurve>
                { new(), new(), new(), new(), new(), new(), new(), new(), new() };

            public IList<BezierCurve> SZOutCurve { get; set; } = new List<BezierCurve>
                { new(), new(), new(), new(), new(), new(), new(), new(), new() };

            public IList<string> LaunchProgram { get; set; } = new List<string>
            {
                string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty,
                string.Empty, string.Empty
            };

            public bool Ds4Mapping { get; set; }

            public IList<SpecialActionV3> Actions { get; set; } = new List<SpecialActionV3>();

            public IList<string>[] ProfileActions { get; set; } =
                { null, null, null, null, null, null, null, null, null };

            public string FakeExeFileName { get; set; } = string.Empty;

            // Cache whether profile has custom action
            public IList<bool> ContainsCustomAction { get; set; } = new bool[TEST_PROFILE_ITEM_COUNT]
                { false, false, false, false, false, false, false, false, false };

            // Cache whether profile has custom extras
            public IList<bool> ContainsCustomExtras { get; set; } = new bool[TEST_PROFILE_ITEM_COUNT]
                { false, false, false, false, false, false, false, false, false };

            public void RefreshExtrasButtons(int deviceNum, List<DS4ControlItem> devButtons)
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
                    SetOutBezierCurveObjArrayItem(LSOutCurve, index, value, BezierCurve.AxisType.LSRS);
                _lsOutCurveMode[index] = value;
            }

            public int GetRsOutCurveMode(int index)
            {
                return _rsOutCurveMode[index];
            }

            public void SetRsOutCurveMode(int index, int value)
            {
                if (value >= 1)
                    SetOutBezierCurveObjArrayItem(RSOutCurve, index, value, BezierCurve.AxisType.LSRS);
                _rsOutCurveMode[index] = value;
            }

            public int GetL2OutCurveMode(int index)
            {
                return _l2OutCurveMode[index];
            }

            public void SetL2OutCurveMode(int index, int value)
            {
                if (value >= 1)
                    SetOutBezierCurveObjArrayItem(L2OutCurve, index, value, BezierCurve.AxisType.L2R2);
                _l2OutCurveMode[index] = value;
            }

            public int GetR2OutCurveMode(int index)
            {
                return _r2OutCurveMode[index];
            }

            public void SetR2OutCurveMode(int index, int value)
            {
                if (value >= 1)
                    SetOutBezierCurveObjArrayItem(R2OutCurve, index, value, BezierCurve.AxisType.L2R2);
                _r2OutCurveMode[index] = value;
            }

            public int GetSXOutCurveMode(int index)
            {
                return _sxOutCurveMode[index];
            }

            public void SetSXOutCurveMode(int index, int value)
            {
                if (value >= 1)
                    SetOutBezierCurveObjArrayItem(SXOutCurve, index, value, BezierCurve.AxisType.SA);
                _sxOutCurveMode[index] = value;
            }

            public int GetSZOutCurveMode(int index)
            {
                return _szOutCurveMode[index];
            }

            public void SetSZOutCurveMode(int index, int value)
            {
                if (value >= 1)
                    SetOutBezierCurveObjArrayItem(SZOutCurve, index, value, BezierCurve.AxisType.SA);
                _szOutCurveMode[index] = value;
            }

            public bool GetSATriggerCondition(int index)
            {
                return SATriggerCondition[index];
            }

            public string GetSAMouseStickTriggers(int device)
            {
                return SAMouseStickTriggers[device];
            }

            public bool IsUsingTouchpadForControls(int index)
            {
                return ProfilesService.Instance.ActiveProfiles.ElementAt(index).TouchOutMode ==
                       TouchpadOutMode.Controls;
            }

            public bool IsUsingSAForControls(int index)
            {
                return ProfilesService.Instance.ActiveProfiles.ElementAt(index).GyroOutputMode == GyroOutMode.Controls;
            }

            public int GetProfileActionCount(int index)
            {
                return profileActionCount[index];
            }

            public ControlSettingsGroup GetControlSettingsGroup(int deviceNum)
            {
                return ds4controlSettings[deviceNum];
            }

            public void CacheProfileCustomsFlags(int device)
            {
                var customAct = false;
                ContainsCustomAction[device] = customAct = HasCustomActions(device);
                ContainsCustomExtras[device] = HasCustomExtras(device);

                if (!customAct)
                {
                    customAct = ProfilesService.Instance.ActiveProfiles.ElementAt(device).GyroOutputMode ==
                                GyroOutMode.MouseJoystick;
                    customAct = customAct ||
                                ProfilesService.Instance.ActiveProfiles.ElementAt(device)
                                    .SASteeringWheelEmulationAxis >= SASteeringWheelEmulationAxisType.VJoy1X;
                    customAct = customAct ||
                                ProfilesService.Instance.ActiveProfiles.ElementAt(device).LSOutputSettings.Mode !=
                                StickMode.Controls;
                    customAct = customAct ||
                                ProfilesService.Instance.ActiveProfiles.ElementAt(device).RSOutputSettings.Mode !=
                                StickMode.Controls;
                    ContainsCustomAction[device] = customAct;
                }
            }

            public void CacheExtraProfileInfo(int device)
            {
                CalculateProfileActionCount(device);
                CalculateProfileActionDicts(device);
                CacheProfileCustomsFlags(device);
            }

            public SpecialActionV3 GetAction(string name)
            {
                //foreach (SpecialAction sA in actions)
                for (int i = 0, actionCount = Actions.Count; i < actionCount; i++)
                {
                    var sA = Actions[i];
                    if (sA.Name == name)
                        return sA;
                }

                return new SpecialActionV3("null", "null", "null", "null");
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

            [Obsolete]
            public void SetSaMouseStickTriggerCond(int index, string text)
            {
                //SAMouseStickTriggerCond[index] = SaTriggerCondValue(text);
            }

            public void SetGyroMouseDZ(int index, int value, ControlService control)
            {
                GyroMouseDeadZone[index] = value;
                if (index < ControlService.CURRENT_DS4_CONTROLLER_LIMIT && control.touchPad[index] != null)
                    control.touchPad[index].CursorGyroDead = value;
            }

            [Obsolete]
            public void SetGyroControlsToggle(int index, bool value, ControlService control)
            {
                //ProfilesService.Instance.ControllerSlotProfiles.ElementAt(index).GyroControlsInfo.TriggerToggle = value;
                if (index < ControlService.CURRENT_DS4_CONTROLLER_LIMIT && control.touchPad[index] != null)
                    control.touchPad[index].ToggleGyroControls = value;
            }

            [Obsolete]
            public void SetGyroMouseToggle(int index, bool value, ControlService control)
            {
                //GyroMouseToggle[index] = value;
                if (index < ControlService.CURRENT_DS4_CONTROLLER_LIMIT && control.touchPad[index] != null)
                    control.touchPad[index].ToggleGyroMouse = value;
            }

            [Obsolete]
            public void SetGyroMouseStickToggle(int index, bool value, ControlService control)
            {
                //GyroMouseStickToggle[index] = value;
                if (index < ControlService.CURRENT_DS4_CONTROLLER_LIMIT && control.touchPad[index] != null)
                    control.touchPad[index].ToggleGyroStick = value;
            }

            public DS4ControlItem GetDs4ControlsByName(string key)
            {
                if (!key.StartsWith("bn"))
                    return (DS4ControlItem)Enum.Parse(typeof(DS4ControlItem), key, true);

                switch (key)
                {
                    case "bnShare": return DS4ControlItem.Share;
                    case "bnL3": return DS4ControlItem.L3;
                    case "bnR3": return DS4ControlItem.R3;
                    case "bnOptions": return DS4ControlItem.Options;
                    case "bnUp": return DS4ControlItem.DpadUp;
                    case "bnRight": return DS4ControlItem.DpadRight;
                    case "bnDown": return DS4ControlItem.DpadDown;
                    case "bnLeft": return DS4ControlItem.DpadLeft;

                    case "bnL1": return DS4ControlItem.L1;
                    case "bnR1": return DS4ControlItem.R1;
                    case "bnTriangle": return DS4ControlItem.Triangle;
                    case "bnCircle": return DS4ControlItem.Circle;
                    case "bnCross": return DS4ControlItem.Cross;
                    case "bnSquare": return DS4ControlItem.Square;

                    case "bnPS": return DS4ControlItem.PS;
                    case "bnLSLeft": return DS4ControlItem.LXNeg;
                    case "bnLSUp": return DS4ControlItem.LYNeg;
                    case "bnRSLeft": return DS4ControlItem.RXNeg;
                    case "bnRSUp": return DS4ControlItem.RYNeg;

                    case "bnLSRight": return DS4ControlItem.LXPos;
                    case "bnLSDown": return DS4ControlItem.LYPos;
                    case "bnRSRight": return DS4ControlItem.RXPos;
                    case "bnRSDown": return DS4ControlItem.RYPos;
                    case "bnL2": return DS4ControlItem.L2;
                    case "bnR2": return DS4ControlItem.R2;

                    case "bnTouchLeft": return DS4ControlItem.TouchLeft;
                    case "bnTouchMulti": return DS4ControlItem.TouchMulti;
                    case "bnTouchUpper": return DS4ControlItem.TouchUpper;
                    case "bnTouchRight": return DS4ControlItem.TouchRight;
                    case "bnGyroXP": return DS4ControlItem.GyroXPos;
                    case "bnGyroXN": return DS4ControlItem.GyroXNeg;
                    case "bnGyroZP": return DS4ControlItem.GyroZPos;
                    case "bnGyroZN": return DS4ControlItem.GyroZNeg;

                    case "bnSwipeUp": return DS4ControlItem.SwipeUp;
                    case "bnSwipeDown": return DS4ControlItem.SwipeDown;
                    case "bnSwipeLeft": return DS4ControlItem.SwipeLeft;
                    case "bnSwipeRight": return DS4ControlItem.SwipeRight;

                    #region OldShiftname

                    case "sbnShare": return DS4ControlItem.Share;
                    case "sbnL3": return DS4ControlItem.L3;
                    case "sbnR3": return DS4ControlItem.R3;
                    case "sbnOptions": return DS4ControlItem.Options;
                    case "sbnUp": return DS4ControlItem.DpadUp;
                    case "sbnRight": return DS4ControlItem.DpadRight;
                    case "sbnDown": return DS4ControlItem.DpadDown;
                    case "sbnLeft": return DS4ControlItem.DpadLeft;

                    case "sbnL1": return DS4ControlItem.L1;
                    case "sbnR1": return DS4ControlItem.R1;
                    case "sbnTriangle": return DS4ControlItem.Triangle;
                    case "sbnCircle": return DS4ControlItem.Circle;
                    case "sbnCross": return DS4ControlItem.Cross;
                    case "sbnSquare": return DS4ControlItem.Square;

                    case "sbnPS": return DS4ControlItem.PS;
                    case "sbnLSLeft": return DS4ControlItem.LXNeg;
                    case "sbnLSUp": return DS4ControlItem.LYNeg;
                    case "sbnRSLeft": return DS4ControlItem.RXNeg;
                    case "sbnRSUp": return DS4ControlItem.RYNeg;

                    case "sbnLSRight": return DS4ControlItem.LXPos;
                    case "sbnLSDown": return DS4ControlItem.LYPos;
                    case "sbnRSRight": return DS4ControlItem.RXPos;
                    case "sbnRSDown": return DS4ControlItem.RYPos;
                    case "sbnL2": return DS4ControlItem.L2;
                    case "sbnR2": return DS4ControlItem.R2;

                    case "sbnTouchLeft": return DS4ControlItem.TouchLeft;
                    case "sbnTouchMulti": return DS4ControlItem.TouchMulti;
                    case "sbnTouchUpper": return DS4ControlItem.TouchUpper;
                    case "sbnTouchRight": return DS4ControlItem.TouchRight;
                    case "sbnGsyroXP": return DS4ControlItem.GyroXPos;
                    case "sbnGyroXN": return DS4ControlItem.GyroXNeg;
                    case "sbnGyroZP": return DS4ControlItem.GyroZPos;
                    case "sbnGyroZN": return DS4ControlItem.GyroZNeg;

                    #endregion

                    case "bnShiftShare": return DS4ControlItem.Share;
                    case "bnShiftL3": return DS4ControlItem.L3;
                    case "bnShiftR3": return DS4ControlItem.R3;
                    case "bnShiftOptions": return DS4ControlItem.Options;
                    case "bnShiftUp": return DS4ControlItem.DpadUp;
                    case "bnShiftRight": return DS4ControlItem.DpadRight;
                    case "bnShiftDown": return DS4ControlItem.DpadDown;
                    case "bnShiftLeft": return DS4ControlItem.DpadLeft;

                    case "bnShiftL1": return DS4ControlItem.L1;
                    case "bnShiftR1": return DS4ControlItem.R1;
                    case "bnShiftTriangle": return DS4ControlItem.Triangle;
                    case "bnShiftCircle": return DS4ControlItem.Circle;
                    case "bnShiftCross": return DS4ControlItem.Cross;
                    case "bnShiftSquare": return DS4ControlItem.Square;

                    case "bnShiftPS": return DS4ControlItem.PS;
                    case "bnShiftLSLeft": return DS4ControlItem.LXNeg;
                    case "bnShiftLSUp": return DS4ControlItem.LYNeg;
                    case "bnShiftRSLeft": return DS4ControlItem.RXNeg;
                    case "bnShiftRSUp": return DS4ControlItem.RYNeg;

                    case "bnShiftLSRight": return DS4ControlItem.LXPos;
                    case "bnShiftLSDown": return DS4ControlItem.LYPos;
                    case "bnShiftRSRight": return DS4ControlItem.RXPos;
                    case "bnShiftRSDown": return DS4ControlItem.RYPos;
                    case "bnShiftL2": return DS4ControlItem.L2;
                    case "bnShiftR2": return DS4ControlItem.R2;

                    case "bnShiftTouchLeft": return DS4ControlItem.TouchLeft;
                    case "bnShiftTouchMulti": return DS4ControlItem.TouchMulti;
                    case "bnShiftTouchUpper": return DS4ControlItem.TouchUpper;
                    case "bnShiftTouchRight": return DS4ControlItem.TouchRight;
                    case "bnShiftGyroXP": return DS4ControlItem.GyroXPos;
                    case "bnShiftGyroXN": return DS4ControlItem.GyroXNeg;
                    case "bnShiftGyroZP": return DS4ControlItem.GyroZPos;
                    case "bnShiftGyroZN": return DS4ControlItem.GyroZNeg;

                    case "bnShiftSwipeUp": return DS4ControlItem.SwipeUp;
                    case "bnShiftSwipeDown": return DS4ControlItem.SwipeDown;
                    case "bnShiftSwipeLeft": return DS4ControlItem.SwipeLeft;
                    case "bnShiftSwipeRight": return DS4ControlItem.SwipeRight;
                }

                return 0;
            }

            public X360ControlItem GetX360ControlsByName(string key)
            {
                X360ControlItem x3c;
                if (Enum.TryParse(key, true, out x3c))
                    return x3c;

                switch (key)
                {
                    case "Back": return X360ControlItem.Back;
                    case "Left Stick": return X360ControlItem.LS;
                    case "Right Stick": return X360ControlItem.RS;
                    case "Start": return X360ControlItem.Start;
                    case "Up Button": return X360ControlItem.DpadUp;
                    case "Right Button": return X360ControlItem.DpadRight;
                    case "Down Button": return X360ControlItem.DpadDown;
                    case "Left Button": return X360ControlItem.DpadLeft;

                    case "Left Bumper": return X360ControlItem.LB;
                    case "Right Bumper": return X360ControlItem.RB;
                    case "Y Button": return X360ControlItem.Y;
                    case "B Button": return X360ControlItem.B;
                    case "A Button": return X360ControlItem.A;
                    case "X Button": return X360ControlItem.X;

                    case "Guide": return X360ControlItem.Guide;
                    case "Left X-Axis-": return X360ControlItem.LXNeg;
                    case "Left Y-Axis-": return X360ControlItem.LYNeg;
                    case "Right X-Axis-": return X360ControlItem.RXNeg;
                    case "Right Y-Axis-": return X360ControlItem.RYNeg;

                    case "Left X-Axis+": return X360ControlItem.LXPos;
                    case "Left Y-Axis+": return X360ControlItem.LYPos;
                    case "Right X-Axis+": return X360ControlItem.RXPos;
                    case "Right Y-Axis+": return X360ControlItem.RYPos;
                    case "Left Trigger": return X360ControlItem.LT;
                    case "Right Trigger": return X360ControlItem.RT;
                    case "Touchpad Click": return X360ControlItem.TouchpadClick;

                    case "Left Mouse Button": return X360ControlItem.LeftMouse;
                    case "Right Mouse Button": return X360ControlItem.RightMouse;
                    case "Middle Mouse Button": return X360ControlItem.MiddleMouse;
                    case "4th Mouse Button": return X360ControlItem.FourthMouse;
                    case "5th Mouse Button": return X360ControlItem.FifthMouse;
                    case "Mouse Wheel Up": return X360ControlItem.WUP;
                    case "Mouse Wheel Down": return X360ControlItem.WDOWN;
                    case "Mouse Up": return X360ControlItem.MouseUp;
                    case "Mouse Down": return X360ControlItem.MouseDown;
                    case "Mouse Left": return X360ControlItem.MouseLeft;
                    case "Mouse Right": return X360ControlItem.MouseRight;
                    case "Unbound": return X360ControlItem.Unbound;
                }

                return X360ControlItem.Unbound;
            }

            public string GetX360ControlString(X360ControlItem key)
            {
                switch (key)
                {
                    case X360ControlItem.Back: return "Back";
                    case X360ControlItem.LS: return "Left Stick";
                    case X360ControlItem.RS: return "Right Stick";
                    case X360ControlItem.Start: return "Start";
                    case X360ControlItem.DpadUp: return "Up Button";
                    case X360ControlItem.DpadRight: return "Right Button";
                    case X360ControlItem.DpadDown: return "Down Button";
                    case X360ControlItem.DpadLeft: return "Left Button";

                    case X360ControlItem.LB: return "Left Bumper";
                    case X360ControlItem.RB: return "Right Bumper";
                    case X360ControlItem.Y: return "Y Button";
                    case X360ControlItem.B: return "B Button";
                    case X360ControlItem.A: return "A Button";
                    case X360ControlItem.X: return "X Button";

                    case X360ControlItem.Guide: return "Guide";
                    case X360ControlItem.LXNeg: return "Left X-Axis-";
                    case X360ControlItem.LYNeg: return "Left Y-Axis-";
                    case X360ControlItem.RXNeg: return "Right X-Axis-";
                    case X360ControlItem.RYNeg: return "Right Y-Axis-";

                    case X360ControlItem.LXPos: return "Left X-Axis+";
                    case X360ControlItem.LYPos: return "Left Y-Axis+";
                    case X360ControlItem.RXPos: return "Right X-Axis+";
                    case X360ControlItem.RYPos: return "Right Y-Axis+";
                    case X360ControlItem.LT: return "Left Trigger";
                    case X360ControlItem.RT: return "Right Trigger";
                    case X360ControlItem.TouchpadClick: return "Touchpad Click";

                    case X360ControlItem.LeftMouse: return "Left Mouse Button";
                    case X360ControlItem.RightMouse: return "Right Mouse Button";
                    case X360ControlItem.MiddleMouse: return "Middle Mouse Button";
                    case X360ControlItem.FourthMouse: return "4th Mouse Button";
                    case X360ControlItem.FifthMouse: return "5th Mouse Button";
                    case X360ControlItem.WUP: return "Mouse Wheel Up";
                    case X360ControlItem.WDOWN: return "Mouse Wheel Down";
                    case X360ControlItem.MouseUp: return "Mouse Up";
                    case X360ControlItem.MouseDown: return "Mouse Down";
                    case X360ControlItem.MouseLeft: return "Mouse Left";
                    case X360ControlItem.MouseRight: return "Mouse Right";
                    case X360ControlItem.Unbound: return "Unbound";
                }

                return "Unbound";
            }

            public async Task<bool> SaveAsNewProfile(int device, string proName)
            {
                ResetProfile(device);
                return await SaveProfile(device, proName);
            }

            /// <summary>
            ///     Persists a <see cref="DS4WindowsProfileV3" /> on disk.
            /// </summary>
            /// <param name="device">The index of the device to store the profile for.</param>
            /// <param name="proName">The profile name (without extension or root path).</param>
            /// <returns>True on success, false otherwise.</returns>
            [ConfigurationSystemComponent]
            [Obsolete]
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

                var profileObject = new DS4WindowsProfileV3(
                    ControlService.CurrentInstance.GetAppSettings(),
                    this,
                    device,
                    ExecutableProductVersion,
                    CONFIG_VERSION
                );

                try
                {
                    foreach (var dcs in Ds4Settings[device])
                    {
                        var property = $"{dcs.Control}.Value";

                        if (dcs.ControlActionType != DS4ControlSettingsV3.ActionType.Default)
                        {
                            var keyType = string.Empty;

                            if (dcs.ControlActionType == DS4ControlSettingsV3.ActionType.Button &&
                                dcs.ActionData.ActionButton == X360ControlItem.Unbound)
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

                            if (dcs.ControlActionType == DS4ControlSettingsV3.ActionType.Macro)
                            {
                                var ii = dcs.ActionData.ActionMacro;

                                SetNestedProperty(property, profileObject.Controls.Macros, string.Join("/", ii));
                            }
                            else if (dcs.ControlActionType == DS4ControlSettingsV3.ActionType.Key)
                            {
                                SetNestedProperty(property, profileObject.Controls.Keys,
                                    dcs.ActionData.ActionKey.ToString());
                            }
                            else if (dcs.ControlActionType == DS4ControlSettingsV3.ActionType.Button)
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

                        if (dcs.ShiftActionType != DS4ControlSettingsV3.ActionType.Default && dcs.ShiftTrigger > 0)
                        {
                            var keyType = string.Empty;

                            if (dcs.ShiftActionType == DS4ControlSettingsV3.ActionType.Button &&
                                dcs.ShiftAction.ActionButton == X360ControlItem.Unbound)
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

                            if (dcs.ShiftActionType == DS4ControlSettingsV3.ActionType.Macro)
                            {
                                var ii = dcs.ShiftAction.ActionMacro;

                                SetNestedProperty(property, profileObject.ShiftControls.Macros, string.Join("/", ii));
                                SetNestedProperty($"{dcs.Control}.ShiftTrigger", profileObject.ShiftControls.Macros,
                                    dcs.ShiftTrigger.ToString());
                            }
                            else if (dcs.ShiftActionType == DS4ControlSettingsV3.ActionType.Key)
                            {
                                SetNestedProperty(property, profileObject.ShiftControls.Keys,
                                    dcs.ShiftAction.ActionKey.ToString());
                                SetNestedProperty($"{dcs.Control}.ShiftTrigger", profileObject.ShiftControls.Keys,
                                    dcs.ShiftTrigger.ToString());
                            }
                            else if (dcs.ShiftActionType == DS4ControlSettingsV3.ActionType.Button)
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
            [Obsolete]
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
                var customMapKeyTypes = new Dictionary<DS4ControlItem, DS4KeyType>();
                var customMapKeys = new Dictionary<DS4ControlItem, ushort>();
                var customMapButtons = new Dictionary<DS4ControlItem, X360ControlItem>();
                var customMapMacros = new Dictionary<DS4ControlItem, string>();
                var customMapExtras = new Dictionary<DS4ControlItem, string>();
                var shiftCustomMapKeyTypes = new Dictionary<DS4ControlItem, DS4KeyType>();
                var shiftCustomMapKeys = new Dictionary<DS4ControlItem, ushort>();
                var shiftCustomMapButtons = new Dictionary<DS4ControlItem, X360ControlItem>();
                var shiftCustomMapMacros = new Dictionary<DS4ControlItem, string>();
                var shiftCustomMapExtras = new Dictionary<DS4ControlItem, string>();
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

                if (File.Exists(profilepath))
                {
                    XmlNode Item;

                    m_Xdoc.Load(profilepath);

                    if (device < MAX_DS4_CONTROLLER_COUNT)
                    {
                        DS4LightBarV3.forcelight[device] = false;
                        DS4LightBarV3.forcedFlash[device] = 0;
                    }

                    var oldContType = ActiveOutDevType[device];

                    // Make sure to reset currently set profile values before parsing
                    ResetProfile(device);
                    ResetMouseProperties(device, control);

                    DS4WindowsProfileV3 profile = null;

                    //
                    // TODO: unfinished
                    // 
                    await using (var stream = File.OpenRead(profilepath))
                    {
                        profile = await DS4WindowsProfileV3.DeserializeAsync(stream);

                        profile.CopyTo(control.GetAppSettings(), this, device);
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
                                    ProfilesService.Instance.ActiveProfiles.ElementAt(device).TouchOutMode =
                                        TouchpadOutMode.Controls;
                        }
                        catch
                        {
                            ProfilesService.Instance.ActiveProfiles.ElementAt(device).TouchOutMode =
                                TouchpadOutMode.Mouse;
                        }

                    // Fallback lookup if GyroOutMode is not set
                    try
                    {
                        Item = m_Xdoc.SelectSingleNode("/" + rootname + "/UseSAforMouse");
                        if (bool.TryParse(Item?.InnerText ?? "", out var temp))
                            if (temp)
                                ProfilesService.Instance.ActiveProfiles.ElementAt(device).GyroOutputMode =
                                    GyroOutMode.Mouse;
                    }
                    catch
                    {
                        ProfilesService.Instance.ActiveProfiles.ElementAt(device).GyroOutputMode = GyroOutMode.Controls;
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
                            ProfilesService.Instance.ActiveProfiles.ElementAt(device).TouchOutMode = value;
                        }
                        catch
                        {
                            ProfilesService.Instance.ActiveProfiles.ElementAt(device).TouchOutMode =
                                TouchpadOutMode.Mouse;
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

                    /*
                     TODO: migrate
                    try
                    {
                        Item = m_Xdoc.SelectSingleNode("/" + rootname + "/L2HipFireDelay");
                        if (int.TryParse(Item?.InnerText, out var temp))
                            L2OutputSettings[device].HipFireMs = Math.Max(Math.Min(0, temp), 5000);
                    }
                    catch
                    {
                    }

                    try
                    {
                        Item = m_Xdoc.SelectSingleNode("/" + rootname + "/R2HipFireDelay");
                        if (int.TryParse(Item?.InnerText, out var temp))
                            R2OutputSettings[device].HipFireMs = Math.Max(Math.Min(0, temp), 5000);
                    }
                    catch
                    {
                    }
                    */

                    /*
                    // Only change xinput devices under certain conditions. Avoid
                    // performing this upon program startup before loading devices.
                    if (xinputChange && device < ControlService.CURRENT_DS4_CONTROLLER_LIMIT)
                        CheckOldDeviceStatus(device, control, oldContType,
                            out xinputPlug, out xinputStatus);
                    */

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

                        foreach (var item in controls.Where(item => Enum.TryParse(item.Name, out DS4ControlItem _)))
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

                            if (Enum.TryParse(item.Name, out DS4ControlItem _))
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
                                Enum.TryParse(item.Name, out DS4ControlItem _))
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
                                                                    Enum.TryParse(item.Name, out DS4ControlItem _)))
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

                            if (keyType == DS4KeyType.None || !Enum.TryParse(item.Name, out DS4ControlItem _)) continue;

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

                            if (Enum.TryParse(item.Name, out DS4ControlItem _))
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

                            if (Enum.TryParse(item.Name, out DS4ControlItem _))
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

                                if (Enum.TryParse(item.Name, out DS4ControlItem _))
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

                        foreach (var item in controls.Where(item => Enum.TryParse(item.Name, out DS4ControlItem _)))
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
                                Enum.TryParse(item.Name, out DS4ControlItem _))
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
                            tempDev.QueueEvent(() => { control.UnplugOutDev(device, tempDev); });
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
                    /*
                     TODO: port
                    ButtonMouseInfos[device].activeButtonSensitivity =
                        ButtonMouseInfos[device].buttonSensitivity;
                    */

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
            [Obsolete]
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
            [Obsolete]
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
            [Obsolete]
            public bool LoadActions()
            {
                var saved = true;
                if (!File.Exists(Path.Combine(RuntimeAppDataPath, Constants.LegacyActionsFileName)))
                {
                    SaveAction("Disconnect Controller", "PS/Options", 5, "0", false);
                    saved = false;
                }

                try
                {
                    Actions.Clear();
                    var doc = new XmlDocument();
                    doc.Load(Path.Combine(RuntimeAppDataPath, Constants.LegacyActionsFileName));
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
                            Actions.Add(new SpecialActionV3(name, controls, type, details, 0, extras));
                        }
                        else if (type == "Macro")
                        {
                            if (x.ChildNodes[3] != null) extras = x.ChildNodes[3].InnerText;
                            else extras = string.Empty;
                            Actions.Add(new SpecialActionV3(name, controls, type, details, 0, extras));
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
                                    new SpecialActionV3(name, controls, type, details, 0, extras2 + '\n' + extras));
                            else
                                Actions.Add(new SpecialActionV3(name, controls, type, details));
                        }
                        else if (type == "DisconnectBT")
                        {
                            double doub;
                            if (double.TryParse(details, NumberStyles.Float, ConfigFileDecimalCulture, out doub))
                                Actions.Add(new SpecialActionV3(name, controls, type, "", doub));
                            else
                                Actions.Add(new SpecialActionV3(name, controls, type, ""));
                        }
                        else if (type == "BatteryCheck")
                        {
                            double doub;
                            if (double.TryParse(details.Split('|')[0], NumberStyles.Float, ConfigFileDecimalCulture,
                                    out doub))
                                Actions.Add(new SpecialActionV3(name, controls, type, details, doub));
                            else if (double.TryParse(details.Split(',')[0], NumberStyles.Float,
                                         ConfigFileDecimalCulture, out doub))
                                Actions.Add(new SpecialActionV3(name, controls, type, details, doub));
                            else
                                Actions.Add(new SpecialActionV3(name, controls, type, details));
                        }
                        else if (type == "Program")
                        {
                            double doub;
                            if (x.ChildNodes[3] != null)
                            {
                                extras = x.ChildNodes[3].InnerText;
                                if (double.TryParse(x.ChildNodes[4].InnerText, NumberStyles.Float,
                                        ConfigFileDecimalCulture, out doub))
                                    Actions.Add(new SpecialActionV3(name, controls, type, details, doub, extras));
                                else
                                    Actions.Add(new SpecialActionV3(name, controls, type, details, 0, extras));
                            }
                            else
                            {
                                Actions.Add(new SpecialActionV3(name, controls, type, details));
                            }
                        }
                        else if (type == "XboxGameDVR" || type == "MultiAction")
                        {
                            Actions.Add(new SpecialActionV3(name, controls, type, details));
                        }
                        else if (type == "SASteeringWheelEmulationCalibrate")
                        {
                            double doub;
                            if (double.TryParse(details, NumberStyles.Float, ConfigFileDecimalCulture, out doub))
                                Actions.Add(new SpecialActionV3(name, controls, type, "", doub));
                            else
                                Actions.Add(new SpecialActionV3(name, controls, type, ""));
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
            [Obsolete]
            public bool LoadLinkedProfiles()
            {
                var loaded = true;
                if (File.Exists(LinkedProfilesPath))
                {
                    try
                    {
                        using var stream = File.OpenRead(LinkedProfilesPath);

                        var profiles = LinkedProfilesV3.Deserialize(stream);

                        LinkedProfiles = profiles.LegacyAssignments.ToDictionary(
                            x => x.Key,
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
            [Obsolete]
            public bool SaveLinkedProfiles()
            {
                var saved = true;

                try
                {
                    using var stream = File.Open(LinkedProfilesPath, FileMode.Create);

                    var profiles = new LinkedProfilesV3
                    {
                        //Assignments = LinkedProfiles.ToDictionary(x => PhysicalAddress.Parse(x.Key), x => Guid.Parse(x.Value))
                        LegacyAssignments = LinkedProfiles.ToDictionary(x => x.Key, x => x.Value)
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
            [Obsolete]
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
            [Obsolete]
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
                DS4ControlItem dc;
                if (buttonName.StartsWith("bn"))
                    dc = GetDs4ControlsByName(buttonName);
                else
                    dc = (DS4ControlItem)Enum.Parse(typeof(DS4ControlItem), buttonName, true);

                var temp = (int)dc;
                if (temp > 0)
                {
                    var index = temp - 1;
                    var dcs = Ds4Settings[deviceNum][index];
                    dcs.UpdateSettings(shift, action, exts, kt, trigger);
                    RefreshActionAlias(dcs, shift);
                }
            }

            public SpecialActionV3 GetProfileAction(int device, string name)
            {
                SpecialActionV3 sA = null;
                profileActionDict[device].TryGetValue(name, out sA);
                return sA;
            }

            public void ChangeLinkedProfile(PhysicalAddress serial, string profile)
            {
                LinkedProfiles[serial] = profile;
            }

            public int GetProfileActionIndexOf(int device, string name)
            {
                var index = -1;
                profileActionIndexDict[device].TryGetValue(name, out index);
                return index;
            }

            public void UpdateDs4ControllerExtra(int deviceNum, string buttonName, bool shift, string exts)
            {
                DS4ControlItem dc;
                if (buttonName.StartsWith("bn"))
                    dc = GetDs4ControlsByName(buttonName);
                else
                    dc = (DS4ControlItem)Enum.Parse(typeof(DS4ControlItem), buttonName, true);

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
                DS4ControlItem dc;
                if (buttonName.StartsWith("bn"))
                    dc = GetDs4ControlsByName(buttonName);
                else
                    dc = (DS4ControlItem)Enum.Parse(typeof(DS4ControlItem), buttonName, true);

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

            public ControlActionData GetDs4Action(int deviceNum, DS4ControlItem dc, bool shift)
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
                DS4ControlItem dc;
                if (buttonName.StartsWith("bn"))
                    dc = GetDs4ControlsByName(buttonName);
                else
                    dc = (DS4ControlItem)Enum.Parse(typeof(DS4ControlItem), buttonName, true);

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
                DS4ControlItem dc;
                if (buttonName.StartsWith("bn"))
                    dc = GetDs4ControlsByName(buttonName);
                else
                    dc = (DS4ControlItem)Enum.Parse(typeof(DS4ControlItem), buttonName, true);

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
                DS4ControlItem dc;
                if (buttonName.StartsWith("bn"))
                    dc = GetDs4ControlsByName(buttonName);
                else
                    dc = (DS4ControlItem)Enum.Parse(typeof(DS4ControlItem), buttonName, true);

                var temp = (int)dc;
                if (temp > 0)
                {
                    var index = temp - 1;
                    var dcs = Ds4Settings[deviceNum][index];
                    return dcs.ShiftTrigger;
                }

                return 0;
            }

            public int GetDs4STrigger(int deviceNum, DS4ControlItem dc)
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

            public DS4ControlSettingsV3 GetDs4ControllerSetting(int deviceNum, string buttonName)
            {
                DS4ControlItem dc;
                if (buttonName.StartsWith("bn"))
                    dc = GetDs4ControlsByName(buttonName);
                else
                    dc = (DS4ControlItem)Enum.Parse(typeof(DS4ControlItem), buttonName, true);

                var temp = (int)dc;
                if (temp > 0)
                {
                    var index = temp - 1;
                    var dcs = Ds4Settings[deviceNum][index];
                    return dcs;
                }

                return null;
            }

            public DS4ControlSettingsV3 GetDs4ControllerSetting(int deviceNum, DS4ControlItem dc)
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
                    if (dcs.ControlActionType != DS4ControlSettingsV3.ActionType.Default ||
                        dcs.ShiftActionType != DS4ControlSettingsV3.ActionType.Default)
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

            [Obsolete]
            public void LoadBlankDs4Profile(int device, bool launchprogram, ControlService control,
                string propath = "", bool xinputChange = true, bool postLoad = true)
            {
                PrepareBlankingProfile(device, control, out var xinputPlug, out var xinputStatus, xinputChange);

                var lsInfo = ProfilesService.Instance.ActiveProfiles.ElementAt(device).LSModInfo;
                lsInfo.DeadZone = (int)(0.00 * 127);
                lsInfo.AntiDeadZone = 0;
                lsInfo.MaxZone = 100;

                var rsInfo = ProfilesService.Instance.ActiveProfiles.ElementAt(device).RSModInfo;
                rsInfo.DeadZone = (int)(0.00 * 127);
                rsInfo.AntiDeadZone = 0;
                rsInfo.MaxZone = 100;

                var l2Info = ProfilesService.Instance.ActiveProfiles.ElementAt(device).L2ModInfo;
                l2Info.DeadZone = (byte)(0.00 * 255);

                var r2Info = ProfilesService.Instance.ActiveProfiles.ElementAt(device).R2ModInfo;
                r2Info.DeadZone = (byte)(0.00 * 255);

                ProfilesService.Instance.ActiveProfiles.ElementAt(device).OutputDeviceType =
                    OutputDeviceType.DualShock4Controller;

                // If a device exists, make sure to transfer relevant profile device
                // options to device instance
                if (postLoad && device < MAX_DS4_CONTROLLER_COUNT)
                    PostLoadSnippet(device, control, xinputStatus, xinputPlug);
            }

            [Obsolete]
            public void LoadBlankProfile(int device, bool launchprogram, ControlService control,
                string propath = "", bool xinputChange = true, bool postLoad = true)
            {
                var xinputPlug = false;
                var xinputStatus = false;

                var oldContType = ActiveOutDevType[device];

                // Make sure to reset currently set profile values before parsing
                ResetProfile(device);
                ResetMouseProperties(device, control);

                /*
                // Only change xinput devices under certain conditions. Avoid
                // performing this upon program startup before loading devices.
                if (xinputChange)
                    CheckOldDeviceStatus(device, control, oldContType,
                        out xinputPlug, out xinputStatus);
                */

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

            [Obsolete]
            public void LoadDefaultGamepadGyroProfile(int device, bool launchprogram, ControlService control,
                string propath = "", bool xinputChange = true, bool postLoad = true)
            {
                var xinputPlug = false;
                var xinputStatus = false;

                var oldContType = ActiveOutDevType[device];

                // Make sure to reset currently set profile values before parsing
                ResetProfile(device);
                ResetMouseProperties(device, control);

                /*
                // Only change xinput devices under certain conditions. Avoid
                // performing this upon program startup before loading devices.
                if (xinputChange)
                    CheckOldDeviceStatus(device, control, oldContType,
                        out xinputPlug, out xinputStatus);
                */

                foreach (var dcs in Ds4Settings[device])
                    dcs.Reset();

                ProfileActions[device].Clear();
                ContainsCustomAction[device] = false;
                ContainsCustomExtras[device] = false;

                ProfilesService.Instance.ActiveProfiles.ElementAt(device).GyroOutputMode = GyroOutMode.MouseJoystick;
                SAMouseStickTriggers[device] = "4";
                //SAMouseStickTriggerCond[device] = true;
                //GyroMouseStickTriggerTurns[device] = false;
                //GyroMouseStickInfo[device].UseSmoothing = true;
                //GyroMouseStickInfo[device].Smoothing = DS4Windows.GyroMouseStickInfo.SmoothingMethod.OneEuro;

                // If a device exists, make sure to transfer relevant profile device
                // options to device instance
                if (postLoad && device < MAX_DS4_CONTROLLER_COUNT)
                    PostLoadSnippet(device, control, xinputStatus, xinputPlug);
            }

            [Obsolete]
            public void LoadDefaultDS4GamepadGyroProfile(int device, bool launchprogram, ControlService control,
                string propath = "", bool xinputChange = true, bool postLoad = true)
            {
                PrepareBlankingProfile(device, control, out var xinputPlug, out var xinputStatus, xinputChange);

                var lsInfo = ProfilesService.Instance.ActiveProfiles.ElementAt(device).LSModInfo;
                lsInfo.DeadZone = (int)(0.00 * 127);
                lsInfo.AntiDeadZone = 0;
                lsInfo.MaxZone = 100;

                var rsInfo = ProfilesService.Instance.ActiveProfiles.ElementAt(device).RSModInfo;
                rsInfo.DeadZone = (int)(0.00 * 127);
                rsInfo.AntiDeadZone = 0;
                rsInfo.MaxZone = 100;

                var l2Info = ProfilesService.Instance.ActiveProfiles.ElementAt(device).L2ModInfo;
                l2Info.DeadZone = (byte)(0.00 * 255);

                var r2Info = ProfilesService.Instance.ActiveProfiles.ElementAt(device).R2ModInfo;
                r2Info.DeadZone = (byte)(0.00 * 255);

                ProfilesService.Instance.ActiveProfiles.ElementAt(device).GyroOutputMode = GyroOutMode.MouseJoystick;
                SAMouseStickTriggers[device] = "4";
                //SAMouseStickTriggerCond[device] = true;
                //GyroMouseStickTriggerTurns[device] = false;
                //GyroMouseStickInfo[device].UseSmoothing = true;
                //GyroMouseStickInfo[device].Smoothing = DS4Windows.GyroMouseStickInfo.SmoothingMethod.OneEuro;

                ProfilesService.Instance.ActiveProfiles.ElementAt(device).OutputDeviceType =
                    OutputDeviceType.DualShock4Controller;

                // If a device exists, make sure to transfer relevant profile device
                // options to device instance
                if (postLoad && device < MAX_DS4_CONTROLLER_COUNT)
                    PostLoadSnippet(device, control, xinputStatus, xinputPlug);
            }

            [Obsolete]
            public void LoadDefaultMixedGyroMouseProfile(int device, bool launchprogram, ControlService control,
                string propath = "", bool xinputChange = true, bool postLoad = true)
            {
                var xinputPlug = false;
                var xinputStatus = false;

                var oldContType = ActiveOutDevType[device];

                // Make sure to reset currently set profile values before parsing
                ResetProfile(device);
                ResetMouseProperties(device, control);

                /*
                // Only change xinput devices under certain conditions. Avoid
                // performing this upon program startup before loading devices.
                if (xinputChange)
                    CheckOldDeviceStatus(device, control, oldContType,
                        out xinputPlug, out xinputStatus);
                */

                foreach (var dcs in Ds4Settings[device])
                    dcs.Reset();

                ProfileActions[device].Clear();
                ContainsCustomAction[device] = false;
                ContainsCustomExtras[device] = false;

                ProfilesService.Instance.ActiveProfiles.ElementAt(device).GyroOutputMode = GyroOutMode.Mouse;
                //SATriggers[device] = "4";
                SATriggerCondition[device] = true;
                //GyroTriggerTurns[device] = false;
                ProfilesService.Instance.ActiveProfiles.ElementAt(device).GyroMouseInfo.EnableSmoothing = true;
                ProfilesService.Instance.ActiveProfiles.ElementAt(device).GyroMouseInfo.Smoothing =
                    GyroMouseInfo.SmoothingMethod.OneEuro;

                var rsInfo = ProfilesService.Instance.ActiveProfiles.ElementAt(device).RSModInfo;
                rsInfo.DeadZone = (int)(0.10 * 127);
                rsInfo.AntiDeadZone = 0;
                rsInfo.MaxZone = 90;

                // If a device exists, make sure to transfer relevant profile device
                // options to device instance
                if (postLoad && device < MAX_DS4_CONTROLLER_COUNT)
                    PostLoadSnippet(device, control, xinputStatus, xinputPlug);
            }

            [Obsolete]
            public void LoadDefaultDs4MixedGyroMouseProfile(int device, bool launchprogram, ControlService control,
                string propath = "", bool xinputChange = true, bool postLoad = true)
            {
                PrepareBlankingProfile(device, control, out var xinputPlug, out var xinputStatus, xinputChange);

                var lsInfo = ProfilesService.Instance.ActiveProfiles.ElementAt(device).LSModInfo;
                lsInfo.DeadZone = (int)(0.00 * 127);
                lsInfo.AntiDeadZone = 0;
                lsInfo.MaxZone = 100;

                var rsInfo = ProfilesService.Instance.ActiveProfiles.ElementAt(device).RSModInfo;
                rsInfo.DeadZone = (int)(0.10 * 127);
                rsInfo.AntiDeadZone = 0;
                rsInfo.MaxZone = 100;

                var l2Info = ProfilesService.Instance.ActiveProfiles.ElementAt(device).L2ModInfo;
                l2Info.DeadZone = (byte)(0.00 * 255);

                var r2Info = ProfilesService.Instance.ActiveProfiles.ElementAt(device).R2ModInfo;
                r2Info.DeadZone = (byte)(0.00 * 255);

                ProfilesService.Instance.ActiveProfiles.ElementAt(device).GyroOutputMode = GyroOutMode.Mouse;
                //SATriggers[device] = "4";
                SATriggerCondition[device] = true;
                //GyroTriggerTurns[device] = false;
                ProfilesService.Instance.ActiveProfiles.ElementAt(device).GyroMouseInfo.EnableSmoothing = true;
                ProfilesService.Instance.ActiveProfiles.ElementAt(device).GyroMouseInfo.Smoothing =
                    GyroMouseInfo.SmoothingMethod.OneEuro;

                ProfilesService.Instance.ActiveProfiles.ElementAt(device).OutputDeviceType =
                    OutputDeviceType.DualShock4Controller;

                // If a device exists, make sure to transfer relevant profile device
                // options to device instance
                if (postLoad && device < MAX_DS4_CONTROLLER_COUNT)
                    PostLoadSnippet(device, control, xinputStatus, xinputPlug);
            }

            [Obsolete]
            public void LoadDefaultDS4MixedControlsProfile(int device, bool launchprogram, ControlService control,
                string propath = "", bool xinputChange = true, bool postLoad = true)
            {
                PrepareBlankingProfile(device, control, out var xinputPlug, out var xinputStatus, xinputChange);

                var setting = GetDs4ControllerSetting(device, DS4ControlItem.RYNeg);
                setting.UpdateSettings(false, X360ControlItem.MouseUp, "", DS4KeyType.None);
                setting = GetDs4ControllerSetting(device, DS4ControlItem.RYPos);
                setting.UpdateSettings(false, X360ControlItem.MouseDown, "", DS4KeyType.None);
                setting = GetDs4ControllerSetting(device, DS4ControlItem.RXNeg);
                setting.UpdateSettings(false, X360ControlItem.MouseLeft, "", DS4KeyType.None);
                setting = GetDs4ControllerSetting(device, DS4ControlItem.RXPos);
                setting.UpdateSettings(false, X360ControlItem.MouseRight, "", DS4KeyType.None);

                var rsInfo = ProfilesService.Instance.ActiveProfiles.ElementAt(device).RSModInfo;
                rsInfo.DeadZone = (int)(0.035 * 127);
                rsInfo.AntiDeadZone = 0;
                rsInfo.MaxZone = 90;

                ProfilesService.Instance.ActiveProfiles.ElementAt(device).OutputDeviceType =
                    OutputDeviceType.DualShock4Controller;

                // If a device exists, make sure to transfer relevant profile device
                // options to device instance
                if (postLoad && device < MAX_DS4_CONTROLLER_COUNT)
                    PostLoadSnippet(device, control, xinputStatus, xinputPlug);
            }

            [Obsolete]
            public void LoadDefaultMixedControlsProfile(int device, bool launchprogram, ControlService control,
                string propath = "", bool xinputChange = true, bool postLoad = true)
            {
                var xinputPlug = false;
                var xinputStatus = false;

                var oldContType = ActiveOutDevType[device];

                // Make sure to reset currently set profile values before parsing
                ResetProfile(device);
                ResetMouseProperties(device, control);

                /*
                // Only change xinput devices under certain conditions. Avoid
                // performing this upon program startup before loading devices.
                if (xinputChange)
                    CheckOldDeviceStatus(device, control, oldContType,
                        out xinputPlug, out xinputStatus);
                */

                foreach (var dcs in Ds4Settings[device])
                    dcs.Reset();

                ProfileActions[device].Clear();
                ContainsCustomAction[device] = false;
                ContainsCustomExtras[device] = false;

                var setting = GetDs4ControllerSetting(device, DS4ControlItem.RYNeg);
                setting.UpdateSettings(false, X360ControlItem.MouseUp, "", DS4KeyType.None);
                setting = GetDs4ControllerSetting(device, DS4ControlItem.RYPos);
                setting.UpdateSettings(false, X360ControlItem.MouseDown, "", DS4KeyType.None);
                setting = GetDs4ControllerSetting(device, DS4ControlItem.RXNeg);
                setting.UpdateSettings(false, X360ControlItem.MouseLeft, "", DS4KeyType.None);
                setting = GetDs4ControllerSetting(device, DS4ControlItem.RXPos);
                setting.UpdateSettings(false, X360ControlItem.MouseRight, "", DS4KeyType.None);

                var rsInfo = ProfilesService.Instance.ActiveProfiles.ElementAt(device).RSModInfo;
                rsInfo.DeadZone = (int)(0.035 * 127);
                rsInfo.AntiDeadZone = 0;
                rsInfo.MaxZone = 90;

                // If a device exists, make sure to transfer relevant profile device
                // options to device instance
                if (postLoad && device < MAX_DS4_CONTROLLER_COUNT)
                    PostLoadSnippet(device, control, xinputStatus, xinputPlug);
            }

            [Obsolete]
            public void LoadDefaultKBMProfile(int device, bool launchprogram, ControlService control,
                string propath = "", bool xinputChange = true, bool postLoad = true)
            {
                var xinputPlug = false;
                var xinputStatus = false;

                var oldContType = ActiveOutDevType[device];

                // Make sure to reset currently set profile values before parsing
                ResetProfile(device);
                ResetMouseProperties(device, control);

                /*
                // Only change xinput devices under certain conditions. Avoid
                // performing this upon program startup before loading devices.
                if (xinputChange)
                    CheckOldDeviceStatus(device, control, oldContType,
                        out xinputPlug, out xinputStatus);
                */

                foreach (var dcs in Ds4Settings[device])
                    dcs.Reset();

                ProfileActions[device].Clear();
                ContainsCustomAction[device] = false;
                ContainsCustomExtras[device] = false;

                var lsInfo = ProfilesService.Instance.ActiveProfiles.ElementAt(device).LSModInfo;
                lsInfo.AntiDeadZone = 0;

                var rsInfo = ProfilesService.Instance.ActiveProfiles.ElementAt(device).RSModInfo;
                rsInfo.DeadZone = (int)(0.035 * 127);
                rsInfo.AntiDeadZone = 0;
                rsInfo.MaxZone = 90;

                var l2Info = ProfilesService.Instance.ActiveProfiles.ElementAt(device).L2ModInfo;
                l2Info.DeadZone = (byte)(0.20 * 255);

                var r2Info = ProfilesService.Instance.ActiveProfiles.ElementAt(device).R2ModInfo;
                r2Info.DeadZone = (byte)(0.20 * 255);

                // Flag to unplug virtual controller
                //DIOnly[device] = true;

                var setting = GetDs4ControllerSetting(device, DS4ControlItem.LYNeg);
                setting.UpdateSettings(false, KeyInterop.VirtualKeyFromKey(Key.W), "", DS4KeyType.None);
                setting = GetDs4ControllerSetting(device, DS4ControlItem.LXNeg);
                setting.UpdateSettings(false, KeyInterop.VirtualKeyFromKey(Key.A), "", DS4KeyType.None);
                setting = GetDs4ControllerSetting(device, DS4ControlItem.LYPos);
                setting.UpdateSettings(false, KeyInterop.VirtualKeyFromKey(Key.S), "", DS4KeyType.None);
                setting = GetDs4ControllerSetting(device, DS4ControlItem.LXPos);
                setting.UpdateSettings(false, KeyInterop.VirtualKeyFromKey(Key.D), "", DS4KeyType.None);
                setting = GetDs4ControllerSetting(device, DS4ControlItem.L3);
                setting.UpdateSettings(false, KeyInterop.VirtualKeyFromKey(Key.LeftShift), "", DS4KeyType.None);

                setting = GetDs4ControllerSetting(device, DS4ControlItem.RYNeg);
                setting.UpdateSettings(false, X360ControlItem.MouseUp, "", DS4KeyType.None);
                setting = GetDs4ControllerSetting(device, DS4ControlItem.RYPos);
                setting.UpdateSettings(false, X360ControlItem.MouseDown, "", DS4KeyType.None);
                setting = GetDs4ControllerSetting(device, DS4ControlItem.RXNeg);
                setting.UpdateSettings(false, X360ControlItem.MouseLeft, "", DS4KeyType.None);
                setting = GetDs4ControllerSetting(device, DS4ControlItem.RXPos);
                setting.UpdateSettings(false, X360ControlItem.MouseRight, "", DS4KeyType.None);
                setting = GetDs4ControllerSetting(device, DS4ControlItem.R3);
                setting.UpdateSettings(false, KeyInterop.VirtualKeyFromKey(Key.LeftCtrl), "", DS4KeyType.None);

                setting = GetDs4ControllerSetting(device, DS4ControlItem.DpadUp);
                setting.UpdateSettings(false, X360ControlItem.Unbound, "", DS4KeyType.None);
                setting = GetDs4ControllerSetting(device, DS4ControlItem.DpadRight);
                setting.UpdateSettings(false, X360ControlItem.WDOWN, "", DS4KeyType.None);
                setting = GetDs4ControllerSetting(device, DS4ControlItem.DpadDown);
                setting.UpdateSettings(false, X360ControlItem.Unbound, "", DS4KeyType.None);
                setting = GetDs4ControllerSetting(device, DS4ControlItem.DpadLeft);
                setting.UpdateSettings(false, X360ControlItem.WUP, "", DS4KeyType.None);

                setting = GetDs4ControllerSetting(device, DS4ControlItem.Cross);
                setting.UpdateSettings(false, KeyInterop.VirtualKeyFromKey(Key.Space), "", DS4KeyType.None);
                setting = GetDs4ControllerSetting(device, DS4ControlItem.Square);
                setting.UpdateSettings(false, KeyInterop.VirtualKeyFromKey(Key.F), "", DS4KeyType.None);
                setting = GetDs4ControllerSetting(device, DS4ControlItem.Triangle);
                setting.UpdateSettings(false, KeyInterop.VirtualKeyFromKey(Key.E), "", DS4KeyType.None);
                setting = GetDs4ControllerSetting(device, DS4ControlItem.Circle);
                setting.UpdateSettings(false, KeyInterop.VirtualKeyFromKey(Key.C), "", DS4KeyType.None);

                setting = GetDs4ControllerSetting(device, DS4ControlItem.L1);
                setting.UpdateSettings(false, KeyInterop.VirtualKeyFromKey(Key.Q), "", DS4KeyType.None);
                setting = GetDs4ControllerSetting(device, DS4ControlItem.L2);
                setting.UpdateSettings(false, X360ControlItem.RightMouse, "", DS4KeyType.None);
                setting = GetDs4ControllerSetting(device, DS4ControlItem.R1);
                setting.UpdateSettings(false, KeyInterop.VirtualKeyFromKey(Key.R), "", DS4KeyType.None);
                setting = GetDs4ControllerSetting(device, DS4ControlItem.R2);
                setting.UpdateSettings(false, X360ControlItem.LeftMouse, "", DS4KeyType.None);

                setting = GetDs4ControllerSetting(device, DS4ControlItem.Share);
                setting.UpdateSettings(false, KeyInterop.VirtualKeyFromKey(Key.Tab), "", DS4KeyType.None);
                setting = GetDs4ControllerSetting(device, DS4ControlItem.Options);
                setting.UpdateSettings(false, KeyInterop.VirtualKeyFromKey(Key.Escape), "", DS4KeyType.None);

                // If a device exists, make sure to transfer relevant profile device
                // options to device instance
                if (postLoad && device < MAX_DS4_CONTROLLER_COUNT)
                    PostLoadSnippet(device, control, xinputStatus, xinputPlug);
            }

            [Obsolete]
            public void LoadDefaultKBMGyroMouseProfile(int device, bool launchprogram, ControlService control,
                string propath = "", bool xinputChange = true, bool postLoad = true)
            {
                var xinputPlug = false;
                var xinputStatus = false;

                var oldContType = ActiveOutDevType[device];

                // Make sure to reset currently set profile values before parsing
                ResetProfile(device);
                ResetMouseProperties(device, control);

                /*
                // Only change xinput devices under certain conditions. Avoid
                // performing this upon program startup before loading devices.
                if (xinputChange)
                    CheckOldDeviceStatus(device, control, oldContType,
                        out xinputPlug, out xinputStatus);
                */

                foreach (var dcs in Ds4Settings[device])
                    dcs.Reset();

                ProfileActions[device].Clear();
                ContainsCustomAction[device] = false;
                ContainsCustomExtras[device] = false;

                var lsInfo = ProfilesService.Instance.ActiveProfiles.ElementAt(device).LSModInfo;
                lsInfo.AntiDeadZone = 0;

                var rsInfo = ProfilesService.Instance.ActiveProfiles.ElementAt(device).RSModInfo;
                rsInfo.DeadZone = (int)(0.105 * 127);
                rsInfo.AntiDeadZone = 0;
                rsInfo.MaxZone = 90;

                var l2Info = ProfilesService.Instance.ActiveProfiles.ElementAt(device).L2ModInfo;
                l2Info.DeadZone = (byte)(0.20 * 255);

                var r2Info = ProfilesService.Instance.ActiveProfiles.ElementAt(device).R2ModInfo;
                r2Info.DeadZone = (byte)(0.20 * 255);

                ProfilesService.Instance.ActiveProfiles.ElementAt(device).GyroOutputMode = GyroOutMode.Mouse;
                //SATriggers[device] = "4";
                SATriggerCondition[device] = true;
                //GyroTriggerTurns[device] = false;
                ProfilesService.Instance.ActiveProfiles.ElementAt(device).GyroMouseInfo.EnableSmoothing = true;
                ProfilesService.Instance.ActiveProfiles.ElementAt(device).GyroMouseInfo.Smoothing =
                    GyroMouseInfo.SmoothingMethod.OneEuro;

                // Flag to unplug virtual controller
                //DIOnly[device] = true;

                var setting = GetDs4ControllerSetting(device, DS4ControlItem.LYNeg);
                setting.UpdateSettings(false, KeyInterop.VirtualKeyFromKey(Key.W), "", DS4KeyType.None);
                setting = GetDs4ControllerSetting(device, DS4ControlItem.LXNeg);
                setting.UpdateSettings(false, KeyInterop.VirtualKeyFromKey(Key.A), "", DS4KeyType.None);
                setting = GetDs4ControllerSetting(device, DS4ControlItem.LYPos);
                setting.UpdateSettings(false, KeyInterop.VirtualKeyFromKey(Key.S), "", DS4KeyType.None);
                setting = GetDs4ControllerSetting(device, DS4ControlItem.LXPos);
                setting.UpdateSettings(false, KeyInterop.VirtualKeyFromKey(Key.D), "", DS4KeyType.None);
                setting = GetDs4ControllerSetting(device, DS4ControlItem.L3);
                setting.UpdateSettings(false, KeyInterop.VirtualKeyFromKey(Key.LeftShift), "", DS4KeyType.None);

                setting = GetDs4ControllerSetting(device, DS4ControlItem.RYNeg);
                setting.UpdateSettings(false, X360ControlItem.MouseUp, "", DS4KeyType.None);
                setting = GetDs4ControllerSetting(device, DS4ControlItem.RYPos);
                setting.UpdateSettings(false, X360ControlItem.MouseDown, "", DS4KeyType.None);
                setting = GetDs4ControllerSetting(device, DS4ControlItem.RXNeg);
                setting.UpdateSettings(false, X360ControlItem.MouseLeft, "", DS4KeyType.None);
                setting = GetDs4ControllerSetting(device, DS4ControlItem.RXPos);
                setting.UpdateSettings(false, X360ControlItem.MouseRight, "", DS4KeyType.None);
                setting = GetDs4ControllerSetting(device, DS4ControlItem.R3);
                setting.UpdateSettings(false, KeyInterop.VirtualKeyFromKey(Key.LeftCtrl), "", DS4KeyType.None);

                setting = GetDs4ControllerSetting(device, DS4ControlItem.DpadUp);
                setting.UpdateSettings(false, X360ControlItem.Unbound, "", DS4KeyType.None);
                setting = GetDs4ControllerSetting(device, DS4ControlItem.DpadRight);
                setting.UpdateSettings(false, X360ControlItem.WDOWN, "", DS4KeyType.None);
                setting = GetDs4ControllerSetting(device, DS4ControlItem.DpadDown);
                setting.UpdateSettings(false, X360ControlItem.Unbound, "", DS4KeyType.None);
                setting = GetDs4ControllerSetting(device, DS4ControlItem.DpadLeft);
                setting.UpdateSettings(false, X360ControlItem.WUP, "", DS4KeyType.None);

                setting = GetDs4ControllerSetting(device, DS4ControlItem.Cross);
                setting.UpdateSettings(false, KeyInterop.VirtualKeyFromKey(Key.Space), "", DS4KeyType.None);
                setting = GetDs4ControllerSetting(device, DS4ControlItem.Square);
                setting.UpdateSettings(false, KeyInterop.VirtualKeyFromKey(Key.F), "", DS4KeyType.None);
                setting = GetDs4ControllerSetting(device, DS4ControlItem.Triangle);
                setting.UpdateSettings(false, KeyInterop.VirtualKeyFromKey(Key.E), "", DS4KeyType.None);
                setting = GetDs4ControllerSetting(device, DS4ControlItem.Circle);
                setting.UpdateSettings(false, KeyInterop.VirtualKeyFromKey(Key.C), "", DS4KeyType.None);

                setting = GetDs4ControllerSetting(device, DS4ControlItem.L1);
                //setting.UpdateSettings(false, KeyInterop.VirtualKeyFromKey(Key.Q), "", DS4KeyType.None);
                setting.UpdateSettings(false, X360ControlItem.Unbound, "", DS4KeyType.None);
                setting = GetDs4ControllerSetting(device, DS4ControlItem.L2);
                setting.UpdateSettings(false, X360ControlItem.RightMouse, "", DS4KeyType.None);
                setting = GetDs4ControllerSetting(device, DS4ControlItem.R1);
                setting.UpdateSettings(false, KeyInterop.VirtualKeyFromKey(Key.R), "", DS4KeyType.None);
                setting = GetDs4ControllerSetting(device, DS4ControlItem.R2);
                setting.UpdateSettings(false, X360ControlItem.LeftMouse, "", DS4KeyType.None);

                setting = GetDs4ControllerSetting(device, DS4ControlItem.Share);
                setting.UpdateSettings(false, KeyInterop.VirtualKeyFromKey(Key.Tab), "", DS4KeyType.None);
                setting = GetDs4ControllerSetting(device, DS4ControlItem.Options);
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

            public void EstablishDefaultSpecialActions(int idx)
            {
                ProfileActions[idx] = new List<string> { "Disconnect Controller" };
                profileActionCount[idx] = ProfileActions[idx].Count;
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

            /// <summary>
            ///     Persists <see cref="DS4WindowsAppSettingsV3" /> on disk.
            /// </summary>
            /// <returns>True on success, false otherwise.</returns>
            [ConfigurationSystemComponent]
            [Obsolete]
            public bool SaveApplicationSettings()
            {
                var saved = true;

                var settings = new DS4WindowsAppSettingsV3(this, ExecutableProductVersion, APP_CONFIG_VERSION);

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
            ///     Restores <see cref="DS4WindowsAppSettingsV3" /> from disk.
            /// </summary>
            /// <returns>True on success, false otherwise.</returns>
            [ConfigurationSystemComponent]
            [Obsolete]
            public async Task<bool> LoadApplicationSettings()
            {
                var loaded = true;

                if (File.Exists(ProfilesPath))
                {
                    await using (var stream = File.OpenRead(ProfilesPath))
                    {
                        (await DS4WindowsAppSettingsV3.DeserializeAsync(stream)).CopyTo(this);
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

            private int AxisOutputCurveId(string name)
            {
                return StickOutputCurveId(name);
            }

            private string OutContDeviceString(OutputDeviceType id)
            {
                var result = "X360";
                switch (id)
                {
                    case OutputDeviceType.None:
                    case OutputDeviceType.Xbox360Controller:
                        result = "X360";
                        break;
                    case OutputDeviceType.DualShock4Controller:
                        result = "DS4";
                        break;
                }

                return result;
            }

            private OutputDeviceType OutContDeviceId(string name)
            {
                var id = OutputDeviceType.Xbox360Controller;
                switch (name)
                {
                    case "None":
                    case "X360":
                        id = OutputDeviceType.Xbox360Controller;
                        break;
                    case "DS4":
                        id = OutputDeviceType.DualShock4Controller;
                        break;
                }

                return id;
            }

            private void PortOldGyroSettings(int device)
            {
                if (ProfilesService.Instance.ActiveProfiles.ElementAt(device).GyroOutputMode == GyroOutMode.None)
                    ProfilesService.Instance.ActiveProfiles.ElementAt(device).GyroOutputMode = GyroOutMode.Controls;
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

            [Obsolete]
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
                DS4ControlItem dc;
                if (buttonName.StartsWith("bn"))
                    dc = GetDs4ControlsByName(buttonName);
                else
                    dc = (DS4ControlItem)Enum.Parse(typeof(DS4ControlItem), buttonName, true);

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

            [Obsolete]
            private void ResetProfile(int device)
            {
                //IdleDisconnectTimeout[device] = 0;

                ProfilesService.Instance.ActiveProfiles.ElementAt(device).LSModInfo.Reset();
                ProfilesService.Instance.ActiveProfiles.ElementAt(device).RSModInfo.Reset();
                ProfilesService.Instance.ActiveProfiles.ElementAt(device).LSModInfo.DeadZone =
                    ProfilesService.Instance.ActiveProfiles.ElementAt(device).RSModInfo.DeadZone = 10;
                ProfilesService.Instance.ActiveProfiles.ElementAt(device).LSModInfo.AntiDeadZone =
                    ProfilesService.Instance.ActiveProfiles.ElementAt(device).RSModInfo.AntiDeadZone = 20;
                ProfilesService.Instance.ActiveProfiles.ElementAt(device).LSModInfo.MaxZone =
                    ProfilesService.Instance.ActiveProfiles.ElementAt(device).RSModInfo.MaxZone = 100;
                ProfilesService.Instance.ActiveProfiles.ElementAt(device).LSModInfo.MaxOutput =
                    ProfilesService.Instance.ActiveProfiles.ElementAt(device).RSModInfo.MaxOutput = 100.0;
                ProfilesService.Instance.ActiveProfiles.ElementAt(device).LSModInfo.Fuzz =
                    ProfilesService.Instance.ActiveProfiles.ElementAt(device).RSModInfo.Fuzz =
                        StickDeadZoneInfo.DefaultFuzz;

                //l2ModInfo[device].deadZone = r2ModInfo[device].deadZone = 0;
                //l2ModInfo[device].antiDeadZone = r2ModInfo[device].antiDeadZone = 0;
                //l2ModInfo[device].maxZone = r2ModInfo[device].maxZone = 100;
                //l2ModInfo[device].maxOutput = r2ModInfo[device].maxOutput = 100.0;
                ProfilesService.Instance.ActiveProfiles.ElementAt(device).L2ModInfo.Reset();
                ProfilesService.Instance.ActiveProfiles.ElementAt(device).R2ModInfo.Reset();

                //TapSensitivity[device] = 0;
                //ScrollSensitivity[device] = 0;
                //TouchPadInvert[device] = 0;
                //BluetoothPollRate[device] = 4;

                /*
                LSOutputSettings[device].ResetSettings();
                RSOutputSettings[device].ResetSettings();
                L2OutputSettings[device].ResetSettings();
                R2OutputSettings[device].ResetSettings();
                */

                LaunchProgram[device] = string.Empty;
                ProfilesService.Instance.ActiveProfiles.ElementAt(device).TouchOutMode = TouchpadOutMode.Mouse;
                //SATriggers[device] = "-1";
                SATriggerCondition[device] = true;
                ProfilesService.Instance.ActiveProfiles.ElementAt(device).GyroOutputMode = GyroOutMode.Controls;
                SAMouseStickTriggers[device] = "-1";
                //SAMouseStickTriggerCond[device] = true;

                //GyroMouseStickInfo[device].Reset();
                //GyroSwipeInfo[device].Reset();

                //GyroMouseStickTriggerTurns[device] = true;
                //SASteeringWheelEmulationAxis[device] = SASteeringWheelEmulationAxisType.None;
                //SASteeringWheelEmulationRange[device] = 360;
                //SAWheelFuzzValues[device] = 0;
                //WheelSmoothInfo[device].Reset();
                TouchDisInvertTriggers[device] = new int[1] { -1 };
                //GyroSensitivity[device] = 100;
                //GyroSensVerticalScale[device] = 100;
                //GyroInvert[device] = 0;
                //GyroTriggerTurns[device] = true;
                //GyroMouseInfo[device].Reset();

                //GyroMouseHorizontalAxis[device] = 0;
                //GyroMouseToggle[device] = false;
                //SquStickInfo[device].LSMode = false;
                //SquStickInfo[device].RSMode = false;
                //SquStickInfo[device].LSRoundness = 5.0;
                //SquStickInfo[device].RSRoundness = 5.0;
                //LSAntiSnapbackInfo[device].Timeout = StickAntiSnapbackInfo.DefaultTimeout;
                //LSAntiSnapbackInfo[device].Delta = StickAntiSnapbackInfo.DefaultDelta;
                //LSAntiSnapbackInfo[device].Enabled = StickAntiSnapbackInfo.DefaultEnabled;
                SetLsOutCurveMode(device, 0);
                SetRsOutCurveMode(device, 0);
                SetL2OutCurveMode(device, 0);
                SetR2OutCurveMode(device, 0);
                SetSXOutCurveMode(device, 0);
                SetSZOutCurveMode(device, 0);
                //TrackballMode[device] = false;
                //TrackballFriction[device] = 10.0;
                //TouchPadAbsMouse[device].Reset();
                //TouchPadRelMouse[device].Reset();
                ProfilesService.Instance.ActiveProfiles.ElementAt(device).OutputDeviceType =
                    OutputDeviceType.Xbox360Controller;
                Ds4Mapping = false;
            }

            [Obsolete]
            private void PrepareBlankingProfile(int device, ControlService control, out bool xinputPlug,
                out bool xinputStatus, bool xinputChange = true)
            {
                xinputPlug = false;
                xinputStatus = false;

                var oldContType = ActiveOutDevType[device];

                // Make sure to reset currently set profile values before parsing
                ResetProfile(device);
                ResetMouseProperties(device, control);

                /*
                // Only change xinput devices under certain conditions. Avoid
                // performing this upon program startup before loading devices.
                if (xinputChange)
                    CheckOldDeviceStatus(device, control, oldContType,
                        out xinputPlug, out xinputStatus);
                */

                foreach (var dcs in Ds4Settings[device])
                    dcs.Reset();

                ProfileActions[device].Clear();
                ContainsCustomAction[device] = false;
                ContainsCustomExtras[device] = false;
            }

            /*
            private void CheckOldDeviceStatus(
                int device, 
                ControlService control,
                OutContType oldContType, 
                out bool xinputPlug, 
                out bool xinputStatus
                )
            {
                xinputPlug = false;
                xinputStatus = false;

                if (device < ControlService.CURRENT_DS4_CONTROLLER_LIMIT)
                {
                    var oldUseDInputOnly = Global.DIOnly[device];
                    var tempDevice = control.DS4Controllers[device];
                    var exists = tempDevice != null;
                    var synced = exists ? tempDevice.IsSynced() : false;
                    var isAlive = exists ? tempDevice.IsAlive() : false;
                    if (DIOnly[device] != oldUseDInputOnly)
                    {
                        if (DIOnly[device])
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
                    else if (!DIOnly[device] &&
                             oldContType != OutputDeviceType[device])
                    {
                        xinputPlug = true;
                        xinputStatus = true;
                    }
                }
            }
            */

            private void PostLoadSnippet(int device, ControlService control, bool xinputStatus, bool xinputPlug)
            {
                var tempDev = control.DS4Controllers[device];
                if (tempDev != null && tempDev.IsSynced())
                    tempDev.QueueEvent(() =>
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

                                var tempContType = ProfilesService.Instance.ActiveProfiles.ElementAt(device)
                                    .OutputDeviceType;
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