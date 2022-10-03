using System.Runtime.InteropServices;

namespace Vapour.Shared.Common.Util
{
    public static class ByteArrayUtils
    {
        public static T ByteArrayToStructure<T>(this byte[] bytes) where T : struct
        {
            T stuff;
            var handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            try
            {
                stuff = (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
            }
            finally
            {
                handle.Free();
            }

            return stuff;
        }
    }
}