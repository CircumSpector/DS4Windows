using System.Runtime.InteropServices;

namespace Vapour.Shared.Common.Util;

public static class StructHelpers
{
    public static void ToBytes<T>(this ref T value, byte[] buffer)
        where T : struct
    {
        MemoryMarshal.Write(buffer, ref value);
    }

    public static T ToStruct<T>(this ReadOnlySpan<byte> data)
        where T : struct
    {
        return MemoryMarshal.AsRef<T>(data);
    }

    public static T ToStruct<T>(this byte[] data)
        where T : struct
    {
        return ToStruct<T>((ReadOnlySpan<byte>)data);
    }
}

public interface IStructArray<T> where T : unmanaged
{
    ref T this[int index] { get; }
    int Length { get; }
    Span<T> AsSpan();
}

public struct StructArray1<T> : IStructArray<T> where T : unmanaged
{
    public StructArray1()
    {

    }

    public StructArray1(T[] data)
    {
        data.CopyTo(AsSpan());
    }

    private T _first;
    public int Length => 1;
    public ref T this[int index] => ref AsSpan()[index];
    public Span<T> AsSpan() => MemoryMarshal.CreateSpan(ref _first, 1);
}

public struct StructArray2<T> : IStructArray<T> where T : unmanaged
{
    public StructArray2()
    {

    }

    public StructArray2(T[] data)
    {
        data.CopyTo(AsSpan());
    }

    private T _first;
    private StructArray1<T> _remainder;
    public int Length => 2;
    public ref T this[int index] => ref AsSpan()[index];
    public Span<T> AsSpan() => MemoryMarshal.CreateSpan(ref _first, 2);
}

public struct StructArray3<T> : IStructArray<T> where T : unmanaged
{
    public StructArray3()
    {

    }

    public StructArray3(T[] data)
    {
        data.CopyTo(AsSpan());
    }

    private T _first;
    private StructArray2<T> _remainder;
    public int Length => 3;
    public ref T this[int index] => ref AsSpan()[index];
    public Span<T> AsSpan() => MemoryMarshal.CreateSpan(ref _first, 3);
}

public struct StructArray4<T> : IStructArray<T> where T : unmanaged
{
    public StructArray4()
    {

    }

    public StructArray4(T[] data)
    {
        data.CopyTo(AsSpan());
    }

    private T _first;
    private StructArray3<T> _remainder;
    public int Length => 4;
    public ref T this[int index] => ref AsSpan()[index];
    public Span<T> AsSpan() => MemoryMarshal.CreateSpan(ref _first, 4);
}

public struct StructArray5<T> : IStructArray<T> where T : unmanaged
{
    public StructArray5()
    {

    }

    public StructArray5(T[] data)
    {
        data.CopyTo(AsSpan());
    }

    private T _first;
    private StructArray4<T> _remainder;
    public int Length => 5;
    public ref T this[int index] => ref AsSpan()[index];
    public Span<T> AsSpan() => MemoryMarshal.CreateSpan(ref _first, 5);
}

public struct StructArray6<T> : IStructArray<T> where T : unmanaged
{
    public StructArray6()
    {

    }

    public StructArray6(T[] data)
    {
        data.CopyTo(AsSpan());
    }

    private T _first;
    private StructArray5<T> _remainder;
    public int Length => 6;
    public ref T this[int index] => ref AsSpan()[index];
    public Span<T> AsSpan() => MemoryMarshal.CreateSpan(ref _first, 6);
}

public struct StructArray7<T> : IStructArray<T> where T : unmanaged
{
    public StructArray7()
    {

    }

    public StructArray7(T[] data)
    {
        data.CopyTo(AsSpan());
    }

    private T _first;
    private StructArray6<T> _remainder;
    public int Length => 7;
    public ref T this[int index] => ref AsSpan()[index];
    public Span<T> AsSpan() => MemoryMarshal.CreateSpan(ref _first, 7);
}

public struct StructArray8<T> : IStructArray<T> where T : unmanaged
{
    public StructArray8()
    {

    }

    public StructArray8(T[] data)
    {
        data.CopyTo(AsSpan());
    }

    private T _first;
    private StructArray7<T> _remainder;
    public int Length => 8;
    public ref T this[int index] => ref AsSpan()[index];
    public Span<T> AsSpan() => MemoryMarshal.CreateSpan(ref _first, 8);
}

public struct StructArray9<T> : IStructArray<T> where T : unmanaged
{
    public StructArray9()
    {

    }

    public StructArray9(T[] data)
    {
        data.CopyTo(AsSpan());
    }

    private T _first;
    private StructArray8<T> _remainder;
    public int Length => 9;
    public ref T this[int index] => ref AsSpan()[index];
    public Span<T> AsSpan() => MemoryMarshal.CreateSpan(ref _first, 9);
}

public struct StructArray10<T> : IStructArray<T> where T : unmanaged
{
    public StructArray10()
    {

    }

    public StructArray10(T[] data)
    {
        data.CopyTo(AsSpan());
    }

    private T _first;
    private StructArray9<T> _remainder;
    public int Length => 10;
    public ref T this[int index] => ref AsSpan()[index];
    public Span<T> AsSpan() => MemoryMarshal.CreateSpan(ref _first, 10);
}