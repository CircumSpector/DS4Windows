namespace DS4WinWPF.DS4Control.Util
{
    /// <summary>
    ///     Utility class to copy (not clone) the properties of one type to another.
    /// </summary>
    /// <typeparam name="TParent">The source type.</typeparam>
    /// <typeparam name="TChild">The target type.</typeparam>
    internal class PropertyCopier<TParent, TChild> where TParent : class
        where TChild : class
    {
        /// <summary>
        ///     Copies all properties from source to target.
        /// </summary>
        /// <param name="parent">The source object.</param>
        /// <param name="child">The target object.</param>
        public static void Copy(TParent parent, TChild child)
        {
            var parentProperties = parent.GetType().GetProperties();
            var childProperties = child.GetType().GetProperties();

            foreach (var parentProperty in parentProperties)
            foreach (var childProperty in childProperties)
                if (parentProperty.Name == childProperty.Name &&
                    parentProperty.PropertyType == childProperty.PropertyType)
                {
                    childProperty.SetValue(child, parentProperty.GetValue(parent));
                    break;
                }
        }
    }
}