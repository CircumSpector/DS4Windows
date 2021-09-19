using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using DS4Windows;

namespace DS4WinWPF.DS4Forms.ViewModels
{
    public class SpecialActionsListViewModel
    {
        private int deviceNum;
        private ObservableCollection<SpecialActionItem> actionCol =
            new ObservableCollection<SpecialActionItem>();
        private int specialActionIndex = -1;

        public ObservableCollection<SpecialActionItem> ActionCol { get => actionCol; }
        public int DeviceNum { get => deviceNum; }
        public int SpecialActionIndex
        {
            get => specialActionIndex;
            set
            {
                if (specialActionIndex == value) return;
                specialActionIndex = value;
                SpecialActionIndexChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler SpecialActionIndexChanged;

        private SpecialActionItem currentSAItem;
        public SpecialActionItem CurrentSpecialActionItem
        {
            get => currentSAItem;
            set
            {
                currentSAItem = value;
            }
        }

        public bool ItemSelected { get => specialActionIndex >= 0; }
        public event EventHandler ItemSelectedChanged;

        public SpecialActionsListViewModel(int deviceNum)
        {
            this.deviceNum = deviceNum;

            SpecialActionIndexChanged += SpecialActionsListViewModel_SpecialActionIndexChanged;
        }

        private void SpecialActionsListViewModel_SpecialActionIndexChanged(object sender, EventArgs e)
        {
            ItemSelectedChanged?.Invoke(this, EventArgs.Empty);
        }

        public void LoadActions(bool newProfile = false)
        {
            actionCol.Clear();

            var pactions = Global.Instance.Config.ProfileActions[deviceNum];
            int idx = 0;
            foreach (SpecialAction action in Global.Instance.Config.Actions)
            {
                string displayName = GetActionDisplayName(action);
                SpecialActionItem item = new SpecialActionItem(action, displayName, idx);

                if (pactions.Contains(action.Name))
                {
                    item.Active = true;
                }
                else if (newProfile && action.TypeId == SpecialAction.ActionTypeId.DisconnectBT)
                {
                    item.Active = true;
                }

                actionCol.Add(item);
                idx++;
            }
        }

        public SpecialActionItem CreateActionItem(SpecialAction action)
        {
            string displayName = GetActionDisplayName(action);
            SpecialActionItem item = new SpecialActionItem(action, displayName, 0);
            return item;
        }

        public string GetActionDisplayName(SpecialAction action)
        {
            string displayName = string.Empty;
            switch (action.TypeId)
            {
                case SpecialAction.ActionTypeId.DisconnectBT:
                    displayName = Properties.Resources.DisconnectBT; break;
                case SpecialAction.ActionTypeId.Macro:
                    displayName = Properties.Resources.Macro + (action.KeyType.HasFlag(DS4KeyType.ScanCode) ? " (" + Properties.Resources.ScanCode + ")" : "");
                    break;
                case SpecialAction.ActionTypeId.Program:
                    displayName = Properties.Resources.LaunchProgram.Replace("*program*", Path.GetFileNameWithoutExtension(action.Details));
                    break;
                case SpecialAction.ActionTypeId.Profile:
                    displayName = Properties.Resources.LoadProfile.Replace("*profile*", action.Details);
                    break;
                case SpecialAction.ActionTypeId.Key:
                    displayName = KeyInterop.KeyFromVirtualKey(int.Parse(action.Details)).ToString() +
                         (action.UTrigger.Count > 0 ? " (Toggle)" : "");
                    break;
                case SpecialAction.ActionTypeId.BatteryCheck:
                    displayName = Properties.Resources.CheckBattery;
                    break;
                case SpecialAction.ActionTypeId.XboxGameDVR:
                    displayName = "Xbox Game DVR";
                    break;
                case SpecialAction.ActionTypeId.MultiAction:
                    displayName = Properties.Resources.MultiAction;
                    break;
                case SpecialAction.ActionTypeId.SASteeringWheelEmulationCalibrate:
                    displayName = Properties.Resources.SASteeringWheelEmulationCalibrate;
                    break;
                default: break;
            }

            return displayName;
        }

        public void ExportEnabledActions()
        {
            List<string> pactions = new List<string>();
            foreach(SpecialActionItem item in actionCol)
            {
                if (item.Active)
                {
                    pactions.Add(item.ActionName);
                }
            }

            Global.Instance.Config.ProfileActions[deviceNum] = pactions;
            Global.Instance.CacheExtraProfileInfo(deviceNum);
        }

        public void RemoveAction(SpecialActionItem item)
        {
            Global.Instance.RemoveAction(item.SpecialAction.Name);
            actionCol.RemoveAt(specialActionIndex);
            Global.Instance.Config.ProfileActions[deviceNum].Remove(item.SpecialAction.Name);
            Global.Instance.CacheExtraProfileInfo(deviceNum);
        }
    }

    public class SpecialActionItem
    {
        private SpecialAction specialAction;
        private bool active;
        private string typeName;
        private int index = 0;

        public SpecialActionItem(SpecialAction specialAction, string displayName,
            int index)
        {
            this.specialAction = specialAction;
            this.typeName = displayName;
            this.index = index;
        }

        /// <summary>
        /// Index of SpecialActionItem in the ObservableCollection
        /// </summary>
        public int Index
        {
            get => index;
            set => index = value;
        }

        /// <summary>
        /// The user defined name for a Special Action
        /// </summary>
        public string ActionName
        {
            get => specialAction.Name;
            set
            {
                specialAction.Name = value;
            }
        }
        public event EventHandler ActionNameChanged;

        /// <summary>
        /// Flag to determine if a Special Action is enabled in a specific Profile
        /// </summary>
        public bool Active
        {
            get => active;
            set
            {
                if (active == value) return;
                active = value;
                ActiveChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler ActiveChanged;

        /// <summary>
        /// Display string with the trigger controls that launch a Special Action
        /// </summary>
        public string Controls { get => specialAction.Controls.Replace("/", ", "); }

        public event EventHandler ControlsChanged;

        /// <summary>
        /// Cached display string for the base type of the Special Action
        /// </summary>
        public string TypeName { get => typeName; }

        /// <summary>
        /// Reference to the SpecialAction instance
        /// </summary>
        public SpecialAction SpecialAction { get => specialAction; }

        public void Refresh()
        {
            ActionNameChanged?.Invoke(this, EventArgs.Empty);
            ControlsChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
