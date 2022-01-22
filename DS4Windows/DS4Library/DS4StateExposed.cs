namespace DS4Windows
{
    public class DS4StateExposed
    {
        private readonly DS4State _state;

        public DS4StateExposed()
        {
            _state = new DS4State();
        }

        public DS4StateExposed(DS4State state)
        {
            _state = state;
        }

        private bool Square => _state.Square;
        private bool Triangle => _state.Triangle;
        private bool Circle => _state.Circle;
        private bool Cross => _state.Cross;
        private bool DpadUp => _state.DpadUp;
        private bool DpadDown => _state.DpadDown;
        private bool DpadLeft => _state.DpadLeft;
        private bool DpadRight => _state.DpadRight;
        private bool L1 => _state.L1;
        private bool L3 => _state.L3;
        private bool R1 => _state.R1;
        private bool R3 => _state.R3;
        private bool Share => _state.Share;
        private bool Options => _state.Options;
        private bool PS => _state.PS;
        private bool Touch1 => _state.Touch1;
        private bool Touch2 => _state.Touch2;
        private bool TouchButton => _state.TouchButton;
        private bool Touch1Finger => _state.Touch1Finger;
        private bool Touch2Fingers => _state.Touch2Fingers;
        private byte LX => _state.LX;
        private byte RX => _state.RX;
        private byte LY => _state.LY;
        private byte RY => _state.RY;
        private byte L2 => _state.L2;
        private byte R2 => _state.R2;
        private int Battery => _state.Battery;

        public SixAxis Motion => _state.Motion;

        public int GyroYaw => _state.Motion.gyroYaw;

        public int GyroPitch => _state.Motion.gyroPitch;

        public int GyroRoll => _state.Motion.gyroRoll;

        public int AccelX => _state.Motion.accelX;

        public int AccelY => _state.Motion.accelY;

        public int AccelZ => _state.Motion.accelZ;

        public int OutputAccelX => _state.Motion.outputAccelX;

        public int OutputAccelY => _state.Motion.outputAccelY;

        public int OutputAccelZ => _state.Motion.outputAccelZ;

        public int getGyroYaw()
        {
            return _state.Motion.gyroYaw;
        }

        public int getGyroPitch()
        {
            return _state.Motion.gyroPitch;
        }

        public int getGyroRoll()
        {
            return _state.Motion.gyroRoll;
        }

        public int getAccelX()
        {
            return _state.Motion.accelX;
        }

        public int getAccelY()
        {
            return _state.Motion.accelY;
        }

        public int getAccelZ()
        {
            return _state.Motion.accelZ;
        }

        public int getOutputAccelX()
        {
            return _state.Motion.outputAccelX;
        }

        public int getOutputAccelY()
        {
            return _state.Motion.outputAccelY;
        }

        public int getOutputAccelZ()
        {
            return _state.Motion.outputAccelZ;
        }
    }
}