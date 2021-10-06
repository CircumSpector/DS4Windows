using DS4WinWPF.DS4Forms.ViewModels;
using System;
using System.Windows;
using AdonisUI.Controls;

namespace DS4WinWPF.DS4Forms
{
    /// <summary>
    /// Interaction logic for ChangelogWindow.xaml
    /// </summary>
    public partial class ChangelogWindow : AdonisWindow
    {
        private ChangelogViewModel changelogVM;

        public ChangelogWindow()
        {
            InitializeComponent();

            changelogVM = new ChangelogViewModel();

            DataContext = changelogVM;

            SetupEvents();

            changelogVM.RetrieveChangelogInfo();

        }

        private void SetupEvents()
        {
            changelogVM.ChangelogDocumentChanged += ChangelogVM_ChangelogDocumentChanged;
        }

        private void ChangelogVM_ChangelogDocumentChanged(object sender, EventArgs e)
        {
            richChangelogTxtBox.Document = changelogVM.ChangelogDocument;
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
