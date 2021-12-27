using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using DS4Windows;
using DS4WinWPF.DS4Forms.ViewModels.Util;
using JetBrains.Annotations;

namespace DS4WinWPF.DS4Forms.ViewModels
{
    public class SpecialActEditorViewModel : NotifyDataErrorBase, INotifyPropertyChanged
    {
        public SpecialActEditorViewModel(int deviceNum, SpecialActionV3 action)
        {
            DeviceNum = deviceNum;
            SavedAction = action;
            EditMode = SavedAction != null;
        }

        public int DeviceNum { get; }

        public int ActionTypeIndex { get; set; }
        public string ActionName { get; set; }

        public SpecialActionV3.ActionTypeId[] TypeAssoc { get; } =
        {
            SpecialActionV3.ActionTypeId.None, SpecialActionV3.ActionTypeId.Macro,
            SpecialActionV3.ActionTypeId.Program, SpecialActionV3.ActionTypeId.Profile,
            SpecialActionV3.ActionTypeId.Key, SpecialActionV3.ActionTypeId.DisconnectBT,
            SpecialActionV3.ActionTypeId.BatteryCheck, SpecialActionV3.ActionTypeId.MultiAction,
            SpecialActionV3.ActionTypeId.SASteeringWheelEmulationCalibrate
        };

        public SpecialActionV3 SavedAction { get; }

        public List<string> ControlTriggerList { get; } = new();

        public List<string> ControlUnloadTriggerList { get; } = new();

        public bool EditMode { get; }

        public bool TriggerError => errors.TryGetValue("TriggerError", out var _);

        public bool ExistingName { get; private set; }

        public void LoadAction(SpecialActionV3 action)
        {
            foreach (var s in action.Controls.Split('/')) ControlTriggerList.Add(s);

            if (action.UControls != null)
                foreach (var s in action.UControls.Split('/'))
                    if (s != "AutomaticUntrigger")
                        ControlUnloadTriggerList.Add(s);

            ActionName = action.Name;
            for (var i = 0; i < TypeAssoc.Length; i++)
            {
                var type = TypeAssoc[i];
                if (type == action.TypeId)
                {
                    ActionTypeIndex = i;
                    break;
                }
            }
        }

        public void SetAction(SpecialActionV3 action)
        {
            action.Name = ActionName;
            action.Controls = string.Join("/", ControlTriggerList.ToArray());
            if (ControlUnloadTriggerList.Count > 0)
                action.UControls = string.Join("/", ControlUnloadTriggerList.ToArray());
            action.TypeId = TypeAssoc[ActionTypeIndex];
        }

        public override bool IsValid(SpecialActionV3 action)
        {
            ClearOldErrors();

            var valid = true;
            var actionNameErrors = new List<string>();
            var triggerErrors = new List<string>();
            var typeErrors = new List<string>();

            if (string.IsNullOrWhiteSpace(ActionName))
            {
                valid = false;
                actionNameErrors.Add("No name provided");
            }
            else if (!EditMode || SavedAction.Name != ActionName)
            {
                // Perform existing name check when creating a new action
                // or if the action name has changed
                foreach (var sA in Global.Instance.Config.Actions)
                    if (sA.Name == ActionName)
                    {
                        valid = false;
                        actionNameErrors.Add("Existing action with name already exists");
                        ExistingName = true;
                        break;
                    }
            }

            if (ControlTriggerList.Count == 0)
            {
                valid = false;
                triggerErrors.Add("No triggers provided");
            }

            if (ActionTypeIndex == 0)
            {
                valid = false;
                typeErrors.Add("Specify an action type");
            }

            if (actionNameErrors.Count > 0)
            {
                errors["ActionName"] = actionNameErrors;
                RaiseErrorsChanged("ActionName");
            }

            if (triggerErrors.Count > 0)
            {
                errors["TriggerError"] = triggerErrors;
                RaiseErrorsChanged("TriggerError");
            }

            if (typeErrors.Count > 0)
            {
                errors["ActionTypeIndex"] = typeErrors;
                RaiseErrorsChanged("ActionTypeIndex");
            }

            return valid;
        }

        public override void ClearOldErrors()
        {
            ExistingName = false;

            if (errors.Count > 0)
            {
                errors.Clear();
                RaiseErrorsChanged("ActionName");
                RaiseErrorsChanged("TriggerError");
                RaiseErrorsChanged("ActionTypeIndex");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [UsedImplicitly]
        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}