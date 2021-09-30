using System;

namespace DS4WinWPF.DS4Control.Attributes
{
    /// <summary>
    ///     Attribute to tag sections that are volatile and will be replaced in the near future.
    /// </summary>
    [AttributeUsage(AttributeTargets.All)]
    internal class IntermediateSolutionAttribute : Attribute
    {
    }
}