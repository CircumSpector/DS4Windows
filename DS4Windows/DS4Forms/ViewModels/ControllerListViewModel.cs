using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using DS4Windows;
using DS4Windows.Shared.Common.Types;
using DS4Windows.Shared.Core.HID;
using DS4Windows.Shared.Core.Util;
using DS4WinWPF.DS4Control.Attributes;
using DS4WinWPF.DS4Control.IoC.Services;
using DS4WinWPF.DS4Control.Logging;
using DS4WinWPF.DS4Control.Util;
using DS4WinWPF.Properties;
using DS4WinWPF.Translations;

namespace DS4WinWPF.DS4Forms.ViewModels
{
    public class ControllerListViewModel
    {
        private readonly IAppSettingsService appSettings;

        //private object _colLockobj = new object();
        private readonly ReaderWriterLockSlim _colListLocker = new();

        private readonly ControlService controlService;

        private readonly ProfileList profileListHolder;

        //public ControllerListViewModel(Tester tester, ProfileList profileListHolder)
        public ControllerListViewModel(ControlService service, ProfileList profileListHolder,
            IAppSettingsService appSettings)
        {
            this.appSettings = appSettings;
            this.profileListHolder = profileListHolder;
            controlService = service;
            service.ServiceStarted += ControllersChanged;
            service.PreServiceStop += ClearControllerList;
            service.HotplugController += Service_HotplugController;
            //tester.StartControllers += ControllersChanged;
            //tester.ControllersRemoved += ClearControllerList;

            var idx = 0;
            foreach (var currentDev in controlService.slotManager.ControllerColl)
            {
                var temp = new CompositeDeviceModel(appSettings, service, currentDev,
                    idx, Global.Instance.Config.ProfilePath[idx], profileListHolder);
                ControllerCol.Add(temp);
                ControllerDict.Add(idx, temp);
                currentDev.Removal += Controller_Removal;
                idx++;
            }

            //BindingOperations.EnableCollectionSynchronization(controllerCol, _colLockobj);
            BindingOperations.EnableCollectionSynchronization(ControllerCol, _colListLocker,
                ColLockCallback);
        }

        public ObservableCollection<CompositeDeviceModel> ControllerCol { get; set; } = new();

        public int CurrentIndex { get; set; }

        public CompositeDeviceModel CurrentItem
        {
            get
            {
                if (CurrentIndex == -1) return null;
                ControllerDict.TryGetValue(CurrentIndex, out var item);
                return item;
            }
        }

        public Dictionary<int, CompositeDeviceModel> ControllerDict { get; set; } = new();

        private void ColLockCallback(IEnumerable collection, object context,
            Action accessMethod, bool writeAccess)
        {
            if (writeAccess)
                using (var locker = new WriteLocker(_colListLocker))
                {
                    accessMethod?.Invoke();
                }
            else
                using (var locker = new ReadLocker(_colListLocker))
                {
                    accessMethod?.Invoke();
                }
        }

        private void Service_HotplugController(ControlService sender,
            DS4Device device, int index)
        {
            // Engage write lock pre-maturely
            using (var readLock = new WriteLocker(_colListLocker))
            {
                // Look if device exists. Also, check if disconnect might be occurring
                if (!ControllerDict.ContainsKey(index) && !device.IsRemoving)
                {
                    var temp = new CompositeDeviceModel(appSettings, controlService, device,
                        index, Global.Instance.Config.ProfilePath[index], profileListHolder);
                    ControllerCol.Add(temp);
                    ControllerDict.Add(index, temp);

                    device.Removal += Controller_Removal;
                }
            }
        }

        private void ClearControllerList(object sender, EventArgs e)
        {
            _colListLocker.EnterReadLock();
            foreach (var temp in ControllerCol) temp.Device.Removal -= Controller_Removal;
            _colListLocker.ExitReadLock();

            _colListLocker.EnterWriteLock();
            ControllerCol.Clear();
            ControllerDict.Clear();
            _colListLocker.ExitWriteLock();
        }

