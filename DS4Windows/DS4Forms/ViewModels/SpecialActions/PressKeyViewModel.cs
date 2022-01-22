using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using DS4Windows;
using DS4Windows.Shared.Common.Types;
using DS4WinWPF.DS4Forms.ViewModels.Util;

namespace DS4WinWPF.DS4Forms.ViewModels.SpecialActions
{
    public class PressKeyViewModel : NotifyDataErrorBase
    {
        private string describeText;
        private int value;
        public bool IsToggle => (KeyType & DS4KeyType.Toggle) != 0;

        public Visibility ShowToggleControls =>
            (KeyType & DS4KeyType.Toggle) != 0 ? Visibility.Visible : Visibility.Collapsed;

        public string DescribeText
        {
            get
            {
                var result = "Select a Key";
                if (!string.IsNullOrEmpty(describeText)) result = describeText;
                ;

                return result;
            }
        }

        public DS4KeyType KeyType { get; set; }

        public int Value
        {
            get => value;
            set => this.value = value;
        }

        public int PressReleaseIndex { get; set; }
        public bool NormalTrigger { get; set; } = true;

        public bool UnloadError => errors.TryGetValue("UnloadError", out _);

        public event EventHandler IsToggleChanged;
        public event EventHandler ShowToggleControlsChanged;
        public event EventHandler DescribeTextChanged;

        public void LoadAction(SpecialActionV3 action)
        {
            KeyType = action.KeyType;
            if (!string.IsNullOrEmpty(action.UControls)) KeyType |= DS4KeyType.Toggle;

            int.TryParse(action.Details, out value);

            if (action.PressRelease) PressReleaseIndex = 1;

            UpdateDescribeText();
            UpdateToggleControls();
        }

        public void UpdateDescribeText()
        {
            describeText = KeyInterop.KeyFromVirtualKey(value) +
                           (KeyType.HasFlag(DS4KeyType.ScanCode) ? " (SC)" : "") +
                           (KeyType.HasFlag(DS4KeyType.Toggle) ? " (Toggle)" : "");

            DescribeTextChanged?.Invoke(this, EventArgs.Empty);
        }

        public void UpdateToggleControls()
        {
            IsToggleChanged?.Invoke(this, EventArgs.Empty);
            ShowToggleControlsChanged?.Invoke(this, EventArgs.Empty);
        }

        public DS4ControlSettingsV3 PrepareSettings()
        {
            var settings = new DS4ControlSettingsV3(DS4Controls.None);
            settings.ActionData.ActionKey = value;
            settings.KeyType = KeyType;
            settings.ControlActionType = DS4ControlSettingsV3.ActionType.Key;
            return settings;
        }

        public void ReadSettings(DS4ControlSettingsV3 settings)
        {
            value = settings.ActionData.ActionKey;
            KeyType = settings.KeyType;
        }

        public void SaveAction(SpecialActionV3 action, bool edit = false)
        {
            string uaction = null;
            if (KeyType.HasFlag(DS4KeyType.Toggle))
            {
                uaction = "Press";
                if (PressReleaseIndex == 1) uaction = "Release";
            }

            Global.Instance.SaveAction(action.Name, action.Controls, 4,
                $"{value}{(KeyType.HasFlag(DS4KeyType.ScanCode) ? " Scan Code" : "")}", edit,
                !string.IsNullOrEmpty(uaction) ? $"{uaction}\n{action.UControls}" : "");
        }

        public override bool IsValid(SpecialActionV3 action)
        {
            ClearOldErrors();

            var valid = true;
            var valueErrors = new List<string>();
            var toggleErrors = new List<string>();

            if (value == 0)
            {
                valueErrors.Add("No key defined");
                errors["Value"] = valueErrors;
                RaiseErrorsChanged("Value");
            }

            if (KeyType.HasFlag(DS4KeyType.Toggle) && string.IsNullOrEmpty(action.UControls))
            {
                toggleErrors.Add("No unload triggers specified");
                errors["UnloadError"] = toggleErrors;
                RaiseErrorsChanged("UnloadError");
            }

            return valid;
        }

        public override void ClearOldErrors()
        {
            if (errors.Count > 0)
            {
                errors.Clear();
                RaiseErrorsChanged("Value");
                RaiseErrorsChanged("UnloadError");
            }
        }
    }
}