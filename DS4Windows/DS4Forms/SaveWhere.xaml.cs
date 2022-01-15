using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using AdonisUI.Controls;
using DS4Windows;
using MessageBoxButton = AdonisUI.Controls.MessageBoxButton;
using MessageBoxImage = AdonisUI.Controls.MessageBoxImage;
using MessageBoxResult = AdonisUI.Controls.MessageBoxResult;
using MessageBox = AdonisUI.Controls.MessageBox;

namespace DS4WinWPF.DS4Forms
{
    /// <summary>
    ///     Interaction logic for SaveWhere.xaml
    /// </summary>
    public partial class SaveWhere : AdonisWindow
    {
        private bool choiceMade;
        private readonly bool multisaves;

        public SaveWhere(bool multisavespots)
        {
            InitializeComponent();
            multisaves = multisavespots;
            if (!multisavespots)
            {
                multipleSavesDockP.Visibility = Visibility.Collapsed;
                pickWhereTxt.Text += Properties.Resources.OtherFileLocation;
            }

            if (Global.IsAdminNeeded) progFolderPanel.IsEnabled = false;
        }

        private void ProgFolderBtn_Click(object sender, RoutedEventArgs e)
        {
            Global.Instance.SaveTo(Global.ExecutableDirectory);
            if (multisaves && dontDeleteCk.IsChecked == false)
                try
                {
                    if (Directory.Exists(Global.RoamingAppDataPath)) Directory.Delete(Global.RoamingAppDataPath, true);
                }
                catch
                {
                }

            choiceMade = true;
            Close();
        }

        private void AppdataBtn_Click(object sender, RoutedEventArgs e)
        {
            if (multisaves && dontDeleteCk.IsChecked == false)
                try
                {
                    Directory.Delete(Path.Combine(Global.ExecutableDirectory, Constants.ProfilesSubDirectory), true);
                    File.Delete(Path.Combine(Global.ExecutableDirectory, Constants.LegacyProfilesFileName));
                    File.Delete(Path.Combine(Global.ExecutableDirectory, Constants.LegacyAutoProfilesFileName));
                }
                catch (UnauthorizedAccessException)
                {
                    MessageBox.Show("Cannot Delete old settings, please manually delete", "DS4Windows");
                }

            Global.Instance.SaveTo(Global.RoamingAppDataPath);
            choiceMade = true;
            Close();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (!choiceMade) e.Cancel = true;
        }
    }
}