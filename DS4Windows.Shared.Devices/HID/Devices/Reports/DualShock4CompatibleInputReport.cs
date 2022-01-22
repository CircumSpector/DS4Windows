namespace DS4Windows.Shared.Devices.HID.Devices.Reports
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

        public byte TouchPacketCounter { get; protected set; }

        public bool TouchOneFingerActive => Touch1 || Touch2;

        public bool TouchTwoFingersActive => Touch1 && Touch2;

        public bool Mute { get; protected set; }

        /// <summary>
        ///     First (one finger) touch is registered.
        /// </summary>
        public bool Touch1 { get; protected set; }

        /// <summary>
        ///     Second (two fingers) touch is registered.
        /// </summary>
        public bool Touch2 { get; protected set; }

        public bool TouchIsOnLeftSide { get; protected set; }

        public bool TouchIsOnRightSide { get; protected set; }

        public override void ParseFrom(byte[] inputReport, int offset)
        {
            base.ParseFrom(inputReport, offset);

            var touchX = ((inputReport[35 + offset] & 0xF) << 8) | inputReport[34 + offset];

            Mute = (inputReport[10 + offset] & (1 << 2)) != 0;

            TrackPadTouch1 = new TrackPadTouch
            {
                RawTrackingNum = inputReport[33 + offset],
                Id = (byte)(inputReport[33 + offset] & 0x7f),
                IsActive = (inputReport[33 + offset] & 0x80) == 0,
                X = (short)(((ushort)(inputReport[35 + offset] & 0x0f) << 8) |
                            inputReport[34 + offset]),
                Y = (short)((inputReport[36 + offset] << 4) |
                            ((ushort)(inputReport[35 + offset] & 0xf0) >> 4))
            };
            TrackPadTouch2 = new TrackPadTouch
            {
                RawTrackingNum = inputReport[37 + offset],
                Id = (byte)(inputReport[37 + offset] & 0x7f),
                IsActive = (inputReport[37 + offset] & 0x80) == 0,
                X = (short)(((ushort)(inputReport[39 + offset] & 0x0f) << 8) |
                            inputReport[38 + offset]),
                Y = (short)((inputReport[40 + offset] << 4) |
                            ((ushort)(inputReport[39 + offset] & 0xf0) >> 4))
            };
            TouchPacketCounter = inputReport[41 + offset];
            Touch1 = inputReport[33 + offset] >> 7 == 0;
            Touch2 = inputReport[37 + offset] >> 7 == 0;
            TouchIsOnLeftSide = !(touchX >= 1920 * 2 / 5); // TODO: port const
            TouchIsOnRightSide = !(touchX < 1920 * 2 / 5); // TODO: port const
        }
    }
}