using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using DS4Windows;
using DS4WinWPF.DS4Control.IoC.Services;
using DS4WinWPF.Properties;
using JetBrains.Annotations;

namespace DS4WinWPF.DS4Forms.ViewModels
{
    /// <summary>
    ///     Controller buttons/axes to remapped actions view model.
    /// </summary>
    public class MappingListViewModel : INotifyPropertyChanged
    {
        private readonly List<MappedControl> extraControls = new();

        private readonly MappedControl gyroSwipeDownControl;

        private readonly MappedControl gyroSwipeLeftControl;
        private readonly MappedControl gyroSwipeRightControl;
        private readonly MappedControl gyroSwipeUpControl;

        private int selectedIndex = -1;

        public MappingListViewModel(IProfilesService profileService)
        {
            Mappings.Add(new MappedControl(profileService, DS4Controls.Cross));
            Mappings.Add(new MappedControl(profileService, DS4Controls.Circle));
            Mappings.Add(new MappedControl(profileService, DS4Controls.Square));
            Mappings.Add(new MappedControl(profileService, DS4Controls.Triangle));
            Mappings.Add(new MappedControl(profileService, DS4Controls.Options));
            Mappings.Add(new MappedControl(profileService, DS4Controls.Share));
            Mappings.Add(new MappedControl(profileService, DS4Controls.DpadUp));
            Mappings.Add(new MappedControl(profileService, DS4Controls.DpadDown));
            Mappings.Add(new MappedControl(profileService, DS4Controls.DpadLeft));
            Mappings.Add(new MappedControl(profileService, DS4Controls.DpadRight));
            Mappings.Add(new MappedControl(profileService, DS4Controls.PS));
            Mappings.Add(new MappedControl(profileService, DS4Controls.Mute));
            Mappings.Add(new MappedControl(profileService, DS4Controls.L1));
            Mappings.Add(new MappedControl(profileService, DS4Controls.R1));
            Mappings.Add(new MappedControl(profileService, DS4Controls.L2));
            Mappings.Add(new MappedControl(profileService, DS4Controls.R2));
            Mappings.Add(new MappedControl(profileService, DS4Controls.L3));
            Mappings.Add(new MappedControl(profileService, DS4Controls.R3));
            Mappings.Add(new MappedControl(profileService, DS4Controls.Capture));
            Mappings.Add(new MappedControl(profileService, DS4Controls.SideL));
            Mappings.Add(new MappedControl(profileService, DS4Controls.SideR));

            Mappings.Add(new MappedControl(profileService, DS4Controls.TouchLeft));
            Mappings.Add(new MappedControl(profileService, DS4Controls.TouchRight));
            Mappings.Add(new MappedControl(profileService, DS4Controls.TouchMulti));
            Mappings.Add(new MappedControl(profileService, DS4Controls.TouchUpper));

            Mappings.Add(new MappedControl(profileService, DS4Controls.LYNeg));
            Mappings.Add(new MappedControl(profileService, DS4Controls.LYPos));
            Mappings.Add(new MappedControl(profileService, DS4Controls.LXNeg));
            Mappings.Add(new MappedControl(profileService, DS4Controls.LXPos));

            Mappings.Add(new MappedControl(profileService, DS4Controls.RYNeg));
            Mappings.Add(new MappedControl(profileService, DS4Controls.RYPos));
            Mappings.Add(new MappedControl(profileService, DS4Controls.RXNeg));
            Mappings.Add(new MappedControl(profileService, DS4Controls.RXPos));

            Mappings.Add(new MappedControl(profileService, DS4Controls.GyroZNeg));
            Mappings.Add(new MappedControl(profileService, DS4Controls.GyroZPos));
            Mappings.Add(new MappedControl(profileService, DS4Controls.GyroXPos));
            Mappings.Add(new MappedControl(profileService, DS4Controls.GyroXNeg));

            Mappings.Add(new MappedControl(profileService, DS4Controls.SwipeUp));
            Mappings.Add(new MappedControl(profileService, DS4Controls.SwipeDown));
            Mappings.Add(new MappedControl(profileService, DS4Controls.SwipeLeft));
            Mappings.Add(new MappedControl(profileService, DS4Controls.SwipeRight));

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
            LsOuterBindControl = new MappedControl(profileService, DS4Controls.LSOuter);
            RsOuterBindControl = new MappedControl(profileService, DS4Controls.RSOuter);

            L2FullPullControl = new MappedControl(profileService, DS4Controls.L2FullPull);
            R2FullPullControl = new MappedControl(profileService, DS4Controls.R2FullPull);

            gyroSwipeLeftControl = new MappedControl(profileService, DS4Controls.GyroSwipeLeft);
            gyroSwipeRightControl =
                new MappedControl(profileService, DS4Controls.GyroSwipeRight);
            gyroSwipeUpControl = new MappedControl(profileService, DS4Controls.GyroSwipeUp);
            gyroSwipeDownControl = new MappedControl(profileService, DS4Controls.GyroSwipeDown);

            extraControls.Add(LsOuterBindControl);
            extraControls.Add(RsOuterBindControl);
            extraControls.Add(L2FullPullControl);
            extraControls.Add(R2FullPullControl);
            extraControls.Add(gyroSwipeLeftControl);
            extraControls.Add(gyroSwipeRightControl);
            extraControls.Add(gyroSwipeUpControl);
            extraControls.Add(gyroSwipeDownControl);

            ControlMap.Add(DS4Controls.LSOuter, LsOuterBindControl);
            ControlMap.Add(DS4Controls.RSOuter, RsOuterBindControl);
            ControlMap.Add(DS4Controls.L2FullPull, L2FullPullControl);
            ControlMap.Add(DS4Controls.R2FullPull, R2FullPullControl);
            ControlMap.Add(DS4Controls.GyroSwipeLeft, gyroSwipeLeftControl);
            ControlMap.Add(DS4Controls.GyroSwipeRight, gyroSwipeRightControl);
            ControlMap.Add(DS4Controls.GyroSwipeUp, gyroSwipeUpControl);
            ControlMap.Add(DS4Controls.GyroSwipeDown, gyroSwipeDownControl);
        }

