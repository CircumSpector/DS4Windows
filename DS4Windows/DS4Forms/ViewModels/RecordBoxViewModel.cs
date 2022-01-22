using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using DS4Windows;
using DS4Windows.Shared.Common.Legacy;
using DS4Windows.Shared.Common.Types;
using DS4Windows.Shared.Configuration.Profiles.Services;
using DS4WinWPF.DS4Control.IoC.Services;

namespace DS4WinWPF.DS4Forms.ViewModels
{
    public class RecordBoxViewModel
    {
        private readonly object _colLockobj = new();

        private readonly Dictionary<int, bool> ds4InputMap = new();

        private int macroStepIndex;

        /// <summary>
        ///     Cached initial profile mode set for Touchpad.
        ///     Needed to revert output control to Touchpad later
        /// </summary>
        private TouchpadOutMode oldTouchpadMode = TouchpadOutMode.None;


        public RecordBoxViewModel(int deviceNum, DS4ControlSettingsV3 controlSettings, bool shift, bool repeatable = true)
        {
            if (KeydownOverrides == null) CreateKeyDownOverrides();

            DeviceNum = deviceNum;
            Settings = controlSettings;
            Shift = shift;
            if (!shift && Settings.KeyType.HasFlag(DS4KeyType.HoldMacro))
                MacroModeIndex = 1;
            else if (shift && Settings.ShiftKeyType.HasFlag(DS4KeyType.HoldMacro)) MacroModeIndex = 1;

            if (!shift && Settings.KeyType.HasFlag(DS4KeyType.ScanCode))
                UseScanCode = true;
            else if (shift && Settings.ShiftKeyType.HasFlag(DS4KeyType.ScanCode)) UseScanCode = true;

            if (!shift && Settings.ControlActionType == DS4ControlSettingsV3.ActionType.Macro)
                LoadMacro();
            else if (shift && Settings.ShiftActionType == DS4ControlSettingsV3.ActionType.Macro) LoadMacro();

            Repeatable = repeatable;

            BindingOperations.EnableCollectionSynchronization(MacroSteps, _colLockobj);

            // By default RECORD button appends new steps. User must select (click) an existing step to insert new steps in front of the selected step
            MacroStepIndex = -1;

            MacroStepItem.CacheImgLocations();

            // Temporarily use Passthru mode for Touchpad. Store old TouchOutMode.
            // Don't conflict Touchpad Click with default output Mouse button controls
            oldTouchpadMode = ProfilesService.Instance.ActiveProfiles.ElementAt(deviceNum).TouchOutMode;
            ProfilesService.Instance.ActiveProfiles.ElementAt(deviceNum).TouchOutMode = TouchpadOutMode.Passthru;
        }

        public int DeviceNum { get; }

        public DS4ControlSettingsV3 Settings { get; }

        public bool Shift { get; }

        public bool RecordDelays { get; set; }

        public int MacroModeIndex { get; set; }

        public bool Recording { get; set; }

        public bool ToggleLightbar { get; set; }

        public bool ToggleRumble { get; set; }

        public ObservableCollection<MacroStepItem> MacroSteps { get; } = new();

