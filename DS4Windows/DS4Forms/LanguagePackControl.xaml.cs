using System;
using System.Windows;
using System.Windows.Controls;
using DS4WinWPF.DS4Forms.ViewModels;

namespace DS4WinWPF.DS4Forms
{
    /// <summary>
    /// Interaction logic for LanguagePackControl.xaml
    /// </summary>
    public partial class LanguagePackControl : UserControl
    {
        private LanguagePackViewModel langPackVM;

        public LanguagePackControl()
        {
            InitializeComponent();

            langPackVM = new LanguagePackViewModel();
            this.DataContext = null;
            langPackVM.ScanFinished += LangPackVM_ScanFinished;
            langPackVM.SelectedIndexChanged += CheckForCultureChange;

            langPackVM.ScanForLangPacks();
        }

        private void CheckForCultureChange(object sender, EventArgs e)
        {
            if (langPackVM.ChangeLanguagePack())
            {
                MessageBox.Show(Properties.Resources.LanguagePackApplyRestartRequired,
                    "DS4Windows", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void LangPackVM_ScanFinished(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                cbCulture.IsEnabled = true;
                this.DataContext = langPackVM;
            }));
        }
    }
}
