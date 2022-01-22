using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Windows;
using AdonisUI.Controls;
using DS4Windows.Shared.Configuration.Application.Services;
using DS4WinWPF.DS4Control.IoC.Services;
using HttpProgress;
using NonFormTimer = System.Timers.Timer;

namespace DS4WinWPF.DS4Forms
{
    /// <summary>
    /// Interaction logic for WelcomeDialog.xaml
    /// </summary>
    public partial class WelcomeDialog : AdonisWindow
    {
        private const string InstallerFakerInputX64 = "https://github.com/Ryochan7/FakerInput/releases/download/v0.1.0/FakerInput_0.1.0_x64.msi";
        private const string InstallerFakerInputX86 = "https://github.com/Ryochan7/FakerInput/releases/download/v0.1.0/FakerInput_0.1.0_x86.msi";

        private const string InstFileName1_16 = "ViGEmBus_Setup_1.16.116.exe";
        private const string InstFileNameX64 = "ViGEmBusSetup_x64.msi";
        private const string InstFileNameX86 = "ViGEmBusSetup_x86.msi";
        private string tempInstFileName;

        private const string InstHidHideFileNameX64 = "HidHideMSI.msi";

        private string installFakerInputDL = "";
        private string instFakerInputFileName = "";

        private string installFileName = InstFileNameX64;

        Process monitorProc;
        NonFormTimer monitorTimer;

        private readonly IAppSettingsService appSettings;

        public WelcomeDialog(IAppSettingsService appSettings, bool loadConfig = false)
        {
            this.appSettings = appSettings;

            if (loadConfig)
            {
                DS4Windows.Global.Instance.FindConfigLocation();
                appSettings.LoadAsync().Wait();
                //DS4Windows.Global.SetCulture(DS4Windows.Global.UseLang);
            }

            InitializeComponent();

            // Run checks for compatible version of ViGEmBus
            if (!DS4Windows.Global.IsWin10OrGreater)
            {
                installFileName = InstFileName1_16;
            }
            else if (!Environment.Is64BitOperatingSystem)
            {
                installFileName = InstFileNameX86;
            }

            // Run checks for FakerInput driver
            if (DS4Windows.Global.IsWin8OrGreater)
            {
                if (Environment.Is64BitOperatingSystem)
                {
                    installFakerInputDL = InstallerFakerInputX64;
                    instFakerInputFileName = new FileInfo(InstallerFakerInputX64).Name;
                }
                else if (!Environment.Is64BitOperatingSystem)
                {
                    installFileName = InstFileNameX86;

                    installFakerInputDL = InstallerFakerInputX86;
                    instFakerInputFileName = new FileInfo(InstallerFakerInputX86).Name;
                }
            }
            else
            {
                step5FakerInputPanel.IsEnabled = false;
            }

            tempInstFileName = DS4Windows.Global.ExecutableDirectory + $"\\{installFileName}.tmp";

            // Disable Xbox 360 driver installer button if running on Windows 8 or greater.
            // Driver comes pre-installed on a standard OS install
            if (DS4Windows.Global.IsWin8OrGreater)
            {
                step2Btn.IsEnabled = false;
            }

            // HidHide only works on Windows 10 x64
            if (!IsHidHideControlCompatible())
            {
                step4HidHidePanel.IsEnabled = false;
            }

            // Just leave panel disabled for now. Download link does not
            // exist currently
            //step5FakerInputPanel.IsEnabled = false;
        }

        private bool IsHidHideControlCompatible()
        {
            // HidHide only works on Windows 10 x64
            return DS4Windows.Global.IsWin10OrGreater &&
                Environment.Is64BitOperatingSystem;
        }

        private bool IsFakerInputControlCompatible()
        {
            // FakerInput works on Windows 8.1 and later. Going to attempt
            // to support x64 and x86 arch
            return DS4Windows.Global.IsWin8OrGreater;
        }

        private void FinishedBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Step2Btn_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("http://www.microsoft.com/accessories/en-gb/d/xbox-360-controller-for-windows");
        }

