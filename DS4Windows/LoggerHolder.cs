using DS4Windows;
using NLog;
using NLog.Targets;
using NLog.Targets.Wrappers;

namespace DS4WinWPF
{
    public class LoggerHolder
    {
        public LoggerHolder(ControlService service)
        {
            var configuration = LogManager.Configuration;
            var wrapTarget = configuration.FindTargetByName<WrapperTargetBase>("logfile");
            var fileTarget = wrapTarget.WrappedTarget as FileTarget;
            fileTarget.FileName = $@"{Global.RuntimeAppDataPath}\Logs\ds4windows_log.txt";
            fileTarget.ArchiveFileName = $@"{Global.RuntimeAppDataPath}\Logs\ds4windows_log_{{#}}.txt";
            LogManager.Configuration = configuration;
            LogManager.ReconfigExistingLoggers();

            Logger = LogManager.GetCurrentClassLogger();

            service.Debug += WriteToLog;
            AppLogger.GuiLog += WriteToLog;
        }

        public Logger Logger { get; }

        private void WriteToLog(object sender, DebugEventArgs e)
        {
            if (e.Temporary) return;

            if (!e.Warning)
                Logger.Info(e.Data);
            else
                Logger.Warn(e.Data);
        }
    }
}