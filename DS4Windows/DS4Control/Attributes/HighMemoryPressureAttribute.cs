using System;

namespace DS4WinWPF.DS4Control.Attributes
{
    /// <summary>
    ///     Flag components which cause high memory allocations.
    /// </summary>
    [AttributeUsage(AttributeTargets.All)]
    internal class HighMemoryPressureAttribute : Attribute
    {
    }
}