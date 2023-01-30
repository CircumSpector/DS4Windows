using System.Configuration;
using System.Diagnostics;

using Microsoft.Extensions.DependencyInjection;

using Nefarius.Utilities.DeviceManagement.PnP;

using Vapour.Shared.Common.Types;
using Vapour.Shared.Devices.HID;
using Vapour.Shared.Devices.HID.DeviceInfos;
using Vapour.Shared.Devices.Services.Configuration;
using Vapour.Shared.Devices.Services.ControllerEnumerators;

namespace Vapour.Shared.Devices.Services;

internal sealed class InputSourceService : IInputSourceService
{
    private readonly IInputSourceBuilderService _inputSourceBuilderService;
    private readonly IInputSourceDataSource _inputSourceDataSource;
    private readonly IFilterService _filterService;
    private readonly IHidDeviceEnumeratorService<HidDevice> _hidEnumeratorService;
    private readonly IHidDeviceEnumeratorService<HidDeviceOverWinUsb> _winUsbDeviceEnumeratorService;
    private readonly IDeviceFactory _deviceFactory;
    private readonly IInputSourceConfigurationService _inputSourceConfigurationService;
    private readonly IGameProcessWatcherService _gameProcessWatcherService;
    private readonly List<ICompatibleHidDevice> _controllers = new();

    private bool _isPerformingFilterAction;

    public InputSourceService(
        IInputSourceBuilderService inputSourceBuilderService,
        IInputSourceDataSource inputSourceDataSource,
        IFilterService filterService,
        IHidDeviceEnumeratorService<HidDevice> hidEnumeratorService,
        IHidDeviceEnumeratorService<HidDeviceOverWinUsb> winUsbDeviceEnumeratorService,
        IDeviceFactory deviceFactory,
        IInputSourceConfigurationService inputSourceConfigurationService,
        IGameProcessWatcherService gameProcessWatcherService)
    {
        _inputSourceBuilderService = inputSourceBuilderService;
        _inputSourceDataSource = inputSourceDataSource;
        _filterService = filterService;
        _hidEnumeratorService = hidEnumeratorService;
        _winUsbDeviceEnumeratorService = winUsbDeviceEnumeratorService;
        _deviceFactory = deviceFactory;
        _inputSourceConfigurationService = inputSourceConfigurationService;
        _gameProcessWatcherService = gameProcessWatcherService;
    }

    public event Action InputSourceListReady;

    public bool ShouldAutoRebuild { get; set; } = true;
    public bool ShouldFixupOnConfigChange { get; set; } = true;

    public async Task Start()
    {
        _hidEnumeratorService.DeviceArrived += SetupDevice;
        _hidEnumeratorService.DeviceRemoved += RemoveDevice;

        _winUsbDeviceEnumeratorService.DeviceArrived += SetupDevice;
        _winUsbDeviceEnumeratorService.DeviceRemoved += RemoveDevice;

        ShouldAutoRebuild = false;

        _hidEnumeratorService.Start();
        _winUsbDeviceEnumeratorService.Start();

        await RebuildInputSourceList();

        ShouldAutoRebuild = true;

        _inputSourceConfigurationService.OnDefaultConfigurationUpdated += DefaultConfigurationUpdated;
        _inputSourceConfigurationService.OnRefreshConfigurations += RefreshConfigurations;

        _gameProcessWatcherService.GameWatchStarted += GameWatchStarted;
        _gameProcessWatcherService.GameWatchStopped += GameWatchStopped;

        InputSourceListReady?.Invoke();
    }

    public void Stop()
    {
        _inputSourceConfigurationService.OnDefaultConfigurationUpdated -= DefaultConfigurationUpdated;
        _inputSourceConfigurationService.OnRefreshConfigurations -= RefreshConfigurations;

        _gameProcessWatcherService.GameWatchStarted -= GameWatchStarted;
        _gameProcessWatcherService.GameWatchStopped -= GameWatchStopped;

        _hidEnumeratorService.DeviceArrived -= SetupDevice;
        _hidEnumeratorService.DeviceRemoved -= RemoveDevice;

        _winUsbDeviceEnumeratorService.DeviceArrived -= SetupDevice;
        _winUsbDeviceEnumeratorService.DeviceRemoved -= RemoveDevice;

        ClearExistingSources();
    }

    private void ClearExistingSources()
    {
        foreach (var inputSource in _inputSourceDataSource.InputSources.ToList())
        {
            inputSource.Stop();

            foreach (var device in _controllers)
            {
                if (device.Connection == ConnectionType.Bluetooth)
                {
                    //sdp unfilter
                }
                else if (device.Connection == ConnectionType.Usb)
                {
                    _filterService.UnfilterController(device.SourceDevice.InstanceId);
                }
            }

            //restart bt host
            
            inputSource.ConfigurationChanged -= InputSource_ConfigurationChanged;
            _inputSourceDataSource.InputSources.Remove(inputSource);
        }
    }
    
    private async Task RebuildInputSourceList()
    {
        ClearExistingSources();

        await Task.Run(async () =>
        {
            var inputSourceList = _inputSourceBuilderService.BuildInputSourceList(_controllers);
            await PerformFilterActionsIfNeeded(inputSourceList);
            StartInputSources();
        });
    }

    private async Task PerformFilterActionsIfNeeded(List<IInputSource> inputSourceList)
    {
        await CheckPerformBluetoothFilterActions(inputSourceList);
        await CheckPerformUsbFilterActions(inputSourceList);
    }

