using System;

namespace Vapour.Shared.Common.Attributes
{
    /// <summary>
    ///     Marks components that engage in logging.
    /// </summary>
    [AttributeUsage(AttributeTargets.All)]
    public class LoggingComponentAttribute : Attribute
    {
    }
}