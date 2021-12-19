using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using DS4Windows;
using DS4Windows.InputDevices;
using DS4WinWPF.DS4Control.IoC.Services;
using DS4WinWPF.DS4Forms.ViewModels.Util;

namespace DS4WinWPF.DS4Forms.ViewModels
{
    public class ControllerRegDeviceOptsViewModel
    {
        private readonly List<ControllerOptionsStore> controllerOptionsStores = new();

        private int controllerSelectedIndex = -1;

        private int currentTabSelectedIndex;

        // Serial, ControllerOptionsStore instance
        private readonly Dictionary<PhysicalAddress, ControllerOptionsStore> inputDeviceSettings = new();

        public ControllerRegDeviceOptsViewModel(IAppSettingsService appSettings,
            ControlService service)
        {
            AppSettings = appSettings;

            var idx = 0;
            foreach (var device in service.DS4Controllers)
            {
                if (device != null)
                {
                    CurrentInputDevices.Add(new DeviceListItem(device));
                    inputDeviceSettings.Add(device.MacAddress, device.OptionsStore);
                    controllerOptionsStores.Add(device.OptionsStore);
                }

                idx++;
            }
        }

        public List<DeviceListItem> CurrentInputDevices { get; } = new();

        public int ControllerSelectedIndex
        {
            get => controllerSelectedIndex;
            set
            {
                if (controllerSelectedIndex == value) return;
                controllerSelectedIndex = value;
                ControllerSelectedIndexChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public DS4ControllerOptions CurrentDS4Options =>
            controllerOptionsStores[controllerSelectedIndex] as DS4ControllerOptions;

        public DualSenseControllerOptions CurrentDSOptions =>
            controllerOptionsStores[controllerSelectedIndex] as DualSenseControllerOptions;

        public SwitchProControllerOptions CurrentSwitchProOptions =>
            controllerOptionsStores[controllerSelectedIndex] as SwitchProControllerOptions;

        public JoyConControllerOptions CurrentJoyConOptions =>
            controllerOptionsStores[controllerSelectedIndex] as JoyConControllerOptions;

        public int CurrentTabSelectedIndex
        {
            get => currentTabSelectedIndex;
            set
            {
                if (currentTabSelectedIndex == value) return;
                currentTabSelectedIndex = value;
                CurrentTabSelectedIndexChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public IAppSettingsService AppSettings { get; }
        public object DataContextObject { get; private set; }

        public event EventHandler ControllerSelectedIndexChanged;
        public event EventHandler CurrentTabSelectedIndexChanged;

        public int FindTabOptionsIndex()
        {
            var currentStore =
                controllerOptionsStores[controllerSelectedIndex];

            var result = 0;

            switch (currentStore)
            {
                case DS4ControllerOptions _:
                    result = 1;
                    break;
                case DualSenseControllerOptions _:
                    result = 2;
                    break;
                case SwitchProControllerOptions _:
                    result = 3;
                    break;
                case JoyConControllerOptions _:
                    result = 4;
                    break;
            }

            return result;
        }

        public void FindFittingDataContext()
        {
            var currentStore =
                controllerOptionsStores[controllerSelectedIndex];

            switch (currentStore)
            {
                case DS4ControllerOptions _:
                    DataContextObject = new DS4ControllerOptionsWrapper(CurrentDS4Options,
                        AppSettings.Settings.DeviceOptions.DS4SupportSettings);
                    break;
                case DualSenseControllerOptions _:
                    DataContextObject = new DualSenseControllerOptionsWrapper(CurrentDSOptions,
                        AppSettings.Settings.DeviceOptions.DualSenseSupportSettings);
                    break;
                case SwitchProControllerOptions _:
                    DataContextObject = new SwitchProControllerOptionsWrapper(CurrentSwitchProOptions,
                        AppSettings.Settings.DeviceOptions.SwitchProSupportSettings);
                    break;
                case JoyConControllerOptions _:
                    DataContextObject = new JoyConControllerOptionsWrapper(CurrentJoyConOptions,
                        AppSettings.Settings.DeviceOptions.JoyConSupportSettings);
                    break;
            }
        }

        public void SaveControllerConfigs()
        {
            foreach (var item in CurrentInputDevices) Global.Instance.Config.SaveControllerConfigs(item.Device);
        }
    }

    public class DeviceListItem
    {
        public DeviceListItem(DS4Device device)
        {
            Device = device;
        }

        public DS4Device Device { get; }

        public string IdText => $"{Device.DisplayName} ({Device.MacAddress})";
    }


    public class DS4ControllerOptionsWrapper
    {
        private readonly DS4DeviceOptions parentOptions;

        public DS4ControllerOptionsWrapper(DS4ControllerOptions options, DS4DeviceOptions parentOpts)
        {
            Options = options;
            parentOptions = parentOpts;
            parentOptions.EnabledChanged += () => { VisibleChanged?.Invoke(this, EventArgs.Empty); };
        }

        public DS4ControllerOptions Options { get; }

        public bool Visible => parentOptions.Enabled;

        public event EventHandler VisibleChanged;
    }

    public class DualSenseControllerOptionsWrapper
    {
        private readonly DualSenseDeviceOptions parentOptions;

        public DualSenseControllerOptionsWrapper(DualSenseControllerOptions options,
            DualSenseDeviceOptions parentOpts)
        {
            Options = options;
            parentOptions = parentOpts;
            parentOptions.EnabledChanged += () => { VisibleChanged?.Invoke(this, EventArgs.Empty); };
        }

        public DualSenseControllerOptions Options { get; }

        public bool Visible => parentOptions.Enabled;

        public List<DSHapticsChoiceEnum> DSHapticOptions { get; } = new()
        {
            new DSHapticsChoiceEnum("Low", DualSenseDevice.HapticIntensity.Low),
            new DSHapticsChoiceEnum("Medium", DualSenseDevice.HapticIntensity.Medium),
            new DSHapticsChoiceEnum("High", DualSenseDevice.HapticIntensity.High)
        };

        public List<EnumChoiceSelection<DualSenseControllerOptions.LEDBarMode>> DsLEDModes { get; } = new()
        {
            new EnumChoiceSelection<DualSenseControllerOptions.LEDBarMode>("Off",
                DualSenseControllerOptions.LEDBarMode.Off),
            new EnumChoiceSelection<DualSenseControllerOptions.LEDBarMode>("Only for multiple controllers",
                DualSenseControllerOptions.LEDBarMode.MultipleControllers),
            new EnumChoiceSelection<DualSenseControllerOptions.LEDBarMode>("Battery Percentage",
                DualSenseControllerOptions.LEDBarMode.BatteryPercentage),
            new EnumChoiceSelection<DualSenseControllerOptions.LEDBarMode>("On",
                DualSenseControllerOptions.LEDBarMode.On)
        };

        public List<EnumChoiceSelection<DualSenseControllerOptions.MuteLEDMode>> DsMuteLEDModes { get; } = new()
        {
            new EnumChoiceSelection<DualSenseControllerOptions.MuteLEDMode>("Off",
                DualSenseControllerOptions.MuteLEDMode.Off),
            new EnumChoiceSelection<DualSenseControllerOptions.MuteLEDMode>("On",
                DualSenseControllerOptions.MuteLEDMode.On),
            new EnumChoiceSelection<DualSenseControllerOptions.MuteLEDMode>("Pulse",
                DualSenseControllerOptions.MuteLEDMode.Pulse)
        };

        public event EventHandler VisibleChanged;
    }

    public class SwitchProControllerOptionsWrapper
    {
        private readonly SwitchProDeviceOptions parentOptions;

        public SwitchProControllerOptionsWrapper(SwitchProControllerOptions options,
            SwitchProDeviceOptions parentOpts)
        {
            Options = options;
            parentOptions = parentOpts;
            parentOptions.EnabledChanged += () => { VisibleChanged?.Invoke(this, EventArgs.Empty); };
        }

        public SwitchProControllerOptions Options { get; }

        public bool Visible => parentOptions.Enabled;
        public event EventHandler VisibleChanged;
    }

    public class JoyConControllerOptionsWrapper
    {
        public JoyConControllerOptionsWrapper(JoyConControllerOptions options,
            JoyConDeviceOptions parentOpts)
        {
            Options = options;
            ParentOptions = parentOpts;
            ParentOptions.EnabledChanged += () => { VisibleChanged?.Invoke(this, EventArgs.Empty); };
        }

        public JoyConControllerOptions Options { get; }

        public JoyConDeviceOptions ParentOptions { get; }

        public bool Visible => ParentOptions.Enabled;

        public List<EnumChoiceSelection<JoyConDeviceOptions.LinkMode>> LinkModes { get; } = new()
        {
            new EnumChoiceSelection<JoyConDeviceOptions.LinkMode>("Split", JoyConDeviceOptions.LinkMode.Split),
            new EnumChoiceSelection<JoyConDeviceOptions.LinkMode>("Joined", JoyConDeviceOptions.LinkMode.Joined)
        };

        public List<EnumChoiceSelection<JoyConDeviceOptions.JoinedGyroProvider>> JoinGyroOptions { get; } = new()
        {
            new EnumChoiceSelection<JoyConDeviceOptions.JoinedGyroProvider>("Left",
                JoyConDeviceOptions.JoinedGyroProvider.JoyConL),
            new EnumChoiceSelection<JoyConDeviceOptions.JoinedGyroProvider>("Right",
                JoyConDeviceOptions.JoinedGyroProvider.JoyConR)
        };

        public event EventHandler VisibleChanged;
    }

    public class DSHapticsChoiceEnum
    {
        public DSHapticsChoiceEnum(string name,
            DualSenseDevice.HapticIntensity intensity)
        {
            DisplayName = name;
            ChoiceValue = intensity;
        }

        public string DisplayName { get; } = string.Empty;

        public DualSenseDevice.HapticIntensity ChoiceValue { get; set; }

        public override string ToString()
        {
            return DisplayName;
        }
    }
}