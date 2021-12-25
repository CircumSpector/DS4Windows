using System;
using System.Windows;
using System.Windows.Media;
using DS4Windows;
using DS4WinWPF.Properties;

namespace DS4WinWPF.DS4Forms.ViewModels
{
    public class BindingWindowViewModel
    {
        public BindingWindowViewModel(int deviceNum, DS4ControlSettings settings)
        {
            DeviceNum = deviceNum;
            Using360Mode = Global.OutDevTypeTemp[deviceNum] == OutContType.X360;
            Settings = settings;
            CurrentOutBind = new OutBinding();
            ShiftOutBind = new OutBinding();
            ShiftOutBind.shiftBind = true;
            PopulateCurrentBinds();
        }

        public bool Using360Mode { get; }

        public int DeviceNum { get; }

        public OutBinding CurrentOutBind { get; }

        public OutBinding ShiftOutBind { get; }

        public OutBinding ActionBinding { get; set; }

        public bool ShowShift { get; set; }

        public bool RumbleActive { get; set; }

        public DS4ControlSettings Settings { get; }

        public void PopulateCurrentBinds()
        {
            var setting = Settings;
            var sc = setting.KeyType.HasFlag(DS4KeyType.ScanCode);
            var toggle = setting.KeyType.HasFlag(DS4KeyType.Toggle);
            CurrentOutBind.input = setting.Control;
            ShiftOutBind.input = setting.Control;
            if (setting.ControlActionType != DS4ControlSettings.ActionType.Default)
                switch (setting.ControlActionType)
                {
                    case DS4ControlSettings.ActionType.Button:
                        CurrentOutBind.outputType = OutBinding.OutType.Button;
                        CurrentOutBind.control = setting.ActionData.ActionButton;
                        break;
                    case DS4ControlSettings.ActionType.Default:
                        CurrentOutBind.outputType = OutBinding.OutType.Default;
                        break;
                    case DS4ControlSettings.ActionType.Key:
                        CurrentOutBind.outputType = OutBinding.OutType.Key;
                        CurrentOutBind.outkey = setting.ActionData.ActionKey;
                        CurrentOutBind.HasScanCode = sc;
                        CurrentOutBind.Toggle = toggle;
                        break;
                    case DS4ControlSettings.ActionType.Macro:
                        CurrentOutBind.outputType = OutBinding.OutType.Macro;
                        CurrentOutBind.macro = setting.ActionData.ActionMacro;
                        CurrentOutBind.macroType = Settings.KeyType;
                        CurrentOutBind.HasScanCode = sc;
                        break;
                }
            else
                CurrentOutBind.outputType = OutBinding.OutType.Default;

            if (!string.IsNullOrEmpty(setting.Extras)) CurrentOutBind.ParseExtras(setting.Extras);

            if (setting.ShiftActionType != DS4ControlSettings.ActionType.Default)
            {
                sc = setting.ShiftKeyType.HasFlag(DS4KeyType.ScanCode);
                toggle = setting.ShiftKeyType.HasFlag(DS4KeyType.Toggle);
                ShiftOutBind.ShiftTrigger = setting.ShiftTrigger;
                switch (setting.ShiftActionType)
                {
                    case DS4ControlSettings.ActionType.Button:
                        ShiftOutBind.outputType = OutBinding.OutType.Button;
                        ShiftOutBind.control = setting.ShiftAction.ActionButton;
                        break;
                    case DS4ControlSettings.ActionType.Default:
                        ShiftOutBind.outputType = OutBinding.OutType.Default;
                        break;
                    case DS4ControlSettings.ActionType.Key:
                        ShiftOutBind.outputType = OutBinding.OutType.Key;
                        ShiftOutBind.outkey = setting.ShiftAction.ActionKey;
                        ShiftOutBind.HasScanCode = sc;
                        ShiftOutBind.Toggle = toggle;
                        break;
                    case DS4ControlSettings.ActionType.Macro:
                        ShiftOutBind.outputType = OutBinding.OutType.Macro;
                        ShiftOutBind.macro = setting.ShiftAction.ActionMacro;
                        ShiftOutBind.macroType = setting.ShiftKeyType;
                        ShiftOutBind.HasScanCode = sc;
                        break;
                }
            }

            if (!string.IsNullOrEmpty(setting.ShiftExtras)) ShiftOutBind.ParseExtras(setting.ShiftExtras);
        }

