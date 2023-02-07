using MessagePipe;

using Vapour.Shared.Common.Types;
using Vapour.Shared.Devices.HID;
using Vapour.Shared.Devices.HID.DeviceInfos;
using Vapour.Shared.Devices.Services.Configuration;
using Vapour.Shared.Devices.Services.Configuration.Messages;
using Vapour.Shared.Devices.Services.ControllerEnumerators;
using Vapour.Shared.Devices.Services.Reporting.CustomActions;

namespace Vapour.Shared.Devices.Services;

internal sealed class InputSourceService : IInputSourceService
{
    private readonly List<ICompatibleHidDevice> _controllers = new();
    private readonly IDeviceFactory _deviceFactory;
    private readonly ManualResetEventSlim _devicesFilterWait = new(false);
    private readonly IFilterService _filterService;
    private readonly IHidDeviceEnumeratorService<HidDevice> _hidEnumeratorService;
    private readonly IInputSourceBuilderService _inputSourceBuilderService;
    private readonly IInputSourceDataSource _inputSourceDataSource;
    private readonly IHidDeviceEnumeratorService<HidDeviceOverWinUsb> _winUsbDeviceEnumeratorService;

    private List<ICompatibleHidDevice> _devicesToFilter;

    private bool _isPerformingFilterAction;

    private readonly IAsyncSubscriber<GameWatchMessage> _gameWatchSubscriber;
    private readonly IAsyncSubscriber<SetPlayerLedAndColorAction> _setPlayerLedAndColorSubscriber;
    private readonly IAsyncSubscriber<GracefulShutdownAction> _gracefulShutdownSubscriber;
    private readonly IAsyncSubscriber<string, bool> _filterDriverEnabledChangedSubscriber;
    private readonly IAsyncSubscriber<string, string> _configurationUpdatedSubscriber;
    private readonly IAsyncPublisher<string, bool> _sourceListReadyPublisher;
    private readonly List<IDisposable> _subscriptions = new();

    public InputSourceService(
        IInputSourceBuilderService inputSourceBuilderService,
        IInputSourceDataSource inputSourceDataSource,
        IFilterService filterService,
        IHidDeviceEnumeratorService<HidDevice> hidEnumeratorService,
        IHidDeviceEnumeratorService<HidDeviceOverWinUsb> winUsbDeviceEnumeratorService,
        IDeviceFactory deviceFactory,
        IAsyncSubscriber<GameWatchMessage> gameWatchSubscriber,
        IAsyncSubscriber<SetPlayerLedAndColorAction> setPlayerLedAndColorSubscriber,
        IAsyncSubscriber<GracefulShutdownAction> gracefulShutdownSubscriber,
        IAsyncSubscriber<string, bool> filterDriverEnabledChangedSubscriber,
        IAsyncSubscriber<string, string> configurationUpdatedSubscriber,
        IAsyncPublisher<string, bool> sourceListReadyPublisher)
    {
        _inputSourceBuilderService = inputSourceBuilderService;
        _inputSourceDataSource = inputSourceDataSource;
        _filterService = filterService;
        _hidEnumeratorService = hidEnumeratorService;
        _winUsbDeviceEnumeratorService = winUsbDeviceEnumeratorService;
        _deviceFactory = deviceFactory;
        _gameWatchSubscriber = gameWatchSubscriber;
        _setPlayerLedAndColorSubscriber = setPlayerLedAndColorSubscriber;
        _gracefulShutdownSubscriber = gracefulShutdownSubscriber;
        _filterDriverEnabledChangedSubscriber = filterDriverEnabledChangedSubscriber;
        _configurationUpdatedSubscriber = configurationUpdatedSubscriber;
        _sourceListReadyPublisher = sourceListReadyPublisher;
    }

    public bool ShouldAutoRebuild { get; set; } = true;

    public bool IsStopping { get; set; }

    public async Task Start(CancellationToken ct = default)
    {
        _subscriptions.Add(_gameWatchSubscriber.Subscribe(OnGameWatchMessage));
        _subscriptions.Add(_setPlayerLedAndColorSubscriber.Subscribe(OnSetPlayerLedNumberAndColor));
        _subscriptions.Add(_gracefulShutdownSubscriber.Subscribe(OnGracefulShutdown));
        _subscriptions.Add(_filterDriverEnabledChangedSubscriber.Subscribe(MessageKeys.FilterDriverEnabledChangedKey, FilterDriverEnabledChanged));
        _subscriptions.Add(_configurationUpdatedSubscriber.Subscribe(MessageKeys.ConfigurationChangedKey, RefreshConfigurations));

        _hidEnumeratorService.DeviceArrived += SetupDevice;
        _hidEnumeratorService.DeviceRemoved += RemoveDevice;

        _winUsbDeviceEnumeratorService.DeviceArrived += SetupDevice;
        _winUsbDeviceEnumeratorService.DeviceRemoved += RemoveDevice;

        ShouldAutoRebuild = false;

        _hidEnumeratorService.Start();
        _winUsbDeviceEnumeratorService.Start();

        await RebuildInputSourceList(ct);

        ShouldAutoRebuild = true;

        await _sourceListReadyPublisher.PublishAsync(MessageKeys.InputSourceReadyKey, true, ct);
    }