    private async Task CheckPerformUsbFilterActions(List<IInputSource> inputSourceList)
    {
        var usbDevicesToFilter = GetNeededFilterListByConnectionType(inputSourceList, ConnectionType.Usb);

        if (usbDevicesToFilter.Any())
        {
            _isPerformingFilterAction = true;

            foreach (var deviceFilterInfo in usbDevicesToFilter)
            {
                if (deviceFilterInfo.IsFiltered)
                {
                    _filterService.UnfilterController(deviceFilterInfo.InstanceId);
                }
                else
                {
                    _filterService.FilterController(deviceFilterInfo.InstanceId);
                }

                //dont need to do anything else because if this works existing
                //compatible hid devices already on input sources should be
                //updated with a new source device
                while (true)
                {
                    if (_controllers.ToList().SingleOrDefault(c =>
                            c.DeviceKey == deviceFilterInfo.DeviceKey &&
                            c.IsFiltered == !deviceFilterInfo.IsFiltered) !=
                        null)
                    {
                        break;
                    }
                }
            }

            _isPerformingFilterAction = false;
        }
    }

    private async Task CheckPerformBluetoothFilterActions(List<IInputSource> inputSourceList)
    {
        var blueToothDevicesToFilerAction =
            GetNeededFilterListByConnectionType(inputSourceList, ConnectionType.Bluetooth);

        if (blueToothDevicesToFilerAction.Any())
        {
            _isPerformingFilterAction = true;

            foreach (var device in blueToothDevicesToFilerAction)
            {
                if (device.IsFiltered)
                {
                    //update sdp record to not filtered
                }
                else
                {
                    //update sdp record to filtered
                }
            }

            //await restart bt host

            //dont need to do anything else because if this works existing
            //compatible hid devices already on input sources should be
            //updated with a new source device
            while (true)
            {
                if (blueToothDevicesToFilerAction.All(f =>
                        _controllers.SingleOrDefault(c =>
                            c.DeviceKey == f.DeviceKey && c.IsFiltered == !f.IsFiltered) != null))
                {
                    break;
                }
            }

            _isPerformingFilterAction = false; 
        }
    }

    private List<(string DeviceKey, string InstanceId, bool IsFiltered)> GetNeededFilterListByConnectionType(List<IInputSource> inputSourceList, ConnectionType connectionType)
    {
        var deviceList = inputSourceList.SelectMany(i => i.Controllers.Where(c =>
                c.Connection == connectionType &&
                ((c.CurrentConfiguration.OutputDeviceType == OutputDeviceType.None && c.IsFiltered) ||
                 (c.CurrentConfiguration.OutputDeviceType != OutputDeviceType.None && !c.IsFiltered))))
            .Select(c => (c.DeviceKey, c.SourceDevice.InstanceId, c.IsFiltered))
            .ToList();

        return deviceList;
    }

    private void StartInputSources()
    {
        for (var i = 0; i < _inputSourceDataSource.InputSources.Count; i++)
        {
            var inputSource = _inputSourceDataSource.InputSources[i];
            inputSource.ConfigurationChanged += InputSource_ConfigurationChanged;
            inputSource.SetPlayerNumberAndColor(i + 1);

            inputSource.Start();
            _inputSourceDataSource.FireCreated(inputSource);
        }
    }
    
    private async void InputSource_ConfigurationChanged(object sender, InputSourceConfiguration e)
    {
        
    }
    
    private async void SetupDevice(IHidDevice hidDevice)
    {
        var deviceInfo = _deviceFactory.IsKnownDevice(hidDevice.VendorId, hidDevice.ProductId);

        if (deviceInfo is null)
        {
            return;
        }

        if ((hidDevice.Capabilities.Usage is not (HidDevice.HidUsageGamepad or HidDevice.HidUsageJoystick) &&
             !deviceInfo.FeatureSet.HasFlag(CompatibleHidDeviceFeatureSet.VendorDefinedDevice))
            || hidDevice.IsVirtual)
        {
            return;
        }

        ICompatibleHidDevice device;

        var existingInputSource = _inputSourceDataSource.GetByDeviceInstanceId(hidDevice.InstanceId);
        if (existingInputSource != null)
        {
            existingInputSource.Stop();
            device = existingInputSource.GetControllerByInstanceId(hidDevice.InstanceId);
        }
        else
        {
            device = _deviceFactory.CreateDevice(deviceInfo, hidDevice);
            _controllers.Add(device);
        }

        // TODO: take Bluetooth into account
        if (hidDevice is HidDeviceOverWinUsb)
        {
            device.IsFiltered = true;
        }

        device.Setup(hidDevice, deviceInfo);

        if (ShouldAutoRebuild)
        {
            await RebuildInputSourceList();
        }
    }

    private async void RemoveDevice(string instanceId)
    {
        if (!_isPerformingFilterAction && ShouldAutoRebuild)
        {
            var existingController =
                _controllers.SingleOrDefault(c => c.SourceDevice.InstanceId.ToLower() == instanceId.ToLower());

            if (existingController != null)
            {
                _controllers.Remove(existingController);
                existingController.Dispose();
                await RebuildInputSourceList();
            }
        }
    }

    private async void RefreshConfigurations()
    {
        ShouldAutoRebuild = false;
        await RebuildInputSourceList();
        ShouldAutoRebuild = true;
    }

    private async void DefaultConfigurationUpdated(string inputSourceKey)
    {
        //optimize rebuild logic to only rebuild the one
        ShouldAutoRebuild = false;
        await RebuildInputSourceList();
        ShouldAutoRebuild = true;
    }

    private async void GameWatchStopped(ProcessorWatchItem obj)
    {
        ShouldAutoRebuild = false;
        await RebuildInputSourceList();
        ShouldAutoRebuild = true;
    }

    private async void GameWatchStarted(ProcessorWatchItem obj)
    {
        ShouldAutoRebuild = false;
        await RebuildInputSourceList();
        ShouldAutoRebuild = true;
    }
}