using Vapour.Shared.Common.Types;
using Vapour.Shared.Devices.HID;
using Vapour.Shared.Devices.HID.DeviceInfos;
using Vapour.Shared.Devices.Services.Configuration;
using Vapour.Shared.Devices.Services.ControllerEnumerators;
using Vapour.Shared.Devices.Services.Reporting.CustomActions;

namespace Vapour.Shared.Devices.Services;

internal sealed class InputSourceService : IInputSourceService
{
    private readonly List<ICompatibleHidDevice> _controllers = new();
    private readonly IDeviceFactory _deviceFactory;
    private readonly IFilterService _filterService;
    private readonly IGameProcessWatcherService _gameProcessWatcherService;
    private readonly IHidDeviceEnumeratorService<HidDevice> _hidEnumeratorService;
    private readonly IInputSourceBuilderService _inputSourceBuilderService;
    private readonly IInputSourceConfigurationService _inputSourceConfigurationService;
    private readonly IInputSourceDataSource _inputSourceDataSource;
    private readonly IHidDeviceEnumeratorService<HidDeviceOverWinUsb> _winUsbDeviceEnumeratorService;
  
    private bool _isPerformingFilterAction;

    private List<ICompatibleHidDevice> _devicesToFilter;
    private ManualResetEventSlim _devicesFilterWait = new(false);

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

    public bool ShouldAutoRebuild { get; set; } = true;

    public bool IsStopping { get; set; }

    public event Action InputSourceListReady;

    public async Task Start(CancellationToken ct = default)
    {
        _hidEnumeratorService.DeviceArrived += SetupDevice;
        _hidEnumeratorService.DeviceRemoved += RemoveDevice;

        _winUsbDeviceEnumeratorService.DeviceArrived += SetupDevice;
        _winUsbDeviceEnumeratorService.DeviceRemoved += RemoveDevice;

        ShouldAutoRebuild = false;

        _hidEnumeratorService.Start();
        _winUsbDeviceEnumeratorService.Start();

        await RebuildInputSourceList(ct);

        ShouldAutoRebuild = true;

        _inputSourceConfigurationService.OnDefaultConfigurationUpdated += DefaultConfigurationUpdated;
        _inputSourceConfigurationService.OnRefreshConfigurations += RefreshConfigurations;

        _gameProcessWatcherService.GameWatchStarted += GameWatchStarted;
        _gameProcessWatcherService.GameWatchStopped += GameWatchStopped;

        _filterService.FilterDriverEnabledChanged += FilterDriverEnabledChanged;

        InputSourceListReady?.Invoke();
    }

    public void Stop()
    {
        _filterService.FilterDriverEnabledChanged -= FilterDriverEnabledChanged;

        _inputSourceConfigurationService.OnDefaultConfigurationUpdated -= DefaultConfigurationUpdated;
        _inputSourceConfigurationService.OnRefreshConfigurations -= RefreshConfigurations;

        _gameProcessWatcherService.GameWatchStarted -= GameWatchStarted;
        _gameProcessWatcherService.GameWatchStopped -= GameWatchStopped;

        _hidEnumeratorService.DeviceArrived -= SetupDevice;
        _hidEnumeratorService.DeviceRemoved -= RemoveDevice;

        _winUsbDeviceEnumeratorService.DeviceArrived -= SetupDevice;
        _winUsbDeviceEnumeratorService.DeviceRemoved -= RemoveDevice;

        IsStopping = true;
        ClearExistingSources();
        IsStopping = false;
    }

    private void ClearExistingSources()
    {
        foreach (IInputSource inputSource in _inputSourceDataSource.InputSources.ToList())
        {
            inputSource.Stop();
            inputSource.OnCustomActionDetected -= OnCustomAction;
            _inputSourceDataSource.InputSources.Remove(inputSource);
            _inputSourceDataSource.FireRemoved(inputSource);
        }

        if (IsStopping)
        {
            var hasBtDevices = _controllers.Any(c => c.Connection == ConnectionType.Bluetooth);
            foreach (ICompatibleHidDevice device in _controllers.ToList())
            {
                _filterService.UnfilterController(device);
            }

            if (hasBtDevices)
            {
                _filterService.RestartBtHost();
            }
        }

        //need some time after stopping input sources before starting more up
        Thread.Sleep(250);
    }

