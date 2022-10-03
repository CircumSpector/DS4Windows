using System;

namespace Vapour.Shared.Common.Attributes
{
    /// <summary>
    ///     Flag components which cause high memory allocations.
    /// </summary>
    [AttributeUsage(AttributeTargets.All)]
    public class HighMemoryPressureAttribute : Attribute
    {
    }
}