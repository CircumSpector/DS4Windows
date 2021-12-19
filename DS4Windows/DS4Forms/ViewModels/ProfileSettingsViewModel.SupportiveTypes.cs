using System.Collections.Generic;
using System.Windows.Input;
using DS4Windows;
using DS4Windows.InputDevices;

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

        public ControlSelection PresetTagIndex(DS4Controls control)
        {
            var controlInput = ControlSelection.None;
            switch (control)
            {
                case DS4Controls.DpadUp:
                case DS4Controls.DpadDown:
                case DS4Controls.DpadLeft:
                case DS4Controls.DpadRight:
                    controlInput = ControlSelection.DPad;
                    break;
                case DS4Controls.LXNeg:
                case DS4Controls.LXPos:
                case DS4Controls.LYNeg:
                case DS4Controls.LYPos:
                case DS4Controls.L3:
                    controlInput = ControlSelection.LeftStick;
                    break;
                case DS4Controls.RXNeg:
                case DS4Controls.RXPos:
                case DS4Controls.RYNeg:
                case DS4Controls.RYPos:
                case DS4Controls.R3:
                    controlInput = ControlSelection.RightStick;
                    break;
                case DS4Controls.Cross:
                case DS4Controls.Circle:
                case DS4Controls.Triangle:
                case DS4Controls.Square:
                    controlInput = ControlSelection.FaceButtons;
                    break;
            }


            return controlInput;
        }

        public void SetHighlightControl(DS4Controls control)
        {
            var controlInput = PresetTagIndex(control);
            HighlightControl = controlInput;
        }

        public List<DS4Controls> ModifySettingWithPreset(int baseTag, int subTag)
        {
            var actionBtns = new List<object>(5);
            var inputControls = new List<DS4Controls>(5);
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
                    inputControls.AddRange(new DS4Controls[4]
                    {
                        DS4Controls.DpadUp, DS4Controls.DpadDown,
                        DS4Controls.DpadLeft, DS4Controls.DpadRight
                    });
                    break;
                case ControlSelection.LeftStick:
                    inputControls.AddRange(new DS4Controls[5]
                    {
                        DS4Controls.LYNeg, DS4Controls.LYPos,
                        DS4Controls.LXNeg, DS4Controls.LXPos, DS4Controls.L3
                    });
                    break;
                case ControlSelection.RightStick:
                    inputControls.AddRange(new DS4Controls[5]
                    {
                        DS4Controls.RYNeg, DS4Controls.RYPos,
                        DS4Controls.RXNeg, DS4Controls.RXPos, DS4Controls.R3
                    });
                    break;
                case ControlSelection.FaceButtons:
                    inputControls.AddRange(new DS4Controls[4]
                    {
                        DS4Controls.Triangle, DS4Controls.Cross,
                        DS4Controls.Square, DS4Controls.Circle
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
