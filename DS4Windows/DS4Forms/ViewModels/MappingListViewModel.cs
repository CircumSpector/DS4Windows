using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using DS4Windows;
using DS4Windows.Shared.Common.Types;
using DS4WinWPF.DS4Control.IoC.Services;
using DS4WinWPF.Properties;
using JetBrains.Annotations;

namespace DS4WinWPF.DS4Forms.ViewModels
{
    public interface IMappingListViewModel
    {
        ReadOnlyObservableCollection<MappedControl> Mappings { get; }
        MappedControl SelectedControl { get; set; }
        Dictionary<DS4ControlItem, MappedControl> ControlMap { get; }

        /// <summary>
        ///     DS4Controls -> Int index map. Store appropriate list index for a stored MappedControl instance
        /// </summary>
        Dictionary<DS4ControlItem, int> ControlIndexMap { get; }

        MappedControl L2FullPullControl { get; }
        MappedControl R2FullPullControl { get; }
        MappedControl LsOuterBindControl { get; }
        MappedControl RsOuterBindControl { get; }
        event PropertyChangedEventHandler PropertyChanged;
        void UpdateMappingDevType(OutputDeviceType devType);
        void UpdateMappings();
    }

    /// <summary>
    ///     Controller buttons/axes to remapped actions view model.
    /// </summary>
    public class MappingListViewModel : INotifyPropertyChanged, IMappingListViewModel
    {
        private readonly ObservableCollection<MappedControl> mappings = new();
        private readonly List<MappedControl> extraControls = new();

        private readonly MappedControl gyroSwipeDownControl;

        private readonly MappedControl gyroSwipeLeftControl;
        private readonly MappedControl gyroSwipeRightControl;
        private readonly MappedControl gyroSwipeUpControl;

