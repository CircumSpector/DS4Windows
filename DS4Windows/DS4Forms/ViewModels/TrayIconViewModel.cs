using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using DS4Windows;
using DS4Windows.Shared.Common.Attributes;
using DS4Windows.Shared.Configuration.Application.Services;
using DS4Windows.Shared.Configuration.Profiles.Schema;
using DS4Windows.Shared.Devices.HID;
using DS4WinWPF.DS4Control.IoC.Services;
using DS4WinWPF.DS4Control.Profiles.Schema;
using JetBrains.Annotations;

namespace DS4WinWPF.DS4Forms.ViewModels
{
    public class TrayIconViewModel
    {
        public const string ballonTitle = "DS4Windows";
        public static string trayTitle = $"DS4Windows v{Global.ExecutableProductVersion}";

        [Obsolete] private readonly ReaderWriterLockSlim _colLocker = new();

        private readonly IAppSettingsService appSettings;
        private readonly MenuItem changeServiceItem;
        private readonly MenuItem closeItem;
        private readonly List<ControllerHolder> controllerList = new();
        private readonly ControlService controlService;
        private readonly MenuItem minimizeItem;
        private readonly MenuItem openItem;
        private readonly MenuItem openProgramItem;

        private readonly IProfilesService profilesService;
        private string iconSource;
        private string tooltipText = "DS4Windows";

        [UsedImplicitly]
        [MissingLocalization]
        public TrayIconViewModel(
            IAppSettingsService appSettings,
            ControlService service,
            IProfilesService profilesService
        )
        {
            this.appSettings = appSettings;
            controlService = service;
            this.profilesService = profilesService;
            ContextMenu = new ContextMenu();
            iconSource = Global.IconChoiceResources[appSettings.Settings.AppIcon];
            changeServiceItem = new MenuItem { Header = "Start" };
            changeServiceItem.Click += ChangeControlServiceItem_Click;
            changeServiceItem.IsEnabled = false;

            profilesService.AvailableProfilesChanged += PopulateContextMenu;

            openItem = new MenuItem
            {
                Header = "Open",
                FontWeight = FontWeights.Bold
            };

            openItem.Click += OpenMenuItem_Click;
            minimizeItem = new MenuItem { Header = "Minimize" };
            minimizeItem.Click += MinimizeMenuItem_Click;
            openProgramItem = new MenuItem { Header = "Open Program Folder" };
            openProgramItem.Click += OpenProgramFolderItem_Click;
            closeItem = new MenuItem { Header = "Exit (Middle Mouse)" };
            closeItem.Click += ExitMenuItem_Click;

            PopulateControllerList();
            PopulateToolText();
            PopulateContextMenu();
            SetupEvents();

            service.ServiceStarted += BuildControllerList;
            service.ServiceStarted += HookEvents;
            service.ServiceStarted += StartPopulateText;
            service.PreServiceStop += ClearToolText;
            service.PreServiceStop += UnhookEvents;
            service.PreServiceStop += ClearControllerList;
            service.RunningChanged += Service_RunningChanged;
            service.HotplugController += Service_HotPlugController;
        }