        private void ControllersChanged(object sender, EventArgs e)
        {
            //IEnumerable<DS4Device> devices = DS4Windows.DS4Devices.getDS4Controllers();
            using (var locker = new ReadLocker(controlService.slotManager.CollectionLocker))
            {
                foreach (var currentDev in controlService.slotManager.ControllerColl)
                {
                    var found = false;
                    _colListLocker.EnterReadLock();
                    foreach (var temp in ControllerCol)
                        if (temp.Device == currentDev)
                        {
                            found = true;
                            break;
                        }

                    _colListLocker.ExitReadLock();

                    // Check for new device. Also, check if disconnect might be occurring
                    if (!found && !currentDev.IsRemoving)
                    {
                        //int idx = controllerCol.Count;
                        _colListLocker.EnterWriteLock();
                        var idx = controlService.slotManager.ReverseControllerDict[currentDev];
                        var temp = new CompositeDeviceModel(appSettings, controlService, currentDev,
                            idx, Global.Instance.Config.ProfilePath[idx], profileListHolder);
                        ControllerCol.Add(temp);
                        ControllerDict.Add(idx, temp);
                        _colListLocker.ExitWriteLock();

                        currentDev.Removal += Controller_Removal;
                    }
                }
            }
        }

        private void Controller_Removal(object sender, EventArgs e)
        {
            var currentDev = sender as DS4Device;
            CompositeDeviceModel found = null;
            _colListLocker.EnterReadLock();
            foreach (var temp in ControllerCol)
                if (temp.Device == currentDev)
                {
                    found = temp;
                    break;
                }

            _colListLocker.ExitReadLock();

            if (found != null)
            {
                _colListLocker.EnterWriteLock();
                ControllerCol.Remove(found);
                ControllerDict.Remove(found.DevIndex);
                Application.Current.Dispatcher.Invoke(async () => { await appSettings.SaveAsync(); });

                _colListLocker.ExitWriteLock();
            }
        }
    }

    public class CompositeDeviceModel
    {
        public delegate void CustomColorHandler(CompositeDeviceModel sender);

        private readonly IAppSettingsService appSettings;

        private readonly ControlService rootHub;

        private ProfileEntity selectedEntity;
        private int selectedIndex = 1;

        public CompositeDeviceModel(IAppSettingsService appSettings, ControlService service, DS4Device device,
            int devIndex, string profile,
            ProfileList collection)
        {
            this.appSettings = appSettings;
            Device = device;
            rootHub = service;

            device.BatteryChanged += sender => BatteryStateChanged?.Invoke(sender);
            device.ChargingChanged += sender => BatteryStateChanged?.Invoke(sender);
            device.MacAddressChanged += (sender, e) => IdTextChanged?.Invoke(this, e);

            DevIndex = devIndex;
            SelectedProfile = profile;
            ProfileEntities = collection;
            if (!string.IsNullOrEmpty(SelectedProfile))
                selectedEntity = ProfileEntities.ProfileListCollection.SingleOrDefault(x => x.Name == SelectedProfile);

            if (selectedEntity != null)
            {
                selectedIndex = ProfileEntities.ProfileListCollection.IndexOf(selectedEntity);
                HookEvents(true);
            }

            UseCustomColor = appSettings.Settings.LightbarSettingInfo[devIndex].Ds4WinSettings.UseCustomLed;
        }

        public DS4Device Device { get; set; }

        public string SelectedProfile { get; set; }

        public ProfileList ProfileEntities { get; set; }

        public ObservableCollection<ProfileEntity> ProfileListCol => ProfileEntities.ProfileListCollection;

        public string LightColor
        {
            get
            {
                var color = appSettings.Settings.LightbarSettingInfo[DevIndex].Ds4WinSettings.UseCustomLed
                    ? appSettings.Settings.LightbarSettingInfo[DevIndex].Ds4WinSettings.CustomLed
                    : appSettings.Settings.LightbarSettingInfo[DevIndex].Ds4WinSettings.Led;
                return $"#FF{color.Red:X2}{color.Green:X2}{color.Blue:X2}";
            }
        }

