namespace DS4Windows.Shared.Configuration.Profiles.Types
{
    /// <summary>
    ///     Describes the state of a changed property.
    /// </summary>
    public class ProfilePropertyChangedEventArgs : EventArgs
    {
        public ProfilePropertyChangedEventArgs(string propertyName, object before, object after)
        {
            PropertyName = propertyName;
            Before = before;
            After = after;
        }

        /// <summary>
        ///     The name of the changed property.
        /// </summary>
        public string PropertyName { get; }

        /// <summary>
        ///     The value before the change has occurred.
        /// </summary>
        public object Before { get; }

        /// <summary>
        ///     The value after the change has occurred.
        /// </summary>
        public object After { get; }
    }
}
