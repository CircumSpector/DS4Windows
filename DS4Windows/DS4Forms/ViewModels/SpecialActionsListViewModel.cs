using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using DS4Windows;
using DS4WinWPF.Properties;
using JetBrains.Annotations;

namespace DS4WinWPF.DS4Forms.ViewModels
{
    public class SpecialActionsListViewModel : INotifyPropertyChanged
    {
        public SpecialActionsListViewModel(int deviceNum)
        {
            DeviceNum = deviceNum;
        }

        public ObservableCollection<SpecialActionItem> ActionCol { get; } = new();

        public int DeviceNum { get; }

        public int SpecialActionIndex { get; set; } = -1;

        public SpecialActionItem CurrentSpecialActionItem { get; set; }

        public bool ItemSelected => SpecialActionIndex >= 0;

        public void LoadActions(bool newProfile = false)
        {
            ActionCol.Clear();

            var pactions = Global.Instance.Config.ProfileActions[DeviceNum];
            var idx = 0;
            foreach (var action in Global.Instance.Config.Actions)
            {
                var displayName = GetActionDisplayName(action);
                var item = new SpecialActionItem(action, displayName, idx);

                if (pactions.Contains(action.Name))
                    item.IsActive = true;
                else if (newProfile && action.TypeId == SpecialAction.ActionTypeId.DisconnectBT) item.IsActive = true;

                ActionCol.Add(item);
                idx++;
            }
        }

        public SpecialActionItem CreateActionItem(SpecialAction action)
        {
            var displayName = GetActionDisplayName(action);
            var item = new SpecialActionItem(action, displayName, 0);
            return item;
        }

        public string GetActionDisplayName(SpecialAction action)
        {
            var displayName = string.Empty;
            switch (action.TypeId)
            {
                case SpecialAction.ActionTypeId.DisconnectBT:
                    displayName = Resources.DisconnectBT;
                    break;
                case SpecialAction.ActionTypeId.Macro:
                    displayName = Resources.Macro + (action.KeyType.HasFlag(DS4KeyType.ScanCode)
                        ? " (" + Resources.ScanCode + ")"
                        : "");
                    break;
                case SpecialAction.ActionTypeId.Program:
                    displayName =
                        Resources.LaunchProgram.Replace("*program*", Path.GetFileNameWithoutExtension(action.Details));
                    break;
                case SpecialAction.ActionTypeId.Profile:
                    displayName = Resources.LoadProfile.Replace("*profile*", action.Details);
                    break;
                case SpecialAction.ActionTypeId.Key:
                    displayName = KeyInterop.KeyFromVirtualKey(int.Parse(action.Details)) +
                                  (action.UTrigger.Count > 0 ? " (Toggle)" : "");
                    break;
                case SpecialAction.ActionTypeId.BatteryCheck:
                    displayName = Resources.CheckBattery;
                    break;
                case SpecialAction.ActionTypeId.XboxGameDVR:
                    displayName = "Xbox Game DVR";
                    break;
                case SpecialAction.ActionTypeId.MultiAction:
                    displayName = Resources.MultiAction;
                    break;
                case SpecialAction.ActionTypeId.SASteeringWheelEmulationCalibrate:
                    displayName = Resources.SASteeringWheelEmulationCalibrate;
                    break;
            }

            return displayName;
        }

        public void ExportEnabledActions()
        {
            var pactions = new List<string>();
            foreach (var item in ActionCol)
                if (item.IsActive)
                    pactions.Add(item.ActionName);

            Global.Instance.Config.ProfileActions[DeviceNum] = pactions;
            Global.Instance.Config.CacheExtraProfileInfo(DeviceNum);
        }

        public void RemoveAction(SpecialActionItem item)
        {
            Global.Instance.Config.RemoveAction(item.SpecialAction.Name);
            ActionCol.RemoveAt(SpecialActionIndex);
            Global.Instance.Config.ProfileActions[DeviceNum].Remove(item.SpecialAction.Name);
            Global.Instance.Config.CacheExtraProfileInfo(DeviceNum);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [UsedImplicitly]
        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class SpecialActionItem : INotifyPropertyChanged
    {
        public SpecialActionItem(SpecialAction specialAction, string displayName,
            int index)
        {
            SpecialAction = specialAction;
            TypeName = displayName;
            Index = index;
        }

        /// <summary>
        ///     Index of SpecialActionItem in the ObservableCollection
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        ///     The user defined name for a Special Action
        /// </summary>
        public string ActionName
        {
            get => SpecialAction.Name;
            set => SpecialAction.Name = value;
        }

        /// <summary>
        ///     Flag to determine if a Special Action is enabled in a specific Profile
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        ///     Display string with the trigger controls that launch a Special Action
        /// </summary>
        public string Controls => SpecialAction.Controls.Replace("/", ", ");

        /// <summary>
        ///     Cached display string for the base type of the Special Action
        /// </summary>
        public string TypeName { get; }

        /// <summary>
        ///     Reference to the SpecialAction instance
        /// </summary>
        public SpecialAction SpecialAction { get; }

        public event PropertyChangedEventHandler PropertyChanged;

        [UsedImplicitly]
        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}