using System;
using DS4Windows;

namespace DS4WinWPF.DS4Control.Profiles.Legacy
{
    /// <summary>
    ///     Represents application-wide settings.
    /// </summary>
    public partial class DS4WindowsAppSettings
    {
        /// <summary>
        ///     Converts properties from <see cref="IBackingStore" /> to this <see cref="DS4WindowsAppSettings" /> instance.
        /// </summary>
        /// <param name="store">The <see cref="IBackingStore" />.</param>
        public void CopyFrom(IBackingStore store)
        {
            UseExclusiveMode = store.UseExclusiveMode;
            StartMinimized = store.StartMinimized;
            MinimizeToTaskbar = store.MinToTaskBar;

            FormWidth = store.FormWidth;
            FormHeight = store.FormHeight;
            FormLocationX = store.FormLocationX;
            FormLocationY = store.FormLocationY;

            LastChecked = store.LastChecked;
            CheckWhen = store.CheckWhen;
            // TODO: improve this conversion mess
            LastVersionChecked = store.LastVersionCheckedNumber.ToString();
            Notifications = store.Notifications;
            DisconnectBTAtStop = store.DisconnectBluetoothAtStop;
            SwipeProfiles = store.SwipeProfiles;
            QuickCharge = store.SwipeProfiles;
            CloseMinimizes = store.CloseMini;
            UseLang = store.UseLang;
            DownloadLang = store.DownloadLang;
            FlashWhenLate = store.FlashWhenLate;
            FlashWhenLateAt = store.FlashWhenLateAt;
            AppIcon = store.UseIconChoice;
            AppTheme = store.ThemeChoice;

            UseUDPServer = store.IsUdpServerEnabled;
            UDPServerPort = store.UdpServerPort;
            UDPServerListenAddress = store.UdpServerListenAddress;
            UDPServerSmoothingOptions.UseSmoothing = store.UseUdpSmoothing;
            UDPServerSmoothingOptions.UdpSmoothMinCutoff = store.UdpSmoothingMincutoff;
            UDPServerSmoothingOptions.UdpSmoothBeta = store.UdpSmoothingBeta;

            UseCustomSteamFolder = store.UseCustomSteamFolder;
            CustomSteamFolder = store.CustomSteamFolder;

            AutoProfileRevertDefaultProfile = store.AutoProfileRevertDefaultProfile;

            DeviceOptions.DS4SupportSettings.Enabled = store.DeviceOptions.Ds4DeviceOpts.Enabled;
            DeviceOptions.DualSenseSupportSettings.Enabled = store.DeviceOptions.DualSenseOpts.Enabled;
            DeviceOptions.SwitchProSupportSettings.Enabled = store.DeviceOptions.SwitchProDeviceOpts.Enabled;
            DeviceOptions.JoyConSupportSettings.Enabled = store.DeviceOptions.JoyConDeviceOpts.Enabled;
            DeviceOptions.JoyConSupportSettings.LinkMode = store.DeviceOptions.JoyConDeviceOpts.LinkedMode;
            DeviceOptions.JoyConSupportSettings.JoinedGyroProvider = store.DeviceOptions.JoyConDeviceOpts.JoinGyroProv;
        }

        /// <summary>
        ///     Injects properties from <see cref="DS4WindowsAppSettings" /> into <see cref="IBackingStore" /> instance.
        /// </summary>
        /// <param name="store">The <see cref="IBackingStore" />.</param>
        public void CopyTo(IBackingStore store)
        {
            store.UseExclusiveMode = UseExclusiveMode;
            store.StartMinimized = StartMinimized;
            store.MinToTaskBar = MinimizeToTaskbar;

            store.FormWidth = FormWidth;
            store.FormHeight = FormHeight;
            store.FormLocationX = Math.Max(FormLocationX, 0);
            store.FormLocationY = Math.Max(FormLocationY, 0);

            store.LastChecked = LastChecked;
            store.CheckWhen = CheckWhen;
            store.LastVersionCheckedNumber = string.IsNullOrEmpty(LastVersionChecked)
                ? 0
                : Global.CompileVersionNumberFromString(LastVersionChecked);
            // TODO: replace with Enum
            store.Notifications = Notifications;
            store.DisconnectBluetoothAtStop = DisconnectBTAtStop;
            store.SwipeProfiles = SwipeProfiles;
            store.QuickCharge = QuickCharge;
            store.CloseMini = CloseMinimizes;
            store.UseLang = UseLang;
            store.DownloadLang = DownloadLang;
            store.FlashWhenLate = FlashWhenLate;
            store.FlashWhenLateAt = FlashWhenLateAt;
            store.UseIconChoice = AppIcon;
            store.ThemeChoice = AppTheme;

            store.IsUdpServerEnabled = UseUDPServer;
            store.UdpServerPort = Math.Min(Math.Max(UDPServerPort, 1024), 65535);
            store.UdpServerListenAddress = UDPServerListenAddress;
            store.UseUdpSmoothing = UDPServerSmoothingOptions.UseSmoothing;
            store.UdpSmoothingMincutoff = UDPServerSmoothingOptions.UdpSmoothMinCutoff;
            store.UdpSmoothingBeta = UDPServerSmoothingOptions.UdpSmoothBeta;

            store.UseCustomSteamFolder = UseCustomSteamFolder;
            store.CustomSteamFolder = CustomSteamFolder;

            store.AutoProfileRevertDefaultProfile = AutoProfileRevertDefaultProfile;

            store.DeviceOptions.Ds4DeviceOpts.Enabled = DeviceOptions.DS4SupportSettings.Enabled;
            store.DeviceOptions.DualSenseOpts.Enabled = DeviceOptions.DualSenseSupportSettings.Enabled;
            store.DeviceOptions.SwitchProDeviceOpts.Enabled = DeviceOptions.SwitchProSupportSettings.Enabled;
            store.DeviceOptions.JoyConDeviceOpts.Enabled = DeviceOptions.JoyConSupportSettings.Enabled;
            store.DeviceOptions.JoyConDeviceOpts.LinkedMode = DeviceOptions.JoyConSupportSettings.LinkMode;
            store.DeviceOptions.JoyConDeviceOpts.JoinGyroProv = DeviceOptions.JoyConSupportSettings.JoinedGyroProvider;
        }
    }
}