        public int MacroStepIndex
        {
            get => macroStepIndex;
            set
            {
                if (macroStepIndex == value) return;
                macroStepIndex = value;
                MacroStepIndexChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public Stopwatch Sw { get; } = new();

        public bool Toggle4thMouse { get; set; }

        public bool Toggle5thMouse { get; set; }

        public int AppendIndex { get; set; } = -1;
        public int EditMacroIndex { get; set; } = -1;

        /// <summary>
        ///     (Output value, active bool)
        /// </summary>
        public Dictionary<int, bool> KeysdownMap { get; } = new();

        public bool UseScanCode { get; set; }

        public static HashSet<int> KeydownOverrides { get; private set; }

        public bool Repeatable { get; }

        public event EventHandler MacroStepIndexChanged;

        private void CreateKeyDownOverrides()
        {
            KeydownOverrides = new HashSet<int>
            {
                44
            };
        }

        public void LoadMacro()
        {
            int[] macro;
            if (!Shift)
                macro = Settings.ActionData.ActionMacro;
            else
                macro = Settings.ShiftAction.ActionMacro;

            var macroParser = new MacroParser(macro);
            macroParser.LoadMacro();
            foreach (var step in macroParser.MacroSteps)
            {
                var item = new MacroStepItem(step);
                MacroSteps.Add(item);
            }
        }

        public void ExportMacro()
        {
            var outmac = new int[MacroSteps.Count];
            var index = 0;
            foreach (var step in MacroSteps)
            {
                outmac[index] = step.Step.Value;
                index++;
            }

            if (!Shift)
            {
                Settings.ActionData.ActionMacro = outmac;
                Settings.ControlActionType = DS4ControlSettingsV3.ActionType.Macro;
                Settings.KeyType = DS4KeyType.Macro;
                if (MacroModeIndex == 1) Settings.KeyType |= DS4KeyType.HoldMacro;
                if (UseScanCode) Settings.KeyType |= DS4KeyType.ScanCode;
            }
            else
            {
                Settings.ShiftAction.ActionMacro = outmac;
                Settings.ShiftActionType = DS4ControlSettingsV3.ActionType.Macro;
                Settings.ShiftKeyType = DS4KeyType.Macro;
                if (MacroModeIndex == 1) Settings.ShiftKeyType |= DS4KeyType.HoldMacro;
                if (UseScanCode) Settings.ShiftKeyType |= DS4KeyType.ScanCode;
            }
        }

        public void WriteCycleProgramsPreset()
        {
            var step = new MacroStep(18, KeyInterop.KeyFromVirtualKey(18).ToString(),
                MacroStep.StepType.ActDown, MacroStep.StepOutput.Key);
            MacroSteps.Add(new MacroStepItem(step));

            step = new MacroStep(9, KeyInterop.KeyFromVirtualKey(9).ToString(),
                MacroStep.StepType.ActDown, MacroStep.StepOutput.Key);
            MacroSteps.Add(new MacroStepItem(step));

            step = new MacroStep(9, KeyInterop.KeyFromVirtualKey(9).ToString(),
                MacroStep.StepType.ActUp, MacroStep.StepOutput.Key);
            MacroSteps.Add(new MacroStepItem(step));

            step = new MacroStep(18, KeyInterop.KeyFromVirtualKey(18).ToString(),
                MacroStep.StepType.ActUp, MacroStep.StepOutput.Key);
            MacroSteps.Add(new MacroStepItem(step));

            step = new MacroStep(1300, "Wait 1000ms",
                MacroStep.StepType.Wait, MacroStep.StepOutput.None);
            MacroSteps.Add(new MacroStepItem(step));
        }

        public void LoadPresetFromFile(string filepath)
        {
            var macs = File.ReadAllText(filepath).Split('/');
            var tmpmacro = new List<int>();
            int temp;
            foreach (var s in macs)
                if (int.TryParse(s, out temp))
                    tmpmacro.Add(temp);

            var macroParser = new MacroParser(tmpmacro.ToArray());
            macroParser.LoadMacro();
            foreach (var step in macroParser.MacroSteps)
            {
                var item = new MacroStepItem(step);
                MacroSteps.Add(item);
            }
        }

        public void SavePreset(string filepath)
        {
            var outmac = new int[MacroSteps.Count];
            var index = 0;
            foreach (var step in MacroSteps)
            {
                outmac[index] = step.Step.Value;
                index++;
            }

            var macro = string.Join("/", outmac);
            var sw = new StreamWriter(filepath);
            sw.Write(macro);
            sw.Close();
        }

        public void AddMacroStep(MacroStep step, bool ignoreDelay = false)
        {
            if (RecordDelays && MacroSteps.Count > 0 && !ignoreDelay)
            {
                var elapsed = (int) Sw.ElapsedMilliseconds + 300;
                var waitstep = new MacroStep(elapsed, $"Wait {elapsed - 300}ms",
                    MacroStep.StepType.Wait, MacroStep.StepOutput.None);
                var waititem = new MacroStepItem(waitstep);
                if (AppendIndex == -1)
                {
                    MacroSteps.Add(waititem);
                }
                else
                {
                    MacroSteps.Insert(AppendIndex, waititem);
                    AppendIndex++;
                }
            }

            Sw.Restart();
            var item = new MacroStepItem(step);
            if (AppendIndex == -1)
            {
                MacroSteps.Add(item);
            }
            else
            {
                MacroSteps.Insert(AppendIndex, item);
                AppendIndex++;
            }
        }

        public void InsertMacroStep(int index, MacroStep step)
        {
            var item = new MacroStepItem(step);
            MacroSteps.Insert(index, item);
        }

        public void StartForcedColor(Color color)
        {
            if (DeviceNum < ControlService.CURRENT_DS4_CONTROLLER_LIMIT)
            {
                var dcolor = new DS4Color(color) {Red = color.R, Green = color.G, Blue = color.B};
                DS4LightBarV3.forcedColor[DeviceNum] = dcolor;
                DS4LightBarV3.forcedFlash[DeviceNum] = 0;
                DS4LightBarV3.forcelight[DeviceNum] = true;
            }
        }

        public void EndForcedColor()
        {
            if (DeviceNum < ControlService.CURRENT_DS4_CONTROLLER_LIMIT)
            {
                DS4LightBarV3.forcedColor[DeviceNum] = new DS4Color(0, 0, 0);
                DS4LightBarV3.forcedFlash[DeviceNum] = 0;
                DS4LightBarV3.forcelight[DeviceNum] = false;
            }
        }

        public void UpdateForcedColor(Color color)
        {
            if (DeviceNum < ControlService.CURRENT_DS4_CONTROLLER_LIMIT)
            {
                var dcolor = new DS4Color(color);
                DS4LightBarV3.forcedColor[DeviceNum] = dcolor;
                DS4LightBarV3.forcedFlash[DeviceNum] = 0;
                DS4LightBarV3.forcelight[DeviceNum] = true;
            }
        }

        public void ProcessDS4Tick()
        {
            if (ControlService.CurrentInstance.DS4Controllers[0] != null)
            {
                var dev = ControlService.CurrentInstance.DS4Controllers[0];
                var cState = dev.GetCurrentStateReference();
                var tp = ControlService.CurrentInstance.touchPad[0];
                for (var dc = DS4ControlItem.LXNeg; dc < DS4ControlItem.Mute; dc++)
                {
                    var macroValue = Global.MacroDs4Values[dc];
                    ds4InputMap.TryGetValue((int) dc, out var isdown);
                    KeysdownMap.TryGetValue(macroValue, out var outputExists);
                    if (!isdown && Mapping.GetBoolMapping(0, dc, cState, null, tp))
                    {
                        var step = new MacroStep(macroValue, MacroParser.macroInputNames[macroValue],
                            MacroStep.StepType.ActDown, MacroStep.StepOutput.Button);
                        AddMacroStep(step);
                        ds4InputMap.Add((int) dc, true);
                        if (!outputExists) KeysdownMap.Add(macroValue, true);
                    }
                    else if (isdown && !Mapping.GetBoolMapping(0, dc, cState, null, tp))
                    {
                        var step = new MacroStep(macroValue, MacroParser.macroInputNames[macroValue],
                            MacroStep.StepType.ActUp, MacroStep.StepOutput.Button);
                        AddMacroStep(step);
                        ds4InputMap.Remove((int) dc);
                        if (outputExists) KeysdownMap.Remove(macroValue);
                    }
                }
            }
        }

        /// <summary>
        ///     Revert any necessary outside
        /// </summary>
        public void RevertControlsSettings()
        {
            ProfilesService.Instance.ActiveProfiles.ElementAt(DeviceNum).TouchOutMode = oldTouchpadMode;
            oldTouchpadMode = TouchpadOutMode.None;
        }
    }

    public class MacroStepItem
    {
        private static string[] imageSources =
        {
            $"/DS4Windows;component/Resources/{(string) Application.Current.FindResource("KeyDownImg")}",
            $"/DS4Windows;component/Resources/{(string) Application.Current.FindResource("KeyUpImg")}",
            $"/DS4Windows;component/Resources/{(string) Application.Current.FindResource("ClockImg")}"
        };

        public MacroStepItem(MacroStep step)
        {
            Step = step;
            Image = imageSources[(int) step.ActType];
        }

        public string Image { get; }

        public MacroStep Step { get; }

        public int DisplayValue
        {
            get
            {
                var result = Step.Value;
                if (Step.ActType == MacroStep.StepType.Wait) result -= 300;

                return result;
            }
            set
            {
                var result = value;
                if (Step.ActType == MacroStep.StepType.Wait) result += 300;

                Step.Value = result;
            }
        }

        public int RumbleHeavy
        {
            get
            {
                var result = Step.Value;
                result -= 1000000;
                var temp = result.ToString();
                result = int.Parse(temp.Substring(0, 3));
                return result;
            }
            set
            {
                var result = Step.Value;
                result -= 1000000;
                var curHeavy = result / 1000;
                var curLight = result - curHeavy * 1000;
                result = curLight + value * 1000 + 1000000;
                Step.Value = result;
            }
        }

        public int RumbleLight
        {
            get
            {
                var result = Step.Value;
                result -= 1000000;
                var temp = result.ToString();
                result = int.Parse(temp.Substring(3, 3));
                return result;
            }
            set
            {
                var result = Step.Value;
                result -= 1000000;
                var curHeavy = result / 1000;
                result = value + curHeavy * 1000 + 1000000;
                Step.Value = result;
            }
        }

        public static void CacheImgLocations()
        {
            imageSources = new[]
            {
                $"/DS4Windows;component/Resources/{(string) Application.Current.FindResource("KeyDownImg")}",
                $"/DS4Windows;component/Resources/{(string) Application.Current.FindResource("KeyUpImg")}",
                $"/DS4Windows;component/Resources/{(string) Application.Current.FindResource("ClockImg")}"
            };
        }

        public void UpdateLightbarValue(Color color)
        {
            Step.Value = 1000000000 + color.R * 1000000 + color.G * 1000 + color.B;
        }

        public Color LightbarColorValue()
        {
            var temp = Step.Value - 1000000000;
            var r = temp / 1000000;
            temp -= r * 1000000;
            var g = temp / 1000;
            temp -= g * 1000;
            var b = temp;
            return new Color {A = 255, R = (byte) r, G = (byte) g, B = (byte) b};
        }
    }
}