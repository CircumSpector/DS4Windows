namespace Vapour.Shared.Common.Types
{
    public class StickDeadZoneInfo
    {
        public enum DeadZoneType : ushort
        {
            Radial,
            Axial
        }

        public const int DefaultDeadZone = 10;
        public const int DefaultAntiDeadZone = 20;
        public const int DefaultMaxZone = 100;
        public const double DefaultMaxOutput = 100.0;
        public const bool DefaultMaxOutputForce = false;
        public const int DefaultFuzz = 0;
        public const DeadZoneType DefaultDeadZoneType = DeadZoneType.Radial;
        public const double DefaultVerticalScale = 100.0;
        public const double DefaultOuterBindDead = 75.0;
        public const bool DefaultOuterBindInvert = false;

        // DeadZone value from 0-127 (old bad convention)
        public int DeadZone { get; set; }

        public int AntiDeadZone { get; set; }

        public int MaxZone { get; set; } = DefaultMaxZone;

        public double MaxOutput { get; set; } = DefaultMaxOutput;

        public bool MaxOutputForce { get; set; } = DefaultMaxOutputForce;

        public int Fuzz { get; set; } = DefaultFuzz;

        public double VerticalScale { get; set; } = DefaultVerticalScale;

        public DeadZoneType DZType { get; set; } = DefaultDeadZoneType;

        public double OuterBindDeadZone { get; set; } = DefaultOuterBindDead;

        public bool OuterBindInvert { get; set; } = DefaultOuterBindInvert;

        public AxisDeadZoneInfo XAxisDeadInfo { get; set; } = new();

        public AxisDeadZoneInfo YAxisDeadInfo { get; set; } = new();

        public void Reset()
        {
            DeadZone = 0;
            AntiDeadZone = 0;
            MaxZone = DefaultMaxZone;
            MaxOutput = DefaultMaxOutput;
            MaxOutputForce = DefaultMaxOutputForce;

            Fuzz = DefaultFuzz;
            VerticalScale = DefaultVerticalScale;
            DZType = DefaultDeadZoneType;
            OuterBindDeadZone = DefaultOuterBindDead;
            OuterBindInvert = DefaultOuterBindInvert;
            XAxisDeadInfo.Reset();
            YAxisDeadInfo.Reset();
        }

        public class AxisDeadZoneInfo
        {
            // DeadZone value from 0-127 (old bad convention)
            public int DeadZone { get; set; } = DefaultDeadZone;

            public int AntiDeadZone { get; set; } = DefaultAntiDeadZone;

            public int MaxZone { get; set; } = DefaultMaxZone;

            public double MaxOutput { get; set; } = DefaultMaxOutput;

            public void Reset()
            {
                DeadZone = DefaultDeadZone;
                AntiDeadZone = DefaultAntiDeadZone;
                MaxZone = DefaultMaxZone;
                MaxOutput = DefaultMaxOutput;
            }
        }
    }
}