using Vapour.Shared.Common.Types;
using Vapour.Shared.Devices.HID;
using Vapour.Shared.Devices.HID.DeviceInfos;
using Vapour.Shared.Devices.Services.Configuration;
using Vapour.Shared.Devices.Services.ControllerEnumerators;
using Vapour.Shared.Devices.Services.Reporting.CustomActions;

namespace Vapour.Shared.Devices.Services;

internal sealed class InputSourceService : IInputSourceService
{
    private readonly ManualResetEventSlim _blueToothFilterWait = new(false);
    private readonly List<ICompatibleHidDevice> _controllers = new();
    private readonly IDeviceFactory _deviceFactory;
    private readonly IFilterService _filterService;
    private readonly IGameProcessWatcherService _gameProcessWatcherService;
    private readonly IHidDeviceEnumeratorService<HidDevice> _hidEnumeratorService;
    private readonly IInputSourceBuilderService _inputSourceBuilderService;
    private readonly IInputSourceConfigurationService _inputSourceConfigurationService;
    private readonly IInputSourceDataSource _inputSourceDataSource;

    private readonly ManualResetEventSlim _usbFilterWait = new(false);
    private readonly IHidDeviceEnumeratorService<HidDeviceOverWinUsb> _winUsbDeviceEnumeratorService;
    private List<(string DeviceKey, bool InitialFilterState)> _blueToothWaitList;

    private bool _isPerformingFilterAction;
    private string _usbWaitDeviceKey;
    private bool _usbWaitInitialFilterState;

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
            foreach (ICompatibleHidDevice device in _controllers.ToList())
            {
                if (device.Connection == ConnectionType.Bluetooth)
                {
                    // TODO: sdp unfilter
                }
                else if (device.Connection == ConnectionType.Usb)
                {
                    _filterService.UnfilterController(device.SourceDevice.InstanceId);
                }
            }

