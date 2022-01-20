namespace DS4Windows.Shared.Core.HID.Devices.Reports
{
    public struct TrackPadTouch
    {
        public bool IsActive;
        public byte Id;
        public short X;
        public short Y;
        public byte RawTrackingNum;
    }

    public class DualShock4CompatibleInputReport : CompatibleHidDeviceInputReport
    {
        public TrackPadTouch TrackPadTouch1;

        public TrackPadTouch TrackPadTouch2;

        public byte TouchPacketCounter { get; init; }

        public bool TouchOneFingerActive => Touch1 || Touch2;

        public bool TouchTwoFingersActive => Touch1 && Touch2;

        public bool Mute { get; init; }

        /// <summary>
        ///     First (one finger) touch is registered.
        /// </summary>
        public bool Touch1 { get; init; }

        /// <summary>
        ///     Second (two fingers) touch is registered.
        /// </summary>
        public bool Touch2 { get; init; }

        public bool TouchIsOnLeftSide { get; init; }

        public bool TouchIsOnRightSide { get; init; }
    }
}