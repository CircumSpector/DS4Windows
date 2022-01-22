using System;
using System.Collections.Generic;
using System.Linq;
using DS4Windows;
using DS4Windows.Shared.Common.Legacy;
using DS4Windows.Shared.Common.Types;
using DS4WinWPF.DS4Forms.ViewModels.Util;

namespace DS4WinWPF.DS4Forms.ViewModels.SpecialActions
{
    public class MacroViewModel : NotifyDataErrorBase
    {
        private string macrostring;

        public bool UseScanCode { get; set; }

        public bool RunTriggerRelease { get; set; }

        public bool SyncRun { get; set; }

        public bool KeepKeyState { get; set; }

        public bool RepeatHeld { get; set; }

        public List<int> Macro { get; set; } = new(1);

        public string Macrostring
        {
            get => macrostring;
            set
            {
                macrostring = value;
                MacrostringChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public event EventHandler MacrostringChanged;

        public void LoadAction(SpecialActionV3 action)
        {
            Macro = action.Macro;
            if (action.Macro.Count > 0)
            {
                var macroParser = new MacroParser(action.Macro.ToArray());
                macroParser.LoadMacro();
                macrostring = string.Join(", ", macroParser.GetMacroStrings());
            }

            UseScanCode = action.KeyType.HasFlag(DS4KeyType.ScanCode);
            RunTriggerRelease = action.PressRelease;
            SyncRun = action.Synchronized;
            KeepKeyState = action.KeepKeyState;
            RepeatHeld = action.KeyType.HasFlag(DS4KeyType.RepeatMacro);
        }

        public DS4ControlSettingsV3 PrepareSettings()
        {
            var settings = new DS4ControlSettingsV3(DS4ControlItem.None);
            settings.ActionData.ActionMacro = Macro.ToArray();
            settings.ControlActionType = DS4ControlSettingsV3.ActionType.Macro;
            settings.KeyType = DS4KeyType.Macro;
            if (RepeatHeld) settings.KeyType |= DS4KeyType.RepeatMacro;

            return settings;
        }

        public void SaveAction(SpecialActionV3 action, bool edit = false)
        {
            var extrasList = new List<string>();
            extrasList.Add(UseScanCode ? "Scan Code" : null);
            extrasList.Add(RunTriggerRelease ? "RunOnRelease" : null);
            extrasList.Add(SyncRun ? "Sync" : null);
            extrasList.Add(KeepKeyState ? "KeepKeyState" : null);
            extrasList.Add(RepeatHeld ? "Repeat" : null);
            Global.Instance.SaveAction(action.Name, action.Controls, 1, string.Join("/", Macro), edit,
                string.Join("/", extrasList.Where(s => !string.IsNullOrEmpty(s))));
        }

        public void UpdateMacroString()
        {
            var temp = "";
            if (Macro.Count > 0)
            {
                var macroParser = new MacroParser(Macro.ToArray());
                macroParser.LoadMacro();
                temp = string.Join(", ", macroParser.GetMacroStrings());
            }

            Macrostring = temp;
        }

        public override bool IsValid(SpecialActionV3 action)
        {
            ClearOldErrors();

            var valid = true;
            var macroErrors = new List<string>();

            if (Macro.Count == 0)
            {
                valid = false;
                macroErrors.Add("No macro defined");
                errors["Macro"] = macroErrors;
                RaiseErrorsChanged("Macro");
            }

            return valid;
        }

        public override void ClearOldErrors()
        {
            if (errors.Count > 0)
            {
                errors.Clear();
                RaiseErrorsChanged("Macro");
            }
        }
    }
}