using System.Runtime.InteropServices;

namespace Vapour.Shared.Common.Util;

public static class BitHelpers
{
    private static readonly uint[] BitCountConversions = {
        0x00, 0x01, 0x03, 0x07, 0x0F, 0x1F, 0x3F, 0x7F, 0xFF, 0x1FF, 0x3FF, 0x7FF, 0xFFF, 0x1FFF, 0x3FFF, 0x7FFF, 0xFFFF,
        0x1FFFF, 0x3FFFF, 0x7FFFF, 0xFFFFF, 0x1FFFFF, 0x3FFFFF, 0x7FFFFF, 0xFFFFFF, 0x1FFFFFF, 0x3FFFFFF, 0x7FFFFFF,
        0xFFFFFFF, 0x1FFFFFFF, 0x3FFFFFFF, 0x7FFFFFFF, 0xFFFFFFFF
    };

    public static byte GetBitsAsByte(this byte value, byte offset, byte count)
    {
        return (byte)((value >> offset) & BitCountConversions[count]);
    }

    public static byte GetBitsAsByte(this ushort value, byte offset, byte count)
    {
        return (byte)((value >> offset) & BitCountConversions[count]);
    }

    public static short GetBitsAsShort(this ushort value, byte offset, byte count)
    {
        return (short)((value >> offset) & BitCountConversions[count]);
    }

    public static byte GetBitsAsByte(this uint value, byte offset, byte count)
    {
        return (byte)((value >> offset) & BitCountConversions[count]);
    }

    public static short GetBitsAsShort(this uint value, byte offset, byte count)
    {
        return (short)((value >> offset) & BitCountConversions[count]);
    }

    public static int GetBitsAsInt(this uint value, byte offset, byte count)
    {
        return (int)((value >> offset) & BitCountConversions[count]);
    }

    public static byte[] StructToByte(object value, int length)
    {
        int size = length;
        byte[] arr = new byte[size];

        IntPtr ptr = IntPtr.Zero;
        try
        {
            ptr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(value, ptr, true);
            Marshal.Copy(ptr, arr, 0, size);
        }
        finally
        {
            Marshal.FreeHGlobal(ptr);
        }

        return arr;
    }
}
