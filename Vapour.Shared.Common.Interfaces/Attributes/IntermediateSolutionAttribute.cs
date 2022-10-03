using System;

namespace Vapour.Shared.Common.Attributes
{
    /// <summary>
    ///     Attribute to tag sections that are volatile and will be replaced in the near future.
    /// </summary>
    [AttributeUsage(AttributeTargets.All)]
    public class IntermediateSolutionAttribute : Attribute
    {
    }
}