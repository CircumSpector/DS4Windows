using System;
using System.Windows.Controls;
using AdonisUI.Controls;
using DS4Windows;
using DS4Windows.Shared.Common.Core;
using DS4WinWPF.DS4Forms.ViewModels;

namespace DS4WinWPF.DS4Forms
{
    /// <summary>
    ///     Interaction logic for LanguagePackControl.xaml
    /// </summary>
    public partial class LanguagePackControl : UserControl
    {
        private readonly LanguagePackViewModel langPackVM;

        public LanguagePackControl()
        {
            InitializeComponent();

            langPackVM = new LanguagePackViewModel();
            DataContext = null;
            langPackVM.ScanFinished += LangPackVM_ScanFinished;
            langPackVM.SelectedIndexChanged += CheckForCultureChange;

            langPackVM.ScanForLangPacks();
        }

        private void CheckForCultureChange(object sender, EventArgs e)
        {
            if (langPackVM.ChangeLanguagePack())
                MessageBox.Show(Properties.Resources.LanguagePackApplyRestartRequired,
                    Constants.ApplicationName, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void LangPackVM_ScanFinished(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                cbCulture.IsEnabled = true;
                DataContext = langPackVM;
            }));
        }
    }
}