        public void WriteBinds()
        {
            CurrentOutBind.WriteBind(Settings);
            ShiftOutBind.WriteBind(Settings);
        }

        public void StartForcedColor(Color color)
        {
            if (DeviceNum < ControlService.CURRENT_DS4_CONTROLLER_LIMIT)
            {
                var dcolor = new DS4Color(color);
                DS4LightBar.forcedColor[DeviceNum] = dcolor;
                DS4LightBar.forcedFlash[DeviceNum] = 0;
                DS4LightBar.forcelight[DeviceNum] = true;
            }
        }

        public void EndForcedColor()
        {
            if (DeviceNum < ControlService.CURRENT_DS4_CONTROLLER_LIMIT)
            {
                DS4LightBar.forcedColor[DeviceNum] = new DS4Color(0, 0, 0);
                DS4LightBar.forcedFlash[DeviceNum] = 0;
                DS4LightBar.forcelight[DeviceNum] = false;
            }
        }

        public void UpdateForcedColor(Color color)
        {
            if (DeviceNum < ControlService.CURRENT_DS4_CONTROLLER_LIMIT)
            {
                var dcolor = new DS4Color(color);
                DS4LightBar.forcedColor[DeviceNum] = dcolor;
                DS4LightBar.forcedFlash[DeviceNum] = 0;
                DS4LightBar.forcelight[DeviceNum] = true;
            }
        }
    }

    public class BindAssociation
    {
        public enum OutType : uint
        {
            Default,
            Key,
            Button,
            Macro
        }

        public X360Controls control;
        public int outkey;

        public OutType outputType;

        public bool IsMouse()
        {
            return outputType == OutType.Button && control >= X360Controls.LeftMouse && control < X360Controls.Unbound;
        }

        public static bool IsMouseRange(X360Controls control)
        {
            return control >= X360Controls.LeftMouse && control < X360Controls.Unbound;
        }
    }

    public class OutBinding
    {
        public enum OutType : uint
        {
            Default,
            Key,
            Button,
            Macro
        }

        public X360Controls control;
        private DS4Color extrasColor = new(255, 255, 255);
        private int flashRate;
        private int heavyRumble;

        public DS4Controls input;
        private int lightRumble;
        public int[] macro;
        public DS4KeyType macroType;
        private int mouseSens = 25;
        public int outkey;
        public OutType outputType;
        public bool shiftBind;

        private bool useExtrasColor;

        private bool useMouseSens;

        public OutBinding()
        {
            ExtrasColorRChanged += OutBinding_ExtrasColorRChanged;
            ExtrasColorGChanged += OutBinding_ExtrasColorGChanged;
            ExtrasColorBChanged += OutBinding_ExtrasColorBChanged;
            UseExtrasColorChanged += OutBinding_UseExtrasColorChanged;
        }

        public bool HasScanCode { get; set; }

        public bool Toggle { get; set; }

        public int ShiftTrigger { get; set; }

        public int HeavyRumble
        {
            get => heavyRumble;
            set => heavyRumble = value;
        }

        public int LightRumble
        {
            get => lightRumble;
            set => lightRumble = value;
        }

