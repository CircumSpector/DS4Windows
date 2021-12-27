namespace DS4WinWPF.DS4Control
{
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

        protected SpecialAction() { }
    }

    public class SpecialActionKey : SpecialAction
    {

    }

    public class SpecialActionProgram : SpecialAction
    {

    }

    public class SpecialActionProfile : SpecialAction
    {

    }

    public class SpecialActionMacro : SpecialAction
    {

    }

    public class SpecialActionDisconnectBluetooth : SpecialAction
    {

    }

    public class SpecialActionBatteryCheck : SpecialAction
    {

    }

    public class SpecialActionMultiAction : SpecialAction
    {

    }

    public class SpecialActionXboxGameDVR : SpecialAction
    {

    }

    public class SpecialActionSteeringWheelEmulationCalibrate : SpecialAction
    {

    }
}