        public Color CustomLightColor =>
            appSettings.Settings.LightbarSettingInfo[DevIndex].Ds4WinSettings.CustomLed.ToColor();

        public string BatteryState => $"{Device.Battery}%{(Device.Charging ? "+" : "")}";

        public int SelectedIndex
        {
            get => selectedIndex;
            set
            {
                if (selectedIndex == value) return;
                selectedIndex = value;
                SelectedIndexChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public string StatusSource
        {
            get
            {
                var imgName =
                    (string) Application.Current.FindResource(Device.ConnectionType == ConnectionType.Usb
                        ? "UsbImg"
                        : "BtImg");
                var source = $"/DS4Windows;component/Resources/{imgName}";
                return source;
            }
        }

        public string ExclusiveSource
        {
            get
            {
                var imgName = (string) Application.Current.FindResource("CancelImg");
                var source = $"/DS4Windows;component/Resources/{imgName}";
                switch (Device.CurrentExclusiveStatus)
                {
                    case DS4Device.ExclusiveStatus.Exclusive:
                        imgName = (string) Application.Current.FindResource("CheckedImg");
                        source = $"/DS4Windows;component/Resources/{imgName}";
                        break;
                    case DS4Device.ExclusiveStatus.HidHideAffected:
                    case DS4Device.ExclusiveStatus.HidGuardAffected:
                        imgName = (string) Application.Current.FindResource("KeyImageImg");
                        source = $"/DS4Windows;component/Resources/{imgName}";
                        break;
                }

                return source;
            }
        }

        public bool LinkedProfile
        {
            get => ProfilesService.Instance.ActiveProfiles.ElementAt(DevIndex).IsLinkedProfile;
            set => ProfilesService.Instance.ActiveProfiles.ElementAt(DevIndex).IsLinkedProfile = value;
        }

        public int DevIndex { get; }

        public int DisplayDevIndex => DevIndex + 1;

        public string TooltipIDText => string.Format(Resources.InputDelay, Device.Latency);
        public bool UseCustomColor { get; set; }

        public ContextMenu LightContext { get; set; }

        public string IdText => $"{Device.DisplayName} ({Device.MacAddress.ToFriendlyName()})";

        public string IsExclusiveText
        {
            get
            {
                var temp = Strings.SharedAccess;
                switch (Device.CurrentExclusiveStatus)
                {
                    case DS4Device.ExclusiveStatus.Exclusive:
                        temp = Strings.ExclusiveAccess;
                        break;
                    case DS4Device.ExclusiveStatus.HidHideAffected:
                        temp = Strings.HidHideAccess;
                        break;
                    case DS4Device.ExclusiveStatus.HidGuardAffected:
                        temp = Strings.HidGuardianAccess;
                        break;
                }

                return temp;
            }
        }

        public bool PrimaryDevice => Device.PrimaryDevice;

        public event EventHandler LightColorChanged;

        public event Action<DS4Device> BatteryStateChanged;
        public event EventHandler SelectedIndexChanged;

        public event EventHandler TooltipIDTextChanged;

        public event EventHandler IdTextChanged;
        public event CustomColorHandler RequestColorPicker;

        public async Task ChangeSelectedProfile()
        {
            if (selectedEntity != null) HookEvents(false);

            var prof = Global.Instance.Config.ProfilePath[DevIndex] = ProfileListCol[selectedIndex].Name;
            if (LinkedProfile)
            {
                Global.Instance.Config.ChangeLinkedProfile(Device.MacAddress,
                    Global.Instance.Config.ProfilePath[DevIndex]);
                Global.Instance.Config.SaveLinkedProfiles();
            }
            else
            {
                Global.Instance.Config.OlderProfilePath[DevIndex] = Global.Instance.Config.ProfilePath[DevIndex];
            }

            //Global.Save();
            await Global.Instance.LoadProfile(DevIndex, true, rootHub);
            var prolog = string.Format(Resources.UsingProfile, (DevIndex + 1).ToString(), prof, $"{Device.Battery}");
            AppLogger.Instance.LogToGui(prolog, false);

            SelectedProfile = prof;
            selectedEntity = ProfileEntities.ProfileListCollection.SingleOrDefault(x => x.Name == prof);
            if (selectedEntity != null)
            {
                selectedIndex = ProfileEntities.ProfileListCollection.IndexOf(selectedEntity);
                HookEvents(true);
            }

            LightColorChanged?.Invoke(this, EventArgs.Empty);
        }

        private void HookEvents(bool state)
        {
            if (state)
            {
                selectedEntity.ProfileSaved += SelectedEntity_ProfileSaved;
                selectedEntity.ProfileDeleted += SelectedEntity_ProfileDeleted;
            }
            else
            {
                selectedEntity.ProfileSaved -= SelectedEntity_ProfileSaved;
                selectedEntity.ProfileDeleted -= SelectedEntity_ProfileDeleted;
            }
        }

        private void SelectedEntity_ProfileDeleted(object sender, EventArgs e)
        {
            HookEvents(false);
            var entity = ProfileEntities.ProfileListCollection.FirstOrDefault();
            if (entity != null) SelectedIndex = ProfileEntities.ProfileListCollection.IndexOf(entity);
        }

        private async void SelectedEntity_ProfileSaved(object sender, EventArgs e)
        {
            await Global.Instance.LoadProfile(DevIndex, false, rootHub);
            LightColorChanged?.Invoke(this, EventArgs.Empty);
        }

        public void RequestUpdatedTooltipID()
        {
            TooltipIDTextChanged?.Invoke(this, EventArgs.Empty);
        }

        [MissingLocalization]
        public void AddLightContextItems()
        {
            var thing = new MenuItem {Header = "Use Profile Color", IsChecked = !UseCustomColor};
            thing.Click += ProfileColorMenuClick;
            LightContext.Items.Add(thing);
            thing = new MenuItem {Header = "Use Custom Color", IsChecked = UseCustomColor};
            thing.Click += CustomColorItemClick;
            LightContext.Items.Add(thing);
        }

        private void ProfileColorMenuClick(object sender, RoutedEventArgs e)
        {
            UseCustomColor = false;
            RefreshLightContext();
            appSettings.Settings.LightbarSettingInfo[DevIndex].Ds4WinSettings.UseCustomLed = false;
            LightColorChanged?.Invoke(this, EventArgs.Empty);
        }

        private void CustomColorItemClick(object sender, RoutedEventArgs e)
        {
            UseCustomColor = true;
            RefreshLightContext();
            appSettings.Settings.LightbarSettingInfo[DevIndex].Ds4WinSettings.UseCustomLed = true;
            LightColorChanged?.Invoke(this, EventArgs.Empty);
            RequestColorPicker?.Invoke(this);
        }

        private void RefreshLightContext()
        {
            (LightContext.Items[0] as MenuItem).IsChecked = !UseCustomColor;
            (LightContext.Items[1] as MenuItem).IsChecked = UseCustomColor;
        }

        public void UpdateCustomLightColor(Color color)
        {
            appSettings.Settings.LightbarSettingInfo[DevIndex].Ds4WinSettings.CustomLed = new DS4Color(color);
            LightColorChanged?.Invoke(this, EventArgs.Empty);
        }

        public void ChangeSelectedProfile(string loadprofile)
        {
            var temp = ProfileEntities.ProfileListCollection.SingleOrDefault(x => x.Name == loadprofile);
            if (temp != null) SelectedIndex = ProfileEntities.ProfileListCollection.IndexOf(temp);
        }

        public void RequestDisconnect()
        {
            if (Device.Synced && !Device.Charging)
            {
                if (Device.ConnectionType == ConnectionType.Bluetooth)
                    //device.StopUpdate();
                    Device.QueueEvent(() => { Device.DisconnectBT(); });
                else if (Device.ConnectionType == ConnectionType.SonyWirelessAdapter) Device.DisconnectDongle();
            }
        }
    }
}