    private async Task RebuildInputSourceList(CancellationToken ct = default)
    {
        bool existingShouldRebuild = ShouldAutoRebuild;
        ShouldAutoRebuild = false;
        ClearExistingSources();

        await Task.Run(async () =>
        {
            List<IInputSource> inputSourceList = _inputSourceBuilderService.BuildInputSourceList(_controllers);
            _inputSourceDataSource.InputSources.AddRange(inputSourceList);
            await CheckPerformFilterActions(inputSourceList);
            StartInputSources();
            ShouldAutoRebuild = existingShouldRebuild;
        }, ct);
    }
    
    private Task CheckPerformFilterActions(List<IInputSource> inputSourceList)
    {
        _devicesToFilter = new List<ICompatibleHidDevice>();

        _devicesToFilter.AddRange(GetNeededFilterListByConnectionType(inputSourceList, ConnectionType.Usb)
            .Where(c => c.CurrentDeviceInfo.WinUsbEndpoints != null).ToList());

        var blueToothDevices = GetNeededFilterListByConnectionType(inputSourceList, ConnectionType.Bluetooth)
            .Where(c => c.CurrentDeviceInfo.IsBtFilterable).ToList();

        _devicesToFilter.AddRange(blueToothDevices);

        if (_devicesToFilter.Any())
        {
            _isPerformingFilterAction = true;

            foreach (var device in _devicesToFilter)
            {
                if (device.IsFiltered)
                {
                    _filterService.UnfilterController(device);
                }
                else
                {
                    _filterService.FilterController(device);
                }
            }

            if (blueToothDevices.Any())
            {
                _filterService.RestartBtHost();
            }

            _devicesFilterWait.Wait();
            _devicesToFilter = null;
            _devicesFilterWait.Reset();

            _isPerformingFilterAction = false;
        }

        return Task.CompletedTask;
    }

    private IEnumerable<ICompatibleHidDevice> GetNeededFilterListByConnectionType(IEnumerable<IInputSource> inputSourceList, ConnectionType connectionType)
    {
        var finalList = new List<ICompatibleHidDevice>();
        foreach (ICompatibleHidDevice controller in inputSourceList
                     .SelectMany(i => i.Controllers.Where(c => c.Connection == connectionType))
                     .ToList())
        {
            if (controller.IsFiltered && !_filterService.IsFilterDriverEnabled)
            {
                finalList.Add(controller);
            }
            else if (controller.IsFiltered && controller.CurrentConfiguration.OutputDeviceType == OutputDeviceType.None)
            {
                finalList.Add(controller);
            }
            else if (!controller.IsFiltered && _filterService.IsFilterDriverEnabled &&
                     controller.CurrentConfiguration.OutputDeviceType != OutputDeviceType.None)
            {
                finalList.Add(controller);
            }
        }

        return finalList;
    }

    private void StartInputSources()
    {
        for (int i = 0; i < _inputSourceDataSource.InputSources.Count; i++)
        {
            IInputSource inputSource = _inputSourceDataSource.InputSources[i];
            inputSource.OnCustomActionDetected += OnCustomAction;
            inputSource.Start();
            inputSource.SetPlayerNumberAndColor(i + 1);
            _inputSourceDataSource.FireCreated(inputSource);
        }
    }

    private async void SetupDevice(IHidDevice hidDevice)
    {
        if (!IsStopping)
        {
            DeviceInfo deviceInfo = _deviceFactory.IsKnownDevice(hidDevice.VendorId, hidDevice.ProductId);

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

            var device = GetDevice(hidDevice, deviceInfo);
            device.Device.Setup(hidDevice, deviceInfo);
            device.Device.Initialize();

            if (device.Device.Connection == ConnectionType.Bluetooth)
            {
                device.Device.IsFiltered = _filterService.IsBtFiltered(device.Device.SourceDevice.InstanceId);
            }
            else if (hidDevice is HidDeviceOverWinUsb)
            {
                device.Device.IsFiltered = true;
            }
            else
            {
                device.Device.IsFiltered = false;
            }

            if (device.ExistingFilterState.HasValue)
            {
                CheckIfWaitingAfterFilterAction(device.Device, device.ExistingFilterState.Value);
            }

            if (ShouldAutoRebuild)
            {
                await RebuildInputSourceList();
            }
        }
    }

