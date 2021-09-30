using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Threading;
using System.Windows.Data;
using DS4Windows;
using DS4WinWPF.DS4Control.Logging;

namespace DS4WinWPF.DS4Forms.ViewModels
{
    public class LogViewModel
    {
        //private object _colLockobj = new object();
        private readonly ReaderWriterLockSlim _logListLocker = new();

        public LogViewModel(ControlService service)
        {
            var version = Global.ExecutableProductVersion;
            LogItems.Add(new LogItem { Time = DateTime.Now, Message = $"DS4Windows version {version}" });
            LogItems.Add(new LogItem
            {
                Time = DateTime.Now,
                Message = $"DS4Windows Assembly Architecture: {(Environment.Is64BitProcess ? "x64" : "x86")}"
            });
            LogItems.Add(new LogItem { Time = DateTime.Now, Message = $"OS Version: {Environment.OSVersion}" });
            LogItems.Add(new LogItem
                { Time = DateTime.Now, Message = $"OS Product Name: {DS4Windows.Util.GetOSProductName()}" });
            LogItems.Add(new LogItem
                { Time = DateTime.Now, Message = $"OS Release ID: {DS4Windows.Util.GetOSReleaseId()}" });
            LogItems.Add(new LogItem
            {
                Time = DateTime.Now,
                Message = $"System Architecture: {(Environment.Is64BitOperatingSystem ? "x64" : "x32")}"
            });

            //logItems.Add(new LogItem { Datetime = DateTime.Now, Message = "DS4Windows version 2.0" });
            //BindingOperations.EnableCollectionSynchronization(logItems, _colLockobj);
            BindingOperations.EnableCollectionSynchronization(LogItems, _logListLocker, LogLockCallback);
            service.Debug += AddLogMessage;
            AppLogger.NewGuiLog += AddLogMessage;
        }

        public ObservableCollection<LogItem> LogItems { get; } = new();

        private void LogLockCallback(IEnumerable collection, object context, Action accessMethod, bool writeAccess)
        {
            if (writeAccess)
                using (var locker = new WriteLocker(_logListLocker))
                {
                    accessMethod?.Invoke();
                }
            else
                using (var locker = new ReadLocker(_logListLocker))
                {
                    accessMethod?.Invoke();
                }
        }

        private void AddLogMessage(object sender, LogEntryEventArgs e)
        {
            var item = new LogItem { Time = e.Time, Message = e.Data, IsWarning = e.IsWarning };
            _logListLocker.EnterWriteLock();
            LogItems.Add(item);
            _logListLocker.ExitWriteLock();
            //lock (_colLockobj)
            //{
            //    logItems.Add(item);
            //}
        }
    }
}