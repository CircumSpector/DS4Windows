using Vapour.Shared.Devices.HID;
using Vapour.Shared.Devices.Services.Configuration;
using Vapour.Shared.Devices.Services.Reporting;

namespace Vapour.Shared.Devices.Services;

internal sealed class InputSourceService : IInputSourceService
{
    private readonly IInputReportProcessorService _inputReportProcessorService;
    private readonly IInputSourceConfigurationService _inputSourceConfigurationService;
    private readonly IInputSourceDataSource _inputSourceDataSource;
    private readonly IFilterService _filterService;

    private readonly List<ICompatibleHidDevice> _controllers = new();

    public InputSourceService(IInputReportProcessorService inputReportProcessorService,
        IInputSourceConfigurationService inputSourceConfigurationService,
        IInputSourceDataSource inputSourceDataSource,
        IFilterService filterService)
    {
        _inputReportProcessorService = inputReportProcessorService;
        _inputSourceConfigurationService = inputSourceConfigurationService;
        _inputSourceDataSource = inputSourceDataSource;
        _filterService = filterService;
    }

    public bool ShouldAutoFixup { get; set; } = true;

    public void Stop()
    {
        foreach (var inputSource in _inputSourceDataSource.InputSources)
        {
            _inputReportProcessorService.StopProcessing(inputSource);
        }
    }

    public async Task Clear()
    {
        foreach (var controller in _inputSourceDataSource.InputSources.ToList().SelectMany(inputSource => inputSource.GetControllers()))
        {
            await RemoveController(controller.SourceDevice.InstanceId);
        }
    }

    public async Task FixupInputSources()
    {
        await Task.Run(async () =>
        {
            var currentAutoFixup = ShouldAutoFixup;
            ShouldAutoFixup = false;
            ClearExistingSources();

            var controllersProcessed = new List<string>();

            foreach (var controller in _controllers.ToList())
            {
                if (controllersProcessed.All(id => id != controller.DeviceKey))
                {
                    var controllersToProcess = new List<ICompatibleHidDevice> { controller };
                    var configuration =
                        _inputSourceConfigurationService.GetMultiControllerConfiguration(controller.DeviceKey);

                    if (configuration != null)
                    {
                        var otherControllers = _controllers.Where(c =>
                            configuration.Controllers.Any(k => k.DeviceKey == c.DeviceKey) && c.DeviceKey != controller.DeviceKey).ToList();

                        if (otherControllers.Count + 1 == configuration.Controllers.Count)
                        {
                            controllersToProcess.AddRange(otherControllers);
                        }
                        else
                        {
                            configuration = null;
                        }
                    }

                    await CreateInputSource(controllersToProcess, controllersProcessed, configuration);
                }
            }

            ShouldAutoFixup = currentAutoFixup;
        });
    }

    private void ClearExistingSources()
    {
        foreach (var inputSource in _inputSourceDataSource.InputSources.ToList())
        {
            _inputReportProcessorService.StopProcessing(inputSource);
            inputSource.ConfigurationChanged -= InputSource_ConfigurationChanged;
            _inputSourceDataSource.InputSources.Remove(inputSource);
            _inputSourceDataSource.FireRemoved(inputSource);
        }
    }

    private async Task CreateInputSource(List<ICompatibleHidDevice> controllers, List<string> controllersProcessed, InputSourceConfiguration configuration = null)
    {
        if (configuration == null)
        {
            configuration = HackGetConfiguration(controllers);
        }

        var inputSource = new InputSource();
        var finalControllers = new List<ICompatibleHidDevice>();
        foreach (var cont in controllers)
        {
            cont.Index = configuration.Controllers.Single(c => c.DeviceKey == cont.DeviceKey).Index;

            var initialFilterState = cont.IsFiltered;
            if (_filterService.FilterUnfilterIfNeeded(cont, configuration.OutputDeviceType))
            {
                ICompatibleHidDevice newController = null;
                while (newController == null)
                {
                    newController = _controllers.ToList().SingleOrDefault(c =>
                        c.DeviceKey == cont.DeviceKey && c.IsFiltered == !initialFilterState);
                    await Task.Delay(100);
                }

                newController.Index = cont.Index;
                finalControllers.Add(newController);
            }
            else
            {
                finalControllers.Add(cont);
            }
        }

        foreach (var controller in finalControllers)
        {
            inputSource.AddController(controller);
            controllersProcessed.Add(controller.DeviceKey);
        }

        _inputSourceConfigurationService.LoadInputSourceConfiguration(inputSource);
        inputSource.ConfigurationChanged += InputSource_ConfigurationChanged;
        _inputSourceDataSource.InputSources.Add(inputSource);
        _inputReportProcessorService.StartProcessing(inputSource);
        _inputSourceDataSource.FireCreated(inputSource);
    }

    private InputSourceConfiguration HackGetConfiguration(List<ICompatibleHidDevice> controllers)
    {
        var inputSource = new InputSource();
        foreach (var controller in controllers)
        {
            inputSource.AddController(controller);
        }
        _inputSourceConfigurationService.LoadInputSourceConfiguration(inputSource);

        return inputSource.Configuration;
    }

    public async Task AddController(ICompatibleHidDevice device)
    {
        if (_controllers.All(c => c.DeviceKey != device.DeviceKey))
        {
            _controllers.Add(device);

            if (ShouldAutoFixup)
            {
                await FixupInputSources();
            }
        }
    }
    
    public async Task RemoveController(string instanceId)
    {
        var existingController = _controllers.SingleOrDefault(c => c.SourceDevice.InstanceId.ToLower() == instanceId.ToLower());
        if (existingController != null)
        {
            _controllers.Remove(existingController);

            if (ShouldAutoFixup)
            {
                existingController.Dispose();
                await FixupInputSources();
            }
        }
    }

    private async void InputSource_ConfigurationChanged(object sender, InputSourceConfiguration e)
    {
        ShouldAutoFixup = false;
        await FixupInputSources();
        ShouldAutoFixup = true;
    }
}