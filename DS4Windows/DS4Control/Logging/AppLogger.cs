using System;
using DS4WinWPF.DS4Control.Attributes;

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
        [IntermediateSolution]
        private static readonly Lazy<AppLogger> LazyInstance = new(new AppLogger());

        [IntermediateSolution]
        public static AppLogger Instance => LazyInstance.Value;

        public event EventHandler<LogEntryEventArgs> NewTrayAreaLog;

        public event EventHandler<LogEntryEventArgs> NewGuiLog;

        public void LogToGui(string data, bool warning, bool temporary = false)
        {
            NewGuiLog?.Invoke(null, new LogEntryEventArgs(data, warning, temporary));
        }

        public void LogToTray(string data, bool warning = false, bool ignoreSettings = false)
        {
            NewTrayAreaLog?.Invoke(ignoreSettings, new LogEntryEventArgs(data, warning));
        }
    }
}