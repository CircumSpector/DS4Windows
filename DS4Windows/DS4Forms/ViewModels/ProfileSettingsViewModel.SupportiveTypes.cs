using System.Collections.Generic;
using System.Windows.Input;
using DS4Windows;
using DS4Windows.InputDevices;
using DS4Windows.Shared.Common.Types;

namespace DS4WinWPF.DS4Forms.ViewModels
{
    public class PresetMenuHelper
    {
        public enum ControlSelection : uint
        {
            None,
            LeftStick,
            RightStick,
            DPad,
            FaceButtons
        }

        private readonly int deviceNum;

        public PresetMenuHelper(int device)
        {
            deviceNum = device;
        }

        public Dictionary<ControlSelection, string> PresetInputLabelDict { get; } = new()
        {
            [ControlSelection.None] = "None",
            [ControlSelection.DPad] = "DPad",
            [ControlSelection.LeftStick] = "Left Stick",
            [ControlSelection.RightStick] = "Right Stick",
            [ControlSelection.FaceButtons] = "Face Buttons"
        };

        public string PresetInputLabel => PresetInputLabelDict[HighlightControl];

        public ControlSelection HighlightControl { get; private set; } = ControlSelection.None;

        public ControlSelection PresetTagIndex(DS4ControlItem control)
        {
            var controlInput = ControlSelection.None;
            switch (control)
            {
                case DS4ControlItem.DpadUp:
                case DS4ControlItem.DpadDown:
                case DS4ControlItem.DpadLeft:
                case DS4ControlItem.DpadRight:
                    controlInput = ControlSelection.DPad;
                    break;
                case DS4ControlItem.LXNeg:
                case DS4ControlItem.LXPos:
                case DS4ControlItem.LYNeg:
                case DS4ControlItem.LYPos:
                case DS4ControlItem.L3:
                    controlInput = ControlSelection.LeftStick;
                    break;
                case DS4ControlItem.RXNeg:
                case DS4ControlItem.RXPos:
                case DS4ControlItem.RYNeg:
                case DS4ControlItem.RYPos:
                case DS4ControlItem.R3:
                    controlInput = ControlSelection.RightStick;
                    break;
                case DS4ControlItem.Cross:
                case DS4ControlItem.Circle:
                case DS4ControlItem.Triangle:
                case DS4ControlItem.Square:
                    controlInput = ControlSelection.FaceButtons;
                    break;
            }


            return controlInput;
        }

        public void SetHighlightControl(DS4ControlItem control)
        {
            var controlInput = PresetTagIndex(control);
            HighlightControl = controlInput;
        }

