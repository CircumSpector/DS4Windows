using System.Reflection;

namespace DS4Windows.Shared.Core.Util
{
    /// <summary>
    ///     https://github.com/Fody/MethodTimer
    /// </summary>
    public static class MethodTimeLogger
    {
        public static void Log(MethodBase methodBase, long milliseconds, string message)
        {
            Serilog.Log.Logger.Verbose("{Class}.{Method} took {Span}ms",
                methodBase.ReflectedType?.Name, methodBase.Name, milliseconds);
        }
    }
}