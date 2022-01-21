using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using AdonisUI.Controls;
using DS4Windows;
using DS4WinWPF.DS4Control.Attributes;
using DS4WinWPF.DS4Control.IoC.Services;
using DS4WinWPF.Translations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using MessageBox = AdonisUI.Controls.MessageBox;
using MessageBoxImage = AdonisUI.Controls.MessageBoxImage;
using MessageBoxResult = AdonisUI.Controls.MessageBoxResult;

namespace DS4WinWPF.DS4Control.IoC.HostedServices
{
    /// <summary>
    ///     Executes various checks on startup and displays user notifications, if attention is required.
    /// </summary>
    internal class StartupChecksUserNotifications : BackgroundService
    {
        private readonly IAppSettingsService appSettings;

        private readonly IConfiguration config;

        private readonly IExternalDependenciesService dependenciesService;

        private readonly ILogger<StartupChecksUserNotifications> logger;

        public StartupChecksUserNotifications(IAppSettingsService appSettings,
            ILogger<StartupChecksUserNotifications> logger,
            IExternalDependenciesService dependenciesService, IConfiguration config)
        {
            this.appSettings = appSettings;
            this.logger = logger;
            this.dependenciesService = dependenciesService;
            this.config = config;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            logger.LogInformation("Performing startup tasks");

            //
            // Required to know if user has suppressed some checks already
            // 
            await appSettings.LoadAsync();

            //
            // Perform each check sequential and display a modal dialog
            // 

            await CheckViGEmBusPresence();

            await CheckHidHidePresence();

            await CheckIsTracingEnabled();

            await CheckWindows11();

            await CheckIsSteamRunning();

            await CheckAppArchitecture();

            logger.LogInformation("Done performing startup tasks");
        }

