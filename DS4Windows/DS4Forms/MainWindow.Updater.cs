using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;
using DS4Windows;
using HttpProgress;
using MessageBox = AdonisUI.Controls.MessageBox;

namespace DS4WinWPF.DS4Forms
{
    public partial class MainWindow
    {
        private void DownloadUpstreamVersionInfo()
        {
            // Sorry other devs, gonna have to find your own server
            var url = new Uri("https://raw.githubusercontent.com/Ryochan7/DS4Windows/jay/DS4Windows/newest.txt");
            var filename = Global.RuntimeAppDataPath + "\\version.txt";
            var success = false;
            using (var downloadStream = new FileStream(filename, FileMode.Create))
            {
                var temp = App.requestClient.GetAsync(url.ToString(), downloadStream);
                try
                {
                    temp.Wait();
                    if (temp.Result.IsSuccessStatusCode) success = true;
                }
                catch (AggregateException)
                {
                }
            }

            if (!success && File.Exists(filename)) File.Delete(filename);
        }

        private string DownloadUpstreamUpdaterVersion()
        {
            var result = string.Empty;
            // Sorry other devs, gonna have to find your own server
            var url = new Uri("https://raw.githubusercontent.com/Ryochan7/DS4Updater/master/Updater2/newest.txt");
            var filename = Path.Combine(Path.GetTempPath(), "DS4Updater_version.txt");
            var readFile = false;
            using (var downloadStream = new FileStream(filename, FileMode.Create))
            {
                var temp = App.requestClient.GetAsync(url.ToString(), downloadStream);
                temp.Wait();

                if (temp.Result.IsSuccessStatusCode) readFile = true;
            }

            if (readFile)
            {
                result = File.ReadAllText(filename).Trim();
                File.Delete(filename);
            }

            return result;
        }

        private void Check_Version(bool showstatus = false)
        {
            var version = Global.ExecutableProductVersion;
            var newversion = string.Empty;
            var versionFilePath = Path.Combine(Global.RuntimeAppDataPath, "version.txt");
            var lastVersionNum = Global.Instance.Config.LastVersionCheckedNumber;
            //ulong lastVersion = Global.CompileVersionNumberFromString("2.1.1");

            var versionFileExists = File.Exists(versionFilePath);
            if (versionFileExists)
                newversion = File.ReadAllText(versionFilePath).Trim();
            //newversion = "2.1.3";

            var newversionNum = !string.IsNullOrEmpty(newversion)
                ? Global.CompileVersionNumberFromString(newversion)
                : 0;

            if (!string.IsNullOrWhiteSpace(newversion) && version.CompareTo(newversion) != 0 &&
                lastVersionNum < newversionNum)
            {
                var result = MessageBoxResult.No;
                Dispatcher.Invoke(() =>
                {
                    var updaterWin = new UpdaterWindow(newversion);
                    updaterWin.ShowDialog();
                    result = updaterWin.Result;
                });

                if (result == MessageBoxResult.Yes)
                {
                    var launch = true;
                    launch = RunUpdaterCheck(launch);

                    if (launch)
                    {
                        using var p = new Process();

                        p.StartInfo.FileName = Path.Combine(Global.ExecutableDirectory, "DS4Updater.exe");
                        var isAdmin = Global.IsAdministrator;
                        var argList = new List<string>();
                        argList.Add("-autolaunch");

                        if (!isAdmin) argList.Add("-user");

                        // Specify current exe to have DS4Updater launch
                        argList.Add("--launchExe");
                        argList.Add(Global.ExecutableFileName);

                        p.StartInfo.Arguments = string.Join(" ", argList);
                        if (Global.IsAdminNeeded)
                            p.StartInfo.Verb = "runas";

                        try
                        {
                            launch = p.Start();
                        }
                        catch (InvalidOperationException)
                        {
                        }
                    }

                    if (launch)
                        Dispatcher.BeginInvoke((Action)(() =>
                        {
                            contextclose = true;
                            Close();
                        }));
                    else
                        Dispatcher.Invoke(() =>
                        {
                            MessageBox.Show(Properties.Resources.PleaseDownloadUpdater);
                            Util.StartProcessHelper(
                                $"https://github.com/Ryochan7/DS4Updater/releases/tag/v{version}/{mainWinVM.updaterExe}");
                        });
                    //Process.Start($"https://github.com/Ryochan7/DS4Updater/releases/download/v{version}/{mainWinVM.updaterExe}");
                }
                else
                {
                    if (versionFileExists)
                        File.Delete(Path.Combine(Global.RuntimeAppDataPath, "version.txt"));
                }
            }
            else
            {
                if (versionFileExists)
                    File.Delete(Path.Combine(Global.RuntimeAppDataPath, "version.txt"));

                if (showstatus)
                    Dispatcher.Invoke(() => MessageBox.Show(Properties.Resources.UpToDate, "DS4Windows Updater"));
            }
        }

        private bool RunUpdaterCheck(bool launch)
        {
            var destPath = Global.ExecutableDirectory + "\\DS4Updater.exe";
            var updaterExists = File.Exists(destPath);
            var version = DownloadUpstreamUpdaterVersion();
            if (!updaterExists ||
                !string.IsNullOrEmpty(version) &&
                FileVersionInfo.GetVersionInfo(destPath).FileVersion.CompareTo(version) != 0)
            {
                launch = false;
                var url2 = new Uri(
                    $"https://github.com/Ryochan7/DS4Updater/releases/download/v{version}/{mainWinVM.updaterExe}");
                var filename = Path.Combine(Path.GetTempPath(), "DS4Updater.exe");
                using (var downloadStream = new FileStream(filename, FileMode.Create))
                {
                    var temp =
                        App.requestClient.GetAsync(url2.ToString(), downloadStream);
                    temp.Wait();
                    if (temp.Result.IsSuccessStatusCode) launch = true;
                }

                if (launch)
                {
                    if (Global.IsAdminNeeded)
                    {
                        var copyStatus = Util.ElevatedCopyUpdater(filename);
                        if (copyStatus != 0) launch = false;
                    }
                    else
                    {
                        if (updaterExists) File.Delete(destPath);
                        File.Move(filename, destPath);
                    }
                }
            }

            return launch;
        }
    }
}