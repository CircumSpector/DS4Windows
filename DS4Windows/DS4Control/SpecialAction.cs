using DS4WinWPF.DS4Control.Profiles.Schema.Converters;
using Newtonsoft.Json;

namespace DS4WinWPF.DS4Control
{
    [JsonConverter(typeof(SpecialActionsConverter))]
    public abstract class SpecialAction
    {
        public static SpecialAction Key = new SpecialActionKey();
        public static SpecialAction Program = new SpecialActionProgram();
        public static SpecialAction Profile = new SpecialActionProfile();
        public static SpecialAction Macro = new SpecialActionMacro();
        public static SpecialAction DisconnectBluetooth = new SpecialActionDisconnectBluetooth();
        public static SpecialAction BatteryCheck = new SpecialActionBatteryCheck();
        public static SpecialAction MultiAction = new SpecialActionMultiAction();
        public static SpecialAction XboxGameDVR = new SpecialActionXboxGameDVR();
        public static SpecialAction SteeringWheelEmulationCalibrate = new SpecialActionSteeringWheelEmulationCalibrate();

        public abstract string Type { get; }

        protected SpecialAction() { }
    }

    public class SpecialActionKey : SpecialAction
    {
        public override string Type => nameof(SpecialActionKey);
    }

    public class SpecialActionProgram : SpecialAction
    {
        public override string Type => nameof(SpecialActionProgram);
    }

    public class SpecialActionProfile : SpecialAction
    {
        public override string Type => nameof(SpecialActionProfile);
    }

    public class SpecialActionMacro : SpecialAction
    {
        public override string Type => nameof(SpecialActionMacro);
    }

    public class SpecialActionDisconnectBluetooth : SpecialAction
    {
        public override string Type => nameof(SpecialActionDisconnectBluetooth);
    }

    public class SpecialActionBatteryCheck : SpecialAction
    {
        public override string Type => nameof(SpecialActionBatteryCheck);
    }

    public class SpecialActionMultiAction : SpecialAction
    {
        public override string Type => nameof(SpecialActionMultiAction);
    }

    public class SpecialActionXboxGameDVR : SpecialAction
    {
        public override string Type => nameof(SpecialActionXboxGameDVR);
    }

    public class SpecialActionSteeringWheelEmulationCalibrate : SpecialAction
    {
        public override string Type => nameof(SpecialActionSteeringWheelEmulationCalibrate);
    }
}
