using System;

namespace DS4Windows.Shared.Common.Attributes
{
    /// <summary>
    ///     Attribute used to keep track of code loading from or writing to disk. Slap this attribute on every method reading,
    ///     writing or modifying the current XML-based configuration system to ease finding sections required to migrate.
    /// </summary>
    [AttributeUsage(AttributeTargets.All)]
    public class ConfigurationSystemComponentAttribute : Attribute
    {
        /// <summary>
        ///     Instantiate attribute.
        /// </summary>
        /// <param name="remark">Optional remark to add.</param>
        public ConfigurationSystemComponentAttribute(string remark = null)
        {
            Remark = remark;
        }

        public string Remark { get; }
    }
}