        public MappingListViewModel(IProfilesService profileService)
        {
            mappings.Add(new MappedControl(profileService, DS4ControlItem.Cross));
            mappings.Add(new MappedControl(profileService, DS4ControlItem.Circle));
            mappings.Add(new MappedControl(profileService, DS4ControlItem.Square));
            mappings.Add(new MappedControl(profileService, DS4ControlItem.Triangle));
            mappings.Add(new MappedControl(profileService, DS4ControlItem.Options));
            mappings.Add(new MappedControl(profileService, DS4ControlItem.Share));
            mappings.Add(new MappedControl(profileService, DS4ControlItem.DpadUp));
            mappings.Add(new MappedControl(profileService, DS4ControlItem.DpadDown));
            mappings.Add(new MappedControl(profileService, DS4ControlItem.DpadLeft));
            mappings.Add(new MappedControl(profileService, DS4ControlItem.DpadRight));
            mappings.Add(new MappedControl(profileService, DS4ControlItem.PS));
            mappings.Add(new MappedControl(profileService, DS4ControlItem.Mute));
            mappings.Add(new MappedControl(profileService, DS4ControlItem.L1));
            mappings.Add(new MappedControl(profileService, DS4ControlItem.R1));
            mappings.Add(new MappedControl(profileService, DS4ControlItem.L2));
            mappings.Add(new MappedControl(profileService, DS4ControlItem.R2));
            mappings.Add(new MappedControl(profileService, DS4ControlItem.L3));
            mappings.Add(new MappedControl(profileService, DS4ControlItem.R3));
            mappings.Add(new MappedControl(profileService, DS4ControlItem.Capture));
            mappings.Add(new MappedControl(profileService, DS4ControlItem.SideL));
            mappings.Add(new MappedControl(profileService, DS4ControlItem.SideR));

            mappings.Add(new MappedControl(profileService, DS4ControlItem.TouchLeft));
            mappings.Add(new MappedControl(profileService, DS4ControlItem.TouchRight));
            mappings.Add(new MappedControl(profileService, DS4ControlItem.TouchMulti));
            mappings.Add(new MappedControl(profileService, DS4ControlItem.TouchUpper));

            mappings.Add(new MappedControl(profileService, DS4ControlItem.LYNeg));
            mappings.Add(new MappedControl(profileService, DS4ControlItem.LYPos));
            mappings.Add(new MappedControl(profileService, DS4ControlItem.LXNeg));
            mappings.Add(new MappedControl(profileService, DS4ControlItem.LXPos));

            mappings.Add(new MappedControl(profileService, DS4ControlItem.RYNeg));
            mappings.Add(new MappedControl(profileService, DS4ControlItem.RYPos));
            mappings.Add(new MappedControl(profileService, DS4ControlItem.RXNeg));
            mappings.Add(new MappedControl(profileService, DS4ControlItem.RXPos));

            mappings.Add(new MappedControl(profileService, DS4ControlItem.GyroZNeg));
            mappings.Add(new MappedControl(profileService, DS4ControlItem.GyroZPos));
            mappings.Add(new MappedControl(profileService, DS4ControlItem.GyroXPos));
            mappings.Add(new MappedControl(profileService, DS4ControlItem.GyroXNeg));

            mappings.Add(new MappedControl(profileService, DS4ControlItem.SwipeUp));
            mappings.Add(new MappedControl(profileService, DS4ControlItem.SwipeDown));
            mappings.Add(new MappedControl(profileService, DS4ControlItem.SwipeLeft));
            mappings.Add(new MappedControl(profileService, DS4ControlItem.SwipeRight));

            Mappings = new ReadOnlyObservableCollection<MappedControl>(mappings);

            var controlIndex = 0;
            foreach (var mapped in Mappings)
            {
                ControlMap.Add(mapped.Control, mapped);
                ControlIndexMap.Add(mapped.Control, controlIndex);
                controlIndex++;
            }

            /*
             * Establish data binding data for virtual button DS4ControlSettings instances
             */
            LsOuterBindControl = new MappedControl(profileService, DS4ControlItem.LSOuter);
            RsOuterBindControl = new MappedControl(profileService, DS4ControlItem.RSOuter);

            L2FullPullControl = new MappedControl(profileService, DS4ControlItem.L2FullPull);
            R2FullPullControl = new MappedControl(profileService, DS4ControlItem.R2FullPull);

            gyroSwipeLeftControl = new MappedControl(profileService, DS4ControlItem.GyroSwipeLeft);
            gyroSwipeRightControl = new MappedControl(profileService, DS4ControlItem.GyroSwipeRight);
            gyroSwipeUpControl = new MappedControl(profileService, DS4ControlItem.GyroSwipeUp);
            gyroSwipeDownControl = new MappedControl(profileService, DS4ControlItem.GyroSwipeDown);

            extraControls.Add(LsOuterBindControl);
            extraControls.Add(RsOuterBindControl);
            extraControls.Add(L2FullPullControl);
            extraControls.Add(R2FullPullControl);
            extraControls.Add(gyroSwipeLeftControl);
            extraControls.Add(gyroSwipeRightControl);
            extraControls.Add(gyroSwipeUpControl);
            extraControls.Add(gyroSwipeDownControl);

            ControlMap.Add(DS4ControlItem.LSOuter, LsOuterBindControl);
            ControlMap.Add(DS4ControlItem.RSOuter, RsOuterBindControl);
            ControlMap.Add(DS4ControlItem.L2FullPull, L2FullPullControl);
            ControlMap.Add(DS4ControlItem.R2FullPull, R2FullPullControl);
            ControlMap.Add(DS4ControlItem.GyroSwipeLeft, gyroSwipeLeftControl);
            ControlMap.Add(DS4ControlItem.GyroSwipeRight, gyroSwipeRightControl);
            ControlMap.Add(DS4ControlItem.GyroSwipeUp, gyroSwipeUpControl);
            ControlMap.Add(DS4ControlItem.GyroSwipeDown, gyroSwipeDownControl);
        }

        public ReadOnlyObservableCollection<MappedControl> Mappings { get; }

        public MappedControl SelectedControl { get; set; }

        public Dictionary<DS4ControlItem, MappedControl> ControlMap { get; } = new();

        /// <summary>
        ///     DS4Controls -> Int index map. Store appropriate list index for a stored MappedControl instance
        /// </summary>
        public Dictionary<DS4ControlItem, int> ControlIndexMap { get; } = new();

        public MappedControl L2FullPullControl { get; }

        public MappedControl R2FullPullControl { get; }

        public MappedControl LsOuterBindControl { get; }

        public MappedControl RsOuterBindControl { get; }

        public event PropertyChangedEventHandler PropertyChanged;

        public void UpdateMappingDevType(OutputDeviceType devType)
        {
            foreach (var mapped in Mappings) mapped.DevType = devType;

            foreach (var mapped in extraControls) mapped.DevType = devType;
        }

