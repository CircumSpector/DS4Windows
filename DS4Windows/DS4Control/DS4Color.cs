using System;
using System.Windows.Media;
using Newtonsoft.Json;

namespace DS4Windows
{
    public class DS4Color : IEquatable<DS4Color>, ICloneable
    {
        public DS4Color()
        {
        }

        public DS4Color(Color color)
        {
            Red = color.R;
            Green = color.G;
            Blue = color.B;
        }

        public DS4Color(System.Drawing.Color c)
        {
            Red = c.R;
            Green = c.G;
            Blue = c.B;
        }

        public DS4Color(byte r, byte g, byte b)
        {
            Red = r;
            Green = g;
            Blue = b;
        }

        public byte Red { get; set; }

        public byte Green { get; set; }

        public byte Blue { get; set; }

        [JsonIgnore]
        public System.Drawing.Color ToColorA
        {
            get
            {
                var alphacolor = Math.Max(Red, Math.Max(Green, Blue));
                var reg = System.Drawing.Color.FromArgb(Red, Green, Blue);
                var full = HuetoRGB(reg.GetHue(), reg.GetBrightness(), ref reg);
                return System.Drawing.Color.FromArgb(alphacolor > 205 ? 255 : alphacolor + 50, full);
            }
        }

        public object Clone()
        {
            return MemberwiseClone();
        }

        public bool Equals(DS4Color other)
        {
            return Red == other.Red && Green == other.Green && Blue == other.Blue;
        }

        public Color ToColor()
        {
            return new Color
            {
                A = 255,
                R = Red,
                G = Green,
                B = Blue
            };
        }

        private System.Drawing.Color HuetoRGB(float hue, float light, ref System.Drawing.Color rgb)
        {
            var L = (float)Math.Max(.5, light);
            var C = 1 - Math.Abs(2 * L - 1);
            var X = C * (1 - Math.Abs(hue / 60 % 2 - 1));
            var m = L - C / 2;
            float R = 0, G = 0, B = 0;
            if (light == 1) return System.Drawing.Color.White;

            if (rgb.R == rgb.G && rgb.G == rgb.B) return System.Drawing.Color.White;

            if (0 <= hue && hue < 60)
            {
                R = C;
                G = X;
            }
            else if (60 <= hue && hue < 120)
            {
                R = X;
                G = C;
            }
            else if (120 <= hue && hue < 180)
            {
                G = C;
                B = X;
            }
            else if (180 <= hue && hue < 240)
            {
                G = X;
                B = C;
            }
            else if (240 <= hue && hue < 300)
            {
                R = X;
                B = C;
            }
            else if (300 <= hue && hue < 360)
            {
                R = C;
                B = X;
            }

            return System.Drawing.Color.FromArgb((int)((R + m) * 255), (int)((G + m) * 255), (int)((B + m) * 255));
        }

        public override string ToString()
        {
            return $"Red: {Red} Green: {Green} Blue: {Blue}";
        }
    }
}