            //TODO: restart bt host
        }

        //need some time after stopping input sources before starting more up
        Thread.Sleep(250);
    }

    private async Task RebuildInputSourceList()
    {
        bool existingShouldRebuild = ShouldAutoRebuild;
        ShouldAutoRebuild = false;
        ClearExistingSources();

        await Task.Run(async () =>
        {
            List<IInputSource> inputSourceList = _inputSourceBuilderService.BuildInputSourceList(_controllers);
            _inputSourceDataSource.InputSources.AddRange(inputSourceList);
            await PerformFilterActionsIfNeeded(inputSourceList);
            StartInputSources();
            ShouldAutoRebuild = existingShouldRebuild;
        });
    }

    private async Task PerformFilterActionsIfNeeded(List<IInputSource> inputSourceList)
    {
        await CheckPerformBluetoothFilterActions(inputSourceList);
        await CheckPerformUsbFilterActions(inputSourceList);
    }

    private Task CheckPerformUsbFilterActions(List<IInputSource> inputSourceList)
    {
        List<(string DeviceKey, string InstanceId, bool IsFiltered, DeviceInfo deviceInfo)> usbDevicesToFilter =
            GetNeededFilterListByConnectionType(inputSourceList, ConnectionType.Usb)
                .Where(c => c.deviceInfo.WinUsbEndpoints != null).ToList();

        if (usbDevicesToFilter.Any())
        {
            _isPerformingFilterAction = true;

            foreach ((string deviceKey, string instanceId, bool isFiltered, DeviceInfo _) in usbDevicesToFilter)
            {
                _usbWaitDeviceKey = deviceKey;
                _usbWaitInitialFilterState = isFiltered;

                if (isFiltered)
                {
                    _filterService.UnfilterController(instanceId);
                }
                else
                {
                    _filterService.FilterController(instanceId);
                }

                //dont need to do anything else because if this works existing
                //compatible hid devices already on input sources should be
                //updated with a new source device

                _usbFilterWait.Wait();
                _usbWaitDeviceKey = null;
                _usbFilterWait.Reset();
            }

            _isPerformingFilterAction = false;
        }

        return Task.CompletedTask;
    }

    private Task CheckPerformBluetoothFilterActions(List<IInputSource> inputSourceList)
    {
        List<(string DeviceKey, string InstanceId, bool IsFiltered, DeviceInfo deviceInfo)>
            blueToothDevicesToFilerAction =
                GetNeededFilterListByConnectionType(inputSourceList, ConnectionType.Bluetooth)
                    .Where(c => c.deviceInfo.IsBtFilterable).ToList();

        if (blueToothDevicesToFilerAction.Any())
        {
            _isPerformingFilterAction = true;

            _blueToothWaitList = new List<(string DeviceKey, bool InitialFilterState)>();

            foreach ((string DeviceKey, string InstanceId, bool IsFiltered, DeviceInfo deviceInfo) device in
                     blueToothDevicesToFilerAction)
            {
                _blueToothWaitList.Add((device.DeviceKey, device.IsFiltered));

                if (device.IsFiltered)
                {
                    _filterService.UnfilterController(device.InstanceId);
                }
                else
                {
                    _filterService.FilterController(device.InstanceId);
                }
            }

            // TODO: await restart bt host

            //dont need to do anything else because if this works existing
            //compatible hid devices already on input sources should be
            //updated with a new source device
            _blueToothFilterWait.Wait();
            _blueToothWaitList = null;
            _blueToothFilterWait.Reset();

            _isPerformingFilterAction = false;
        }

        return Task.CompletedTask;
    }

    private IEnumerable<(string DeviceKey, string InstanceId, bool IsFiltered, DeviceInfo deviceInfo)>
        GetNeededFilterListByConnectionType(IEnumerable<IInputSource> inputSourceList, ConnectionType connectionType)
    {
        List<(string DeviceKey, string InstanceId, bool IsFiltered, DeviceInfo deviceInfo)> finalList = new();
        foreach (ICompatibleHidDevice controller in inputSourceList
                     .SelectMany(i => i.Controllers.Where(c => c.Connection == connectionType))
                     .ToList())
        {
            if (controller.IsFiltered && !_filterService.IsFilterDriverEnabled)
            {
                finalList.Add((controller.DeviceKey, controller.SourceDevice.InstanceId, controller.IsFiltered,
                    controller.CurrentDeviceInfo));
            }
            else if (controller.IsFiltered && controller.CurrentConfiguration.OutputDeviceType == OutputDeviceType.None)
            {
                finalList.Add((controller.DeviceKey, controller.SourceDevice.InstanceId, controller.IsFiltered,
                    controller.CurrentDeviceInfo));
            }
            else if (!controller.IsFiltered && _filterService.IsFilterDriverEnabled &&
                     controller.CurrentConfiguration.OutputDeviceType != OutputDeviceType.None)
            {
                finalList.Add((controller.DeviceKey, controller.SourceDevice.InstanceId, controller.IsFiltered,
                    controller.CurrentDeviceInfo));
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

            ICompatibleHidDevice device = GetDevice(hidDevice, deviceInfo);

            // TODO: take Bluetooth into account
            if (hidDevice is HidDeviceOverWinUsb)
            {
                device.IsFiltered = true;
            }
            else
            {
                device.IsFiltered = false;
            }

            device.Setup(hidDevice, deviceInfo);
            device.Initialize();

            CheckIfWaitingAfterFilterAction(device);

            if (ShouldAutoRebuild)
            {
                await RebuildInputSourceList();
            }
        }
    }

    private void CheckIfWaitingAfterFilterAction(ICompatibleHidDevice device)
    {
        if (device.Connection == ConnectionType.Usb && !string.IsNullOrWhiteSpace(_usbWaitDeviceKey) &&
            _usbWaitDeviceKey == device.DeviceKey &&
            _usbWaitInitialFilterState == !device.IsFiltered)
        {
            _usbFilterWait.Set();
        }
        else if (device.Connection == ConnectionType.Bluetooth && _blueToothWaitList != null)
        {
            (string DeviceKey, bool InitialFilterState) existingWait = _blueToothWaitList.SingleOrDefault(c =>
                c.DeviceKey == device.DeviceKey && c.InitialFilterState == !device.IsFiltered);
            if (existingWait != default)
            {
                _blueToothWaitList.Remove(existingWait);
                if (_blueToothWaitList.Count == 0)
                {
                    _blueToothFilterWait.Set();
                }
            }
        }
    }

    private ICompatibleHidDevice GetDevice(IHidDevice hidDevice, DeviceInfo deviceInfo)
    {
        ICompatibleHidDevice device = null;

        if (hidDevice is HidDeviceOverWinUsb)
        {
            IInputSource existingInputSource = _inputSourceDataSource.GetByDeviceParentInstanceId(hidDevice.InstanceId);
            if (existingInputSource != null)
            {
                existingInputSource.Stop();
                device = existingInputSource.GetControllerByParentInstanceId(hidDevice.InstanceId);
                device.SourceDevice.CloseDevice();
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
            }
        }

        if (device == null)
        {
            device = _deviceFactory.CreateDevice(deviceInfo, hidDevice);
            _controllers.Add(device);
        }

        return device;
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

    private async Task QuickRebuild()
    {
        ShouldAutoRebuild = false;
        await RebuildInputSourceList();
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