        private void BluetoothSetLink_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("control", "bthprops.cpl");
        }

        private void FakerInputInstallBtn_Click(object sender, RoutedEventArgs e)
        {
            if (File.Exists(DS4Windows.Global.ExecutableDirectory + $"\\{instFakerInputFileName}"))
            {
                File.Delete(DS4Windows.Global.ExecutableDirectory + $"\\{instFakerInputFileName}");
            }

            string tempInstFakerInputName = DS4Windows.Global.ExecutableDirectory + $"\\{instFakerInputFileName}.tmp";
            if (File.Exists(tempInstFakerInputName))
            {
                File.Delete(tempInstFakerInputName);
            }

            EnableControls(false);
            FakerInputDownloadLaunch();
        }

        private async void FakerInputDownloadLaunch()
        {
            Progress<ICopyProgress> progress = new Progress<ICopyProgress>(x => // Please see "Notes on IProgress<T>"
            {
                // This is your progress event!
                // It will fire on every buffer fill so don't do anything expensive.
                // Writing to the console IS expensive, so don't do the following in practice...
                fakerInputInstallBtn.Content = Properties.Resources.Downloading.Replace("*number*%",
                    x.PercentComplete.ToString("P"));
                //Console.WriteLine(x.PercentComplete.ToString("P"));
            });

            string tempInstFakerInputFileName = DS4Windows.Global.ExecutableDirectory + $"\\{instFakerInputFileName}.tmp";
            string filename = DS4Windows.Global.ExecutableDirectory + $"\\{instFakerInputFileName}";
            bool success = false;
            using (var downloadStream = new FileStream(tempInstFakerInputFileName, FileMode.CreateNew))
            {
                HttpResponseMessage response = await App.requestClient.GetAsync(installFakerInputDL,
                    downloadStream, progress);
                success = response.IsSuccessStatusCode;
            }

            if (success)
            {
                File.Move(tempInstFakerInputFileName, filename);
            }
            success = false; // Reset for later check

            if (File.Exists(DS4Windows.Global.ExecutableDirectory + $"\\{instFakerInputFileName}"))
            {
                //vigemInstallBtn.Content = Properties.Resources.OpeningInstaller;
                ProcessStartInfo startInfo = new ProcessStartInfo(DS4Windows.Global.ExecutableDirectory + $"\\{instFakerInputFileName}");
                startInfo.UseShellExecute = true; // Needed to run program as admin
                monitorProc = Process.Start(startInfo);
                fakerInputInstallBtn.Content = Properties.Resources.Installing;
                success = true;
            }

            if (success)
            {
                monitorTimer = new NonFormTimer();
                monitorTimer.Elapsed += FakerInputInstallTimer_Elapsed;
                monitorTimer.Start();
            }
            else
            {
                fakerInputInstallBtn.Content = Properties.Resources.InstallFailed;
                EnableControls(true);
            }
        }

        private void FakerInputInstallTimer_Elapsed(object sender,
            System.Timers.ElapsedEventArgs e)
        {
            ((NonFormTimer)sender).Stop();
            bool finished = false;
            if (monitorProc != null && monitorProc.HasExited)
            {
                if (DS4Windows.Global.IsFakerInputInstalled)
                {
                    Dispatcher.BeginInvoke((Action)(() =>
                    {
                        fakerInputInstallBtn.Content = Properties.Resources.InstallComplete;
                        EnableControls(true);
                    }));
                }
                else
                {
                    Dispatcher.BeginInvoke((Action)(() =>
                    {
                        fakerInputInstallBtn.Content = Properties.Resources.InstallFailed;
                        EnableControls(true);
                    }), null);
                }

                File.Delete(DS4Windows.Global.ExecutableDirectory + $"\\{instFakerInputFileName}");
                ((NonFormTimer)sender).Stop();
                finished = true;
            }

            if (!finished)
            {
                ((NonFormTimer)sender).Start();
            }
        }

        private void EnableControls(bool on)
        {
            vigemInstallBtn.IsEnabled = on;
            step4HidHidePanel.IsEnabled = on;
            step5FakerInputPanel.IsEnabled = on;

            // Perform compatibility checks for controls that might need
            // to be disabled when on is set to true
            if (on)
            {
                LateControlsCheck();
            }
        }

        /// <summary>
        /// Possibly disable some controls for components that are not compatible
        /// with the installed version of Windows or system configuration
        /// </summary>
        private void LateControlsCheck()
        {
            step4HidHidePanel.IsEnabled = IsHidHideControlCompatible();
            step5FakerInputPanel.IsEnabled = IsFakerInputControlCompatible();
        }
    }
}
