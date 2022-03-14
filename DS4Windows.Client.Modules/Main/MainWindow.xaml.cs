using DS4Windows.Shared.Devices.Util;
using MaterialDesignExtensions.Controls;
using System;
using System.ComponentModel;
using System.Windows;

namespace DS4Windows.Client.Modules.Main
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MaterialWindow, IMainView
    {
        private readonly IDeviceNotificationListener deviceNotificationListener;

        public MainWindow(IDeviceNotificationListener deviceNotificationListener)
        {
            InitializeComponent();
            this.deviceNotificationListener = deviceNotificationListener;
        }
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            var hidGuid = new Guid();

            NativeMethods.HidD_GetHidGuid(ref hidGuid);

            deviceNotificationListener.StartListen(this, hidGuid);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            deviceNotificationListener.EndListen();
            base.OnClosing(e);
        }
    }
}
