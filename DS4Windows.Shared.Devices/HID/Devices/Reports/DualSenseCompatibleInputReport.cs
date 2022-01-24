namespace DS4Windows.Shared.Devices.HID.Devices.Reports
{
    public class DualSenseCompatibleInputReport : DualShock4CompatibleInputReport
    {
        public override void ParseFrom(byte[] inputReport, int offset = 0)
        {
            ReportId = inputReport[0 + offset];

            LeftThumbX = inputReport[1 + offset];
            LeftThumbY = inputReport[2 + offset];
            RightThumbX = inputReport[3 + offset];
            RightThumbY = inputReport[4 + offset];
            LeftTrigger = inputReport[5 + offset];
            RightTrigger = inputReport[6 + offset];

            Triangle = (inputReport[8 + offset] & (1 << 7)) != 0;
            Circle = (inputReport[8 + offset] & (1 << 6)) != 0;
            Cross = (inputReport[8 + offset] & (1 << 5)) != 0;
            Square = (inputReport[8 + offset] & (1 << 4)) != 0;

            DPad = (DPadDirection)(inputReport[8 + offset] & 0x0F);

            LeftThumb = (inputReport[9 + offset] & (1 << 7)) != 0;
            RightThumb = (inputReport[9 + offset] & (1 << 6)) != 0;
            Options = (inputReport[9 + offset] & (1 << 5)) != 0;
            Share = (inputReport[9 + offset] & (1 << 4)) != 0;
            RightTriggerButton = (inputReport[9 + offset] & (1 << 3)) != 0;
            LeftTriggerButton = (inputReport[9 + offset] & (1 << 2)) != 0;
            RightShoulder = (inputReport[9 + offset] & (1 << 1)) != 0;
            LeftShoulder = (inputReport[9 + offset] & (1 << 0)) != 0;

            PS = (inputReport[10 + offset] & (1 << 0)) != 0;

            var touchX = ((inputReport[35 + offset] & 0xF) << 8) | inputReport[34 + offset];

            TouchClick = (inputReport[10 + offset] & (1 << 1)) != 0;
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