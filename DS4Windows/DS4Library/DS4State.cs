using System;

namespace DS4Windows
{
    public class DS4State : ICloneable
    {
        public uint PacketCounter;
        public DateTime ReportTimeStamp;
        public bool Square, Triangle, Circle, Cross;
        public bool DpadUp, DpadDown, DpadLeft, DpadRight;
        public bool L1, L2Btn, L3, R1, R2Btn, R3;
        public bool Share, Options, PS, Mute, Touch1, Touch2, TouchButton, TouchRight,
            TouchLeft, Touch1Finger, Touch2Fingers, OutputTouchButton,
            Capture, SideL, SideR;
        public byte Touch1Identifier, Touch2Identifier;
        public byte LX, RX, LY, RY, L2, R2;
        public byte FrameCounter; // 0, 1, 2...62, 63, 0....
        public byte TouchPacketCounter; // we break these out automatically
        public byte Battery; // 0 for charging, 10/20/30/40/50/60/70/80/90/100 for percentage of full
        public double LSAngle; // Calculated bearing of the LS X,Y coordinates
        public double RSAngle; // Calculated bearing of the RS X,Y coordinates
        public double LSAngleRad; // Calculated bearing of the LS X,Y coordinates (in radians)
        public double RSAngleRad; // Calculated bearing of the RS X,Y coordinates (in radians)
        public double LXUnit;
        public double LYUnit;
        public double RXUnit;
        public double RYUnit;
        public byte OutputLSOuter = 0, OutputRSOuter = 0;
        public double elapsedTime = 0.0;
        public ulong totalMicroSec = 0;
        public ushort ds4Timestamp = 0;
        public SixAxis Motion = null;
        public static readonly int DEFAULT_AXISDIR_VALUE = 127;
        public Int32 SASteeringWheelEmulationUnit;

        public struct TrackPadTouch
        {
            public bool IsActive;
            public byte Id;
            public short X;
            public short Y;
            public byte RawTrackingNum;
        }

        public TrackPadTouch TrackPadTouch0;
        public TrackPadTouch TrackPadTouch1;

        public DS4State()
        {
            PacketCounter = 0;
            Square = Triangle = Circle = Cross = false;
            DpadUp = DpadDown = DpadLeft = DpadRight = false;
            L1 = L2Btn = L3 = R1 = R2Btn = R3 = false;
            Share = Options = PS = Mute = Touch1 = Touch2 = TouchButton =
                OutputTouchButton = TouchRight = TouchLeft =
                Capture = SideL = SideR = false;
            Touch1Finger = Touch2Fingers = false;
            LX = RX = LY = RY = 128;
            L2 = R2 = 0;
            FrameCounter = 255; // only actually has 6 bits, so this is a null indicator
            TouchPacketCounter = 255; // 8 bits, no great junk value
            Battery = 0;
            LSAngle = 0.0;
            LSAngleRad = 0.0;
            RSAngle = 0.0;
            RSAngleRad = 0.0;
            LXUnit = 0.0;
            LYUnit = 0.0;
            RXUnit = 0.0;
            RYUnit = 0.0;
            elapsedTime = 0.0;
            totalMicroSec = 0;
            ds4Timestamp = 0;
            Motion = new SixAxis(0, 0, 0, 0, 0, 0, 0.0);
            TrackPadTouch0.IsActive = false;
            TrackPadTouch1.IsActive = false;
            SASteeringWheelEmulationUnit = 0;
            OutputLSOuter = OutputRSOuter = 0;
        }

        public object Clone()
        {
            return MemberwiseClone();
        }

        /// <summary>
        /// Only copy extra DS4State data that is not output directly tied
        /// to the mapper routine. Gyro motion data, Touchpad touch data,
        /// and timestamp data are copied
        /// </summary>
        /// <param name="state">State object to copy data to</param>
        public void CopyExtrasTo(DS4State state)
        {
            state.Motion = Motion;
            state.ds4Timestamp = ds4Timestamp;
            state.FrameCounter = FrameCounter;
            state.TouchPacketCounter = TouchPacketCounter;
            state.TrackPadTouch0 = TrackPadTouch0;
            state.TrackPadTouch1 = TrackPadTouch1;
        }

        public void CalculateStickAngles()
        {
            double lsangle = Math.Atan2(-(LY - 128), (LX - 128));
            LSAngleRad = lsangle;
            lsangle = (lsangle >= 0 ? lsangle : (2 * Math.PI + lsangle)) * 180 / Math.PI;
            LSAngle = lsangle;
            LXUnit = Math.Abs(Math.Cos(LSAngleRad));
            LYUnit = Math.Abs(Math.Sin(LSAngleRad));

            double rsangle = Math.Atan2(-(RY - 128), (RX - 128));
            RSAngleRad = rsangle;
            rsangle = (rsangle >= 0 ? rsangle : (2 * Math.PI + rsangle)) * 180 / Math.PI;
            RSAngle = rsangle;
            RXUnit = Math.Abs(Math.Cos(RSAngleRad));
            RYUnit = Math.Abs(Math.Sin(RSAngleRad));
        }

        public void RotateLSCoordinates(double rotation)
        {
            double sinAngle = Math.Sin(rotation), cosAngle = Math.Cos(rotation);
            double tempLX = LX - 128.0, tempLY = LY - 128.0;
            LX = (Byte)(Global.Clamp(-128.0, (tempLX * cosAngle - tempLY * sinAngle), 127.0) + 128.0);
            LY = (Byte)(Global.Clamp(-128.0, (tempLX * sinAngle + tempLY * cosAngle), 127.0) + 128.0);
        }

        public void RotateRSCoordinates(double rotation)
        {
            double sinAngle = Math.Sin(rotation), cosAngle = Math.Cos(rotation);
            double tempRX = RX - 128.0, tempRY = RY - 128.0;
            RX = (Byte)(Global.Clamp(-128.0, (tempRX * cosAngle - tempRY * sinAngle), 127.0) + 128.0);
            RY = (Byte)(Global.Clamp(-128.0, (tempRX * sinAngle + tempRY * cosAngle), 127.0) + 128.0);
        }
    }
}
