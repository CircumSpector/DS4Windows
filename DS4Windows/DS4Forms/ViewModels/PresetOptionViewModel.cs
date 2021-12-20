using System;
using System.Collections.Generic;
using DS4Windows;
using DS4WinWPF.DS4Control;
using DS4WinWPF.DS4Forms.ViewModels.Util;

namespace DS4WinWPF.DS4Forms.ViewModels
{
    public class PresetOptionViewModel
    {
        private int presetIndex;

        public PresetOptionViewModel(ControlService service)
        {
            PresetsList = new List<PresetOption>
            {
                new GamepadPreset(service),
                new GamepadGyroCamera(service),
                new MixedPreset(service),
                new MixedGyroMousePreset(service),
                new KBMPreset(service),
                new KBMGyroMouse(service)
            };

            PresetIndexChanged += PresetOptionViewModel_PresetIndexChanged;
        }

        public int PresetIndex
        {
            get => presetIndex;
            set
            {
                if (presetIndex == value) return;
                presetIndex = value;
                PresetIndexChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public List<PresetOption> PresetsList { get; }

        public string PresetDescription => PresetsList[presetIndex].Description;

        public bool PresetDisplayOutputCont => PresetsList[presetIndex].OutputControllerChoice;

        public PresetOption.OutputContChoice ControllerChoice { get; set; } = PresetOption.OutputContChoice.Xbox360;

        public List<EnumChoiceSelection<PresetOption.OutputContChoice>> OutputChoices { get; } = new()
        {
            new("Xbox 360", PresetOption.OutputContChoice.Xbox360),
            new("DualShock 4", PresetOption.OutputContChoice.DualShock4)
        };

        public event EventHandler PresetIndexChanged;
        public event EventHandler PresetDescriptionChanged;
        public event EventHandler PresetDisplayOutputContChanged;

        private void PresetOptionViewModel_PresetIndexChanged(object sender, EventArgs e)
        {
            PresetDescriptionChanged?.Invoke(this, EventArgs.Empty);
            PresetDisplayOutputContChanged?.Invoke(this, EventArgs.Empty);
        }

        public void ApplyPreset(int index)
        {
            if (presetIndex >= 0)
            {
                var current = PresetsList[presetIndex];
                if (current.OutputControllerChoice &&
                    ControllerChoice != PresetOption.OutputContChoice.None)
                    current.OutputCont = ControllerChoice;

                current.ApplyPreset(index);
            }
        }
    }
}