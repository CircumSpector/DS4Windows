using FastDeepCloner;

namespace DS4WinWPF.DS4Control.Util
{
    internal static class CloningExtensions
    {
        /// <summary>
        ///     Performs deep (full) copy of object and related graph
        /// </summary>
        public static T DeepClone<T>(this T obj)
        {
            return (T)obj.Clone();
        }

        /// <summary>
        ///     Performs deep (full) copy of object and related graph to existing object
        /// </summary>
        /// <returns>existing filled object</returns>
        /// <remarks>Method is valid only for classes, classes should be descendants in reality, not in declaration</remarks>
        public static TTo DeepCloneTo<TFrom, TTo>(this TFrom objFrom, TTo objTo) where TTo : class, TFrom
        {
            objFrom.CloneTo(objTo);

            return objTo;
        }
    }
}