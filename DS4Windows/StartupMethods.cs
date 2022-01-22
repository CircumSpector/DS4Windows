using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;
using DS4Windows;
using DS4Windows.Shared.Common.Core;
using Microsoft.Win32;
using Microsoft.Win32.TaskScheduler;

namespace DS4WinWPF
{
    [SuppressUnmanagedCodeSecurity]
    public static class StartupMethods
    {
        private const string net5SubKey = @"SOFTWARE\dotnet\Setup\InstalledVersions";

        public static string lnkpath =
            Environment.GetFolderPath(Environment.SpecialFolder.Startup) + "\\DS4Windows.lnk";

        private static readonly string taskBatPath = Path.Combine(Global.ExecutableDirectory, "task.bat");

        public static bool HasStartProgEntry()
        {
            // Exception handling should not be needed here. Method handles most cases
            var exists = File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.Startup) + "\\DS4Windows.lnk");
            return exists;
        }

        public static bool HasTaskEntry()
        {
            var ts = new TaskService();
            var tasker = ts.FindTask("RunDS4Windows");
            return tasker != null;
        }

        public static void WriteStartProgEntry()
        {
            var t = Type.GetTypeFromCLSID(Constants
                .WindowsScriptHostShellObjectGuild); // Windows Script Host Shell Object
            dynamic shell = Activator.CreateInstance(t);
            try
            {
                var lnk = shell.CreateShortcut(lnkpath);
                try
                {
                    var app = Global.ExecutableLocation;
                    lnk.TargetPath = Global.ExecutableLocation;
                    lnk.Arguments = "-m";

                    //lnk.TargetPath = Assembly.GetExecutingAssembly().Location;
                    //lnk.Arguments = "-m";
                    lnk.IconLocation = app.Replace('\\', '/');
                    lnk.Save();
                }
                finally
                {
                    Marshal.FinalReleaseComObject(lnk);
                }
            }
            finally
            {
                Marshal.FinalReleaseComObject(shell);
            }
        }

        public static void DeleteStartProgEntry()
        {
            if (File.Exists(lnkpath) && !new FileInfo(lnkpath).IsReadOnly) File.Delete(lnkpath);
        }

        public static void DeleteOldTaskEntry()
        {
            var ts = new TaskService();
            var tasker = ts.FindTask("RunDS4Windows");
            if (tasker != null)
                foreach (var act in tasker.Definition.Actions)
                    if (act.ActionType == TaskActionType.Execute)
                    {
                        var temp = act as ExecAction;
                        if (temp.Path != taskBatPath)
                        {
                            ts.RootFolder.DeleteTask("RunDS4Windows");
                            break;
                        }
                    }
        }

        public static bool CanWriteStartEntry()
        {
            var result = false;
            if (!new FileInfo(lnkpath).IsReadOnly) result = true;

            return result;
        }

        public static void WriteTaskEntry()
        {
            DeleteTaskEntry();

            // Create new version of task.bat file using current exe
            // filename. Allow dynamic file
            RefreshTaskBat();

            var ts = new TaskService();
            var td = ts.NewTask();
            td.Triggers.Add(new LogonTrigger());
            var dir = Global.ExecutableDirectory;
            td.Actions.Add(new ExecAction($@"{dir}\task.bat",
                "",
                dir));

            td.Principal.RunLevel = TaskRunLevel.Highest;
            td.Settings.StopIfGoingOnBatteries = false;
            td.Settings.DisallowStartIfOnBatteries = false;
            ts.RootFolder.RegisterTaskDefinition("RunDS4Windows", td);
        }

        public static void DeleteTaskEntry()
        {
            var ts = new TaskService();
            var tasker = ts.FindTask("RunDS4Windows");
            if (tasker != null) ts.RootFolder.DeleteTask("RunDS4Windows");
        }

        public static bool CheckStartupExeLocation()
        {
            var lnkprogpath = ResolveShortcut(lnkpath);
            return lnkprogpath != Global.ExecutableLocation;
        }

        public static Version NetVersionInstalled()
        {
            var result = new Version("0.0.0");
            var archLookup = Environment.Is64BitProcess ? "x64" : "x86";
            using (var baseKey = Registry.LocalMachine.OpenSubKey($@"{net5SubKey}\{archLookup}\sharedhost"))
            {
                if (baseKey != null)
                {
                    var tempVersion = baseKey.GetValue("Version")?.ToString() ?? string.Empty;
                    if (!string.IsNullOrEmpty(tempVersion)) result = new Version(tempVersion);
                }
            }

            return result;
        }

        public static void LaunchOldTask()
        {
            var ts = new TaskService();
            var tasker = ts.FindTask("RunDS4Windows");
            if (tasker != null) tasker.Run("");
        }

        private static string ResolveShortcut(string filePath)
        {
            var t = Type.GetTypeFromCLSID(Constants
                .WindowsScriptHostShellObjectGuild); // Windows Script Host Shell Object
            dynamic shell = Activator.CreateInstance(t);
            string result;

            try
            {
                var shortcut = shell.CreateShortcut(filePath);
                result = shortcut.TargetPath;
                Marshal.FinalReleaseComObject(shortcut);
            }
            catch (COMException)
            {
                // A COMException is thrown if the file is not a valid shortcut (.lnk) file 
                result = null;
            }
            finally
            {
                Marshal.FinalReleaseComObject(shell);
            }

            return result;
        }

        private static void RefreshTaskBat()
        {
            var dir = Global.ExecutableDirectory;
            var path = $@"{dir}\task.bat";
            var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write);
            using var w = new StreamWriter(fileStream);
            var temp = string.Empty;
            w.WriteLine("@echo off"); // Turn off echo
            w.WriteLine("SET mypath=\"%~dp0\"");
            temp = $"cmd.exe /c start \"RunDS4Windows\" %mypath%\\{Global.ExecutableFileName} -m";
            w.WriteLine(temp);
            w.WriteLine("exit");
        }
    }
}