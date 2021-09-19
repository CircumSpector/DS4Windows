using System;

namespace DS4Windows
{
    public class DS4ControlSettings
    {
        public enum ActionType : byte
        {
            Default,
            Key,
            Button,
            Macro
        }

        public const int MAX_MACRO_VALUE = 286;

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

        public DS4ControlSettings(DS4Controls ctrl)
        {
            Control = ctrl;
        }

        public bool IsDefault => ControlActionType == ActionType.Default;

        public bool IsShiftDefault => ShiftActionType == ActionType.Default;

        public void Reset()
        {
            Extras = null;
            KeyType = DS4KeyType.None;
            ControlActionType = ActionType.Default;
            ActionData = new ControlActionData {actionAlias = 0};
            ShiftActionType = ActionType.Default;
            ShiftAction = new ControlActionData {actionAlias = 0};
            ShiftTrigger = 0;
            ShiftExtras = null;
            ShiftKeyType = DS4KeyType.None;
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
                    ControlActionType = ActionType.Key;
                    ActionData.actionKey = Convert.ToInt32(act);
                }
                else if (act is string || act is X360Controls)
                {
                    ControlActionType = ActionType.Button;
                    if (act is X360Controls)
                        ActionData.actionBtn = (X360Controls) act;
                    else
                        Enum.TryParse(act.ToString(), out ActionData.actionBtn);
                }
                else if (act is int[])
                {
                    ControlActionType = ActionType.Macro;
                    ActionData.actionMacro = (int[]) act;
                }
                else
                {
                    ControlActionType = ActionType.Default;
                    ActionData.actionKey = 0;
                }

                Extras = exts;
                KeyType = kt;
            }
            else
            {
                if (act is int || act is ushort)
                {
                    ShiftActionType = ActionType.Key;
                    ShiftAction.actionKey = Convert.ToInt32(act);
                }
                else if (act is string || act is X360Controls)
                {
                    ShiftActionType = ActionType.Button;
                    if (act is X360Controls)
                        ShiftAction.actionBtn = (X360Controls) act;
                    else
                        Enum.TryParse(act.ToString(), out ShiftAction.actionBtn);
                }
                else if (act is int[])
                {
                    ShiftActionType = ActionType.Macro;
                    ShiftAction.actionMacro = (int[]) act;
                }
                else
                {
                    ShiftActionType = ActionType.Default;
                    ShiftAction.actionKey = 0;
                }

                ShiftExtras = exts;
                ShiftKeyType = kt;
                ShiftTrigger = trigger;
            }
        }
    }
}