using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DS4Windows;

namespace DS4WinWPF.DS4Control
{
    public abstract class PresetOption
    {
        protected readonly ControlService rootHub;

        public PresetOption(ControlService service)
        {
            rootHub = service;
        }

        public enum OutputContChoice : ushort
        {
            None,
            Xbox360,
            DualShock4,
        }

        protected string name;
        protected string description;
        protected bool outputControllerChoice;
        protected OutputContChoice outputCont;

        public string Name { get => name; }
        public string Description { get => description; }
        public bool OutputControllerChoice { get => outputControllerChoice; }
        public OutputContChoice OutputCont
        {
            get => outputCont;
            set => outputCont = value;
        }

        public abstract void ApplyPreset(int idx);
    }

    public class GamepadPreset : PresetOption
    {
        public GamepadPreset(ControlService service) : base(service)
        {
            name = Translations.Strings.GamepadPresetName;
            description = Translations.Strings.GamepadPresetDescription;
            outputControllerChoice = true;
            outputCont = OutputContChoice.Xbox360;
        }

        public override void ApplyPreset(int idx)
        {
            if (outputCont == OutputContChoice.Xbox360)
            {
                Global.Instance.LoadBlankDevProfile(idx, false, rootHub, false);
            }
            else if (outputCont == OutputContChoice.DualShock4)
            {
                Global.Instance.LoadBlankDS4Profile(idx, false, rootHub, false);
            }
        }
    }

    public class GamepadGyroCamera : PresetOption
    {
        public GamepadGyroCamera(ControlService service) : base(service)
        {
            name = Translations.Strings.GamepadGyroCameraName;
            description = Translations.Strings.GamepadGyroCameraDescription;
            outputControllerChoice = true;
            outputCont = OutputContChoice.Xbox360;
        }

        public override void ApplyPreset(int idx)
        {
            if (outputCont == OutputContChoice.Xbox360)
            {
                Global.Instance.LoadDefaultGamepadGyroProfile(idx, false, rootHub, false);
            }
            else if (outputCont == OutputContChoice.DualShock4)
            {
                Global.Instance.LoadDefaultDS4GamepadGyroProfile(idx, false, rootHub, false);
            }
        }
    }

    public class MixedPreset : PresetOption
    {
        public MixedPreset(ControlService service) : base(service)
        {
            name = Translations.Strings.MixedPresetName;
            description = Translations.Strings.MixedPresetDescription;
            outputControllerChoice = true;
            outputCont = OutputContChoice.Xbox360;
        }

        public override void ApplyPreset(int idx)
        {
            if (outputCont == OutputContChoice.Xbox360)
            {
                Global.Instance.LoadDefaultMixedControlsProfile(idx, false, rootHub, false);
            }
            else if (outputCont == OutputContChoice.DualShock4)
            {
               Global.Instance.LoadDefaultMixedControlsProfile(idx, false, rootHub, false);
            }
        }
    }

    public class MixedGyroMousePreset : PresetOption
    {
        public MixedGyroMousePreset(ControlService service) : base(service)
        {
            name = Translations.Strings.MixedGyroMousePresetName;
            description = Translations.Strings.MixedGyroMousePresetDescription;
            outputControllerChoice = true;
            outputCont = OutputContChoice.Xbox360;
        }

        public override void ApplyPreset(int idx)
        {
            if (outputCont == OutputContChoice.Xbox360)
            {
               Global.Instance.LoadDefaultMixedGyroMouseProfile(idx, false, rootHub, false);
            }
            else if (outputCont == OutputContChoice.DualShock4)
            {
               Global.Instance.LoadDefaultDS4MixedGyroMouseProfile(idx, false, rootHub, false);
            }
        }
    }

    public class KBMPreset : PresetOption
    {
        public KBMPreset(ControlService service) : base(service)
        {
            name = Translations.Strings.KBMPresetName;
            description = Translations.Strings.KBMPresetDescription;
        }

        public override void ApplyPreset(int idx)
        {
           Global.Instance.LoadDefaultKBMProfile(idx, false, rootHub, false);
        }
    }

    public class KBMGyroMouse : PresetOption
    {
        public KBMGyroMouse(ControlService service) : base(service)
        {
            name = Translations.Strings.KBMGyroMouseName;
            description = Translations.Strings.KBMGyroMouseDescription;
        }

        public override void ApplyPreset(int idx)
        {
           Global.Instance.LoadDefaultKBMGyroMouseProfile(idx, false, rootHub, false);
        }
    }
}
