using Ds4Windows.Shared.Devices.Interfaces.HID;

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

        public bool TouchClick { get; protected set; }

        /// <inheritdoc />
        public override void ParseFrom(byte[] inputReport, int offset = 0)
        {
            ReportId = inputReport[0 + offset];

            LeftThumbX = inputReport[1 + offset];
            LeftThumbY = inputReport[2 + offset];
            RightThumbX = inputReport[3 + offset];
            RightThumbY = inputReport[4 + offset];
            LeftTrigger = inputReport[8 + offset];
            RightTrigger = inputReport[9 + offset];

            Triangle = (inputReport[5 + offset] & (1 << 7)) != 0;
            Circle = (inputReport[5 + offset] & (1 << 6)) != 0;
            Cross = (inputReport[5 + offset] & (1 << 5)) != 0;
            Square = (inputReport[5 + offset] & (1 << 4)) != 0;

            DPad = (DPadDirection)(inputReport[5 + offset] & 0x0F);

            LeftThumb = (inputReport[6 + offset] & (1 << 6)) != 0;
            RightThumb = (inputReport[6 + offset] & (1 << 7)) != 0;
            Options = (inputReport[6 + offset] & (1 << 5)) != 0;
            Share = (inputReport[6 + offset] & (1 << 4)) != 0;
            RightTriggerButton = (inputReport[6 + offset] & (1 << 3)) != 0;
            LeftTriggerButton = (inputReport[6 + offset] & (1 << 2)) != 0;
            RightShoulder = (inputReport[6 + offset] & (1 << 1)) != 0;
            LeftShoulder = (inputReport[6 + offset] & (1 << 0)) != 0;

            PS = (inputReport[7 + offset] & (1 << 0)) != 0;
            TouchClick = (inputReport[7 + offset] & (1 << 1)) != 0;

            FrameCounter = (byte)(inputReport[7 + offset] >> 2);

            var touchX = ((inputReport[35 + offset] & 0xF) << 8) | inputReport[34 + offset];
            
            TrackPadTouch1 = new TrackPadTouch
            {
                RawTrackingNum = inputReport[35 + offset],
                Id = (byte)(inputReport[35 + offset] & 0x7f),
                IsActive = (inputReport[35 + offset] & 0x80) == 0,
                X = (short)(((ushort)(inputReport[37 + offset] & 0x0f) << 8) |
                            inputReport[36 + offset]),
                Y = (short)((inputReport[39 + offset] << 4) |
                            ((ushort)(inputReport[37 + offset] & 0xf0) >> 4))
            };
            TrackPadTouch2 = new TrackPadTouch
            {
                RawTrackingNum = inputReport[39 + offset],
                Id = (byte)(inputReport[39 + offset] & 0x7f),
                IsActive = (inputReport[39 + offset] & 0x80) == 0,
                X = (short)(((ushort)(inputReport[41 + offset] & 0x0f) << 8) |
                            inputReport[40 + offset]),
                Y = (short)((inputReport[42 + offset] << 4) |
                            ((ushort)(inputReport[41 + offset] & 0xf0) >> 4))
            };
            TouchPacketCounter = inputReport[34 + offset];
            Touch1 = inputReport[35 + offset] >> 7 == 0;
            Touch2 = inputReport[39 + offset] >> 7 == 0;
            TouchIsOnLeftSide = !(touchX >= 1920 * 2 / 5); // TODO: port const
            TouchIsOnRightSide = !(touchX < 1920 * 2 / 5); // TODO: port const
        }

        /// <inheritdoc />
        public override bool GetIsIdle()
        {
            if (!base.GetIsIdle())
                return false;

            if (Touch1 || Touch2 || TouchClick)
                return false;

            return true;
        }
    }
}