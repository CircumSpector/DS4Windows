using System;

namespace DS4WinWPF.DS4Control.Attributes
{
    /// <summary>
    ///     Marks components that engage in logging.
    /// </summary>
    [AttributeUsage(AttributeTargets.All)]
    internal class LoggingComponentAttribute : Attribute
    {
    }
}