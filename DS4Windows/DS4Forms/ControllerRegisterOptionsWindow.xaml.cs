using System;
using System.Windows.Controls;
using AdonisUI.Controls;
using DS4WinWPF.DS4Forms.ViewModels;
using DS4Windows;
using DS4WinWPF.DS4Control.Profiles.Legacy;

namespace DS4WinWPF.DS4Forms
{
    /// <summary>
    /// Interaction logic for ControllerRegisterOptions.xaml
    /// </summary>
    public partial class ControllerRegisterOptionsWindow : AdonisWindow
    {
        private ControllerRegDeviceOptsViewModel deviceOptsVM;

        public ControllerRegisterOptionsWindow(DeviceOptions deviceOptions, ControlService service)
        {
            InitializeComponent();

            deviceOptsVM = new ControllerRegDeviceOptsViewModel(deviceOptions, service);

            devOptionsDockPanel.DataContext = deviceOptsVM;
            deviceOptsVM.ControllerSelectedIndexChanged += ChangeActiveDeviceTab;
        }

        private void ChangeActiveDeviceTab(object sender, EventArgs e)
        {
            TabItem currentTab = deviceSettingsTabControl.SelectedItem as TabItem;
            if (currentTab != null)
            {
                currentTab.DataContext = null;
            }

            int tabIdx = deviceOptsVM.FindTabOptionsIndex();
            if (tabIdx >= 0)
            {
                TabItem pendingTab = deviceSettingsTabControl.Items[tabIdx] as TabItem;
                deviceOptsVM.FindFittingDataContext();
                pendingTab.DataContext = deviceOptsVM.DataContextObject;
            }

            deviceOptsVM.CurrentTabSelectedIndex = tabIdx;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            deviceOptsVM.SaveControllerConfigs();
        }
    }
}