        public int FlashRate
        {
            get => flashRate;
            set
            {
                flashRate = value;
                FlashRateChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public int MouseSens
        {
            get => mouseSens;
            set
            {
                mouseSens = value;
                MouseSensChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public bool UseMouseSens
        {
            get => useMouseSens;
            set
            {
                useMouseSens = value;
                UseMouseSensChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public bool UseExtrasColor
        {
            get => useExtrasColor;
            set
            {
                useExtrasColor = value;
                UseExtrasColorChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public int ExtrasColorR
        {
            get => extrasColor.Red;
            set
            {
                extrasColor.Red = (byte) value;
                ExtrasColorRChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public string ExtrasColorRString => $"#{extrasColor.Red:X2}FF0000";

        public int ExtrasColorG
        {
            get => extrasColor.Green;
            set
            {
                extrasColor.Green = (byte) value;
                ExtrasColorGChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public string ExtrasColorGString => $"#{extrasColor.Green:X2}00FF00";

        public int ExtrasColorB
        {
            get => extrasColor.Blue;
            set
            {
                extrasColor.Blue = (byte) value;
                ExtrasColorBChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public string ExtrasColorBString => $"#{extrasColor.Blue:X2}0000FF";

        public string ExtrasColorString =>
            $"#FF{extrasColor.Red:X2}{extrasColor.Green:X2}{extrasColor.Blue:X2}";

        public Color ExtrasColorMedia =>
            new()
            {
                A = 255,
                R = extrasColor.Red,
                B = extrasColor.Blue,
                G = extrasColor.Green
            };

        public int ShiftTriggerIndex { get; set; }

        public string DefaultColor => outputType == OutType.Default ? Colors.LimeGreen.ToString() : Application.Current.FindResource("SecondaryColor").ToString();

        public string UnboundColor =>
            outputType == OutType.Button && control == X360Controls.Unbound
                ? Colors.LimeGreen.ToString()
                : Application.Current.FindResource("SecondaryColor").ToString();

        public string DefaultBtnString
        {
            get
            {
                var result = "Default";
                if (shiftBind) result = Resources.FallBack;

                return result;
            }
        }

        public Visibility MacroLbVisible => outputType == OutType.Macro ? Visibility.Visible : Visibility.Hidden;

        public event EventHandler FlashRateChanged;
        public event EventHandler MouseSensChanged;
        public event EventHandler UseMouseSensChanged;
        public event EventHandler UseExtrasColorChanged;
        public event EventHandler ExtrasColorRChanged;
        public event EventHandler ExtrasColorRStringChanged;
        public event EventHandler ExtrasColorGChanged;
        public event EventHandler ExtrasColorGStringChanged;
        public event EventHandler ExtrasColorBChanged;
        public event EventHandler ExtrasColorBStringChanged;
        public event EventHandler ExtrasColorStringChanged;

        private void OutBinding_ExtrasColorBChanged(object sender, EventArgs e)
        {
            ExtrasColorStringChanged?.Invoke(this, EventArgs.Empty);
            ExtrasColorBStringChanged?.Invoke(this, EventArgs.Empty);
        }

        private void OutBinding_ExtrasColorGChanged(object sender, EventArgs e)
        {
            ExtrasColorStringChanged?.Invoke(this, EventArgs.Empty);
            ExtrasColorGStringChanged?.Invoke(this, EventArgs.Empty);
        }

        private void OutBinding_ExtrasColorRChanged(object sender, EventArgs e)
        {
            ExtrasColorStringChanged?.Invoke(this, EventArgs.Empty);
            ExtrasColorRStringChanged?.Invoke(this, EventArgs.Empty);
        }

        private void OutBinding_UseExtrasColorChanged(object sender, EventArgs e)
        {
            if (!useExtrasColor)
            {
                ExtrasColorR = 255;
                ExtrasColorG = 255;
                ExtrasColorB = 255;
            }
        }

        public bool IsShift()
        {
            return shiftBind;
        }

        public bool IsMouse()
        {
            return outputType == OutType.Button && control >= X360Controls.LeftMouse && control < X360Controls.Unbound;
        }

        public static bool IsMouseRange(X360Controls control)
        {
            return control >= X360Controls.LeftMouse && control < X360Controls.Unbound;
        }

        public void ParseExtras(string extras)
        {
            var temp = extras.Split(',');
            if (temp.Length == 9)
            {
                int.TryParse(temp[0], out heavyRumble);
                int.TryParse(temp[1], out lightRumble);
                int.TryParse(temp[2], out var useColor);
                if (useColor == 1)
                {
                    useExtrasColor = true;
                    byte.TryParse(temp[3], out var r);
                    byte.TryParse(temp[4], out var g);
                    byte.TryParse(temp[5], out var b);
                    int.TryParse(temp[6], out flashRate);

                    extrasColor = new DS4Color(r, g, b);
                }
                else
                {
                    useExtrasColor = false;
                    extrasColor.Red = extrasColor.Green = extrasColor.Blue = 255;
                    flashRate = 0;
                }

                int.TryParse(temp[7], out var useM);
                if (useM == 1)
                {
                    useMouseSens = true;
                    int.TryParse(temp[8], out mouseSens);
                }
                else
                {
                    useMouseSens = false;
                    mouseSens = 25;
                }
            }
        }

        public string CompileExtras()
        {
            var result = $"{heavyRumble},{lightRumble},";
            if (useExtrasColor)
                result += $"1,{extrasColor.Red},{extrasColor.Green},{extrasColor.Blue},{flashRate},";
            else
                result += "0,0,0,0,0,";

            if (useMouseSens)
                result += $"1,{mouseSens}";
            else
                result += "0,0";

            return result;
        }

        public bool IsUsingExtras()
        {
            var result = false;
            result = result || heavyRumble != 0;
            result = result || lightRumble != 0;
            result = result || useExtrasColor;
            result = result ||
                     extrasColor.Red != 255 && extrasColor.Green != 255 &&
                     extrasColor.Blue != 255;

            result = result || flashRate != 0;
            result = result || useMouseSens;
            result = result || mouseSens != 25;
            return result;
        }

        public void WriteBind(DS4ControlSettings settings)
        {
            if (!shiftBind)
            {
                settings.KeyType = DS4KeyType.None;

                if (outputType == OutType.Default)
                {
                    settings.ActionData.ActionKey = 0;
                    settings.ControlActionType = DS4ControlSettings.ActionType.Default;
                }
                else if (outputType == OutType.Button)
                {
                    settings.ActionData.ActionButton = control;
                    settings.ControlActionType = DS4ControlSettings.ActionType.Button;
                    if (control == X360Controls.Unbound) settings.KeyType |= DS4KeyType.Unbound;
                }
                else if (outputType == OutType.Key)
                {
                    settings.ActionData.ActionKey = outkey;
                    settings.ControlActionType = DS4ControlSettings.ActionType.Key;
                    if (HasScanCode) settings.KeyType |= DS4KeyType.ScanCode;

                    if (Toggle) settings.KeyType |= DS4KeyType.Toggle;
                }
                else if (outputType == OutType.Macro)
                {
                    settings.ActionData.ActionMacro = macro;
                    settings.ControlActionType = DS4ControlSettings.ActionType.Macro;
                    if (macroType.HasFlag(DS4KeyType.HoldMacro))
                        settings.KeyType |= DS4KeyType.HoldMacro;
                    else
                        settings.KeyType |= DS4KeyType.Macro;

                    if (HasScanCode) settings.KeyType |= DS4KeyType.ScanCode;
                }

                if (IsUsingExtras())
                    settings.Extras = CompileExtras();
                else
                    settings.Extras = string.Empty;

                Global.RefreshActionAlias(settings, shiftBind);
            }
            else
            {
                settings.ShiftKeyType = DS4KeyType.None;
                settings.ShiftTrigger = ShiftTrigger;

                if (outputType == OutType.Default || ShiftTrigger == 0)
                {
                    settings.ShiftAction.ActionKey = 0;
                    settings.ShiftActionType = DS4ControlSettings.ActionType.Default;
                }
                else if (outputType == OutType.Button)
                {
                    settings.ShiftAction.ActionButton = control;
                    settings.ShiftActionType = DS4ControlSettings.ActionType.Button;
                    if (control == X360Controls.Unbound) settings.ShiftKeyType |= DS4KeyType.Unbound;
                }
                else if (outputType == OutType.Key)
                {
                    settings.ShiftAction.ActionKey = outkey;
                    settings.ShiftActionType = DS4ControlSettings.ActionType.Key;
                    if (HasScanCode) settings.ShiftKeyType |= DS4KeyType.ScanCode;

                    if (Toggle) settings.ShiftKeyType |= DS4KeyType.Toggle;
                }
                else if (outputType == OutType.Macro)
                {
                    settings.ShiftAction.ActionMacro = macro;
                    settings.ShiftActionType = DS4ControlSettings.ActionType.Macro;

                    if (macroType.HasFlag(DS4KeyType.HoldMacro))
                        settings.ShiftKeyType |= DS4KeyType.HoldMacro;
                    else
                        settings.ShiftKeyType |= DS4KeyType.Macro;

                    if (HasScanCode) settings.ShiftKeyType |= DS4KeyType.ScanCode;
                }

                if (IsUsingExtras())
                    settings.ShiftExtras = CompileExtras();
                else
                    settings.ShiftExtras = string.Empty;

                Global.RefreshActionAlias(settings, shiftBind);
            }
        }

        public void UpdateExtrasColor(Color color)
        {
            ExtrasColorR = color.R;
            ExtrasColorG = color.G;
            ExtrasColorB = color.B;
        }
    }
}