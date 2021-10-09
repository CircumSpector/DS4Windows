using System;

namespace DS4WinWPF.DS4Control.Attributes
{
    /// <summary>
    ///     Section misses localization for non-English locales.
    /// </summary>
    [AttributeUsage(AttributeTargets.All)]
    internal class MissingLocalizationAttribute : Attribute
    {
    }
}