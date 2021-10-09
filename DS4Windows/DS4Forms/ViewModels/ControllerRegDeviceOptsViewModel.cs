using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using DS4Windows;
using DS4WinWPF.DS4Control.IoC.Services;
using DS4WinWPF.DS4Forms.ViewModels.Util;
using LEDBarMode = DS4Windows.DualSenseControllerOptions.LEDBarMode;
using MuteLEDMode = DS4Windows.DualSenseControllerOptions.MuteLEDMode;
using LinkMode = DS4Windows.JoyConDeviceOptions.LinkMode;
using JoinedGyroProvider = DS4Windows.JoyConDeviceOptions.JoinedGyroProvider;

namespace DS4WinWPF.DS4Forms.ViewModels
{
    public class ControllerRegDeviceOptsViewModel
    {
        private List<DeviceListItem> currentInputDevices = new List<DeviceListItem>();
        public List<DeviceListItem> CurrentInputDevices { get => currentInputDevices; }

        // Serial, ControllerOptionsStore instance
        private Dictionary<PhysicalAddress, ControllerOptionsStore> inputDeviceSettings = new();
        private List<ControllerOptionsStore> controllerOptionsStores = new List<ControllerOptionsStore>();

        private int controllerSelectedIndex = -1;
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
        public event EventHandler ControllerSelectedIndexChanged;

        public DS4ControllerOptions CurrentDS4Options
        {
            get => controllerOptionsStores[controllerSelectedIndex] as DS4ControllerOptions;
        }

        public DualSenseControllerOptions CurrentDSOptions
        {
            get => controllerOptionsStores[controllerSelectedIndex] as DualSenseControllerOptions;
        }

        public SwitchProControllerOptions CurrentSwitchProOptions
        {
            get => controllerOptionsStores[controllerSelectedIndex] as SwitchProControllerOptions;
        }

        public JoyConControllerOptions CurrentJoyConOptions
        {
            get => controllerOptionsStores[controllerSelectedIndex] as JoyConControllerOptions;
        }

        private int currentTabSelectedIndex = 0;
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
        public event EventHandler CurrentTabSelectedIndexChanged;

        public IAppSettingsService AppSettings { get; }

        public ControllerRegDeviceOptsViewModel(IAppSettingsService appSettings,
            ControlService service)
        {
            AppSettings = appSettings;

            int idx = 0;
            foreach(DS4Device device in service.DS4Controllers)
            {
                if (device != null)
                {
                    currentInputDevices.Add(new DeviceListItem(device));
                    inputDeviceSettings.Add(device.MacAddress, device.OptionsStore);
                    controllerOptionsStores.Add(device.OptionsStore);
                }
                idx++;
            }
        }

        private object dataContextObject = null;
        public object DataContextObject { get => dataContextObject; }

        public int FindTabOptionsIndex()
        {
            ControllerOptionsStore currentStore =
                controllerOptionsStores[controllerSelectedIndex];

            int result = 0;

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
            ControllerOptionsStore currentStore =
                controllerOptionsStores[controllerSelectedIndex];

            switch (currentStore)
            {
                case DS4ControllerOptions _:
                    dataContextObject = new DS4ControllerOptionsWrapper(CurrentDS4Options, AppSettings.Settings.DeviceOptions.DS4SupportSettings);
                    break;
                case DualSenseControllerOptions _:
                    dataContextObject = new DualSenseControllerOptionsWrapper(CurrentDSOptions, AppSettings.Settings.DeviceOptions.DualSenseSupportSettings);
                    break;
                case SwitchProControllerOptions _:
                    dataContextObject = new SwitchProControllerOptionsWrapper(CurrentSwitchProOptions, AppSettings.Settings.DeviceOptions.SwitchProSupportSettings);
                    break;
                case JoyConControllerOptions _:
                    dataContextObject = new JoyConControllerOptionsWrapper(CurrentJoyConOptions, AppSettings.Settings.DeviceOptions.JoyConSupportSettings);
                    break;
            }
        }

        public void SaveControllerConfigs()
        {
            foreach (DeviceListItem item in currentInputDevices)
            {
                Global.Instance.Config.SaveControllerConfigs(item.Device);
            }
        }
    }

    public class DeviceListItem
    {
        private DS4Device device;
        public DS4Device Device { get => device; }

        public string IdText
        {
            get => $"{device.DisplayName} ({device.MacAddress})";
        }

        public DeviceListItem(DS4Device device)
        {
            this.device = device;
        }
    }


    public class DS4ControllerOptionsWrapper
    {
        private DS4ControllerOptions options;
        public DS4ControllerOptions Options { get => options; }

        private DS4DeviceOptions parentOptions;
        public bool Visible
        {
            get => parentOptions.Enabled;
        }
        public event EventHandler VisibleChanged;

        public DS4ControllerOptionsWrapper(DS4ControllerOptions options, DS4DeviceOptions parentOpts)
        {
            this.options = options;
            this.parentOptions = parentOpts;
            parentOptions.EnabledChanged += () =>
            {
                VisibleChanged?.Invoke(this, EventArgs.Empty);
            };
        }
    }

    public class DualSenseControllerOptionsWrapper
    {
        private DualSenseControllerOptions options;
        public DualSenseControllerOptions Options { get => options; }

