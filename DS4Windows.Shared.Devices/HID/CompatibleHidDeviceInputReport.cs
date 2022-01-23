namespace DS4Windows.Shared.Devices.HID
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
        public byte ReportId { get; protected set; }

        public byte? Battery { get; protected set; }

        public DPadDirection DPad { get; protected set; }

        public ushort Timestamp { get; protected set; }

        public byte FrameCounter { get; protected set; }

        public bool LeftShoulder { get; protected set; }

        public bool RightShoulder { get; protected set; }

        public byte LeftTrigger { get; protected set; }

        public bool LeftTriggerButton { get; protected set; }

        public byte RightTrigger { get; protected set; }
        
        public bool RightTriggerButton { get; protected set; }

        public bool LeftThumb { get; protected set; }

        public bool RightThumb { get; protected set; }

        public bool Share { get; protected set; }

        public bool Options { get; protected set; }

        public bool PS { get; protected set; }

        public bool Square { get; protected set; }

        public bool Triangle { get; protected set; }

        public bool Circle { get; protected set; }

        public bool Cross { get; protected set; }

        public byte LeftThumbX { get; protected set; } = 128;

        public byte LeftThumbY { get; protected set; } = 128;

        public byte RightThumbX { get; protected set; } = 128;

        public byte RightThumbY { get; protected set; } = 128;

        /// <summary>
        ///     Parse a raw byte array into this <see cref="CompatibleHidDeviceInputReport"/>.
        /// </summary>
        /// <param name="inputReport">The raw input report buffer.</param>
        /// <param name="offset">An optional offset where to expect the start byte (report ID).</param>
        public virtual void ParseFrom(byte[] inputReport, int offset = 0)
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
        }
    }
}