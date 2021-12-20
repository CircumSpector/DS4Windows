using DS4Windows;
using DS4WinWPF.Translations;

namespace DS4WinWPF.DS4Control
{
    public abstract class PresetOption
    {
        public enum OutputContChoice : ushort
        {
            None,
            Xbox360,
            DualShock4
        }

        protected readonly ControlService rootHub;
        protected string description;

        protected string name;
        protected OutputContChoice outputCont;
        protected bool outputControllerChoice;

        public PresetOption(ControlService service)
        {
            rootHub = service;
        }

        public string Name => name;
        public string Description => description;
        public bool OutputControllerChoice => outputControllerChoice;

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
            name = Strings.GamepadPresetName;
            description = Strings.GamepadPresetDescription;
            outputControllerChoice = true;
            outputCont = OutputContChoice.Xbox360;
        }

        public override void ApplyPreset(int idx)
        {
            if (outputCont == OutputContChoice.Xbox360)
                Global.Instance.LoadBlankDevProfile(idx, false, rootHub, false);
            else if (outputCont == OutputContChoice.DualShock4)
                Global.Instance.LoadBlankDS4Profile(idx, false, rootHub, false);
        }
    }

    public class GamepadGyroCamera : PresetOption
    {
        public GamepadGyroCamera(ControlService service) : base(service)
        {
            name = Strings.GamepadGyroCameraName;
            description = Strings.GamepadGyroCameraDescription;
            outputControllerChoice = true;
            outputCont = OutputContChoice.Xbox360;
        }

        public override void ApplyPreset(int idx)
        {
            if (outputCont == OutputContChoice.Xbox360)
                Global.Instance.LoadDefaultGamepadGyroProfile(idx, false, rootHub, false);
            else if (outputCont == OutputContChoice.DualShock4)
                Global.Instance.LoadDefaultDS4GamepadGyroProfile(idx, false, rootHub, false);
        }
    }

    public class MixedPreset : PresetOption
    {
        public MixedPreset(ControlService service) : base(service)
        {
            name = Strings.MixedPresetName;
            description = Strings.MixedPresetDescription;
            outputControllerChoice = true;
            outputCont = OutputContChoice.Xbox360;
        }

        public override void ApplyPreset(int idx)
        {
            if (outputCont == OutputContChoice.Xbox360)
                Global.Instance.LoadDefaultMixedControlsProfile(idx, false, rootHub, false);
            else if (outputCont == OutputContChoice.DualShock4)
                Global.Instance.LoadDefaultMixedControlsProfile(idx, false, rootHub, false);
        }
    }

    public class MixedGyroMousePreset : PresetOption
    {
        public MixedGyroMousePreset(ControlService service) : base(service)
        {
            name = Strings.MixedGyroMousePresetName;
            description = Strings.MixedGyroMousePresetDescription;
            outputControllerChoice = true;
            outputCont = OutputContChoice.Xbox360;
        }

        public override void ApplyPreset(int idx)
        {
            if (outputCont == OutputContChoice.Xbox360)
                Global.Instance.LoadDefaultMixedGyroMouseProfile(idx, false, rootHub, false);
            else if (outputCont == OutputContChoice.DualShock4)
                Global.Instance.LoadDefaultDS4MixedGyroMouseProfile(idx, false, rootHub, false);
        }
    }

    public class KBMPreset : PresetOption
    {
        public KBMPreset(ControlService service) : base(service)
        {
            name = Strings.KBMPresetName;
            description = Strings.KBMPresetDescription;
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
            name = Strings.KBMGyroMouseName;
            description = Strings.KBMGyroMouseDescription;
        }

        public override void ApplyPreset(int idx)
        {
            Global.Instance.LoadDefaultKBMGyroMouseProfile(idx, false, rootHub, false);
        }
    }
}