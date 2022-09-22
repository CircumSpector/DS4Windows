using Ds4Windows.Shared.Devices.Interfaces.HID;

namespace DS4Windows.Shared.Devices.HID.Devices.Reports
{
    public class DualSenseCompatibleInputReport : DualShock4CompatibleInputReport
    {
        public override void Parse(ReadOnlySpan<byte> input)
        {
            // Eliminate bounds checks
            input = input.Slice(0, 41);

            ReportId = input[0];

            LeftThumbX = input[1];
            LeftThumbY = input[2];
            RightThumbX = input[3];
            RightThumbY = input[4];
            LeftTrigger = input[5];
            RightTrigger = input[6];

            Triangle = (input[8] & (1 << 7)) != 0;
            Circle = (input[8] & (1 << 6)) != 0;
            Cross = (input[8] & (1 << 5)) != 0;
            Square = (input[8] & (1 << 4)) != 0;

            DPad = (DPadDirection)(input[8] & 0x0F);

            LeftThumb = (input[9] & (1 << 6)) != 0;
            RightThumb = (input[9] & (1 << 7)) != 0;
            Options = (input[9] & (1 << 5)) != 0;
            Share = (input[9] & (1 << 4)) != 0;
            RightTriggerButton = (input[9] & (1 << 3)) != 0;
            LeftTriggerButton = (input[9] & (1 << 2)) != 0;
            RightShoulder = (input[9] & (1 << 1)) != 0;
            LeftShoulder = (input[9] & (1 << 0)) != 0;

            PS = (input[10] & (1 << 0)) != 0;

            var touchX = ((input[35] & 0xF) << 8) | input[34];

            TouchClick = (input[10] & (1 << 1)) != 0;
            Mute = (input[10] & (1 << 2)) != 0;

            TrackPadTouch1 = new TrackPadTouch
            {
                RawTrackingNum = input[33],
                Id = (byte)(input[33] & 0x7f),
                IsActive = (input[33] & 0x80) == 0,
                X = (short)(((ushort)(input[35] & 0x0f) << 8) |
                            input[34]),
                Y = (short)((input[36] << 4) |
                            ((ushort)(input[35] & 0xf0) >> 4))
            };
            TrackPadTouch2 = new TrackPadTouch
            {
                RawTrackingNum = input[37],
                Id = (byte)(input[37] & 0x7f),
                IsActive = (input[37] & 0x80) == 0,
                X = (short)(((ushort)(input[39] & 0x0f) << 8) |
                            input[38]),
                Y = (short)((input[40] << 4) |
                            ((ushort)(input[39] & 0xf0) >> 4))
            };
            TouchPacketCounter = input[41];
            Touch1 = input[33] >> 7 == 0;
            Touch2 = input[37] >> 7 == 0;
            TouchIsOnLeftSide = !(touchX >= 1920 * 2 / 5); // TODO: port const
            TouchIsOnRightSide = !(touchX < 1920 * 2 / 5); // TODO: port const
        }
    }
}