    private void CheckIfWaitingAfterFilterAction(ICompatibleHidDevice device, bool previousFilterState)
    {
        var existingWait = _devicesToFilter.SingleOrDefault(c =>
            c.DeviceKey == device.DeviceKey && device.IsFiltered == !previousFilterState);
        if (existingWait != default)
        {
            _devicesToFilter.Remove(existingWait);
            if (_devicesToFilter.Count == 0)
            {
                _devicesFilterWait.Set();
            }
        }
    }

    private (ICompatibleHidDevice Device, bool? ExistingFilterState) GetDevice(IHidDevice hidDevice, DeviceInfo deviceInfo)
    {
        ICompatibleHidDevice device = null;
        bool? existingFilterState = null;
        if (hidDevice is HidDeviceOverWinUsb)
        {
            IInputSource existingInputSource = _inputSourceDataSource.GetByDeviceParentInstanceId(hidDevice.InstanceId);
            if (existingInputSource != null)
            {
                existingInputSource.Stop();
                device = existingInputSource.GetControllerByParentInstanceId(hidDevice.InstanceId);
                device.SourceDevice.CloseDevice();
                existingFilterState = device.IsFiltered;
            }
        }
        else
        {
            IInputSource existingInputSource = _inputSourceDataSource.GetByDeviceInstanceId(hidDevice.ParentInstance);
            if (existingInputSource != null)
            {
                existingInputSource.Stop();
                device = existingInputSource.GetControllerByInstanceId(hidDevice.ParentInstance);
                device.SourceDevice.CloseDevice();
                existingFilterState = device.IsFiltered;
            }
        }

        if (device == null)
        {
            IInputSource existingInputSource = _inputSourceDataSource.GetByDeviceInstanceId(hidDevice.InstanceId);
            if (existingInputSource != null)
            {
                existingInputSource.Stop();
                device = existingInputSource.GetControllerByInstanceId(hidDevice.InstanceId);
                device.SourceDevice.CloseDevice();
                existingFilterState = device.IsFiltered;
            }
        }

        if (device == null)
        {
            device = _deviceFactory.CreateDevice(deviceInfo, hidDevice);
            _controllers.Add(device);
        }

        return (device, existingFilterState);
    }

    private async void RemoveDevice(string instanceId)
    {
        if (!IsStopping && !_isPerformingFilterAction && ShouldAutoRebuild)
        {
            IInputSource existingInputSource = _inputSourceDataSource.GetByDeviceInstanceId(instanceId);
            if (existingInputSource != null)
            {
                existingInputSource.Stop();
                ICompatibleHidDevice device = existingInputSource.GetControllerByInstanceId(instanceId);
                device.SourceDevice.CloseDevice();
                _controllers.Remove(device);
                await RebuildInputSourceList();
            }
        }
    }

    private async void RefreshConfigurations()
    {
        await QuickRebuild();
    }

    private async void DefaultConfigurationUpdated(string inputSourceKey)
    {
        //optimize rebuild logic to only rebuild the one
        await QuickRebuild();
    }

    private async void GameWatchStopped(ProcessorWatchItem obj)
    {
        await QuickRebuild();
    }

    private async void GameWatchStarted(ProcessorWatchItem obj)
    {
        await QuickRebuild();
    }

    private async void FilterDriverEnabledChanged(bool obj)
    {
        await QuickRebuild();
    }

    private async Task QuickRebuild(CancellationToken ct = default)
    {
        ShouldAutoRebuild = false;
        await RebuildInputSourceList(ct);
        ShouldAutoRebuild = true;
    }

    private async void OnCustomAction(ICustomAction customAction)
    {
        switch (customAction)
        {
            case SetPlayerLedAndColorAction playerLedAction:
                playerLedAction.InputSource.SetPlayerNumberAndColor(playerLedAction.PlayerNumber);
                break;
            case GracefulShutdownAction gracefulShutdownAction:
                ShouldAutoRebuild = false;
                await gracefulShutdownAction.InputSource.DisconnectControllers();
                foreach (ICompatibleHidDevice device in gracefulShutdownAction.InputSource.Controllers)
                {
                    _controllers.Remove(device);
                }

                await RebuildInputSourceList();
                ShouldAutoRebuild = true;
                break;
        }
    }
}