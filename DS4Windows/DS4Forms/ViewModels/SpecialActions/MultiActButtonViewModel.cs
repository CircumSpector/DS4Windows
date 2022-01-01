using System;
using System.Collections.Generic;
using DS4Windows;
using DS4WinWPF.DS4Forms.ViewModels.Util;
using DS4WinWPF.Properties;

namespace DS4WinWPF.DS4Forms.ViewModels.SpecialActions
{
    public class MultiActButtonViewModel : NotifyDataErrorBase
    {
        private readonly List<int>[] loadAccessArray;

        public MultiActButtonViewModel()
        {
            loadAccessArray = new List<int>[3] {TapMacro, HoldMacro, DoubleTapMacro};
        }

        public List<int> TapMacro { get; } = new();

        public List<int> HoldMacro { get; } = new();

        public List<int> DoubleTapMacro { get; } = new();

        public string TapMacroText
        {
            get
            {
                var result = Resources.SelectMacro;
                if (TapMacro.Count > 0) result = Resources.MacroRecorded;

                return result;
            }
        }

        public string HoldMacroText
        {
            get
            {
                var result = Resources.SelectMacro;
                if (HoldMacro.Count > 0) result = Resources.MacroRecorded;

                return result;
            }
        }

        public string DoubleTapMacroText
        {
            get
            {
                var result = Resources.SelectMacro;
                if (DoubleTapMacro.Count > 0) result = Resources.MacroRecorded;

                return result;
            }
        }

        public event EventHandler TapMacroTextChanged;
        public event EventHandler HoldMacroTextChanged;
        public event EventHandler DoubleTapMacroTextChanged;

        public void LoadAction(SpecialActionV3 action)
        {
            var dets = action.Details.Split(',');
            for (var i = 0; i < 3; i++)
            {
                var macs = dets[i].Split('/');
                foreach (var s in macs)
                    if (int.TryParse(s, out var v))
                        loadAccessArray[i].Add(v);
            }
        }

        public void UpdateTapDisplayText()
        {
            TapMacroTextChanged?.Invoke(this, EventArgs.Empty);
        }

        public void UpdateHoldDisplayText()
        {
            HoldMacroTextChanged?.Invoke(this, EventArgs.Empty);
        }

        public void UpdateDoubleTapDisplayText()
        {
            DoubleTapMacroTextChanged?.Invoke(this, EventArgs.Empty);
        }

        public DS4ControlSettingsV3 PrepareTapSettings()
        {
            var settings = new DS4ControlSettingsV3(DS4Controls.None);
            settings.ActionData.ActionMacro = TapMacro.ToArray();
            settings.ControlActionType = DS4ControlSettingsV3.ActionType.Macro;
            settings.KeyType = DS4KeyType.Macro;
            return settings;
        }

        public DS4ControlSettingsV3 PrepareHoldSettings()
        {
            var settings = new DS4ControlSettingsV3(DS4Controls.None);
            settings.ActionData.ActionMacro = HoldMacro.ToArray();
            settings.ControlActionType = DS4ControlSettingsV3.ActionType.Macro;
            settings.KeyType = DS4KeyType.Macro;
            return settings;
        }

        public DS4ControlSettingsV3 PrepareDoubleTapSettings()
        {
            var settings = new DS4ControlSettingsV3(DS4Controls.None);
            settings.ActionData.ActionMacro = DoubleTapMacro.ToArray();
            settings.ControlActionType = DS4ControlSettingsV3.ActionType.Macro;
            settings.KeyType = DS4KeyType.Macro;
            return settings;
        }

        public void SaveAction(SpecialActionV3 action, bool edit = false)
        {
            var details = string.Join("/", TapMacro) + "," +
                          string.Join("/", HoldMacro) + "," +
                          string.Join("/", DoubleTapMacro);
            Global.Instance.SaveAction(action.Name, action.Controls, 7, details, edit);
        }

        public override bool IsValid(SpecialActionV3 action)
        {
            ClearOldErrors();

            var valid = true;
            var tapMacroErrors = new List<string>();
            var holdMacroErrors = new List<string>();
            var doubleTapMacroErrors = new List<string>();

            if (TapMacro.Count == 0)
            {
                tapMacroErrors.Add("No tap macro defined");
                errors["TapMacro"] = tapMacroErrors;
                RaiseErrorsChanged("TapMacro");
            }

            if (HoldMacro.Count == 0)
            {
                holdMacroErrors.Add("No hold macro defined");
                errors["HoldMacro"] = holdMacroErrors;
                RaiseErrorsChanged("HoldMacro");
            }

            if (DoubleTapMacro.Count == 0)
            {
                doubleTapMacroErrors.Add("No double tap macro defined");
                errors["DoubleTapMacro"] = doubleTapMacroErrors;
                RaiseErrorsChanged("DoubleTapMacro");
            }

            return valid;
        }

        public override void ClearOldErrors()
        {
            if (errors.Count > 0)
            {
                errors.Clear();
                RaiseErrorsChanged("TapMacro");
                RaiseErrorsChanged("HoldMacro");
                RaiseErrorsChanged("DoubleTapMacro");
            }
        }
    }
}