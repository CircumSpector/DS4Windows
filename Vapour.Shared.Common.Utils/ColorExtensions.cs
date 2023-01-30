using System.Diagnostics.CodeAnalysis;
using System.Drawing;

namespace Vapour.Shared.Common.Util;

[SuppressMessage("ReSharper", "UnusedMember.Global")]
public static class ColorConverterExtensions
{
    public static string ToHexString(this Color c)
    {
        return $"#{c.R:X2}{c.G:X2}{c.B:X2}";
    }

    public static string ToRgbString(this Color c)
    {
        return $"RGB({c.R}, {c.G}, {c.B})";
    }
}
