using System.Diagnostics;
using System.IO;
using System.Windows;
using DS4Windows;
using DS4Windows.Shared.Common.Core;
using DS4WinWPF.Translations;

namespace DS4WinWPF.DS4Forms
{
    public partial class MainWindow
    {
        private void Html5GameBtn_Click(object sender, RoutedEventArgs e)
        {
            Util.StartProcessHelper("https://gamepad-tester.com/");
        }

        private void HidHideBtn_Click(object sender, RoutedEventArgs e)
        {
            var driveLetter = Path.GetPathRoot(Global.ExecutableDirectory);
            var path = Path.Combine(driveLetter, "Program Files",
                "Nefarius Software Solutions e.U", "HidHideClient", "HidHideClient.exe");

            if (!File.Exists(path)) return;

            try
            {
                var startInfo = new ProcessStartInfo(path);
                startInfo.UseShellExecute = true;
                using (var proc = Process.Start(startInfo))
                {
                }
            }
            catch
            {
            }
        }

        private void FakeExeNameExplainBtn_Click(object sender, RoutedEventArgs e)
        {
            var message = Strings.CustomExeNameInfo;
            MessageBox.Show(message, "Custom Exe Name Info", MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void XinputCheckerBtn_Click(object sender, RoutedEventArgs e)
        {
            var path = Path.Combine(Global.ExecutableDirectory, "Tools",
                "XInputChecker", "XInputChecker.exe");

            if (File.Exists(path))
                try
                {
                    using (var proc = Process.Start(path))
                    {
                    }
                }
                catch
                {
                }
        }

        private void ControlPanelBtn_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("control", "joy.cpl");
        }

        // TODO: fix me!
        private void ProfFolderBtn_Click(object sender, RoutedEventArgs e)
        {
            var startInfo =
                new ProcessStartInfo(Path.Combine(Global.RuntimeAppDataPath, Constants.ProfilesSubDirectory))
                {
                    UseShellExecute = true
                };
            try
            {
                using (var temp = Process.Start(startInfo))
                {
                }
            }
            catch
            {
            }
        }
    }
}