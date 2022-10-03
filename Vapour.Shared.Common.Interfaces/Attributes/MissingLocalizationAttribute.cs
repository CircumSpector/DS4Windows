using System;

namespace Vapour.Shared.Common.Attributes
{
    /// <summary>
    ///     Section misses localization for non-English locales.
    /// </summary>
    [AttributeUsage(AttributeTargets.All)]
    public class MissingLocalizationAttribute : Attribute
    {
    }
}