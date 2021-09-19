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
        public ControlActionData action = new();
        public ActionType actionType = ActionType.Default;

        public DS4Controls control;
        public string extras;
        public DS4KeyType keyType = DS4KeyType.None;
        public ControlActionData shiftAction = new();

        public ActionType shiftActionType = ActionType.Default;
        public string shiftExtras;
        public DS4KeyType shiftKeyType = DS4KeyType.None;
        public int shiftTrigger;

        public DS4ControlSettings(DS4Controls ctrl)
        {
            control = ctrl;
        }

        public bool IsDefault => actionType == ActionType.Default;
        public bool IsShiftDefault => shiftActionType == ActionType.Default;

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
                        action.actionBtn = (X360Controls) act;
                    else
                        Enum.TryParse(act.ToString(), out action.actionBtn);
                }
                else if (act is int[])
                {
                    actionType = ActionType.Macro;
                    action.actionMacro = (int[]) act;
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
                        shiftAction.actionBtn = (X360Controls) act;
                    else
                        Enum.TryParse(act.ToString(), out shiftAction.actionBtn);
                }
                else if (act is int[])
                {
                    shiftActionType = ActionType.Macro;
                    shiftAction.actionMacro = (int[]) act;
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
}