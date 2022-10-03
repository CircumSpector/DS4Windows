using System.Drawing;

namespace Vapour.Shared.Common.Types
{
    /// <summary>
    ///     Lightbar-specific properties like colors etc.
    /// </summary>
    public class LightbarDS4WinInfo
    {
        public bool UseCustomLed { get; set; } = false;

        public bool LedAsBattery { get; set; } = false;

        public DS4Color CustomLed { get; set; } = new(Color.Blue);

        public DS4Color Led { get; set; } = new(Color.Blue);

        public DS4Color LowLed { get; set; } = new(Color.Black);

        public DS4Color ChargingLed { get; set; } = new(Color.Black);

        public DS4Color FlashLed { get; set; } = new(Color.Black);

        public double Rainbow { get; set; }

        public double MaxRainbowSaturation { get; set; } = 1.0;

        /// <summary>
        ///     Battery % when flashing occurs. Smaller 0 means disabled.
        /// </summary>
        public int FlashAt { get; set; }

        public byte FlashType { get; set; }

        public int ChargingType { get; set; }
    }
}