using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using DS4Windows;
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

        //private int devIndex;

        private readonly MappedControl gyroSwipeDownControl;

        private readonly MappedControl gyroSwipeLeftControl;
        private readonly MappedControl gyroSwipeRightControl;
        private readonly MappedControl gyroSwipeUpControl;

        private int selectedIndex = -1;

        public MappingListViewModel(int devIndex, OutContType devType)
        {
            Mappings.Add(new MappedControl(devIndex, DS4Controls.Cross, "Cross", devType));
            Mappings.Add(new MappedControl(devIndex, DS4Controls.Circle, "Circle", devType));
            Mappings.Add(new MappedControl(devIndex, DS4Controls.Square, "Square", devType));
            Mappings.Add(new MappedControl(devIndex, DS4Controls.Triangle, "Triangle", devType));
            Mappings.Add(new MappedControl(devIndex, DS4Controls.Options, "Options", devType));
            Mappings.Add(new MappedControl(devIndex, DS4Controls.Share, "Share", devType));
            Mappings.Add(new MappedControl(devIndex, DS4Controls.DpadUp, "Up", devType));
            Mappings.Add(new MappedControl(devIndex, DS4Controls.DpadDown, "Down", devType));
            Mappings.Add(new MappedControl(devIndex, DS4Controls.DpadLeft, "Left", devType));
            Mappings.Add(new MappedControl(devIndex, DS4Controls.DpadRight, "Right", devType));
            Mappings.Add(new MappedControl(devIndex, DS4Controls.PS, "PS", devType));
            Mappings.Add(new MappedControl(devIndex, DS4Controls.Mute, "Mute", devType));
            Mappings.Add(new MappedControl(devIndex, DS4Controls.L1, "L1", devType));
            Mappings.Add(new MappedControl(devIndex, DS4Controls.R1, "R1", devType));
            Mappings.Add(new MappedControl(devIndex, DS4Controls.L2, "L2", devType));
            Mappings.Add(new MappedControl(devIndex, DS4Controls.R2, "R2", devType));
            Mappings.Add(new MappedControl(devIndex, DS4Controls.L3, "L3", devType));
            Mappings.Add(new MappedControl(devIndex, DS4Controls.R3, "R3", devType));
            Mappings.Add(new MappedControl(devIndex, DS4Controls.Capture, "Capture", devType));
            Mappings.Add(new MappedControl(devIndex, DS4Controls.SideL, "Side L", devType));
            Mappings.Add(new MappedControl(devIndex, DS4Controls.SideR, "Side R", devType));

            Mappings.Add(new MappedControl(devIndex, DS4Controls.TouchLeft, "Left Touch", devType));
            Mappings.Add(new MappedControl(devIndex, DS4Controls.TouchRight, "Right Touch", devType));
            Mappings.Add(new MappedControl(devIndex, DS4Controls.TouchMulti, "Multitouch", devType));
            Mappings.Add(new MappedControl(devIndex, DS4Controls.TouchUpper, "Upper Touch", devType));

            Mappings.Add(new MappedControl(devIndex, DS4Controls.LYNeg, "LS Up", devType));
            Mappings.Add(new MappedControl(devIndex, DS4Controls.LYPos, "LS Down", devType));
            Mappings.Add(new MappedControl(devIndex, DS4Controls.LXNeg, "LS Left", devType));
            Mappings.Add(new MappedControl(devIndex, DS4Controls.LXPos, "LS Right", devType));

            Mappings.Add(new MappedControl(devIndex, DS4Controls.RYNeg, "RS Up", devType));
            Mappings.Add(new MappedControl(devIndex, DS4Controls.RYPos, "RS Down", devType));
            Mappings.Add(new MappedControl(devIndex, DS4Controls.RXNeg, "RS Left", devType));
            Mappings.Add(new MappedControl(devIndex, DS4Controls.RXPos, "RS Right", devType));

            Mappings.Add(new MappedControl(devIndex, DS4Controls.GyroZNeg, "Tilt Up", devType));
            Mappings.Add(new MappedControl(devIndex, DS4Controls.GyroZPos, "Tilt Down", devType));
            Mappings.Add(new MappedControl(devIndex, DS4Controls.GyroXPos, "Tilt Left", devType));
            Mappings.Add(new MappedControl(devIndex, DS4Controls.GyroXNeg, "Tilt Right", devType));

            Mappings.Add(new MappedControl(devIndex, DS4Controls.SwipeUp, "Swipe Up", devType));
            Mappings.Add(new MappedControl(devIndex, DS4Controls.SwipeDown, "Swipe Down", devType));
            Mappings.Add(new MappedControl(devIndex, DS4Controls.SwipeLeft, "Swipe Left", devType));
            Mappings.Add(new MappedControl(devIndex, DS4Controls.SwipeRight, "Swipe Right", devType));

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
            LsOuterBindControl = new MappedControl(devIndex, DS4Controls.LSOuter, "LS Outer", devType);
            RsOuterBindControl = new MappedControl(devIndex, DS4Controls.RSOuter, "RS Outer", devType);

            L2FullPullControl = new MappedControl(devIndex, DS4Controls.L2FullPull, "L2 Full Pull", devType);
            R2FullPullControl = new MappedControl(devIndex, DS4Controls.R2FullPull, "R2 Full Pull", devType);

            gyroSwipeLeftControl = new MappedControl(devIndex, DS4Controls.GyroSwipeLeft, "Gyro Swipe Left", devType);
            gyroSwipeRightControl =
                new MappedControl(devIndex, DS4Controls.GyroSwipeRight, "Gyro Swipe Right", devType);
            gyroSwipeUpControl = new MappedControl(devIndex, DS4Controls.GyroSwipeUp, "Gyro Swipe Up", devType);
            gyroSwipeDownControl = new MappedControl(devIndex, DS4Controls.GyroSwipeDown, "Gyro Swipe Down", devType);

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
        ///     DS4Controls -> Int index map. Store approriate list index for a stored MappedControl instance
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
        private OutContType devType;

        public MappedControl(
            int devIndex,
            DS4Controls control,
            string controlName,
            OutContType devType,
            bool initMap = false
        )
        {
            DevIndex = devIndex;
            this.devType = devType;
            Control = control;
            ControlName = controlName;
            Setting = Global.Instance.Config.GetDs4ControllerSetting(devIndex, control);
            //mappingName = "?";
            if (initMap)
            {
                MappingName = GetMappingString();
                if (HasShiftAction)
                    ShiftMappingName = ShiftTrigger(Setting.ShiftTrigger) + " -> " + GetMappingString(true);
            }

            DevTypeChanged += MappedControl_DevTypeChanged;
        }

        public int DevIndex { get; }

        public DS4Controls Control { get; }

        public DS4ControlSettings Setting { get; }

        public string ControlName { get; }

        public string MappingName { get; private set; }

        public OutContType DevType
        {
            get => devType;
            set
            {
                devType = value;
                DevTypeChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public string ShiftMappingName { get; set; }

        public event EventHandler DevTypeChanged;

        public event EventHandler MappingNameChanged;

        private void MappedControl_DevTypeChanged(object sender, EventArgs e)
        {
            UpdateMappingName();
        }

        public void UpdateMappingName()
        {
            MappingName = GetMappingString();
            if (HasShiftAction)
                ShiftMappingName = ShiftTrigger(Setting.ShiftTrigger) + " -> " + GetMappingString(true);
            else
                ShiftMappingName = "";

            MappingNameChanged?.Invoke(this, EventArgs.Empty);
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
                    tag = Global.GetX360ControlString(action.ActionButton, devType);
                    temp = tag;
                }
                else
                {
                    temp = Global.GetX360ControlString(Global.DefaultButtonMapping[(int)Control], devType);
                }
            }
            else if (!extra && !shift)
            {
                var tempOutControl = Global.DefaultButtonMapping[(int)Control];
                if (tempOutControl != X360Controls.None) temp = Global.GetX360ControlString(tempOutControl, devType);
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
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}