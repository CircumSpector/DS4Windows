using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;
using DS4Windows;
using DS4Windows.Shared.Common.Types;
using DS4WinWPF.DS4Control.IoC.Services;
using DS4WinWPF.Properties;
using JetBrains.Annotations;

namespace DS4WinWPF.DS4Forms.ViewModels
{
    public interface IBindingWindowViewModel
    {
        bool Using360Mode { get; }
        int DeviceNum { get; }
        OutBinding CurrentOutBind { get; }
        OutBinding ShiftOutBind { get; }
        OutBinding ActionBinding { get; set; }
        bool ShowShift { get; set; }
        bool RumbleActive { get; set; }
        DS4ControlSettingsV3 Settings { get; }
        void PopulateCurrentBinds();
        void WriteBinds();
        void StartForcedColor(Color color);
        void EndForcedColor();
        void UpdateForcedColor(Color color);
    }

    public class BindingWindowViewModel : IBindingWindowViewModel
    {
        private readonly IProfilesService profilesService;

        [UsedImplicitly]
        public BindingWindowViewModel(IProfilesService profilesService)
        {
            this.profilesService = profilesService;
        }

        public BindingWindowViewModel(int deviceNum, DS4ControlSettingsV3 settings)
        {
            DeviceNum = deviceNum;
            Using360Mode = Global.OutDevTypeTemp[deviceNum] == OutContType.X360;
            Settings = settings;
            CurrentOutBind = new OutBinding();
            ShiftOutBind = new OutBinding
            {
                ShiftBind = true
            };

            PopulateCurrentBinds();
        }

        public bool Using360Mode { get; }

        [Obsolete]
        public int DeviceNum { get; }

        public OutBinding CurrentOutBind { get; }

        public OutBinding ShiftOutBind { get; }

        public OutBinding ActionBinding { get; set; }

        public bool ShowShift { get; set; }

        public bool RumbleActive { get; set; }

        public DS4ControlSettingsV3 Settings { get; }