        public List<DS4ControlItem> ModifySettingWithPreset(int baseTag, int subTag)
        {
            var actionBtns = new List<object>(5);
            var inputControls = new List<DS4ControlItem>(5);
            if (baseTag == 0)
                actionBtns.AddRange(new object[5]
                {
                    null, null, null, null, null
                });
            else if (baseTag == 1)
                switch (subTag)
                {
                    case 0:
                        actionBtns.AddRange(new object[5]
                        {
                            X360Controls.DpadUp, X360Controls.DpadDown,
                            X360Controls.DpadLeft, X360Controls.DpadRight, X360Controls.Unbound
                        });
                        break;
                    case 1:
                        actionBtns.AddRange(new object[5]
                        {
                            X360Controls.DpadDown, X360Controls.DpadUp,
                            X360Controls.DpadRight, X360Controls.DpadLeft, X360Controls.Unbound
                        });
                        break;
                    case 2:
                        actionBtns.AddRange(new object[5]
                        {
                            X360Controls.DpadUp, X360Controls.DpadDown,
                            X360Controls.DpadRight, X360Controls.DpadLeft, X360Controls.Unbound
                        });
                        break;
                    case 3:
                        actionBtns.AddRange(new object[5]
                        {
                            X360Controls.DpadDown, X360Controls.DpadUp,
                            X360Controls.DpadLeft, X360Controls.DpadRight, X360Controls.Unbound
                        });
                        break;
                    case 4:
                        actionBtns.AddRange(new object[5]
                        {
                            X360Controls.DpadRight, X360Controls.DpadLeft,
                            X360Controls.DpadUp, X360Controls.DpadDown, X360Controls.Unbound
                        });
                        break;
                    case 5:
                        actionBtns.AddRange(new object[5]
                        {
                            X360Controls.DpadLeft, X360Controls.DpadRight,
                            X360Controls.DpadDown, X360Controls.DpadUp, X360Controls.Unbound
                        });
                        break;
                }
            else if (baseTag == 2)
                switch (subTag)
                {
                    case 0:
                        actionBtns.AddRange(new object[5]
                        {
                            X360Controls.LYNeg, X360Controls.LYPos,
                            X360Controls.LXNeg, X360Controls.LXPos, X360Controls.LS
                        });
                        break;
                    case 1:
                        actionBtns.AddRange(new object[5]
                        {
                            X360Controls.LYPos, X360Controls.LYNeg,
                            X360Controls.LXPos, X360Controls.LXNeg, X360Controls.LS
                        });
                        break;
                    case 2:
                        actionBtns.AddRange(new object[5]
                        {
                            X360Controls.LYNeg, X360Controls.LYPos,
                            X360Controls.LXPos, X360Controls.LXNeg, X360Controls.LS
                        });
                        break;
                    case 3:
                        actionBtns.AddRange(new object[5]
                        {
                            X360Controls.LYPos, X360Controls.LYNeg,
                            X360Controls.LXNeg, X360Controls.LXPos, X360Controls.LS
                        });
                        break;
                    case 4:
                        actionBtns.AddRange(new object[5]
                        {
                            X360Controls.LXPos, X360Controls.LXNeg,
                            X360Controls.LYNeg, X360Controls.LYPos, X360Controls.LS
                        });
                        break;
                    case 5:
                        actionBtns.AddRange(new object[5]
                        {
                            X360Controls.LXNeg, X360Controls.LXPos,
                            X360Controls.LYPos, X360Controls.LYNeg, X360Controls.LS
                        });
                        break;
                }
            else if (baseTag == 3)
                switch (subTag)
                {
                    case 0:
                        actionBtns.AddRange(new object[5]
                        {
                            X360Controls.RYNeg, X360Controls.RYPos,
                            X360Controls.RXNeg, X360Controls.RXPos, X360Controls.RS
                        });
                        break;
                    case 1:
                        actionBtns.AddRange(new object[5]
                        {
                            X360Controls.RYPos, X360Controls.RYNeg,
                            X360Controls.RXPos, X360Controls.RXNeg, X360Controls.RS
                        });
                        break;
                    case 2:
                        actionBtns.AddRange(new object[5]
                        {
                            X360Controls.RYNeg, X360Controls.RYPos,
                            X360Controls.RXPos, X360Controls.RXNeg, X360Controls.RS
                        });
                        break;
                    case 3:
                        actionBtns.AddRange(new object[5]
                        {
                            X360Controls.RYPos, X360Controls.RYNeg,
                            X360Controls.RXNeg, X360Controls.RXPos, X360Controls.RS
                        });
                        break;
                    case 4:
                        actionBtns.AddRange(new object[5]
                        {
                            X360Controls.RXPos, X360Controls.RXNeg,
                            X360Controls.RYNeg, X360Controls.RYPos, X360Controls.RS
                        });
                        break;
                    case 5:
                        actionBtns.AddRange(new object[5]
                        {
                            X360Controls.RXNeg, X360Controls.RXPos,
                            X360Controls.RYPos, X360Controls.RYNeg, X360Controls.RS
                        });
                        break;
                }
            else if (baseTag == 4)
                switch (subTag)
                {
                    case 0:
                        // North, South, West, East
                        actionBtns.AddRange(new object[5]
                        {
                            X360Controls.Y, X360Controls.A, X360Controls.X, X360Controls.B, X360Controls.Unbound
                        });
                        break;
                    case 1:
                        actionBtns.AddRange(new object[5]
                        {
                            X360Controls.B, X360Controls.X, X360Controls.Y, X360Controls.A, X360Controls.Unbound
                        });
                        break;
                    case 2:
                        actionBtns.AddRange(new object[5]
                        {
                            X360Controls.X, X360Controls.B, X360Controls.A, X360Controls.Y, X360Controls.Unbound
                        });
                        break;
                }
            else if (baseTag == 5)
                switch (subTag)
                {
                    case 0:
                        // North, South, West, East
                        actionBtns.AddRange(new object[5]
                        {
                            KeyInterop.VirtualKeyFromKey(Key.W), KeyInterop.VirtualKeyFromKey(Key.S),
                            KeyInterop.VirtualKeyFromKey(Key.A), KeyInterop.VirtualKeyFromKey(Key.D),
                            X360Controls.Unbound
                        });
                        break;
                    case 1:
                        actionBtns.AddRange(new object[5]
                        {
                            KeyInterop.VirtualKeyFromKey(Key.D), KeyInterop.VirtualKeyFromKey(Key.A),
                            KeyInterop.VirtualKeyFromKey(Key.W), KeyInterop.VirtualKeyFromKey(Key.S),
                            X360Controls.Unbound
                        });
                        break;
                    case 2:
                        actionBtns.AddRange(new object[5]
                        {
                            KeyInterop.VirtualKeyFromKey(Key.A), KeyInterop.VirtualKeyFromKey(Key.D),
                            KeyInterop.VirtualKeyFromKey(Key.S), KeyInterop.VirtualKeyFromKey(Key.W),
                            X360Controls.Unbound
                        });
                        break;
                }
            else if (baseTag == 6)
                switch (subTag)
                {
                    case 0:
                        // North, South, West, East
                        actionBtns.AddRange(new object[5]
                        {
                            KeyInterop.VirtualKeyFromKey(Key.Up), KeyInterop.VirtualKeyFromKey(Key.Down),
                            KeyInterop.VirtualKeyFromKey(Key.Left), KeyInterop.VirtualKeyFromKey(Key.Right),
                            X360Controls.Unbound
                        });
                        break;
                    case 1:
                        actionBtns.AddRange(new object[5]
                        {
                            KeyInterop.VirtualKeyFromKey(Key.Right), KeyInterop.VirtualKeyFromKey(Key.Left),
                            KeyInterop.VirtualKeyFromKey(Key.Up), KeyInterop.VirtualKeyFromKey(Key.Down),
                            X360Controls.Unbound
                        });
                        break;
                    case 2:
                        actionBtns.AddRange(new object[5]
                        {
                            KeyInterop.VirtualKeyFromKey(Key.Left), KeyInterop.VirtualKeyFromKey(Key.Right),
                            KeyInterop.VirtualKeyFromKey(Key.Down), KeyInterop.VirtualKeyFromKey(Key.Up),
                            X360Controls.Unbound
                        });
                        break;
                }
            else if (baseTag == 7)
                switch (subTag)
                {
                    case 0:
                        actionBtns.AddRange(new object[5]
                        {
                            X360Controls.MouseUp, X360Controls.MouseDown,
                            X360Controls.MouseLeft, X360Controls.MouseRight,
                            X360Controls.Unbound
                        });
                        break;
                    case 1:
                        actionBtns.AddRange(new object[5]
                        {
                            X360Controls.MouseDown, X360Controls.MouseUp,
                            X360Controls.MouseRight, X360Controls.MouseLeft,
                            X360Controls.Unbound
                        });
                        break;
                    case 2:
                        actionBtns.AddRange(new object[5]
                        {
                            X360Controls.MouseUp, X360Controls.MouseDown,
                            X360Controls.MouseRight, X360Controls.MouseLeft,
                            X360Controls.Unbound
                        });
                        break;
                    case 3:
                        actionBtns.AddRange(new object[5]
                        {
                            X360Controls.MouseDown, X360Controls.MouseUp,
                            X360Controls.MouseLeft, X360Controls.MouseRight,
                            X360Controls.Unbound
                        });
                        break;
                    case 4:
                        actionBtns.AddRange(new object[5]
                        {
                            X360Controls.MouseRight, X360Controls.MouseLeft,
                            X360Controls.MouseUp, X360Controls.MouseDown,
                            X360Controls.Unbound
                        });
                        break;
                    case 5:
                        actionBtns.AddRange(new object[5]
                        {
                            X360Controls.MouseLeft, X360Controls.MouseRight,
                            X360Controls.MouseDown, X360Controls.MouseUp,
                            X360Controls.Unbound
                        });
                        break;
                }
            else if (baseTag == 8)
                actionBtns.AddRange(new object[5]
                {
                    X360Controls.Unbound, X360Controls.Unbound,
                    X360Controls.Unbound, X360Controls.Unbound,
                    X360Controls.Unbound
                });


            switch (HighlightControl)
            {
                case ControlSelection.DPad:
                    inputControls.AddRange(new DS4ControlItem[4]
                    {
                        DS4ControlItem.DpadUp, DS4ControlItem.DpadDown,
                        DS4ControlItem.DpadLeft, DS4ControlItem.DpadRight
                    });
                    break;
                case ControlSelection.LeftStick:
                    inputControls.AddRange(new DS4ControlItem[5]
                    {
                        DS4ControlItem.LYNeg, DS4ControlItem.LYPos,
                        DS4ControlItem.LXNeg, DS4ControlItem.LXPos, DS4ControlItem.L3
                    });
                    break;
                case ControlSelection.RightStick:
                    inputControls.AddRange(new DS4ControlItem[5]
                    {
                        DS4ControlItem.RYNeg, DS4ControlItem.RYPos,
                        DS4ControlItem.RXNeg, DS4ControlItem.RXPos, DS4ControlItem.R3
                    });
                    break;
                case ControlSelection.FaceButtons:
                    inputControls.AddRange(new DS4ControlItem[4]
                    {
                        DS4ControlItem.Triangle, DS4ControlItem.Cross,
                        DS4ControlItem.Square, DS4ControlItem.Circle
                    });
                    break;
                case ControlSelection.None:
                default:
                    break;
            }

            var idx = 0;
            foreach (var dsControl in inputControls)
            {
                var setting = Global.Instance.Config.GetDs4ControllerSetting(deviceNum, dsControl);
                setting.Reset();
                if (idx < actionBtns.Count && actionBtns[idx] != null)
                {
                    var outAct = actionBtns[idx];
                    var defaultControl = Global.DefaultButtonMapping[(int)dsControl];
                    if (!(outAct is X360Controls) || defaultControl != (X360Controls)outAct)
                    {
                        setting.UpdateSettings(false, outAct, null, DS4KeyType.None);
                        Global.RefreshActionAlias(setting, false);
                    }
                }

                idx++;
            }

            return inputControls;
        }
    }

    public class TriggerModeChoice
    {
        public TriggerMode mode;

        public TriggerModeChoice(string name, TriggerMode mode)
        {
            DisplayName = name;
            this.mode = mode;
        }

        public string DisplayName { get; set; }

        public TriggerMode Mode
        {
            get => mode;
            set => mode = value;
        }

        public override string ToString()
        {
            return DisplayName;
        }
    }

    public class TwoStageChoice
    {
        public TwoStageChoice(string name, TwoStageTriggerMode mode)
        {
            DisplayName = name;
            Mode = mode;
        }

        public string DisplayName { get; set; }

        public TwoStageTriggerMode Mode { get; set; }
    }

    public class TriggerEffectChoice
    {
        public TriggerEffectChoice(string name, TriggerEffects mode)
        {
            DisplayName = name;
            Mode = mode;
        }

        public string DisplayName { get; set; }

        public TriggerEffects Mode { get; set; }
    }
}
