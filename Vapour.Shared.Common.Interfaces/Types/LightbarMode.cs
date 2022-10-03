namespace Vapour.Shared.Common.Types
{
    public enum LightbarMode : uint
    {
        /// <summary>
        ///     Unknown state.
        /// </summary>
        None,
        /// <summary>
        ///     Application is in control of Lightbar appearance.
        /// </summary>
        DS4Win,
        /// <summary>
        ///     Game is in control of Lightbar appearance.
        /// </summary>
        Passthru
    }
}
