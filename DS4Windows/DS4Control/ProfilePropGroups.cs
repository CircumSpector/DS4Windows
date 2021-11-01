using System;
using System.Drawing;
using DS4Windows.InputDevices;
using JetBrains.Annotations;
using PropertyChanged;
using Sensorit.Base;

namespace DS4Windows
{
    public class SquareStickInfo
    {
        public bool LSMode { get; set; }

        public bool RSMode { get; set; }

        public double LSRoundness { get; set; } = 5.0;

        public double RSRoundness { get; set; } = 5.0;
    }

    public class StickDeadZoneInfo
    {
        public enum DeadZoneType : ushort
        {
            Radial,
            Axial
        }

        public const int DefaultDeadZone = 10;
        public const int DefaultAntiDeadZone = 20;
        public const int DefaultMaxZone = 100;
        public const double DefaultMaxOutput = 100.0;
        public const bool DefaultMaxOutputForce = false;
        public const int DefaultFuzz = 0;
        public const DeadZoneType DefaultDeadZoneType = DeadZoneType.Radial;
        public const double DefaultVerticalScale = 100.0;
        public const double DefaultOuterBindDead = 75.0;
        public const bool DefaultOuterBindInvert = false;

        public class AxisDeadZoneInfo
        {
            // DeadZone value from 0-127 (old bad convention)
            public int DeadZone { get; set; } = DefaultDeadZone;

            public int AntiDeadZone { get; set; } = DefaultAntiDeadZone;

            public int MaxZone { get; set; } = DefaultMaxZone;

            public double MaxOutput { get; set; } = DefaultMaxOutput;

            public void Reset()
            {
                DeadZone = DefaultDeadZone;
                AntiDeadZone = DefaultAntiDeadZone;
                MaxZone = DefaultMaxZone;
                MaxOutput = DefaultMaxOutput;
            }
        }

        // DeadZone value from 0-127 (old bad convention)
        public int DeadZone { get; set; }
        public int AntiDeadZone { get; set; }
        public int MaxZone  { get; set; } = DefaultMaxZone;
        public double MaxOutput { get; set; } = DefaultMaxOutput;
        public bool MaxOutputForce { get; set; } = DefaultMaxOutputForce;
        public int Fuzz { get; set; } = DefaultFuzz;
        public double VerticalScale { get; set; } = DefaultVerticalScale;
        public DeadZoneType DZType { get; set; } = DefaultDeadZoneType;
        public double OuterBindDeadZone { get; set; } = DefaultOuterBindDead;
        public bool OuterBindInvert { get; set; } = DefaultOuterBindInvert;
        public AxisDeadZoneInfo XAxisDeadInfo { get; set; } = new();
        public AxisDeadZoneInfo YAxisDeadInfo { get; set; } = new();

        public void Reset()
        {
            DeadZone = 0;
            AntiDeadZone = 0;
            MaxZone = DefaultMaxZone;
            MaxOutput = DefaultMaxOutput;
            MaxOutputForce = DefaultMaxOutputForce;

            Fuzz = DefaultFuzz;
            VerticalScale = DefaultVerticalScale;
            DZType = DefaultDeadZoneType;
            OuterBindDeadZone = DefaultOuterBindDead;
            OuterBindInvert = DefaultOuterBindInvert;
            XAxisDeadInfo.Reset();
            YAxisDeadInfo.Reset();
        }
    }

    public class StickAntiSnapbackInfo
    {
        public const double DefaultDelta = 135;
        public const int DefaultTimeout = 50;
        public const bool DefaultEnabled = false;

        public bool Enabled { get; set; } = DefaultEnabled;

        public double Delta { get; set; } = DefaultDelta;

        public int Timeout { get; set; } = DefaultTimeout;
    }

    [AddINotifyPropertyChangedInterface]
    public class TriggerDeadZoneZInfo
    {
        // Trigger deadzone is expressed in axis units (bad old convention)
        public byte DeadZone { get; set; }