        [MissingLocalization]
        private async Task CheckViGEmBusPresence()
        {
            if (dependenciesService.ViGEmBusGen1LatestVersion is not null)
                return;

            var messageBox = new MessageBoxModel
            {
                Text =
                    "Hello, Gamer!" +
                    "\r\n\r\nThe emulation driver ViGEmBus seems to be missing on this system. "
                    + "\r\n\r\nWithout this component almost all application features will not be available. "
                    + "Please install it now and restart the application afterwards or read up on troubleshooting."
                    + "\r\n\r\nThanks for your attention ❤️",
                Caption = "ViGEmBus not found",
                Icon = MessageBoxImage.Warning,
                Buttons = new[]
                {
                    MessageBoxButtons.Custom("Take me to the download!"),
                    MessageBoxButtons.No("Didn't work, I need help!"),
                    MessageBoxButtons.Yes("I know what I'm doing")
                },
                IsSoundEnabled = false
            };

            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                MessageBox.Show(Application.Current.MainWindow, messageBox);

                switch (messageBox.Result)
                {
                    case MessageBoxResult.Custom:
                        DS4Windows.Util.StartProcessHelper(Constants.ViGEmBusGen1DownloadUri);
                        break;
                    case MessageBoxResult.No:
                        DS4Windows.Util.StartProcessHelper(Constants.ViGEmBusGen1GuideUri);
                        break;
                }
            });
        }

        [MissingLocalization]
        private async Task CheckHidHidePresence()
        {
            if (dependenciesService.HidHideLatestVersion is not null)
                return;

            var messageBox = new MessageBoxModel
            {
                Text =
                    "Hello, Gamer!" +
                    "\r\n\r\nThe filter driver HidHide seems to be missing on this system. "
                    + "\r\n\r\nWithout this component many games may probably see two controllers: "
                    + "your \"real\" (hardware) one and a \"fake\" (virtual) one your inputs get mapped to. "
                    + "\r\n\r\nThis will lead to so called doubled inputs or other glitches, because two controllers "
                    + "will report the same input and the game may not know which one is the dominant one. "
                    + "\r\n\r\nTo mitigate this, please install HidHide now and restart your machine afterwards or read up on troubleshooting."
                    + "\r\n\r\nThanks for your attention ❤️",
                Caption = "HidHide not found",
                Icon = MessageBoxImage.Warning,
                Buttons = new[]
                {
                    MessageBoxButtons.Custom("Take me to the download!"),
                    MessageBoxButtons.No("Didn't work, I need help!"),
                    MessageBoxButtons.Yes("I know what I'm doing")
                },
                IsSoundEnabled = false
            };

            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                MessageBox.Show(Application.Current.MainWindow, messageBox);

                switch (messageBox.Result)
                {
                    case MessageBoxResult.Custom:
                        DS4Windows.Util.StartProcessHelper(Constants.HidHideDownloadUri);
                        break;
                    case MessageBoxResult.No:
                        DS4Windows.Util.StartProcessHelper(Constants.HidHideGuideUri);
                        break;
                }
            });
        }

        [MissingLocalization]
        private async Task CheckIsTracingEnabled()
        {
            if (!bool.TryParse(config.GetSection("OpenTelemetry:IsEnabled").Value, out var isEnabled) ||
                !isEnabled)
                return;

            var messageBox = new MessageBoxModel
            {
                Text =
                    "Hello, Gamer!" +
                    "\r\n\r\nYou have enabled Tracing in appsettings.json file. This is an advanced feature useful for diagnosing "
                    + "issues with lag or stutter and general remapping performance. "
                    + "\r\n\r\nTracing is a very memory-hungry operation and requires additional software to be useful. "
                    + "Do not leave Tracing enabled if you simply wanna play your games, it's for diagnostics only."
                    + "\r\n\r\nThanks for your attention ❤️",
                Caption = "Performance Tracing is enabled",
                Icon = MessageBoxImage.Warning,
                Buttons = new[]
                {
                    MessageBoxButtons.Custom("Tell me more"),
                    MessageBoxButtons.Yes("Understood")
                },
                IsSoundEnabled = false
            };

            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                MessageBox.Show(Application.Current.MainWindow, messageBox);

                switch (messageBox.Result)
                {
                    case MessageBoxResult.Custom:
                        DS4Windows.Util.StartProcessHelper(Constants.TracingGuideUri);
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
                    + "\r\n\r\nPlease bear in mind that compatibility with Windows 11 currently is in its very early stage, "
                    + "if something that worked on Windows 10 is broken, for now, you're on your own!"
                    + "\r\n\r\nThanks for your attention ❤️",
                Caption = "Windows 11 detected",
                Icon = MessageBoxImage.Warning,
                Buttons = new[]
                {
                    MessageBoxButtons.Yes("Understood")
                },
                CheckBoxes = new[]
                {
                    new MessageBoxCheckBoxModel(Strings.NotAMoronConfirmationCheckbox)
                    {
                        IsChecked = false,
                        Placement = MessageBoxCheckBoxPlacement.BelowText
                    }
                },
                IsSoundEnabled = false
            };

            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                MessageBox.Show(Application.Current.MainWindow, messageBox);

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
                Icon = MessageBoxImage.Warning,
                Buttons = new[]
                {
                    MessageBoxButtons.Custom("Show me what to do"),
                    MessageBoxButtons.Yes("Understood")
                },
                CheckBoxes = new[]
                {
                    new MessageBoxCheckBoxModel(Strings.NotAMoronConfirmationCheckbox)
                    {
                        IsChecked = false,
                        Placement = MessageBoxCheckBoxPlacement.BelowText
                    }
                },
                IsSoundEnabled = false
            };

            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                MessageBox.Show(Application.Current.MainWindow, messageBox);

                appSettings.Settings.HasUserConfirmedSteamWarning = messageBox.CheckBoxes.First().IsChecked;

                if (messageBox.Result == MessageBoxResult.Custom)
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
                    + "\r\n\r\nIf this isn't by intention you've probably downloaded the wrong build of"
                    + $" {Constants.ApplicationName}."
                    + "\r\n\r\nIt is highly recommended to run the 64-Bit (x64) edition on a 64-Bit operating system "
                    + "or you will most likely encounter unsolvable issues."
                    + "\r\n\r\nThanks for your attention ❤️",
                Caption = "Architecture mismatch detected",
                Icon = MessageBoxImage.Warning,
                Buttons = new[]
                {
                    MessageBoxButtons.Yes("Understood")
                },
                CheckBoxes = new[]
                {
                    new MessageBoxCheckBoxModel(Strings.NotAMoronConfirmationCheckbox)
                    {
                        IsChecked = false,
                        Placement = MessageBoxCheckBoxPlacement.BelowText
                    }
                },
                IsSoundEnabled = false
            };

            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                MessageBox.Show(Application.Current.MainWindow, messageBox);

                appSettings.Settings.HasUserConfirmedArchitectureWarning = messageBox.CheckBoxes.First().IsChecked;
            });
        }
    }
}