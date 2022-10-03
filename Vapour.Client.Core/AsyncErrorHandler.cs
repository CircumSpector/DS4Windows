using JetBrains.Annotations;
using Serilog;

namespace Vapour.Client.Core
{
    /// <summary>
    ///     Catches unhandled async exceptions.
    /// </summary>
    /// <remarks>https://github.com/Fody/AsyncErrorHandler</remarks>
    public static class AsyncErrorHandler
    {
        [UsedImplicitly]
        public static void HandleException(Exception exception)
        {
            Log.Logger.Error(exception, "Unhandled async exception occurred");
        }
    }
}
