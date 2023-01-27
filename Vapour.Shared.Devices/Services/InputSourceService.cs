using Microsoft.Extensions.DependencyInjection;

using Vapour.Shared.Devices.HID;
using Vapour.Shared.Devices.HID.DeviceInfos;
using Vapour.Shared.Devices.Services.Configuration;

namespace Vapour.Shared.Devices.Services;

internal sealed class InputSourceService : IInputSourceService
{
    private readonly IInputSourceConfigurationService _inputSourceConfigurationService;
    private readonly IInputSourceDataSource _inputSourceDataSource;
    private readonly IFilterService _filterService;
    private readonly IServiceProvider _services;

    private readonly List<ICompatibleHidDevice> _controllers = new();

    public InputSourceService(
        IInputSourceConfigurationService inputSourceConfigurationService,
        IInputSourceDataSource inputSourceDataSource,
        IFilterService filterService,
        IServiceProvider services
        )
    {
        _inputSourceConfigurationService = inputSourceConfigurationService;
        _inputSourceDataSource = inputSourceDataSource;
        _filterService = filterService;
        _services = services;
    }

    public bool ShouldAutoFixup { get; set; } = true;

    public bool ShouldAutoCombineJoyCons { get; set; } = true;

    public void Stop()
    {
        foreach (var inputSource in _inputSourceDataSource.InputSources)
        {
            inputSource.Stop();
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

            if (ShouldAutoCombineJoyCons)
            {
                await AutoCombineJoyCons();
            }

            for (var i = 0; i < _inputSourceDataSource.InputSources.Count; i++)
            {
                var inputSource = _inputSourceDataSource.InputSources[i];
                inputSource.SetPlayerNumberAndColor(i + 1);

                inputSource.Start();
                _inputSourceDataSource.FireCreated(inputSource);
            }

            ShouldAutoFixup = currentAutoFixup;
        });
    }

    private async Task AutoCombineJoyCons()
    {
        var leftJoyCons = GetJoyCons(typeof(JoyConLeftDeviceInfo));
        var rightJoyCons = GetJoyCons(typeof(JoyConRightDeviceInfo));

        while (leftJoyCons.Count > 0 && rightJoyCons.Count > 0)
        {
            var firstLeftJoyCon = leftJoyCons.First();
            var firstRightJoyCon = rightJoyCons.First();

            var leftDevice =
                _controllers.Single(c => c.DeviceKey == firstLeftJoyCon.Configuration.Controllers[0].DeviceKey);
            var rightDevice =
                _controllers.Single(c => c.DeviceKey == firstRightJoyCon.Configuration.Controllers[0].DeviceKey);

            ClearSource(firstLeftJoyCon);
            ClearSource(firstRightJoyCon);

            List<ICompatibleHidDevice> list = new()
            {
                leftDevice,
                rightDevice
            };

            await CreateInputSource(list, new List<string>());

            leftJoyCons.Remove(firstRightJoyCon);
            rightJoyCons.Remove(firstRightJoyCon);
        }
    }

    private List<IInputSource> GetJoyCons(Type deviceInfoType)
    {
        return (from inputSource in _inputSourceDataSource.InputSources
            where inputSource.Configuration.Controllers.Count == 1
            let controller =
                _controllers.SingleOrDefault(c => c.DeviceKey == inputSource.Configuration.Controllers[0].DeviceKey)
            where controller != null && controller.CurrentDeviceInfo.GetType() == deviceInfoType
            select inputSource).ToList();
    }

    private void ClearExistingSources()
    {
        foreach (var inputSource in _inputSourceDataSource.InputSources.ToList())
        {
            ClearSource(inputSource);
            _inputSourceDataSource.FireRemoved(inputSource);
        }
    }

    private void ClearSource(IInputSource inputSource)
    {
        inputSource.Stop();
        inputSource.ConfigurationChanged -= InputSource_ConfigurationChanged;
        _inputSourceDataSource.InputSources.Remove(inputSource);
    }

    private async Task CreateInputSource(List<ICompatibleHidDevice> controllers, List<string> controllersProcessed, InputSourceConfiguration configuration = null)
    {
        configuration ??= HackGetConfiguration(controllers);

        var inputSource = _services.GetService<IInputSource>();
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
    }

    private InputSourceConfiguration HackGetConfiguration(List<ICompatibleHidDevice> controllers)
    {
        var inputSource = _services.GetRequiredService<IInputSource>();

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