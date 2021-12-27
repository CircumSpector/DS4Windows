using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DS4Windows;
using DS4WinWPF.DS4Forms.ViewModels.Util;

namespace DS4WinWPF.DS4Forms.ViewModels.SpecialActions
{
    public class MacroViewModel : NotifyDataErrorBase
    {
        private bool useScanCode;
        private bool runTriggerRelease;
        private bool syncRun;
        private bool keepKeyState;
        private bool repeatHeld;
        private List<int> macro = new List<int>(1);
        private string macrostring;

        public bool UseScanCode { get => useScanCode; set => useScanCode = value; }
        public bool RunTriggerRelease { get => runTriggerRelease; set => runTriggerRelease = value; }
        public bool SyncRun { get => syncRun; set => syncRun = value; }
        public bool KeepKeyState { get => keepKeyState; set => keepKeyState = value; }
        public bool RepeatHeld { get => repeatHeld; set => repeatHeld = value; }
        public List<int> Macro { get => macro; set => macro = value; }
        public string Macrostring { get => macrostring;
            set
            {
                macrostring = value;
                MacrostringChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public event EventHandler MacrostringChanged;

        public void LoadAction(SpecialActionV3 action)
        {
            macro = action.Macro;
            if (action.Macro.Count > 0)
            {
                MacroParser macroParser = new MacroParser(action.Macro.ToArray());
                macroParser.LoadMacro();
                macrostring = string.Join(", ", macroParser.GetMacroStrings());
            }

            useScanCode = action.KeyType.HasFlag(DS4KeyType.ScanCode);
            runTriggerRelease = action.PressRelease;
            syncRun = action.Synchronized;
            keepKeyState = action.KeepKeyState;
            repeatHeld = action.KeyType.HasFlag(DS4KeyType.RepeatMacro);
        }

        public DS4ControlSettings PrepareSettings()
        {
            DS4ControlSettings settings = new DS4ControlSettings(DS4Controls.None);
            settings.ActionData.ActionMacro = macro.ToArray();
            settings.ControlActionType = DS4ControlSettings.ActionType.Macro;
            settings.KeyType = DS4KeyType.Macro;
            if (repeatHeld)
            {
                settings.KeyType |= DS4KeyType.RepeatMacro;
            }

            return settings;
        }

        public void SaveAction(SpecialActionV3 action, bool edit = false)
        {
            List<string> extrasList = new List<string>();
            extrasList.Add(useScanCode ? "Scan Code" : null);
            extrasList.Add(runTriggerRelease ? "RunOnRelease" : null);
            extrasList.Add(syncRun ? "Sync" : null);
            extrasList.Add(keepKeyState ? "KeepKeyState" : null);
            extrasList.Add(repeatHeld ? "Repeat" : null);
            Global.Instance.SaveAction(action.Name, action.Controls, 1, string.Join("/", macro), edit,
                string.Join("/", extrasList.Where(s => !string.IsNullOrEmpty(s))));
        }

        public void UpdateMacroString()
        {
            string temp = "";
            if (macro.Count > 0)
            {
                MacroParser macroParser = new MacroParser(macro.ToArray());
                macroParser.LoadMacro();
                temp = string.Join(", ", macroParser.GetMacroStrings());
            }

            Macrostring = temp;
        }

        public override bool IsValid(SpecialActionV3 action)
        {
            ClearOldErrors();

            bool valid = true;
            List<string> macroErrors = new List<string>();

            if (macro.Count == 0)
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
