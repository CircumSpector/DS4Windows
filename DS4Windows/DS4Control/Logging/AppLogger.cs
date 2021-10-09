using System;
using DS4WinWPF.DS4Control.Attributes;
using Microsoft.Extensions.Logging;

namespace DS4WinWPF.DS4Control.Logging
{
    public class LogEntryEventArgs : EventArgs
    {
        public LogEntryEventArgs(string data, bool warn, bool temporary = false)
        {
            Data = data;
            IsWarning = warn;
            IsTemporary = temporary;
        }

        public DateTime Time { get; protected set; } = DateTime.Now;

        public string Data { get; protected set; }

        public bool IsWarning { get; protected set; }

        public bool IsTemporary { get; protected set; }
    }

    /// <summary>
    ///     Utility class to send log messages visible in UI or tray area.
    /// </summary>
    public class AppLogger
    {
        private readonly ILogger<AppLogger> logger;

        public AppLogger(ILogger<AppLogger> logger)
        {
            this.logger = logger;

            Instance = this;
        }

        /// <summary>
        ///     TODO: intermediate hack until DI is propagated throughout all classes
        /// </summary>
        [IntermediateSolution]
        public static AppLogger Instance { get; private set; }

        public event EventHandler<LogEntryEventArgs> NewTrayAreaLog;

        public event EventHandler<LogEntryEventArgs> NewGuiLog;

        public void LogToGui(string data, bool warning, bool temporary = false)
        {
            //
            // Proxy UI logging through to logging sub-system (e.g. file)
            // 
            if (warning)
                logger.LogWarning(data);
            else
                logger.LogInformation(data);

            NewGuiLog?.Invoke(null, new LogEntryEventArgs(data, warning, temporary));
        }

        public void LogToTray(string data, bool warning = false, bool ignoreSettings = false)
        {
            NewTrayAreaLog?.Invoke(ignoreSettings, new LogEntryEventArgs(data, warning));
        }
    }
}