        public void UpdateMappings()
        {
            foreach (var mapped in Mappings) mapped.UpdateMappingName();

            foreach (var mapped in extraControls) mapped.UpdateMappingName();
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    /// <summary>
    ///     Remapped control entity description.
    /// </summary>
    public class MappedControl : INotifyPropertyChanged
    {
        public MappedControl(
            IProfilesService profileService,
            DS4ControlItem control,
            bool initMap = false
        )
        {
            // TODO: implement me!
            Setting = profileService.CurrentlyEditedProfile.PerControlSettings[control];
            DevType = profileService.CurrentlyEditedProfile.OutputDeviceType;
            Control = control;

            //mappingName = "?";
            if (initMap)
            {
                MappingName = GetMappingString();
                if (HasShiftAction)
                    ShiftMappingName = ShiftTrigger(Setting.ShiftTrigger) + " -> " + GetMappingString(true);
            }
        }

        public DS4ControlItem Control { get; }

        public DS4ControlSettingsV3 Setting { get; }

        public string ControlName => Control.ToDisplayName();

        public string MappingName { get; private set; }

        public OutputDeviceType DevType { get; set; }

        public string ShiftMappingName { get; set; }

        public bool HasShiftAction => Setting.ShiftActionType != DS4ControlSettingsV3.ActionType.Default;

        public event PropertyChangedEventHandler PropertyChanged;

        public void UpdateMappingName()
        {
            MappingName = GetMappingString();
            if (HasShiftAction)
                ShiftMappingName = ShiftTrigger(Setting.ShiftTrigger) + " -> " + GetMappingString(true);
            else
                ShiftMappingName = "";
        }

        public string GetMappingString(bool shift = false)
        {
            var temp = Resources.Unassigned;
            var action = !shift ? Setting.ActionData : Setting.ShiftAction;
            var sc = !shift
                ? Setting.KeyType.HasFlag(DS4KeyType.ScanCode)
                : Setting.ShiftKeyType.HasFlag(DS4KeyType.ScanCode);
            var extra = Control >= DS4ControlItem.GyroXPos && Control <= DS4ControlItem.SwipeDown;
            var actionType = !shift ? Setting.ControlActionType : Setting.ShiftActionType;
            if (actionType != DS4ControlSettingsV3.ActionType.Default)
            {
                if (actionType == DS4ControlSettingsV3.ActionType.Key)
                {
                    //return (Keys)int.Parse(action.ToString()) + (sc ? " (" + Properties.Resources.ScanCode + ")" : "");
                    temp = KeyInterop.KeyFromVirtualKey(action.ActionKey) + (sc ? " (" + Resources.ScanCode + ")" : "");
                }
                else if (actionType == DS4ControlSettingsV3.ActionType.Macro)
                {
                    temp = Resources.Macro + (sc ? " (" + Resources.ScanCode + ")" : "");
                }
                else if (actionType == DS4ControlSettingsV3.ActionType.Button)
                {
                    string tag;
                    tag = Global.GetX360ControlString(action.ActionButton, DevType);
                    temp = tag;
                }
                else
                {
                    temp = Global.GetX360ControlString(Global.DefaultButtonMapping[(int)Control], DevType);
                }
            }
            else if (!extra && !shift)
            {
                var tempOutControl = Global.DefaultButtonMapping[(int)Control];
                if (tempOutControl != X360Controls.None) temp = Global.GetX360ControlString(tempOutControl, DevType);
            }
            else if (shift)
            {
                temp = "";
            }

            return temp;
        }

        private static string ShiftTrigger(int trigger)
        {
            return trigger switch
            {
                1 => "Cross",
                2 => "Circle",
                3 => "Square",
                4 => "Triangle",
                5 => "Options",
                6 => "Share",
                7 => "Dpad Up",
                8 => "Dpad Down",
                9 => "Dpad Left",
                10 => "Dpad Right",
                11 => "PS",
                12 => "L1",
                13 => "R1",
                14 => "L2",
                15 => "R2",
                16 => "L3",
                17 => "R3",
                18 => "Left Touch",
                19 => "Upper Touch",
                20 => "Multi Touch",
                21 => "Right Touch",
                22 => Resources.TiltUp,
                23 => Resources.TiltDown,
                24 => Resources.TiltLeft,
                25 => Resources.TiltRight,
                26 => "Finger on Touchpad",
                27 => "Mute",
                28 => "Capture",
                29 => "Side L",
                30 => "Side R",
                _ => ""
            };
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            switch (propertyName)
            {
                case nameof(DevType):
                    UpdateMappingName();
                    break;
            }

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}