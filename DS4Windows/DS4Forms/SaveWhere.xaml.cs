using System;
using System.IO;
using System.Windows;
using DS4Windows;


namespace DS4WinWPF.DS4Forms
{
    /// <summary>
    /// Interaction logic for SaveWhere.xaml
    /// </summary>
    public partial class SaveWhere : Window
    {
        private bool multisaves;
        private bool choiceMade = false;

        public SaveWhere(bool multisavespots)
        {
            InitializeComponent();
            multisaves = multisavespots;
            if (!multisavespots)
            {
                multipleSavesDockP.Visibility = Visibility.Collapsed;
                pickWhereTxt.Text += Properties.Resources.OtherFileLocation;
            }

            if (DS4Windows.Global.IsAdminNeeded)
            {
                progFolderPanel.IsEnabled = false;
            }
        }

        private void ProgFolderBtn_Click(object sender, RoutedEventArgs e)
        {
            Global.Instance.SaveTo(DS4Windows.Global.ExecutableDirectory);
            if (multisaves && dontDeleteCk.IsChecked == false)
            {
                try
                {
                    if (Directory.Exists(DS4Windows.Global.RoamingAppDataPath))
                    {
                        Directory.Delete(DS4Windows.Global.RoamingAppDataPath, true);
                    }
                }
                catch { }
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
                    File.Delete(Path.Combine(Global.ExecutableDirectory, Constants.ProfilesFileName));
                    File.Delete(Path.Combine(Global.ExecutableDirectory, Constants.AutoProfilesFileName));
                }
                catch (UnauthorizedAccessException)
                {
                    MessageBox.Show("Cannot Delete old settings, please manually delete", "DS4Windows");
                }

            Global.Instance.SaveTo(Global.RoamingAppDataPath);
            choiceMade = true;
            Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!choiceMade)
            {
                e.Cancel = true;
            }
        }
    }
}