        public ObservableCollection<MappedControl> Mappings { get; } = new();

        public int SelectedIndex
        {
            get => selectedIndex;
            set
            {
                if (selectedIndex == value) return;
                selectedIndex = value;
                SelectedIndexChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public Dictionary<DS4Controls, MappedControl> ControlMap { get; } = new();

        /// <summary>
        ///     DS4Controls -> Int index map. Store appropriate list index for a stored MappedControl instance
        /// </summary>
        public Dictionary<DS4Controls, int> ControlIndexMap { get; } = new();

        public MappedControl L2FullPullControl { get; }

        public MappedControl R2FullPullControl { get; }

        public MappedControl LsOuterBindControl { get; }

        public MappedControl RsOuterBindControl { get; }

        public event EventHandler SelectedIndexChanged;

        public void UpdateMappingDevType(OutContType devType)
        {
            foreach (var mapped in Mappings) mapped.DevType = devType;

            foreach (var mapped in extraControls) mapped.DevType = devType;
        }

        public void UpdateMappings()
        {
            foreach (var mapped in Mappings) mapped.UpdateMappingName();

            foreach (var mapped in extraControls) mapped.UpdateMappingName();
        }

        public event PropertyChangedEventHandler PropertyChanged;

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
            DS4Controls control,
            bool initMap = false
        )
        {
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

        public DS4Controls Control { get; }

        public DS4ControlSettings Setting { get; }

        public string ControlName => Control.ToDisplayName();

        public string MappingName { get; private set; }

        public OutContType DevType { get; set; }

        public string ShiftMappingName { get; set; }

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
            var extra = Control >= DS4Controls.GyroXPos && Control <= DS4Controls.SwipeDown;
            var actionType = !shift ? Setting.ControlActionType : Setting.ShiftActionType;
            if (actionType != DS4ControlSettings.ActionType.Default)
            {
                if (actionType == DS4ControlSettings.ActionType.Key)
                {
                    //return (Keys)int.Parse(action.ToString()) + (sc ? " (" + Properties.Resources.ScanCode + ")" : "");
                    temp = KeyInterop.KeyFromVirtualKey(action.ActionKey) + (sc ? " (" + Resources.ScanCode + ")" : "");
                }
                else if (actionType == DS4ControlSettings.ActionType.Macro)
                {
                    temp = Resources.Macro + (sc ? " (" + Resources.ScanCode + ")" : "");
                }
                else if (actionType == DS4ControlSettings.ActionType.Button)
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

        public bool HasShiftAction => Setting.ShiftActionType != DS4ControlSettings.ActionType.Default;

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

        public event PropertyChangedEventHandler PropertyChanged;

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