    public void Stop()
    {
        foreach (var subscription in _subscriptions)
        {
            subscription.Dispose();
        }
        _subscriptions.Clear();

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
            _inputSourceDataSource.InputSources.Remove(inputSource);
            _inputSourceDataSource.FireRemoved(inputSource);
        }

        if (IsStopping)
        {
            foreach (ICompatibleHidDevice device in _controllers.ToList())
            {
                _filterService.UnfilterController(device);
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

        await Task.Run(() =>
        {
            List<IInputSource> inputSourceList = _inputSourceBuilderService.BuildInputSourceList(_controllers);
            _inputSourceDataSource.InputSources.AddRange(inputSourceList);
            CheckPerformFilterActions(inputSourceList, ct);
            StartInputSources();
            ShouldAutoRebuild = existingShouldRebuild;
        }, ct);
    }

    private void CheckPerformFilterActions(IReadOnlyCollection<IInputSource> inputSourceList,
        CancellationToken ct = default)
    {
        _devicesToFilter = new List<ICompatibleHidDevice>();

        _devicesToFilter.AddRange(GetNeededFilterListByConnectionType(inputSourceList, ConnectionType.Usb)
            .Where(c => c.CurrentDeviceInfo.WinUsbEndpoints != null).ToList());

        List<ICompatibleHidDevice> blueToothDevices =
            GetNeededFilterListByConnectionType(inputSourceList, ConnectionType.Bluetooth)
                .Where(c => c.CurrentDeviceInfo.IsBtFilterable).ToList();

        _devicesToFilter.AddRange(blueToothDevices);

        if (_devicesToFilter.Any())
        {
            _isPerformingFilterAction = true;

            foreach (ICompatibleHidDevice device in _devicesToFilter.ToList())
            {
                if (device.IsFiltered)
                {
                    _filterService.UnfilterController(device, ct);
                }
                else
                {
                    _filterService.FilterController(device, ct);
                }
            }

            _devicesFilterWait.Wait(ct);
            _devicesToFilter = null;
            _devicesFilterWait.Reset();

            _isPerformingFilterAction = false;
        }
    }

    private IEnumerable<ICompatibleHidDevice> GetNeededFilterListByConnectionType(
        IEnumerable<IInputSource> inputSourceList, ConnectionType connectionType)
    {
        List<ICompatibleHidDevice> finalList = new();
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
            //inputSource.OnCustomActionDetected += OnCustomAction;
            inputSource.Start();
            inputSource.SetPlayerNumberAndColor(i + 1);
            _inputSourceDataSource.FireCreated(inputSource);
        }
    }

    private async void SetupDevice(IHidDevice hidDevice)
    {
        if (IsStopping)
        {
            return;
        }

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

        (ICompatibleHidDevice device, bool? isFiltered) = GetDevice(hidDevice, deviceInfo);
        device.Setup(hidDevice, deviceInfo);
        device.Initialize();

        if (device.Connection == ConnectionType.Bluetooth)
        {
            device.IsFiltered = _filterService.IsBtFiltered(device.SourceDevice.InstanceId);
        }
        else if (hidDevice is HidDeviceOverWinUsb)
        {
            device.IsFiltered = true;
        }
        else
        {
            device.IsFiltered = false;
        }

        if (isFiltered.HasValue)
        {
            CheckIfWaitingAfterFilterAction(device, isFiltered.Value);
        }

        if (ShouldAutoRebuild)
        {
            await RebuildInputSourceList();
        }
    }

    private void CheckIfWaitingAfterFilterAction(ICompatibleHidDevice device, bool previousFilterState)
    {
        ICompatibleHidDevice existingWait = _devicesToFilter.SingleOrDefault(c =>
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

    private (ICompatibleHidDevice Device, bool? ExistingFilterState) GetDevice(IHidDevice hidDevice,
        DeviceInfo deviceInfo)
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

    private async ValueTask RefreshConfigurations(string inputSourceKey, CancellationToken cs)
    {
        if (string.IsNullOrWhiteSpace(inputSourceKey))
        {
            await QuickRebuild(cs);
        }
        else
        {
            // optimize rebuild logic to only rebuild the one
            await QuickRebuild(cs);
        }
    }

    private async ValueTask OnGameWatchMessage(GameWatchMessage message, CancellationToken cs)
    {
        await QuickRebuild(cs);
    }

    private async ValueTask FilterDriverEnabledChanged(bool isEnabled, CancellationToken cs)
    {
        await QuickRebuild(cs);
    }

    private async Task QuickRebuild(CancellationToken ct = default)
    {
        ShouldAutoRebuild = false;
        await RebuildInputSourceList(ct);
        ShouldAutoRebuild = true;
    }

    private ValueTask OnSetPlayerLedNumberAndColor(SetPlayerLedAndColorAction message, CancellationToken cs)
    {
        message.InputSource.SetPlayerNumberAndColor(message.PlayerNumber);
        return  ValueTask.CompletedTask;
    }

    private async ValueTask OnGracefulShutdown(GracefulShutdownAction message, CancellationToken cs)
    {
        ShouldAutoRebuild = false;
        await message.InputSource.DisconnectControllers();
        
        foreach (ICompatibleHidDevice device in message.InputSource.Controllers)
        {
            _controllers.Remove(device);
        }

        await RebuildInputSourceList(cs);
        ShouldAutoRebuild = true;
    }
}