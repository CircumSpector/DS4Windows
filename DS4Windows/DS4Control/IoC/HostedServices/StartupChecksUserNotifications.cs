using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using AdonisUI.Controls;
using DS4Windows;
using DS4WinWPF.DS4Control.Attributes;
using DS4WinWPF.DS4Control.IoC.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Win32;

namespace DS4WinWPF.DS4Control.IoC.HostedServices
{
    /// <summary>
    ///     Executes various checks on startup and displays user notifications, if attention is required.
    /// </summary>
    internal class StartupChecksUserNotifications : BackgroundService
    {
        private readonly IAppSettingsService appSettings;

        public StartupChecksUserNotifications(IAppSettingsService appSettings)
        {
            this.appSettings = appSettings;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await CheckIsTracingEnabled();

            await CheckWindows11();

            await CheckIsSteamRunning();

            await CheckAppArchitecture();
        }

        [MissingLocalization]
        private async Task CheckIsTracingEnabled()
        {
            if (appSettings.Settings.IsTracingEnabled)
                return;

            var messageBox = new MessageBoxModel
            {
                Text =
                    "Hello, Gamer!" +
                    "\r\n\r\nYou have enabled Tracing in the application settings. This is an advanced feature useful for diagnosing "
                    + "issues with lag or stutter and general remapping performance. "
                    + "\r\n\r\nTracing is a very memory-hungry operation and requires additional software to be useful. "
                    + "Do not leave Tracing enabled if you simply wanna play your games, it's for diagnostics only."
                    + "\r\n\r\nThanks for your attention ❤️",
                Caption = "Performance Tracing is enabled",
                Icon = AdonisUI.Controls.MessageBoxImage.Warning,
                Buttons = new[]
                {
                    MessageBoxButtons.Custom("Tell me more"),
                    MessageBoxButtons.No("Uh, turn it off, please!"),
                    MessageBoxButtons.Yes("Understood")
                },
                IsSoundEnabled = false
            };

            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                AdonisUI.Controls.MessageBox.Show(Application.Current.MainWindow, messageBox);

                switch (messageBox.Result)
                {
                    case AdonisUI.Controls.MessageBoxResult.Custom:
                        DS4Windows.Util.StartProcessHelper(Constants.TracingGuideUri);
                        break;
                    case AdonisUI.Controls.MessageBoxResult.No:
                        appSettings.Settings.IsTracingEnabled = false;
                        break;
                }
            });
        }
        
        [MissingLocalization]
        private async Task CheckWindows11()
        {
            if (appSettings.Settings.HasUserConfirmedWindows11Warning)
                return;

            //
            // TODO: quite primitive but currently the most reliable check
            // 
            if (!DS4Windows.Util.BrandingFormatString("%WINDOWS_LONG%").Contains("Windows 11"))
                return;

            var messageBox = new MessageBoxModel
            {
                Text =
                    "Hello, Gamer!" +
                    "\r\n\r\nYou're running this application on Windows 11. "
                    + $"\r\n\r\nPlease bear in mind that compatibility with Windows 11 currently is in its very early stage, "
                    + "if something that worked on Windows 10 is broken, for now, you're on your own!"
                    + "\r\n\r\nThanks for your attention ❤️",
                Caption = "Windows 11 detected",
                Icon = AdonisUI.Controls.MessageBoxImage.Warning,
                Buttons = new[]
                {
                    MessageBoxButtons.Yes("Understood")
                },
                CheckBoxes = new[]
                {
                    new MessageBoxCheckBoxModel(Translations.Strings.NotAMoronConfirmationCheckbox)
                    {
                        IsChecked = false,
                        Placement = MessageBoxCheckBoxPlacement.BelowText
                    }
                },
                IsSoundEnabled = false
            };

            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                AdonisUI.Controls.MessageBox.Show(Application.Current.MainWindow, messageBox);

                appSettings.Settings.HasUserConfirmedWindows11Warning = messageBox.CheckBoxes.First().IsChecked;
            });
        }

        [MissingLocalization]
        private async Task CheckIsSteamRunning()
        {
            if (appSettings.Settings.HasUserConfirmedSteamWarning)
                return;

            using var key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Valve\\Steam\\ActiveProcess");

            if (key?.GetValue("pid") is not int pid || pid == 0) return;

            var messageBox = new MessageBoxModel
            {
                Text =
                    "Hello, Gamer!" +
                    "\r\n\r\nIt has been detected that Steam is running. "
                    + $"\r\n\r\nSteam itself offers native support for many game controllers {Constants.ApplicationName} "
                    + "supports, as well as the virtual controllers produced in the process. "
                    + $"\r\n\r\nSteam can detect {Constants.ApplicationName} running and alters its behaviour to "
                    + "not interfere, but depending on your Steam and DS4Windows settings you can still suffer "
                    + "from remapping conflicts between the two. "
                    + "\r\n\r\nIt is highly recommended that you seek aid in the online documentation for more details, " +
                    "should you encounter issues."
                    + "\r\n\r\nThanks for your attention ❤️",
                Caption = "Steam is running",
                Icon = AdonisUI.Controls.MessageBoxImage.Warning,
                Buttons = new[]
                {
                    MessageBoxButtons.Custom("Show me what to do"),
                    MessageBoxButtons.Yes("Understood")
                },
                CheckBoxes = new[]
                {
                    new MessageBoxCheckBoxModel(Translations.Strings.NotAMoronConfirmationCheckbox)
                    {
                        IsChecked = false,
                        Placement = MessageBoxCheckBoxPlacement.BelowText
                    }
                },
                IsSoundEnabled = false
            };

            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                AdonisUI.Controls.MessageBox.Show(Application.Current.MainWindow, messageBox);

                appSettings.Settings.HasUserConfirmedSteamWarning = messageBox.CheckBoxes.First().IsChecked;

                if (messageBox.Result == AdonisUI.Controls.MessageBoxResult.Custom)
                    DS4Windows.Util.StartProcessHelper(Constants.SteamTroubleshootingUri);
            });
        }

        [MissingLocalization]
        private async Task CheckAppArchitecture()
        {
            if (appSettings.Settings.HasUserConfirmedArchitectureWarning)
                return;

            if (!Environment.Is64BitOperatingSystem || Environment.Is64BitProcess) return;

            var messageBox = new MessageBoxModel
            {
                Text =
                    "Hello, Gamer!" +
                    "\r\n\r\nYou're running the 32-Bit edition on a 64-Bit system. "
                    + $"\r\n\r\nIf this isn't by intention you've probably downloaded the wrong build of"
                    + $" {Constants.ApplicationName}."
                    + $"\r\n\r\nIt is highly recommended to run the 64-Bit (x64) edition on a 64-Bit operating system "
                    + "or you will most likely encounter unsolvable issues."
                    + "\r\n\r\nThanks for your attention ❤️",
                Caption = "Architecture mismatch detected",
                Icon = AdonisUI.Controls.MessageBoxImage.Warning,
                Buttons = new[]
                {
                    MessageBoxButtons.Yes("Understood")
                },
                CheckBoxes = new[]
                {
                    new MessageBoxCheckBoxModel(Translations.Strings.NotAMoronConfirmationCheckbox)
                    {
                        IsChecked = false,
                        Placement = MessageBoxCheckBoxPlacement.BelowText
                    }
                },
                IsSoundEnabled = false
            };

            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                AdonisUI.Controls.MessageBox.Show(Application.Current.MainWindow, messageBox);

                appSettings.Settings.HasUserConfirmedArchitectureWarning = messageBox.CheckBoxes.First().IsChecked;
            });
        }
    }
}