        public string TooltipText
        {
            get => tooltipText;
            set
            {
                var temp = value;
                if (value.Length > 63) temp = value.Substring(0, 63);
                if (tooltipText == temp) return;
                tooltipText = temp;
                TooltipTextChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public string IconSource
        {
            get => iconSource;
            set
            {
                if (iconSource == value) return;
                iconSource = value;
                IconSourceChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public ContextMenu ContextMenu { get; }

        public event EventHandler TooltipTextChanged;

        public event EventHandler IconSourceChanged;
        public event EventHandler RequestShutdown;
        public event EventHandler RequestOpen;
        public event EventHandler RequestMinimize;
        public event EventHandler RequestServiceChange;

        [MissingLocalization]
        private void Service_RunningChanged(object sender, EventArgs e)
        {
            var temp = controlService.IsRunning ? "Stop" : "Start";
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                changeServiceItem.Header = temp;
                changeServiceItem.IsEnabled = true;
            }));
        }

        private void ClearControllerList(object sender, EventArgs e)
        {
            _colLocker.EnterWriteLock();
            controllerList.Clear();
            _colLocker.ExitWriteLock();
        }

        private void UnhookEvents(object sender, EventArgs e)
        {
            _colLocker.EnterReadLock();
            foreach (var holder in controllerList)
            {
                var currentDev = holder.Device;
                RemoveDeviceEvents(currentDev);
            }

            _colLocker.ExitReadLock();
        }

        private void Service_HotPlugController(ControlService sender, DS4Device device, int index)
        {
            SetupDeviceEvents(device);
            _colLocker.EnterWriteLock();
            controllerList.Add(new ControllerHolder(device, index));
            _colLocker.ExitWriteLock();
        }

        private void BuildControllerList(object sender, EventArgs e)
        {
            PopulateControllerList();
        }

        [MissingLocalization]
        public void PopulateContextMenu()
        {
            ContextMenu.Items.Clear();
            var items = ContextMenu.Items;
            MenuItem item;
            var idx = 0;

            using (var locker = new ReadLocker(_colLocker))
            {
                foreach (var holder in controllerList)
                {
                    var currentDev = holder.Device;
                    item = new MenuItem
                    {
                        Header = $"Controller {idx + 1}",
                        Tag = idx
                    };

                    var subitems = item.Items;
                    var currentProfile = profilesService.ActiveProfiles.ElementAt(idx);

                    foreach (var profile in profilesService.AvailableProfiles)
                    {
                        // Need to escape profile name to disable Access Keys for control
                        var name = Regex.Replace(profile.DisplayName, "_{1}", "__");

                        var newItem = new MenuItem
                        {
                            Header = name,
                            Tag = new SlotToProfileMap
                            {
                                Slot = idx,
                                Profile = profile
                            }
                        };

                        newItem.Click += (sender, args) =>
                        {
                            var map = (SlotToProfileMap)((MenuItem)sender).Tag;

                            profilesService.SetActiveTo(map.Slot, map.Profile);
                        };

                        if (profile.Equals(currentProfile)) newItem.IsChecked = true;

                        subitems.Add(newItem);
                    }

                    items.Add(item);
                    idx++;
                }

                item = new MenuItem { Header = "Disconnect Menu" };
                idx = 0;

                foreach (var holder in controllerList)
                {
                    var tempDev = holder.Device;
                    if (tempDev.Synced && !tempDev.Charging)
                    {
                        var subitem = new MenuItem { Header = $"Disconnect Controller {idx + 1}" };
                        subitem.Click += DisconnectMenuItem_Click;
                        subitem.Tag = idx;
                        item.Items.Add(subitem);
                    }

                    idx++;
                }

                if (idx == 0) item.IsEnabled = false;
            }

            items.Add(item);
            items.Add(new Separator());
            PopulateStaticItems();
        }

        private void ChangeControlServiceItem_Click(object sender, RoutedEventArgs e)
        {
            changeServiceItem.IsEnabled = false;
            RequestServiceChange?.Invoke(this, EventArgs.Empty);
        }

        private void OpenProgramFolderItem_Click(object sender, RoutedEventArgs e)
        {
            var startInfo = new ProcessStartInfo(Global.ExecutableDirectory)
            {
                UseShellExecute = true
            };
            using (Process.Start(startInfo))
            {
            }
        }

        private void OpenMenuItem_Click(object sender, RoutedEventArgs e)
        {
            RequestOpen?.Invoke(this, EventArgs.Empty);
        }

        private void MinimizeMenuItem_Click(object sender, RoutedEventArgs e)
        {
            RequestMinimize?.Invoke(this, EventArgs.Empty);
        }

        private void DisconnectMenuItem_Click(object sender,
            RoutedEventArgs e)
        {
            var item = sender as MenuItem;
            var idx = Convert.ToInt32(item.Tag);
            var holder = controllerList[idx];
            var tempDev = holder?.Device;
            if (tempDev != null && tempDev.Synced && !tempDev.Charging)
            {
                if (tempDev.ConnectionType == ConnectionType.Bluetooth)
                    //tempDev.StopUpdate();
                    tempDev.DisconnectBT();
                else if (tempDev.ConnectionType == ConnectionType.SonyWirelessAdapter) tempDev.DisconnectDongle();
            }

            //controllerList[idx] = null;
        }

        private void PopulateControllerList()
        {
            var idx = 0;
            _colLocker.EnterWriteLock();
            foreach (var currentDev in controlService.slotManager.ControllerColl)
            {
                controllerList.Add(new ControllerHolder(currentDev, idx));
                idx++;
            }

            _colLocker.ExitWriteLock();
        }

        private void StartPopulateText(object sender, EventArgs e)
        {
            PopulateToolText();
            //PopulateContextMenu();
        }

        private void PopulateToolText()
        {
            var items = new List<string>();
            items.Add(trayTitle);
            //IEnumerable<DS4Device> devices = DS4Devices.getDS4Controllers();
            var idx = 1;
            //foreach (DS4Device currentDev in devices)
            _colLocker.EnterReadLock();
            foreach (var holder in controllerList)
            {
                var currentDev = holder.Device;
                items.Add(
                    $"{idx}: {currentDev.ConnectionType} {currentDev.Battery}%{(currentDev.Charging ? "+" : "")}");
                idx++;
            }

            _colLocker.ExitReadLock();

            TooltipText = string.Join("\n", items);
        }

        private void SetupEvents()
        {
            //IEnumerable<DS4Device> devices = DS4Devices.getDS4Controllers();
            //foreach (DS4Device currentDev in devices)
            _colLocker.EnterReadLock();
            foreach (var holder in controllerList)
            {
                var currentDev = holder.Device;
                SetupDeviceEvents(currentDev);
            }

            _colLocker.ExitReadLock();
        }

        private void SetupDeviceEvents(DS4Device device)
        {
            device.BatteryChanged += UpdateForBattery;
            device.ChargingChanged += UpdateForBattery;
            device.Removal += CurrentDev_Removal;
        }

        private void RemoveDeviceEvents(DS4Device device)
        {
            device.BatteryChanged -= UpdateForBattery;
            device.ChargingChanged -= UpdateForBattery;
            device.Removal -= CurrentDev_Removal;
        }

        private void CurrentDev_Removal(object sender, EventArgs e)
        {
            var currentDev = sender as DS4Device;
            ControllerHolder item = null;
            var idx = 0;

            using (var locker = new WriteLocker(_colLocker))
            {
                foreach (var holder in controllerList)
                {
                    if (currentDev == holder.Device)
                    {
                        item = holder;
                        break;
                    }

                    idx++;
                }

                if (item != null)
                {
                    controllerList.RemoveAt(idx);
                    RemoveDeviceEvents(currentDev);
                }
            }

            PopulateToolText();
        }

        private void HookEvents(object sender, EventArgs e)
        {
            SetupEvents();
        }

        private void UpdateForBattery(object sender)
        {
            PopulateToolText();
        }

        private void ClearToolText(object sender, EventArgs e)
        {
            TooltipText = "DS4Windows";
            //contextMenu.Items.Clear();
        }

        private void PopulateStaticItems()
        {
            var items = ContextMenu.Items;
            items.Add(changeServiceItem);
            items.Add(openItem);
            items.Add(minimizeItem);
            items.Add(openProgramItem);
            items.Add(new Separator());
            items.Add(closeItem);
        }

        public void ClearContextMenu()
        {
            ContextMenu.Items.Clear();
            PopulateStaticItems();
        }

        private void ExitMenuItem_Click(object sender, RoutedEventArgs e)
        {
            RequestShutdown?.Invoke(this, EventArgs.Empty);
        }

        private sealed class SlotToProfileMap
        {
            public int Slot { get; init; }

            public DS4WindowsProfile Profile { get; init; }
        }
    }

    public class ControllerHolder
    {
        public ControllerHolder(DS4Device device, int index)
        {
            Device = device;
            Index = index;
        }

        public DS4Device Device { get; }

        public int Index { get; }
    }
}