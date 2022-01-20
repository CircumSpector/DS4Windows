namespace DS4Windows.Shared.Core.HID
{
    public enum DPadDirection
    {
        Default = 0x8,
        NorthWest = 0x7,
        West = 0x6,
        SouthWest = 0x5,
        South = 0x4,
        SouthEast = 0x3,
        East = 0x2,
        NorthEast = 0x1,
        North = 0x0
    }

    /// <summary>
    ///     Describes the bare minimum common properties an input report of any compatible device can deliver.
    /// </summary>
    public class CompatibleHidDeviceInputReport
    {
        private byte ReportId { get; init; }

        public byte? Battery { get; init; }

        private DPadDirection DPad { get; init; }

        public ushort Timestamp { get; init; }

        public byte FrameCounter { get; init; } = byte.MaxValue;

        public byte LeftShoulder { get; init; }

        public byte RightShoulder { get; init; }

        public byte LeftTrigger { get; init; }

        public byte RightTrigger { get; init; }

        public bool LeftThumb { get; init; }

        public bool RightThumb { get; init; }

        public bool Share { get; init; }

        public bool Options { get; init; }

        public bool PS { get; init; }

        public bool Square { get; init; }

        public bool Triangle { get; init; }

        public bool Circle { get; init; }

        public bool Cross { get; init; }

        public byte LeftThumbX { get; init; } = 128;

        public byte LeftThumbY { get; init; } = 128;

        public byte RightThumbX { get; init; } = 128;

        public byte RightThumbY { get; init; } = 128;
    }
}