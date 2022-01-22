using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using DS4Windows;
using DS4Windows.Shared.Common.Attributes;
using MessageBoxButton = AdonisUI.Controls.MessageBoxButton;
using MessageBoxImage = AdonisUI.Controls.MessageBoxImage;
using MessageBoxResult = AdonisUI.Controls.MessageBoxResult;
using MessageBox = AdonisUI.Controls.MessageBox;

namespace DS4WinWPF.DS4Forms
{
    /// <summary>
    ///     Interaction logic for DupBox.xaml
    /// </summary>
    public partial class DupBox : UserControl
    {
        public delegate void SaveHandler(DupBox sender, string profilename);

        public DupBox()
        {
            InitializeComponent();
        }

        public string OldFilename { get; set; }

        public event EventHandler Cancel;
        public event SaveHandler Save;

        [ConfigurationSystemComponent]
        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            var profile = profileTxt.Text;
            if (!string.IsNullOrWhiteSpace(profile) &&
                profile.IndexOfAny(Path.GetInvalidFileNameChars()) == -1)
            {
                File.Copy(Path.Combine(Global.RuntimeAppDataPath, "Profiles", $"{OldFilename}.xml"),
                    Path.Combine(Global.RuntimeAppDataPath, "Profiles", $"{profile}.xml"), true);
                Save?.Invoke(this, profile);
            }
            else
            {
                MessageBox.Show(Properties.Resources.ValidName, Properties.Resources.NotValid,
                    MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            Cancel?.Invoke(this, EventArgs.Empty);
        }
    }
}