        [UsedImplicitly]
        private void OnDeadZoneChanged()
        {
            DeadZoneChanged?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler DeadZoneChanged;

        public int AntiDeadZone { get; set; }
        public int maxZone = 100;
        public int MaxZone
        {
            get => maxZone;
            set
            {
                if (maxZone == value) return;
                maxZone = value;
                MaxZoneChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler MaxZoneChanged;

        public double maxOutput = 100.0;

        public double MaxOutput
        {
            get => maxOutput;
            set
            {
                if (maxOutput == value) return;
                maxOutput = value;
                MaxOutputChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler MaxOutputChanged;

        public void Reset()
        {
            DeadZone = 0;
            AntiDeadZone = 0;
            MaxZone = 100;
            MaxOutput = 100.0;
        }

        public void ResetEvents()
        {
            MaxZoneChanged = null;
            MaxOutputChanged = null;
            DeadZoneChanged = null;
        }
    }

    public class GyroMouseInfo
    {
        public enum SmoothingMethod : byte
        {
            None,
            OneEuro,
            WeightedAverage,
        }

        public const double DefaultMinCutoff = 1.0;
        public const double DefaultBeta = 0.7;
        public const string DefaultSmoothTechnique = "one-euro";
        public const double DefaultMinThreshold = 1.0;

        public bool enableSmoothing = false;
        public double smoothingWeight = 0.5;
        public SmoothingMethod smoothingMethod;

        public double minCutoff = DefaultMinCutoff;
        public double beta = DefaultBeta;
        public double minThreshold = DefaultMinThreshold;

        public delegate void GyroMouseInfoEventHandler(GyroMouseInfo sender, EventArgs args);

        public double MinCutoff
        {
            get => minCutoff;
            set
            {
                if (minCutoff == value) return;
                minCutoff = value;
                MinCutoffChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event GyroMouseInfoEventHandler MinCutoffChanged;

        public double Beta
        {
            get => beta;
            set
            {
                if (beta == value) return;
                beta = value;
                BetaChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event GyroMouseInfoEventHandler BetaChanged;

        public void Reset()
        {
            minCutoff = DefaultMinCutoff;
            beta = DefaultBeta;
            enableSmoothing = false;
            smoothingMethod = SmoothingMethod.None;
            smoothingWeight = 0.5;
            minThreshold = DefaultMinThreshold;
        }

        public void ResetSmoothing()
        {
            enableSmoothing = false;
            ResetSmoothingMethods();
        }

        public void ResetSmoothingMethods()
        {
            smoothingMethod = SmoothingMethod.None;
        }

        public void DetermineSmoothMethod(string identier)
        {
            ResetSmoothingMethods();

            switch (identier)
            {
                case "weighted-average":
                    smoothingMethod = SmoothingMethod.WeightedAverage;
                    break;
                case "one-euro":
                    smoothingMethod = SmoothingMethod.OneEuro;
                    break;
                default:
                    smoothingMethod = SmoothingMethod.None;
                    break;
            }
        }

        public string SmoothMethodIdentifier()
        {
            string result = "none";
            if (smoothingMethod == SmoothingMethod.OneEuro)
            {
                result = "one-euro";
            }
            else if (smoothingMethod == SmoothingMethod.WeightedAverage)
            {
                result = "weighted-average";
            }

            return result;
        }

        public void SetRefreshEvents(OneEuroFilter euroFilter)
        {
            BetaChanged += (sender, args) =>
            {
                euroFilter.Beta = beta;
            };

            MinCutoffChanged += (sender, args) =>
            {
                euroFilter.MinCutoff = minCutoff;
            };
        }

        public void RemoveRefreshEvents()
        {
            BetaChanged = null;
            MinCutoffChanged = null;
        }
    }

    public class GyroMouseStickInfo
    {
        public enum SmoothingMethod : byte
        {
            None,
            OneEuro,
            WeightedAverage,
        }

        public enum OutputStick : byte
        {
            None,
            LeftStick,
            RightStick,
        }

        public enum OutputStickAxes : byte
        {
            None,
            XY,
            X,
            Y
        }

        public const double DefaultMinCutoff = 0.4;
        public const double DefaultBeta = 0.7;
        public const string DefaultSmoothTechnique = "one-euro";
        public const OutputStick DefaultOutputStick = OutputStick.RightStick;
        public const OutputStickAxes DefaultOutputStickAxes = OutputStickAxes.XY;

        public int DeadZone { get; set; }
        public int MaxZone { get; set; }
        public double AntiDeadX { get; set; }
        public double AntiDeadY { get; set; }
        public int VerticalScale { get; set; }
        public bool MaxOutputEnabled { get; set; }
        public double MaxOutput { get; set; } = 100.0;
        // Flags representing invert axis choices
        public uint Inverted { get; set; }
        public bool UseSmoothing { get; set; }
        public double SmoothWeight { get; set; }
        public SmoothingMethod Smoothing { get; set; }
        public double minCutoff = DefaultMinCutoff;
        public double beta = DefaultBeta;
        public OutputStick outputStick = DefaultOutputStick;
        public OutputStickAxes outputStickDir = DefaultOutputStickAxes;

        public delegate void GyroMouseStickInfoEventHandler(GyroMouseStickInfo sender,
            EventArgs args);

        public double MinCutoff
        {
            get => minCutoff;
            set
            {
                if (minCutoff == value) return;
                minCutoff = value;
                MinCutoffChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event GyroMouseStickInfoEventHandler MinCutoffChanged;

        public double Beta
        {
            get => beta;
            set
            {
                if (beta == value) return;
                beta = value;
                BetaChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event GyroMouseStickInfoEventHandler BetaChanged;

        public void Reset()
        {
            DeadZone = 30; MaxZone = 830;
            AntiDeadX = 0.4; AntiDeadY = 0.4;
            Inverted = 0; VerticalScale = 100;
            MaxOutputEnabled = false; MaxOutput = 100.0;
            outputStick = DefaultOutputStick;
            outputStickDir = DefaultOutputStickAxes;

            minCutoff = DefaultMinCutoff;
            beta = DefaultBeta;
            Smoothing = SmoothingMethod.None;
            UseSmoothing = false;
            SmoothWeight = 0.5;
        }

        public void ResetSmoothing()
        {
            UseSmoothing = false;
            ResetSmoothingMethods();
        }

        public void ResetSmoothingMethods()
        {
            Smoothing = SmoothingMethod.None;
        }

        public void DetermineSmoothMethod(string identier)
        {
            ResetSmoothingMethods();

            switch (identier)
            {
                case "weighted-average":
                    Smoothing = SmoothingMethod.WeightedAverage;
                    break;
                case "one-euro":
                    Smoothing = SmoothingMethod.OneEuro;
                    break;
                default:
                    Smoothing = SmoothingMethod.None;
                    break;
            }
        }

        public string SmoothMethodIdentifier()
        {
            string result = "none";
            switch (Smoothing)
            {
                case SmoothingMethod.WeightedAverage:
                    result = "weighted-average";
                    break;
                case SmoothingMethod.OneEuro:
                    result = "one-euro";
                    break;
                default:
                    break;
            }

            return result;
        }

        public void SetRefreshEvents(OneEuroFilter euroFilter)
        {
            BetaChanged += (sender, args) =>
            {
                euroFilter.Beta = beta;
            };

            MinCutoffChanged += (sender, args) =>
            {
                euroFilter.MinCutoff = minCutoff;
            };
        }

        public void RemoveRefreshEvents()
        {
            BetaChanged = null;
            MinCutoffChanged = null;
        }

        public bool OutputHorizontal()
        {
            return outputStickDir == OutputStickAxes.XY ||
                outputStickDir == OutputStickAxes.X;
        }

        public bool OutputVertical()
        {
            return outputStickDir == OutputStickAxes.XY ||
                outputStickDir == OutputStickAxes.Y;
        }
    }

    public class GyroDirectionalSwipeInfo
    {
        public enum XAxisSwipe : ushort
        {
            Yaw,
            Roll,
        }

        public const string DefaultTriggers = "-1";
        public const int DefaultGyroDirSpeed = 80; // degrees per second
        public const bool DefaultTriggerCond = true;
        public const bool DefaultTriggerTurns = true;
        public const XAxisSwipe DefaultXAxis = XAxisSwipe.Yaw;
        public const int DefaultDelayTime = 0;

        public int DeadZoneX { get; set; } = DefaultGyroDirSpeed;
        public int DeadZoneY { get; set; }= DefaultGyroDirSpeed;
        public string Triggers { get; set; } = DefaultTriggers;
        public bool TriggerCondition { get; set; } = DefaultTriggerCond;
        public bool TriggerTurns { get; set; } = DefaultTriggerTurns;
        public XAxisSwipe XAxis { get; set; } = DefaultXAxis;
        public int DelayTime { get; set; } = DefaultDelayTime;

        public void Reset()
        {
            DeadZoneX = DefaultGyroDirSpeed;
            DeadZoneY = DefaultGyroDirSpeed;
            Triggers = DefaultTriggers;
            TriggerCondition = DefaultTriggerCond;
            TriggerTurns = DefaultTriggerTurns;
            XAxis = DefaultXAxis;
            DelayTime = DefaultDelayTime;
        }
    }

    public class GyroControlsInfo
    {
        public const string DefaultTriggers = "-1";
        public const bool DefaultTriggerCond = true;
        public const bool DefaultTriggerTurns = true;
        public const bool DefaultTriggerToggle = false;

        public string Triggers { get; set; } = DefaultTriggers;
        public bool TriggerCond { get; set; } = DefaultTriggerCond;
        public bool TriggerTurns { get; set; } = DefaultTriggerTurns;
        public bool TriggerToggle { get; set; } = DefaultTriggerToggle;

        public void Reset()
        {
            Triggers = DefaultTriggers;
            TriggerCond = DefaultTriggerCond;
            TriggerTurns = DefaultTriggerTurns;
            TriggerToggle = DefaultTriggerToggle;
        }
    }

    public class ButtonMouseInfo
    {
        //public const double MOUSESTICKANTIOFFSET = 0.0128;
        public const double MouseStickAntiOffset = 0.008;
        public const int DefaultButtonSens = 25;
        public const double DefaultButtonVerticalScale = 1.0;
        public const int DefaultTempSens = -1;

        public int buttonSensitivity = DefaultButtonSens;
        public int ButtonSensitivity
        {
            get => buttonSensitivity;
            set
            {
                if (buttonSensitivity == value) return;
                buttonSensitivity = value;
                ButtonMouseInfoChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler ButtonMouseInfoChanged;

        public bool MouseAcceleration { get; set; }
        public int ActiveButtonSensitivity { get; set; } = DefaultButtonSens;
        public int TempButtonSensitivity { get; set; } = DefaultTempSens;
        public double MouseVelocityOffset { get; set; } = MouseStickAntiOffset;
        public double ButtonVerticalScale { get; set; } = DefaultButtonVerticalScale;

        public ButtonMouseInfo()
        {
            ButtonMouseInfoChanged += ButtonMouseInfo_ButtonMouseInfoChanged;
        }

        private void ButtonMouseInfo_ButtonMouseInfoChanged(object sender, EventArgs e)
        {
            if (TempButtonSensitivity == DefaultTempSens)
            {
                ActiveButtonSensitivity = buttonSensitivity;
            }
        }

        public void SetActiveButtonSensitivity(int sens)
        {
            ActiveButtonSensitivity = sens;
        }

        public void Reset()
        {
            buttonSensitivity = DefaultButtonSens;
            MouseAcceleration = false;
            ActiveButtonSensitivity = DefaultButtonSens;
            TempButtonSensitivity = DefaultTempSens;
            MouseVelocityOffset = MouseStickAntiOffset;
            ButtonVerticalScale = DefaultButtonVerticalScale;
        }
    }

    public enum LightbarMode : uint
    {
        None,
        DS4Win,
        Passthru,
    }

    /// <summary>
    ///     Lightbar-specific properties like colors etc.
    /// </summary>
    public class LightbarDS4WinInfo
    {
        public bool UseCustomLed { get; set; }

        public bool LedAsBattery { get; set; }

        public DS4Color CustomLed { get; set; } = new(0, 0, 255);

        public DS4Color Led { get; set; }

        public DS4Color LowLed { get; set; } = new(Color.Black);

        public DS4Color ChargingLed { get; set; } = new(Color.Black);

        public DS4Color FlashLed { get; set; } = new(Color.Black);

        public double Rainbow { get; set; }

        public double MaxRainbowSaturation { get; set; } = 1.0;

        public int FlashAt { get; set; } // Battery % when flashing occurs. <0 means disabled

        public byte FlashType { get; set; }

        public int ChargingType { get; set; }
    }

    /// <summary>
    ///     Lightbar behaviour settings.
    /// </summary>
    public class LightbarSettingInfo
    {
        private LightbarMode mode = LightbarMode.DS4Win;

        public LightbarDS4WinInfo Ds4WinSettings { get; } = new();

        public LightbarMode Mode
        {
            get => mode;
            set
            {
                if (mode == value) return;
                mode = value;
                ModeChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public event EventHandler ModeChanged;
    }

    public class SteeringWheelSmoothingInfo
    {
        private double minCutoff = OneEuroFilterPair.DEFAULT_WHEEL_CUTOFF;
        private double beta = OneEuroFilterPair.DEFAULT_WHEEL_BETA;
        public bool enabled = false;

        public delegate void SmoothingInfoEventHandler(SteeringWheelSmoothingInfo sender, EventArgs args);

        public double MinCutoff
        {
            get => minCutoff;
            set
            {
                if (minCutoff == value) return;
                minCutoff = value;
                MinCutoffChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event SmoothingInfoEventHandler MinCutoffChanged;

        public double Beta
        {
            get => beta;
            set
            {
                if (beta == value) return;
                beta = value;
                BetaChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event SmoothingInfoEventHandler BetaChanged;

        public bool Enabled
        {
            get => enabled;
            set => enabled = value;
        }

        public void Reset()
        {
            MinCutoff = OneEuroFilterPair.DEFAULT_WHEEL_CUTOFF;
            Beta = OneEuroFilterPair.DEFAULT_WHEEL_BETA;
            enabled = false;
        }

        public void SetFilterAttrs(OneEuroFilter euroFilter)
        {
            euroFilter.MinCutoff = minCutoff;
            euroFilter.Beta = beta;
        }

        public void SetRefreshEvents(OneEuroFilter euroFilter)
        {
            BetaChanged += (sender, args) =>
            {
                euroFilter.Beta = beta;
            };

            MinCutoffChanged += (sender, args) =>
            {
                euroFilter.MinCutoff = minCutoff;
            };
        }
    }

    public class TouchPadRelMouseSettings
    {
        public const double DefaultAngDegree = 0.0;
        public const double DefaultAngRad = DefaultAngDegree * Math.PI / 180.0;
        public const double DefaultMinThreshold = 1.0;

        public double Rotation { get; set; } = DefaultAngRad;
        public double MinThreshold { get; set; } = DefaultMinThreshold;

        public void Reset()
        {
            Rotation = DefaultAngRad;
            MinThreshold = DefaultMinThreshold;
        }
    }

    public class TouchPadAbsMouseSettings
    {
        public const int DefaultMaxZoneX = 90;
        public const int DefaultMaxZoneY = 90;
        public const bool DefaultSnapCenter = false;

        public int MaxZoneX { get; set; } = DefaultMaxZoneX;
        public int MaxZoneY { get; set; } = DefaultMaxZoneY;
        public bool SnapToCenter { get; set; } = DefaultSnapCenter;

        public void Reset()
        {
            MaxZoneX = DefaultMaxZoneX;
            MaxZoneY = DefaultMaxZoneY;
            SnapToCenter = DefaultSnapCenter;
        }
    }

    public enum StickMode : uint
    {
        None,
        Controls,
        FlickStick,
    }

    public enum TriggerMode : uint
    {
        Normal,
        TwoStage,
    }

    public enum TwoStageTriggerMode : uint
    {
        Disabled,
        Normal,
        ExclusiveButtons,
        HairTrigger,
        HipFire,
        HipFireExclusiveButtons,
    }

    public class FlickStickSettings
    {
        public const double DefaultFlickThreshold = 0.9;
        public const double DefaultFlickTime = 0.1;  // In seconds
        public const double DefaultRealWorldCalibration = 5.3;
        public const double DefaultMinAngleThreshold = 0.0;

        public const double DefaultMinCutoff = 0.4;
        public const double DefaultBeta = 0.4;

        public double flickThreshold = DefaultFlickThreshold;
        public double flickTime = DefaultFlickTime; // In seconds
        public double realWorldCalibration = DefaultRealWorldCalibration;
        public double minAngleThreshold = DefaultMinAngleThreshold;

        public double minCutoff = DefaultMinCutoff;
        public double beta = DefaultBeta;

        public delegate void FlickStickSettingsEventHandler(FlickStickSettings sender,
           EventArgs args);

        public double MinCutoff
        {
            get => minCutoff;
            set
            {
                if (minCutoff == value) return;
                minCutoff = value;
                MinCutoffChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event FlickStickSettingsEventHandler MinCutoffChanged;

        public double Beta
        {
            get => beta;
            set
            {
                if (beta == value) return;
                beta = value;
                BetaChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event FlickStickSettingsEventHandler BetaChanged;

        public void Reset()
        {
            flickThreshold = DefaultFlickThreshold;
            flickTime = DefaultFlickTime;
            realWorldCalibration = DefaultRealWorldCalibration;
            minAngleThreshold = DefaultMinAngleThreshold;

            minCutoff = DefaultMinCutoff;
            beta = DefaultBeta;
        }

        public void SetRefreshEvents(OneEuroFilter euroFilter)
        {
            BetaChanged += (sender, args) =>
            {
                euroFilter.Beta = beta;
            };

            MinCutoffChanged += (sender, args) =>
            {
                euroFilter.MinCutoff = minCutoff;
            };
        }

        public void RemoveRefreshEvents()
        {
            BetaChanged = null;
            MinCutoffChanged = null;
        }
    }

    public class StickControlSettings
    {
        public void Reset()
        {
        }
    }

    public class StickModeSettings
    {
        public FlickStickSettings FlickSettings { get; set; } = new();

        public StickControlSettings ControlSettings { get; set; } = new();
    }

    public class StickOutputSetting
    {
        public StickMode Mode { get; set; } = StickMode.Controls;

        public StickModeSettings OutputSettings { get; set; } = new();

        public void ResetSettings()
        {
            Mode = StickMode.Controls;
            OutputSettings.ControlSettings.Reset();
            OutputSettings.FlickSettings.Reset();
        }
    }

    public class TriggerOutputSettings
    {
        private const TwoStageTriggerMode DEFAULT_TRIG_MODE = TwoStageTriggerMode.Disabled;
        private const int DEFAULT_HIP_TIME = 100;
        private const TriggerEffects DEFAULT_TRIGGER_EFFECT = TriggerEffects.None;

        //public TriggerMode mode = TriggerMode.Normal;
        public TwoStageTriggerMode twoStageMode = DEFAULT_TRIG_MODE;

        public TwoStageTriggerMode TwoStageMode
        {
            get => twoStageMode;
            set
            {
                if (twoStageMode == value) return;
                twoStageMode = value;
                TwoStageModeChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public event EventHandler TwoStageModeChanged;

        public int HipFireMs { get; set; } = DEFAULT_HIP_TIME;

        public TriggerEffects triggerEffect = DEFAULT_TRIGGER_EFFECT;

        public TriggerEffects TriggerEffect
        {
            get => triggerEffect;
            set
            {
                if (triggerEffect == value) return;
                triggerEffect = value;
                TriggerEffectChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public event EventHandler TriggerEffectChanged;

        public TriggerEffectSettings EffectSettings { get; set; } = new();

        public void ResetSettings()
        {
            //mode = TriggerMode.Normal;
            twoStageMode = DEFAULT_TRIG_MODE;
            HipFireMs = DEFAULT_HIP_TIME;
            triggerEffect = DEFAULT_TRIGGER_EFFECT;
            TwoStageModeChanged?.Invoke(this, EventArgs.Empty);
            TriggerEffectChanged?.Invoke(this, EventArgs.Empty);
        }

        public void ResetEvents()
        {
            TwoStageModeChanged = null;
            TriggerEffectChanged = null;
        }
    }
}