        public void PopulateCurrentBinds()
        {
            var setting = Settings;
            var sc = setting.KeyType.HasFlag(DS4KeyType.ScanCode);
            var toggle = setting.KeyType.HasFlag(DS4KeyType.Toggle);
            CurrentOutBind.Input = setting.Control;
            ShiftOutBind.Input = setting.Control;
            if (setting.ControlActionType != DS4ControlSettingsV3.ActionType.Default)
                switch (setting.ControlActionType)
                {
                    case DS4ControlSettingsV3.ActionType.Button:
                        CurrentOutBind.OutputType = OutBinding.OutType.Button;
                        CurrentOutBind.Control = setting.ActionData.ActionButton;
                        break;
                    case DS4ControlSettingsV3.ActionType.Default:
                        CurrentOutBind.OutputType = OutBinding.OutType.Default;
                        break;
                    case DS4ControlSettingsV3.ActionType.Key:
                        CurrentOutBind.OutputType = OutBinding.OutType.Key;
                        CurrentOutBind.OutKey = setting.ActionData.ActionKey;
                        CurrentOutBind.HasScanCode = sc;
                        CurrentOutBind.Toggle = toggle;
                        break;
                    case DS4ControlSettingsV3.ActionType.Macro:
                        CurrentOutBind.OutputType = OutBinding.OutType.Macro;
                        CurrentOutBind.Macro = setting.ActionData.ActionMacro;
                        CurrentOutBind.MacroType = Settings.KeyType;
                        CurrentOutBind.HasScanCode = sc;
                        break;
                }
            else
                CurrentOutBind.OutputType = OutBinding.OutType.Default;

            if (!string.IsNullOrEmpty(setting.Extras)) CurrentOutBind.ParseExtras(setting.Extras);

            if (setting.ShiftActionType != DS4ControlSettingsV3.ActionType.Default)
            {
                sc = setting.ShiftKeyType.HasFlag(DS4KeyType.ScanCode);
                toggle = setting.ShiftKeyType.HasFlag(DS4KeyType.Toggle);
                ShiftOutBind.ShiftTrigger = setting.ShiftTrigger;
                switch (setting.ShiftActionType)
                {
                    case DS4ControlSettingsV3.ActionType.Button:
                        ShiftOutBind.OutputType = OutBinding.OutType.Button;
                        ShiftOutBind.Control = setting.ShiftAction.ActionButton;
                        break;
                    case DS4ControlSettingsV3.ActionType.Default:
                        ShiftOutBind.OutputType = OutBinding.OutType.Default;
                        break;
                    case DS4ControlSettingsV3.ActionType.Key:
                        ShiftOutBind.OutputType = OutBinding.OutType.Key;
                        ShiftOutBind.OutKey = setting.ShiftAction.ActionKey;
                        ShiftOutBind.HasScanCode = sc;
                        ShiftOutBind.Toggle = toggle;
                        break;
                    case DS4ControlSettingsV3.ActionType.Macro:
                        ShiftOutBind.OutputType = OutBinding.OutType.Macro;
                        ShiftOutBind.Macro = setting.ShiftAction.ActionMacro;
                        ShiftOutBind.MacroType = setting.ShiftKeyType;
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
    }

    public class BindAssociation
    {
        public enum OutType : uint
        {
            Default,
            /// <summary>
            ///     Keyboard key press.
            /// </summary>
            Key,
            /// <summary>
            ///     Face button on output controller.
            /// </summary>
            Button,
            Macro
        }

        public X360Controls Control { get; set; }

        public int OutKey { get; set; }

        public OutType OutputType { get; set; }

        public bool IsMouse()
        {
            return OutputType == OutType.Button && Control is >= X360Controls.LeftMouse and < X360Controls.Unbound;
        }

        public static bool IsMouseRange(X360Controls control)
        {
            return control is >= X360Controls.LeftMouse and < X360Controls.Unbound;
        }
    }

    public class OutBinding : INotifyPropertyChanged
    {
        public enum OutType : uint
        {
            Default,
            Key,
            Button,
            Macro
        }

        public X360Controls Control;

        private DS4Color extrasColor = new(255, 255, 255);

        private int flashRate;

        private int heavyRumble;

        public DS4Controls Input;

        private int lightRumble;

        public int[] Macro { get; set; }

        public DS4KeyType MacroType { get; set; }

        private int mouseSens = 25;

        public int OutKey { get; set; }

        public OutType OutputType { get; set; }

        public bool ShiftBind { get; set; }

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
            set => flashRate = value;
        }

        public int MouseSens
        {
            get => mouseSens;
            set => mouseSens = value;
        }

        public bool UseMouseSens { get; set; }

        public bool UseExtrasColor { get; set; }

        public int ExtrasColorR
        {
            get => extrasColor.Red;
            set => extrasColor.Red = (byte)value;
        }

        public string ExtrasColorRString => $"#{extrasColor.Red:X2}FF0000";

        public int ExtrasColorG
        {
            get => extrasColor.Green;
            set => extrasColor.Green = (byte)value;
        }

        public string ExtrasColorGString => $"#{extrasColor.Green:X2}00FF00";

        public int ExtrasColorB
        {
            get => extrasColor.Blue;
            set => extrasColor.Blue = (byte)value;
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

        public string DefaultColor => OutputType == OutType.Default
            ? Colors.LimeGreen.ToString()
            : Application.Current.FindResource("SecondaryColor").ToString();

        public string UnboundColor =>
            OutputType == OutType.Button && Control == X360Controls.Unbound
                ? Colors.LimeGreen.ToString()
                : Application.Current.FindResource("SecondaryColor").ToString();

        public string DefaultBtnString
        {
            get
            {
                var result = "Default";
                if (ShiftBind) result = Resources.FallBack;

                return result;
            }
        }

        public Visibility MacroLbVisible => OutputType == OutType.Macro ? Visibility.Visible : Visibility.Hidden;

        private void OutBinding_UseExtrasColorChanged(object sender, EventArgs e)
        {
            if (!UseExtrasColor)
            {
                ExtrasColorR = 255;
                ExtrasColorG = 255;
                ExtrasColorB = 255;
            }
        }

        public bool IsShift()
        {
            return ShiftBind;
        }

        public bool IsMouse()
        {
            return OutputType == OutType.Button && Control >= X360Controls.LeftMouse && Control < X360Controls.Unbound;
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
                    UseExtrasColor = true;
                    byte.TryParse(temp[3], out var r);
                    byte.TryParse(temp[4], out var g);
                    byte.TryParse(temp[5], out var b);
                    int.TryParse(temp[6], out flashRate);

                    extrasColor = new DS4Color(r, g, b);
                }
                else
                {
                    UseExtrasColor = false;
                    extrasColor.Red = extrasColor.Green = extrasColor.Blue = 255;
                    flashRate = 0;
                }

                int.TryParse(temp[7], out var useM);
                if (useM == 1)
                {
                    UseMouseSens = true;
                    int.TryParse(temp[8], out mouseSens);
                }
                else
                {
                    UseMouseSens = false;
                    mouseSens = 25;
                }
            }
        }

        public string CompileExtras()
        {
            var result = $"{heavyRumble},{lightRumble},";
            if (UseExtrasColor)
                result += $"1,{extrasColor.Red},{extrasColor.Green},{extrasColor.Blue},{flashRate},";
            else
                result += "0,0,0,0,0,";

            if (UseMouseSens)
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
            result = result || UseExtrasColor;
            result = result ||
                     extrasColor.Red != 255 && extrasColor.Green != 255 &&
                     extrasColor.Blue != 255;

            result = result || flashRate != 0;
            result = result || UseMouseSens;
            result = result || mouseSens != 25;
            return result;
        }

        public void WriteBind(DS4ControlSettingsV3 settings)
        {
            if (!ShiftBind)
            {
                settings.KeyType = DS4KeyType.None;

                if (OutputType == OutType.Default)
                {
                    settings.ActionData.ActionKey = 0;
                    settings.ControlActionType = DS4ControlSettingsV3.ActionType.Default;
                }
                else if (OutputType == OutType.Button)
                {
                    settings.ActionData.ActionButton = Control;
                    settings.ControlActionType = DS4ControlSettingsV3.ActionType.Button;
                    if (Control == X360Controls.Unbound) settings.KeyType |= DS4KeyType.Unbound;
                }
                else if (OutputType == OutType.Key)
                {
                    settings.ActionData.ActionKey = OutKey;
                    settings.ControlActionType = DS4ControlSettingsV3.ActionType.Key;
                    if (HasScanCode) settings.KeyType |= DS4KeyType.ScanCode;

                    if (Toggle) settings.KeyType |= DS4KeyType.Toggle;
                }
                else if (OutputType == OutType.Macro)
                {
                    settings.ActionData.ActionMacro = Macro;
                    settings.ControlActionType = DS4ControlSettingsV3.ActionType.Macro;
                    if (MacroType.HasFlag(DS4KeyType.HoldMacro))
                        settings.KeyType |= DS4KeyType.HoldMacro;
                    else
                        settings.KeyType |= DS4KeyType.Macro;

                    if (HasScanCode) settings.KeyType |= DS4KeyType.ScanCode;
                }

                if (IsUsingExtras())
                    settings.Extras = CompileExtras();
                else
                    settings.Extras = string.Empty;

                Global.RefreshActionAlias(settings, ShiftBind);
            }
            else
            {
                settings.ShiftKeyType = DS4KeyType.None;
                settings.ShiftTrigger = ShiftTrigger;

                if (OutputType == OutType.Default || ShiftTrigger == 0)
                {
                    settings.ShiftAction.ActionKey = 0;
                    settings.ShiftActionType = DS4ControlSettingsV3.ActionType.Default;
                }
                else if (OutputType == OutType.Button)
                {
                    settings.ShiftAction.ActionButton = Control;
                    settings.ShiftActionType = DS4ControlSettingsV3.ActionType.Button;
                    if (Control == X360Controls.Unbound) settings.ShiftKeyType |= DS4KeyType.Unbound;
                }
                else if (OutputType == OutType.Key)
                {
                    settings.ShiftAction.ActionKey = OutKey;
                    settings.ShiftActionType = DS4ControlSettingsV3.ActionType.Key;
                    if (HasScanCode) settings.ShiftKeyType |= DS4KeyType.ScanCode;

                    if (Toggle) settings.ShiftKeyType |= DS4KeyType.Toggle;
                }
                else if (OutputType == OutType.Macro)
                {
                    settings.ShiftAction.ActionMacro = Macro;
                    settings.ShiftActionType = DS4ControlSettingsV3.ActionType.Macro;

                    if (MacroType.HasFlag(DS4KeyType.HoldMacro))
                        settings.ShiftKeyType |= DS4KeyType.HoldMacro;
                    else
                        settings.ShiftKeyType |= DS4KeyType.Macro;

                    if (HasScanCode) settings.ShiftKeyType |= DS4KeyType.ScanCode;
                }

                if (IsUsingExtras())
                    settings.ShiftExtras = CompileExtras();
                else
                    settings.ShiftExtras = string.Empty;

                Global.RefreshActionAlias(settings, ShiftBind);
            }
        }

        public void UpdateExtrasColor(Color color)
        {
            ExtrasColorR = color.R;
            ExtrasColorG = color.G;
            ExtrasColorB = color.B;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}