        private DualSenseDeviceOptions parentOptions;
        public bool Visible { get => parentOptions.Enabled; }
        public event EventHandler VisibleChanged;

        private List<DSHapticsChoiceEnum> dsHapticOptions = new List<DSHapticsChoiceEnum>()
        {
            new DSHapticsChoiceEnum("Low", DS4Windows.InputDevices.DualSenseDevice.HapticIntensity.Low),
            new DSHapticsChoiceEnum("Medium", DS4Windows.InputDevices.DualSenseDevice.HapticIntensity.Medium),
            new DSHapticsChoiceEnum("High", DS4Windows.InputDevices.DualSenseDevice.HapticIntensity.High)
        };
        public List<DSHapticsChoiceEnum> DSHapticOptions { get => dsHapticOptions; }

        private List<EnumChoiceSelection<LEDBarMode>> dsLEDModeOptions = new List<EnumChoiceSelection<LEDBarMode>>()
        {
            new EnumChoiceSelection<LEDBarMode>("Off", LEDBarMode.Off),
            new EnumChoiceSelection<LEDBarMode>("Only for multiple controllers", LEDBarMode.MultipleControllers),
            new EnumChoiceSelection<LEDBarMode>("Battery Percentage", LEDBarMode.BatteryPercentage),
            new EnumChoiceSelection<LEDBarMode>("On", LEDBarMode.On),
        };
        public List<EnumChoiceSelection<LEDBarMode>> DsLEDModes { get => dsLEDModeOptions; }

        private List<EnumChoiceSelection<MuteLEDMode>> dsMuteLEDModes = new List<EnumChoiceSelection<MuteLEDMode>>()
        {
            new EnumChoiceSelection<MuteLEDMode>("Off", MuteLEDMode.Off),
            new EnumChoiceSelection<MuteLEDMode>("On", MuteLEDMode.On),
            new EnumChoiceSelection<MuteLEDMode>("Pulse", MuteLEDMode.Pulse),
        };
        public List<EnumChoiceSelection<MuteLEDMode>> DsMuteLEDModes { get => dsMuteLEDModes; }

        public DualSenseControllerOptionsWrapper(DualSenseControllerOptions options,
            DualSenseDeviceOptions parentOpts)
        {
            this.options = options;
            this.parentOptions = parentOpts;
            parentOptions.EnabledChanged += () => { VisibleChanged?.Invoke(this, EventArgs.Empty); };
        }
    }

    public class SwitchProControllerOptionsWrapper
    {
        private SwitchProControllerOptions options;
        public SwitchProControllerOptions Options { get => options; }

        private SwitchProDeviceOptions parentOptions;
        public bool Visible { get => parentOptions.Enabled; }
        public event EventHandler VisibleChanged;

        public SwitchProControllerOptionsWrapper(SwitchProControllerOptions options,
            SwitchProDeviceOptions parentOpts)
        {
            this.options = options;
            this.parentOptions = parentOpts;
            parentOptions.EnabledChanged += () => { VisibleChanged?.Invoke(this, EventArgs.Empty); };
        }
    }

    public class JoyConControllerOptionsWrapper
    {
        private JoyConControllerOptions options;
        public JoyConControllerOptions Options { get => options; }

        private JoyConDeviceOptions parentOptions;
        public JoyConDeviceOptions ParentOptions { get => parentOptions; }

        public bool Visible { get => parentOptions.Enabled; }
        public event EventHandler VisibleChanged;

        private List<EnumChoiceSelection<LinkMode>> linkModes = new List<EnumChoiceSelection<LinkMode>>()
        {
            new EnumChoiceSelection<LinkMode>("Split", LinkMode.Split),
            new EnumChoiceSelection<LinkMode>("Joined", LinkMode.Joined),
        };
        public List<EnumChoiceSelection<LinkMode>> LinkModes { get => linkModes; }

        private List<EnumChoiceSelection<JoinedGyroProvider>> joinGyroOptions = new List<EnumChoiceSelection<JoinedGyroProvider>>()
        {
            new EnumChoiceSelection<JoinedGyroProvider>("Left", JoinedGyroProvider.JoyConL),
            new EnumChoiceSelection<JoinedGyroProvider>("Right", JoinedGyroProvider.JoyConR),
        };
        public List<EnumChoiceSelection<JoinedGyroProvider>> JoinGyroOptions { get => joinGyroOptions; }

        public JoyConControllerOptionsWrapper(JoyConControllerOptions options,
            JoyConDeviceOptions parentOpts)
        {
            this.options = options;
            this.parentOptions = parentOpts;
            parentOptions.EnabledChanged += () => { VisibleChanged?.Invoke(this, EventArgs.Empty); };
        }
    }

    public class DSHapticsChoiceEnum
    {
        private string displayName = string.Empty;
        public string DisplayName { get => displayName; }

        private DS4Windows.InputDevices.DualSenseDevice.HapticIntensity choiceValue;
        public DS4Windows.InputDevices.DualSenseDevice.HapticIntensity ChoiceValue
        {
            get => choiceValue;
            set => choiceValue = value;
        }

        public DSHapticsChoiceEnum(string name,
            DS4Windows.InputDevices.DualSenseDevice.HapticIntensity intensity)
        {
            displayName = name;
            choiceValue = intensity;
        }

        public override string ToString()
        {
            return displayName;
        }
    }
}
