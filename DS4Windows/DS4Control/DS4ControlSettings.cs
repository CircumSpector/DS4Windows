using System;
using PropertyChanged;

namespace DS4Windows
{
    [AddINotifyPropertyChangedInterface]
    public class DS4ControlSettings
    {
        public enum ActionType : byte
        {
            Default,
            Key,
            Button,
            Macro
        }

        public const int MaxMacroValue = 286;

        public DS4ControlSettings(DS4Controls ctrl)
        {
            Control = ctrl;
        }

        public ControlActionData ActionData { get; private set; } = new();

        public ActionType ControlActionType { get; set; } = ActionType.Default;

        public DS4Controls Control { get; set; }

        public string Extras { get; set; }

        public DS4KeyType KeyType { get; set; } = DS4KeyType.None;

        public ControlActionData ShiftAction { get; private set; } = new();

        public ActionType ShiftActionType { get; set; } = ActionType.Default;

        public string ShiftExtras { get; set; }

        public DS4KeyType ShiftKeyType { get; set; } = DS4KeyType.None;

        public int ShiftTrigger { get; set; }

        public bool IsDefault => ControlActionType == ActionType.Default;

        public bool IsShiftDefault => ShiftActionType == ActionType.Default;

        public void Reset()
        {
            Extras = null;
            KeyType = DS4KeyType.None;
            ControlActionType = ActionType.Default;
            ActionData = new ControlActionData { ActionAlias = 0 };
            ShiftActionType = ActionType.Default;
            ShiftAction = new ControlActionData { ActionAlias = 0 };
            ShiftTrigger = 0;
            ShiftExtras = null;
            ShiftKeyType = DS4KeyType.None;
        }

        public bool IsExtrasEmpty(string extraStr)
        {
            return string.IsNullOrEmpty(extraStr) || extraStr == "0,0,0,0,0,0,0,0,0";
        }

        internal void UpdateSettings(bool shift, object act, string extras, DS4KeyType kt, int trigger = 0)
        {
            if (!shift)
            {
                switch (act)
                {
                    case int or ushort:
                        ControlActionType = ActionType.Key;
                        ActionData.ActionKey = Convert.ToInt32(act);
                        break;
                    case string or X360Controls:
                    {
                        ControlActionType = ActionType.Button;
                        if (act is X360Controls controls)
                            ActionData.ActionButton = controls;
                        else
                            Enum.TryParse(act.ToString(), out ActionData.ActionButton);
                        break;
                    }
                    case int[] values:
                        ControlActionType = ActionType.Macro;
                        ActionData.ActionMacro = values;
                        break;
                    default:
                        ControlActionType = ActionType.Default;
                        ActionData.ActionKey = 0;
                        break;
                }

                Extras = extras;
                KeyType = kt;
            }
            else
            {
                switch (act)
                {
                    case int:
                    case ushort:
                        ShiftActionType = ActionType.Key;
                        ShiftAction.ActionKey = Convert.ToInt32(act);
                        break;
                    case string:
                    case X360Controls:
                    {
                        ShiftActionType = ActionType.Button;
                        if (act is X360Controls controls)
                            ShiftAction.ActionButton = controls;
                        else
                            Enum.TryParse(act.ToString(), out ShiftAction.ActionButton);
                        break;
                    }
                    case int[] values:
                        ShiftActionType = ActionType.Macro;
                        ShiftAction.ActionMacro = values;
                        break;
                    default:
                        ShiftActionType = ActionType.Default;
                        ShiftAction.ActionKey = 0;
                        break;
                }

                ShiftExtras = extras;
                ShiftKeyType = kt;
                ShiftTrigger = trigger;
            }
        }
    }
}